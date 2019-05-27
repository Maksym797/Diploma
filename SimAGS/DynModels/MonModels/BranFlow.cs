using cern.colt.matrix;
using java.lang;
using SimAGS.Components;
using SimAGS.Handlers;

namespace SimAGS.DynModels.MonModels
{
    public class BRANFLOW
    {
        public const double w0 = 2 * 60 * Math.PI;          // default value 				

        // ---------------- define algebraic variables ---------------//
        public const int NUM_ALGE = 1;                  // algebraic variable number
        public double MWflow = 0.0;                     // MW flow 
        public int MWflow_Pos = 0;
        public int last_AlgeVar_Pos = 0;

        //------------------ define state variable --------------------//
        public const int NUM_STATE = 0;
        public int last_StateVar_Pos = 0;

        //---------------- intermediate variables -------------------//
        public double frV = 0.0;
        public double frA = 0.0;
        public double toV = 0.0;
        public double toA = 0.0;

        public int frBusV_Pos = 0;
        public int frBusA_Pos = 0;
        public int toBusV_Pos = 0;
        public int toBusA_Pos = 0;

        public double bij = 0;

        //------------------- object inherited from --------------------//
        public branch mBranch;


        // constructor calculate variables after power flow 
        public BRANFLOW(branch branTemp, int numAlge, int numState)
        {
            mBranch = branTemp;
            frBusV_Pos = mBranch.frBusV_Pos;
            frBusA_Pos = mBranch.frBusA_Pos;
            toBusV_Pos = mBranch.toBusV_Pos;
            toBusA_Pos = mBranch.toBusA_Pos;
            bij = mBranch.calcMutualB;

            // algebraic variable index 
            MWflow_Pos = MWflow_Pos + numAlge;
            last_AlgeVar_Pos = numAlge + NUM_ALGE;

            // state variable index  
            last_StateVar_Pos = numState + NUM_STATE;

            // update branch status 
            branTemp.bHasMWFlowMeaseure = true;
            branTemp.MWFlowMon = this;
            branTemp.MWFlow_Pos = MWflow_Pos;

            CustomMessageHandler.println("MW Measure Model at branch [" + branTemp.I + ", " + branTemp.J + "," + branTemp.CKT + "] is loaded!");
        }


        // initialize the variables 
        public void ini(DoubleMatrix2D yVector, DoubleMatrix2D xVector)
        {
            frV = yVector.getQuick(frBusV_Pos, 0);
            frA = yVector.getQuick(frBusA_Pos, 0);
            toV = yVector.getQuick(toBusV_Pos, 0);
            toA = yVector.getQuick(toBusA_Pos, 0);

            // calculate non-loss MW flow 
            MWflow = -bij * frV * toV * Math.sin(frA - toA);
            yVector.setQuick(MWflow_Pos, 0, MWflow);
        }


        // update variables at the beginning of each iteration 
        public void update_Var(DoubleMatrix2D yVector, DoubleMatrix2D xVector)
        {
            frV = yVector.getQuick(frBusV_Pos, 0);
            frA = yVector.getQuick(frBusA_Pos, 0);
            toV = yVector.getQuick(toBusV_Pos, 0);
            toA = yVector.getQuick(toBusA_Pos, 0);
            MWflow = yVector.getQuick(MWflow_Pos, 0);
        }

        // update -g(x,y) (where g(x,y) = -bij*Vi*Vj*sin(frA-toA) - P) 
        public void update_g(DoubleMatrix2D g)
        {
            g.setQuick(MWflow_Pos, 0, -(-bij * frV * toV * Math.sin(frA - toA) - MWflow));
        }


        // calculate gy = dg/dy element of dg(x,y)/dy
        public void update_gy(DoubleMatrix2D jacMat, int startRow, int startColumn)
        {
            double cosA = Math.cos(frA - toA);
            double sinA = Math.sin(frA - toA);
            jacMat.setQuick(startRow + MWflow_Pos, startColumn + MWflow_Pos, -1);
            jacMat.setQuick(startRow + MWflow_Pos, startColumn + frBusA_Pos, -bij * frV * toV * cosA);
            jacMat.setQuick(startRow + MWflow_Pos, startColumn + toBusA_Pos, bij * frV * toV * cosA);
            jacMat.setQuick(startRow + MWflow_Pos, startColumn + frBusV_Pos, -bij * frV * sinA);
            jacMat.setQuick(startRow + MWflow_Pos, startColumn + toBusV_Pos, -bij * toV * sinA);
        }

        // calculate gx = dg/dx 
        public void update_gx(DoubleMatrix2D jacMat, int startRow, int startColumn)
        {

        }

        // calculate dx/dt = f = f(x,y)
        public void update_f(DoubleMatrix2D f)
        {

        }


        // calculate fx = df/dx 
        public void update_fx(DoubleMatrix2D jacMat, int startRow, int startColumn, double simTheta, double h)
        {

        }


        // calculate fy = df/dy 
        public void update_fy(DoubleMatrix2D jacMat, int startRow, int startColumn, double simTheta, double h)
        {

        }

    }
}
