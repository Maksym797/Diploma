using System;
using System.Collections.Generic;
using System.Windows.Forms;
using cern.colt.matrix;
using SimAGS.Components;
using SimAGS.Handlers;

namespace SimAGS.DynModels.WindModels
{
    public class DYNWIND : abstractPfElement
    {
        // default parameters position in input token 
        public const int Default_TR = 1;
        public const int Default_INIWINDMW = 2;

        // model dynamic parameter fields 
        public double TR = 0.0;             // time constant
        public double INIWINDMW = 0.0;          // initial value 

        // algebraic variables 
        public const int NUM_ALGE = 0;      // algebraic variable number
        public int last_AlgeVar_Pos = 0;

        // state variables  
        public const int NUM_STATE = 1;         // state variable number
        public double SV1 = 0.0;                // state variable 

        public int SV1_Pos = 0;             // state variable index 
        public int last_StateVar_Pos = 0;       // starting position in the state vector

        // intermediate variable 
        public double setMWInj = 0.0;       // setting value from wind farm injection files 

        // common element inherit from bus  	
        public bus mBus;
        public int busNum = 0;                  // bus number
        public int vtAng_Pos = 0;               // bus angle index (MW balance equation index) 

        // presenting data purpose 
        public override String[] header {get; set;} = { "Number", "TR", "INI_MW" };   // wind farm bus 

        public static int tableColNum = 3;


        public DYNWIND(bus busTemp, String[] token, int numAlge, int numState)
        {
            mBus = busTemp;
            busNum = mBus.I;
            vtAng_Pos = mBus.vangPos;

            // algebraic variable index 
            last_AlgeVar_Pos = numAlge + NUM_ALGE;

            // state variable index 
            SV1_Pos = SV1_Pos + numState;
            last_StateVar_Pos = numState + NUM_STATE;

            // update wind dynamic model status 
            TR = Double.Parse(token[Default_TR]);
            INIWINDMW = Double.Parse(token[Default_INIWINDMW]) / 100;

            setMWInj = INIWINDMW;
            mBus.bWindMWEnable = true;
            mBus.windModel = this;
            mBus.windMWInj = setMWInj;      // load the initial value 
        }

        // initialize the element in yVector and xVector 
        public void ini(DoubleMatrix2D yVector, DoubleMatrix2D xVector)
        {
            SV1 = setMWInj;
            // update the state variable
            xVector.setQuick(SV1_Pos, 0, SV1);
        }

        // update variables at the beginning of each iteration 
        public void update_Var(DoubleMatrix2D yVector, DoubleMatrix2D xVector)
        {
            SV1 = xVector.getQuick(SV1_Pos, 0);
        }

        // calculate power injection 
        public void update_g(DoubleMatrix2D g)
        {
            g.setQuick(vtAng_Pos, 0, g.getQuick(vtAng_Pos, 0) + SV1);
        }

        // calculate gy = dg/dy
        public void update_gy(DoubleMatrix2D jacMat, int startRow, int startColumn)
        {

        }

        // calculate gx = dg/dx
        public void update_gx(DoubleMatrix2D jacMat, int startRow, int startColumn)
        {
            jacMat.setQuick(startRow + vtAng_Pos, startColumn + SV1_Pos, -1);
        }

        // calculate f(x,y) 
        public void update_f(DoubleMatrix2D fVector)
        {
            fVector.setQuick(SV1_Pos, 0, 1 / TR * (setMWInj - SV1));
        }

        // calculate fy = df/dy
        public void update_fy(DoubleMatrix2D jacMat, int startRow, int startColumn, double simTheta, double h)
        {
        }

        // calculate fx = df/dx 
        public void update_fx(DoubleMatrix2D jacMat, int startRow, int startColumn, double simTheta, double h)
        {
            double tStepCof = -simTheta * h;
            jacMat.setQuick(SV1_Pos + startRow, SV1_Pos + startColumn, 1 / TR * (-1) * tStepCof);
        }

        // scale load for eventList
        public void schdMW(double setVal)
        {
            setMWInj = setVal;
        }

        //the real wind power injection
        public double getCurrentMW()
        {
            return SV1;
        }


        // export data for tabular showing 
        public Object[] setTable()
        {
            Object[] ret = new Object[tableColNum];
            ret[0] = busNum;
            ret[1] = TR;
            ret[2] = _String.format("%1.2f", INIWINDMW * 100);
            CustomMessageHandler.Show("--->" + ret[1]);
            return ret;
        }
        public override string[] AsArrayForRow()
        {
            var ret = new List<string> { };
            ret[0] = busNum.ToString();
            ret[1] = TR.ToString();
            ret[2] = _String.format("%1.2f", INIWINDMW * 100);
            CustomMessageHandler.Show("--->" + ret[1]);
            return ret.ToArray();
        }

    }
}
