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
    public class TGOV1 : govModel
    {

        // default parameters position in input token 
        public const int Default_R = 3; // p.u.

        public const int Default_T1 = 4; // p.u.
        public const int Default_VMAX = 5; // p.u.
        public const int Default_VMIN = 6;
        public const int Default_T2 = 7;
        public const int Default_T3 = 8;
        public const int Default_Dt = 9;

        // model dynamic parameter fields 
        public double R = 0.0; // Droop (p.u.)	

        public double T1 = 0.0; //
        public double Vmax = 0.0; // 
        public double Vmin = 0.0; //  
        public double T2 = 0.0; // 
        public double T3 = 0.0; //  
        public double Dt = 0.0; //  

        // algebraic variables 
        public const int NUM_ALGE = 1; // algebraic variable number

        public double pRefIn = 0.0; // synthesized mechanic power reference, before limiter

        public int pRefIn_Pos = 0;
        public int last_AlgeVar_Pos = 0;

        // differential variables 
        public const int NUM_STATE = 2;

        public double st_1 = 0.0; // state variable associated with 1/(1+T1s)
        public double st_2 = 0.0; // state variable associated with (1+T2s)/(1+T3s)

        public int st_1_Pos = 0;
        public int st_2_Pos = 1;
        public int last_StateVar_Pos = 0;

        // variables inherit from generator class
        public gen atGen;

        public int atBusNum = 0;
        public String ID = "";
        public double MBase = 0.0; // machine base

        public double omega = 0.0;
        public int omega_Pos = 0; // omega index 

        public double pmech = 0.0;
        public int pmech_Pos = 0; // mechanical power setting inherit from generators 

        public double agcMWWeight = 0.0; // 
        public int agcPRefTotal_Pos;

        public double prefMax = 0.0; // generator Pmax 
        public double prefMin = 0.0; // generator Pmin

        // extra data and variables 
        public double omegaRef = 1.0; // speed reference (p.u.) 

        public double baseRatio = 0.0; // MBase/SBASE
        public double K_b2 = 0.0; // T2/T3 (constant for block 2) 

        // external setting values 
        public double prefSchOrder = 0.0; // P reference from day-ahead schedules 

        public double prefAGCOrder = 0.0; // additional ref from AGC control  prefAGCOrder = AGCPRefTotal*weight

        // limiter
        public nWLSTCB Vlimiter;

        // sanity check warning string
        public String sanityCheckStr = "";

        // presenting data purpose 
        public override String[] header { get; set; } = { "Number", "ID", "R", "T1", "Vmax", "Vmin", "T2", "T3", "Dt" };


        public static int tableColNum = 9;


        // constructor
        public TGOV1(gen genTemp, String[] token, int numAlge, int numState)
        {

            // load data can be loaded from power flow case
            atGen = genTemp;
            atBusNum = atGen.I;
            ID = atGen.ID;
            prefMax = atGen.PT;
            prefMin = atGen.PB;
            MBase = atGen.MBASE;

            // algebraic variable index 
            pRefIn_Pos = pRefIn_Pos + numAlge;
            last_AlgeVar_Pos = numAlge + NUM_ALGE;

            // variable position index 
            st_1_Pos = st_1_Pos + numState;
            st_2_Pos = st_2_Pos + numState;
            last_StateVar_Pos = numState + NUM_STATE;

            baseRatio = MBase / SBASE; // defined before loading data 

            // dynamic parameters
            R = Double.Parse(token[Default_R]);
            T1 = Double.Parse(token[Default_T1]);
            Vmax = Double.Parse(token[Default_VMAX]);
            Vmin = Double.Parse(token[Default_VMIN]);
            T2 = Double.Parse(token[Default_T2]);
            T3 = Double.Parse(token[Default_T3]);
            Dt = Double.Parse(token[Default_Dt]);

            // SanityTest 
            if (!paraSanityTest())
                CustomMessageHandler.print(_String.format("Warning: TGOV1 at [%3d, %2s]\n", atBusNum, ID) +
                                           sanityCheckStr);

            // convert to system base
            Vmax = Vmax * baseRatio;
            Vmin = Vmin * baseRatio;
            Dt = Dt * baseRatio;

            Vlimiter = new nWLSTCB(Vmax, Vmin, 1, T1);

            K_b2 = T2 / T3;

            // update generator dynamic model status 
            genTemp.govDyn = this;
            genTemp.hasGovModel = true;

            CustomMessageHandler.println("TGOV1 Model at " + atBusNum + " ID = " + ID + " is loaded!");
        }


        // initialize the element in x vector 
        public void ini(DoubleMatrix2D yVector, DoubleMatrix2D xVector)
        {

            // data available after generator dynamic data are read
            omega_Pos = atGen.omega_Pos;
            pmech_Pos = atGen.pmech_Pos;

            // calculate initial values of algebraic variables 
            pmech = atGen.genDyn.getPm();
            st_1 = pmech;
            st_2 = (1 - K_b2) * st_1;
            pRefIn = st_1;
            prefSchOrder = pRefIn; // hourly schedule order + AGC schedule setting 
            prefAGCOrder = 0.0; // AGC supplemental order

            agcMWWeight = atGen.AGCSMWhare;
            agcPRefTotal_Pos = atGen.agcPRefTotal_Pos;

            // update the dynamic state variable index 
            yVector.setQuick(pRefIn_Pos, 0, pRefIn);

            // initialize state variables
            xVector.setQuick(st_1_Pos, 0, st_1);
            xVector.setQuick(st_2_Pos, 0, st_2);

            CustomMessageHandler.println("st_1 = " + st_1);
            // if limiter is violated at initialization stage, program stalls 
            if (!Vlimiter.checkLimit(st_1).isEmpty())
                throw new simException(_String.format("ERROR: TGOV1 at [%3d, %2s] Vlimiter", atBusNum, ID) +
                                       Vlimiter.getCheckMesg());

            // enable limit 
            Vlimiter.enable();

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
                omega = xVector.getQuick(omega_Pos, 0);

                pmech = yVector.getQuick(pmech_Pos, 0); // update algebraic variables 
                pRefIn = yVector.getQuick(pRefIn_Pos, 0);

                Vlimiter.calc(pRefIn, st_1);
                st_1 = Vlimiter.get_x(); // update state variables associated with limiter

                //prefAGCOrder = agcMWWeight*yVector.getQuick(agcPRefTotal_Pos,0); 
            }
        }


        // calculate algebraic equations   
        public void update_g(DoubleMatrix2D g)
        {
            if (atGen.InService)
            {
                g.setQuick(pRefIn_Pos, 0, -((prefAGCOrder + prefSchOrder) - 1 / R * (omega - omegaRef) - pRefIn));

                // calculate its impact on generator mechanical torque equation //g.setQuick(pmech_Pos, 0, -(k1*k2*stg1 + k2*stg2 + stg3 - pmech));	
                g.setQuick(pmech_Pos, 0, g.getQuick(pmech_Pos, 0) - (K_b2 * st_1 + st_2 - Dt * (omega - omegaRef)));

                //System.out.println("Alge equ = " + g.getQuick(pRefIn_Pos, 0));
            }
        }


        // calculate gy = dg/dy
        public void update_gy(DoubleMatrix2D jacMat, int startRow, int startColumn)
        {
            if (atGen.InService)
            {
                jacMat.setQuick(pRefIn_Pos + startRow, pRefIn_Pos + startColumn, -1);
            }
            else
            {
                jacMat.setQuick(pRefIn_Pos + startRow, pRefIn_Pos + startColumn, 1);
            }
        }


        // calculate gx = dg/dx
        public void update_gx(DoubleMatrix2D jacMat, int startRow, int startColumn)
        {
            if (atGen.InService)
            {
                jacMat.setQuick(pRefIn_Pos + startRow, omega_Pos + startColumn, -1 / R);

                jacMat.setQuick(pmech_Pos + startRow, st_1_Pos + startColumn, K_b2);
                jacMat.setQuick(pmech_Pos + startRow, st_2_Pos + startColumn, 1);
                jacMat.setQuick(pmech_Pos + startRow, omega_Pos + startColumn, -Dt);
            }
        }


        // calculate dx/dt = f = f(x,y)
        public void update_f(DoubleMatrix2D f)
        {
            if (atGen.InService)
            {
                f.setQuick(st_1_Pos, 0, Vlimiter.get_dx());
                f.setQuick(st_2_Pos, 0, ((1 - K_b2) * st_1 - st_2) / T3);
                //System.out.println("Dif equ = " + f.getQuick(st_1_Pos, 0) + "\n" + f.getQuick(st_2_Pos, 0));
            }
        }

        // d_agcPRef/dt = (prefAGCOrder - agcPRef)/agcPRampT);
        public void update_fy(DoubleMatrix2D jacMat, int startRow, int startColumn, double simTheta, double h)
        {
            double tStepCof = -simTheta * h;
            if (atGen.InService)
            {
                jacMat.setQuick(st_1_Pos + startRow, pRefIn_Pos + startColumn, Vlimiter.get_fy() * tStepCof);
            }
        }

        // calculate fx = df/dx 
        public void update_fx(DoubleMatrix2D jacMat, int startRow, int startColumn, double simTheta, double h)
        {
            double tStepCof = -simTheta * h;
            if (atGen.InService)
            {
                jacMat.setQuick(st_1_Pos + startRow, st_1_Pos + startColumn, Vlimiter.get_fx() * tStepCof);

                jacMat.setQuick(st_2_Pos + startRow, st_1_Pos + startColumn, (1 - K_b2) * (1) * tStepCof);
                jacMat.setQuick(st_2_Pos + startRow, st_2_Pos + startColumn, (-1) / T3 * tStepCof);
            }
            else
            {
                jacMat.setQuick(st_1_Pos + startRow, st_1_Pos + startColumn, tStepCof);
                jacMat.setQuick(st_2_Pos + startRow, st_2_Pos + startColumn, tStepCof);
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
            sanityCheckStr = dataProcess.dataLimitCheck("R", R, 0, "", false, 0.1, "", true, "\n")
                             + dataProcess.dataLimitCheck("T1", T1, 0, "", false, 0.5, "", true, "\n")
                             + dataProcess.dataLimitCheck("VMAX", Vmax, 0.5, "", false, 1.2, "", true, "\n")
                             + dataProcess.dataLimitCheck("VMIN", Vmin, 0, "", true, 1.0, "", true, "\n")
                             + dataProcess.dataLimitCheck("VMIN", Vmin, -999, "", false, Vmax, "VMAX", false, "\n")
                             + dataProcess.dataLimitCheck("T2", T2, 0, "", false, 0.5 * T3, "0.5*T3", true, "\n")
                             + dataProcess.dataLimitCheck("T3", T3, 0, "", false, 10, "", true, "\n")
                             + dataProcess.dataLimitCheck("Dt", Dt, 0, "", true, 0.5, "", true, "\n");

            return sanityCheckStr.isEmpty();
        }


        public override string[] AsArrayForRow()
        {
            return new[]
            {
                atBusNum.ToString(),
                ID,
                _String.format("%1.4f", R),
                _String.format("%1.4f", T1),
                _String.format("%1.4f", Vmax / baseRatio),
                _String.format("%1.4f", Vmin / baseRatio),
                _String.format("%1.4f", T2),
                _String.format("%1.4f", T3),
                _String.format("%1.4f", Dt / baseRatio)
            };
        }

        // export data for tabular showing 
        //public Object[] setTable()
        //{
        //    Object[] ret = new Object[tableColNum];
        //    ret[0] = atBusNum;
        //    ret[1] = ID;
        //    ret[2] = _String.format("%1.4f", R);
        //    ret[3] = _String.format("%1.4f", T1);
        //    ret[4] = _String.format("%1.4f", Vmax / baseRatio);
        //    ret[5] = _String.format("%1.4f", Vmin / baseRatio);
        //    ret[6] = _String.format("%1.4f", T2);
        //    ret[7] = _String.format("%1.4f", T3);
        //    ret[8] = _String.format("%1.4f", Dt / baseRatio);
        //    return ret;
        //}

        // debug code [check limit output]
        public double limitTest()
        {
            return st_1;
        }

    }
}
