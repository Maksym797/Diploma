using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cern.colt.matrix;
using com.sun.xml.@internal.bind.v2.model.core;
using SimAGS.Components;
using SimAGS.Handlers;
using SimAGS.SimUtil;

namespace SimAGS.DynModels.ExcModels
{
    public class ESAC4A : excModel
    {

        // default parameters position in input token 
        public const int Default_TR = 3;
        public const int Default_VIMAX = 4;
        public const int Default_VIMIN = 5;
        public const int Default_TC = 6;
        public const int Default_TB = 7;
        public const int Default_KA = 8;
        public const int Default_TA = 9;
        public const int Default_VRMAX = 10;
        public const int Default_VRMIN = 11;
        public const int Default_KC = 12;

        // model dynamic parameter fields 
        public double TR = 0.0;         // voltage filter time constant (s) 
        public double VIMAX = 0.0;
        public double VIMIN = 0.0;
        public double TC = 0.0;         // (s) 
        public double TB = 0.0;         // (s)
        public double KA = 0.0;         // gain 
        public double TA = 0.0;         // (s) 		
        public double VRMAX = 0.0;          // voltage max 
        public double VRMIN = 0.0;
        public double KC = 0.0;         // will be ignored for the sake of simplicity 

        // voltage setting value 
        public double vref = 0.0;           // voltage reference

        // algebraic variables 
        public const int NUM_ALGE = 2;      // algebraic variable number	// 
        public double v1 = 0.0;
        public double v2 = 0.0;

        public int v1_Pos = 0;
        public int v2_Pos = 1;
        public int last_AlgeVar_Pos = 0;

        // total number of state variables 
        public const int NUM_STATE = 3;     // state variable number
        public double st_1 = 0.0;           // voltage low pass filter (1st)	
        public double st_2 = 0.0;           // low-high pass filter (2nd) 	
        public double st_3 = 0.0;           // amplifier (3nd) 	

        public int st_1_Pos = 0;
        public int st_2_Pos = 1;
        public int st_3_Pos = 2;
        public int last_StateVar_Pos = 0;

        // intermediate variable 		
        public magLimit V1Limiter;
        public magLimit V2Limiter;
        public nWLSTCB efdLimiter;

        // intermediate variable
        public double efd = 0.0;        // exciter output fed into generator  
        public int efd_Pos = 0;         // efd index defined in generator 

        // variables inherit from generator class (for initialization only)
        //public genModel genDynModel;	 	
        public gen atGen;
        public int atBusNum = 0;
        public String ID = "";
        public double vt = 0.0;         // measured bus voltage 		
        public int vt_Pos = 0;          // to access regulated bus voltage at each time instant 

        // constant 
        public double K1 = 0.0;         // K1 = TC/TB

        // sanity check warning string
        public String sanityCheckStr = "";

        // presenting data purpose 
        public override string[] header { get; set; } = { "Number", "ID", "TR", "VIMAX", "VIMIN", "TC", "TB", "KA", "TA", "VRMAX", "VRMIN", "KC" };


        public static int tableColNum = 12;

        // constructor
        public ESAC4A(gen genTemp, String[] token, int numAlge, int numState)
        {

            // load data can be loaded from power flow case
            atGen = genTemp;
            atBusNum = genTemp.I;
            ID = genTemp.ID;
            vt_Pos = genTemp.hostBus.vmagPos;

            // algebraic variable position index 
            v1_Pos = v1_Pos + numAlge;
            v2_Pos = v2_Pos + numAlge;
            last_AlgeVar_Pos = numAlge + NUM_ALGE;

            // state variable position index 
            st_1_Pos = st_1_Pos + numState;
            st_2_Pos = st_2_Pos + numState;
            st_3_Pos = st_3_Pos + numState;
            last_StateVar_Pos = numState + NUM_STATE;

            // dynamic parameters
            TR = Double.Parse(token[Default_TR]);
            VIMAX = Double.Parse(token[Default_VIMAX]);
            VIMIN = Double.Parse(token[Default_VIMIN]);
            TC = Double.Parse(token[Default_TC]);
            TB = Double.Parse(token[Default_TB]);
            KA = Double.Parse(token[Default_KA]);
            TA = Double.Parse(token[Default_TA]);
            VRMAX = Double.Parse(token[Default_VRMAX]);
            VRMIN = Double.Parse(token[Default_VRMIN]);
            KC = Double.Parse(token[Default_KC]);

            // SanityTest 
            if (!paraSanityTest()) CustomMessageHandler.print(_String.format("Warning: ESAC4A at [%3d, %2s]\n", atBusNum, ID) + sanityCheckStr);
            //if (!paraSanityTest()) CustomMessageHandler.print(String.format("Warning: ESAC4A at [%3d, %2s]\n", atBusNum, ID) + sanityCheckStr);

            // calculate intermediate parameters 
            K1 = TC / TB;

            V1Limiter = new magLimit(VIMAX, VIMIN);
            V2Limiter = new magLimit(9999, -9999);      // VUEL = 999, 
            efdLimiter = new nWLSTCB(VRMAX, VRMIN, KA, TA); // uplimit = VRMAX - KCI(IFD), KCI is fixed to 0.000

            // update generator dynamic model status 
            genTemp.hasExcModel = true;
            genTemp.excDyn = this;

            CustomMessageHandler.println("ESAC4A Exciter Model at " + genTemp.I + " ID = " + genTemp.ID + " is loaded!");
        }


