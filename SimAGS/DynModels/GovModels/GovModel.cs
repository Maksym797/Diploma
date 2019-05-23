using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cern.colt.matrix;

namespace SimAGS.DynModels.GovModels
{
    public class GovModel
    {

        public double SBASE = 100;

        public void ini(DoubleMatrix2D yVector, DoubleMatrix2D xVector) 
        {

        }


        public void update_Var(DoubleMatrix2D y, DoubleMatrix2D x)
        {

        }

        public void update_g(DoubleMatrix2D g)
        {

        }

        public void update_f(DoubleMatrix2D f)
        {

        }


        public void update_fx(DoubleMatrix2D jacMat, int starRow, int startColumn, double simTheta, double h)
        {

        }

        public void update_fy(DoubleMatrix2D jacMat, int starRow, int startColumn, double simTheta, double h)
        {

        }

        public void update_gx(DoubleMatrix2D jacMat, int startRow, int startColumn)
        {

        }

        public void update_gy(DoubleMatrix2D jacMat, int startRow, int startColumn)
        {

        }

        public void setAGCSingal(double setVal)
        {

        }

        public double getTotalMWRef()
        {
            return 0.0;
        }

        public void setMWRef(double val)
        {

        }

        public bool paraSanityTest()
        {
            return true;
        }

        // debug code [check limit output]
        public double limitTest()
        {
            return 0.0;
        }
        public Object[] setTable()
        {
            return new Object[] { };
        }
    }
}
