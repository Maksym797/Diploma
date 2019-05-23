using System.Collections.Generic;
using cern.colt.matrix;
using java.lang;
using SimAGS.Components;

namespace SimAGS.SimUtil
{
    public class jacob
    {

        public static double LARGENUM = 1E10;     // diagonal element 


        // type = 0 : formulate jacobian matrix for pf; 
        // type = 1 : formulate jacobian matrix for time domain simulation

        //public static void update(DoubleMatrix2D yMatRe, DoubleMatrix2D yMatIm, DoubleMatrix2D sol, DoubleMatrix2D JMat, ArrayList <bus> busList, int type){
        public static void update(yMatrix yMat, DoubleMatrix2D sol, DoubleMatrix2D JMat, List<bus> busList, int type)
        {

            //int nNodes = busList.size();
            double a, b;
            double Vi, Vj, Ai, Aj, sinAij, cosAij, c, d, e, f;


            // formulate power jacobian matrix from input sol = [V;Theta]' and pq = [P;Q]' treating all buses as PQ buses 
            // formulate jacobian matrix assuming all buses are PQ bus
            foreach (bus busTemp in busList)
            {
                // [1] set diagonal element based on fixed shunt and switched shunts  
                Ai = sol.getQuick(busTemp.vangPos, 0);
                Vi = sol.getQuick(busTemp.vmagPos, 0);
                a = Vi * yMat.yMatRe.getQuick(busTemp.yMatIndx, busTemp.yMatIndx);  // Vi*Vi*Gii
                b = Vi * yMat.yMatIm.getQuick(busTemp.yMatIndx, busTemp.yMatIndx);      // Vi*Vi*Bii

                JMat.setQuick(busTemp.vangPos, busTemp.vmagPos, -(-2 * a));         // self-derivatives (YMat)
                JMat.setQuick(busTemp.vmagPos, busTemp.vmagPos, -2 * b);                // self-derivatives (YMat)

                // [2] append extra element corresponding to dPL/dV and qQL/dVt to jacbian matrix valid for PF calculation
                if (busTemp.bHasLoad && type == 0)
                {
                    JMat.setQuick(busTemp.vangPos, busTemp.vmagPos, JMat.getQuick(busTemp.vangPos, busTemp.vmagPos) + busTemp.aggCCLoadP + 2 * Vi * busTemp.aggCYLoadP);
                    JMat.setQuick(busTemp.vmagPos, busTemp.vmagPos, JMat.getQuick(busTemp.vmagPos, busTemp.vmagPos) + busTemp.aggCCLoadQ - 2 * Vi * busTemp.aggCYLoadQ);
                }

                // [3] append branch related jacobian matrix elements 
                foreach (bus neigBus in busTemp.neighborBusList)
                {

                    Aj = sol.getQuick(neigBus.vangPos, 0);
                    Vj = sol.getQuick(neigBus.vmagPos, 0);
                    sinAij = Math.sin(Ai - Aj);
                    cosAij = Math.cos(Ai - Aj);
                    c = Vi * Vj * yMat.yMatRe.getQuick(busTemp.yMatIndx, neigBus.yMatIndx) * sinAij;            // Vi*Vj*G(i,j)*sin(Ai-Aj)
                    d = Vi * Vj * yMat.yMatIm.getQuick(busTemp.yMatIndx, neigBus.yMatIndx) * cosAij;            // Vi*Vj*B(i,j)*cos(Ai-Aj)
                    e = Vi * Vj * yMat.yMatRe.getQuick(busTemp.yMatIndx, neigBus.yMatIndx) * cosAij;            // Vi*Vj*G(i,j)*cos(Ai-Aj)
                    f = Vi * Vj * yMat.yMatIm.getQuick(busTemp.yMatIndx, neigBus.yMatIndx) * sinAij;            // Vi*Vj*B(i,j)*sin(Ai-Aj) 

                    JMat.setQuick(busTemp.vangPos, busTemp.vangPos, JMat.getQuick(busTemp.vangPos, busTemp.vangPos) - (c - d));                 // update Matrix H diagonal element 
                    JMat.setQuick(busTemp.vangPos, busTemp.vmagPos, JMat.getQuick(busTemp.vangPos, busTemp.vmagPos) - (-e - f) / Vi);               // update Matrix N diagonal element 
                    JMat.setQuick(busTemp.vmagPos, busTemp.vangPos, JMat.getQuick(busTemp.vmagPos, busTemp.vangPos) - (-e - f));                    // update Matrix K diagonal element
                    JMat.setQuick(busTemp.vmagPos, busTemp.vmagPos, JMat.getQuick(busTemp.vmagPos, busTemp.vmagPos) - (-c + d) / Vi);               // update Matrix L diagonal element

                    JMat.setQuick(busTemp.vangPos, neigBus.vangPos, -(-c + d));         // Matrix H off-diagonal element
                    JMat.setQuick(busTemp.vangPos, neigBus.vmagPos, -(-e - f) / Vj);        // Matrix N off-diagonal element
                    JMat.setQuick(busTemp.vmagPos, neigBus.vangPos, -(e + f));          // Matrix K off-diagonal element
                    JMat.setQuick(busTemp.vmagPos, neigBus.vmagPos, -(-c + d) / Vj);        // Matrix L	off-diagonal element
                }
            }

            // process to define PV bus 
            if (type == 0)
            {
                foreach (bus busTemp in busList)
                {
                    // PV bus 
                    // Revise Q equation for PV bus
                    if (busTemp.calcType == 2)
                    {
                        foreach (bus neigBus in busTemp.neighborBusList)
                        {
                            JMat.setQuick(busTemp.vmagPos, neigBus.vangPos, 0.0);
                            JMat.setQuick(busTemp.vmagPos, neigBus.vmagPos, 0.0);
                        }
                        JMat.setQuick(busTemp.vmagPos, busTemp.vangPos, 0.0);
                        JMat.setQuick(busTemp.vmagPos, busTemp.vmagPos, 1.0);
                    }
                }
            }

            // process to define slack bus 
            if (type == 0)
            {
                foreach (bus busTemp in busList)
                {
                    // Revise P and Q equations for Slack bus  
                    if (busTemp.calcType == 3)
                    {
                        foreach (bus neigBus in busTemp.neighborBusList)
                        {
                            JMat.setQuick(busTemp.vangPos, neigBus.vangPos, 0.0);
                            JMat.setQuick(busTemp.vangPos, neigBus.vmagPos, 0.0);

                            JMat.setQuick(busTemp.vmagPos, neigBus.vangPos, 0.0);
                            JMat.setQuick(busTemp.vmagPos, neigBus.vmagPos, 0.0);
                        }

                        JMat.setQuick(busTemp.vangPos, busTemp.vangPos, 1.0);
                        JMat.setQuick(busTemp.vangPos, busTemp.vmagPos, 0.0);

                        JMat.setQuick(busTemp.vmagPos, busTemp.vangPos, 0.0);
                        JMat.setQuick(busTemp.vmagPos, busTemp.vmagPos, 1.0);
                    }
                }
            }
        }
    }
}
