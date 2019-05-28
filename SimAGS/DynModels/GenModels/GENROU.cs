using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cern.colt.matrix;
using SimAGS.Components;
using SimAGS.Handlers;

namespace SimAGS.DynModels.GenModels
{
    public class GENROU : genModel
    {
        // default value of Tgov if no GOV is modeled  
        public const double w0 = 2 * 60 * Math.PI;

        // default parameters position in input token 
        public const int Default_Td0p = 3;
        public const int Default_Td0pp = 4;
        public const int Default_Tq0p = 5;
        public const int Default_Tq0pp = 6;
        public const int Default_H = 7;
        public const int Default_D = 8;
        public const int Default_Xd = 9;
        public const int Default_Xq = 10;
        public const int Default_Xdp = 11;
        public const int Default_Xqp = 12;
        public const int Default_Xdpp = 13;
        public const int Default_Xl = 14;
        public const int Default_S10 = 15;
        public const int Default_S12 = 16;

        // model dynamic parameter fields 
        public double Td0p = 0.0;
        public double Td0pp = 0.0;
        public double Tq0p = 0.0;
        public double Tq0pp = 0.0;
        public double H = 0.0;
        public double D = 0.0;
        public double Xd = 0.0;
        public double Xq = 0.0;
        public double Xdp = 0.0;
        public double Xqp = 0.0;
        public double Xdpp = 0.0;
        public double Xl = 0.0;
        public double S10 = 0.1;            // default value = 0.1
        public double S12 = 0.4;            // default value = 0.4

        public double Ra = 0.0;             // loaded from power flow case
        public double Xqpp = 0.0;           // by default Xqpp = Xdpp 
        public double baseRatio = 0.0;

        // algebraic variables 
        public const int NUM_ALGE = 8;  // algebraic variable number
        public double iq = 0.0;         // current at q axis 	
        public double id = 0.0;         // current at d axis
        public double pe = 0.0;         // generator MW injection, in pu
        public double qe = 0.0;         // generator MVar injection, in pu 
        public double pm = 0.0;         // mechanic power (can be changed by governor model if exists), rev 07-28-2012
        public double efd = 0.0;            // internal exciter voltage 
        public double psidpp = 0.0;         // psi_d''
        public double psiqpp = 0.0;         // psi_q''

        public int iq_Pos = 0;
        public int id_Pos = 1;
        public int pe_Pos = 2;
        public int qe_Pos = 3;
        public int pm_Pos = 4;
        public int efd_Pos = 5;
        public int psidpp_Pos = 6;
        public int psiqpp_Pos = 7;
        public int last_AlgeVar_Pos = 0;

        // calculated algebraic variables 
        public double vq = 0.0;
        public double vd = 0.0;
        public double cosA = 0.0;
        public double sinA = 0.0;

        // -------------  state variables  --------------// 
        public const int NUM_STATE = 6;                 // total number of state variables
        public double eq_pp = 0.0;          // pis_d along d-axis in pss/e manual 
        public double ed_pp = 0.0;          // psi_q along q-axis in pss/e manual 
        public double eq_p = 0.0;           // Eq'
        public double ed_p = 0.0;           // Ed' 
        public double omega = 0.0;          // generator rotor speed in radians/sec 
        public double delta = 0.0;          // generator rotor angle 

        // state variables position in state vector X 
        public int eq_pp_Pos = 0;
        public int ed_pp_Pos = 1;
        public int eq_p_Pos = 2;
        public int ed_p_Pos = 3;
        public int omega_Pos = 4;
        public int delta_Pos = 5;
        public int last_StateVar_Pos = 0;   // starting position in the state vector

        // variables inherit from generator class (for initialization only)
        public gen atGen;
        public int atBusNum = 0;            // bus number
        public String ID = "";          // generator ID
        public double MBase = 0.0;          // machine base 

        // related position in network variable vector y
        public double vtMag = 0.0;          // terminal voltage magnitude 
        public double vtAng = 0.0;          // terminal voltage angle

        public int vtMag_Pos = 0;
        public int vtAng_Pos = 0;

        // constant 
        public double Kq1 = 0.0;
        public double Kq2 = 0.0;
        public double Kq3 = 0.0;

        public double Kd1 = 0.0;
        public double Kd2 = 0.0;
        public double Kd3 = 0.0;

