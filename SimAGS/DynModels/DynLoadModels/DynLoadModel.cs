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


        public void ini(double[,] yVector, double[,] xVector)
        {

        }

        public void update_h(double h)
        {

        }

        public void update_Var(double[,] x, double[,] y, double tCurrent)
        {

        }

        public void update_g(double[,] g)
        {

        }

        public void update_f(double[,] f)
        {

        }

        public void update_gx(double[,] jacMat, int startRow, int startColumn)
        {

        }

        public void update_gy(double[,] jacMat, int startRow, int startColumn)
        {

        }

        public void update_fx(double[,] jacMat, int starRow, int startColumn, double simTheta, double h)
        {

        }

        public void update_fy(double[,] jacMat, int starRow, int startColumn, double simTheta, double h)
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
