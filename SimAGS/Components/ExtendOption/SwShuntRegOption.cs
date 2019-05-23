using System;
using System.Windows.Forms;
using org.ojalgo.optimisation;
using SimAGS.Handlers;

namespace SimAGS.Components.ExtendOption
{
    public class SwShuntRegOption : BaseExtOption
    {
        //public int regBusNum	= 0;					// regulating generator bus 
        public bus hostBus;                             // the bus hosting this function 
        public int voltOptmVarIndx = 0;                 // index in the final optimum solution vector 

        public double swshuntBusBMax = 0.0;
        public double swshuntBusBMin = 0.0;

        public bool bYMatReBuild = false;
        
        // default constructor
        public SwShuntRegOption(bus busTemp)
        {
            this.hostBus = busTemp;
            this.swshuntBusBMax = busTemp.aggSWshuntBusBMax;
            this.swshuntBusBMin = busTemp.aggSWshuntBusBMin;
        }

        // build the coefficient matrix on the right-hand side of LLMat*delta_V = LG_gen*delta_Vg + LG_sw*delta_B + LG_trans*delta_k  
        public void buildLGMatWithRegCtr(double[,] LGMat)
        {
            LGMat[hostBus.LLIndx, voltOptmVarIndx] = 1;
        }


        // build the inequality constraints coefficient matrix 
        public void buildInequConCofMat(bool isToUpdate, double[,] optmInEquConConfMat)
        {
            if (isToUpdate)
            {
                optmInEquConConfMat[2 * voltOptmVarIndx, voltOptmVarIndx] = 1;          // upper limit 
                optmInEquConConfMat[2 * voltOptmVarIndx + 1, voltOptmVarIndx]= -1;             // lower limit 
            }
        }

        // build the right-hand side of the inequality constraints 
        public void buildInequbMat(double[,] optmInEqubMat)
        {

            optmInEqubMat[2 * voltOptmVarIndx, 0] = swshuntBusBMax - hostBus.swshuntCalcB;      // upper limit 
            optmInEqubMat[2 * voltOptmVarIndx + 1, 0] = -(swshuntBusBMin - hostBus.swshuntCalcB);   // lower limit 

        }

        // update regulating variables after each iteration 
        public void updateRegVar(Optimisation.Result result)
        {

            double oldSWShunt = hostBus.swshuntCalcB;
            double newSWShunt = oldSWShunt + result.get(voltOptmVarIndx).doubleValue();
            bYMatReBuild = false;

            // update the swshuntCalcB setting 
            if (newSWShunt > swshuntBusBMax)
            {
                newSWShunt = swshuntBusBMax;
                CustomMessageHandler.Show("---> SW Shunt at bus " + hostBus.I + " hits high limit");
            }
            if (newSWShunt < swshuntBusBMin)
            {
                newSWShunt = swshuntBusBMin;
                CustomMessageHandler.Show("---> SW Shunt at bus " + hostBus.I + " hits low limit");
            }
            hostBus.swshuntCalcB = newSWShunt;
            bYMatReBuild = true;
            CustomMessageHandler.Show("----> SWShunt at bus " + hostBus.I + " changes from " + String.Format("%.2f", oldSWShunt) + " to " + String.Format("%.2f", newSWShunt));
        }

        // set voltOptmVarIndx by pfVoltageHelper
        public void setVoltOptmVarIndx(int setVal)
        {
            voltOptmVarIndx = setVal;
        }


        // get voltOptmVarIndx 
        public int getVoltOptmVarIndx()
        {
            return voltOptmVarIndx;
        }

        // return if needs to update yMat
        public bool getIsUpdateYMat()
        {
            return bYMatReBuild;
        }


    }
}
