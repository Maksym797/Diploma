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

namespace SimAGS.DynModels.GovModels
{
    public class IEEEG2 : govModel
    {

        // default parameters position in input token 
        public const int Default_K = 3;
        public const int Default_T1 = 4;
        public const int Default_T2 = 5;
        public const int Default_T3 = 6;
        public const int Default_PMAX = 7;
        public const int Default_PMIN = 8;
        public const int Default_T4 = 9;

        // model dynamic parameter fields 
        public double K = 0.0;          // gain (no unit)	
        public double T1 = 0.0;         // (sec.)
        public double T2 = 0.0;         // (sec.)
        public double T3 = 0.0;         // (sec.) 
        public double PMAX = 0.0;           // Pmax (pu on machine MVA rating) 
        public double PMIN = 0.0;           // Pmin (pu on machine MVA rating) 
        public double T4 = 0.0;         // (>0, sec.) water starting time   

        // algebraic variables 
        public const int NUM_ALGE = 1;      // algebraic variable number
        public double v1 = 0.0;     // limiter output 

        public int v1_Pos = 0;
        public int last_AlgeVar_Pos = 0;

        // differential variables 
        public const int NUM_STATE = 3;
        public double st_1 = 0.0;       // state variable for (1+sT2)/(1+sT1)	
        public double st_2 = 0.0;       // state variable for K(1+sT3)
        public double st_3 = 0.0;       // start variable for (1-sT4)/(1+0.5sT4)

        public int st_1_Pos = 0;
        public int st_2_Pos = 1;
        public int st_3_Pos = 2;
        public int last_StateVar_Pos = 0;

        // variables inherit from generator class
        public gen atGen;
        public int atBusNum = 0;
        public String ID = "";
        public double MBase = 0.0;          // machine base

        public double omega = 0.0;
        public int omega_Pos = 0;           // omega index 

        public double pmech = 0.0;
        public int pmech_Pos = 0;           // mechanical power setting inherit from generators 

        public double agcMWWeight = 0.0;    // 
        public int agcPRefTotal_Pos;

        public double pf_pMax = 0.0;        // generator Pmax loaded from power flow 
        public double pf_pMin = 0.0;        // generator Pmin loaded from power flow 

        // extra data and variables 
        public double omegaRef = 1.0;       // speed reference (p.u.) 
        public double baseRatio = 0.0;      // MBase/SBASE
        public double K1 = 0.0;     // K1 = T2/T1
        public double K2 = 0.0;     // -T4/(0.5T4) = -2

        // external setting values 
        public double prefSchOrder = 0.0;   // P reference from day-ahead schedules 
        public double prefAGCOrder = 0.0;   // additional ref from AGC control  prefAGCOrder = AGCPRefTotal*weight

        // limiter
        public magLimit V1limiter;

        // sanity check warning string
        public String sanityCheckStr = "";

        // presenting data purpose 
        public override String[] header { get; set; } = {"Number", "ID", "K", "T1", "T2", "T3", "Pmax", "Pmin", "T4"};

        public static int tableColNum = 9;


        // constructor
        public IEEEG2(gen genTemp, String[] token, int numAlge, int numState)
        {

            // load data can be loaded from power flow case
            atGen = genTemp;
            atBusNum = atGen.I;
            ID = atGen.ID;
            MBase = atGen.MBASE;
            pf_pMax = atGen.PT * SBASE / MBase;         // on machine base 
            pf_pMin = atGen.PB * SBASE / MBase;         // on machine base

            // algebraic variable index 
            v1_Pos = v1_Pos + numAlge;
            last_AlgeVar_Pos = numAlge + NUM_ALGE;

            // variable position index 
            st_1_Pos = st_1_Pos + numState;
            st_2_Pos = st_2_Pos + numState;
            st_3_Pos = st_3_Pos + numState;
            last_StateVar_Pos = numState + NUM_STATE;

            baseRatio = MBase / SBASE;              // defined before loading data 

            // dynamic parameters
            K = Double.Parse(token[Default_K]);
            T1 = Double.Parse(token[Default_T1]);
            T2 = Double.Parse(token[Default_T2]);
            T3 = Double.Parse(token[Default_T3]);
            PMAX = Double.Parse(token[Default_PMAX]);
            PMIN = Double.Parse(token[Default_PMIN]);
            T4 = Double.Parse(token[Default_T4]);

            // SanityTest 
            if (!paraSanityTest()) CustomMessageHandler.print(_String.format("Warning: IEEEG2 at [%3d, %2s]\n", atBusNum, ID) + sanityCheckStr);

            // convert to system base
            PMAX = PMAX * baseRatio;
            PMIN = PMIN * baseRatio;

            V1limiter = new magLimit(PMAX, PMIN);

            K1 = T2 / T1;
            K2 = -2.0;

            // update generator dynamic model status 
            genTemp.govDyn = this;
            genTemp.hasGovModel = true;

            CustomMessageHandler.println("IEEEG2 Model at " + genTemp.I + " ID = " + genTemp.ID + " is loaded!");
        }


