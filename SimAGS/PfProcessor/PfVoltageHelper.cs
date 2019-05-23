using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cern.colt.matrix;
using cern.colt.matrix.impl;
using cern.colt.matrix.linalg;
using ikvm.extensions;
using org.ojalgo.matrix.store;
using org.ojalgo.optimisation;
using org.ojalgo.optimisation.quadratic;
using SimAGS.Components;
using SimAGS.Components.ExtendOption;
using SimAGS.Handlers;
using SimAGS.SimUtil;

namespace SimAGS.PfProcessor
{
    public class pfVoltageHelper
    {
        public List<bus> loadBusArrayList;
        public List<bus> genBusArrayList;
        public List<bus> optmLoadBusArrayList;
        public int regVarNum;

        public int[] loadQPosArray;
        public int[] genQPosArray;

        public int[] optmLoadBusPosArray;
        public int[] optmRegCtrPosArray;

        public Algebra matrixOpt = new Algebra();       // matrix operator object 
        public PFCase pfCase;                           // direct to the pfCase 
        public double voltRegTol = 1e-3;                // regulating voltage mismatch

        public DoubleMatrix2D loadBusVoltDif;
        public DoubleMatrix2D LLMat;                    // nAllLoadBus*nAllLoadBus matrix
        public DoubleMatrix2D LGMat;                    // nAllLoadBus*nRegTotalNum matrix 
        public DoubleMatrix2D genRegSensMat;            // nAllLoadBus*nRegTotalNum matrix
        public DoubleMatrix2D optmEquConCofMat;
        public DoubleMatrix2D optmEqubMat;
        public DoubleMatrix2D optmInEquConConfMat;
        public DoubleMatrix2D optmInEqubMat;

        public PhysicalStore.Factory tmpFactory = PrimitiveDenseStore.FACTORY;
        public MatrixStore QMat;
        public MatrixStore PMat;
        public MatrixStore AIMat;
        public MatrixStore BIMat;

        public PQDecoupleJacob decoupleJacMat;

        public bool isEnable = false;

        public solType solveStatus;

        public List<abstractPfElement> regElementList = new List<abstractPfElement>();

        // constructor
        public pfVoltageHelper(PFCase pfCase, double voltRegTol)
        {

            loadBusArrayList = pfCase.loadBusList;
            genBusArrayList = pfCase.genBusList;
            optmLoadBusArrayList = new List<bus>();
            regVarNum = 0;
            this.pfCase = pfCase;
            this.voltRegTol = voltRegTol;
            ini();
        }

        //initialization and assign spaces 
        public void ini()
        {

            // ----------------formulate the index matrix (elements from Jacobian matrix)---------------//
            loadQPosArray = new int[loadBusArrayList.size()];
            for (int i = 0; i < loadBusArrayList.size(); i++)
            {
                loadQPosArray[i] = loadBusArrayList.get(i).yMatIndx;
            }

            genQPosArray = new int[genBusArrayList.size()];
            for (int i = 0; i < genBusArrayList.size(); i++)
            {
                genQPosArray[i] = genBusArrayList.get(i).yMatIndx;
            }

            // ----- formulate the index matrix of voltage controllers in the optimum solution vector ---// 
            foreach (bus busTemp in pfCase.sortBusArrayList)
            {
                if (busTemp.bBusHasRegGen)
                {
                    busTemp.getVoltExtOption().setVoltOptmVarIndx(regVarNum);
                    busTemp.getVoltExtOption().setYMat(pfCase.yMat);
                    busTemp.getVoltExtOption().setLoadBusList(loadBusArrayList);
                    regElementList.add(busTemp);
                    regVarNum++;
                }
            }

            foreach (bus busTemp in pfCase.sortBusArrayList)
            {
                if (busTemp.bHasSwShunt)
                {
                    busTemp.getVoltExtOption().setVoltOptmVarIndx(regVarNum);
                    regElementList.add(busTemp);
                    regVarNum++;
                }
            }

            foreach (twoWindTrans transTemp in pfCase.twoWindTransArrayList)
            {
                if (transTemp.COD1 == 1)
                {
                    int foundCount = 0;
                    // update from and to bus index in load-bus only matrix 
                    foreach (bus busTemp in loadBusArrayList)
                    {
                        if (busTemp.I == transTemp.I)
                        {
                            transTemp.fromLoadBusLLIndx = busTemp.LLIndx;
                            foundCount++;
                        }
                        else if (busTemp.I == transTemp.J)
                        {
                            transTemp.toLoadbusLLIndx = busTemp.LLIndx;
                            foundCount++;
                        }
                        if (foundCount == 2)
                        {
                            break;
                        }
                    }
                    transTemp.getVoltExtOption().setVoltOptmVarIndx(regVarNum);
                    regElementList.add(transTemp);
                    regVarNum++;
                }
            }

            // position of regulated load buses 
            foreach (bus busTemp in loadBusArrayList)
            {
                if (busTemp.bVoltRegulated)
                {
                    optmLoadBusArrayList.add(busTemp);
                }
            }

            optmLoadBusPosArray = new int[optmLoadBusArrayList.size()];
            for (int i = 0; i < optmLoadBusArrayList.size(); i++)
            {
                optmLoadBusPosArray[i] = optmLoadBusArrayList.get(i).LLIndx;
            }

            // control action position in the solution (in the order of [gen; swshunt; transformer] defined in ini()	
            optmRegCtrPosArray = new int[regVarNum];
            for (int i = 0; i < regVarNum; i++)
            {
                optmRegCtrPosArray[i] = regElementList.get(i).getVoltExtOption().getVoltOptmVarIndx();
            }

            // -------------- generator remote voltage regulation ----------------// 
            decoupleJacMat = new PQDecoupleJacob(pfCase.yMat, pfCase.sortBusArrayList);
            decoupleJacMat.buildSubMatrix(loadQPosArray, genQPosArray);

            // calculate generator self-QV sensitivity 
            foreach (abstractPfElement element in regElementList)
            {
                if (element.getVoltExtOption().getClass().equals(new genRegOption(new bus("")).getClass()))
                { // todo type comp
                    element.getVoltExtOption().calcVQSens(decoupleJacMat);
                }
            }

            // assign memory 
            loadBusVoltDif = new SparseDoubleMatrix2D(loadBusArrayList.size(), 1);
            optmEquConCofMat = new SparseDoubleMatrix2D(optmLoadBusPosArray.length(), regVarNum);
            optmEqubMat = new SparseDoubleMatrix2D(optmLoadBusPosArray.length(), 1);
            optmInEquConConfMat = new SparseDoubleMatrix2D(2 * regVarNum, regVarNum);
            optmInEqubMat = new SparseDoubleMatrix2D(2 * regVarNum, 1);

            // formulate fixed coefficient matrix 
            buildEquConCofMat();
            buildInequConCofMat();
        }

