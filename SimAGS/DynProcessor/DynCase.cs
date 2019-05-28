using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cern.colt.matrix;
using cern.colt.matrix.impl;
using cern.colt.matrix.linalg;
using java.io;
using java.lang;
using SimAGS;
using SimAGS.Components;
using SimAGS.DistEvent;
using SimAGS.DynModels.AgcModel;
using SimAGS.Handlers;
using SimAGS.PfProcessor;
using SimAGS.SimUtil;
using Math = java.lang.Math;
using String = com.sun.org.apache.xpath.@internal.operations.String;

namespace SimAGS.DynProcessor
{
    public class DynCase
    {

        //public final int MAX_ITER  = 20; 			// maximum iteration number 
        //public final double DEFAULT_F_TOL = 1E-5;	// tolerance for initial value of dx/dt 	
        //public final double SBASE = 100.0;			// system base

        // simulation parameters 
        public double setSBASE;                 // system base for dyn sim
        public double setEndTime;               // simulation ending time
        public double setDyntol;                // tolerance for NR method
        public int setDynMaxItr;                // set maximum iteration number
        public double setIntMaxStep;            // set maximum integration step size 

        // dynamic load conversion related 
        public bool bEnableLoadConv = false;     // convert load to specified ZIP model (if enabled, constant impedance model of P and Q will be applied by default)? 
        public bool bEnableLoaFreq = false;      // enable model frequency component in load modeling?
        public double loadConvZP_Pct = 100.0;       // percentage of constant impedance MW load
        public double loadConvIP_Pct = 0.0;     // percentage of constant current MW load 
        public double loadConvPP_Pct = 0.0;     // percentage of constant power MW load
        public double loadConvZQ_Pct = 100.0;       // percentage of constant impedance MVar load
        public double loadConvIQ_Pct = 0.0;     // percentage of constant current MVar load 
        public double loadConvPQ_Pct = 0.0;     // percentage of constant power MVar load
        public double loadP_FreqCoef = 0.0;     // frequent component coefficient for MW load
        public double loadQ_FreqCoef = 0.0;     // frequent component coefficient for MVar load

        public PFCase pfCase;                   // converged power flow case 
        public List<branch> branchList = new List<branch>();
        public List<agcModel> AGCList = new List<agcModel>();

        // global variable
        public int numState = 0;                // total number of system state variables 
        public int numAlgeVar = 0;              // total number of system algebraic variables 
        public int numNetVar = 0;               // total number of system network variables 
        public DoubleMatrix2D xVector;
        public DoubleMatrix2D yVector;
        public DoubleMatrix2D fVector;          // dx/dt = f(x,y) 
        public DoubleMatrix2D gAdjVector;

        public DoubleMatrix2D dfVector;
        public DoubleMatrix2D dgVector;
        public DoubleMatrix2D fxMat;
        public DoubleMatrix2D fyMat;
        public DoubleMatrix2D gxMat;
        public DoubleMatrix2D gyMat;

        public DoubleMatrix2D JacAll;

        public DoubleMatrix2D fVector_n;
        public DoubleMatrix2D xVector_n;
        public DoubleMatrix2D yVector_n;

        public yMatrix yMat;
        public List<bus> busList;
        public int nYBus;
        public Algebra matOperator = new Algebra();

        public int nGENCLS = 0;
        public int nGENROU = 0;
        public int nTGOV1 = 0;
        public int nHYGOV = 0;
        public int nIEEEG2 = 0;
        public int nESDC1A = 0;
        public int nESAC4A = 0;
        public int nAGC = 0;
        public int nFreqBus = 0;
        public int nWindDyn = 0;
        public int nDynLoadUDM = 0;
        public int nEvent = 0;

