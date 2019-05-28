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
    class ESDC1A : excModel
    {

        // default parameters position in input token 
        public const int Default_TR = 3;
        public const int Default_KA = 4;
        public const int Default_TA = 5;
        public const int Default_TB = 6;
        public const int Default_TC = 7;
        public const int Default_VRMAX = 8;
        public const int Default_VRMIN = 9;
        public const int Default_KE = 10;
        public const int Default_TE = 11;
        public const int Default_KF = 12;
        public const int Default_TF1 = 13;
        public const int Default_SWITCH = 14;
        public const int Default_E1 = 15;
        public const int Default_SE1 = 16;
        public const int Default_E2 = 17;
        public const int Default_SE2 = 18;

        // model dynamic parameter fields 
        public double TR = 0.0;         // voltage filter time constant (s) 
        public double KA = 0.0;         // gain 
        public double TA = 0.0;         // (s) 
        public double TB = 0.0;         // (s)
        public double TC = 0.0;         // (s) 
        public double VRMAX = 0.0;          // voltage max 
        public double VRMIN = 0.0;
        public double KE = 0.0;         // saturation gain 
        public double TE = 0.0;         // saturation time constant (s) 
        public double KF = 0.0;         // saturation gain 
        public double TF1 = 0.0;
        public int SWITCH = 0;          // swtich by default = 0 
        public double E1 = 0.0;
        public double SE1 = 0.0;
        public double E2 = 0.0;
        public double SE2 = 0.0;

        // algebraic variable 
        public const int NUM_ALGE = 3;
        public double v1 = 0.0;
        public double v2 = 0.0;
        public double vX = 0.0;

        public int v1_Pos = 0;
        public int v2_Pos = 1;
        public int vX_Pos = 2;
        public int last_AlgeVar_Pos = 0;

        // total number of state variables 
        public const int NUM_STATE = 5;                 // state variable number
        public double st_1 = 0.0;                       // block 1: sensed Vt
        public double st_2 = 0.0;                       // block 2: lead lag
        public double st_3 = 0.0;                       // block 3: regulator output
        public double st_4 = 0.0;                       // block 4: exciter output
        public double st_5 = 0.0;                       // block 5: Rate feedback: Rf = (KF/TF)*EFD - VF=> VF = (KF/TF)*EFD - RF =====> d(RF) = KF/TF1^2*EFD - RF/TF1

        // state variables position in state vector X 
        public int st_1_Pos = 0;
        public int st_2_Pos = 1;
        public int st_3_Pos = 2;
        public int st_4_Pos = 3;
        public int st_5_Pos = 4;
        public int last_StateVar_Pos = 0;

        // define limiter block 
        magLimit V2Limiter;                 // simple output limit block 
        nWLSTCB VRLimiter;                  // non wind-up single time constant block 
        nWIB EFDLimiter;                    // integrator with non-windup limit for EFD 

        // intermediate variable
        public double efd = 0.0;        // exciter output fed into generator  
        public int efd_Pos = 0;         // efd index defined in generator 

        // variable inherit from generator class 
        public gen atGen;
        public int atBusNum = 0;
        public String ID = "";
        public double vt = 0.0;         // measured bus voltage 		
        public int vt_Pos = 0;          // to access regulated bus voltage at each time instant 

        // constant 
        public double K1 = 0.0;         // K1 = TC/TB
        public double K2 = 0.0;         // K2 = KF/TF

        // voltage setting value 
        public double vref = 0.0;           // voltage reference 

        // calculate saturation-related parameter
        public double A = 0.0;              // SE = B*(EFD - A)^2, A=0 & B=0 if not saturated	
        public double B = 0.0;

        // VUEL limiter parameters, feed in by other application (set to 999)
        public double VUEL = 999;

        // sanity check warning string
        public String sanityCheckStr = "";

        // presenting data purpose 
        public override string[] header { get; set; } =
        {
            "Number", "ID", "TR", "KA", "TA", "TB", "TC", "VRMAX", "VRMIN", "KE", "TE", "KF", "TF1", "MODE", "E1",
            "SE(E1)", "E2", "SE(E2)"
        };

        public static int tableColNum = 18;


        // constructor
        public ESDC1A(gen genTemp, String[] token, int numAlge, int numState)
        {

            double kTemp = 0.0;         // KTemp = sqrt((SE1*E1)/(SE2*E2))

            // load data can be loaded from power flow case
            atGen = genTemp;
            atBusNum = genTemp.I;
            ID = genTemp.ID;
            vt_Pos = genTemp.hostBus.vmagPos;

            // algebraic variable position index 
            v1_Pos = v1_Pos + numAlge;
            v2_Pos = v2_Pos + numAlge;
            vX_Pos = vX_Pos + numAlge;
            last_AlgeVar_Pos = numAlge + NUM_ALGE;

            // variable position index 
            st_1_Pos = st_1_Pos + numState;
            st_2_Pos = st_2_Pos + numState;
            st_3_Pos = st_3_Pos + numState;
            st_4_Pos = st_4_Pos + numState;
            st_5_Pos = st_5_Pos + numState;
            last_StateVar_Pos = numState + NUM_STATE;

            // dynamic parameters
            TR = Double.Parse(token[Default_TR]);
            KA = Double.Parse(token[Default_KA]);
            TA = Double.Parse(token[Default_TA]);
            TB = Double.Parse(token[Default_TB]);
            TC = Double.Parse(token[Default_TC]);
            VRMAX = Double.Parse(token[Default_VRMAX]);
            VRMIN = Double.Parse(token[Default_VRMIN]);
            KE = Double.Parse(token[Default_KE]);
            TE = Double.Parse(token[Default_TE]);
            KF = Double.Parse(token[Default_KF]);
            TF1 = Double.Parse(token[Default_TF1]);
            SWITCH = Integer.parseInt(token[Default_SWITCH]);
            E1 = Double.Parse(token[Default_E1]);
            SE1 = Double.Parse(token[Default_SE1]);
            E2 = Double.Parse(token[Default_E2]);
            SE2 = Double.Parse(token[Default_SE2]);

            // SanityTest 
            if (!paraSanityTest()) CustomMessageHandler.print(_String.format("Warning: ESDC1A at [%3d, %2s]\n", atBusNum, ID) + sanityCheckStr);


            // intermediate constant variables 
            K1 = TC / TB;
            K2 = KF / TF1;

            if (SE1 != 0 && SE2 != 0)
            {
                kTemp = Math.Sqrt(E1 * SE1 / E2 / SE2);
                A = (kTemp * E2 - E1) / (kTemp - 1);
                B = SE1 * E1 / Math.Pow(E1 - A, 2);
            }

            V2Limiter = new magLimit(VUEL, -1E6);                   // no lower limit VUEL = 999, 
            VRLimiter = new nWLSTCB(VRMAX, VRMIN, KA, TA);          // Single time constant block with non-windup limit 
            EFDLimiter = new nWIB(1E6, 0, TE);                      // limit EFD output to 0	 

            // update generator dynamic model status 
            genTemp.hasExcModel = true;
            genTemp.excDyn = this;

            CustomMessageHandler.println("ESDC1A Exciter Model at " + genTemp.I + " ID = " + genTemp.ID + " is loaded!");
        }


        // initialize the element in y and x  vector (KE is re-calculated to ensure that VR - VFE = 0) 
        public void ini(DoubleMatrix2D yVector, DoubleMatrix2D xVector)
        {

            // data available after generator dynamic data are read 
            efd_Pos = atGen.efd_Pos;

            // calculate initial values 
            vt = yVector.getQuick(vt_Pos, 0);
            efd = atGen.genDyn.getEfd0();

            st_4 = efd;
            vX = B * (st_4 - A) * (st_4 - A);
            st_1 = vt;
            st_5 = K2 * st_4;
            st_3 = 0;                               // self-excited, KE is selected so that st_3 = 0, per Kunder's book p.363
            KE = (st_3 - vX) / st_4;                // KE is determined so that the summation is ZERO
            v2 = st_3 / KA;
            v1 = v2;
            st_2 = (1 - K1) * v1;
            vref = v1 + st_1 + K2 * st_4 - st_5;

            // initialize algebraic variables 
            yVector.setQuick(v1_Pos, 0, v1);
            yVector.setQuick(v2_Pos, 0, v2);
            yVector.setQuick(vX_Pos, 0, vX);

            // initialize state variables
            xVector.setQuick(st_1_Pos, 0, st_1);
            xVector.setQuick(st_2_Pos, 0, st_2);
            xVector.setQuick(st_3_Pos, 0, st_3);
            xVector.setQuick(st_4_Pos, 0, st_4);
            xVector.setQuick(st_5_Pos, 0, st_5);

            // if limiter is violated at initialization stage, program stalls 
            if (!V2Limiter.checkLimit(K1 * v1 + st_2).isEmpty()) throw new simException(_String.format("ERROR: ESDC1A at [%3d, %2s] Vlimiter", atBusNum, ID) + V2Limiter.getCheckMesg());
            if (!VRLimiter.checkLimit(st_3).isEmpty()) throw new simException(_String.format("ERROR: ESDC1A at [%3d, %2s] VRLimiter", atBusNum, ID) + VRLimiter.getCheckMesg());
            if (!EFDLimiter.checkLimit(st_4).isEmpty()) throw new simException(_String.format("ERROR: ESDC1A at [%3d, %2s] EFDLimiter", atBusNum, ID) + EFDLimiter.getCheckMesg());

            //set limiter to Enable
            V2Limiter.enable();
            VRLimiter.enable();
            EFDLimiter.enable();

            //disable efd0 in generator model		[mandatory]
            atGen.genDyn.zeroEfd0();
        }


        // update variables at the beginning of each iteration 
        public void update_Var(DoubleMatrix2D yVector, DoubleMatrix2D xVector)
        {
            if (atGen.InService)
            {
                // update network variable 
                vt = yVector.getQuick(vt_Pos, 0);

                v1 = yVector.getQuick(v1_Pos, 0);
                v2 = yVector.getQuick(v2_Pos, 0);
                vX = yVector.getQuick(vX_Pos, 0);

                st_1 = xVector.getQuick(st_1_Pos, 0);
                st_2 = xVector.getQuick(st_2_Pos, 0);
                st_3 = xVector.getQuick(st_3_Pos, 0);
                st_4 = xVector.getQuick(st_4_Pos, 0);
                st_5 = xVector.getQuick(st_5_Pos, 0);

                V2Limiter.calc(K1 * v1 + st_2);

                VRLimiter.calc(v2, st_3);
                st_3 = VRLimiter.get_x();                       // update state variables associated with limiter

                EFDLimiter.calc(st_3 - KE * st_4 - vX, st_4);
                st_4 = EFDLimiter.get_x();
            }
        }


        // calculate -g(x,y,u) 
        public void update_g(DoubleMatrix2D g)
        {
            if (atGen.InService)
            {
                g.setQuick(v1_Pos, 0, -(vref - st_1 - (K2 * st_4 - st_5) - v1));
                g.setQuick(v2_Pos, 0, -(V2Limiter.get_x() - v2));
                g.setQuick(vX_Pos, 0, -(B * (st_4 - A) * (st_4 - A) - vX));

                g.setQuick(efd_Pos, 0, g.getQuick(efd_Pos, 0) - st_4);
                //System.out.println("========== Alg Eq============\n" + g.getQuick(v1_Pos, 0) + "\n" + g.getQuick(v2_Pos, 0) + "\n" + g.getQuick(vX_Pos, 0)); 
            }
        }


        // calculate gy = dg/dy
        public void update_gy(DoubleMatrix2D jacMat, int startRow, int startColumn)
        {
            if (atGen.InService)
            {
                jacMat.setQuick(v1_Pos + startRow, v1_Pos + startColumn, -1);

                jacMat.setQuick(v2_Pos + startRow, v1_Pos + startColumn, V2Limiter.get_dx_dy() * K1);
                jacMat.setQuick(v2_Pos + startRow, v2_Pos + startColumn, -1);

                jacMat.setQuick(vX_Pos + startRow, vX_Pos + startColumn, -1);
            }
            else
            {
                jacMat.setQuick(v1_Pos + startRow, v1_Pos + startColumn, 1);
                jacMat.setQuick(v2_Pos + startRow, v2_Pos + startColumn, 1);
                jacMat.setQuick(vX_Pos + startRow, vX_Pos + startColumn, 1);
            }
        }

        // calculate gx = dx/dy
        public void update_gx(DoubleMatrix2D jacMat, int startRow, int startColumn)
        {
            if (atGen.InService)
            {
                jacMat.setQuick(v1_Pos + startRow, st_1_Pos + startColumn, -1);
                jacMat.setQuick(v1_Pos + startRow, st_4_Pos + startColumn, -K2);
                jacMat.setQuick(v1_Pos + startRow, st_5_Pos + startColumn, 1);

                jacMat.setQuick(v2_Pos + startRow, st_2_Pos + startColumn, V2Limiter.get_dx_dy());
                jacMat.setQuick(vX_Pos + startRow, st_4_Pos + startColumn, 2 * B * (st_4 - A));

                jacMat.setQuick(efd_Pos + startRow, st_4_Pos + startColumn, 1);
            }
        }



        // calculate dx/dt = f = f(x,y)
        public void update_f(DoubleMatrix2D f)
        {
            if (atGen.InService)
            {
                f.setQuick(st_1_Pos, 0, 1 / TR * (vt - st_1));
                f.setQuick(st_2_Pos, 0, 1 / TB * ((1 - K1) * v1 - st_2));
                f.setQuick(st_3_Pos, 0, VRLimiter.get_dx());
                f.setQuick(st_4_Pos, 0, EFDLimiter.get_dx());
                f.setQuick(st_5_Pos, 0, 1 / TF1 * (K2 * st_4 - st_5));

                //f.setQuick(st_4_Pos, 0, 1/TE*(st_3 - KE*st_4 - vX));
                //f.setQuick(st_4_Pos, 0, 1/TE*(VRLimiter.get_x() - KE*st_4 - vX));
                //System.out.println("========== Dif Equ ==========\n" + f.getQuick(st_1_Pos, 0) + "\n" + f.getQuick(st_2_Pos, 0) + "\n" + f.getQuick(st_3_Pos, 0) + "\n" + f.getQuick(st_4_Pos, 0) + "\n" + f.getQuick(st_5_Pos, 0));
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
                jacMat.setQuick(startRow + st_3_Pos, startColumn + st_3_Pos, VRLimiter.get_fx() * tStepCof);
                jacMat.setQuick(startRow + st_4_Pos, startColumn + st_3_Pos, 1 / TE * (1) * tStepCof);
                jacMat.setQuick(startRow + st_4_Pos, startColumn + st_4_Pos, 1 / TE * (-KE) * tStepCof);
                jacMat.setQuick(startRow + st_5_Pos, startColumn + st_4_Pos, 1 / TF1 * (K2) * tStepCof);
                jacMat.setQuick(startRow + st_5_Pos, startColumn + st_5_Pos, 1 / TF1 * (-1) * tStepCof);

            }
            else
            {
                jacMat.setQuick(startRow + st_1_Pos, startColumn + st_1_Pos, tStepCof);
                jacMat.setQuick(startRow + st_2_Pos, startColumn + st_2_Pos, tStepCof);
                jacMat.setQuick(startRow + st_3_Pos, startColumn + st_3_Pos, tStepCof);
                jacMat.setQuick(startRow + st_4_Pos, startColumn + st_4_Pos, tStepCof);
                jacMat.setQuick(startRow + st_5_Pos, startColumn + st_5_Pos, tStepCof);
            }
        }


        public void update_fy(DoubleMatrix2D jacMat, int startRow, int startColumn, double simTheta, double h)
        {
            double tStepCof = -simTheta * h;
            if (atGen.InService)
            {
                jacMat.setQuick(st_1_Pos + startRow, vt_Pos + startColumn, 1 / TR * (1) * tStepCof);
                jacMat.setQuick(st_2_Pos + startRow, v1_Pos + startColumn, 1 / TB * (1 - K1) * tStepCof);
                jacMat.setQuick(st_3_Pos + startRow, v2_Pos + startColumn, VRLimiter.get_fy() * tStepCof);
                jacMat.setQuick(st_4_Pos + startRow, vX_Pos + startColumn, 1 / TE * (-1) * tStepCof);
            }
        }


        // data sanity check per p.641 on PSS/E Volume II Program Application Guide
        public bool paraSanityTest()
        {
            sanityCheckStr = dataProcess.dataLimitCheck("TR", TR, 0, "", false, 0.5, "", true, "\n")
                             + dataProcess.dataLimitCheck("KA", KA, 10, "", false, 500, "", true, "\n")
                             + dataProcess.dataLimitCheck("TA", TA, 0, "", false, 1, "", true, "\n")
                             + dataProcess.dataLimitCheck("TB", TB, 0, "", false, 99, "", true, "\n")
                             + dataProcess.dataLimitCheck("TC", TC, 0, "", false, 99, "", true, "\n")
                              + dataProcess.dataLimitCheck("VRMAX", VRMAX, 0.5, "", false, 10, "", true, "\n")
                             + dataProcess.dataLimitCheck("VRMIN", VRMIN, -10, "", false, 0, "", true, "\n")
                             + dataProcess.dataLimitCheck("TF1", TF1, 0, "", false, 1.5, "", true, "\n")
                             + dataProcess.dataLimitCheck("E1", E1, 0, "", true, E2, "E2", true, "\n")
                             + dataProcess.dataLimitCheck("SE1", SE1, 0, "", true, 1, "", true, "\n")
                             + dataProcess.dataLimitCheck("E2", SE1, 0, "", true, 999, "", true, "\n")
                             + dataProcess.dataLimitCheck("SE2", SE1, SE1, "", true, 999, "", true, "\n");
            return sanityCheckStr.isEmpty();
        }


        // export data for tabular showing 
        public Object[] setTable()
        {
            Object[] ret = new Object[tableColNum];
            ret[0] = atBusNum;
            ret[1] = ID;
            ret[2] = _String.format("%1.4f", TR);
            ret[3] = _String.format("%1.4f", KA);
            ret[4] = _String.format("%1.4f", TA);
            ret[5] = _String.format("%1.4f", TB);
            ret[6] = _String.format("%1.4f", TC);
            ret[7] = _String.format("%1.4f", VRMAX);
            ret[8] = _String.format("%1.4f", VRMIN);
            ret[9] = _String.format("%1.4f", KE);
            ret[10] = _String.format("%1.4f", TE);
            ret[11] = _String.format("%1.4f", KF);
            ret[12] = _String.format("%1.4f", TF1);
            ret[13] = _String.format("%2d", SWITCH);
            ret[14] = _String.format("%1.4f", E1);
            ret[15] = _String.format("%1.4f", SE1);
            ret[16] = _String.format("%1.4f", E2);
            ret[17] = _String.format("%1.4f", SE2);
            return ret;
        }


        public override string[] AsArrayForRow()
        {
            return new[]
            {
                atBusNum.ToString(),
                ID,
                _String.format("%1.4f", TR),
                _String.format("%1.4f", KA),
                _String.format("%1.4f", TA),
                _String.format("%1.4f", TB),
                _String.format("%1.4f", TC),
                _String.format("%1.4f", VRMAX),
                _String.format("%1.4f", VRMIN),
                _String.format("%1.4f", KE),
                _String.format("%1.4f", TE),
                _String.format("%1.4f", KF),
                _String.format("%1.4f", TF1),
                _String.format("%2d", SWITCH),
                _String.format("%1.4f", E1),
                _String.format("%1.4f", SE1),
                _String.format("%1.4f", E2),
                _String.format("%1.4f", SE2)
            };
        }


        // debug code [check limit output]
        public double limitTest()
        {
            return st_4;
        }
    }
}