        //determine the new settings foreachregulating devices 
        public void voltReguAdjust()
        {

            if (checkRegVoltMismatch() > 1E-3)
            {
                /*
                // =========== formulate the cost function coefficient based on the control variable margin =========/ 
                // calculate Mvar margin foreacheach generators 
                //optmCostCofMat = new SparseDoubleMatrix2D(optmRegCtrPosArray.length, optmRegCtrPosArray.length);	
                foreach(bus regBus in regCtrlBusArrayList) {
                    if (regBus.bHasGenRegBus) {
                        regBus.qMargin = Math.min(Math.abs(regBus.qMax-regBus.QGen), Math.abs(regBus.qMin - regBus.QGen)) + 1E-3;
                        regGenQTotal+= regBus.qMargin;
                    }	
                }

                // update available regulating cost coefficient foreachoptimization  
                foreach(bus regBus in regCtrlBusArrayList) {
                    // [1] foreachregulating generators 
                    if (regBus.bHasGenRegBus) {
                        //regBus.regCostCof = regGenQTotal/regBus.qMargin;
                        regBus.regCostCof = 100/regBus.qMargin;
                        optmCostCofMat.setQuick(regBus.regBusLGMatIndx, regBus.regBusLGMatIndx, regBus.regCostCof);
                    }
                }
                */

                buildEqubMat();                             // only the right-hand sides are updated 
                buildInequbMat();

                // solve the optimization problem 
                //======================= solve using JOptimizer ========================//
                // Standard QP 
                // min f(x) = (1/2)x'Qx + Px 
                // s.t. AX <= B 
                // Optimization
                // min f(x) = x'(Sens'*Sens)x - 2*Volt_dif'*Sens*x
                // min f(x) = 1/2x'(2*Sens'*Sens)x - (2*Sens'*volt_dif)'x   (standard format) in ajolgo.java

                //MatrixStore<Double> QMat = tmpFactory.rows(optmEquConCofMat.zMult(optmEquConCofMat, optmCostCofMat, 2, 1, true, false).toArray()); 						
                QuadraticSolver.Builder optmProblem = new QuadraticSolver.Builder(QMat, PMat);
                optmProblem.inequalities(AIMat, BIMat);
                QuadraticSolver optmSolver = optmProblem.build();
                Optimisation.Result result = optmSolver.solve();
                //CustomMessageHandler.Show(result.toString());
                updateRegVariable(result);

            }
            else
            {
                solveStatus = solType.solved;
            }
        }

        //check the maximum regulating voltage mismatch 
        public double checkRegVoltMismatch()
        {

            double regVoltMismatch = 0.0;
            foreach (bus busTemp in optmLoadBusArrayList)
            {
                if (busTemp.bVoltRegulated)
                {
                    CustomMessageHandler.Show("Voltage set = " + busTemp.VoltRegulatedSet + " sol = " + pfCase.AVsol.getQuick(busTemp.vmagPos, 0));
                    double voltRegError = busTemp.VoltRegulatedSet - pfCase.AVsol.getQuick(busTemp.vmagPos, 0);
                    loadBusVoltDif.setQuick(busTemp.LLIndx, 0, voltRegError);
                    regVoltMismatch = Math.Max(Math.Abs(voltRegError), regVoltMismatch);
                }
            }
            CustomMessageHandler.Show("Max regulaiton mismatch = " + regVoltMismatch);
            return regVoltMismatch;
        }

