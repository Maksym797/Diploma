using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using cern.colt.matrix;
using SimAGS.Components.ExtendOption;
using org.ojalgo.optimisation;
using SimAGS.Handlers;

namespace SimAGS.Components
{
    public class twoWindTrans : abstractPfElement
    {
        // raw data section <Line 1> s
        public int I = 0;       // 1st bus number
        public int J = 0;       // 2nd bus number
        public int K = 0;       // 3rd bus number
        public String CKT = "'1'";  // ID 
        public int CW = 1;      // winding data I/O code
        public int CZ = 1;      // impedance data I/O code
        public int CM = 1;      // magnetizing admittance I/O code
        public double MAG1 = 0.0;       // no load losses
        public double MAG2 = 0.0;       // exicting current 
        public int NMETR = 2;       // non-metered end code (1- 1st winding; 2 - 2nd winding; 3 - 3nd winding )
        public String NAME = "";        // name
        public int STAT = 0;        // 1 - in-service; 0 - OOS 
        public int O1 = 0;      // 1st owner 
        public double F1 = 0.0;     // 1st owner percentage
        public int O2 = 0;      // 2nd owner 
        public double F2 = 0.0;     // 2nd owner percentage
        public int O3 = 0;      // 3rd owner 
        public double F3 = 0.0;     // 3rd owner percentage
        public int O4 = 0;      // 4th owner 
        public double F4 = 0.0;     // 4th owner percentage

        // raw data section <Line 2>
        public double R1_2 = 0.0;       // 1st winding R
        public double X1_2 = 0.0;       // 1st winding X 
        public double SBASE1_2 = 0.0;       // 1st winding BASE

        // raw data section <Line 3> 
        public double WINDV1 = 1.0;     // 1st winding off-nominal turns ratio 
        public double NOMV1 = 0.0;      // rated 1st winding voltage in kV 
        public double ANG1 = 0.0;       // 1 st winding phase shift angle in degrees 
        public double RATA1 = 0.0;      // RATE 1 
        public double RATB1 = 0.0;      // RATE 2 
        public double RATC1 = 0.0;      // RATE 3 
        public int COD1 = 0;        // control mode 
        public int CONT1 = 0;       // controlled bus for voltage control 
        public double RMA1 = 1.1;       // upper limit
        public double RMI1 = 0.9;       // lower limit
        public double VMA1 = 1.1;       // upper limit
        public double VMI1 = 0.9;       // lower limit
        public int NTP1 = 33;       // number of tap positions available
        public int TAB1 = 0;        // correction table
        public double CR1 = 0.0;        // drop compensation impedance
        public double CX1 = 0.0;        // drop compensation impedance

        // raw data section <Line 4> 
        public double WINDV2 = 1.0;     // 2nd winding off-nominal turns ratio 
        public double NOMV2 = 0.0;      // rated 2nd winding voltage in kV 
        public double ANG2 = 0.0;       // 2nd winding phase shift angle in degrees 

        // extended variables 
        public bus frBus;
        public bus toBus;

        public int frBusV_Pos = 0;
        public int frBusA_Pos = 0;
        public int toBusV_Pos = 0;
        public int toBusA_Pos = 0;

        // calculated variables 
        public double kRatio = 0.0;     // calculated ratio 
        public double pAngle = 0.0;     // calculated phase angle in randians 

        public double calcGII = 0.0;        // Real(1,1) element on P 246 of kunder's book
        public double calcGIJ = 0.0;        // Real(1,2) 
        public double calcGJI = 0.0;        // Real(2,1)
        public double calcGJJ = 0.0;        // Real(2,2) 
        public double calcBII = 0.0;        // Image(1,1) element on P 246 of kunder's book 
        public double calcBIJ = 0.0;        // Image(1,2)
        public double calcBJI = 0.0;        // Image(2,1)
        public double calcBJJ = 0.0;        // Image(2,2) 

        public double dcalcBII_dk = 0.0;
        public double dcalcGIJ_dk = 0.0;
        public double dcalcBIJ_dk = 0.0;

        public double dcalcBJJ_dk = 0.0;
        public double dcalcGJI_dk = 0.0;
        public double dcalcBJI_dk = 0.0;


