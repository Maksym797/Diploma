using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ikvm.extensions;
using java.io;
using SimAGS.Components;
using SimAGS.DynModels.AgcModel;
using SimAGS.DynProcessor;
using SimAGS.Handlers;

namespace SimAGS.SimUtil
{
    public class simBuffer
    {

        private DynCase dynProc;

        public List<Object[]> resultHeader
            ; // 1-> Dynamic type (Generator); 2-> Bus#; 3-> ID; 4-> Variable; 5-> Unit; 6-> index in xVector; 7; index in yVector (=0 if not exists)

        public List<double[]> resultMat;
        public List<Double> timeMat; // store the time instant


        // constructor 
        public simBuffer(DynCase dynProcInst)
        {
            resultHeader = new List<Object[]>();
            resultMat = new List<double[]>();
            timeMat = new List<Double>();
            dynProc = dynProcInst;
            ini(); // dynProc.resultHeader is initialized 
        }


        // initialize buffer 
        public void ini()
        {
            // ----------- set up result storing matrix (default format)  --------------//
            // 1-> Dynamic type (Generator); 2-> Bus#; 3-> ID; 4-> Variable; 5-> Unit; 6-> index in arraylist; 7 -> unique name 8-> gen name  

            // generator output 
            // generator speed 
            for (int i = 0; i < dynProc.busList.size(); i++)
            {
                bus busTemp = dynProc.busList.get(i);
                for (int j = 0; j < busTemp.busGens.size(); j++)
                {
                    gen genTemp = busTemp.busGens.get(j);

                    // generator speed 
                    Object[] genHeader = new Object[9];
                    genHeader[0] = "Generator";
                    genHeader[1] = genTemp.I;
                    genHeader[2] = genTemp.ID;
                    genHeader[3] = "Speed";
                    genHeader[4] = "pu";
                    genHeader[5] = i; // refers to bus number
                    genHeader[6] = j; // refers to generator in busGens
                    genHeader[7] = "Generator" + "_" + "Speed" + "_" + _String.valueOf(genTemp.I) + "_" + genTemp.ID;
                    genHeader[8] = _String.valueOf(genTemp.I) + "_" + genTemp.ID;
                    resultHeader.add(genHeader);

                    // generator MW output 
                    genHeader = new Object[9];
                    genHeader[0] = "Generator";
                    genHeader[1] = genTemp.I;
                    genHeader[2] = genTemp.ID;
                    genHeader[3] = "PGen";
                    genHeader[4] = "MW";
                    genHeader[5] = i;
                    genHeader[6] = j;
                    genHeader[7] = "Generator" + "_" + "PGen" + "_" + _String.valueOf(genTemp.I) + "_" + genTemp.ID;
                    genHeader[8] = _String.valueOf(genTemp.I) + "_" + genTemp.ID;
                    resultHeader.add(genHeader);

                    // generator MVar output 
                    genHeader = new Object[9];
                    genHeader[0] = "Generator";
                    genHeader[1] = genTemp.I;
                    genHeader[2] = genTemp.ID;
                    genHeader[3] = "QGen";
                    genHeader[4] = "MVar";
                    genHeader[5] = i;
                    genHeader[6] = j;
                    genHeader[7] = "Generator" + "_" + "QGen" + "_" + _String.valueOf(genTemp.I) + "_" + genTemp.ID;
                    genHeader[8] = _String.valueOf(genTemp.I) + "_" + genTemp.ID;
                    resultHeader.add(genHeader);

                    // generator Vt  
                    genHeader = new Object[9];
                    genHeader[0] = "Generator";
                    genHeader[1] = genTemp.I;
                    genHeader[2] = genTemp.ID;
                    genHeader[3] = "Vt";
                    genHeader[4] = "pu";
                    genHeader[5] = i;
                    genHeader[6] = j;
                    genHeader[7] = "Generator" + "_" + "Vt" + "_" + _String.valueOf(genTemp.I) + "_" + genTemp.ID;
                    genHeader[8] = _String.valueOf(genTemp.I) + "_" + genTemp.ID;
                    resultHeader.add(genHeader);

                    // generator field voltage 
                    genHeader = new Object[9];
                    genHeader[0] = "Generator";
                    genHeader[1] = genTemp.I;
                    genHeader[2] = genTemp.ID;
                    genHeader[3] = "Efd";
                    genHeader[4] = "pu";
                    genHeader[5] = i;
                    genHeader[6] = j;
                    genHeader[7] = "Generator" + "_" + "Efd" + "_" + _String.valueOf(genTemp.I) + "_" + genTemp.ID;
                    genHeader[8] = _String.valueOf(genTemp.I) + "_" + genTemp.ID;
                    resultHeader.add(genHeader);

                    // generator Pm setting value 
                    genHeader = new Object[9];
                    genHeader[0] = "Generator";
                    genHeader[1] = genTemp.I;
                    genHeader[2] = genTemp.ID;
                    genHeader[3] = "PGenSet";
                    genHeader[4] = "MW";
                    genHeader[5] = i;
                    genHeader[6] = j;
                    genHeader[7] = "Generator" + "_" + "PGenSet" + "_" + _String.valueOf(genTemp.I) + "_" + genTemp.ID;
                    genHeader[8] = _String.valueOf(genTemp.I) + "_" + genTemp.ID;
                    resultHeader.add(genHeader);
                }
            }

            // AGC output 
            for (int i = 0; i < dynProc.AGCList.size(); i++)
            {
                // area frequency 
                agcModel AGCTemp = dynProc.AGCList.get(i);
                Object[] AGCHeader = new Object[9];
                AGCHeader[0] = "AGC";
                AGCHeader[1] = AGCTemp.getAGCAreaNum();
                AGCHeader[2] = "";
                AGCHeader[3] = "Freq";
                AGCHeader[4] = "pu";
                AGCHeader[5] = i;
                AGCHeader[6] = 0;
                AGCHeader[7] = "AGC" + "_" + "Freq" + "_" + _String.valueOf(AGCTemp.getAGCAreaNum());
                AGCHeader[8] = _String.valueOf(AGCTemp.getAGCAreaNum());
                resultHeader.add(AGCHeader);

                // Tie line flows 
                AGCHeader = new Object[9];
                AGCHeader[0] = "AGC";
                AGCHeader[1] = AGCTemp.getAGCAreaNum();
                AGCHeader[2] = "";
                AGCHeader[3] = "Tie Flow";
                AGCHeader[4] = "MW";
                AGCHeader[5] = i;
                AGCHeader[6] = 0;
                AGCHeader[7] = "AGC" + "_" + "Tie Flow" + "_" + _String.valueOf(AGCTemp.getAGCAreaNum());
                AGCHeader[8] = _String.valueOf(AGCTemp.getAGCAreaNum());
                resultHeader.add(AGCHeader);

                // ACE 
                AGCHeader = new Object[9];
                AGCHeader[0] = "AGC";
                AGCHeader[1] = AGCTemp.getAGCAreaNum();
                AGCHeader[2] = "";
                AGCHeader[3] = "ACE";
                AGCHeader[4] = "MW";
                AGCHeader[5] = i;
                AGCHeader[6] = 0;
                AGCHeader[7] = "AGC" + "_" + "ACE" + "_" + _String.valueOf(AGCTemp.getAGCAreaNum());
                AGCHeader[8] = _String.valueOf(AGCTemp.getAGCAreaNum());
                resultHeader.add(AGCHeader);

                // Total AGC generator MW output reference 
                AGCHeader = new Object[9];
                AGCHeader[0] = "AGC";
                AGCHeader[1] = AGCTemp.getAGCAreaNum();
                AGCHeader[2] = "";
                AGCHeader[3] = "P_RefTotal";
                AGCHeader[4] = "MW";
                AGCHeader[5] = i;
                AGCHeader[6] = 0;
                AGCHeader[7] = "AGC" + "_" + "P_RefTotal" + "_" + _String.valueOf(AGCTemp.getAGCAreaNum());
                AGCHeader[8] = _String.valueOf(AGCTemp.getAGCAreaNum());
                resultHeader.add(AGCHeader);

                // CPS1 
                AGCHeader = new Object[9];
                AGCHeader[0] = "AGC";
                AGCHeader[1] = AGCTemp.getAGCAreaNum();
                AGCHeader[2] = "";
                AGCHeader[3] = "CPS1";
                AGCHeader[4] = "%";
                AGCHeader[5] = i;
                AGCHeader[6] = 0;
                AGCHeader[7] = "AGC" + "_" + "CPS1" + "_" + _String.valueOf(AGCTemp.getAGCAreaNum());
                AGCHeader[8] = _String.valueOf(AGCTemp.getAGCAreaNum());
                resultHeader.add(AGCHeader);

                // CPS2 
                AGCHeader = new Object[9];
                AGCHeader[0] = "AGC";
                AGCHeader[1] = AGCTemp.getAGCAreaNum();
                AGCHeader[2] = "";
                AGCHeader[3] = "CPS2";
                AGCHeader[4] = "%";
                AGCHeader[5] = i;
                AGCHeader[6] = 0;
                AGCHeader[7] = "AGC" + "_" + "CPS2" + "_" + _String.valueOf(AGCTemp.getAGCAreaNum());
                AGCHeader[8] = _String.valueOf(AGCTemp.getAGCAreaNum());
                resultHeader.add(AGCHeader);
            }

            // bus related output  
            for (int i = 0; i < dynProc.busList.size(); i++)
            {
                bus busTemp = dynProc.busList.get(i);

                // bus voltage 
                if (busTemp.IDE != 4 && busTemp.bHasDynLoad)
                {
                    Object[] busHeader = new Object[9];
                    busHeader[0] = "Bus";
                    busHeader[1] = busTemp.I;
                    busHeader[2] = "";
                    busHeader[3] = "Voltage";
                    busHeader[4] = "pu";
                    busHeader[5] = i; // index  
                    busHeader[6] = 0;
                    busHeader[7] = "Bus" + "_" + "Voltage" + "_" + _String.valueOf(busTemp.I);
                    busHeader[8] = _String.valueOf(busTemp.I);
                    resultHeader.add(busHeader);
                }

                /*
                // bus phase angle 
                if (busTemp.IDE!=4 && busTemp.bDynSimMon) {
                    Object[] busHeader = new Object[9];
                    busHeader[0] = "Bus"; 
                    busHeader[1] = busTemp.I; 
                    busHeader[2] = ""; 
                    busHeader[3] = "Angle"; 
                    busHeader[4] = "rad"; 
                    busHeader[5] =  i;													// index  
                    busHeader[6] =  0;
                    busHeader[7] = "Bus" + "_" + "Angle" + "_" + _String.valueOf(busTemp.I);
                    busHeader[8] = _String.valueOf(busTemp.I); 
                    resultHeader.add(busHeader);
                }
                */

                // bus frequency 
                if (busTemp.IDE != 4 && busTemp.bHasFreqMeasure)
                {
                    Object[] busHeader = new Object[9];
                    busHeader[0] = "Bus";
                    busHeader[1] = busTemp.I;
                    busHeader[2] = "";
                    busHeader[3] = "Freq";
                    busHeader[4] = "pu";
                    busHeader[5] = i; // index  
                    busHeader[6] = 0;
                    busHeader[7] = "Bus" + "_" + "Freq" + "_" + _String.valueOf(busTemp.I);
                    busHeader[8] = _String.valueOf(busTemp.I);
                    resultHeader.add(busHeader);
                }

                // bus power consumption (PLoad in MW)
                if (busTemp.IDE != 4 && busTemp.bHasDynLoad)
                {
                    Object[] busHeader = new Object[9];
                    busHeader[0] = "Bus";
                    busHeader[1] = busTemp.I;
                    busHeader[2] = "";
                    busHeader[3] = "PLoad";
                    busHeader[4] = "MW";
                    busHeader[5] = i;
                    busHeader[6] = 0;
                    busHeader[7] = "Bus" + "_" + "PLoad" + "_" + _String.valueOf(busTemp.I);
                    busHeader[8] = _String.valueOf(busTemp.I);
                    resultHeader.add(busHeader);
                }

                // bus power consumption (QLoad in MVar)
                if (busTemp.IDE != 4 && busTemp.bHasDynLoad)
                {
                    Object[] busHeader = new Object[9];
                    busHeader[0] = "Bus";
                    busHeader[1] = busTemp.I;
                    busHeader[2] = "";
                    busHeader[3] = "QLoad";
                    busHeader[4] = "MVar";
                    busHeader[5] = i;
                    busHeader[6] = 0;
                    busHeader[7] = "Bus" + "_" + "QLoad" + "_" + _String.valueOf(busTemp.I);
                    busHeader[8] = _String.valueOf(busTemp.I);
                    resultHeader.add(busHeader);
                }

                // Wind power injection 
                if (busTemp.IDE != 4 && busTemp.bWindMWEnable)
                {
                    Object[] busHeader = new Object[9];
                    busHeader[0] = "Wind";
                    busHeader[1] = busTemp.I;
                    busHeader[2] = "";
                    busHeader[3] = "Injection";
                    busHeader[4] = "MW";
                    busHeader[5] = i;
                    busHeader[6] = 0;
                    busHeader[7] = "Wind" + "_" + "Injection" + "_" + _String.valueOf(busTemp.I);
                    busHeader[8] = _String.valueOf(busTemp.I);
                    resultHeader.add(busHeader);
                }
            }
            CustomMessageHandler.println("Total of " + resultHeader.size() + " variables are stored for output!");
        }


