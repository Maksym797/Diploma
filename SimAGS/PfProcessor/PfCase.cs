using cern.colt.matrix;
using cern.colt.matrix.impl;
using cern.colt.matrix.linalg;
using SimAGS.Components;
using SimAGS.Handlers;
using SimAGS.SimUtil;
using System;
using System.Collections.Generic;

namespace SimAGS.PfProcessor
{
    public class PFCase
    {
        #region state
        public List<bus> busArrayList { get; set; }
        public List<branch> branchArrayList { get; set; }
        public List<twoWindTrans> twoWindTransArrayList { get; set; }
        public List<threeWindTrans> thrWindTransArrayList { get; set; }
        public List<area> areaArrayList { get; set; }
        public List<zone> zoneArrayList { get; set; }
        public List<owner> ownerArrayList { get; set; }
        public List<bus> sortBusArrayList { get; set; }

        // default system parameters (fixed)
        public const double DefaultBlowup = 1E5;           // terminate power flow calculation if difference is too large 
        public const double DefaultVmax = 1.5;             // maximum voltage for PQ bus
        public const double DefaultVmin = 0.2;             // minimum voltage for PQ bus 
        public const int DefaultVcontrolitre = 10;         // maximum iterations for out voltage control loop (to avoid control oscillation)
        public const double Rad2Deg = 180 / Math.PI;
        // default system parameters (fixed)
        public const double DEFAULT_BLOWUP = 1E5;           // terminate power flow calculation if difference is too large 
        public const double DEFAULT_VMAX = 1.5;             // maximum voltage for PQ bus
        public const double DEFAULT_VMIN = 0.2;             // minimum voltage for PQ bus 
        public const int DEFAULT_VCONTROLITRE = 10;         // maximum iterations for out voltage control loop (to avoid control oscillation)


        // computation control variable
        public double setSBASE;                             // system base
        public double setPFTol;                             // 0.1 MW tolerance
        public int setPFMaxItr;                             // maximum iterations for inner power calculation loop
        public bool bEnableVoltRegLoop;                  // enable voltage regulation (generator remote voltage, switchable shunt, tap-change transformers)
        public double setVoltRegLoopTol;                    // tolerance for generator reactive power overshooting to to avoid PV->PQ oscillations

        public int nBus;                                    // total bus number including dead bus 
        public int nYBus;                                   // total active bus number
        public int nLoad;
        public int nGen;
        public int nBranch;
        public int nTwoWindXfrm;
        public int nThrWindXfrm;
        public int nArea;
        public int nSWShunt;
        public int nZone;
        public int nOwner;

        public double sysPLoss = 0.0;                       // system-wide P losses
        public double sysQLoss = 0.0;                       // system-wide Q losses 

        // extended object list

        // system matrix 
        public yMatrix yMat;                                // system y matrix; 

        // array storing all effective bus numbers 
        public DoubleMatrix2D AVsol;                        // Angle and Vmag 
        public DoubleMatrix2D PQsol;                        // P and Q 
        public DoubleMatrix2D PQobj;                        // P and Q objective for PQ and PV bus (Q for PV and P&Q for SW are zeros)
        public DoubleMatrix2D JMat;                         // System Jacobian matrix 
        public jacob jacobObj;                              // Jacobian matrix 

        // calculation related variables 
        public bool bInLoopSolved = false;          // power flow inner loop converged 
        public bool bConverged = false;         // power flow out loop converged 

        // voltage regulation 
        public pfVoltageHelper voltHelper;
        public List<bus> loadBusList = new List<bus>();
        public List<bus> genBusList = new List<bus>();
        public int[] loadQPosArray;
        public int[] genQPosArray;

        // case verification 
        public bool bCaseCheck = true;
        
        // matrix operator object
        public Algebra matrixOpt = new Algebra();

        #endregion

        public PFCase()
        {
            busArrayList = new List<bus>();
            branchArrayList = new List<branch>();
            twoWindTransArrayList = new List<twoWindTrans>();
            thrWindTransArrayList = new List<threeWindTrans>();
            areaArrayList = new List<area>();
            zoneArrayList = new List<zone>();
            ownerArrayList = new List<owner>();

            sortBusArrayList = new List<bus>();
        }