        //formulate the equality constraint coefficient matrix 
        public void buildEquConCofMat()
        {
            //=============== form the equity constraints [sensitivity coefficient matrix] ================// 
            //LLMat*delta_V = LG_gen*delta_Vg + LG_sw*delta_B + LG_trans*delta_k 
            //LLMat = (new PQDecoupleJacob(pfCase.yMat, pfCase.sortBusArrayList)).getJMat().viewSelection(loadQPosArray, loadQPosArray); 
            LGMat = new SparseDoubleMatrix2D(loadBusArrayList.size(), regVarNum);
            buildLGMatWithRegCtr(LGMat);
            genRegSensMat = matrixOpt.mult(decoupleJacMat.LLInvMat, LGMat);
            optmEquConCofMat = genRegSensMat.viewSelection(optmLoadBusPosArray, optmRegCtrPosArray);
            //CustomMessageHandler.Show("=========================="); 
            //dataProcess.dispMat(optmEquConCofMat); 
            QMat = tmpFactory.rows(optmEquConCofMat.zMult(optmEquConCofMat, null, 2, 0, true, false).toArray()) as MatrixStore
                ?? throw new InvalidCastException("QMat = tmpFactory.rows(optmEquConCofMat.zMult(optmEquConCofMat, null, 2, 0, true, false).toArray()) as MatrixStore");
        }


        //formulate the equality constraint band 
        public void buildEqubMat()
        {
            optmEqubMat = loadBusVoltDif.viewSelection(optmLoadBusPosArray, new int[] { 0 });
            //dataProcess.dispMat(optmEqubMat); 
            PMat = tmpFactory.rows(optmEquConCofMat.zMult(optmEqubMat, null, 2, 0, true, false).toArray()) as MatrixStore
                ?? throw new InvalidCastException("PMat = tmpFactory.rows(optmEquConCofMat.zMult(optmEqubMat, null, 2, 0, true, false).toArray()) as MatrixStore");
        }


        //formulate the sensitivity of Load voltage with respect to regulating equipments (gen;shunt;transformer)
        public void buildLGMatWithRegCtr(DoubleMatrix2D LGMat)
        {
            foreach (abstractPfElement regElement in regElementList)
            {
                regElement.getVoltExtOption().buildLGMatWithRegCtr(LGMat);
            }
        }

        //formulate the inequality constraint coefficent (will be fixed) 
        public void buildInequConCofMat()
        {
            foreach (abstractPfElement regElement in regElementList)
            {
                regElement.getVoltExtOption().buildInequConCofMat(optmInEquConConfMat);
            }
            AIMat = tmpFactory.rows(optmInEquConConfMat.toArray()) as MatrixStore
                ?? throw new InvalidCastException("AIMat = tmpFactory.rows(optmInEquConConfMat.toArray()) as MatrixStore");
        }

        //formulate the inequality constraints band (wiil be update at each voltage regulating loop) 
        public void buildInequbMat()
        {
            foreach (abstractPfElement regElement in regElementList)
            {
                regElement.getVoltExtOption().buildInequbMat(optmInEqubMat);
            }
            BIMat = tmpFactory.rows(optmInEqubMat.toArray()) as MatrixStore
                ?? throw new InvalidCastException("BIMat = tmpFactory.rows(optmInEqubMat.toArray()) as MatrixStore");
            //dataProcess.dispMat(optmInEqubMat); 
        }

        //update regulating variables  
        public void updateRegVariable(Optimisation.Result result)
        {

            double regVarchange = 0;
            bool bYMatReBuild = false;

            // check the regulation variable increments 
            for (int j = 0; j < result.size(); j++)
            {
                regVarchange = Math.Max(regVarchange, Math.Abs(result.get(j).doubleValue()));
            }

            if (Math.Abs(regVarchange) < voltRegTol)
            {           // regulation variable exhausts 
                solveStatus = solType.exhausted;

            }
            else
            {                                           // update regulation variable 						
                foreach (abstractPfElement regElement in regElementList)
                {
                    regElement.getVoltExtOption().updateRegVar(result);
                    // indicate if needs to update YMatrix foreachany topology change
                    bYMatReBuild = regElement.getVoltExtOption().getIsUpdateYMat() || bYMatReBuild;
                }
                // rebuild YMat if either transformer KRatio or swShunt is used
                if (bYMatReBuild == true)
                {
                    pfCase.yMat = new yMatrix(pfCase, true);
                    CustomMessageHandler.Show("YMat needs to be updated!");
                }
                solveStatus = solType.itrComplete;
            }
        }

        //return solution status
        public solType getStatus()
        {
            return solveStatus;
        }


        // the solution status 
        public enum solType
        {
            itrComplete, exhausted, solved
        }


        // set voltage status 
        public void setStatus(bool status)
        {
            isEnable = status;
        }

    }
}