        public double t = 0.0;              // global parameter  
        public double h = 0.0;              // time step 
        public double hStepMax = 0.05;          // maximum time step 
        public double hStepMin = 0.002;             // minimum time step 
        public int hDecStepCount = 0;               // 
        public int itrNum = 0;              // store iteration number 
        public bool simDone = false;
        public double simTheta = 0.0;               // theta 
        public bool bSolved = false;
        //public double dynTol   = 0.0;				// dynamic simulation tolerance 
        //public double tEnd = 0.0;					// simulation ending time 

        // data for result viewing
        //public List<Object[]> resultHeader = new List<Object[]>();		// 1-> Dynamic type (Generator); 2-> Bus#; 3-> ID; 4-> Variable; 5-> Unit; 6-> index in xVector; 7; index in yVector (=0 if not exists)
        //public List<double []>	resultMat 	= new List<double[]>(); 
        //public List<Double> timeMat 		= new List<Double>();			// store the time instant

        // for disturbance modeling
        public distEventList dynEventList;          // real eventList dynamically adjusted 

        // interruption 
        public volatile bool bStop = false;
        public bool resetVangIni = false;

        // convert load to constant impedance 
        public bool bConvLoad = false;

        // for thread control 
        public System.Threading.Thread calcThread;
        public volatile bool bInterrupt = false;             // used to interrupt program for stop button 

        // load dynamic data process
        public DynCaseLoad loadInst;

        // simulation result buffer
        public simBuffer simResultBuffer;

        // default constructor 
        public DynCase(PFCase PfCase)
        {
            pfCase = PfCase;
            dynEventList = new distEventList(this);     // event list
            busList = pfCase.sortBusArrayList;
            branchList = pfCase.branchArrayList;
            itrNum = 0;
        }


        // update simulation parameters 
        public void loadSimPara(double setSBASE, double setEndTime, double setDynTol, int setDynMaxItr, double setIntStep, int NRType, double AGCStep)
        {
            this.setSBASE = setSBASE;
            this.setEndTime = setEndTime;
            this.setDyntol = setDynTol;
            this.setDynMaxItr = setDynMaxItr;
            this.setIntMaxStep = setIntStep;
        }

        // update load conversion data 
        public void loadDynLoadConvPara(bool bEnableLoadConv, bool bEnableLoaFreq, double loadConvZP_Pct, double loadConvIP_Pct, double loadConvPP_Pct, double loadConvZQ_Pct, double loadConvIQ_Pct, double loadConvPQ_Pct, double loadP_FreqCoef, double loadQ_FreqCoef)
        {
            this.bEnableLoadConv = bEnableLoadConv;
            this.bEnableLoaFreq = bEnableLoaFreq;
            this.loadConvZP_Pct = loadConvZP_Pct;
            this.loadConvIP_Pct = loadConvIP_Pct;
            this.loadConvPP_Pct = loadConvPP_Pct;
            this.loadConvZQ_Pct = loadConvZQ_Pct;
            this.loadConvIQ_Pct = loadConvIQ_Pct;
            this.loadConvPQ_Pct = loadConvPQ_Pct;
            this.loadP_FreqCoef = loadP_FreqCoef;
            this.loadQ_FreqCoef = loadQ_FreqCoef;

            loadInst.dynLoadCov(bEnableLoadConv, bEnableLoaFreq, loadConvZP_Pct, loadConvIP_Pct, loadConvPP_Pct, loadConvZQ_Pct, loadConvIQ_Pct, loadConvPQ_Pct, loadP_FreqCoef, loadQ_FreqCoef);                                                   // load conversion data 
        }

        //---------------------------------------------------------------------------------------//
        //------------------------------------ load dynamic data---------------------------------//
        //---------------------------------------------------------------------------------------//

        public void loadCaseFile(File dynDataFile, File AGCDataFile, File windDataFile, File contDataFile, File genSchdDataFile, File loadSchdDataFie)
        {
            loadInst = new DynCaseLoad(this);
            loadInst.exec(dynDataFile, AGCDataFile, windDataFile, contDataFile, genSchdDataFile, loadSchdDataFie);
        }

        // create dynamic simulation buffer
        public void buildSimResultBuffer()
        {
            simResultBuffer = new simBuffer(this);
        }