        public double frBusV = 0.0;     // from bus voltage pu 
        public double frBusA = 0.0;     // from bus voltage angle 
        public double toBusV = 0.0;     // to bus voltage pu 
        public double toBusA = 0.0;     // to bus voltage angle 
        public double frBusP = 0.0;     // MW flow at from bus 
        public double frBusQ = 0.0;     // MVar flow at from bus 
        public double toBusP = 0.0;     // MW flow at to bus
        public double toBusQ = 0.0;     // MVar flow at to bus
        public double pLoss = 0.0;      // MW loss 
        public double qLoss = 0.0;      // MVar loss 

        // for regulating control 
        public BaseExtOption voltExtOption = null;

        public int fromLoadBusLLIndx = 0;
        public int voltOptmVarIndx = 0;
        public int toLoadbusLLIndx = 0;

        // presenting data purpose 
        // 2 winding transformer
        public static String[] header = { "FrBus", "ToBus", "ID", "Status", "R", "X", "MagG", "MagB", "WindMVA", "Wind1 Ratio", "Wind1 Ang", "Wind2 Ratio", "Wind2 Ang", "RateA", "RateB", "RateC" };
        public static int tableColNum = 16;


        // read data from string line 
        public twoWindTrans(String Line1, String Line2, String Line3, String Line4)
        {

            // Read data line 1 
            String[] dataEntry = dataProcess.getDataFields(Line1, ",");
            I = Integer.parseInt(dataEntry[0]);
            J = Integer.parseInt(dataEntry[1]);
            K = Integer.parseInt(dataEntry[2]);
            CKT = dataEntry[3];
            CW = Integer.parseInt(dataEntry[4]);
            CZ = Integer.parseInt(dataEntry[5]);
            CM = Integer.parseInt(dataEntry[6]);
            MAG1 = Double.Parse(dataEntry[7]);
            MAG2 = Double.Parse(dataEntry[8]);
            NMETR = Integer.parseInt(dataEntry[9]);
            NAME = dataEntry[10];
            STAT = Integer.parseInt(dataEntry[11]);
            O1 = Integer.parseInt(dataEntry[12]);
            F1 = Double.Parse(dataEntry[13]);
            if (dataEntry.Length > 14)
            {
                O2 = Integer.parseInt(dataEntry[14]);
                F2 = Double.Parse(dataEntry[15]);
                if (dataEntry.Length > 16)
                {
                    O3 = Integer.parseInt(dataEntry[16]);
                    F3 = Double.Parse(dataEntry[17]);
                    if (dataEntry.Length > 18)
                    {
                        O4 = Integer.parseInt(dataEntry[18]);
                        F4 = Double.Parse(dataEntry[19]);
                    }
                }
            }

            // Read data line 2 
            dataEntry = dataProcess.getDataFields(Line2, ",");
            R1_2 = Double.Parse(dataEntry[0]);
            X1_2 = Double.Parse(dataEntry[1]);
            if (dataEntry.Length == 2)
            {                           // default 100 MVA if not specified
                SBASE1_2 = 100.0;
            }
            else
            {
                SBASE1_2 = Double.Parse(dataEntry[2]);
            }

            // Read data line 3 
            dataEntry = dataProcess.getDataFields(Line3, ",");
            WINDV1 = Double.Parse(dataEntry[0]);
            NOMV1 = Double.Parse(dataEntry[1]);
            ANG1 = Double.Parse(dataEntry[2]);
            RATA1 = Double.Parse(dataEntry[3]);
            RATB1 = Double.Parse(dataEntry[4]);
            RATC1 = Double.Parse(dataEntry[5]);
            COD1 = Integer.parseInt(dataEntry[6]);
            CONT1 = Integer.parseInt(dataEntry[7]);
            RMA1 = Double.Parse(dataEntry[8]);    // upper limit
            RMI1 = Double.Parse(dataEntry[9]);    // low limit		
            VMA1 = Double.Parse(dataEntry[10]);
            VMI1 = Double.Parse(dataEntry[11]);
            NTP1 = Integer.parseInt(dataEntry[12]);
            TAB1 = Integer.parseInt(dataEntry[13]);
            CR1 = Double.Parse(dataEntry[14]);
            CX1 = Double.Parse(dataEntry[15]);

            // Read data line 4 
            dataEntry = dataProcess.getDataFields(Line4, ",");
            WINDV2 = Double.Parse(dataEntry[0]);
            NOMV2 = Double.Parse(dataEntry[1]);

            // calculate transformer parameters
            // transformer impedance adjustment 
            if (CW == 1)
            {
                if (CZ == 1)
                {
                    // R and X is on system base, however voltage and toBus taping ratio adjustment is applied
                    R1_2 = R1_2 * WINDV2 * WINDV2;
                    X1_2 = X1_2 * WINDV2 * WINDV2;

                }
                else if (CZ == 2)
                {
                    R1_2 = R1_2 * SBASE / SBASE1_2 * WINDV2 * WINDV2;
                    X1_2 = X1_2 * SBASE / SBASE1_2 * WINDV2 * WINDV2;

                }
                else
                {
                    CustomMessageHandler.Show("### ERROR: trans R and X parameter conversion");
                    Environment.Exit(1);
                }
            }
            else
            {
                CustomMessageHandler.Show("### ERROR: trans off-normal turning ratio");
                Environment.Exit(1);
            }

            kRatio = WINDV1 / WINDV2;
            pAngle = ANG1 * Deg2Rad;
            calcTWPara();

            if (Math.Abs(COD1) == 1 && CONT1 != 0)
            {
                CustomMessageHandler.Show("[defined in twoWindTrans.java]Transformer at bus " + I + " to " + J + " regulates bus voltaget at " + CONT1 + " VoltMax = " + VMA1 + " VoltMin = " + VMI1);
                voltExtOption = new tapRegOption(this);
            }
        }