        public double Kqd = 0.0;            // Kqd = (Xq-Xl)/(Xd-Xl)

        // setting values interface with other devices 
        public double pm0 = 0.0;            // pm setting value 
        public double efd0 = 0.0;           // efd setting value

        // calculate saturation-related parameter
        public double A = 0.0;              // S = B*(E - A)^2/E, S10=0 & S12=0 if not saturated	
        public double B = 0.0;
        public bool bSaturation = false; // 
        public double psipp = 0.0;          // sqrt(psidpp^2 + psidqpp^2)
        public double SCof = 0.0;           // calculated saturation coefficient = SE*Efd = B(Efd-A)^2
        public double Sd = 0.0;         // Sd = SCof*psidpp
        public double Sq = 0.0;         // Sq = SCof*psiqpp*(Xq-Xl)/(Xd-Xl)
        public double Sd_psidpp = 0.0;      // d(Sd)/d(Psid'')
        public double Sd_psiqpp = 0.0;      // d(Sd)/d(Psiq'')
        public double Sq_psidpp = 0.0;      // d(Sq)/d(Psid'')
        public double Sq_psiqpp = 0.0;      // d(Sq)/d(Psiq'')

        // sanity check warning string
        public String sanityCheckStr = "";

        // presenting data purpose 
        public override String[] header {get; set;} = { "Number", "ID", "Td0'", "Td0''", "Tq0'", "Tq0''", "H", "D", "Xd", "Xq", "Xd'", "Xq'", "Xd''", "Xl", "S10", "S12" };
        public static int tableColNum = 16;

        // constructor calculate variables after power flow 
        public GENROU(gen genTemp, String[] token, int numAlge, int numState)
        {

            // data loaded from solved power flow 
            atGen = genTemp;
            Ra = atGen.ZR;              // already converted to system base (resistance is loaded from power flow case)
                                        //Xdp 	 = atGen.ZX; 				// already converted to system base
            MBase = atGen.MBASE;
            atBusNum = atGen.I;
            ID = atGen.ID;

            // algebraic variable index 
            iq_Pos = iq_Pos + numAlge;
            id_Pos = id_Pos + numAlge;
            pe_Pos = pe_Pos + numAlge;
            qe_Pos = qe_Pos + numAlge;
            pm_Pos = pm_Pos + numAlge;
            efd_Pos = efd_Pos + numAlge;
            psiqpp_Pos = psiqpp_Pos + numAlge;
            psidpp_Pos = psidpp_Pos + numAlge;
            last_AlgeVar_Pos = numAlge + NUM_ALGE;

            // variable position index 
            eq_pp_Pos = eq_pp_Pos + numState;
            ed_pp_Pos = ed_pp_Pos + numState;
            eq_p_Pos = eq_p_Pos + numState;
            ed_p_Pos = ed_p_Pos + numState;
            omega_Pos = omega_Pos + numState;
            delta_Pos = delta_Pos + numState;
            last_StateVar_Pos = numState + NUM_STATE;

            baseRatio = MBase / SBASE;

            // dynamic parameters 
            Td0p = Double.Parse(token[Default_Td0p]);
            Td0pp = Double.Parse(token[Default_Td0pp]);
            Tq0p = Double.Parse(token[Default_Tq0p]);
            Tq0pp = Double.Parse(token[Default_Tq0pp]);
            H = Double.Parse(token[Default_H]);
            D = Double.Parse(token[Default_D]);
            Xd = Double.Parse(token[Default_Xd]);
            Xq = Double.Parse(token[Default_Xq]);
            Xdp = Double.Parse(token[Default_Xdp]);
            Xqp = Double.Parse(token[Default_Xqp]);
            Xdpp = Double.Parse(token[Default_Xdpp]);
            Xl = Double.Parse(token[Default_Xl]);
            S10 = Double.Parse(token[Default_S10]);
            S12 = Double.Parse(token[Default_S12]);

            // SanityTest 
            if (!paraSanityTest()) CustomMessageHandler.println(_String.format("Warning: GENROU at [%3d, %2s]\n", atBusNum, ID) + sanityCheckStr);
            //if (!paraSanityTest()) CustomMessageHandler.print(String.format("Warning: GENROU at [%3d, %2s]\n", atBusNum, ID) + sanityCheckStr);

            // convert to system base
            H = H * baseRatio;
            D = D * baseRatio;
            Xd = Xd / baseRatio;
            Xq = Xq / baseRatio;
            Xdp = Xdp / baseRatio;
            Xqp = Xqp / baseRatio;
            Xdpp = Xdpp / baseRatio;
            Xl = Xl / baseRatio;
            Xqpp = Xdpp;                                // Xqpp must equal to Xdpp

            //calculate constant coefficient
            Kd1 = (Xdp - Xdpp) / (Xdp - Xl);
            Kd2 = (Xd - Xdp) / (Xdp - Xl);
            Kd3 = (Xdpp - Xl) / (Xdp - Xl);

            Kq1 = (Xqp - Xqpp) / (Xqp - Xl);
            Kq2 = (Xq - Xqp) / (Xqp - Xl);
            Kq3 = (Xqpp - Xl) / (Xqp - Xl);

            Kqd = (Xq - Xl) / (Xd - Xl);

            // calculate saturation parameters (if any)
            if (S10 != 0 && S12 != 0)
            {
                double kTemp = Math.Sqrt(1.0 * S10 / 1.2 / S12);
                A = (kTemp * 1.2 - 1.0) / (kTemp - 1);
                B = S10 * 1.0 / Math.Pow(1.0 - A, 2);
                bSaturation = true;
            }

            // update generator dynamic model status 
            genTemp.omega_Pos = omega_Pos;      // rotor speed shared with other devices 
            genTemp.pmech_Pos = pm_Pos;         // mechanical torque shared with other devices
            genTemp.efd_Pos = efd_Pos;
            genTemp.genDyn = this;
            genTemp.hasGenModel = true;

            CustomMessageHandler.println("GENROU Model at " + atBusNum + " ID = " + ID + " is loaded!");
        }