        //---------------------------------------------------------------------------------------//
        //----------------------------- time-domain integration ---------------------------------//
        //---------------------------------------------------------------------------------------//

        // initialize system dynamic models for simulations  
        public void init()
        {

            nYBus = pfCase.nYBus;
            numNetVar = 2 * nYBus;

            //----------------- rebuilt Y matrix without P(V,t) and Q(V,t) modeled ----------// 
            yMat = new yMatrix(pfCase, false);

            // -------------------- initialize yVector and xVector --------------------------//
            // * iterate all buses, branches, AGCs to initialize network and state variables
            xVector = new SparseDoubleMatrix2D(numState, 1);
            yVector = new SparseDoubleMatrix2D(numAlgeVar, 1);
            fVector = new SparseDoubleMatrix2D(numState, 1);

            // * bus related element
            foreach (bus busTemp in busList) busTemp.dynIni(yVector, xVector);        // initialize yVector and xVector 

            // * branch related element
            foreach (branch branTemp in branchList)
            {
                if (branTemp.bHasMWFlowMeaseure) branTemp.MWFlowMon.ini(yVector, xVector);
            }

            // * AGC related element
            foreach (agcModel agcTemp in AGCList)
            {
                agcTemp.deactivate();
                agcTemp.ini(yVector, xVector);
            }
            // -------------END: initialize yVector and xVector----------------//

            // calculate initial value of dx/dt which may not be ZEROS 
            if (matOperator.normInfinity(fVector.viewColumn(0)) > setDyntol)
            {
                // interrupt program and pops up error message 
                throw new simException("ERROR: dx/dt initial value exceeds tolerance!");
            }

            // reset eventList
            //dynEventList = new distEventList(DynCase.this);
            dynEventList.reset();

            // re-flush buffer to store results 
            //simResultBuffer.reset(); 

            // ##### Debug Code ######################//
            //dataProcess.dispMat(xVector);
            //CustomMessageHandler.println("----------------");
            //dataProcess.dispMat(yVector);
            // #######################################//

            CustomMessageHandler.println(">>>>>>>> ... Initialization of dynamic model is completed!");
        }


