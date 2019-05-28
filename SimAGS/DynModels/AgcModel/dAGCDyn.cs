using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cern.colt.matrix;
using ikvm.extensions;
using SimAGS.Components;
using SimAGS.DistEvent;
using SimAGS.Handlers;

namespace SimAGS.DynModels.AgcModel
{
    public class dAGCDyn : agcModel
    {
        // constant  
        public const double SBASE = 100.0; // system base 

        // ---------------- define algebraic variables ---------------//
        public int AGCAreaNum = 0;

        public const int NUM_ALGE = 4; // algebraic variable number
        public double areaFreq = 0.0; // weighted area frequency 
        public double tieMWFlow = 0.0; // tie line flow 
        public double ACE = 0.0; // ACE 
        public double pRefTotal = 0.0; // controller output at k 

        public int areaFreq_Pos = 0;
        public int tieMWFlow_Pos = 1;
        public int ACE_Pos = 2;
        public int pRefTotal_Pos = 3;
        public int last_AlgeVar_Pos = 0;

        //------------------ define state variable --------------------//
        public const int NUM_STATE = 0;

        public int last_StateVar_Pos = 0;

        //---------------- intermediate variables -------------------//
        public List<bus> busForFreqList = new List<bus>();

        public List<branch> tieLineList = new List<branch>();
        public List<gen> genOnAGCList = new List<gen>();

        public double tieMWFlow_temp = 0.0;
        public double areaFreq_temp = 0.0;

        // AGC parameters 
        public double freqCof = 0.0; // frequency deviation, originally MW/0.1Hz converted to pu

        public double MWCof = 0.0; // MW flow deviation coefficient 
        public double kp = 0.0; // P gain in PI controller
        public double ti = 0.0; // time constant in PI 
        public double iMax = 0.0; // integer output limit 

        public double tieMWFlowSet = 0.0; // setting value for tie line MW flow 
        public double areaFreqSet = 1.0; // area frequency setting 1.0 pu 
        public double intTimeStep = 4; // default integration time step (4 sec) 	

        // discrete AGC no state variables defined 
        public double ACEm1 = 0.0; // error at k-1

        public double pRefTotalm1 = 0.0; // controller output at k-1
        public double k1 = 0.0; //  Kp*(1+ deltaT/Ti) --> deltaT = AGC updating interval = 2 or 4 sec.
        public double k2 = 0.0; // -Kp*
        public bool bAGCAct = false; // if AGC acts at current time instant

        // CPS1 related (1 minutes average ACE)
        public double CPS1TimeInterval = 60; // 1 minutes

        public int CPS1StoreCount = 0;
        public int CPS1Count = 0;
        public double ACE1minAverage = 0.0; // ACE 1 minutes average
        public double dF1minAverage = 0.0;
        public double CF1Val = 0.0;
        public double CPS1Val = 0.0;
        public double Epsilon1 = 0.020 / 60; // 18 mHz 

        // CPS2 related (10 minutes average ACE)
        public double CPS2TimeInterval = 10 * 60; // 10 minutes

        public int CPS2StoreCount = 0;
        public int CPS2Count = 0;
        public double ACE10minAverage = 0.0;
        public double CF2Val = 0.0;
        public double CPS2Val = 0.0;
        public double L10Val = 0.0;
        public double RVal = 0.0;
        public double Epsilon10 = 0.0057 / 60; //5.7 mHz

        public int CF2GT1 = 0; // count for CF2 greater than 1.0

        //public LinkedList<int> CF2ViolateList = new LinkedList<int>();
        public List<int> CF2ViolateList = new List<int>();

        public double sumCF2Violate = 0;

        // for dynamically adjustting event queue 
        public distEventList eventList;

        // for presenting data purpose 
        public override String[] header {get; set;} =
            {"AGC Area", "Freq Cof", "MW Cof", "KP", "TI", "iMAX", "Freq Bus", "Tie Line", "AGC Gen"};

       

        public static int tableColNum = 9;

        public void dAGCCDyn()
        {

        }


