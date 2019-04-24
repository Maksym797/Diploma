using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimAGS.Components
{
    public class swshunt
    {
        public int I = 0;       // bus number
        public int MODSW = 0;       // control mode
        public double VSWHI = 1.0;      // control upper limit
        public double VSWLO = 1.0;      // control lower limit
        public int SWREM = 0;       // regulated bus number
        public double RMPCT = 0.0;      // percentage of capacity 
        public String RMIDNT = "";      // name 
        public double BINIT = 0.0;      // shunt admittance in MVar at unity voltage 
        public int N1 = 0;      // numbter of steps for block 1 
        public double B1 = 0;       // admittance increment for each of N1 steps in block 1 in MVar at unity voltage  (- reactor; + capacitor)
        public int N2 = 0;      // numbter of steps for block 2 
        public double B2 = 0;       // admittance increment for each of N1 steps in block 2 in MVar at unity voltage 	
        public int N3 = 0;      // numbter of steps for block 3 
        public double B3 = 0;       // admittance increment for each of N1 steps in block 3 in MVar at unity voltage 	
        public int N4 = 0;      // numbter of steps for block 4 
        public double B4 = 0;       // admittance increment for each of N1 steps in block 4 in MVar at unity voltage 
        public int N5 = 0;      // numbter of steps for block 5 
        public double B5 = 0;       // admittance increment for each of N1 steps in block 5 in MVar at unity voltage 
        public int N6 = 0;      // numbter of steps for block 6 
        public double B6 = 0;       // admittance increment for each of N1 steps in block 6 in MVar at unity voltage 
        public int N7 = 0;      // numbter of steps for block 7 
        public double B7 = 0;       // admittance increment for each of N1 steps in block 7 in MVar at unity voltage 
        public int N8 = 0;      // numbter of steps for block 8 
        public double B8 = 0;       // admittance increment for each of N1 steps in block 8 in MVar at unity voltage 

        public double SBASE = 100;

        // calculate variables 
        public double calcBMax = 0.0;   // calculated BMax
        public double calcBMin = 0.0;   // calculated BMin


        // for table display
        public static String[] header = {"Number", "CtrlMode", "VswHI", "VswLO", "SWREM", "RMPCT", "RMIDNT", "B0" , "N1", "B1",
        "N2", "B2", "N3", "B3", "N4", "B4", "N5","B5","N6", "B6","N7","B7","N8","B8"};
        public static int tableColNum = 24; // default data line 


        // Read data from string line 
        public swshunt(String line)
        {
            String[] dataEntry = dataProcess.getDataFields(line, ",");
            int iData = dataEntry.Length;
            calcBMax = 0.0;
            calcBMin = 0.0;

            I = int.Parse(dataEntry[0]);
            MODSW = int.Parse(dataEntry[1]);
            VSWHI = Double.Parse(dataEntry[2]);
            VSWLO = Double.Parse(dataEntry[3]);
            SWREM = int.Parse(dataEntry[4]);
            RMPCT = Double.Parse(dataEntry[5]) / 100;
            RMIDNT = dataEntry[6];
            BINIT = Double.Parse(dataEntry[7]) / SBASE;
            N1 = int.Parse(dataEntry[8]);
            B1 = Double.Parse(dataEntry[9]) / SBASE;
            if (iData > 10)
            {
                N2 = int.Parse(dataEntry[10]);
                B2 = Double.Parse(dataEntry[11]) / SBASE;
                if (iData > 12)
                {
                    N3 = int.Parse(dataEntry[12]);
                    B3 = Double.Parse(dataEntry[13]) / SBASE;
                    if (iData > 14)
                    {
                        N4 = int.Parse(dataEntry[14]);
                        B4 = Double.Parse(dataEntry[15]) / SBASE;
                        if (iData > 16)
                        {
                            N5 = int.Parse(dataEntry[16]);
                            B5 = Double.Parse(dataEntry[17]) / SBASE;
                            if (iData > 18)
                            {
                                N6 = int.Parse(dataEntry[18]);
                                B6 = Double.Parse(dataEntry[19]) / SBASE;
                                if (iData > 20)
                                {
                                    N7 = int.Parse(dataEntry[20]);
                                    B7 = Double.Parse(dataEntry[21]) / SBASE;
                                    if (iData > 22)
                                    {
                                        N8 = int.Parse(dataEntry[22]);
                                        B8 = Double.Parse(dataEntry[23]) / SBASE;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // 
            calcBMin = Math.Min(B1 * N1, 0);
            calcBMax = Math.Max(B1 * N1 + B2 * N2 + B3 * N3 + B4 * N4 + B5 * N5 + B6 * N6 + B7 * N7 + B8 * N8, 0);
            //System.out.println("calcB = " + calcB + " calcBMax" + calcBMax + " calcBMin " + calcBMin);
        }




        // export data for tabular showing 
        public Object[] setTable()
        {
            Object[] ret = new Object[tableColNum];
            ret[0] = I;
            ret[1] = MODSW;
            ret[2] = VSWHI;
            ret[3] = VSWLO;
            ret[4] = SWREM;
            ret[5] = RMPCT * 100;
            ret[6] = RMIDNT;
            ret[7] = BINIT * SBASE;
            ret[8] = N1;
            ret[9] = B1 * SBASE;
            ret[10] = N2;
            ret[11] = B2 * SBASE;
            ret[12] = N3;
            ret[13] = B3 * SBASE;
            ret[14] = N4;
            ret[15] = B4 * SBASE;
            ret[16] = N5;
            ret[17] = B5 * SBASE;
            ret[18] = N6;
            ret[19] = B6 * SBASE;
            ret[20] = N7;
            ret[21] = B7 * SBASE;
            ret[22] = N8;
            ret[23] = B8 * SBASE;
            return ret;
        }
    }
}