        // initialize the element in solution vector solVector =[yVector;xVector] 
        public void ini(DoubleMatrix2D yVector, DoubleMatrix2D xVector)
        {

            vtMag_Pos = atGen.vmagPos;
            vtAng_Pos = atGen.vangPos;
            vtMag = atGen.calcVt;
            vtAng = atGen.calcAng;
            pe = atGen.calcPgen;
            qe = atGen.calcQgen;


            double pfAng = Math.Atan(qe / pe);                          // power factor (pf) angle 
            double iMag = Math.Sqrt(pe * pe + qe * qe) / vtMag;             // current magnitude 
            double vx = vtMag * Math.Cos(vtAng);
            double vy = vtMag * Math.Sin(vtAng);
            double ix = iMag * Math.Cos(vtAng - pfAng);
            double iy = iMag * Math.Sin(vtAng - pfAng);
            double eq_x = Ra * ix - Xq * iy + vx;
            double eq_y = Ra * iy + Xq * ix + vy;

            delta = Math.Atan(eq_y / eq_x);                             // initialize state variable [needs to calculate delta first]
            omega = 1.0;

            vq = vtMag * Math.Cos(delta - vtAng);
            vd = vtMag * Math.Sin(delta - vtAng);
            iq = iMag * Math.Cos(delta - vtAng + pfAng);
            id = iMag * Math.Sin(delta - vtAng + pfAng);

            eq_pp = vq + Ra * iq + Xl * id;
            ed_pp = vd + Ra * id - Xl * iq;
            eq_p = vq + Ra * iq + Xdp * id;
            ed_p = vd + Ra * id - Xqp * iq;
            efd = vq + Ra * iq + Xd * id;
            pm = pe + iMag * iMag * Ra;
            pm0 = pm;
            efd0 = efd;
            psiqpp = -Kq1 * ed_pp - Kq3 * ed_p;
            psidpp = Kd1 * eq_pp + Kd3 * eq_p;

            // update the dynamic state variable index 
            yVector.setQuick(iq_Pos, 0, iq);
            yVector.setQuick(id_Pos, 0, id);
            yVector.setQuick(pe_Pos, 0, pe);
            yVector.setQuick(qe_Pos, 0, qe);
            yVector.setQuick(pm_Pos, 0, pm);
            yVector.setQuick(efd_Pos, 0, efd);
            yVector.setQuick(psiqpp_Pos, 0, psiqpp);
            yVector.setQuick(psidpp_Pos, 0, psidpp);

            // initialize state variables  
            xVector.setQuick(eq_pp_Pos, 0, eq_pp);
            xVector.setQuick(ed_pp_Pos, 0, ed_pp);
            xVector.setQuick(eq_p_Pos, 0, eq_p);
            xVector.setQuick(ed_p_Pos, 0, ed_p);
            xVector.setQuick(omega_Pos, 0, omega);
            xVector.setQuick(delta_Pos, 0, delta);

        }


