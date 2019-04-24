using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimAGS.Components;

namespace SimAGS
{
    internal class dataProcess
    {
        // read in data using specified token 
        public static String[] getDataFields(String str, String token)
        {
            //final StringTokenizer st = new StringTokenizer(str, token);
            //int m = st.countTokens();
            //String s1[] = new String[m];
            //for (int i = 0; i < m; i++)
            //{
            //    s1[i] = st.nextToken().trim();
            //}
            //return s1;
            throw new NotImplementedException();
        }

        //// combine two vector x = [x1;x2]
        //public static double[,] combVector(double[,] x1, double[,] x2)
        //{
        //    //int rowX1 = x1.rows();
        //    //int rowX2 = x2.rows();
        //    //int colX1 = x1.columns();
        //    //int colX2 = x2.columns();
        //    //
        //    //if (colX1 != colX2)
        //    //{
        //    //    System.out.println("ERROR: Matrix columns are not matched!");
        //    //    System.exit(0);
        //    //}
        //    //
        //    //double[,] ret = new Sparsedouble[,](rowX1 + rowX2, colX1);
        //    //
        //    //for (int i = 0; i < rowX1; i++)
        //    //{
        //    //    for (int j = 0; j < colX1; j++)
        //    //    {
        //    //        ret.setQuick(i, j, x1.getQuick(i, j));
        //    //    }
        //    //}
        //    //
        //    //for (int i = 0; i < rowX2; i++)
        //    //{
        //    //    for (int j = 0; j < colX2; j++)
        //    //    {
        //    //        ret.setQuick(i + rowX1, j, x2.getQuick(i, j));
        //    //    }
        //    //}
        //    //
        //    //return ret;
        //    throw new NotImplementedException();
        //}

        //// matrix operation a*x1 + b*x2
        //public static double[,] matAdd(double[,] x1, double[,] x2, double a, double b)
        //{
        //    //int rowX1 = x1.rows();
        //    //int rowX2 = x2.rows();
        //    //int colX1 = x1.columns();
        //    //int colX2 = x2.columns();
        //    //
        //    //if (rowX1 != rowX2 || colX1 != colX2)
        //    //{
        //    //    System.out.println(rowX1 + "VS" + rowX2);
        //    //    System.out.println(colX1 + "VS" + colX2);
        //    //    System.out.println("ERROR: matrix length doesn't match!");
        //    //    System.exit(0);
        //    //}
        //    //
        //    //double[,] ret = new Sparsedouble[,](rowX1, colX1);
        //    //for (int i = 0; i < rowX1; i++)
        //    //{
        //    //    for (int j = 0; j < colX1; j++)
        //    //    {
        //    //        ret.setQuick(i, j, a * x1.getQuick(i, j) + b * x2.getQuick(i, j));
        //    //    }
        //    //}
        //    //return ret;
        //    throw new NotImplementedException();
        //}


        //public static void dispMat(double[,] xMat)
        //{
        //    //String str = "";
        //    //for (int i = 0; i < xMat.rows(); i++)
        //    //{
        //    //    for (int j = 0; j < xMat.columns(); j++)
        //    //    {
        //    //        str = str + "\t" + String.format("%9.5f", xMat.getQuick(i, j));
        //    //    }
        //    //    str = str + "\n";
        //    //}
        //    //System.out.println(str);
        //    //System.out.println();
        //    throw new NotImplementedException();
        //}


        //public static String dataLimitCheck(String valStr, double inputVal, double lowLimit, String lowLimitVarName, boolean lowLimitIncludeEq, double upLimit, String upLimitVarName, boolean upLimitInclueEq, String delimitorStr)
        //{

        //    //if (inputVal > upLimit || (inputVal == upLimit && !upLimitInclueEq))
        //    //{
        //    //    return String.format(" %8s = %10.3f exceeds up  limit = %10.3f %8s %8s", valStr, inputVal, upLimit, upLimitVarName, delimitorStr);
        //    //}
        //    //else if (inputVal < lowLimit || (inputVal == lowLimit && !lowLimitIncludeEq))
        //    //{
        //    //    return String.format(" %8s = %10.3f exceeds low limit = %10.3f %8s %8s", valStr, inputVal, lowLimit, lowLimitVarName, delimitorStr);
        //    //}
        //    //else
        //    //{
        //    //    return "";
        //    //}
        //    throw new NotImplementedException();
        //}


        ////------------------ miscellaneous functions -----------------------//
        //// get bus position 
        public static bus getBusAt(int busNum, List<bus> busList)
        {
            //for (int i = 0; i < busList.size(); i++)
            //{
            //    bus busTemp = busList.get(i);
            //    if (busTemp.I == busNum)
            //    {
            //        return busTemp;
            //    }
            //}
            //return null;
            throw new NotImplementedException();
        }
    }
}