        public void LoadCaseData(JFile pfFile)
        {
            new PFCaseLoad(this).exec(pfFile);
        }

        public void ini()
        {

            // ---------------------- (1) Process abnormal data --------------------------------//
            //process ZBR (pending) creating the mapping table [pending]

            //sortBusArrayList = busArrayList;
            //nYBus = sortBusArrayList.size(); 

            int loadBusRank = 0;            // the ranking of loads in all load buses 
            int genBusRrank = 0;            // the ranking of generators in all generator buses
            sortBusArrayList = new List<bus>();
            for (int i = 0; i < busArrayList.size(); i++)
            {
                bus busTemp = busArrayList.get(i);
                busTemp.yMatIndx = i;
                sortBusArrayList.add(busTemp);

                if (busTemp.IDE == 1)
                {                               // load bus
                    busTemp.LLIndx = loadBusRank;
                    loadBusList.add(busTemp);
                    loadBusRank++;
                }
                else if (busTemp.IDE == 2 || busTemp.IDE == 3)
                {   // generator buses 
                    busTemp.GGIndx = genBusRrank;
                    genBusList.add(busTemp);
                    genBusRrank++;
                }
            }
            nYBus = sortBusArrayList.size();

            //--------------- (2) MOD: added to coordinate dynamic data loading --------------//
            for (int i = 0; i < nYBus; i++)
            {
                sortBusArrayList.get(i).vangPos = i;
                sortBusArrayList.get(i).vmagPos = i + nYBus;
            }

            // ------------------ (3) update branch terminal bus index ----------------- //
            foreach (branch branchTemp in branchArrayList)
            {
                bus frBus = dataProcess.getBusAt(branchTemp.I, sortBusArrayList);
                bus toBus = dataProcess.getBusAt(branchTemp.J, sortBusArrayList);
                branchTemp.setFromBus(frBus);                       // update from bus index
                branchTemp.setToBus(toBus);                         // update to bus index 

                // update terminal buses 
                frBus.addNeighborBus(toBus);
                toBus.addNeighborBus(frBus);

            }

            foreach (twoWindTrans twoWindTransTemp in twoWindTransArrayList)
            {
                bus frBus = dataProcess.getBusAt(twoWindTransTemp.I, sortBusArrayList);
                bus toBus = dataProcess.getBusAt(twoWindTransTemp.J, sortBusArrayList);
                twoWindTransTemp.setFromBus(frBus);
                twoWindTransTemp.setToBus(toBus);

                // update terminal buses
                frBus.addNeighborBus(toBus);
                toBus.addNeighborBus(frBus);
            }

            //--------- (4) update the buses that are under voltage regulation -----------//
            loadBusUnderVoltReg();

            // ------------- (5) initialize the solArray = [Theta;V]' matrix ------------- // 
            AVsol = new  SparseDoubleMatrix2D(2 * nYBus, 1);
            foreach (bus busTemp in sortBusArrayList)
            {
                AVsol.setQuick(busTemp.vangPos, 0, busTemp.ang);
                AVsol.setQuick(busTemp.vmagPos, 0, busTemp.volt);
            }
            CustomMessageHandler.Show(">>>>>>>> Process.... Case initialization is completed!");
        }
        // --------------------------------------------------------------------------------//	
        // ------------------------ create system admittance matrix -----------------------//
        // --------------------------------------------------------------------------------//

        public void buildYMatrix()
        {
            // create system admittance matrix 
            yMat = new yMatrix(this, true);
            CustomMessageHandler.Show(">>>>>>>> Process.... System admittance matrix is created!");
        }


        // --------------------------------------------------------------------------------//
        // --------------------------- calculate power flow -------------------------------//
        // --------------------------------------------------------------------------------//

