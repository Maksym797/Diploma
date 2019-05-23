using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cern.colt.matrix;
using cern.colt.matrix.impl;
using cern.colt.matrix.linalg;
using SimAGS.Components;

namespace SimAGS.SimUtil
{
    public class PQDecoupleJacob
    {

        public Algebra matrixOpt = new Algebra();       // matrix operator object 

        public DoubleMatrix2D JMat;
        public DoubleMatrix2D PAMat;                // PAMat = dP/dA
        public DoubleMatrix2D QVMat;                // QVMat = dQ/dV 

        public DoubleMatrix2D LLMat;                // LLMat = dPl/dVl
        public DoubleMatrix2D LGMat;                // LGMat = dPl/dVg
        public DoubleMatrix2D GLMat;                // GLMat = dPg/dVl
        public DoubleMatrix2D GGMat;                // GGMat = dPg/dVg
        public DoubleMatrix2D LLInvMat;

        public yMatrix yMat;
        public List<bus> busList;
        public int nBus;

        /*
         * full Jacobian matrix 
         */
        public PQDecoupleJacob(yMatrix yMat, List<bus> busList)
        {
            this.yMat = yMat;
            this.busList = busList;
            this.nBus = busList.size();

            buildQVMat();
        }


        /*
         * build QVMat
         */
        public void buildQVMat()
        {
            // formulate QVMat
            QVMat = new SparseDoubleMatrix2D(nBus, nBus);
            for (int i = 0; i < nBus; i++)
            {
                for (int j = 0; j < nBus; j++)
                {
                    QVMat.setQuick(i, j, -yMat.yMatIm.getQuick(i, j));
                }
            }
        }

        /*
         * build PVMat
         */
        public void buildPAMat()
        {

        }


        /*
         *  formulate LLMat, LGMat, GLMat, GGMat 
         */
        public void buildSubMatrix(int[] loadIndx, int[] genIndx)
        {

            LLMat = QVMat.viewSelection(loadIndx, loadIndx);
            LGMat = QVMat.viewSelection(loadIndx, genIndx);
            GLMat = QVMat.viewSelection(genIndx, loadIndx);
            GGMat = QVMat.viewSelection(genIndx, genIndx);
            LLInvMat = matrixOpt.inverse(LLMat);
        }


    }
}
