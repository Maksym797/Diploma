using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cern.colt.matrix;
using SimAGS.Handlers;

namespace SimAGS.DynModels.GenModels
{
    public abstract class genModel: AbstractTableViewing
    {
        public String modelType = "";
        public double SBASE = 100;
        public const double LARGE_NUMBER = 1E5;


        public void ini(DoubleMatrix2D yVector, DoubleMatrix2D xVector)
        {

        }

        public void update_h(double h)
        {

        }

        public void update_Var(DoubleMatrix2D x, DoubleMatrix2D y)
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

        // reset pm0 
        public void zeroPm0()
        {

        }

        // rest edf 
        public void zeroEfd0()
        {

        }

        // use as the interface for Govenor model to change generator tm input 
        public void setPmRef(double pmSet)
        {

        }

        // for governor initialization only 
        public double getPm()
        {
            return 0;
        }

        public int getOmegaPos()
        {
            return 0;
        }

        public int getDeltaPos()
        {
            return 0;
        }

        public double getPegen()
        {
            return 0;
        }

        public double getQegen()
        {
            return 0;
        }

        public double getVt()
        {
            return 0;
        }

        public double getDelta()
        {
            return 0;
        }


        public double getOmega()
        {
            return 0;
        }

        public double getPm0()
        {
            return 0;
        }

        public double getEfd0()
        {
            return 0;
        }

        public double getEfd()
        {
            return 0;
        }


        public void setVf(double efdSetting)
        {

        }

        // data sanity check process 
        public bool paraSanityTest()
        {
            return true;
        }

        //public Object[] setTable()
        //{
        //    return new Object[] { };
        //}
    }

}