        // update variables at the beginning of each iteration 
        public void update_Var(DoubleMatrix2D yVector, DoubleMatrix2D xVector)
        {
            if (atGen.InService)
            {
                vtMag = yVector.getQuick(vtMag_Pos, 0);
                vtAng = yVector.getQuick(vtAng_Pos, 0);

                iq = yVector.getQuick(iq_Pos, 0);
                id = yVector.getQuick(id_Pos, 0);
                pe = yVector.getQuick(pe_Pos, 0);
                qe = yVector.getQuick(qe_Pos, 0);
                pm = yVector.getQuick(pm_Pos, 0);
                efd = yVector.getQuick(efd_Pos, 0);
                psiqpp = yVector.getQuick(psiqpp_Pos, 0);
                psidpp = yVector.getQuick(psidpp_Pos, 0);

                eq_pp = xVector.getQuick(eq_pp_Pos, 0);
                ed_pp = xVector.getQuick(ed_pp_Pos, 0);
                eq_p = xVector.getQuick(eq_p_Pos, 0);
                ed_p = xVector.getQuick(ed_p_Pos, 0);
                omega = xVector.getQuick(omega_Pos, 0);
                delta = xVector.getQuick(delta_Pos, 0);

                cosA = Math.Cos(delta - vtAng);
                sinA = Math.Sin(delta - vtAng);
                vq = vtMag * cosA;
                vd = vtMag * sinA;
                psipp = Math.Sqrt(psiqpp * psiqpp + psidpp * psidpp);           // total flux 

                //CustomMessageHandler.println("psiqpp = " + psiqpp + " psidpp = " + psidpp + " psipp = " + psipp); 
                calcSaturation();
            }
        }


        // calculate saturation adjustment
        public void calcSaturation()
        {
            if (bSaturation)
            {
                if (Math.Abs(psipp) < A)
                {
                    SCof = 0.0;
                    Sd = 0.0;
                    Sq = 0.0;
                    Sd_psidpp = 0.0;
                    Sd_psiqpp = 0.0;
                    Sq_psidpp = 0.0;
                    Sq_psiqpp = 0.0;

                }
                else
                {
                    SCof = B * (psipp - A) * (psipp - A);
                    Sd = SCof * psidpp;
                    Sq = SCof * psiqpp * Kqd;
                    Sd_psidpp = SCof + psidpp * (2 * B * (psipp - A) * psidpp / psipp);
                    Sd_psiqpp = psidpp * (2 * B * (psipp - A) * psiqpp / psipp);
                    Sq_psidpp = psidpp * Kqd * (2 * B * (psipp - A) * psidpp / psipp);
                    Sd_psiqpp = SCof * Kqd + psidpp * Kqd * (2 * B * (psipp - A) * psiqpp / psipp);
                }
            }
        }


        public void update_g(DoubleMatrix2D g)
        {
            if (atGen.InService)
            {
                g.setQuick(vtAng_Pos, 0, g.getQuick(vtAng_Pos, 0) + pe);        // update network  MW injection [in case multiple generators]
                g.setQuick(vtMag_Pos, 0, g.getQuick(vtMag_Pos, 0) + qe);        // update network MVar injection balance equation 

                g.setQuick(iq_Pos, 0, -(Kd1 * eq_pp + Kd3 * eq_p - Xdpp * id - Ra * iq - vq));  // for simulation
                g.setQuick(id_Pos, 0, -(Kq1 * ed_pp + Kq3 * ed_p + Xqpp * iq - Ra * id - vd));
                g.setQuick(pe_Pos, 0, -(pe - vq * iq - vd * id));
                g.setQuick(qe_Pos, 0, -(qe - vq * id + vd * iq));
                g.setQuick(pm_Pos, 0, -(pm0 - pm));
                g.setQuick(efd_Pos, 0, -(efd0 - efd));
                g.setQuick(psiqpp_Pos, 0, -(psiqpp + Kq1 * ed_pp + Kq3 * ed_p));
                g.setQuick(psidpp_Pos, 0, -(psidpp - Kd1 * eq_pp - Kd3 * eq_p));

                //CustomMessageHandler.println("agle equ at gen " + atBusNum + "\n" + g.getQuick(iq_Pos,0) + "\n" + g.getQuick(id_Pos,0) + "\n" + g.getQuick(pe_Pos,0) + "\n" + g.getQuick(qe_Pos,0) +  "\n" + g.getQuick(pm_Pos,0) + "\n" + g.getQuick(vf_Pos,0)
                //		+ "\n" + g.getQuick(psiqpp_Pos,0) + "\n" + g.getQuick(psidpp_Pos,0));

            }
        }