        // initialize the element in x vector 
        public void ini(DoubleMatrix2D yVector, DoubleMatrix2D xVector)
        {

            // info can be loaded after generator info has been loaded 
            omega_Pos = atGen.omega_Pos;
            pmech_Pos = atGen.pmech_Pos;

            // calculate initial values of algebraic variables 
            pmech = atGen.genDyn.getPm();
            v1 = pmech;
            st_1 = 0.0;         // pending 
            st_2 = 0.0;         // pending
            st_3 = v1 * (1 - K2);   // pending 
            prefSchOrder = v1;          // hourly schedule order, comparing TSAT and PSS/E models 
            prefAGCOrder = 0.0;         // AGC supplemental order

            agcMWWeight = atGen.AGCSMWhare;
            agcPRefTotal_Pos = atGen.agcPRefTotal_Pos;

            // update the dynamic state variable index 
            yVector.setQuick(v1_Pos, 0, v1);

            // initialize state variables
            xVector.setQuick(st_1_Pos, 0, st_1);
            xVector.setQuick(st_2_Pos, 0, st_2);
            xVector.setQuick(st_3_Pos, 0, st_3);


            // if limiter is violated at initialization stage, program stalls 
            if (!V1limiter.checkLimit(prefSchOrder + prefAGCOrder - st_2).isEmpty()) throw new simException(_String.format("ERROR: IEEEG2 at [%3d, %2s] V1Limiter", atBusNum, ID) + V1limiter.getCheckMesg());

            // enable V1Limiter is the input value within the range 
            V1limiter.enable();

            // set Pm0 = 0, link governor with generator
            atGen.genDyn.zeroPm0();
        }


        // update variables at the beginning of each iteration 
        public void update_Var(DoubleMatrix2D yVector, DoubleMatrix2D xVector)
        {

            if (atGen.InService)
            {
                st_1 = xVector.getQuick(st_1_Pos, 0);
                st_2 = xVector.getQuick(st_2_Pos, 0);
                st_3 = xVector.getQuick(st_3_Pos, 0);
                omega = xVector.getQuick(omega_Pos, 0);

                pmech = yVector.getQuick(pmech_Pos, 0);     // update algebraic variables 
                v1 = yVector.getQuick(v1_Pos, 0);

                V1limiter.calc(prefSchOrder + prefAGCOrder - st_2);

                //prefAGCOrder = agcMWWeight*yVector.getQuick(agcPRefTotal_Pos,0); 
            }
        }


        // calculate algebraic equations   
        public void update_g(DoubleMatrix2D g)
        {
            if (atGen.InService)
            {
                g.setQuick(v1_Pos, 0, -(V1limiter.get_x() - v1));

                // calculate its impact on generator mechanical torque equation
                g.setQuick(pmech_Pos, 0, g.getQuick(pmech_Pos, 0) - (K2 * v1 + st_3));

                //CustomMessageHandler.println("Alge equ = " + g.getQuick(v1_Pos, 0));
            }
        }


        // calculate gy = dg/dy
        public void update_gy(DoubleMatrix2D jacMat, int startRow, int startColumn)
        {
            if (atGen.InService)
            {
                jacMat.setQuick(v1_Pos + startRow, v1_Pos + startColumn, -1);

                jacMat.setQuick(pmech_Pos + startRow, v1_Pos + startColumn, K2);
            }
            else
            {
                jacMat.setQuick(v1_Pos + startRow, v1_Pos + startColumn, 1);
            }
        }


        // calculate gx = dg/dx
        public void update_gx(DoubleMatrix2D jacMat, int startRow, int startColumn)
        {
            if (atGen.InService)
            {
                jacMat.setQuick(v1_Pos + startRow, st_2_Pos + startColumn, V1limiter.get_dx_dy() * (-1));       //V1limiter.get_dx_dy = 1.0

                jacMat.setQuick(pmech_Pos + startRow, st_3_Pos + startColumn, 1);
            }
        }

        // calculate dx/dt = f = f(x,y)
        public void update_f(DoubleMatrix2D f)
        {
            if (atGen.InService)
            {
                f.setQuick(st_1_Pos, 0, ((omega - omegaRef) * (1 - K1) - st_1) / T1);
                f.setQuick(st_2_Pos, 0, (((omega - omegaRef) * K1 + st_1) * K - st_2) / T3);
                f.setQuick(st_3_Pos, 0, ((1 - K2) * v1 - st_3) / (0.5 * T4));

                //CustomMessageHandler.println("Dif equ = " + f.getQuick(st_1_Pos, 0) + "\n" + f.getQuick(st_2_Pos, 0) + "\n" + f.getQuick(st_2_Pos, 0));
            }
        }

