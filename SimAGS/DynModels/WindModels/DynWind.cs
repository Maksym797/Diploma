using System;
using System.Windows.Forms;
using SimAGS.Components;
using SimAGS.Handlers;

namespace SimAGS.DynModels.WindModels
{
    public class DynWind
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
        public static String[] header = { "Number", "TR", "INI_MW" };   // wind farm bus 
        public static int tableColNum = 3;


        public DynWind(bus busTemp, String[] token, int numAlge, int numState)
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
        public void ini(double[,] yVector, double[,] xVector)
        {
            SV1 = setMWInj;
            // update the state variable
            xVector[SV1_Pos, 0] = SV1;
        }

        // update variables at the beginning of each iteration 
        public void update_Var(double[,] yVector, double[,] xVector)
        {
            SV1 = xVector[SV1_Pos, 0];
        }

        // calculate power injection 
        public void update_g(double[,] g)
        {
            g[vtAng_Pos, 0] = g[vtAng_Pos, 0] + SV1;
        }

        // calculate gy = dg/dy
        public void update_gy(double[,] jacMat, int startRow, int startColumn)
        {

        }

        // calculate gx = dg/dx
        public void update_gx(double[,] jacMat, int startRow, int startColumn)
        {
            jacMat[startRow + vtAng_Pos, startColumn + SV1_Pos] = -1;
        }

        // calculate f(x,y) 
        public void update_f(double[,] fVector)
        {
            fVector[SV1_Pos, 0] = 1 / TR * (setMWInj - SV1);
        }

        // calculate fy = df/dy
        public void update_fy(double[,] jacMat, int startRow, int startColumn, double simTheta, double h)
        {
        }

        // calculate fx = df/dx 
        public void update_fx(double[,] jacMat, int startRow, int startColumn, double simTheta, double h)
        {
            double tStepCof = -simTheta * h;
            jacMat[SV1_Pos + startRow, SV1_Pos + startColumn] = 1 / TR * (-1) * tStepCof;
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
            ret[2] = String.Format("%1.2f", INIWINDMW * 100);
            CustomMessageHandler.Show("--->" + ret[1]);
            return ret;
        }
    }
}