        // initialize the element in x vector 
        public void ini(DoubleMatrix2D yVector, DoubleMatrix2D xVector)
        {

            // data available after generator dynamic data are read 
            efd_Pos = atGen.efd_Pos;

            // 
            vt = yVector.getQuick(vt_Pos, 0);
            efd = atGen.genDyn.getEfd0();

            // initialize state variables
            st_1 = vt;
            st_3 = efd;
            v2 = st_3 / KA;
            v1 = v2;
            st_2 = (1 - K1) * v1;
            vref = st_1 + v1;

            // initialize algebraic variables 
            yVector.setQuick(v1_Pos, 0, v1);
            yVector.setQuick(v2_Pos, 0, v2);

            // initialize state variables 
            xVector.setQuick(st_1_Pos, 0, st_1);
            xVector.setQuick(st_2_Pos, 0, st_2);
            xVector.setQuick(st_3_Pos, 0, st_3);

            // if limiter is violated at initialization stage, program stalls 
            if (!V1Limiter.checkLimit(vref - st_1).isEmpty()) throw new simException(_String.format("ERROR: ESAC4A at [%3d, %2s] V1limiter", atBusNum, ID) + V1Limiter.getCheckMesg());
            if (!V2Limiter.checkLimit(K1 * v1 + st_2).isEmpty()) throw new simException(_String.format("ERROR: ESAC4A at [%3d, %2s] V2limiter", atBusNum, ID) + V2Limiter.getCheckMesg());
            if (!efdLimiter.checkLimit(st_3).isEmpty()) throw new simException(_String.format("ERROR: ESAC4A at [%3d, %2s] efdLimiter", atBusNum, ID) + efdLimiter.getCheckMesg());

            //set limiter to Enable
            V1Limiter.enable();
            V2Limiter.enable();
            efdLimiter.enable();

            //disable efd0 in generator model
            atGen.genDyn.zeroEfd0();
        }


        // update variables at the beginning of each iteration 
        public void update_Var(DoubleMatrix2D yVector, DoubleMatrix2D xVector)
        {

            if (atGen.InService)
            {
                vt = yVector.getQuick(vt_Pos, 0);           // update network variable

                v1 = yVector.getQuick(v1_Pos, 0);           // update alge variable
                v2 = yVector.getQuick(v2_Pos, 0);

                st_1 = xVector.getQuick(st_1_Pos, 0);       // update state variables
                st_2 = xVector.getQuick(st_2_Pos, 0);
                st_3 = xVector.getQuick(st_3_Pos, 0);

                V1Limiter.calc(vref - st_1);

                V2Limiter.calc(K1 * v1 + st_2);

                efdLimiter.calc(v2, st_3);                  //update the state variables associated the limiter (all algebraic variables are fixed when forming the g vector)
                st_3 = efdLimiter.get_x();
            }
        }


        // calculate -g(x,y,u) 
        public void update_g(DoubleMatrix2D g)
        {
            if (atGen.InService)
            {
                g.setQuick(v1_Pos, 0, -(V1Limiter.get_x() - v1));
                g.setQuick(v2_Pos, 0, -(V2Limiter.get_x() - v2));

                g.setQuick(efd_Pos, 0, g.getQuick(efd_Pos, 0) - st_3);
                //System.out.println("Alg Equ for gen at " + atBusNum + "\n" + g.getQuick(v1_Pos, 0) + "\n" + g.getQuick(v2_Pos, 0) + "\n" + g.getQuick(efd_Pos, 0) + "\n===================\n");
            }
        }

        // calculate gy = dg/dy
        public void update_gy(DoubleMatrix2D jacMat, int startRow, int startColumn)
        {
            if (atGen.InService)
            {
                jacMat.setQuick(v1_Pos + startRow, v1_Pos + startColumn, -1);

                jacMat.setQuick(v2_Pos + startRow, v2_Pos + startColumn, -1);
                jacMat.setQuick(v2_Pos + startRow, v1_Pos + startColumn, K1);

            }
            else
            {
                jacMat.setQuick(v1_Pos + startRow, v1_Pos + startColumn, 1);
                jacMat.setQuick(v2_Pos + startRow, v2_Pos + startColumn, 1);
            }
        }


        // calculate gx = dx/dy
        public void update_gx(DoubleMatrix2D jacMat, int startRow, int startColumn)
        {
            if (atGen.InService)
            {
                jacMat.setQuick(v1_Pos + startRow, st_1_Pos + startColumn, V1Limiter.get_dx_dy() * (-1));
                jacMat.setQuick(v2_Pos + startRow, st_2_Pos + startColumn, V2Limiter.get_dx_dy() * (-1));

                jacMat.setQuick(efd_Pos + startRow, st_3_Pos + startColumn, 1);
            }
        }


