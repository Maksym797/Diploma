using cern.colt.matrix;
using SimAGS.SimUtil;
using System.Collections.Generic;
using java.lang;
using org.ojalgo.optimisation;
using SimAGS.Handlers;

namespace SimAGS.Components.ExtendOption
{
    public class genRegOption : BaseExtOption
    {
        // self element 
        public double VQSens = 0.0;                     // terminal voltage w.s.t q injection at generator bus 
        public double estLocVSetMax = 0.0;              // estimated local voltage setting (maximum) = (Qmax - Qgen)/sens + Vset
        public double estLocVSetMin = 0.0;              // estimated local voltage setting (minimum) = (Qmin - Qgen)/sens + Vset;
        public int voltOptmVarIndx = 0;                 // index in the final optimum solution vector 

        // inherent variables 
        public bus hostBus;                             // the bus hosting this function 
        public yMatrix yMat;

        public List<bus> loadBusArrayList = new List<bus>();

        // default constructor
        public genRegOption(bus busTemp)
        {
            this.hostBus = busTemp;
        }

        public void setYMat(yMatrix yMat)
        {
            this.yMat = yMat;
        }

        public void setLoadBusList(List<bus> list)
        {
            loadBusArrayList = list;
        }

        // public void calcVQSens(PQDecoupleJacob decoupleJacMat)
        // {
        //     // calculate the self-QV sensitivity 
        //     VQSens = decoupleJacMat.GGMat.getQuick(hostBus.GGIndx, hostBus.GGIndx)
        //             - matrixOpt.mult(decoupleJacMat.GLMat.viewRow(hostBus.GGIndx), matrixOpt.mult(decoupleJacMat.LLInvMat, decoupleJacMat.LGMat.viewColumn(hostBus.GGIndx)));
        // }

        // build the coefficient matrix on the right-hand side of LLMat*delta_V = LG_gen*delta_Vg + LG_sw*delta_B + LG_trans*delta_k  
        public void buildLGMatWithRegCtr(DoubleMatrix2D LGMat)
        {
            foreach(var loadBus in loadBusArrayList)
            {
                double bij = yMat.yMatIm[hostBus.yMatIndx, loadBus.yMatIndx];
                if (bij != 0)
                {
                    LGMat.setQuick(loadBus.LLIndx, voltOptmVarIndx, bij);                              // Matrix L	off-diagonal element
                }
            }
        }

        // build the inequality constraints coefficient matrix 
        public void buildInequConCofMat(DoubleMatrix2D optmInEquConConfMat)
        {
            optmInEquConConfMat.setQuick(2 * voltOptmVarIndx, voltOptmVarIndx, 1);              // upper limit 
            optmInEquConConfMat.setQuick(2 * voltOptmVarIndx + 1, voltOptmVarIndx, -1); 			// lower limit 
        }

        // build the right-hand side of the inequality constraints 
        public void buildInequbMat(DoubleMatrix2D optmInEqubMat)
        {

            double voltUpperMargin = hostBus.aggQUpperMargin / VQSens;
            double voltBottomMargin = hostBus.aggQBottomMargin / VQSens;
            //CustomMessageHandler.Show("genBus" + hostBus.I + " " + voltUpperMargin + " " + voltBottomMargin);

            optmInEqubMat.setQuick(2 * voltOptmVarIndx, 0, voltUpperMargin);                    // upper limit 
            optmInEqubMat.setQuick(2 * voltOptmVarIndx + 1, 0, voltBottomMargin);					// lower limit
        }

        // update regulating variables after each iteration 
        public void updateRegVar(Optimisation.Result result)
        {
            double oldVoltSet = hostBus.genBusVoltSetCalc;
            double newVoltSet = oldVoltSet + result.get(voltOptmVarIndx).doubleValue();
            hostBus.genBusVoltSetCalc = newVoltSet;
            CustomMessageHandler.Show("----->VoltSet at gen bus " + hostBus.I + " changes from " + String.format("%5.5f", oldVoltSet) + " to " + String.format("%5.5f", newVoltSet));
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
            return false;
        }
    }
}