        // insert simulation results
        public void insert(double t)
        {

            // ---------------- store output variables (update time instant) ----------------// 
            timeMat.add(t);

            double[] resultTemp = new double[resultHeader.size()];
            for (int i = 0; i < resultHeader.size(); i++)
            {
                String typeTemp = (String) resultHeader.get(i)[0];
                String varTemp = (String) resultHeader.get(i)[3];

                if (typeTemp._equalsIgnoreCase("Generator"))
                {
                    bus busTemp = dynProc.busList.get((int) resultHeader.get(i)[5]);

                    if (varTemp._equalsIgnoreCase("Speed"))
                    {
                        resultTemp[i] = busTemp.busGens.get((int) resultHeader.get(i)[6]).genDyn.getOmega();
                    }
                    else if (varTemp._equalsIgnoreCase("PGen"))
                    {
                        resultTemp[i] = busTemp.busGens.get((int) resultHeader.get(i)[6]).genDyn.getPegen() *
                                        dynProc.setSBASE;
                    }
                    else if (varTemp._equalsIgnoreCase("QGen"))
                    {
                        resultTemp[i] = busTemp.busGens.get((int) resultHeader.get(i)[6]).genDyn.getQegen() *
                                        dynProc.setSBASE;
                    }
                    else if (varTemp._equalsIgnoreCase("Vt"))
                    {
                        resultTemp[i] = busTemp.busGens.get((int) resultHeader.get(i)[6]).genDyn.getVt();
                    }
                    else if (varTemp._equalsIgnoreCase("Efd"))
                    {
                        resultTemp[i] = busTemp.busGens.get((int) resultHeader.get(i)[6]).genDyn.getEfd();
                    }
                    else if (varTemp._equalsIgnoreCase("PGenSet"))
                    {
                        resultTemp[i] = busTemp.busGens.get((int) resultHeader.get(i)[6]).getMWSetting() *
                                        dynProc.setSBASE;
                        //resultTemp[i] = busTemp.busGens.get((int) resultHeader.get(i)[6]).excDyn.getEfd(); 
                        //resultTemp[i] = busTemp.busGens.get((int) resultHeader.get(i)[6]).genDyn.getPm0()*setSBASE; 
                    }

                }
                else if (typeTemp._equalsIgnoreCase("AGC"))
                {
                    if (varTemp._equalsIgnoreCase("Freq"))
                    {
                        resultTemp[i] = dynProc.AGCList.get((int) resultHeader.get(i)[5]).get_areaFreq();
                    }
                    else if (varTemp._equalsIgnoreCase("Tie Flow"))
                    {
                        resultTemp[i] = dynProc.AGCList.get((int) resultHeader.get(i)[5]).get_tieMWFlow() *
                                        dynProc.setSBASE;
                    }
                    else if (varTemp._equalsIgnoreCase("ACE"))
                    {
                        resultTemp[i] = dynProc.AGCList.get((int) resultHeader.get(i)[5]).get_ACE();
                    }
                    else if (varTemp._equalsIgnoreCase("P_RefTotal"))
                    {
                        resultTemp[i] = dynProc.AGCList.get((int) resultHeader.get(i)[5]).get_pRefTotal() *
                                        dynProc.setSBASE;
                    }
                    else if (varTemp._equalsIgnoreCase("CPS1"))
                    {
                        resultTemp[i] = dynProc.AGCList.get((int) resultHeader.get(i)[5]).get_CPS1();
                    }
                    else if (varTemp._equalsIgnoreCase("CPS2"))
                    {
                        resultTemp[i] = dynProc.AGCList.get((int) resultHeader.get(i)[5]).get_CPS2();
                    }

                }
                else if (typeTemp._equalsIgnoreCase("Bus"))
                {
                    if (varTemp._equalsIgnoreCase("Voltage"))
                    {
                        resultTemp[i] = dynProc.busList.get((int) resultHeader.get(i)[5]).getVolt(dynProc.yVector);
                    }
                    //else if (varTemp._equalsIgnoreCase("Angle")) {
                    //	resultTemp[i] = dynProc.busList.get((int) resultHeader.get(i)[5]).getAngle(dynProc.yVector);
                    //}
                    else if (varTemp._equalsIgnoreCase("Freq"))
                    {
                        resultTemp[i] = dynProc.busList.get((int) resultHeader.get(i)[5]).busFreq;
                    }
                    else if (varTemp._equalsIgnoreCase("PLoad"))
                    {
                        resultTemp[i] = dynProc.busList.get((int) resultHeader.get(i)[5]).dynLoad.getDynPInj() *
                                        dynProc.setSBASE;
                    }
                    else if (varTemp._equalsIgnoreCase("QLoad"))
                    {
                        resultTemp[i] = dynProc.busList.get((int) resultHeader.get(i)[5]).dynLoad.getDynQInj() *
                                        dynProc.setSBASE;
                    }
                }
                else if (typeTemp._equalsIgnoreCase("Wind"))
                {
                    if (varTemp._equalsIgnoreCase("Injection"))
                    {
                        resultTemp[i] = dynProc.busList.get((int) resultHeader.get(i)[5]).windModel.getCurrentMW() *
                                        dynProc.setSBASE;
                    }
                }
            }
            resultMat.add(resultTemp);
        }