        // calculate dx/dt = f = f(x,y)
        public void update_f(DoubleMatrix2D f)
        {
            if (atGen.InService)
            {
                f.setQuick(st_1_Pos, 0, 1 / TR * (vt - st_1));
                f.setQuick(st_2_Pos, 0, 1 / TB * ((1 - K1) * v1 - st_2));
                f.setQuick(st_3_Pos, 0, efdLimiter.get_dx());

                //System.out.println("Dif Equ for gen at " + atBusNum + "\n" + f.getQuick(st_1_Pos, 0) + "\n" + f.getQuick(st_2_Pos, 0) + "\n" + f.getQuick(st_3_Pos, 0) + "\n================\n");
            }
        }


        // calculate fx = df/dx 
        public void update_fx(DoubleMatrix2D jacMat, int startRow, int startColumn, double simTheta, double h)
        {
            double tStepCof = -simTheta * h;
            if (atGen.InService)
            {
                jacMat.setQuick(startRow + st_1_Pos, startColumn + st_1_Pos, 1 / TR * (-1) * tStepCof);
                jacMat.setQuick(startRow + st_2_Pos, startColumn + st_2_Pos, 1 / TB * (-1) * tStepCof);
                jacMat.setQuick(startRow + st_3_Pos, startColumn + st_3_Pos, efdLimiter.get_fx() * tStepCof);
            }
            else
            {
                jacMat.setQuick(startRow + st_1_Pos, startColumn + st_1_Pos, tStepCof);
                jacMat.setQuick(startRow + st_2_Pos, startColumn + st_2_Pos, tStepCof);
                jacMat.setQuick(startRow + st_3_Pos, startColumn + st_3_Pos, tStepCof);
            }
        }


        public void update_fy(DoubleMatrix2D jacMat, int startRow, int startColumn, double simTheta, double h)
        {
            double tStepCof = -simTheta * h;
            if (atGen.InService)
            {
                jacMat.setQuick(startRow + st_1_Pos, startColumn + vt_Pos, 1 / TR * tStepCof);
                jacMat.setQuick(startRow + st_2_Pos, startColumn + v1_Pos, 1 / TB * (1 - K1) * tStepCof);
                jacMat.setQuick(startRow + st_3_Pos, startColumn + v2_Pos, efdLimiter.get_fy() * tStepCof);
            }
        }

        // data sanity check per p.641 on PSS/E Volume II Program Application Guide
        public bool paraSanityTest()
        {
            sanityCheckStr = dataProcess.dataLimitCheck("TR", TR, 0, "", true, 0.1, "", true, "\n")
                             + dataProcess.dataLimitCheck("VIMAX", VIMAX, 0, "", false, 0.2, "", true, "\n")
                             + dataProcess.dataLimitCheck("VIMIN", VIMIN, -0.2, "", false, 0.0, "", true, "\n")
                             + dataProcess.dataLimitCheck("TC", TC, 0, "", true, 10.0, "", true, "\n")
                             + dataProcess.dataLimitCheck("TB", TB, 0, "", false, 20.0, "", true, "\n")
                             + dataProcess.dataLimitCheck("KA", KA, 50, "", true, 1000, "", true, "\n")
                             + dataProcess.dataLimitCheck("TA", TA, 0, "", true, 0.5, "", true, "\n")
                             + dataProcess.dataLimitCheck("VRMAX", VRMAX, 3, "", true, 8, "", true, "\n")
                             + dataProcess.dataLimitCheck("VRMIN", VRMIN, -8, "", true, -3, "", true, "\n")
                             + dataProcess.dataLimitCheck("KC", KC, 0, "", true, 0.3, "", true, "\n");
            return sanityCheckStr.isEmpty();
        }


        // export data for tabular showing 
        // public Object[] setTable()
        // {
        //     Object[] ret = new Object[tableColNum];
        //     ret[0] = atBusNum;
        //     ret[1] = ID;
        //     ret[2] = String.format("%1.4f", TR);
        //     ret[3] = String.format("%1.4f", VIMAX);
        //     ret[4] = String.format("%1.4f", VIMIN);
        //     ret[5] = String.format("%1.4f", TC);
        //     ret[6] = String.format("%1.4f", TB);
        //     ret[7] = String.format("%1.4f", KA);
        //     ret[8] = String.format("%1.4f", TA);
        //     ret[9] = String.format("%1.4f", VRMAX);
        //     ret[10] = String.format("%1.4f", VRMIN);
        //     ret[11] = String.format("%1.4f", KC);
        //     return ret;
        // }

        public override string[] AsArrayForRow()
        {
            return new[]
            {
                atBusNum.ToString(),
                ID,
                _String.format("%1.4f", TR),
                _String.format("%1.4f", VIMAX),
                _String.format("%1.4f", VIMIN),
                _String.format("%1.4f", TC),
                _String.format("%1.4f", TB),
                _String.format("%1.4f", KA),
                _String.format("%1.4f", TA),
                _String.format("%1.4f", VRMAX),
                _String.format("%1.4f", VRMIN),
                _String.format("%1.4f", KC)
            };
        }
        
        // debug code [check limit output]
        public double limitTest()
        {
            return st_3;
        }

    }
}