        // default constructor 
        public dAGCDyn(String[] token, int numAlge, int numState)
        {

            busForFreqList = new List<bus>();
            genOnAGCList = new List<gen>();
            tieLineList = new List<branch>();

            // algebraic variable index 
            areaFreq_Pos = areaFreq_Pos + numAlge;
            tieMWFlow_Pos = tieMWFlow_Pos + numAlge;
            ACE_Pos = ACE_Pos + numAlge;
            pRefTotal_Pos = pRefTotal_Pos + numAlge;
            last_AlgeVar_Pos = numAlge + NUM_ALGE;

            // state variable index 
            last_StateVar_Pos = numState + NUM_STATE;

            AGCAreaNum = Integer.parseInt(token[1]);
            freqCof = Double.Parse(token[2]) / 100 * 60;
            MWCof = Double.Parse(token[3]);
            kp = Double.Parse(token[4]);
            ti = Double.Parse(token[5]);
            iMax = Double.Parse(token[6]);

            CustomMessageHandler.println("Discrete AGC at area " + AGCAreaNum + " is loaded![FreqCof = " +
                                         _String.format("%.4f", freqCof) + " MWCof = " + _String.format("%.4f", MWCof) +
                                         "]");
        }


        // ------------------------------ read in data ---------------------------------//
        // add bus for freq to busList 
        public void addBusForFreq(bus busTemp, double freqWeight)
        {
            if (busTemp != null)
            {
                if (busTemp.bHasFreqMeasure)
                {
                    busTemp.areaFreqWeight = freqWeight;
                    busForFreqList.add(busTemp);
                }
                else
                {
                    CustomMessageHandler.println("ERROR: please specify bus " + busTemp.I +
                                                 " for frequency measurement in dyn data!");
                    Environment.Exit(0);
                }
            }
            else
            {
                CustomMessageHandler.println(
                    "ERROR: bus data is not found when setting bus for frequency calculation!");
                Environment.Exit(0);
            }
        }

        // add generator for AGC control 
        public void addGenOnAGC(gen genTemp, double AGCWeight)
        {
            if (genTemp != null)
            {
                if (genTemp.hasGovModel)
                {
                    genTemp.hasAGCCtrl = true;
                    genTemp.agcPRefTotal_Pos = this.pRefTotal_Pos;
                    genTemp.AGCSMWhare = AGCWeight;
                    genOnAGCList.add(genTemp);
                }
                else
                {
                    CustomMessageHandler.println("ERROR: AGC gen at " + genTemp.I +
                                                 " can't load because gov doesn't exist");
                    Environment.Exit(0);
                }
            }
            else
            {
                CustomMessageHandler.println("ERROR: gen data is not found when setting gen for AGC control!");
            }
        }


        // add tieLine flow for AGC control 
        public void addTieLine(branch branchTemp, int measureSide)
        {
            if (branchTemp != null)
            {
                if (branchTemp.bHasMWFlowMeaseure)
                {
                    tieLineList.add(branchTemp);
                }
            }
            else
            {
                CustomMessageHandler.println("ERROR: branch data is not found when setting tie-line for AGC control!");
                Environment.Exit(0);
            }
        }


