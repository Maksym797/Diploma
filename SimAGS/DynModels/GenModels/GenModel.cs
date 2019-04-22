using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimAGS.DynModels.GenModels
{
    public class GenModel
    {

        public String modelType = "";
        public double SBASE = 100;
        public const double LARGE_NUMBER = 1E5;


        public void ini(double[,] yVector, double[,] xVector)
        {

        }

        public void update_h(double h)
        {

        }

        public void update_Var(double[,] x, double[,] y)
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

        public Object[] setTable()
        {
            return new Object[] { };
        }
    }

}