        public void solvePQ()
        {

            voltHelper = bEnableVoltRegLoop ? new pfVoltageHelper(this, setVoltRegLoopTol) : null;    // true -- fix coefficient matrix 
            JMat = JMat = new SparseDoubleMatrix2D(2 * nYBus, 2 * nYBus);
            bConverged = false;

            for (int i = 0; i < DEFAULT_VCONTROLITRE; i++)
            {
                // inner power flow (includes PV->PQ conversion) then check regulation

                if (calcPQLoop() == true)
                {
                    // voltage regulation loop
                    if (voltHelper != null)
                    {
                        // check if all voltage regulation is done (-2->initial value; -1-> control exhausts ;0-> next round needed; 1-> succeed)
                        voltHelper.voltReguAdjust();

                        if (voltHelper.getStatus().Equals(pfVoltageHelper.solType.exhausted))
                        {
                            CustomMessageHandler.Show("============= Voltage regulation exhausts ===============");
                            bConverged = false;
                            break;
                        }
                        else if (voltHelper.getStatus().Equals(pfVoltageHelper.solType.itrComplete))
                        {
                            CustomMessageHandler.Show("============= Finished voltage control iteration: < " + i + " > ================\n");
                            bConverged = false;
                        }
                        else if (voltHelper.getStatus().Equals(pfVoltageHelper.solType.solved))
                        {
                            CustomMessageHandler.Show("=============== Voltage regulation succeeds =============");
                            bConverged = true;
                            break;
                        }
                    }
                    else
                    {
                        bConverged = true;
                        break;
                    }
                }
                else
                {
                    bConverged = false;                 // power flow is diverged
                    break;
                }
            }

            // only effective after the entire loop is converged 
            if (bConverged)
            {
                calcPQ();                               // determine final power injections 
                updateResults();
                //displayResults();
            }
            else
            {
                throw new simException("Error: Power flow is diverged!");
            }
        }


        /*
         * calculate power flow given fixed bus types
         * assuming the generator terminal bus setting are already adjusted to avoid violation of MVar limit  
         */
        public bool calcPQLoop()
        {

            sysPLoss = 0;
            sysQLoss = 0;

            bool bPfSolved = false;

            DoubleMatrix2D calcDif = new SparseDoubleMatrix2D(2 * nYBus, 1);
            DoubleMatrix2D AVDev = new SparseDoubleMatrix2D(2 * nYBus, 1);

            for (int i = 0; i < 5; i++)
            {               // re-compute power flow when PV->PQ conversion is complete

                bPfSolved = false;                                                      // inner-loop solvable flag

                // power flow calculation internal loop 
                for (int j = 0; j < setPFMaxItr; j++)
                {
                    calcPQ();                                                               // calculate the P and Q leaving from a given nodes based on network Y matrix, excluding ZIP load model 					
                    setPQObj();                                                             // set P and Q objective for each bus (e.g, Gen and load) 
                    calcDif = pqDif(PQobj, PQsol);                                          // PQ_set - PQ_calc												 

                    // check if converged 
                    double mismatch = matrixOpt.normInfinity(calcDif.viewColumn(0));        // get maximum value					
                    CustomMessageHandler.Show("\t Iteration Number: " + j + "\t mismatch = " + mismatch);

                    if (mismatch < setPFTol)
                    {
                        bInLoopSolved = true;
                        break;
                    }
                    else if (mismatch > DEFAULT_BLOWUP)
                    {
                        bInLoopSolved = false;
                        CustomMessageHandler.Show("ERROR: power flow blows up [maximum tolerance is reached]!");
                        break;
                    }
                    else
                    {
                        // update the Jacobian and calculate power flow
                        JMat = new SparseDoubleMatrix2D(2 * nYBus, 2 * nYBus);
                        jacob.update(yMat, AVsol, JMat, sortBusArrayList, 0);               // Calculate Jacobian matrix 		
                        AVDev = matrixOpt.solve(JMat, calcDif);                             // Solve JMat*delta_VA = delat_PQ = [PQ_calc - PQ_set] (don't contain negative sign)
                        AVsol = dataProcess.matAdd(AVsol, AVDev, 1, 1);
                        //CustomMessageHandler.Show("Voltage sol = " + AVsol.getQuick(62, 0));
                    }
                }

                //----------------- check generator Q limit  --------------//
                if (!bInLoopSolved)
                {                                                       // inner power flow diverged 
                    bPfSolved = false;
                    CustomMessageHandler.Show("ERROR: POWER FLOW CANNOT CONVERGE");
                    return bPfSolved;
                }
                else
                {
                    bConverged = false;
                    // check if needs to reset regulating voltage 
                    if (checkPV2PQ())
                    {
                        if (checkPQ2PV())
                        {                       // PQ->PV conversion (if any)
                            restGenVoltReg();
                            bPfSolved = true;
                            CustomMessageHandler.Show("Power Flow Converged");
                            return bPfSolved;
                        }
                        else
                        {
                            CustomMessageHandler.Show("[debug] restGenVoltReg acts");
                        }
                    }
                }
            } // End of the voltage loop

            return bPfSolved;
        }


