using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cern.colt.matrix;
using cern.colt.matrix.linalg;
using org.ojalgo.optimisation;
using SimAGS.SimUtil;

namespace SimAGS.Components.ExtendOption
{
    public class BaseExtOption
    {
        //TODO
        public Algebra matrixOpt = new Algebra();

        public int getVoltOptmVarIndx() { return 0; }

        public void buildLGMatWithRegCtr(DoubleMatrix2D LGMat) { }

        public void buildInequConCofMat(DoubleMatrix2D optmInEquConConfMat) { }

        public void buildInequbMat(DoubleMatrix2D optmInEqubMat) { }

        public void updateRegVar(Optimisation.Result result) { }

        public bool getIsUpdateYMat() { return false; }

        public void calcVQSens(PQDecoupleJacob decoupleJacMat) { }

        public void setVoltOptmVarIndx(int setVal) { }

        public void setYMat(yMatrix yMat) { }

        public void setLoadBusList(List<bus> list) { }
    }
}
