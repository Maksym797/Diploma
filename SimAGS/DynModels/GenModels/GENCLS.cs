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
    public class GENCLS : genModel
    {
        public const double w0 = 2 * 60 * Math.PI;

        // default parameters position in input token 
        public const int Default_H = 3;
        public const int Default_D = 4;

        // model dynamic parameter fields 
        public double xl = 0.0;
        public double ra = 0.0;     // ra 
        public double Xdp = 0.0;        // X'd 
        public double H = 0.0;      // inertia 
        public double D = 0.0;      // damping ratio 

        // algebraic variables 
        public const int NUM_ALGE = 6;  // algebraic variable number
        public double iq = 0.0;         // current at q axis 	
        public double id = 0.0;         // current at d axis
        public double pe = 0.0;         // generator MW injection
        public double qe = 0.0;         // generator MVar injection 
        public double pm = 0.0;         // mechanic power (can be changed by governor model if exists), rev 07-28-2012
        public double efd = 0.0;            // internal voltage behind Xdp

        public int iq_Pos = 0;
        public int id_Pos = 1;
        public int pe_Pos = 2;
        public int qe_Pos = 3;
        public int pm_Pos = 4;
        public int efd_Pos = 5;
        public int last_AlgeVar_Pos = 0;

        // state variables  
        public const int NUM_STATE = 2;     // state variable number
        public double omega = 0.0;          // generator rotor speed in radians/sec 
        public double delta = 0.0;          // generator rotor angle 

        public int omega_Pos = 0;
        public int delta_Pos = 1;
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

        // calculated algebraic variables 
        public double vq = 0.0;             // voltage at q axis
        public double vd = 0.0;             // voltage at d axis 
        public double cosA = 0.0;           // cos(delta - ang)
        public double sinA = 0.0;           // sin(delta - ang)

        // setting values interface with other devices 
        public double pm0 = 0.0;            // pm setting value 
        public double efd0 = 0.0;           // exciting field voltage setting value

        // sanity check warning string
        public String sanityCheckStr = "";


        // presenting data purpose 
        public override String[] header {get; set;} = { "Number", "ID", "H", "D" };
        public static int tableColNum = 4;

        // constructor calculate variables after power flow 
        public GENCLS(gen genTemp, String[] token, int numAlge, int numState)
        {
            // data loaded from power flow case
            atGen = genTemp;
            atBusNum = genTemp.I;
            ID = genTemp.ID;
            ra = genTemp.ZR;
            Xdp = genTemp.ZX;
            MBase = genTemp.MBASE;

            // algebraic variable index 
            iq_Pos = iq_Pos + numAlge;
            id_Pos = id_Pos + numAlge;
            pe_Pos = pe_Pos + numAlge;
            qe_Pos = qe_Pos + numAlge;
            pm_Pos = pm_Pos + numAlge;
            efd_Pos = efd_Pos + numAlge;
            last_AlgeVar_Pos = numAlge + NUM_ALGE;

            // state variable index 
            omega_Pos = omega_Pos + numState;
            delta_Pos = delta_Pos + numState;
            last_StateVar_Pos = numState + NUM_STATE;

            // dynamic parameters 
            H = Double.Parse(token[Default_H]);
            D = Double.Parse(token[Default_D]);

            // SanityTest 
            if (!paraSanityTest()) CustomMessageHandler.println(_String.format("Warning: GENCLS at [%3d, %2s]\n", atBusNum, ID) + sanityCheckStr);
            //if (!paraSanityTest()) CustomMessageHandler.print(_String.format("Warning: GENCLS at [%3d, %2s]\n", atBusNum, ID) + sanityCheckStr);

            // convert to system base
            H = H * MBase / SBASE;
            D = D * MBase / SBASE;

            // update generator dynamic model status 
            genTemp.omega_Pos = omega_Pos;      // rotor speed shared with other devices 
            genTemp.pmech_Pos = pm_Pos;         // mechanical torque shared with other devices
            genTemp.efd_Pos = efd_Pos;
            genTemp.genDyn = this;
            genTemp.hasGenModel = true;

            CustomMessageHandler.println("GENCLS Model at " + atBusNum + " ID = " + ID + " is loaded!");
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


            double pfAng = Math.Atan(qe / pe);                                      // power factor (pf) angle 
            double iMag = Math.Sqrt(pe * pe + qe * qe) / vtMag;                         // current magnitude 
            double vx = vtMag * Math.Cos(vtAng);
            double vy = vtMag * Math.Sin(vtAng);
            double ix = iMag * Math.Cos(vtAng - pfAng);
            double iy = iMag * Math.Sin(vtAng - pfAng);

            double eqp_x = ra * ix - Xdp * iy + vx;
            double eqp_y = ra * iy + Xdp * ix + vy;
            efd = Math.Sqrt(eqp_x * eqp_x + eqp_y * eqp_y);                       // initial voltage behind xd'
            delta = Math.Atan(eqp_y / eqp_x);                                       // initial rotor angle
            omega = 1.0;                                                            // initial rotor speed 
            pm = pe + iMag * iMag * ra;
            vq = vtMag * Math.Cos(delta - vtAng);
            vd = vtMag * Math.Sin(delta - vtAng);
            iq = iMag * Math.Cos(delta - vtAng + pfAng);
            id = iMag * Math.Sin(delta - vtAng + pfAng);
            pm0 = pm;
            efd0 = efd;

            // update the dynamic state variable index 
            yVector.setQuick(iq_Pos, 0, iq);
            yVector.setQuick(id_Pos, 0, id);
            yVector.setQuick(pe_Pos, 0, pe);
            yVector.setQuick(qe_Pos, 0, qe);
            yVector.setQuick(pm_Pos, 0, pm);
            yVector.setQuick(efd_Pos, 0, efd);

            // initialize state variables  
            xVector.setQuick(omega_Pos, 0, omega);
            xVector.setQuick(delta_Pos, 0, delta);
        }


        // update variables at the beginning of each iteration 
        public void update_Var(DoubleMatrix2D yVector, DoubleMatrix2D xVector)
        {
            if (atGen.InService)
            {
                // update common algebraic variables 
                vtMag = yVector.getQuick(vtMag_Pos, 0);
                vtAng = yVector.getQuick(vtAng_Pos, 0);

                // update specific algebraic variables 
                iq = yVector.getQuick(iq_Pos, 0);
                id = yVector.getQuick(id_Pos, 0);
                pe = yVector.getQuick(pe_Pos, 0);
                qe = yVector.getQuick(qe_Pos, 0);
                pm = yVector.getQuick(pm_Pos, 0);
                efd = yVector.getQuick(efd_Pos, 0);

                // update state variables 
                omega = xVector.getQuick(omega_Pos, 0);
                delta = xVector.getQuick(delta_Pos, 0);

                // update intermediate variables
                cosA = Math.Cos(delta - vtAng);
                sinA = Math.Sin(delta - vtAng);
                vq = vtMag * cosA;
                vd = vtMag * sinA;

                //if (atGen.hasGovModel) pm = yVector.getQuick(atGen.pmech_Pos, 0);
            }
        }


        // calculate power injection 
        public void update_g(DoubleMatrix2D g)
        {
            if (atGen.InService)
            {
                g.setQuick(vtAng_Pos, 0, g.getQuick(vtAng_Pos, 0) + pe);    // update network MW injection [in case multiple generators] 	PGenInj - PNetwowrk Out Node - PLoad Out = 0 
                g.setQuick(vtMag_Pos, 0, g.getQuick(vtMag_Pos, 0) + qe);        // update network MVar injection balance equation    			QGenInj - QNetwowrk Out Node - QLoad Out = 0 

                // set the other variables 
                g.setQuick(iq_Pos, 0, -(efd - ra * iq - Xdp * id - vq));
                g.setQuick(id_Pos, 0, -(0 + Xdp * iq - ra * id - vd));
                g.setQuick(pe_Pos, 0, -(pe - vq * iq - vd * id));
                g.setQuick(qe_Pos, 0, -(qe - vq * id + vd * iq));
                g.setQuick(pm_Pos, 0, -(pm0 - pm));                         // for simulation
                g.setQuick(efd_Pos, 0, -(efd0 - efd));

                //CustomMessageHandler.println("agle equ at gen after " + atBusNum + "\n" + g.getQuick(vtAng_Pos,0) + "\n" + g.getQuick(vtMag_Pos,0));
                //CustomMessageHandler.println("agle equ at gen " + atBusNum + "\n" + g.getQuick(iq_Pos,0) + "\n" + g.getQuick(id_Pos,0) + "\n" + g.getQuick(pe_Pos,0) + "\n" + g.getQuick(qe_Pos,0) +  "\n" + g.getQuick(pm_Pos,0) + "\n" + g.getQuick(vf_Pos,0));
            }
        }


        // calculate gy = dg/dy
        public void update_gy(DoubleMatrix2D jacMat, int startRow, int startColumn)
        {
            if (atGen.InService)
            {
                jacMat.setQuick(vtAng_Pos + startRow, pe_Pos + startColumn, -1);
                jacMat.setQuick(vtMag_Pos + startRow, qe_Pos + startColumn, -1);

                jacMat.setQuick(iq_Pos + startRow, iq_Pos + startColumn, -ra);
                jacMat.setQuick(iq_Pos + startRow, id_Pos + startColumn, -Xdp);
                jacMat.setQuick(iq_Pos + startRow, vtMag_Pos + startColumn, -cosA);
                jacMat.setQuick(iq_Pos + startRow, vtAng_Pos + startColumn, -vd);   // vq = vtMag*cos(delta-Ang)
                jacMat.setQuick(iq_Pos + startRow, efd_Pos + startColumn, 1);

                jacMat.setQuick(id_Pos + startRow, iq_Pos + startColumn, Xdp);
                jacMat.setQuick(id_Pos + startRow, id_Pos + startColumn, -ra);
                jacMat.setQuick(id_Pos + startRow, vtMag_Pos + startColumn, -sinA); // vd = vtMag*sin(delta-Ang)
                jacMat.setQuick(id_Pos + startRow, vtAng_Pos + startColumn, vq);

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

                jacMat.setQuick(pm_Pos + startRow, pm_Pos + startColumn, -1);   // for simulation -g(*) = - (pm0-pm) = 0 
                jacMat.setQuick(efd_Pos + startRow, efd_Pos + startColumn, -1);
            }
            else
            {
                jacMat.setQuick(iq_Pos + startRow, iq_Pos + startColumn, 1);
                jacMat.setQuick(id_Pos + startRow, id_Pos + startColumn, 1);
                jacMat.setQuick(pe_Pos + startRow, pe_Pos + startColumn, 1);
                jacMat.setQuick(qe_Pos + startRow, qe_Pos + startColumn, 1);
                jacMat.setQuick(pm_Pos + startRow, pm_Pos + startColumn, 1);
                jacMat.setQuick(efd_Pos + startRow, efd_Pos + startColumn, 1);
            }
        }

        // calculate gx = dg/dx
        public void update_gx(DoubleMatrix2D jacMat, int startRow, int startColumn)
        {
            if (atGen.InService)
            {
                jacMat.setQuick(iq_Pos + startRow, delta_Pos + startColumn, vd);
                jacMat.setQuick(id_Pos + startRow, delta_Pos + startColumn, -vq);
                jacMat.setQuick(pe_Pos + startRow, delta_Pos + startColumn, -qe);
                jacMat.setQuick(qe_Pos + startRow, delta_Pos + startColumn, pe);
            }
        }



        // calculate dx/dt = f = f(x,y)
        public void update_f(DoubleMatrix2D f)
        {
            if (atGen.InService)
            {
                f.setQuick(omega_Pos, 0, 0.5 / H * (pm - pe - (id * id + iq * iq) * ra - (omega - 1) * D));
                f.setQuick(delta_Pos, 0, w0 * (omega - 1));
                //CustomMessageHandler.println("dif equ at gen " + atBusNum + " " + f.getQuick(omega_Pos,0) + " " + f.getQuick(delta_Pos,0));
            }
        }



        // calculate fx = df/dx 
        public void update_fx(DoubleMatrix2D jacMat, int startRow, int startColumn, double simTheta, double h)
        {
            double tStepCof = -simTheta * h;
            if (atGen.InService)
            {
                jacMat.setQuick(omega_Pos + startRow, omega_Pos + startColumn, (-0.5 / H * D) * tStepCof);
                jacMat.setQuick(delta_Pos + startRow, omega_Pos + startColumn, w0 * tStepCof);
            }
            else
            {
                jacMat.setQuick(omega_Pos + startRow, omega_Pos + startColumn, 1 * tStepCof);
                jacMat.setQuick(delta_Pos + startRow, delta_Pos + startColumn, 1 * tStepCof);
            }
        }

        // calculate fy = df/dy
        public void update_fy(DoubleMatrix2D jacMat, int startRow, int startColumn, double simTheta, double h)
        {
            if (atGen.InService)
            {
                double tStepCof = -simTheta * h;
                jacMat.setQuick(omega_Pos + startRow, pe_Pos + startColumn, (-0.5 / H) * tStepCof);
                jacMat.setQuick(omega_Pos + startRow, iq_Pos + startColumn, (-0.5 / H * 2 * iq * ra) * tStepCof);
                jacMat.setQuick(omega_Pos + startRow, id_Pos + startColumn, (-0.5 / H * 2 * id * ra) * tStepCof);
                jacMat.setQuick(omega_Pos + startRow, pm_Pos + startColumn, (0.5 / H) * tStepCof);
            }
        }

        // data change interface 
        public void setPmRef(double pmSetVal) { pm0 = pmSetVal; }

        public void zeroPm0() { pm0 = 0; }          // reset pm0 at generator so that output from governor can be loaded

        public double getPm() { return pm; }

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
            sanityCheckStr = dataProcess.dataLimitCheck("H", H, 1, "", false, 10, "", true, "\n")
                             + dataProcess.dataLimitCheck("D", D, 0, "", true, 3, "", true, "\n");
            return sanityCheckStr.isEmpty();
        }

        public override string[] AsArrayForRow()
        {
            return new[]
            {
                $"{atBusNum}",
                $"{ID}",
                $"{_String.format("%1.2f", H * SBASE / MBase)}",
                $"{_String.format("%1.2f", D * SBASE / MBase)}"
            };
        }

        // export data for tabular showing 
        //public Object[] setTable()
        //{
        //    Object[] ret = new Object[tableColNum];
        //    ret[0] = atBusNum;
        //    ret[1] = ID;
        //    ret[2] = _String.format("%1.2f", H * SBASE / MBase);
        //    ret[3] = _String.format("%1.2f", D * SBASE / MBase);
        //    return ret;
        //}
    }
}