        // check if PV bus hit Q limits; if true change bus type from PV to PQ 
        public bool checkPV2PQ()
        {
            bool noViolateFound = true;

            foreach (bus busTemp in genBusList)
            {
                busTemp.aggQGen = PQsol.getQuick(busTemp.vmagPos, 0) + busTemp.aggCPLoadQ + busTemp.aggCCLoadQ * AVsol.getQuick(busTemp.vmagPos, 0);

                // calculate the upper and bottom margins 
                busTemp.aggQUpperMargin = busTemp.aggQMax - busTemp.aggQGen;
                busTemp.aggQBottomMargin = busTemp.aggQGen - busTemp.aggQMin;

                if (busTemp.calcType == 2)
                {
                    if (busTemp.aggQUpperMargin < 0)
                    {
                        busTemp.calcType = 1;
                        busTemp.aggQGen = busTemp.aggQMax;
                        CustomMessageHandler.Show("PV bus at " + busTemp.I + " PV -> PQ (hit Qmax limit)");
                        noViolateFound = false;
                    }
                    else if (busTemp.aggQBottomMargin < 0)
                    {
                        busTemp.calcType = 1;
                        busTemp.aggQGen = busTemp.aggQMin;
                        CustomMessageHandler.Show("PV bus at " + busTemp.I + " PV -> PQ (hit Qmin limit)");
                        noViolateFound = false;
                    }
                }
            }
            return noViolateFound;
        }


        /*
         * update the target voltage setting and switch back the bus type from load bus to generator bus if needed
         */
        public bool checkPQ2PV()
        {

            bool bNoConversion = true;

            foreach (bus busTemp in genBusList)
            {
                double calcGenBusVolt = AVsol.getQuick(busTemp.vmagPos, 0);

                // check if the regulating voltage can be restored after the Q is fixed 
                if (busTemp.calcType == 1)
                {

                    if (calcGenBusVolt < busTemp.regBusVoltSet && busTemp.aggQUpperMargin == 0)
                    {
                        busTemp.genBusVoltSetCalc = busTemp.regBusVoltSet;
                        CustomMessageHandler.Show("[Info] Orignal gen at " + busTemp.I + " Qlimit is restored to the original regulating voltage!");
                        bNoConversion = false;
                    }

                    if (calcGenBusVolt > busTemp.regBusVoltSet && busTemp.aggQBottomMargin == 0)
                    {
                        busTemp.genBusVoltSetCalc = busTemp.regBusVoltSet;
                        CustomMessageHandler.Show("[Info] Orignal gen at " + busTemp.I + " Qlimit is restored to the original regulating voltage!");
                        bNoConversion = false;
                    }
                }
            }
            return bNoConversion;
        }

        /*
         * rest all generators buses back to PV type if they are on limit  
         */
        public void restGenVoltReg()
        {
            foreach (bus busTemp in genBusList)
            {
                // check if the regulating voltage can be restored after the Q is fixed 
                if (busTemp.calcType == 1)
                {
                    busTemp.calcType = 2;
                    busTemp.genBusVoltSetCalc = AVsol.getQuick(busTemp.vmagPos, 0);
                    CustomMessageHandler.Show("Gen at " + busTemp.I + " regulating votlage is reset");
                }
            }
        }


