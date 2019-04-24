﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using org.ojalgo.optimisation;

namespace SimAGS.Components.ExtendOption
{
    class tapRegOption : BaseExtOption
    {

        public int voltOptmVarIndx = 0;                 // index in the final optimum solution vector 

        public twoWindTrans hostTrans;                  // the branches hosting this function 
        public bool bYMatReBuild = false;
        //public int frBusLLIndx = 0; 
        //public int toBusLLIndx = 0;	

        // default constructor
        public tapRegOption(twoWindTrans transTemp)
        {
            this.hostTrans = transTemp;
            this.voltOptmVarIndx = transTemp.voltOptmVarIndx;
        }


        // build the coefficient matrix on the right-hand side of LLMat*delta_V = LG_gen*delta_Vg + LG_sw*delta_B + LG_trans*delta_k  
        public void buildLGMatWithRegCtr(double[,] LGMat)
        {
            double dfrBusQ_dk = -hostTrans.dcalcBII_dk - hostTrans.dcalcBIJ_dk;
            double dtoBusQ_dk = -hostTrans.dcalcBJJ_dk - hostTrans.dcalcBJI_dk;

            LGMat.setQuick(hostTrans.fromLoadBusLLIndx, voltOptmVarIndx, -dfrBusQ_dk);          // from bus 
            LGMat.setQuick(hostTrans.toLoadbusLLIndx, voltOptmVarIndx, -dtoBusQ_dk);            // to bus 
        }


        // build the inequality constraints coefficient matrix 
        public void buildInequConCofMat(double[,] optmInEquConConfMat)
        {
            optmInEquConConfMat.setQuick(2 * voltOptmVarIndx, voltOptmVarIndx, 1);          // upper limit 
            optmInEquConConfMat.setQuick(2 * voltOptmVarIndx + 1, voltOptmVarIndx, -1);             // lower limit 
        }


        // build the right-hand side of the inequality constraints 
        public void buildInequbMat(double[,] optmInEqubMat)
        {
            optmInEqubMat.setQuick(2 * voltOptmVarIndx, 0, hostTrans.RMA1 - hostTrans.kRatio);  // upper limit 
            optmInEqubMat.setQuick(2 * voltOptmVarIndx + 1, 0, -(hostTrans.RMI1 - hostTrans.kRatio));   // lower limit 
        }

        // update regulating variables after each iteration 
        public void updateRegVar(Optimisation.Result result)
        {

            double oldKRatio = hostTrans.kRatio;
            double newKRatio = oldKRatio + result.get(voltOptmVarIndx).doubleValue();

            hostTrans.kRatio = newKRatio;
            hostTrans.calcTWPara();
            bYMatReBuild = true;
            MessageBox.Show("----> Trans at (" + hostTrans.I + "," + hostTrans.J + ") tap changes from " + String.Format("%.4f", oldKRatio) + " to " + String.Format("%.4f", newKRatio));
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
