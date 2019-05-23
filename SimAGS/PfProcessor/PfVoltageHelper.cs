using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cern.colt.matrix.linalg;
using org.ojalgo.matrix.store;
using org.ojalgo.optimisation;
using org.ojalgo.optimisation.quadratic;
using SimAGS.Components;
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

        public double[,] loadBusVoltDif;
        public double[,] LLMat;                    // nAllLoadBus*nAllLoadBus matrix
        public double[,] LGMat;                    // nAllLoadBus*nRegTotalNum matrix 
        public double[,] genRegSensMat;            // nAllLoadBus*nRegTotalNum matrix
        public double[,] optmEquConCofMat;
        public double[,] optmEqubMat;
        public double[,] optmInEquConConfMat;
        public double[,] optmInEqubMat;

        public PhysicalStore.Factory tmpFactory = PrimitiveDenseStore.FACTORY;
        public MatrixStore QMat;
        public MatrixStore PMat;
        public MatrixStore AIMat;
        public MatrixStore BIMat;

        public PQDecoupleJacob decoupleJacMat;

        public boolean isEnable = false;

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
            for (bus busTemp: pfCase.sortBusArrayList)
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

            for (bus busTemp: pfCase.sortBusArrayList)
            {
                if (busTemp.bHasSwShunt)
                {
                    busTemp.getVoltExtOption().setVoltOptmVarIndx(regVarNum);
                    regElementList.add(busTemp);
                    regVarNum++;
                }
            }

            for (twoWindTrans transTemp: pfCase.twoWindTransArrayList)
            {
                if (transTemp.COD1 == 1)
                {
                    int foundCount = 0;
                    // update from and to bus index in load-bus only matrix 
                    for (bus busTemp: loadBusArrayList)
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
            for (bus busTemp: loadBusArrayList)
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
            for (abstractPfElement element: regElementList)
            {
                if (element.getVoltExtOption().getClass().equals(genRegOption.class)){
				element.getVoltExtOption().calcVQSens(decoupleJacMat);
    }
}

// assign memory 
loadBusVoltDif = new Sparsedouble[,](loadBusArrayList.size(),1);
optmEquConCofMat = new Sparsedouble[,](optmLoadBusPosArray.length, regVarNum);
optmEqubMat = new Sparsedouble[,](optmLoadBusPosArray.length,1);
optmInEquConConfMat = new Sparsedouble[,](2*regVarNum, regVarNum);
optmInEqubMat = new Sparsedouble[,](2*regVarNum,1);

// formulate fixed coefficient matrix 
buildEquConCofMat();
buildInequConCofMat(); 
	}
	
	//determine the new settings for regulating devices 
	public void voltReguAdjust()
{

    if (checkRegVoltMismatch() > 1E-3)
    {
        /*
        // =========== formulate the cost function coefficient based on the control variable margin =========/ 
        // calculate Mvar margin for each generators 
        //optmCostCofMat = new Sparsedouble[,](optmRegCtrPosArray.length, optmRegCtrPosArray.length);	
        for (bus regBus: regCtrlBusArrayList) {
            if (regBus.bHasGenRegBus) {
                regBus.qMargin = Math.min(Math.abs(regBus.qMax-regBus.QGen), Math.abs(regBus.qMin - regBus.QGen)) + 1E-3;
                regGenQTotal+= regBus.qMargin;
            }	
        }

        // update available regulating cost coefficient for optimization  
        for (bus regBus: regCtrlBusArrayList) {
            // [1] for regulating generators 
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
        //System.out.println(result.toString());
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
    for (bus busTemp: optmLoadBusArrayList)
    {
        if (busTemp.bVoltRegulated)
        {
            System.out.println("Voltage set = " + busTemp.VoltRegulatedSet + " sol = " + pfCase.AVsol.getQuick(busTemp.vmagPos, 0));
            double voltRegError = busTemp.VoltRegulatedSet - pfCase.AVsol.getQuick(busTemp.vmagPos, 0);
            loadBusVoltDif.setQuick(busTemp.LLIndx, 0, voltRegError);
            regVoltMismatch = Math.max(Math.abs(voltRegError), regVoltMismatch);
        }
    }
    System.out.println("Max regulaiton mismatch = " + regVoltMismatch);
    return regVoltMismatch;
}

//formulate the equality constraint coefficient matrix 
public void buildEquConCofMat()
{
    //=============== form the equity constraints [sensitivity coefficient matrix] ================// 
    //LLMat*delta_V = LG_gen*delta_Vg + LG_sw*delta_B + LG_trans*delta_k 
    //LLMat = (new PQDecoupleJacob(pfCase.yMat, pfCase.sortBusArrayList)).getJMat().viewSelection(loadQPosArray, loadQPosArray); 
    LGMat = new Sparsedouble[,](loadBusArrayList.size(), regVarNum);
    buildLGMatWithRegCtr(LGMat);
    genRegSensMat = matrixOpt.mult(decoupleJacMat.LLInvMat, LGMat);
    optmEquConCofMat = genRegSensMat.viewSelection(optmLoadBusPosArray, optmRegCtrPosArray);
    //System.out.println("=========================="); 
    //dataProcess.dispMat(optmEquConCofMat); 
    QMat = tmpFactory.rows(optmEquConCofMat.zMult(optmEquConCofMat, null, 2, 0, true, false).toArray());
}


//formulate the equality constraint band 
public void buildEqubMat()
{
    optmEqubMat = loadBusVoltDif.viewSelection(optmLoadBusPosArray, new int[] { 0 });
    //dataProcess.dispMat(optmEqubMat); 
    PMat = tmpFactory.rows(optmEquConCofMat.zMult(optmEqubMat, null, 2, 0, true, false).toArray());
}


//formulate the sensitivity of Load voltage with respect to regulating equipments (gen;shunt;transformer)
public void buildLGMatWithRegCtr(double[,] LGMat)
{
    for (abstractPfElement regElement: regElementList)
    {
        regElement.getVoltExtOption().buildLGMatWithRegCtr(LGMat);
    }
}

//formulate the inequality constraint coefficent (will be fixed) 
public void buildInequConCofMat()
{
    for (abstractPfElement regElement: regElementList)
    {
        regElement.getVoltExtOption().buildInequConCofMat(optmInEquConConfMat);
    }
    AIMat = tmpFactory.rows(optmInEquConConfMat.toArray());
}

//formulate the inequality constraints band (wiil be update at each voltage regulating loop) 
public void buildInequbMat()
{
    for (abstractPfElement regElement: regElementList)
    {
        regElement.getVoltExtOption().buildInequbMat(optmInEqubMat);
    }
    BIMat = tmpFactory.rows(optmInEqubMat.toArray());
    //dataProcess.dispMat(optmInEqubMat); 
}

//update regulating variables  
public void updateRegVariable(Optimisation.Result result)
{

    double regVarchange = 0;
    boolean bYMatReBuild = false;

    // check the regulation variable increments 
    for (int j = 0; j < result.size(); j++)
    {
        regVarchange = Math.max(regVarchange, Math.abs(result.get(j).doubleValue()));
    }

    if (Math.abs(regVarchange) < voltRegTol)
    {           // regulation variable exhausts 
        solveStatus = solType.exhausted;

    }
    else
    {                                           // update regulation variable 						
        for (abstractPfElement regElement: regElementList)
        {
            regElement.getVoltExtOption().updateRegVar(result);
            // indicate if needs to update YMatrix for any topology change
            bYMatReBuild = regElement.getVoltExtOption().getIsUpdateYMat() ? true : bYMatReBuild;
        }
        // rebuild YMat if either transformer KRatio or swShunt is used
        if (bYMatReBuild == true)
        {
            pfCase.yMat = new yMatrix(pfCase, true);
            System.out.println("YMat needs to be updated!");
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
public static enum solType
{
    itrComplete, exhausted, solved
}


// set voltage status 
public void setStatus(boolean status)
{
    isEnable = status;
}
	
    }
}
