using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimAGS.Components.ExtendOption
{
    public class SwShuntRegOption
    {
        //public int regBusNum	= 0;					// regulating generator Bus 
        public Bus hostBus;                             // the Bus hosting this function 
        public int voltOptmVarIndx = 0;                 // index in the final optimum solution vector 

        public double swshuntBusBMax = 0.0;
        public double swshuntBusBMin = 0.0;

        public bool bYMatReBuild = false;
        
        // default constructor
        public SwShuntRegOption(Bus busTemp)
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
                optmInEquConConfMat[2 * voltOptmVarIndx, voltOptmVarIndx] 1;          // upper limit 
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
        public void updateRegVar(/*Optimisation.Result*/object result)
        {

            double oldSWShunt = hostBus.swshuntCalcB;
            double newSWShunt = oldSWShunt + result.get(voltOptmVarIndx).doubleValue();
            bYMatReBuild = false;

            // update the swshuntCalcB setting 
            if (newSWShunt > swshuntBusBMax)
            {
                newSWShunt = swshuntBusBMax;
                MessageBox.Show("---> SW Shunt at Bus " + hostBus.I + " hits high limit");
            }
            if (newSWShunt < swshuntBusBMin)
            {
                newSWShunt = swshuntBusBMin;
                MessageBox.Show("---> SW Shunt at Bus " + hostBus.I + " hits low limit");
            }
            hostBus.swshuntCalcB = newSWShunt;
            bYMatReBuild = true;
            MessageBox.Show("----> SWShunt at Bus " + hostBus.I + " changes from " + String.Format("%.2f", oldSWShunt) + " to " + String.Format("%.2f", newSWShunt));
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
