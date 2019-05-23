using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimAGS.Components;
using SimAGS.Handlers;

namespace SimAGS.DynModels.MonModels
{
    public class BusFreq
    {
        public const double w0 = 2 * 60 * Math.PI;          // default value 				



        public const int NUM_ALGE = 1;  // algebraic variable number
        public double busFreq = 0.0;    // bus frequency in pu 

        public int busFreq_Pos = 0;
        public int last_AlgeVar_Pos = 0;

        //state variable number [no state varaibles needed]
        public const int NUM_STATE = 0;
        public int last_StateVar_Pos = 0;

        //inter-mediate variables 
        public double vtAng_n = 0.0;                    // bus phase angle value at n-1 time instant 
        public double busFreq_n = 0.0;                  // bus frequency deviation at n-1 time instant
        public double hStep = 0.0;
        public double vtAng = 0.0;                  // measured phase angle 
        public int vtAng_Pos = 0;                   // bus voltage angle index in the network variable vector

        public bus mBus;                                // measured bus for data exchange purpose 


        // constructor calculate variables after power flow 
        public BusFreq(bus busTemp, int numAlge, int numState)
        {
            mBus = busTemp;
            vtAng_Pos = busTemp.vangPos;

            // algebraic variable index 
            busFreq_Pos = busFreq_Pos + numAlge;
            last_AlgeVar_Pos = numAlge + NUM_ALGE;

            // state variable index  
            last_StateVar_Pos = numState + NUM_STATE;

            // update bus status 
            busTemp.bHasFreqMeasure = true;
            busTemp.busFreqCalc = this;
            busTemp.busFreq_Pos = busFreq_Pos;

            CustomMessageHandler.Show("Freq Measure Model at " + busTemp.I + " is loaded!");
        }


        // initialize the element in x vector 
        public void ini(DoubleMatrix2D yVector, DoubleMatrix2D xVector)
        {
            vtAng_n = mBus.ang;
            vtAng = vtAng_n;

            // initialize bus frequency 
            busFreq = 0;
            busFreq_n = busFreq;
            yVector[busFreq_Pos, 0] = busFreq;
        }


        // update variables at the beginning of each iteration 
        public void update_Var(DoubleMatrix2D yVector, DoubleMatrix2D xVector)
        {
            busFreq = yVector[busFreq_Pos, 0];
            vtAng = yVector[vtAng_Pos, 0];
            mBus.busFreq = busFreq;
        }

        // update -g(x,y)
        public void update_g(DoubleMatrix2D g)
        {
            g[busFreq_Pos, 0] = -(vtAng - vtAng_n - w0 * hStep * busFreq);
        }

        //update -g(x,y) at occurrence of event
        public void update_gT0(DoubleMatrix2D g)
        {
            g[busFreq_Pos, 0] = -(busFreq - busFreq_n);
        }


        // calculate gy = dg/dy element of dg(x,y)/dy
        public void update_gy(DoubleMatrix2D jacMat, int startRow, int startColumn)
        {
            jacMat[busFreq_Pos + startRow, busFreq_Pos + startColumn] = -w0 * hStep;
            jacMat[busFreq_Pos + startRow, vtAng_Pos + startColumn] = 1;
        }

        // calculate gy at T0 to avoid sudden change in frequency 
        public void update_gyT0(DoubleMatrix2D jacMat, int startRow, int startColumn)
        {
            //jacMat.setQuick(busFreq_Pos + startRow, busFreq_Pos + startColumn, -w0*hStep);
            jacMat[busFreq_Pos + startRow, busFreq_Pos + startColumn] = 1;
        }

        // calculate gx = dg/dx 
        public void update_gx(DoubleMatrix2D jacMat, int startRow, int startColumn)
        {

        }

        // calculate dx/dt = f = f(x,y)
        public void update_f(DoubleMatrix2D f)
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


        public void update_BusFreqMeasurement(double h, DoubleMatrix2D yVector)
        {
            hStep = h;
            vtAng_n = yVector[vtAng_Pos, 0];
            busFreq_n = yVector[busFreq_Pos, 0];
            //CustomMessageHandler.Show("hStep = " + hStep + " Freq_n = " + busFreq_n); 
        }
    }
}