        // calculate gy = dg/dy
        public void update_gy(DoubleMatrix2D jacMat, int startRow, int startColumn)
        {
            if (atGen.InService)
            {
                jacMat.setQuick(vtAng_Pos + startRow, pe_Pos + startColumn, -1);
                jacMat.setQuick(vtMag_Pos + startRow, qe_Pos + startColumn, -1);

                jacMat.setQuick(iq_Pos + startRow, iq_Pos + startColumn, -Ra);
                jacMat.setQuick(iq_Pos + startRow, id_Pos + startColumn, -Xdpp);
                jacMat.setQuick(iq_Pos + startRow, vtMag_Pos + startColumn, -cosA);
                jacMat.setQuick(iq_Pos + startRow, vtAng_Pos + startColumn, -vd);       // -vtMag*sin(delta-Ang)

                jacMat.setQuick(id_Pos + startRow, iq_Pos + startColumn, Xqpp);
                jacMat.setQuick(id_Pos + startRow, id_Pos + startColumn, -Ra);
                jacMat.setQuick(id_Pos + startRow, vtMag_Pos + startColumn, -sinA);
                jacMat.setQuick(id_Pos + startRow, vtAng_Pos + startColumn, vq);        // vtMag*cos(delta-Ang)

                jacMat.setQuick(pe_Pos + startRow, iq_Pos + startColumn, -vq);
                jacMat.setQuick(pe_Pos + startRow, id_Pos + startColumn, -vd);
                jacMat.setQuick(pe_Pos + startRow, pe_Pos + startColumn, 1);
                jacMat.setQuick(pe_Pos + startRow, vtMag_Pos + startColumn, -iq * cosA - id * sinA);
                jacMat.setQuick(pe_Pos + startRow, vtAng_Pos + startColumn, qe);

                jacMat.setQuick(qe_Pos + startRow, iq_Pos + startColumn, vd);
                jacMat.setQuick(qe_Pos + startRow, id_Pos + startColumn, -vq);
                jacMat.setQuick(qe_Pos + startRow, qe_Pos + startColumn, 1);
                jacMat.setQuick(qe_Pos + startRow, vtMag_Pos + startColumn, -id * cosA + iq * sinA);
                jacMat.setQuick(qe_Pos + startRow, vtAng_Pos + startColumn, -pe);

                jacMat.setQuick(pm_Pos + startRow, pm_Pos + startColumn, -1);           // for simulation -g(*) = - (pm0-pm) = 0 
                jacMat.setQuick(efd_Pos + startRow, efd_Pos + startColumn, -1);

                jacMat.setQuick(psiqpp_Pos + startRow, psiqpp_Pos + startColumn, 1);
                jacMat.setQuick(psidpp_Pos + startRow, psidpp_Pos + startColumn, 1);

            }
            else
            {
                jacMat.setQuick(iq_Pos + startRow, iq_Pos + startColumn, 1);
                jacMat.setQuick(id_Pos + startRow, id_Pos + startColumn, 1);
                jacMat.setQuick(pe_Pos + startRow, pe_Pos + startColumn, 1);
                jacMat.setQuick(qe_Pos + startRow, qe_Pos + startColumn, 1);

                jacMat.setQuick(pm_Pos + startRow, pm_Pos + startColumn, 1);
                jacMat.setQuick(efd_Pos + startRow, efd_Pos + startColumn, 1);

                jacMat.setQuick(psiqpp_Pos + startRow, psiqpp_Pos + startColumn, 1);
                jacMat.setQuick(psidpp_Pos + startRow, psidpp_Pos + startColumn, 1);
            }
        }