        // ---------------------------------- for dynamic simulation -------------------------------//
        // initialize the element in x vector 
        public void ini(DoubleMatrix2D yVector, DoubleMatrix2D xVector)
        {

            // calculate initial ACE value 
            areaFreq = 0.0; // assume that area frequency  = 1.0 pu

            // calculate MW flow difference 
            tieMWFlow = 0.0;
            foreach (branch branTemp in tieLineList)
            {
                tieMWFlow += yVector.getQuick(branTemp.MWFlow_Pos, 0);
            }

            // set initial tie-line flow setting 
            tieMWFlowSet = tieMWFlow;
            CustomMessageHandler.println("--> AGC tie line MW flow setting value = " + tieMWFlow);

            // calculate initial ACE (assume that ACE = 0.0 initially)
            ACE = 0.0;
            pRefTotal = 0.0;
            ACEm1 = 0.0;
            pRefTotalm1 = 0.0;
            k1 = kp * (1 + intTimeStep / ti);
            k2 = -kp;

            // initialize yVector element 
            yVector.setQuick(areaFreq_Pos, 0, areaFreq);
            yVector.setQuick(tieMWFlow_Pos, 0, tieMWFlow);
            yVector.setQuick(ACE_Pos, 0, ACE);
            yVector.setQuick(pRefTotal_Pos, 0, pRefTotal);

            // CPS1 initialization
            CPS1StoreCount = (int) (CPS1TimeInterval / intTimeStep);
            ACE1minAverage = 0.0;
            dF1minAverage = 0.0;
            CF1Val = 0.0;
            CPS1Val = (2 - CF1Val) * 100;

            // CPS2 initialization
            CPS2StoreCount = (int) (CPS2TimeInterval / intTimeStep);
            ACE10minAverage = 0.0;
            L10Val = 1.65 * Epsilon10 *
                     Math.Sqrt((-10 * freqCof) * (-10 * 2 * freqCof)); // assuming two balance areas are defined
            CF2Val = 0.0;
            sumCF2Violate = 0;
            RVal = 0.0;
            CPS2Val = (1 - RVal) * 100;
            for (int j = 0; j < 6; j++)
            {
                CF2ViolateList.Add(0); // assume that no violation exists prior to simulation 
                sumCF2Violate = sumCF2Violate + 0;
            }
        }


        // update variables at the beginning of each iteration 
        public void update_Var(DoubleMatrix2D yVector, DoubleMatrix2D xVector)
        {
            areaFreq = yVector.getQuick(areaFreq_Pos, 0);
            tieMWFlow = yVector.getQuick(tieMWFlow_Pos, 0);
            ACE = yVector.getQuick(ACE_Pos, 0);
            pRefTotal = yVector.getQuick(pRefTotal_Pos, 0);

            if (bAGCAct == true)
            {
                // calculate intermediate variables 
                areaFreq_temp = 0.0;
                foreach (bus busTemp in busForFreqList)
                {
                    areaFreq_temp += busTemp.areaFreqWeight * yVector.getQuick(busTemp.busFreq_Pos, 0);
                }
                // calculate MW flow difference 
                tieMWFlow_temp = 0.0;
                foreach (branch branTemp in tieLineList)
                {
                    tieMWFlow_temp += yVector.getQuick(branTemp.MWFlow_Pos, 0);
                }
            }
        }

        // g(ACE) = -10*freqCof*areaDOmega + MWCof*(interfaceMWSetting - interfaceMW) - ACE  
        // g(pRefTotal) = pRefTotalm1 + k1*ACE + k2*ACEm1 - pRefTotal; 
        // update_g = -g(ACE), ..., -g(pRefTotal); 
        public void update_g(DoubleMatrix2D g)
        {
            if (bAGCAct == true)
            {
                g.setQuick(areaFreq_Pos, 0, -(areaFreq_temp - areaFreq));
                g.setQuick(tieMWFlow_Pos, 0, -(tieMWFlow_temp - tieMWFlow));
                g.setQuick(ACE_Pos, 0, -(-10 * freqCof * areaFreq + MWCof * (tieMWFlowSet - tieMWFlow) - ACE));
                g.setQuick(pRefTotal_Pos, 0, -(pRefTotalm1 + k1 * ACE + k2 * ACEm1 - pRefTotal));
            }
        }