        // main simulation function 
        public bool sim()
        {
            simTheta = 0.5;                                                             // simTheta for extended simulation simTheta = 1.0 for switching event 
            t = 0;                                                                      // current time in seconds  
            h = setIntMaxStep;

            xVector_n = new SparseDoubleMatrix2D(numState, 1);                           // network variable at the time instant of n-1
            yVector_n = new SparseDoubleMatrix2D(numAlgeVar, 1);
            fVector_n = new SparseDoubleMatrix2D(numState, 1);                           // dx/dt at the time instant of n-1 
                                                                                         //DoubleMatrix2D yVectorStore = new SparseDoubleMatrix2D(numAlgeVar,1);		// store the network solution before fault

            gAdjVector = new SparseDoubleMatrix2D(numAlgeVar, 1);                        // define P injection adjustment vector

            xVector_n = xVector;
            yVector_n = yVector;
            fVector_n = fVector;

            // main program thread 
            calcThread = new System.Threading.Thread(() => //new Runnable()
            {
                // overwrite running behaviors 
                CustomMessageHandler.println(">>>>>>>> ... Time domain simulation begins!");
                DateTime startTime = DateTime.Now;

                while (true && !bInterrupt)
                {                                                       // outside loop for entire simulation duration 	

                    //reset bus frequency calculation module  
                    //update_BusFreqMeasurement(h, yVector_n); 
                    bSolved = intStep(xVector_n, fVector_n, h, simTheta, setDyntol);                // results stored in xVector and yVector 

                    // for divergent case, reduce the step size to hStepMin and try that again 
                    if (bSolved == false && h != hStepMin)
                    {
                        h = hStepMin;
                        xVector = xVector_n;
                        yVector = yVector_n;
                        bSolved = intStep(xVector_n, fVector_n, h, simTheta, setDyntol);            // results stored in xVector and yVector 				
                        CustomMessageHandler.println("========== Time step is reduced to hStepMin for divergence case ============");
                    }

                    if (bSolved == false)
                    {                                                   // diverged after maximum iteration number
                        CustomMessageHandler.println("ERROR: <Max iteration number is reached> Simluation terminates at t = " + t);
                        break;
                    }
                    else
                    {
                        // ------------- store output variables and update time instant -----------------// 
                        //storeResult(t); 
                        simResultBuffer.insert(t);

                        string strDisp = "---->" + _String.format("%8.5f", t) + "\t " +
                                //_String.format("%1.5f", busList.get(6).volt) + "\t " + 
                                _String.format(" iter = %2d", itrNum) + "\t " +
                                _String.format(" hStep = %5.3f", h) + "\t " +
                                //_String.format(" PLoad = %5.3f @bus%3d", busList.get(11).dynLoad.getDynPInj(), busList.get(11).I) + "\t" + 
                                //_String.format("%10.3f", busList.get(23).getAngle(yVector)) + "\t" + 
                                //_String.format("%10.3f", busList.get(23).getVolt(yVector)) + "\t" + 
                                //_String.format("%10.3f", yVector.getQuick(branchList.get(9).MWFlow_Pos,0)) + "\t" + 
                                //_String.format("%1.5f", findGenAt(1, "U1").getMWSetting()) + "\t " + 
                                //_String.format("%10.3f", busList.get(8).dynLoad.getDynPInj()) + "\t " + 
                                //_String.format("%10.3f", busList.get(8).dynLoad.getDynQInj()) + "\t " + 
                                //_String.format("%10.3f", busList.get(0).busGens.get(0).govDyn.limitTest()) + "\t " + 
                                //_String.format("%1.5f", genList.get(3).govDyn.getPRef()) + "\t " + 
                                //_String.format("%1.2f", genList.get(1).excDyn.getVr()) + "\t " + 
                                //_String.format("%1.5f", yVector.getQuick(busList.get(4).vmagPos,0)); // + "\t " + 
                                //_String.format("%1.5f", genList.get(1).excDyn.getEfd());
                                //_String.format("%1.5f", AGCGrpList.get(0).get_ACE()) + " \t" + 
                                //_String.format("%1.5f", AGCCtrl.getpRefTotal()) + 
                                "";
                        CustomMessageHandler.println(strDisp);

                        // --------------- store the current value as the x@(n-1) and f@(n-1) ---------------//
                        xVector_n = xVector;
                        yVector_n = yVector;
                        fVector_n = fVector;

                        //--------------------------- determine next time step ----------------------------//
                        // [1] time step adjustment 
                        if (itrNum < 10)
                        {           // result converged quickly 
                            hDecStepCount++;
                            if (hDecStepCount >= 0)
                            {
                                h = Math.min(2 * h, hStepMax);
                            }
                        }
                        else if (itrNum < 15)
                        {
                            h = Math.max(h / 2, hStepMin);
                            hDecStepCount = -10;                            // If step is reduced, only after ten successful time steps are used then time step increases
                        }
                        else
                        {
                            h = hStepMin;
                        }

                        // [2] time step adjustment for event 
                        h = (t + h > setEndTime) ? setEndTime - t : h;              // (a) check if get the ending time 
                        h = dynEventList.getTimeInc(h, t);                  // (b) check if the disturbance will occur

                        //---------check if all time discrete event (AGC or OLTC) works ----------//
                        dynEventList.applyEvent(h);                         // check if discrete event needs to be applied (when bEventOccur == true, apply fault)

                        // calculate t0 instant 
                        if (dynEventList.bT0Calc == true)
                        {
                            if (calcT0Plus() == false)
                            {
                                simDone = false;
                            }
                            else
                            {
                                if (dynEventList.bT0Calc)
                                {
                                    dynEventList.bEventOccur = false;               // rest for next round 

                                    // store current AGC results and activate it 	// define the next activation time instance for AGC
                                    foreach (agcModel agcTemp in AGCList)
                                    {
                                        if (agcTemp.isActivate())
                                        {
                                            agcTemp.store_Var();
                                            agcTemp.deactivate();
                                            dynEventList.addEvent(new actAGC(t + agcTemp.getNextTimeInterval(), agcTemp));
                                        }
                                    }
                                }
                            }
                        }

                        // go to the next time instant 
                        if (t + 1e-6 > setEndTime)
                        {
                            simDone = true;
                            break;
                        }
                        else
                        {
                            t = t + h;
                        }
                    }
                }

                // display simulation final status
                if (simDone == true)
                {
                    CustomMessageHandler.println("======== Simulaiton is Completed [Time = " + _String.format("%1.3f", (double)(DateTime.Now - startTime).Milliseconds / 1000) + " seconds] =========");
                }
                else
                {
                    // interrupted case
                    if (bInterrupt == true)
                    {
                        CustomMessageHandler.println("Info: Program is terminated!");
                    }
                    else
                    {
                        CustomMessageHandler.println("### Error: simulation terminates for divergence at " + _String.format("%1.3f", t));
                        //throw new simException();
                    }
                }

            });
            calcThread.Start();

            return bSolved;
        }