        // calculate gx = dg/dx
        public void update_gx(DoubleMatrix2D jacMat, int startRow, int startColumn)
        {
            if (atGen.InService)
            {
                jacMat.setQuick(iq_Pos + startRow, eq_pp_Pos + startColumn, Kd1);
                jacMat.setQuick(iq_Pos + startRow, eq_p_Pos + startColumn, Kd3);

                jacMat.setQuick(id_Pos + startRow, ed_pp_Pos + startColumn, Kq1);
                jacMat.setQuick(id_Pos + startRow, ed_p_Pos + startColumn, Kq3);

                jacMat.setQuick(psiqpp_Pos + startRow, ed_pp_Pos + startColumn, Kq1);
                jacMat.setQuick(psidpp_Pos + startRow, ed_p_Pos + startColumn, Kq3);

                jacMat.setQuick(psidpp_Pos + startRow, eq_pp_Pos + startColumn, -Kd1);
                jacMat.setQuick(psidpp_Pos + startRow, eq_p_Pos + startColumn, -Kd3);
            }
        }


        // calculate dx/dt = f = f(x,y)
        public void update_f(DoubleMatrix2D f)
        {
            if (atGen.InService)
            {
                f.setQuick(eq_pp_Pos, 0, 1 / Td0pp * (-eq_pp + eq_p - (Xdp - Xl) * id));
                f.setQuick(ed_pp_Pos, 0, 1 / Tq0pp * (-ed_pp + ed_p + (Xqp - Xl) * iq));
                f.setQuick(eq_p_Pos, 0, 1 / Td0p * (efd + Kd1 * Kd2 * eq_pp - (1 + Kd1 * Kd2) * eq_p - (Xd - Xdp) * Kd3 * id) - Sd);   // - Sd            
                f.setQuick(ed_p_Pos, 0, 1 / Tq0p * (Kq1 * Kq2 * ed_pp - (1 + Kq1 * Kq2) * ed_p + (Xq - Xqp) * Kq3 * iq) + Sq);    // + Sq
                f.setQuick(omega_Pos, 0, 0.5 / H * (pm - (Kd3 * eq_p * iq + Kd1 * eq_pp * iq + Kq3 * ed_p * id + Kq1 * ed_pp * id + (Xqpp - Xdpp) * id * iq) - (omega - 1) * D));
                f.setQuick(delta_Pos, 0, w0 * (omega - 1));
            }
        }


        // calculate fx = df/dx 
        public void update_fx(DoubleMatrix2D jacMat, int startRow, int startColumn, double simTheta, double h)
        {
            double tStepCof = -simTheta * h;
            if (atGen.InService)
            {

                jacMat.setQuick(startRow + eq_pp_Pos, startColumn + eq_pp_Pos, 1 / Td0pp * (-1) * tStepCof);
                jacMat.setQuick(startRow + eq_pp_Pos, startColumn + eq_p_Pos, 1 / Td0pp * (1) * tStepCof);

                jacMat.setQuick(startRow + ed_pp_Pos, startColumn + ed_pp_Pos, 1 / Tq0pp * (-1) * tStepCof);
                jacMat.setQuick(startRow + ed_pp_Pos, startColumn + ed_p_Pos, 1 / Tq0pp * (1) * tStepCof);

                jacMat.setQuick(startRow + eq_p_Pos, startColumn + eq_pp_Pos, 1 / Td0p * (Kd1 * Kd2) * tStepCof);
                jacMat.setQuick(startRow + eq_p_Pos, startColumn + eq_p_Pos, 1 / Td0p * (-(1 + Kd1 * Kd2)) * tStepCof);

                jacMat.setQuick(startRow + ed_p_Pos, startColumn + ed_pp_Pos, 1 / Tq0p * (Kq1 * Kq2) * tStepCof);
                jacMat.setQuick(startRow + ed_p_Pos, startColumn + ed_p_Pos, 1 / Tq0p * (-(1 + Kq1 * Kq2)) * tStepCof);

                jacMat.setQuick(startRow + omega_Pos, startColumn + omega_Pos, 0.5 / H * (-D) * tStepCof);
                jacMat.setQuick(startRow + omega_Pos, startColumn + eq_pp_Pos, 0.5 / H * (-Kd1 * iq) * tStepCof);
                jacMat.setQuick(startRow + omega_Pos, startColumn + ed_pp_Pos, 0.5 / H * (-Kq1 * id) * tStepCof);
                jacMat.setQuick(startRow + omega_Pos, startColumn + eq_p_Pos, 0.5 / H * (-Kd3 * iq) * tStepCof);
                jacMat.setQuick(startRow + omega_Pos, startColumn + ed_p_Pos, 0.5 / H * (-Kq3 * id) * tStepCof);

                jacMat.setQuick(startRow + delta_Pos, startColumn + omega_Pos, w0 * tStepCof);

            }
            else
            {           // for out-of-service
                jacMat.setQuick(startRow + eq_pp_Pos, startColumn + eq_pp_Pos, tStepCof);
                jacMat.setQuick(startRow + ed_pp_Pos, startColumn + ed_pp_Pos, tStepCof);
                jacMat.setQuick(startRow + eq_p_Pos, startColumn + eq_p_Pos, tStepCof);
                jacMat.setQuick(startRow + ed_p_Pos, startColumn + ed_p_Pos, tStepCof);
                jacMat.setQuick(startRow + omega_Pos, startColumn + omega_Pos, tStepCof);
                jacMat.setQuick(startRow + delta_Pos, startColumn + delta_Pos, tStepCof);
            }
        }