        // calculate two-winding transformer parameters  
        public void calcTWPara()
        {
            // calculated variables (refers to Kurder's book on page 246) isolated to customized for the application of adjusting ratio and shift angle   
            double zTotal = R1_2 * R1_2 + X1_2 * X1_2;
            double g = R1_2 / zTotal;                           // Ye = g + jb
            double b = -X1_2 / zTotal;
            double cosA = Math.Cos(pAngle);
            double sinA = Math.Sin(pAngle);

            //------- important pending improvement ---------//
            // Kj^2*RT + kj^2*jXT = equivalent Z, 

            calcGII = g / kRatio / kRatio;
            calcGIJ = -g * cosA / kRatio + b * sinA / kRatio;
            calcGJI = -g * cosA / kRatio - b * sinA / kRatio;
            calcGJJ = g;

            calcBII = b / kRatio / kRatio;
            calcBIJ = -g * sinA / kRatio - b * cosA / kRatio;
            calcBJI = g * sinA / kRatio - b * cosA / kRatio;
            calcBJJ = b;

            // for sensitivity 
            dcalcBII_dk = -2 * b / kRatio / kRatio / kRatio;
            dcalcGIJ_dk = g * cosA / kRatio / kRatio - b * sinA / kRatio / kRatio;
            dcalcBIJ_dk = g * sinA / kRatio / kRatio + b * cosA / kRatio / kRatio;

            dcalcBJJ_dk = 0;
            dcalcGJI_dk = g * cosA / kRatio / kRatio + b * sinA / kRatio / kRatio;
            dcalcBJI_dk = -g * sinA / kRatio / kRatio + b * cosA / kRatio / kRatio;
        }


        // calculate power injection and losses 
        public void calPQFlow()
        {
            // update voltages 
            frBusV = frBus.volt;
            frBusA = frBus.ang;
            toBusV = toBus.volt;
            toBusA = toBus.ang;

            if (STAT == 1)
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

                //CustomMessageHandler.Show("Trans frP = " + frBusP + " frQ = " + frBusQ); 
                //CustomMessageHandler.Show("Trans toP = " + toBusP + " toQ = " + toBusQ); 
            }
        }

        // return bus extended option for voltage regulation

        public BaseExtOption getVoltExtOption()
        {
            return voltExtOption;
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
            if (STAT == 1)
            {
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
        // 2-winding transformer
        public override string[] AsArrayForRow()
        {
            return new[]
            {
                $"{I}",
                $"{J}",
                $"{CKT}",
                $"{(STAT == 1 ? "Closed" : "Open")}",
                $"{R1_2}",
                $"{X1_2}",
                $"{MAG1}",
                $"{MAG2}",
                $"{SBASE1_2}",
                $"{WINDV1}",
                $"{ANG1}",
                $"{WINDV2}",
                $"{ANG2}",
                $"{RATA1}",
                $"{RATB1}",
                $"{RATC1}",
            };
        }


    }
}