        /*
         *  check if the setting voltage is within the allowable regulation range 
         *  if not, reset local voltage settings
         *  [created on 04/29/13] to limit Q by changing the terminal voltage regulation voltage settings 
         */
        /*
        public boolean checkPV2PQTemp(){
            boolean bNoViolation = true;

            for (bus busTemp: genBusList) {
                busTemp.QGen = PQsol.getQuick(busTemp.vmagPos, 0) + busTemp.cPLoadQ + busTemp.cCLoadQ*AVsol.getQuick(busTemp.vmagPos,0);

                // calculate the upper and bottom margins 
                busTemp.qUpperMargin  = busTemp.qMax - busTemp.QGen; 
                busTemp.qBottomMargin = busTemp.QGen - busTemp.qMin; 

                // estimate the allowable local voltage regulation setting 
                busTemp.estLocVSetMax = (busTemp.qMax - busTemp.QGen)/busTemp.VQSens + busTemp.genBusVoltSetCalc;
                busTemp.estLocVSetMin = (busTemp.qMin - busTemp.QGen)/busTemp.VQSens + busTemp.genBusVoltSetCalc; 

                if (-busTemp.qUpperMargin > DEFAULT_GENQTol || -busTemp.qBottomMargin > DEFAULT_GENQTol) {

                    // CustomMessageHandler.Show("QMax = " + busTemp.qMax + " QMin = " + busTemp.qMin + " Q = " + busTemp.QGen); 
                    // check if the allowable range covers the original voltage setting 
                    if (busTemp.estLocVSetMax < busTemp.regBusVoltSet) {
                        busTemp.genBusVoltSetCalc = busTemp.estLocVSetMax;
                        CustomMessageHandler.Show("PV bus at " + busTemp.I + " hit Qmax limit " + "genTerminalVoltSet --> " + busTemp.genBusVoltSetCalc);
                        bNoViolation = false; 
                    } else if (busTemp.estLocVSetMin > busTemp.regBusVoltSet) {
                        CustomMessageHandler.Show("PV bus at " + busTemp.I + " hit Qmin limit)" + "genTerminalVoltSet --> " + busTemp.genBusVoltSetCalc);
                        busTemp.genBusVoltSetCalc = busTemp.estLocVSetMin;
                        bNoViolation = false; 
                    } 
                } else {
                    if (busTemp.genBusVoltSetCalc!=busTemp.regBusVoltSet){
                        busTemp.genBusVoltSetCalc = busTemp.regBusVoltSet;
                        bNoViolation = false; 
                    }
                }	
            }
            return bNoViolation;
        }
        */



