using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cern.colt.matrix;
using SimAGS.Components;
using SimAGS.Handlers;

namespace SimAGS.DynModels.DynLoadModels
{
    class DynLoadUDM : dynLoadModel
    {

        // default parameters position in input token
        public const int Default_ID = 2;
        public const int Default_T0 = 3;
        public const int Default_Td = 4;
        public const int Default_PINIT = 5;
        public const int Default_PFIN = 6;
        public const int Default_QINIT = 7;
        public const int Default_QFIN = 8;
        public const int Default_TAU = 9;

        // model dynamic parameter fields 
        public String ID = "";      // load ID 
        public double t0 = 0.5;         // model starts at 0.5 sec.
        public double td = 5;       // ramping up time 5 sec.
        public double Pinit = 0;        // initial MW injection when energized (MW)
        public double Pfin = 0;     // steady state MW injection (MW)
        public double Qinit = 0;        // initial MVar injection when energized (MVar)
        public double Qfin = 0;     // initial MVar injection when energized (MVar)
        public double tau = 0.5;        // MVar dynamic time constant (s) 

        // algebraic variables 
        public const int NUM_ALGE = 0;      // algebraic variable number (will include in Bus Power equation to reduce the dimension of system)
        public int last_AlgeVar_Pos = 0;

        // state variables  
        public const int NUM_STATE = 0;     // state variable number
        public int last_StateVar_Pos = 0;       // starting position in the state vector

        // common element inherit from bus  	
        public bus mBus;
        public int busNum = 0;                      // bus number
        public int vtMag_Pos = 0;
        public int vtAng_Pos = 0;

        public double vtMag = 0.0;                  // load bus voltage magnitude 

        // store the converted value for << time-domain simulation >> 
        public double realPLoad = 0.0;          // calculated dynamic P injection
        public double realQLoad = 0.0;          // calculated dynamic Q injection

        // global variables storing the current time instance
        public double tCurrent = 0.0;               // current time instance 

        // presenting data purpose 
        public override string[] header { get; set; } = {"Number", "ID", "t0", "td", "Pinit", "Pfin", "Qinit", "Qfin", "tau"};

        public static int tableColNum = 9;


        public DynLoadUDM(bus busTemp, String[] token, int numAlge, int numState)
        {
            mBus = busTemp;
            vtMag_Pos = mBus.vmagPos;
            vtAng_Pos = mBus.vangPos;

            // algebraic variable index (template) 
            last_AlgeVar_Pos = numAlge + NUM_ALGE;

            // state variable index (template)
            last_StateVar_Pos = numState + NUM_STATE;

            // dynamic parameters 
            ID = token[Default_ID];
            t0 = Double.Parse(token[Default_T0]);
            td = Double.Parse(token[Default_Td]);
            Pinit = Double.Parse(token[Default_PINIT]) / SBASE;
            Pfin = Double.Parse(token[Default_PFIN]) / SBASE;
            Qinit = Double.Parse(token[Default_QINIT]) / SBASE;
            Qfin = Double.Parse(token[Default_QFIN]) / SBASE;
            tau = Double.Parse(token[Default_TAU]);

            mBus.dynLoad = this;                // assign the bus dynamic load member
            mBus.bHasDynLoad = true;
            //mBus.bDynSimMon	 = true; 

            CustomMessageHandler.println("DynLoad Model at " + mBus.I + " ID = " + ID + " is loaded!");
        }

        // initialize the element in x vector 
        public void ini(DoubleMatrix2D yVector, DoubleMatrix2D xVector)
        {
            vtMag = mBus.volt;
            realPLoad = 0.0;
            realQLoad = 0.0;
        }

        // update variables at the beginning of each iteration 
        public void update_Var(DoubleMatrix2D yVector, DoubleMatrix2D xVector, double t)
        {
            vtMag = yVector.getQuick(vtMag_Pos, 0);
            tCurrent = t;   // used to calculate time-dependent load 

            // calculate time-dependent active and reactive power injection (pu)
            if (tCurrent < t0)
            {
                realPLoad = 0.0;
                realQLoad = 0.0;
            }
            else if (tCurrent >= t0 && tCurrent < (t0 + td))
            {
                realPLoad = Pinit - (Pinit - Pfin) / td * (tCurrent - t0);
                realQLoad = Qinit - (Qinit - Qfin) * (1 - Math.Exp(-(tCurrent - t0) / tau));
            }
            else if (tCurrent >= (t0 + td))
            {
                realPLoad = Pfin;
                realQLoad = Qinit - (Qinit - Qfin) * (1 - Math.Exp(-(tCurrent - t0) / tau));
            }
        }

        // calculate power injection 
        public void update_g(DoubleMatrix2D g)
        {
            g.setQuick(vtAng_Pos, 0, g.getQuick(vtAng_Pos, 0) - realPLoad);
            g.setQuick(vtMag_Pos, 0, g.getQuick(vtMag_Pos, 0) - realQLoad);
        }

        // calculate gy = dg/dy
        public void update_gy(DoubleMatrix2D jacMat, int startRow, int startColumn)
        {

        }


        // calculate gx = dg/dx
        public void update_gx(DoubleMatrix2D jacMat, int startRow, int startColumn)
        {

        }

        // calculate f(x,y) 
        public void update_f(DoubleMatrix2D fVector)
        {

        }

        // calculate fx = df/dx 
        public void update_fx(DoubleMatrix2D jacMat, int startRow, int startColumn, double simTheta, double h)
        {

        }

        // calculate fy = df/dy
        public void update_fy(DoubleMatrix2D jacMat, int startRow, int startColumn, double simTheta, double h)
        {
        }

        public double getDynPInj()
        {
            return realPLoad;
        }

        public double getDynQInj()
        {
            return realQLoad;
        }

        // export data for tabular showing 
        //public Object[] setTable()
        //{
        //    Object[] ret = new Object[tableColNum];
        //    ret[0] = mBus.I;
        //    ret[1] = ID;
        //    ret[2] = String.format("%4.2f", t0);
        //    ret[3] = String.format("%4.2f", td);
        //    ret[4] = String.format("%4.2f", Pinit * SBASE);
        //    ret[5] = String.format("%4.2f", Pfin * SBASE);
        //    ret[6] = String.format("%4.2f", Qinit * SBASE);
        //    ret[7] = String.format("%4.2f", Qfin * SBASE);
        //    ret[8] = String.format("%4.2f", tau);
        //    return ret;
        //}

        public override string[] AsArrayForRow()
        {
            return new[]
            {
                mBus.I.ToString(),
                ID,
                _String.format("%4.2f", t0),
                _String.format("%4.2f", td),
                _String.format("%4.2f", Pinit * SBASE),
                _String.format("%4.2f", Pfin * SBASE),
                _String.format("%4.2f", Qinit * SBASE),
                _String.format("%4.2f", Qfin * SBASE),
                _String.format("%4.2f", tau),
            };
        }


    }
}