        // calculate network variable at t0+ [assuming dynamic variables keeps constant]
        public bool calcT0Plus()
        {
            bool bSolved = false;
            simTheta = 1.0;
            if (bSolved = intStep(xVector_n, fVector_n, 0, simTheta, setDyntol) == true)
            {
                //storeResult(t); 												// store t0+ time instant (assume that t0+ = t0- = t0)
                simResultBuffer.insert(t);
                // update network variables at n-1 time instant		
                yVector_n = yVector;
                simTheta = 0.5;
                h = hStepMin;                                                   // after event reduce time step
            }
            return bSolved;
        }



        // integrate for one time step 
        public bool intStep(DoubleMatrix2D xVector_n, DoubleMatrix2D fVector_n, double h, double simTheta, double tol)
        {

            bool bSolved = false;
            DoubleMatrix2D dfVector_n = update_fVector_n(xVector_n, fVector_n, h, simTheta);            // calculate dfVector_n = [(1-theta)*tStep*fn-1] + [xn-1]
            update_BusFreqMeasurement(h, yVector_n);                                                    // stored bus frequency at the previous step; 

            for (int i = 0; i < setDynMaxItr; i++)
            {                                                           // inner loop for each time instants			
                                                                        // calculate variable deviations 
                update_Variables(t + h);                                                                    // update network and state variables  
                dgVector = update_dg();
                fVector = update_fVector();

                if (simTheta != 1.0)
                {
                    dfVector = update_df(dfVector_n, h, simTheta);                                      // fVector is included in computation
                }
                else
                {
                    dfVector = new SparseDoubleMatrix2D(numState, 1);                                   // switching event 
                }
                DoubleMatrix2D calcDif = dataProcess.combVector(dgVector, dfVector);

                // calculate full Jacobian matrix 
                JacAll = updateJacAll(h, simTheta);                                                     // fxMat, fyMat, gxMat, and gyMat				

                // calculate variable increment
                DoubleMatrix2D dVar = matOperator.solve(JacAll, calcDif);

                double mismatch = matOperator.normInfinity(dVar.viewColumn(0));
                //CustomMessageHandler.println("itr = " + i + " mistmatch = " + mismatch); 

                //CustomMessageHandler.println("========== g equ ================");
                //dataProcess.dispMat(dVar.viewPart(0,0,numAlgeVar,1)); 

                //CustomMessageHandler.println("========== f equ ================");
                //dataProcess.dispMat(dVar.viewPart(numAlgeVar,0,numState,1));

                //System.exit(0);

                if (mismatch < tol)
                {
                    itrNum = i + 1;
                    bSolved = true;
                    break;
                }

                yVector = dataProcess.matAdd(yVector, dVar.viewPart(0, 0, numAlgeVar, 1), 1, 1);
                xVector = dataProcess.matAdd(xVector, dVar.viewPart(numAlgeVar, 0, numState, 1), 1, 1);
            }
            return bSolved;
        }



