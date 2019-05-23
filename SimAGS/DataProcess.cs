using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using cern.colt.matrix;
using java.util;
using SimAGS.Components;
using SimAGS.Handlers;

namespace SimAGS
{
    internal class dataProcess
    {
        // read in data using specified token 
        public static String[] getDataFields(String str, String token)
        {
            StringTokenizer st = new StringTokenizer(str, token);
            int m = st.countTokens();
            var s1 = new string[m];
            for (int i = 0; i < m; i++)
            {
                s1[i] = st.nextToken().trim();
            }
            return s1;
        }

        //// combine two vector x = [x1;x2]
        public static DoubleMatrix2D combVector(DoubleMatrix2D x1, DoubleMatrix2D x2)
        {
            int rowX1 = x1.rows();
            int rowX2 = x2.rows();
            int colX1 = x1.columns();
            int colX2 = x2.columns();

            if (colX1 != colX2)
            {
                CustomMessageHandler.Show("ERROR: Matrix columns are not matched!");
                Environment.Exit(0);
            }

            DoubleMatrix2D ret = new double[rowX1 + rowX2, colX1];

            for (int i = 0; i < rowX1; i++)
            {
                for (int j = 0; j < colX1; j++)
                {
                    ret.setQuick(i, j, x1.getQuick(i, j));
                }
            }

            for (int i = 0; i < rowX2; i++)
            {
                for (int j = 0; j < colX2; j++)
                {
                    ret.setQuick(i + rowX1, j, x2.getQuick(i, j));
                }
            }

            return ret;
        }

        // matrix operation a*x1 + b*x2
        public static DoubleMatrix2D matAdd(DoubleMatrix2D x1, DoubleMatrix2D x2, double a, double b)
        {
            int rowX1 = x1.rows();
            int rowX2 = x2.rows();
            int colX1 = x1.columns();
            int colX2 = x2.columns();

            if (rowX1 != rowX2 || colX1 != colX2)
            {
                CustomMessageHandler.Show(rowX1 + "VS" + rowX2 + "\n" + colX1 + "VS" + colX2 + "\nERROR: matrix length doesn't match!");
                Environment.Exit(0);
            }

            DoubleMatrix2D ret = new double[rowX1, colX1];
            for (int i = 0; i < rowX1; i++)
            {
                for (int j = 0; j < colX1; j++)
                {
                    ret.setQuick(i, j, a * x1.getQuick(i, j) + b * x2.getQuick(i, j));
                }
            }
            return ret;
        }


        public static void dispMat(DoubleMatrix2D xMat)
        {
            String str = "";
            for (int i = 0; i < xMat.rows(); i++)
            {
                for (int j = 0; j < xMat.columns(); j++)
                {
                    str = str + "\t" + String.Format("%9.5f", xMat.getQuick(i, j));
                }
                str = str + "\n";
            }
            CustomMessageHandler.Show(str);
            Environment.Exit(0);
        }

        public static String dataLimitCheck(String valStr, double inputVal, double lowLimit, String lowLimitVarName, bool lowLimitIncludeEq, double upLimit, String upLimitVarName, bool upLimitInclueEq, String delimitorStr)
        {

            if (inputVal > upLimit || (inputVal == upLimit && !upLimitInclueEq))
            {
                return String.Format(" %8s = %10.3f exceeds up  limit = %10.3f %8s %8s", valStr, inputVal, upLimit, upLimitVarName, delimitorStr);
            }
            if (inputVal < lowLimit || (inputVal == lowLimit && !lowLimitIncludeEq))
            {
                return String.Format(" %8s = %10.3f exceeds low limit = %10.3f %8s %8s", valStr, inputVal, lowLimit, lowLimitVarName, delimitorStr);
            }
            return "";
        }


        ////------------------ miscellaneous functions -----------------------//
        //// get bus position 
        public static bus getBusAt(int busNum, List<bus> busList)
        {
            for (int i = 0; i < busList.size(); i++)
            {
                bus busTemp = busList.get(i);
                if (busTemp.I == busNum)
                {
                    return busTemp;
                }
            }
            return null;
        }
    }
}
