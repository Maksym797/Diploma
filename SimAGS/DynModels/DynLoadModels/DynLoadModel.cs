using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimAGS.DynModels.DynLoadModels
{
    public class DynLoadModel
    {

        public String modelType = "";
        public double SBase = 100;
        public const double LARGE_NUMBER = 1E5;


        public void ini(DoubleMatrix2D yVector, DoubleMatrix2D xVector)
        {

        }

        public void update_h(double h)
        {

        }

        public void update_Var(DoubleMatrix2D x, DoubleMatrix2D y, double tCurrent)
        {

        }

        public void update_g(DoubleMatrix2D g)
        {

        }

        public void update_f(DoubleMatrix2D f)
        {

        }

        public void update_gx(DoubleMatrix2D jacMat, int startRow, int startColumn)
        {

        }

        public void update_gy(DoubleMatrix2D jacMat, int startRow, int startColumn)
        {

        }

        public void update_fx(DoubleMatrix2D jacMat, int starRow, int startColumn, double simTheta, double h)
        {

        }

        public void update_fy(DoubleMatrix2D jacMat, int starRow, int startColumn, double simTheta, double h)
        {

        }

        public double getDynPInj() { return 0.0; }

        public double getDynQInj() { return 0.0; }


        public Object[] setTable()
        {
            return new Object[] { };
        }
    }
}