        // update fVector at n-1 time instant 
        public DoubleMatrix2D update_fVector_n(DoubleMatrix2D xVector_n, DoubleMatrix2D fVector_n, double h, double simTheta)
        {
            DoubleMatrix2D ret = new SparseDoubleMatrix2D(numState, 1);
            for (int i = 0; i < numState; i++)
            {
                ret.setQuick(i, 0, (1 - simTheta) * h * fVector_n.getQuick(i, 0) + xVector_n.getQuick(i, 0));
            }
            return ret;
        }


        // public power balance equation difference (PQ_GEN - PQ_NetWork - PQ_Load = 0)
        public DoubleMatrix2D update_dg()
        {
            DoubleMatrix2D ret = new SparseDoubleMatrix2D(numAlgeVar, 1);

            //----------------- calculate power injection from network ----------------//
            double pTemp, qTemp;
            double Vi, Vj, Ai, Aj, Gij, Bij, sinAij, cosAij;

            for (int i = 0; i < nYBus; i++)
            {
                pTemp = 0.0;
                qTemp = 0.0;
                Ai = yVector.getQuick(i, 0);
                Vi = yVector.getQuick(i + nYBus, 0);

                for (int j = 0; j < nYBus; j++)
                {                   // select non-zeros elements
                    Aj = yVector.getQuick(j, 0);
                    Vj = yVector.getQuick(j + nYBus, 0);
                    Gij = yMat.yMatRe.getQuick(i, j);
                    Bij = yMat.yMatIm.getQuick(i, j);
                    sinAij = Math.sin(Ai - Aj);
                    cosAij = Math.cos(Ai - Aj);
                    pTemp = pTemp + Vi * Vj * (Gij * cosAij + Bij * sinAij);
                    qTemp = qTemp + Vi * Vj * (Gij * sinAij - Bij * cosAij);
                }
                ret.setQuick(i, 0, -pTemp);
                ret.setQuick(i + nYBus, 0, -qTemp);
            }

            // calculate dynamic model power injection 
            foreach (bus busTemp in busList)
            {
                //-------------- calculate dynamic model power injection -----------// 
                foreach (gen genTemp in busTemp.busGens)
                {
                    genTemp.genDyn.update_g(ret);

                    // update governor 
                    if (genTemp.hasGovModel) genTemp.govDyn.update_g(ret);

                    // update exciter 
                    if (genTemp.hasExcModel) genTemp.excDyn.update_g(ret);
                }

                // --------------- update bus frequency impacts --------------------//
                if (busTemp.bHasFreqMeasure)
                {
                    if (simTheta != 1)
                    {
                        busTemp.BusfreqCalc.update_g(ret);
                    }
                    else
                    {
                        busTemp.BusfreqCalc.update_gT0(ret);
                    }
                }

                //--------------------- load power injection ----------------------//		
                if (busTemp.bHasDynLoad) busTemp.dynLoad.update_g(ret);                 // calculate dynamic load power injection and apply the "Load" power injection  

                //------------- add wind power or sudden load change --------------// 
                //if (busTemp.bWindMWEnable) busTemp.windModel.update_g(gfVector);
            }

            // branch power flow measurement model 
            //for (branch branTemp in branchList) {
            //	if (branTemp.bHasMWFlowMeaseure) {
            //		branTemp.MWFlowMon.update_g(gfVector);
            //	}
            //}

            // update g function associated with AGC
            //for (agcModel agcTemp in AGCList){
            //	agcTemp.update_g(gfVector);
            //}

            return ret;
        }