        // calculate fy = df/dy
        public void update_fy(DoubleMatrix2D jacMat, int startRow, int startColumn, double simTheta, double h)
        {
            double tStepCof = -simTheta * h;
            if (atGen.InService)
            {
                jacMat.setQuick(startRow + eq_pp_Pos, startColumn + id_Pos, 1 / Td0pp * (-(Xdp - Xl)) * tStepCof);
                jacMat.setQuick(startRow + ed_pp_Pos, startColumn + iq_Pos, 1 / Tq0pp * (Xqp - Xl) * tStepCof);

                jacMat.setQuick(startRow + eq_p_Pos, startColumn + id_Pos, 1 / Td0p * (-(Xd - Xdp) * Kd3) * tStepCof);
                jacMat.setQuick(startRow + eq_p_Pos, startColumn + efd_Pos, 1 / Td0p * tStepCof);
                jacMat.setQuick(startRow + eq_p_Pos, startColumn + psiqpp_Pos, 1 / Td0p * (-Sd_psiqpp) * tStepCof);
                jacMat.setQuick(startRow + eq_p_Pos, startColumn + psidpp_Pos, 1 / Td0p * (-Sd_psidpp) * tStepCof);

                jacMat.setQuick(startRow + ed_p_Pos, startColumn + iq_Pos, 1 / Tq0p * ((Xq - Xqp) * Kq3) * tStepCof);
                jacMat.setQuick(startRow + ed_p_Pos, startColumn + psidpp_Pos, 1 / Tq0p * (Sq_psidpp) * tStepCof);
                jacMat.setQuick(startRow + ed_p_Pos, startColumn + psiqpp_Pos, 1 / Tq0p * (Sq_psiqpp) * tStepCof);

                jacMat.setQuick(startRow + omega_Pos, startColumn + iq_Pos, 0.5 / H * (-Kd3 * eq_p - Kd1 * eq_pp - (Xqpp - Xdpp) * id) * tStepCof);
                jacMat.setQuick(startRow + omega_Pos, startColumn + id_Pos, 0.5 / H * (-Kq3 * ed_p - Kq1 * ed_pp - (Xqpp - Xdpp) * iq) * tStepCof);
                jacMat.setQuick(startRow + omega_Pos, startColumn + pm_Pos, 0.5 / H * tStepCof);
            }
        }


        public void setPmRef(double pmSet) { pm0 = pmSet; }

        public void zeroPm0() { pm0 = 0; }          // reset pm0 and replaced by governor output

        public void zeroEfd0() { efd0 = 0; }        // rest vf and replaced by exciter output

        public double getPm() { return pm; }

        public double getEfd0() { return efd0; }

        public double getPm0() { return pm0; }

        public double getPegen() { return pe; }

        public double getQegen() { return qe; }

        public double getVt() { return vtMag; }

        public double getDelta() { return delta; }

        public double getOmega() { return omega; }

