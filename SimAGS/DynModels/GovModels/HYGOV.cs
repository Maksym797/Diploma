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
    class HYGOV : govModel
    {

        // default parameters position in input token 
        public const int Default_R = 3;             // R, permanent droop
        public const int Default_r = 4;             // r, temporary droop
        public const int Default_Tr = 5;            // Tr(>0), governor time constant
        public const int Default_Tf = 6;            // Tf(>0), filter time constant 			
        public const int Default_Tg = 7;            // Tg(>0), servo time constant 	
        public const int Default_VELM = 8;          // gate velocity limit 
        public const int Default_GMAX = 9;          // GMAX, maximum gate limit 
        public const int Default_GMIN = 10;         // GMIN, minimum gate limit
        public const int Default_Tw = 11;           // water time constant
        public const int Default_At = 12;           // At, turbine gain
        public const int Default_Dturb = 13;            // Dturb, turbnine damping
        public const int Default_qNL = 14;          // QNL, no power flow 

        // model dynamic parameter fields 
        public double R = 0.0;                      // Droop (p.u.)	
        public double r = 0.0;
        public double Tr = 0.0;
        public double Tf = 0.0;
        public double Tg = 0.0;
        public double VELM = 0.0;
        public double GMAX = 0.0;
        public double GMIN = 0.0;
        public double Tw = 0.0;
        public double At = 0.0;
        public double Dturb = 0.0;
        public double qNL = 0.0;


        // algebraic variables 
        public const int NUM_ALGE = 2;      // algebraic variable number
        public double pRefIn = 0.0;         // synthesized mechanic power reference, before limiter
        public double ag_h = 0.0;           // water head

        public int pRefIn_Pos = 0;
        public int ag_h_Pos = 1;
        public int last_AlgeVar_Pos = 0;

        // differential variables 
        public const int NUM_STATE = 4;
        public double st_e = 0.0;           // state variable e	
        public double st_c = 0.0;           // state variable c
        public double st_g = 0.0;           // state variable g (gate position from governor) 
        public double st_q = 0.0;           // state variable q (water flow rate, p.u.)

        public int st_e_Pos = 0;
        public int st_c_Pos = 1;
        public int st_g_Pos = 2;
        public int st_q_Pos = 3;
        public int last_StateVar_Pos = 0;

        // calculate parameters
        public double K1 = 0.0;         // k1 = Tr/Tf
        public double baseRatio = 0.0;      // ratio = MBase/SBASE;

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

        public double prefMax = 0.0;        // generator Pmax 
        public double prefMin = 0.0;        // generator Pmin

        // external setting values 
        public double prefSchOrder = 0.0;   // P reference from day-ahead schedules 
        public double prefAGCOrder = 0.0;   // additional ref from AGC control  prefAGCOrder = AGCPRefTotal*weight

        public double omegaRef = 1.0;       // speed reference (p.u.) 

        // limiter 
        public magLimit gPosLimiter;    // gate position limiter
        public rateLimit gVelLimiter;   // gate velocity limiter (NEED to define the integrator limiter)

        // sanity check warning string
        public String sanityCheckStr = "";

        // presenting data purpose 
        public override string[] header { get; set; } =
            {"Number", "ID", "R", "r", "Tr", "Tf", "Tg", "VELM", "GMAX", "GMIN", "Tw", "At", "Dturb", "qNL"};

        public static int tableColNum = 14;

        // constructor
        public HYGOV(gen genTemp, String[] token, int numAlge, int numState)
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
            ag_h_Pos = ag_h_Pos + numAlge;
            last_AlgeVar_Pos = numAlge + NUM_ALGE;

            // variable position index 

            st_e_Pos = st_e_Pos + numState;
            st_c_Pos = st_c_Pos + numState;
            st_g_Pos = st_g_Pos + numState;
            st_q_Pos = st_q_Pos + numState;
            last_StateVar_Pos = numState + NUM_STATE;

            baseRatio = MBase / SBASE;                  // defined before loading data

            // dynamic parameters
            R = Double.Parse(token[Default_R]);       // on system base 
            r = Double.Parse(token[Default_r]);       // on system base 
            Tr = Double.Parse(token[Default_Tr]);
            Tf = Double.Parse(token[Default_Tf]);
            Tg = Double.Parse(token[Default_Tg]);
            VELM = Double.Parse(token[Default_VELM]) * baseRatio;
            GMAX = Double.Parse(token[Default_GMAX]) * baseRatio;
            GMIN = Double.Parse(token[Default_GMIN]) * baseRatio;
            Tw = Double.Parse(token[Default_Tw]);
            At = Double.Parse(token[Default_At]);     //gain - no unit
            Dturb = Double.Parse(token[Default_Dturb]);       //gain - no unit
            qNL = Double.Parse(token[Default_qNL]);

            // SanityTest 
            if (!paraSanityTest()) CustomMessageHandler.print(_String.format("Warning: HYGOV at [%3d, %2s]\n", atBusNum, ID) + sanityCheckStr);

            // convert to system base
            //R	= R/baseRatio;				
            //r	= r/baseRatio; 

            // assign limiters to limit position and velocity of state variable c
            gPosLimiter = new magLimit(GMAX, GMIN);
            gVelLimiter = new rateLimit(VELM, -VELM, Tr * r);

            K1 = Tr / Tf;

            // update generator dynamic model status 
            genTemp.govDyn = this;
            genTemp.hasGovModel = true;

            CustomMessageHandler.println("HYGOV Model at " + genTemp.I + " ID = " + genTemp.ID + " is loaded!");
        }


        // initialize the element in x vector 
        public void ini(DoubleMatrix2D yVector, DoubleMatrix2D xVector)
        {

            // data available after generator dynamic data are read
            omega_Pos = atGen.omega_Pos;
            pmech_Pos = atGen.pmech_Pos;

            // calculate initial values of algebraic variables 
            pmech = atGen.genDyn.getPm();
            ag_h = 1.0;                     // set d(st_q)/d(t) = 0.0; 	
            st_q = pmech / At / ag_h + qNL;         // set PM = PM calculated from generators 		
            st_g = st_q / Math.Sqrt(ag_h);
            st_c = st_g;                        // st_c = st_g	
            st_e = 0.0;                     // e + Tr*d(e) = rTr*d(c) 
            pRefIn = 0.0;
            prefSchOrder = pRefIn + R * st_c;    // generator scheduling setting point 

            // update the dynamic state variable index 
            yVector.setQuick(pRefIn_Pos, 0, pRefIn);
            yVector.setQuick(ag_h_Pos, 0, ag_h);

            // initialize state variables
            xVector.setQuick(st_e_Pos, 0, st_e);
            xVector.setQuick(st_c_Pos, 0, st_c);
            xVector.setQuick(st_g_Pos, 0, st_g);
            xVector.setQuick(st_q_Pos, 0, st_q);

            // if limiter is violated at initialization stage, program stalls 
            if (!gPosLimiter.checkLimit(st_c).isEmpty()) throw new simException(_String.format("ERROR: HYGOV at [%3d, %2s] gPosLimiter", atBusNum, ID) + gPosLimiter.getCheckMesg());
            if (!gVelLimiter.checkLimit((K1 * pRefIn + (1 - K1) * st_e)).isEmpty()) throw new simException(_String.format("ERROR: HYGOV at [%3d, %2s] gVelLimiter", atBusNum, ID) + gVelLimiter.getCheckMesg());

            // enable limiter
            gPosLimiter.enable();
            gVelLimiter.enable();

            // 
            atGen.genDyn.zeroPm0();         // set Pm0 = 0;
        }

        public void update_Var(DoubleMatrix2D yVector, DoubleMatrix2D xVector)
        {

            if (atGen.InService)
            {
                //update state variables
                st_e = xVector.getQuick(st_e_Pos, 0);
                st_c = xVector.getQuick(st_c_Pos, 0);
                st_g = xVector.getQuick(st_g_Pos, 0);
                st_q = xVector.getQuick(st_q_Pos, 0);

                //
                pRefIn = yVector.getQuick(pRefIn_Pos, 0);
                ag_h = yVector.getQuick(ag_h_Pos, 0);

                // update gate position limiter
                gPosLimiter.calc(st_c);
                st_c = gPosLimiter.get_x();                     // update state variable limited by the limiter block 

                gVelLimiter.calc(K1 * pRefIn + (1 - K1) * st_e);

                //update intermediate variables 
                omega = xVector.getQuick(omega_Pos, 0);
                pmech = yVector.getQuick(pmech_Pos, 0);     //

                //prefAGCOrder = agcMWWeight*yVector.getQuick(agcPRefTotal_Pos,0); 
            }
        }


        // calculate algebraic equations   
        public void update_g(DoubleMatrix2D g)
        {
            if (atGen.InService)
            {
                g.setQuick(pRefIn_Pos, 0, -((prefAGCOrder + prefSchOrder - (omega - omegaRef) - st_c * R) - pRefIn));
                g.setQuick(ag_h_Pos, 0, -((st_g / st_q) * (st_g / st_q) - ag_h));

                g.setQuick(pmech_Pos, 0, g.getQuick(pmech_Pos, 0) - (At * ag_h * (st_q - qNL) - (omega - omegaRef) * st_g * Dturb));
                //System.out.println("======alge equ =======\n" + g.getQuick(pRefIn_Pos,0) + "\n" + g.getQuick(ag_h_Pos,0) + "\n" + g.getQuick(pmech_Pos, 0));
            }
        }


        // calculate gy = dg/dy
        public void update_gy(DoubleMatrix2D jacMat, int startRow, int startColumn)
        {
            if (atGen.InService)
            {
                jacMat.setQuick(pRefIn_Pos + startRow, pRefIn_Pos + startColumn, -1);
                jacMat.setQuick(ag_h_Pos + startRow, ag_h_Pos + startColumn, -1);

                jacMat.setQuick(pmech_Pos + startRow, ag_h_Pos + startColumn, At * (st_q - qNL));
            }
            else
            {
                jacMat.setQuick(pRefIn_Pos + startRow, pRefIn_Pos + startColumn, 1);
                jacMat.setQuick(ag_h_Pos + startRow, ag_h_Pos + startColumn, 1);
            }
        }


        // calculate gx = dg/dx
        public void update_gx(DoubleMatrix2D jacMat, int startRow, int startColumn)
        {
            if (atGen.InService)
            {
                jacMat.setQuick(pRefIn_Pos + startRow, omega_Pos + startColumn, -1);
                jacMat.setQuick(pRefIn_Pos + startRow, st_c_Pos + startColumn, -R);

                jacMat.setQuick(ag_h_Pos + startRow, st_g_Pos + startColumn, 2 * st_g / st_q / st_q);
                jacMat.setQuick(ag_h_Pos + startRow, st_q_Pos + startColumn, -2 * st_g * st_g / st_q / st_q / st_q);

                jacMat.setQuick(pmech_Pos + startRow, omega_Pos + startColumn, -st_g * Dturb);
                jacMat.setQuick(pmech_Pos + startRow, st_g_Pos + startColumn, -(omega - omegaRef) * Dturb);
                jacMat.setQuick(pmech_Pos + startRow, st_q_Pos + startColumn, At * ag_h);
            }
        }


        // calculate dx/dt = f = f(x,y)
        public void update_f(DoubleMatrix2D f)
        {
            if (atGen.InService)
            {
                f.setQuick(st_e_Pos, 0, (pRefIn - st_e) / Tf);
                f.setQuick(st_c_Pos, 0, gVelLimiter.get_dx());
                f.setQuick(st_g_Pos, 0, (st_c - st_g) / Tg);
                f.setQuick(st_q_Pos, 0, (1 - ag_h) / Tw);
                //System.out.println("======dif equ =======\n" + f.getQuick(st_e_Pos,0) + "\n" + f.getQuick(st_c_Pos,0) + "\n" + f.getQuick(st_g_Pos, 0) + "\n" + f.getQuick(st_q_Pos, 0));
            }
        }


        // d_agcPRef/dt = (prefAGCOrder - agcPRef)/agcPRampT);
        public void update_fy(DoubleMatrix2D jacMat, int startRow, int startColumn, double simTheta, double h)
        {
            double tStepCof = -simTheta * h;
            if (atGen.InService)
            {
                jacMat.setQuick(startRow + st_e_Pos, startColumn + pRefIn_Pos, 1 / Tf * tStepCof);
                jacMat.setQuick(startRow + st_c_Pos, startColumn + pRefIn_Pos, gVelLimiter.get_fy() * K1 * tStepCof);
                jacMat.setQuick(startRow + st_q_Pos, startColumn + ag_h_Pos, 1 / Tw * (-1) * tStepCof);
            }
        }

        // calculate fx = df/dx 
        public void update_fx(DoubleMatrix2D jacMat, int startRow, int startColumn, double simTheta, double h)
        {
            double tStepCof = -simTheta * h;
            if (atGen.InService)
            {
                jacMat.setQuick(startRow + st_e_Pos, startColumn + st_e_Pos, (-1) / Tf * tStepCof);
                jacMat.setQuick(startRow + st_c_Pos, startColumn + st_e_Pos, gVelLimiter.get_fy() * (1 - K1) * tStepCof);
                jacMat.setQuick(startRow + st_g_Pos, startColumn + st_c_Pos, 1 / Tg * tStepCof);
                jacMat.setQuick(startRow + st_g_Pos, startColumn + st_g_Pos, -1 / Tg * tStepCof);
            }
            else
            {
                jacMat.setQuick(startRow + st_e_Pos, startColumn + st_e_Pos, tStepCof);
                jacMat.setQuick(startRow + st_c_Pos, startColumn + st_c_Pos, tStepCof);
                jacMat.setQuick(startRow + st_g_Pos, startColumn + st_g_Pos, tStepCof);
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
                             + dataProcess.dataLimitCheck("r", r, 0, "", false, 2.0, "", true, "\n")
                              + dataProcess.dataLimitCheck("Tr", Tr, 0, "", false, 30.0, "", true, "\n")
                              + dataProcess.dataLimitCheck("Tf", Tf, 0, "", false, 0.1, "", true, "\n")
                              + dataProcess.dataLimitCheck("Tg", Tg, 0, "", false, 1.0, "", true, "\n")
                              + dataProcess.dataLimitCheck("VELM", VELM, 0, "", false, 0.3, "", true, "\n")
                              + dataProcess.dataLimitCheck("GMAX", GMAX, 0, "", false, 1.0, "", true, "\n")
                              + dataProcess.dataLimitCheck("GMIN", GMIN, 0, "", true, 1.0, "", true, "\n")
                              + dataProcess.dataLimitCheck("GMIN", GMIN, -999, "", false, GMAX, "GMAX", false, "\n")
                              + dataProcess.dataLimitCheck("Tw", Tw, 0.5, "", false, 3.0, "", true, "\n")
                              + dataProcess.dataLimitCheck("At", At, 0.8, "", false, 1.5, "", true, "\n")
                              + dataProcess.dataLimitCheck("Dturb", Dturb, 0, "", true, 0.5, "", true, "\n")
                              + dataProcess.dataLimitCheck("qNL", qNL, 0, "", false, 0.15, "", true, "\n");

            return sanityCheckStr.isEmpty();
        }


        public override string[] AsArrayForRow()
        {
            return new[]
            {
                atBusNum.ToString(),
                ID,
                _String.format("%1.4f", R),
                _String.format("%1.4f", r),
                _String.format("%1.4f", Tr),
                _String.format("%1.4f", Tf),
                _String.format("%1.4f", Tg),
                _String.format("%1.4f", VELM / baseRatio),
                _String.format("%1.4f", GMAX / baseRatio),
                _String.format("%1.4f", GMIN / baseRatio),
                _String.format("%1.4f", Tw),
                _String.format("%1.4f", At),
                _String.format("%1.4f", Dturb),
                _String.format("%1.4f", qNL)
            };
        }
        // export data for tabular showing 
        //public Object[] setTable()
        //{
        //    Object[] ret = new Object[tableColNum];
        //    ret[0] = atBusNum;
        //    ret[1] = ID;
        //    ret[2] = _String.format("%1.4f", R);
        //    ret[3] = _String.format("%1.4f", r);
        //    ret[4] = _String.format("%1.4f", Tr);
        //    ret[5] = _String.format("%1.4f", Tf);
        //    ret[6] = _String.format("%1.4f", Tg);
        //    ret[7] = _String.format("%1.4f", VELM / baseRatio);
        //    ret[8] = _String.format("%1.4f", GMAX / baseRatio);
        //    ret[9] = _String.format("%1.4f", GMIN / baseRatio);
        //    ret[10] = _String.format("%1.4f", Tw);
        //    ret[11] = _String.format("%1.4f", At);
        //    ret[12] = _String.format("%1.4f", Dturb);
        //    ret[13] = _String.format("%1.4f", qNL);
        //    return ret;
        //}

        // debug code [check limit output]
        public double limitTest()
        {
            return gVelLimiter.get_dx();
        }
    }

}
