using SimAGS.DynModels.MonModels;
using System;
using cern.colt.matrix;
using ikvm.extensions;

namespace SimAGS.Components
{
    public class branch : abstractPfElement
    {
        // data loaded from raw file 
        public int I = 0;       // from bus number
        public int J = 0;       // to bus number
        public String CKT = "'1'";  // ID 
        public double R = 0.0;      // R in pu
        public double X = 0.0;      // X in pu 
        public double B = 0.0;      // total charging susceptance
        public double RATEA = 0.0;      // 1st loading rating in MVA
        public double RATEB = 0.0;      // 2nd loading rating in MVA
        public double RATEC = 0.0;      // 3rd loading rating in MVA
        public double GI = 0.0;     // admittance of the line shunt at bus I in pu (assume it is zero)
        public double BI = 0.0;     // admittance of the line shunt at bus I in pu (assume it is zero)
        public double GJ = 0.0;     // admittance of the line shunt at bus J in pu (assume it is zero)
        public double BJ = 0.0;     // admittance of the line shunt at bus J in pu (assume it is zero) 
        public int ST = 0;      // status 
        public double LEN = 0.0;        // line length in user-selected units;
        public int O1 = 0;      // 1st owner
        public double F1 = 0.0;     // 1st owner percentage 
        public int O2 = 0;      // 2nd owner
        public double F2 = 0.0;     // 2nd owner percentage
        public int O3 = 0;      // 3rd owner
        public double F3 = 0.0;     // 3rd owner percentage
        public int O4 = 0;      // 4th owner 
        public double F4 = 0.0;     // 4th owner percentage

        public static int DATALENGTH = 23;  // default data line 

        // extended variable 
        public int measureEnd = 0;      // 0 - measured at from bus; 1 - measured at to bus 
        public bus frBus = null;        // from bus object
        public bus toBus = null;        // to bus object
        public int frBusV_Pos = 0;
        public int frBusA_Pos = 0;
        public int toBusV_Pos = 0;
        public int toBusA_Pos = 0;


        // calculated variables 
        public double calcMutualG = 0.0;        // g of mutual impedance 
        public double calcMutualB = 0.0;        // b of mutual impedance 
        public double calcGII = 0.0;        // sum of calculated self-G and mutual G at from bus that in Y matrix
        public double calcBII = 0.0;        // sum of calculated self-B and mutual B at from bus 
        public double calcGJJ = 0.0;        // sum of calculated self-G and mutual G at to bus 
        public double calcBJJ = 0.0;        // sum of calculated self-B and mutual B at to bus 
        public double calcGIJ = 0.0;        // Real of Y(i,j)
        public double calcBIJ = 0.0;        // Imag of Y(i,j)
        public double calcGJI = 0.0;        // Real of Y(j,i) 
        public double calcBJI = 0.0;        // Imag of Y(j,i) 

        public double frBusV = 0.0;     // from bus voltage  		[pu]
        public double frBusA = 0.0;     // from bus voltage angle	[rad] 
        public double toBusV = 0.0;     // to bus voltage pu 		[pu]
        public double toBusA = 0.0;     // to bus voltage angle 	[rad]
        public double frBusP = 0.0;     // MW flow at from bus 		[pu]
        public double frBusQ = 0.0;     // MVar flow at from bus 	[pu]
        public double toBusP = 0.0;     // MW flow at to bus		[pu]
        public double toBusQ = 0.0;     // MVar flow at to bus		[pu]
        public double pLoss = 0.0;      // MW loss 					[pu]
        public double qLoss = 0.0;      // MVar loss 				[pu]
        public double overLoadRateA = 0.0;      // percentage of existing MVA follow compared to Rate A [%]

        // for branch flow measurement (dynamic simulation) 
        public bool bHasMWFlowMeaseure = false;
        public BranFlow MWFlowMon = null;
        public int MWFlow_Pos = 0;

        // presenting data purpose 
        public static String[] header = { "From bus", "To bus", "ID", "Status", "R", "X", "B", "MW", "MVar", "Lim A", "Lim B", "Lim C", "OverLoad[%]" };
        public static int tableColNum = 13;

        // Read data from string line 
        public branch(String line)
        {
            String[] dataEntry = dataProcess.getDataFields(line, ",");
            I = int.Parse(dataEntry[0]);
            J = int.Parse(dataEntry[1]);
            if (J < 0)
            {
                J = Math.Abs(J);
                measureEnd = 1;
            }
            CKT = dataEntry[2].substring(1, dataEntry[2].lastIndexOf("'")).trim();
            R = Double.Parse(dataEntry[3]);
            X = Double.Parse(dataEntry[4]);
            B = Double.Parse(dataEntry[5]);
            RATEA = Double.Parse(dataEntry[6]) / SBASE;
            RATEA = RATEA <= 0 ? 99 : RATEA;                        // avoid zero rating
            RATEB = Double.Parse(dataEntry[7]) / SBASE;
            RATEC = Double.Parse(dataEntry[8]) / SBASE;
            GI = Double.Parse(dataEntry[9]);
            BI = Double.Parse(dataEntry[10]);
            GJ = Double.Parse(dataEntry[11]);
            BJ = Double.Parse(dataEntry[12]);
            ST = Integer.parseInt(dataEntry[13]);
            LEN = Double.Parse(dataEntry[14]);
            O1 = Integer.parseInt(dataEntry[15]);
            F1 = Double.Parse(dataEntry[16]);
            if (dataEntry.length() > 17)
            {
                O2 = Integer.parseInt(dataEntry[17]);
                F2 = Double.Parse(dataEntry[18]);
                if (dataEntry.length() > 19)
                {
                    O3 = Integer.parseInt(dataEntry[19]);
                    F3 = Double.Parse(dataEntry[20]);
                    if (dataEntry.length() > 21)
                    {
                        O4 = Integer.parseInt(dataEntry[21]);
                        F4 = Double.Parse(dataEntry[22]);
                    }
                }
            }

            calcPara();
        }