        public int getOmegaPos() { return omega_Pos; }

        public int getDeltaPos() { return delta_Pos; }

        public double getEfd() { return efd; }

        // data sanity check per p.630 on PSS/E Volume II Program Application Guide
        public bool paraSanityTest()
        {
            sanityCheckStr = dataProcess.dataLimitCheck("T'd0", Td0p, 1, "", false, 10, "", true, "\n")
                             + dataProcess.dataLimitCheck("T''d0", Td0pp, 0, "", false, 0.2, "", true, "\n")
                             + dataProcess.dataLimitCheck("T'q0", Tq0p, 0.2, "", false, 1.5, "", true, "\n")
                             + dataProcess.dataLimitCheck("T''q0", Tq0pp, 0, "", true, 0.2, "", true, "\n")
                             + dataProcess.dataLimitCheck("    H", H, 1, "", false, 10, "", true, "\n")
                             + dataProcess.dataLimitCheck("D", D, 0, "", true, 3, "", true, "\n")
                             + dataProcess.dataLimitCheck("Xd", Xd, 0, "", false, 2.5, "", true, "\n")
                             + dataProcess.dataLimitCheck("Xq", Xq, 0, "", false, Xd, "[Xd]", false, "\n")
                             + dataProcess.dataLimitCheck("X'd", Xdp, Xdpp, "[X''d]", false, Xqp, "[X'q]", false, "\n")
                             + dataProcess.dataLimitCheck("X'q", Xqp, Xqpp, "[X''q]", false, Xq, "[Xq]", false, "\n")
                             + dataProcess.dataLimitCheck("X''d", Xdpp, Xl, "[Xl]", false, Xdp, "[X'd]", false, "\n")
                             + dataProcess.dataLimitCheck("S10", S10, 0, "", true, S12, "[S12]", true, "\n")
                              + dataProcess.dataLimitCheck("S12", S12, 0, "", true, 999, "", true, "\n");

            return sanityCheckStr.isEmpty();
        }

        public override string[] AsArrayForRow()
        {
            return new[]
            {
                $"{atBusNum}",
                $"{ID}",
                $"{_String.format("%1.4f", Td0p)}",
                $"{_String.format("%1.4f", Td0pp)}",
                $"{_String.format("%1.4f", Tq0p)}",
                $"{_String.format("%1.4f", Tq0pp)}",
                $"{_String.format("%1.4f", H / baseRatio)}",
                $"{_String.format("%1.4f", D / baseRatio)}",
                $"{_String.format("%1.4f", Xd * baseRatio)}",
                $"{_String.format("%1.4f", Xq * baseRatio)}",
                $"{_String.format("%1.4f", Xdp * baseRatio)}",
                $"{_String.format("%1.4f", Xqp * baseRatio)}",
                $"{_String.format("%1.4f", Xdpp * baseRatio)}",
                $"{_String.format("%1.4f", Xl * baseRatio)}",
                $"{_String.format("%1.4f", S10)}",
                $"{_String.format("%1.4f", S12)}",
            };
        }
        
        // export data for tabular showing 
        //public Object[] setTable()
        //{
        //    Object[] ret = new Object[tableColNum];
        //    ret[0] = atBusNum;
        //    ret[1] = ID;
        //    ret[2] = _String.format("%1.4f", Td0p);
        //    ret[3] = _String.format("%1.4f", Td0pp);
        //    ret[4] = _String.format("%1.4f", Tq0p);
        //    ret[5] = _String.format("%1.4f", Tq0pp);
        //    ret[6] = _String.format("%1.4f", H / baseRatio);
        //    ret[7] = _String.format("%1.4f", D / baseRatio);
        //    ret[8] = _String.format("%1.4f", Xd * baseRatio);
        //    ret[9] = _String.format("%1.4f", Xq * baseRatio);
        //    ret[10] = _String.format("%1.4f", Xdp * baseRatio);
        //    ret[11] = _String.format("%1.4f", Xqp * baseRatio);
        //    ret[12] = _String.format("%1.4f", Xdpp * baseRatio);
        //    ret[13] = _String.format("%1.4f", Xl * baseRatio);
        //    ret[14] = _String.format("%1.4f", S10);
        //    ret[15] = _String.format("%1.4f", S12);
        //    return ret;
        //}
    }
}