        // d_agcPRef/dt = (prefAGCOrder - agcPRef)/agcPRampT);
        public void update_fy(DoubleMatrix2D jacMat, int startRow, int startColumn, double simTheta, double h)
        {
            double tStepCof = -simTheta * h;
            if (atGen.InService)
            {
                jacMat.setQuick(st_3_Pos + startRow, v1_Pos + startColumn, (1 - K2) / (0.5 * T4) * tStepCof);
            }
        }

        // calculate fx = df/dx 
        public void update_fx(DoubleMatrix2D jacMat, int startRow, int startColumn, double simTheta, double h)
        {
            double tStepCof = -simTheta * h;
            if (atGen.InService)
            {
                jacMat.setQuick(st_1_Pos + startRow, st_1_Pos + startColumn, (-1 / T1) * tStepCof);
                jacMat.setQuick(st_1_Pos + startRow, omega_Pos + startColumn, ((1 - K1) / T1) * tStepCof);

                jacMat.setQuick(st_2_Pos + startRow, st_1_Pos + startColumn, K / T3 * tStepCof);
                jacMat.setQuick(st_2_Pos + startRow, st_2_Pos + startColumn, -1 / T3 * tStepCof);
                jacMat.setQuick(st_2_Pos + startRow, omega_Pos + startColumn, K1 * K / T3 * tStepCof);

                jacMat.setQuick(st_3_Pos + startRow, st_3_Pos + startColumn, -1 / (0.5 * T4) * tStepCof);
            }
            else
            {
                jacMat.setQuick(st_1_Pos + startRow, st_1_Pos + startColumn, tStepCof);
                jacMat.setQuick(st_2_Pos + startRow, st_2_Pos + startColumn, tStepCof);
                jacMat.setQuick(st_3_Pos + startRow, st_3_Pos + startColumn, tStepCof);
            }
        }


        // set AGC additional power setting in terms of torque (check the generator capacity)
        public void setAGCSingal(double setVal)
        {
            prefAGCOrder = setVal;
        }

        // set Governor MW setting point
        public void setMWRef(double setVal)
        {
            prefSchOrder = setVal;
        }

        public double getTotalMWRef()
        {
            return (prefSchOrder + prefAGCOrder);
        }


        // data sanity check per p.641 on PSS/E Volume II Program Application Guide
        public bool paraSanityTest()
        {
            sanityCheckStr = dataProcess.dataLimitCheck("K", K, 5, "", true, 30.0, "", true, "\n")
                             + dataProcess.dataLimitCheck("T1", T1, 0, "", true, 100.0, "", false, "\n")
                             + dataProcess.dataLimitCheck("T2", T2, 0, "", true, 10.0, "", false, "\n")
                             + dataProcess.dataLimitCheck("T3", T3, 0, "", false, 1.0, "", true, "\n")
                             + dataProcess.dataLimitCheck("PMAX", PMAX, 0.5, "", true, 1.5, "", true, "\n")
                              + dataProcess.dataLimitCheck("PMIN", PMIN, 0, "", true, 0.5, "", true, "\n")
                              + dataProcess.dataLimitCheck("T4", T4, 0, "", false, 5.0, "", true, "\n")
                             + dataProcess.dataLimitCheck("PMAX", PMAX, -999.0, "", false, pf_pMax, "pfPMAX", false, "\n")
                             + dataProcess.dataLimitCheck("PMIN", PMIN, pf_pMin, "pfPMIN", false, 999.0, "", false, "\n");

            return sanityCheckStr.isEmpty();
        }



        // export data for tabular showing 
        public Object[] setTable()
        {
            Object[] ret = new Object[tableColNum];
            ret[0] = atBusNum;
            ret[1] = ID;
            ret[2] = _String.format("%1.4f", K);
            ret[3] = _String.format("%1.4f", T1);
            ret[4] = _String.format("%1.4f", T2);
            ret[5] = _String.format("%1.4f", T3);
            ret[4] = _String.format("%1.4f", PMAX / baseRatio);
            ret[5] = _String.format("%1.4f", PMIN / baseRatio);
            ret[8] = _String.format("%1.4f", T4);
            return ret;
        }


        // debug code [check limit output]
        public double limitTest()
        {
            return v1;
        }

        public override string[] AsArrayForRow()
        {
            return new[]
            {
                atBusNum.ToString(),
                ID,
                _String.format("%1.4f", K),
                _String.format("%1.4f", T1),
                _String.format("%1.4f", T2),
                _String.format("%1.4f", T3),
                _String.format("%1.4f", PMAX / baseRatio),
                _String.format("%1.4f", PMIN / baseRatio),
                _String.format("%1.4f", T4),
            };
        }
    }

}