        /*
         * calculate bus PQ injection based on existing network solution 
         */
        public void calcPQ()
        {

            PQsol = new SparseDoubleMatrix2D(2 * nYBus, 1);
            double pTemp, qTemp;
            double Vi, Vj, Ai, Aj, Gij, Bij, sinAij, cosAij;

            foreach (bus busTemp in sortBusArrayList)
            {
                Ai = AVsol.getQuick(busTemp.vangPos, 0);
                Vi = AVsol.getQuick(busTemp.vmagPos, 0);
                pTemp = Vi * Vi * yMat.yMatRe.getQuick(busTemp.yMatIndx, busTemp.yMatIndx);
                qTemp = -Vi * Vi * yMat.yMatIm.getQuick(busTemp.yMatIndx, busTemp.yMatIndx);

                // [3] append branch related jacobian matrix elements 
                foreach (bus neigBus in busTemp.neighborBusList)
                {
                    Aj = AVsol.getQuick(neigBus.vangPos, 0);
                    Vj = AVsol.getQuick(neigBus.vmagPos, 0);
                    sinAij = Math.Sin(Ai - Aj);
                    cosAij = Math.Cos(Ai - Aj);
                    Gij = yMat.yMatRe.getQuick(busTemp.yMatIndx, neigBus.yMatIndx);
                    Bij = yMat.yMatIm.getQuick(busTemp.yMatIndx, neigBus.yMatIndx);
                    pTemp = pTemp + Vi * Vj * (Gij * cosAij + Bij * sinAij);
                    qTemp = qTemp + Vi * Vj * (Gij * sinAij - Bij * cosAij);
                }
                PQsol.setQuick(busTemp.vangPos, 0, pTemp);
                PQsol.setQuick(busTemp.vmagPos, 0, qTemp);
            }

            /*
            // ### for (int i=0;i<nYBus;i++){
            // ###	pTemp = 0.0;
            // ###	qTemp = 0.0; 
            // ###	Ai = AVsol.getQuick(i, 0);
            // ### Vi = AVsol.getQuick(i+nYBus, 0);	

            // ###	for (int j=0;j<nYBus;j++){				
            // ###		Aj = AVsol.getQuick(j,0);
            // ###		Vj = AVsol.getQuick(j+nYBus,0); 
            // ###		Gij = yMat.yMatRe.getQuick(i,j);
            // ###		Bij = yMat.yMatIm.getQuick(i,j); 
            // ###		sinAij = Math.sin(Ai-Aj);
            // ###		cosAij = Math.cos(Ai-Aj); 				
            // ###		pTemp  = pTemp + Vi*Vj*(Gij*cosAij + Bij*sinAij); 
            // ###		qTemp  = qTemp + Vi*Vj*(Gij*sinAij - Bij*cosAij);
            // ###	}	

            // ###	PQsol.setQuick(i, 0, pTemp); 
            // ###	PQsol.setQuick(i+nYBus, 0, qTemp); 
            // ###}
            */
        }


        /*
         * calculate P and Q object for each bus (additional power injection is taken into accounts)
         */
        public void setPQObj()
        {
            PQobj = new SparseDoubleMatrix2D(2 * nYBus, 1);
            foreach (bus busTemp in sortBusArrayList)
            {
                busTemp.calcExtPQInj(AVsol.getQuick(busTemp.vmagPos, 0));
                // set bus power injection setting 
                PQobj.setQuick(busTemp.vangPos, 0, busTemp.extPInj);
                PQobj.setQuick(busTemp.vmagPos, 0, busTemp.extQInj);
            }
        }


        /*
         *  calculate the power flow solution mismatch 
         *  	for PQ bus: Pset- Pcalc; Qset - Qcalc;
         *  	for PV bus: Pset- Pcalc; Vst  - Vcalc; 
         */
        public DoubleMatrix2D pqDif(DoubleMatrix2D A, DoubleMatrix2D B)
        {
            DoubleMatrix2D ret = new SparseDoubleMatrix2D(2 * nYBus, 1);

            int row = A.rows();
            if (A.columns() != 1)
            {
                CustomMessageHandler.Show("[Error] matrix column number needs to be 1");
                Environment.Exit(0);
            }
            for (int i = 0; i < row; i++)
            {
                ret.setQuick(i, 0, A.getQuick(i, 0) - B.getQuick(i, 0));
            }

            for (int i = 0; i < nYBus; i++)
            {
                if (sortBusArrayList.get(i).calcType == 2)
                {
                    bus busTemp = sortBusArrayList.get(i);
                    //ret.setQuick(i+nYBus, 0, 0.0);														// delta_Q --> using the adding large number method to update Jacobian
                    ret.setQuick(i + nYBus, 0, busTemp.genBusVoltSetCalc - AVsol.get(busTemp.vmagPos, 0));  // delta_V --> Zero rows and columns and put 1 at the diagonal position

                }
                else if (sortBusArrayList.get(i).calcType == 3)
                {
                    ret.setQuick(i, 0, 0.0);            // delta_P
                    ret.setQuick(i + nYBus, 0, 0.0);        // delta_Q
                }
            }
            return ret;
        }