        // calculate parameters for building Y matrix 
        public void calcPara()
        {
            double zTotal = R * R + X * X;
            calcMutualG = R / zTotal;                   //  g+jb = 1/(R+jX)
            calcMutualB = -X / zTotal;

            calcGII = GI + calcMutualG;
            calcBII = BI + calcMutualB + B / 2;             // charging B/2 
            calcGJJ = GJ + calcMutualG;
            calcBJJ = BJ + calcMutualB + B / 2;

            calcGIJ = -calcMutualG;
            calcBIJ = -calcMutualB;
            calcGJI = -calcMutualG;
            calcBJI = -calcMutualB;
        }

        // calculate power flow 
        public void calPQFlow()
        {
            frBusV = frBus.volt;
            frBusA = frBus.ang;
            toBusV = toBus.volt;
            toBusA = toBus.ang;

            if (ST == 1)
            {
                // calculate PQ flow 
                double sinAij = Math.Sin(frBusA - toBusA);
                double cosAij = Math.Cos(frBusA - toBusA);
                frBusP = frBusV * frBusV * calcGII + frBusV * toBusV * (calcGIJ * cosAij + calcBIJ * sinAij);
                frBusQ = -frBusV * frBusV * calcBII + frBusV * toBusV * (calcGIJ * sinAij - calcBIJ * cosAij);

                toBusP = toBusV * toBusV * calcGJJ + toBusV * frBusV * (calcGJI * cosAij - calcBJI * sinAij);
                toBusQ = -toBusV * toBusV * calcBJJ + toBusV * frBusV * (-calcGJI * sinAij - calcBJI * cosAij);

                // calculate PQ loss
                pLoss = frBusP + toBusP;
                qLoss = frBusQ + toBusQ;

                // calculate overloading at normal condition 
                overLoadRateA = measureEnd == 0 ? Math.Sqrt(frBusP * frBusP + frBusQ * frBusQ) / RATEA * 100 : Math.Sqrt(toBusP * toBusP + toBusQ * toBusQ) / RATEA * 100;
            }
        }

        // set from bus 
        public void setFromBus(bus busTemp)
        {
            frBus = busTemp;
            frBusV_Pos = frBus.vmagPos;
            frBusA_Pos = frBus.vangPos;
        }

        // set to bus 
        public void setToBus(bus busTemp)
        {
            toBus = busTemp;
            toBusV_Pos = toBus.vmagPos;
            toBusA_Pos = toBus.vangPos;
        }

        // update Y Matrix 
        public void updateYMat(DoubleMatrix2D yMatRe, DoubleMatrix2D yMatIm)
        {
            if (ST == 1)
            {       // for closed line only 
                int iPos = frBus.yMatIndx;
                int jPos = toBus.yMatIndx;

                // off-diagonal element 
                yMatRe.setQuick(iPos, jPos, calcGIJ + yMatRe.getQuick(iPos, jPos));
                yMatIm.setQuick(iPos, jPos, calcBIJ + yMatIm.getQuick(iPos, jPos));
                yMatRe.setQuick(jPos, iPos, calcGJI + yMatRe.getQuick(jPos, iPos));
                yMatIm.setQuick(jPos, iPos, calcBJI + yMatIm.getQuick(jPos, iPos));

                // diagonal element
                yMatRe.setQuick(iPos, iPos, calcGII + yMatRe.getQuick(iPos, iPos));
                yMatIm.setQuick(iPos, iPos, calcBII + yMatIm.getQuick(iPos, iPos));
                yMatRe.setQuick(jPos, jPos, calcGJJ + yMatRe.getQuick(jPos, jPos));
                yMatIm.setQuick(jPos, jPos, calcBJJ + yMatIm.getQuick(jPos, jPos));
            }
        }



        // export data for tabular showing 
        public Object[] setTable()
        {
            Object[] ret = new Object[tableColNum];
            return ret;
        }


        public override string[] AsArrayForRow()
        {
            return new[]
            {
                $"{I}",
                $"{J}",
                $"{CKT}",
                $"{(ST == 1 ? "Closed" : "Open")}",
                $"{R}",
                $"{X}",
                $"{B}",
                $"{(measureEnd == 0 ? frBusP * SBASE : toBusP * SBASE)}",
                $"{(measureEnd == 1 ? frBusQ * SBASE : toBusQ * SBASE)}",
                $"{RATEA * SBASE}",
                $"{RATEB * SBASE}",
                $"{RATEC * SBASE}",
                $"{overLoadRateA}",
            };
        }
    }
}