        // save results to a specified output file 
        public void exportResult(String fileName)
        {
            try
            {
                PrintWriter writer = new PrintWriter(new BufferedWriter(new FileWriter(fileName)));

                String printStr = "Time" + ",";
                for (int i = 0; i < resultHeader.size(); i++)
                {
                    printStr = printStr + (resultHeader.get(i)[0] + "_" + resultHeader.get(i)[3] + "_" +
                                           resultHeader.get(i)[8]) + ",";
                }
                writer.printf(printStr + "\n");

                // print out simulation results 
                for (int j = 0; j < timeMat.size(); j++)
                {
                    printStr = _String.format("%2.5f", timeMat.get(j)) + ",";
                    for (int i = 0; i < resultHeader.size(); i++)
                    {
                        printStr = printStr + _String.format("%2.4f", resultMat.get(j)[i]) + ",";
                    }
                    printStr = printStr + "\n";
                    writer.printf(printStr);
                }
                CustomMessageHandler.println("Info... Results are exported to " + fileName);

            }
            catch (IOException e)
            {
                throw new simException("Error: cannot export result to " + fileName);
            }
        }




        //result the buffer to reload result from t=0; 
        public void reset()
        {
            timeMat = new List<Double>(); // store the time instant
            resultMat = new List<double[]>();
        }

    }
}