        // update f deviation 
        public DoubleMatrix2D update_df(DoubleMatrix2D dfVector_n, double h, double simTheta)
        {
            DoubleMatrix2D ret = new SparseDoubleMatrix2D(numState, 1);
            for (int i = 0; i < numState; i++)
            {
                ret.setQuick(i, 0, dfVector_n.getQuick(i, 0) - xVector.getQuick(i, 0) + simTheta * h * fVector.getQuick(i, 0));
            }
            return ret;
        }


        // -------------------------update full jacobian matrix during simulation --------------------------------//
        public DoubleMatrix2D updateJacAll(double h, double simTheta)
        {

            // formulate Jacobian matrix 
            DoubleMatrix2D ret = new SparseDoubleMatrix2D(numAlgeVar + numState, numAlgeVar + numState);

            // initialize gy and results stored in gyMat 
            jacob.update(yMat, yVector, ret, busList, 1);

            foreach (bus busTemp in busList)
            {
                // for remaining generator model 
                foreach (gen genTemp in busTemp.busGens)
                {
                    genTemp.genDyn.update_gy(ret, 0, 0);                // 0, 0 initial row number, column number in ret (ret is re-written)
                    genTemp.genDyn.update_gx(ret, 0, numAlgeVar);
                    genTemp.genDyn.update_fy(ret, numAlgeVar, 0, simTheta, h);
                    genTemp.genDyn.update_fx(ret, numAlgeVar, numAlgeVar, simTheta, h);

                    if (genTemp.hasGovModel)
                    {
                        genTemp.govDyn.update_gy(ret, 0, 0);
                        genTemp.govDyn.update_gx(ret, 0, numAlgeVar);
                        genTemp.govDyn.update_fy(ret, numAlgeVar, 0, simTheta, h);
                        genTemp.govDyn.update_fx(ret, numAlgeVar, numAlgeVar, simTheta, h);
                    }

                    if (genTemp.hasExcModel)
                    {
                        genTemp.excDyn.update_gy(ret, 0, 0);
                        genTemp.excDyn.update_gx(ret, 0, numAlgeVar);
                        genTemp.excDyn.update_fy(ret, numAlgeVar, 0, simTheta, h);
                        genTemp.excDyn.update_fx(ret, numAlgeVar, numAlgeVar, simTheta, h);
                    }
                }

                // for bus measurement model 
                if (busTemp.bHasFreqMeasure)
                {
                    if (simTheta != 1)
                    {
                        busTemp.BusfreqCalc.update_gy(ret, 0, 0);
                    }
                    else
                    {
                        busTemp.BusfreqCalc.update_gyT0(ret, 0, 0);
                    }
                }

                // for dynamic load model 
                if (busTemp.bHasDynLoad)
                {
                    busTemp.dynLoad.update_gy(ret, 0, 0);
                    busTemp.dynLoad.update_gx(ret, 0, numAlgeVar);
                    busTemp.dynLoad.update_fx(ret, numAlgeVar, numAlgeVar, simTheta, h);
                }

                // for wind MW injection
                if (busTemp.bWindMWEnable)
                {
                    busTemp.windModel.update_gx(ret, 0, numAlgeVar);
                    busTemp.windModel.update_fx(ret, numAlgeVar, numAlgeVar, simTheta, h);
                }
            }

            // branch power flow measurement model 
            foreach (branch branTemp in branchList)
            {
                if (branTemp.bHasMWFlowMeaseure)
                {
                    branTemp.MWFlowMon.update_gy(ret, 0, 0);
                }
            }

            // AGC models 
            foreach (agcModel agcTemp in AGCList)
            {
                agcTemp.update_gy(ret, 0, 0);
                agcTemp.update_gx(ret, 0, numAlgeVar);
                agcTemp.update_fy(ret, numAlgeVar, 0, simTheta, h);
            }

            // identical matrix
            for (int i = 0; i < numState; i++)
            {
                ret.setQuick(numAlgeVar + i, numAlgeVar + i, ret.getQuick(numAlgeVar + i, numAlgeVar + i) + 1);
            }
            return ret;
        }