        // calculate gy = dg/dy
        public void update_gy(DoubleMatrix2D jacMat, int startRow, int startColumn)
        {

            jacMat.setQuick(startRow + areaFreq_Pos, startColumn + areaFreq_Pos, -1);
            jacMat.setQuick(startRow + tieMWFlow_Pos, startColumn + tieMWFlow_Pos, -1);
            jacMat.setQuick(startRow + ACE_Pos, startColumn + ACE_Pos, -1);
            jacMat.setQuick(startRow + pRefTotal_Pos, startColumn + pRefTotal_Pos, -1);

            if (bAGCAct == true)
            {
                foreach (bus busTemp in busForFreqList)
                {
                    jacMat.setQuick(startRow + areaFreq_Pos, startColumn + busTemp.busFreq_Pos, busTemp.areaFreqWeight);
                }

                foreach (branch branTemp in tieLineList)
                {
                    jacMat.setQuick(startRow + tieMWFlow_Pos, startColumn + branTemp.MWFlow_Pos, 1);
                }

                jacMat.setQuick(startRow + ACE_Pos, startColumn + areaFreq_Pos, -10 * freqCof);
                jacMat.setQuick(startRow + ACE_Pos, startColumn + tieMWFlow_Pos, -MWCof);

                jacMat.setQuick(startRow + pRefTotal_Pos, startColumn + ACE_Pos, k1);
            }
        }


        // update variables after the current instant is done (calculate latest CPS1 and CPS2 and store ACE and pReftotal for the next time instant)
        public void store_Var()
        {
            //------------------ check CPS1 criterion ----------------------//	
            // it is assumed that AVG(CF1) is averaged for just one-minute-clock average
            if (CPS1Count < CPS1StoreCount)
            {
                ACE1minAverage += ACE * intTimeStep / CPS1TimeInterval;
                dF1minAverage += areaFreq * intTimeStep / CPS1TimeInterval;
                CPS1Count++;
                //CustomMessageHandler.println(" ACE = " + ACE + " ACE1minAverage = " + ACE1minAverage + " areaDOmega = " + areaDOmega);
                //CustomMessageHandler.println("CPS1Count = " + CPS1Count + " vs CPS1StoreCount = " + CPS1StoreCount); 
            }
            else
            {
                // calculate CPS1 and reset 
                CF1Val = 1 / Epsilon1 / Epsilon1 * ACE1minAverage / (-10 * freqCof) * dF1minAverage;
                //CustomMessageHandler.println("========== CF1Val = " + CF1Val + " Epsilon1 =" + Epsilon1 + "========================"); 

                CPS1Val = (2 - Math.Abs(CF1Val)) * 100;
                //
                ACE1minAverage = 0.0;
                dF1minAverage = 0.0;
                CPS1Count = 0;
            }

            //-------------------- check CPS2 criterion ---------------------//
            // it is assumed that 
            if (CPS2Count < CPS2StoreCount)
            {
                ACE10minAverage += ACE * intTimeStep / CPS2TimeInterval;
                CPS2Count++;
            }
            else
            {
                // remove the oldest data 
                sumCF2Violate = sumCF2Violate - CF2ViolateList.removeFirst();

                // calculate CPS2 and reset 
                CF2Val = Math.Abs(ACE10minAverage) / L10Val;
                CustomMessageHandler.println("CF2Val = " + CF2Val);

                if (CF2Val > 1)
                {
                    CF2ViolateList.add(1);
                    sumCF2Violate = sumCF2Violate + 1;
                }
                else
                {
                    CF2ViolateList.add(0);
                    sumCF2Violate = sumCF2Violate + 0;
                }

                RVal = sumCF2Violate / 6; // assume that the simulation duration is only 1 hour  
                CPS2Val = 100 * (1 - RVal);
                CustomMessageHandler.println("sumCF2Violate = " + sumCF2Violate + " RVal = " + RVal + " CPS2Val = " +
                                             CPS2Val);
                //
                ACE10minAverage = 0.0;
                CPS2Count = 0;
            }

            // refresh memory
            ACEm1 = ACE;
            pRefTotalm1 = pRefTotal;
        }

        public bool isActivate()
        {
            return bAGCAct;
        }

        public void activate()
        {
            bAGCAct = true;
        }

        public void deactivate()
        {
            bAGCAct = false;
        }

        public double getNextTimeInterval()
        {
            return intTimeStep;
        }

        public int getAGCAreaNum()
        {
            return AGCAreaNum;
        }

