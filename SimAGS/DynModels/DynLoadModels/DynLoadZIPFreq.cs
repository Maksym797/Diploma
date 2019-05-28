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
    public class DynLoadZIPFreq : dynLoadModel
    {
        // algebraic variables 
        public const int NUM_ALGE = 0; // algebraic variable number (will include in Bus Power equation)

        public int last_AlgeVar_Pos = 0;

        // state variables  
        public const int NUM_STATE = 0; // state variable number

        public int last_StateVar_Pos = 0; // starting position in the state vector

        // common element inherit from bus  	
        public bus mBus;

        public int busNum = 0; // bus number
        public int vtMag_Pos = 0;
        public int vtAng_Pos = 0;
        public int busFreq_Pos = 0;

        public double vtMag = 0.0; // load bus voltage magnitude 
        public double busFreqDev = 0.0; // frequency deviation in rad/s

        // store the converted value for << time-domain simulation >> 
        public double cConvPLoadP = 0.0;

        public double cConvPLoadQ = 0.0;
        public double cConvCLoadP = 0.0;
        public double cConvCLoadQ = 0.0;
        public double cConvYLoadP = 0.0;
        public double cConvYLoadQ = 0.0;

        public double realPLoad = 0.0; // calculated dynamic P injection
        public double realQLoad = 0.0; // calculated dynamic Q injection

        // for ZIP dynamic load 
        public bool bLoadConv = true; // convert to specified ZIP model

        public double PLoadP_Perc = 0.0;
        public double CLoadP_Perc = 0.0;
        public double YLoadP_Perc = 1.0;

        public double PLoadQ_Perc = 0.0;
        public double CLoadQ_Perc = 0.0;
        public double YLoadQ_Perc = 1.0;

        public bool bFreqEnable = false; // without freq impacts
        public double kfP = 0.0; // for MW
        public double kfQ = 0.0; // for Mvar

        public double tCurrent = 0.0; // current time instance 


        public DynLoadZIPFreq(bus busTemp, int numAlge, int numState)
        {
            mBus = busTemp;
            vtMag_Pos = mBus.vmagPos;
            vtAng_Pos = mBus.vangPos;
            busFreq_Pos = mBus.busFreq_Pos;
            mBus.dynLoad = this; // assign the bus dynamic load member
            mBus.bHasDynLoad = true;

            // algebraic variable index (template) 
            last_AlgeVar_Pos = numAlge + NUM_ALGE;

            // state variable index (template)
            last_StateVar_Pos = numState + NUM_STATE;
        }

        public void setLoadZIPComposite(bool bConv, double PConstZ, double PConstI, double PConstP, double QConstZ,
            double QConstI, double QConstP)
        {
            if ((bLoadConv = bConv) == true)
            {
                YLoadP_Perc = PConstZ / 100;
                CLoadP_Perc = PConstI / 100;
                PLoadP_Perc = PConstP / 100;

                YLoadQ_Perc = QConstZ / 100;
                CLoadQ_Perc = QConstI / 100;
                PLoadQ_Perc = QConstP / 100;
            }
        }


        public void setLoadFreqCompsite(bool bFreq, double kfP_Cof, double kfQ_Cof)
        {
            if ((bFreqEnable = bFreq) == true)
            {
                kfP = kfP_Cof;
                kfQ = kfQ_Cof;
            }
        }

        // initialize the element in x vector 
        public void ini(DoubleMatrix2D yVector, DoubleMatrix2D xVector)
        {
            //vtMag = yVector.getQuick(vtMag_Pos,0); 
            vtMag = mBus.volt;
            if (bLoadConv)
            {
                // convert all load to constant-impedance type 
                double pfPLoadTotal = mBus.aggCPLoadP + mBus.aggCCLoadP * vtMag + mBus.aggCYLoadP * vtMag * vtMag;
                double pfQLoadTotal = mBus.aggCPLoadQ + mBus.aggCCLoadQ * vtMag - mBus.aggCYLoadQ * vtMag * vtMag;
                cConvPLoadP = pfPLoadTotal * PLoadP_Perc;
                cConvPLoadQ = pfQLoadTotal * PLoadQ_Perc;
                cConvCLoadP = pfPLoadTotal * CLoadP_Perc / vtMag;
                cConvCLoadQ = pfQLoadTotal * CLoadQ_Perc / vtMag;
                cConvYLoadP = pfPLoadTotal * YLoadP_Perc / vtMag / vtMag;
                cConvYLoadQ = -pfQLoadTotal * YLoadQ_Perc / vtMag / vtMag; // + for capacitor; - for inductance
                CustomMessageHandler.println("Load at bus " + mBus.I + " is converted to [" +
                                             _String.format("%5.2f, %5.2f, %5.2f, %5.2f, %5.2f, %5.2f", YLoadP_Perc,
                                                 CLoadP_Perc, PLoadP_Perc, YLoadQ_Perc, CLoadQ_Perc, PLoadQ_Perc) +
                                             " ] model for simulation");

            }
            else
            {
                // retain the original component of load models 
                cConvPLoadP = mBus.aggCPLoadP;
                cConvPLoadQ = mBus.aggCPLoadQ;
                cConvCLoadP = mBus.aggCCLoadP;
                cConvCLoadQ = mBus.aggCCLoadQ;
                cConvYLoadP = mBus.aggCYLoadP;
                cConvYLoadQ = mBus.aggCYLoadQ;
            }

            realPLoad = (1 + kfP * busFreqDev) * (cConvPLoadP + cConvCLoadP * vtMag + cConvYLoadP * vtMag * vtMag);
            realQLoad = (1 + kfQ * busFreqDev) * (cConvPLoadQ + cConvCLoadQ * vtMag - cConvYLoadQ * vtMag * vtMag);
        }

        // update variables at the beginning of each iteration 
        public void update_Var(DoubleMatrix2D yVector, DoubleMatrix2D xVector, double t)
        {
            vtMag = yVector.getQuick(vtMag_Pos, 0);
            busFreqDev = bFreqEnable ? yVector.getQuick(busFreq_Pos, 0) : 0;
            realPLoad = (1 + kfP * busFreqDev) * (cConvPLoadP + cConvCLoadP * vtMag + cConvYLoadP * vtMag * vtMag);
            realQLoad = (1 + kfQ * busFreqDev) * (cConvPLoadQ + cConvCLoadQ * vtMag - cConvYLoadQ * vtMag * vtMag);
            tCurrent = t; // used to calculate time-depedent load 
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

            jacMat.setQuick(startRow + vtAng_Pos, startColumn + vtMag_Pos,
                jacMat.getQuick(startRow + vtAng_Pos, startColumn + vtMag_Pos) +
                (1 + kfP * busFreqDev) * (cConvCLoadP + 2 * vtMag * cConvYLoadP));
            jacMat.setQuick(startRow + vtMag_Pos, startColumn + vtMag_Pos,
                jacMat.getQuick(startRow + vtMag_Pos, startColumn + vtMag_Pos) +
                (1 + kfQ * busFreqDev) * (cConvCLoadQ - 2 * vtMag * cConvYLoadQ));

            if (bFreqEnable)
            {
                jacMat.setQuick(startRow + vtAng_Pos, startColumn + busFreq_Pos,
                    jacMat.getQuick(startRow + vtAng_Pos, startColumn + busFreq_Pos) +
                    kfP * (cConvPLoadP + vtMag * cConvCLoadP + vtMag * vtMag * cConvYLoadP));
                jacMat.setQuick(startRow + vtMag_Pos, startColumn + busFreq_Pos,
                    jacMat.getQuick(startRow + vtMag_Pos, startColumn + busFreq_Pos) +
                    kfQ * (cConvPLoadQ + vtMag * cConvCLoadQ - vtMag * vtMag * cConvYLoadQ));
            }
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

        public override string[] header { get; set; }
        public override string[] AsArrayForRow()
        {
            throw new NotImplementedException();
        }
    }
}