        // update bus frequency module 
        public void update_BusFreqMeasurement(double h, DoubleMatrix2D yVector_n)
        {
            foreach (bus busTemp in busList)
            {
                if (busTemp.bHasFreqMeasure)
                {
                    busTemp.BusfreqCalc.update_BusFreqMeasurement(h, yVector_n);
                }
            }
        }


        // update state and network variables for dynamic model 
        public void update_Variables(double tCurrent)
        {

            foreach (bus busTemp in busList)
            {
                // update regular dynamic model
                foreach (gen genTemp in busTemp.busGens)
                {
                    // update generator model 
                    genTemp.genDyn.update_Var(yVector, xVector);
                    // update governor model 
                    if (genTemp.hasGovModel) genTemp.govDyn.update_Var(yVector, xVector);
                    // update exciter model  
                    if (genTemp.hasExcModel) genTemp.excDyn.update_Var(yVector, xVector);
                }

                // update bus frequency 
                if (busTemp.bHasFreqMeasure)
                {
                    busTemp.BusfreqCalc.update_Var(yVector, xVector);
                }

                // update dynamic loupdate_dg();ad (frequency dependent) 
                //if (busTemp.bHasLoad) {
                if (busTemp.bHasDynLoad)
                {
                    busTemp.dynLoad.update_Var(yVector, xVector, tCurrent);
                }

                // update wind data
                if (busTemp.bWindMWEnable)
                {
                    busTemp.windModel.update_Var(yVector, xVector);
                }
            }

            foreach (branch branTemp in branchList)
            {
                if (branTemp.bHasMWFlowMeaseure)
                {
                    branTemp.MWFlowMon.update_Var(yVector, xVector);
                }
            }

            // update AGC 
            foreach (agcModel agcTemp in AGCList)
            {
                agcTemp.update_Var(yVector, xVector);
            }
        }


        // update fVecotor based on newly updated xVector and yVector
        public DoubleMatrix2D update_fVector()
        {
            fVector = new SparseDoubleMatrix2D(numState, 1);

            foreach (bus busTemp in busList)
            {
                // update regular dynamic model
                foreach (gen genTemp in busTemp.busGens)
                {
                    // update generator models 
                    genTemp.genDyn.update_f(fVector);

                    // update governor model because it affect pm in generator model 
                    if (genTemp.hasGovModel) genTemp.govDyn.update_f(fVector);

                    // update exciter model because it affects efd in generator model 
                    if (genTemp.hasExcModel) genTemp.excDyn.update_f(fVector);
                }

                // dynamic load model
                if (busTemp.bHasDynLoad) busTemp.dynLoad.update_f(fVector);

                // wind data 
                if (busTemp.bWindMWEnable) busTemp.windModel.update_f(fVector);
            }

            // update AGC-related variables 
            foreach (agcModel agcTemp in AGCList)
            {
                agcTemp.update_f(fVector);
            }

            //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            // update AGC second (because it may affect generator governor state variable subjected to limit)
            // DO NOTHING
            //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            return fVector;
        }


        // reset voltage angle initial value for computing bus frequency 
        //public void resetBusFreqIniAng(){
        //	for (bus busTemp in busList){
        //		if (busTemp.bHasFreqMeasure){
        //			busTemp.BusfreqCalc.updateThetaZero(yVector);			// update frequency theta_0
        //		}
        //	}
        //}

        // functions to add wind data or sudden load change (assume that constant power is added) 
        //public void addLoadMW (int busNum, double addMWVal){
        //	bus busTemp = findBusAt(busNum); 
        //	if (busTemp!=null) {
        //		busTemp.windMWInj = addMWVal; 
        //		busTemp.bWindMWEnable = true; 
        //	}
        //}

        public bus findBusAt(int BusNum)
        {
            foreach (bus busTemp in busList)
            {
                if (busTemp.I == BusNum)
                {
                    return busTemp;
                }
            }
            CustomMessageHandler.println("ERROR: bus(" + BusNum + ") is not found!");
            return null;
        }

    }
}
