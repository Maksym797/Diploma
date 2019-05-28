using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cern.colt.matrix;
using SimAGS.Components;
using SimAGS.Handlers;

namespace SimAGS.DynModels.AgcModel
{
    public abstract class agcModel : AbstractTableViewing
    {
        public String modelType = "";
        public double SBASE = 100;
        public const double LARGE_NUMBER = 1E5;

        public int getAGCAreaNum() { return 0; }

        //public int getLast_AlgeVarNum() {return 0;}

        //public int getLast_StateVarNum() {return 0;}

        public void addBusForFreq(bus busTemp, double freqW) { }

        public void addTieLine(branch branTemp, int measureSide) { }

        public void addGenOnAGC(gen genTemp, double AGCWeight) { }

        public void ini(DoubleMatrix2D yVector, DoubleMatrix2D xVector) { }

        public void update_h(double h) { }

        public void update_Var(DoubleMatrix2D x, DoubleMatrix2D y) { }

        public void update_g(DoubleMatrix2D g) { }

        public void update_f(DoubleMatrix2D f) { }

        public void update_gx(DoubleMatrix2D jacMat, int startRow, int startColumn) { }

        public void update_gy(DoubleMatrix2D jacMat, int startRow, int startColumn) { }

        public void update_fx(DoubleMatrix2D jacMat, int starRow, int startColumn, double simTheta, double h) { }

        public void update_fy(DoubleMatrix2D jacMat, int starRow, int startColumn, double simTheta, double h) { }

        public void store_Var() { }

        public void activate() { }

        public void deactivate() { }

        public double getNextTimeInterval() { return 0; }

        public bool isActivate()
        {
            return false;
        }

        public double get_ACE()
        {
            return 0.0;
        }

        public double get_areaFreq()
        {
            return 0.0;
        }

        public double get_tieMWFlow()
        {
            return 0.0;
        }

        public double get_CPS1()
        {
            return 0.0;
        }

        public double get_CPS2()
        {
            return 0.0;
        }

        public double get_pRefTotal()
        {
            return 0.0;
        }

        public Object[] setTable()
        {
            return new Object[] { };
        }
    }
}