        public int getLast_AlgeVarNum()
        {
            return last_AlgeVar_Pos;
        }

        public int getLast_StateVarNum()
        {
            return last_StateVar_Pos;
        }

        // set tie-line flow object 
        public void setMWFLow(double setVal)
        {
            tieMWFlowSet = setVal;
        }

        // get area frequency 
        public double get_areaFreq()
        {
            return areaFreq;
        }

        // get tie line MW flow 
        public double get_tieMWFlow()
        {
            return tieMWFlow;
        }

        // get ACE
        public double get_ACE()
        {
            return ACE * SBASE;
        }

        // get pref deviation
        public double get_pRefTotal()
        {
            return pRefTotal;
        }

        // get CPS1 
        public double get_CPS1()
        {
            return CPS1Val;
        }

        // get CPS2 
        public double getC_PS2()
        {
            return CPS2Val;
        }

        // export data for tabular showing 
        public Object[] setTable()
        {
            Object[] ret = new Object[tableColNum];
            ret[0] = AGCAreaNum;
            ret[1] = _String.format("%1.4f", freqCof * 100 / 60);
            ret[2] = _String.format("%1.4f", MWCof * 100);
            ret[3] = _String.format("%1.4f", kp);
            ret[4] = _String.format("%1.4f", ti);
            ret[5] = _String.format("%1.4f", iMax);

            // bus for frequency measurement 
            String tableElement = "";
            foreach (bus busTemp in busForFreqList)
            {
                tableElement = tableElement + busTemp.I + "[" + _String.format("%1.2f", busTemp.areaFreqWeight) + "]";
                tableElement = tableElement + ",";
            }
            ret[6] = tableElement.substring(0, tableElement.length() - 1);

            // tie line 
            tableElement = "";
            foreach (branch branTemp in tieLineList)
            {
                tableElement = tableElement + branTemp.I + "_" + branTemp.J + "_" + branTemp.CKT;
                tableElement = tableElement + ",";
            }
            ret[7] = tableElement.substring(0, tableElement.length() - 1);

            // AGC generator 
            tableElement = "";
            foreach (gen genTemp in genOnAGCList)
            {
                tableElement = tableElement + genTemp.I + "_" + genTemp.ID + "[" +
                               _String.format("%1.2f", genTemp.AGCSMWhare) + "]";
                tableElement = tableElement + ",";
            }
            ret[8] = tableElement.substring(0, tableElement.length() - 1);

            return ret;
        }
        public override string[] AsArrayForRow()
        {
            var ret = new List<string>
            {
                $"{AGCAreaNum}",
                $"{_String.format("%1.4f", freqCof * 100 / 60)}",
                $"{_String.format("%1.4f", MWCof * 100)}",
                $"{_String.format("%1.4f", kp)}",
                $"{_String.format("%1.4f", ti)}",
                $"{_String.format("%1.4f", iMax)}",
            };

            // bus for frequency measurement 
            String tableElement = "";
            foreach (bus busTemp in busForFreqList)
            {
                tableElement = tableElement + busTemp.I + "[" + _String.format("%1.2f", busTemp.areaFreqWeight) + "]";
                tableElement = tableElement + ",";
            }
            ret[6] = tableElement.substring(0, tableElement.length() - 1);

            // tie line 
            tableElement = "";
            foreach (branch branTemp in tieLineList)
            {
                tableElement = tableElement + branTemp.I + "_" + branTemp.J + "_" + branTemp.CKT;
                tableElement = tableElement + ",";
            }
            ret[7] = tableElement.substring(0, tableElement.length() - 1);

            // AGC generator 
            tableElement = "";
            foreach (gen genTemp in genOnAGCList)
            {
                tableElement = tableElement + genTemp.I + "_" + genTemp.ID + "[" +
                               _String.format("%1.2f", genTemp.AGCSMWhare) + "]";
                tableElement = tableElement + ",";
            }
            ret[8] = tableElement.substring(0, tableElement.length() - 1);

            return ret.ToArray();
        }
    }
}