        // Update corresponding results after power flow is converged 
        public void updateResults()
        {

            // update converged voltage to busArrayList element 
            foreach(bus busTemp in sortBusArrayList)
            {
                busTemp.ang = AVsol.getQuick(busTemp.vangPos, 0);
                busTemp.volt = AVsol.getQuick(busTemp.vmagPos, 0);

                // update load
                busTemp.aggTotalPLoad = busTemp.aggCPLoadP + busTemp.volt * busTemp.aggCCLoadP + busTemp.volt * busTemp.volt * busTemp.aggCYLoadP;
                busTemp.aggTotalQLoad = busTemp.aggCPLoadQ + busTemp.volt * busTemp.aggCCLoadQ - busTemp.volt * busTemp.volt * busTemp.aggCYLoadQ;

                busTemp.aggPGen = PQsol.getQuick(busTemp.vangPos, 0) + busTemp.aggTotalPLoad;
                busTemp.aggQGen = PQsol.getQuick(busTemp.vmagPos, 0) + busTemp.aggTotalQLoad;

                // split generator MW and MVar outputs
                busTemp.updateResults();
            }

            // update branch calculated variables and system losses 
            foreach (branch branchTemp in branchArrayList)
            {
                branchTemp.calPQFlow();
                sysPLoss = sysPLoss + branchTemp.pLoss;
                sysQLoss = sysQLoss + branchTemp.qLoss;
            }

            // update transformer power and update system losses 
            foreach (twoWindTrans transTemp in twoWindTransArrayList)
            {
                transTemp.calPQFlow();
                sysPLoss = sysPLoss + transTemp.pLoss;
                sysQLoss = sysQLoss + transTemp.qLoss;
            }
        }


        public bool getPFConvergeStatus()
        {
            return bConverged;
        }


        // display results 
        public void displayResults()
        {

            // display bus voltages 
            for (int i = 0; i < sortBusArrayList.size(); i++)
            {
                String strTemp = "";
                bus busTemp = sortBusArrayList.get(i);
                strTemp = strTemp + "Bus " + String.Format("%2d", busTemp.I)
                        + "	\tVolt = " + String.Format("%2.5f", busTemp.volt)
                        + "	\tAng = " + String.Format("%2.5f", busTemp.ang * Rad2Deg)
                        + "	\tPLoad = " + String.Format("%4.2f", busTemp.aggTotalPLoad * setSBASE)
                        + "	\tQLoad = " + String.Format("%4.2f", busTemp.aggTotalQLoad * setSBASE)
                        + "	\tPGen  = " + String.Format("%4.2f", busTemp.aggPGen * setSBASE)
                        + "	\tQGen  = " + String.Format("%4.2f", busTemp.aggQGen * setSBASE);
                CustomMessageHandler.Show(strTemp);
            }

            // display system power losses 
            CustomMessageHandler.Show("Sys Losses " + sysPLoss + " +j " + sysQLoss);
        }


        /*
         * initial voltage regulating control 
         */
        public void loadBusUnderVoltReg()
        {

            // [1] setup regulated buses (generator)
            foreach (bus busTemp in sortBusArrayList)
            {
                if (busTemp.bBusHasRegGen)
                {
                    bus regulatedBus = dataProcess.getBusAt(busTemp.genRegBusNum, sortBusArrayList);
                    regulatedBus.bVoltRegulated = true;
                    regulatedBus.VoltRegulatedSet = busTemp.regBusVoltSet;
                }
            }

            // [2] setup regulated buses (switchable shunt)
            foreach (bus busTemp in sortBusArrayList)
            {
                if (busTemp.bHasSwShunt)
                {
                    bus regulatedBus = dataProcess.getBusAt(busTemp.swshuntRegBusNum, sortBusArrayList);
                    regulatedBus.bVoltRegulated = true;
                    regulatedBus.VoltRegulatedSet = busTemp.regBusVoltSet;
                }
            }

            // [3] setup the voltage regulating transformer 
            foreach (twoWindTrans twoWindTransTemp in twoWindTransArrayList)
            {
                if (twoWindTransTemp.COD1 == 1)
                {
                    bus regulatedBus = dataProcess.getBusAt(twoWindTransTemp.CONT1, sortBusArrayList);
                    regulatedBus.bVoltRegulated = true;
                    regulatedBus.VoltRegulatedSet = twoWindTransTemp.VMI1;
                }
            }
        }





    }
}
