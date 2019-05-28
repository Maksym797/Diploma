using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ikvm.extensions;
using java.io;
using SimAGS.Components;
using SimAGS.DistEvent;
using SimAGS.DynModels.AgcModel;
using SimAGS.DynModels.GenModels;
using SimAGS.DynModels.MonModels;
using SimAGS.Handlers;

namespace SimAGS.DynProcessor
{
    public class DynCaseLoad
    {
        public int numState;
        public int numAlge;

        public int nGENCLS;
        public int nGENROU;
        public int nESDC1A;
        public int nESAC4A;
        public int nTGOV1;
        public int nHYGOV;
        public int nIEEEG2;
        public int nFreqBus;
        public int nMonBran;
        public int nDynLoadUDM;
        public int nAGC;
        public int nWindDyn;
        public int nEvent;
        public double SBASE;

        public List<bus> busList;
        public List<branch> branchList;
        public List<agcModel> AGCList;
        public distEventList eventList;

        public DynCase mainDynCase;

        //constructor 
        public DynCaseLoad(DynCase inputCase)
        {

            numState = 0;
            numAlge = 2 * inputCase.pfCase.nYBus;
            nGENCLS = 0;
            nGENROU = 0;
            nESDC1A = 0;
            nESAC4A = 0;
            nTGOV1 = 0;
            nHYGOV = 0;
            nIEEEG2 = 0;
            nFreqBus = 0;
            nAGC = 0;
            nWindDyn = 0;
            nEvent = 0;
            nDynLoadUDM = 0;

            mainDynCase = inputCase;
            busList = inputCase.busList;
            branchList = inputCase.branchList;
            eventList = inputCase.dynEventList;
            AGCList = inputCase.AGCList;
        }


        // load dynamic data, AGC data, wind data, and cont data 
        public void exec(File DynDataFile, File AGCDataFile, File windDataFile, File contDataFile, File genSchdDataFile,
            File loadScdDataFile)
        {
            if (DynDataFile != null) loadDynFile(DynDataFile);
            if (AGCDataFile != null) loadAGCFile(AGCDataFile);
            if (windDataFile != null) loadWindFile(windDataFile);
            if (contDataFile != null) loadContFile(contDataFile);
            if (genSchdDataFile != null) loadGenHourlySchd(genSchdDataFile);
            if (loadScdDataFile != null) loadLoadHourlySchd(loadScdDataFile);

            mainDynCase.numState = numState;
            mainDynCase.numAlgeVar = numAlge;
            mainDynCase.nGENCLS = nGENCLS;
            mainDynCase.nGENROU = nGENROU;
            mainDynCase.nESDC1A = nESDC1A;
            mainDynCase.nESAC4A = nESAC4A;
            mainDynCase.nTGOV1 = nTGOV1;
            mainDynCase.nHYGOV = nHYGOV;
            mainDynCase.nIEEEG2 = nIEEEG2;
            mainDynCase.nFreqBus = nFreqBus;
            mainDynCase.nDynLoadUDM = nDynLoadUDM;
            mainDynCase.nAGC = nAGC;
            mainDynCase.nWindDyn = nWindDyn;
            mainDynCase.nEvent = nEvent;
        }



        // ------------------------ load generator dynamic data -----------------------//
        public void loadDynFile(File dynDataFile) //throws simException
        {
            String dataLine = "";
            try
            {
                BufferedReader din = new BufferedReader(new FileReader(dynDataFile.getAbsolutePath()));

                while ((dataLine = din.readLine()) != null)
                {
                    dataLine = dataLine.trim();
                    if (!dataLine.startsWith("//") && !dataLine.isEmpty())
                    {
                        // not comments lines 
                        if (dataLine.contains("/"))
                        {
                            dataLine = dataLine.substring(0, dataLine.indexOf("/")).trim();
                        }

                        // locate generators
                        String[] token = dataProcess.getDataFields(dataLine, ", \t");

                        // add dynamic model to the generator array list
                        addDynModel(token);
                    }
                }

                din.close();
                //CustomMessageHandler.println("============= Total of " + numState + " state variables and " + numAlge + " algebraic variable have been loaded ==============");  
            }
            catch (IOException e)
            {
                throw new simException("Error: cannot open dyn file at " + dynDataFile);
            }
        }


        // add model 
        public void addDynModel(String[] token)
        {

            String type = token[1].substring(1, token[1].lastIndexOf("'"));
            int busNum = Integer.parseInt(token[0]);

            // ~~~~~~~~~~~~~~~~~~~ generator models ~~~~~~~~~~~~~~~~~~~~~~~~~~~//
            if (type.equalsIgnoreCase("GENCLS"))
            {
                GENCLS loadedModel = new GENCLS(findGenAt(busNum, token[2]), token, numAlge, numState);
                numAlge = loadedModel.last_AlgeVar_Pos;
                numState = loadedModel.last_StateVar_Pos;
                nGENCLS++;
            }

            if (type.equalsIgnoreCase("GENROU"))
            {
                GENROU loadedModel = new GENROU(findGenAt(busNum, token[2]), token, numAlge, numState);
                numAlge = loadedModel.last_AlgeVar_Pos;
                numState = loadedModel.last_StateVar_Pos;
                nGENROU++;
            }

            // exciter models 
            // if (type.equalsIgnoreCase("ESDC1A"))
            // {
            //     ESDC1A loadedModel = new ESDC1A(findGenAt(busNum, token[2]), token, numAlge, numState);
            //     numAlge = loadedModel.last_AlgeVar_Pos;
            //     numState = loadedModel.last_StateVar_Pos;
            //     nESDC1A++;
            // }
            // 
            // if (type.equalsIgnoreCase("ESAC4A"))
            // {
            //     ESAC4A loadedModel = new ESAC4A(findGenAt(busNum, token[2]), token, numAlge, numState);
            //     numAlge = loadedModel.last_AlgeVar_Pos;
            //     numState = loadedModel.last_StateVar_Pos;
            //     nESAC4A++;
            // }
            // 
            // // ~~~~~~~~~~~~~~~~~~~~~~~~~ governor models ~~~~~~~~~~~~~~~~~~~~~~~~//  
            // if (type.equalsIgnoreCase("TGOV1"))
            // {
            //     TGOV1 loadedModel = new TGOV1(findGenAt(busNum, token[2]), token, numAlge, numState);
            //     numAlge = loadedModel.last_AlgeVar_Pos;
            //     numState = loadedModel.last_StateVar_Pos;
            //     nTGOV1++;
            // }
            // 
            // if (type.equalsIgnoreCase("HYGOV"))
            // {
            //     HYGOV loadedModel = new HYGOV(findGenAt(busNum, token[2]), token, numAlge, numState);
            //     numAlge = loadedModel.last_AlgeVar_Pos;
            //     numState = loadedModel.last_StateVar_Pos;
            //     nHYGOV++;
            // }
            // 
            // 
            // if (type.equalsIgnoreCase("IEEEG2"))
            // {
            //     IEEEG2 loadedModel = new IEEEG2(findGenAt(busNum, token[2]), token, numAlge, numState);
            //     numAlge = loadedModel.last_AlgeVar_Pos;
            //     numState = loadedModel.last_StateVar_Pos;
            //     nIEEEG2++;
            // }
            // 
            // 
            // // ~~~~~~~~~~~~~~~~~~~~~ dynamic data load data ~~~~~~~~~~~~~~~~~~~~~~~// 
            // if (type.equalsIgnoreCase("DYNLOADUDM"))
            // {
            //     DynLoadUDM loadedModel = new DynLoadUDM(findBusAt(busNum), token, numAlge, numState);
            //     numAlge = loadedModel.last_AlgeVar_Pos;
            //     numState = loadedModel.last_StateVar_Pos;
            //     nDynLoadUDM++;
            // }
            // 
            // // ~~~~~~~~~~~~~~~~~ bus frequency measurement data ~~~~~~~~~~~~~~~~~~~~//
            // if (type.equalsIgnoreCase("FREQBUS"))
            // {
            //     if (findBusAt(busNum).bHasFreqMeasure == false)
            //     {
            //         BUSFREQ loadedModel = new BUSFREQ(findBusAt(busNum), numAlge, numState);
            //         numAlge = loadedModel.last_AlgeVar_Pos;
            //         numState = loadedModel.last_StateVar_Pos;
            //         nFreqBus++;
            //     }
            //     else
            //     {
            //         CustomMessageHandler.println("[Warning] FREQ Measurement model at bus " + busNum +
            //                                      " is skipped because it is already created by other models");
            //     }
            // }

            // ~~~~~~~~~~~~~~~~~ branch MW flow measurement data ~~~~~~~~~~~~~~~~~~~//
            if (type.equalsIgnoreCase("BRANMON"))
            {
                branch branTemp;
                int frBusNum = Integer.parseInt(token[2]);
                int toBusNum = Integer.parseInt(token[3]);
                String branID = token[4].substring(1, token[4].lastIndexOf("'"));

                if ((branTemp = findBranchAt(frBusNum, toBusNum, branID)) != null)
                {
                    if (branTemp.bHasMWFlowMeaseure == false)
                    {
                        BRANFLOW loadedModel = new BRANFLOW(branTemp, numAlge, numState);
                        numAlge = loadedModel.last_AlgeVar_Pos;
                        numState = loadedModel.last_StateVar_Pos;
                        nMonBran++;
                    }
                    else
                    {
                        CustomMessageHandler.println("[Warning] MW Flow Measurement model at branch [" + frBusNum +
                                                     "," + toBusNum + "," + branID +
                                                     " ] is skipped as it is already created!");
                    }

                }
            }
        }

        // ----------------------- load conversion for dynamic simulation ---------------//

        public void dynLoadCov(bool bEnableLoadConv, bool bEnableLoaFreq, double loadConvZP_Pct, double loadConvIP_Pct,
            double loadConvPP_Pct, double loadConvZQ_Pct, double loadConvIQ_Pct, double loadConvPQ_Pct,
            double loadP_FreqCoef, double loadQ_FreqCoef)
        {
            // foreach (bus busTemp in busList)
            // {
            //     if (busTemp.bHasLoad)
            //     {
            //         if (bEnableLoaFreq == true)
            //         {
            //             // check if the bus frequency monitor is applied 
            //             if (!busTemp.bHasFreqMeasure)
            //             {
            //                 CustomMessageHandler.println("[Info]: bus freq measurement moduel is added at " +
            //                                              busTemp.I + " to model freq-dependent load");
            //                 BUSFREQ loadedModel = new BUSFREQ(findBusAt(busTemp.I), numAlge, numState);
            //                 numAlge = loadedModel.last_AlgeVar_Pos;
            //                 numState = loadedModel.last_StateVar_Pos;
            //                 nFreqBus++;
            //             }
            //         }
            // 
            //         // create dynamic load will be assigned to load bus 
            //         DynLoadZIPFreq dynLoadTemp = new DynLoadZIPFreq(busTemp, numAlge, numState);
            //         dynLoadTemp.setLoadZIPComposite(bEnableLoadConv, loadConvZP_Pct, loadConvIP_Pct, loadConvPP_Pct,
            //             loadConvZQ_Pct, loadConvIQ_Pct, loadConvPQ_Pct); // ZIP component share (%)
            //         dynLoadTemp.setLoadFreqCompsite(bEnableLoaFreq, loadP_FreqCoef, loadQ_FreqCoef);
            //         numAlge = dynLoadTemp.last_AlgeVar_Pos;
            //         numState = dynLoadTemp.last_StateVar_Pos;
            //     }
            // }
        }



        /*
         * ------------------------- Load AGC dynamic data -------------------------------//
         */
        public void loadAGCFile(File AGCDataFile) //throws simException
        {

            bool bDiscreteAGC = true; // load model as discreteAGC
            String dataLine = "";
            int sectionNum = -999;
            agcModel AGCTemp = null;

            try
            {
                BufferedReader din = new BufferedReader(new FileReader(AGCDataFile.getAbsolutePath()));

                while ((dataLine = din.readLine()) != null)
                {
                    dataLine = dataLine.trim();
                    if (!dataLine.startsWith("/") && !dataLine.isEmpty())
                    {
                        // not comments lines 
                        if (dataLine.contains("/"))
                        {
                            dataLine = dataLine.substring(0, dataLine.indexOf("/")).trim();
                        }

                        // check the start of the AGC area (for multiple areas) 
                        if (dataLine.startsWith("AGC_AREA"))
                        {
                            sectionNum = 0;
                        }

                        if (dataLine.startsWith("0"))
                        {
                            // beginning of new data section 
                            sectionNum++;

                        }
                        else
                        {

                            // read in data 
                            String[] token = dataProcess.getDataFields(dataLine, ",");
                            if (sectionNum == 0)
                            {
                                // create AGC object 			    			
                                if (bDiscreteAGC)
                                {
                                    AGCTemp = new dAGCDyn(token, numAlge, numState);
                                    numAlge = ((dAGCDyn) AGCTemp).last_AlgeVar_Pos;
                                    numState = ((dAGCDyn) AGCTemp).last_StateVar_Pos;
                                    AGCList.add(AGCTemp);
                                    eventList.addEvent(new actAGC(4, AGCTemp));
                                    nAGC++;
                                }
                                else
                                {
                                    // AGCTemp = new AGCDyn(token, numAlge, numState);
                                    // numAlge = ((AGCDyn) AGCTemp).last_AlgeVar_Pos;
                                    // numState = ((AGCDyn) AGCTemp).last_StateVar_Pos;
                                    // AGCList.add(AGCTemp);
                                    // eventList.addEvent(new actAGC(4, AGCTemp));
                                    // nAGC++;
                                }

                            }
                            else if (sectionNum == 1)
                            {
                                // add bus frequency measurement 
                                int busNum = Integer.parseInt(token[0]);
                                double freqWeigt = Double.Parse(token[1]);
                                // check if the bus has frequency measurement 
                                bus busTemp = findBusAt(busNum);
                                if (busTemp != null)
                                {
                                    if (!busTemp.bHasFreqMeasure)
                                    {
                                        // todo BUSFREQ loadedModel = new BUSFREQ(busTemp, numAlge, numState);
                                        // todo numAlge = loadedModel.last_AlgeVar_Pos;
                                        // todo numState = loadedModel.last_StateVar_Pos;
                                        nFreqBus++;
                                    }
                                    AGCTemp.addBusForFreq(findBusAt(busNum), freqWeigt);
                                }
                                else
                                {
                                    CustomMessageHandler.println(
                                        "[Error] AGC includes bus at " + busNum + " is incorrect");
                                }


                            }
                            else if (sectionNum == 2)
                            {
                                // Line Data Section is Ignored
                                int frNum = Integer.parseInt(token[0]);
                                int toNum = Integer.parseInt(token[1]);
                                String id = token[2].substring(1, token[2].lastIndexOf("'"));
                                int measureSide = Integer.parseInt(token[3]);
                                // check if the line MW flow measurement is available 
                                branch branTemp = findBranchAt(frNum, toNum, id);
                                if (branTemp != null)
                                {
                                    if (!branTemp.bHasMWFlowMeaseure)
                                    {
                                        BRANFLOW loadedModel = new BRANFLOW(branTemp, numAlge, numState);
                                        numAlge = loadedModel.last_AlgeVar_Pos;
                                        numState = loadedModel.last_StateVar_Pos;
                                        nMonBran++;
                                    }
                                    AGCTemp.addTieLine(findBranchAt(frNum, toNum, id), measureSide);
                                }
                                else
                                {
                                    CustomMessageHandler.println(
                                        "[Error] AGC include branch [" + frNum + "," + toNum + "," + id +
                                        "] is incorrect");
                                }

                            }
                            else if (sectionNum == 3)
                            {
                                // add generator on AGC control 
                                int busNum = Integer.parseInt(token[0]);
                                String id = token[1].substring(1, token[1].lastIndexOf("'"));
                                double AGCWeight = Double.Parse(token[2]);
                                AGCTemp.addGenOnAGC(findGenAt(busNum, id), AGCWeight);

                            }
                            else
                            {
                                sectionNum = -999; // search for next AGC area definition 	
                                // reserved for future development
                            }
                        }
                    }
                }
                din.close();

            }
            catch (IOException e)
            {
                throw new simException("Error: cannot open AGC data file at " + AGCDataFile);
            }
        }


        //---------------------- Load hourly generator schedule data ----------------// 
        public void loadGenHourlySchd(File genHourlySchdFile) //throws simException
        {

            double tStart = 0.0;
            double tDataInterval = 0.0;
            double tCurrent = 0.0;
            String[]
                token;
            List<int> genNumList = new List<int>();
            List<String> genIDList = new List<String>();
            double genPeSet = 0.0;

            int secNum = 0;
            int genDataTotalNum = 0;
            String dataLine = "";

            try
            {
                BufferedReader din = new BufferedReader(new FileReader(genHourlySchdFile.getAbsolutePath()));
                while ((dataLine = din.readLine()) != null)
                {
                    dataLine = dataLine.trim();
                    if (!dataLine.startsWith("/") && !dataLine.isEmpty())
                    {
                        // not comments lines 
                        if (dataLine.contains("/"))
                        {
                            dataLine = dataLine.substring(0, dataLine.indexOf("/")).trim();
                        }

                        if (dataLine.startsWith("#"))
                        {
                            secNum++;
                        }
                        else
                        {

                            if (secNum == 1)
                            {
                                // general setting 
                                token = dataProcess.getDataFields(dataLine, ",");
                                tStart = Double.Parse(token[0]);
                                tDataInterval = Double.Parse(token[1]);
                                tCurrent = tStart; // initialize starting time instant 

                            }
                            else if (secNum == 2)
                            {
                                // generator info 
                                token = dataProcess.getDataFields(dataLine, ",");
                                genNumList.add(Integer.parseInt(token[0]));
                                genIDList.add(token[1]);
                                genDataTotalNum++;

                            }
                            else if (secNum == 3)
                            {
                                token = dataProcess.getDataFields(dataLine, " ");
                                for (int i = 0; i < genDataTotalNum; i++)
                                {
                                    genPeSet = Double.Parse(token[i]);
                                    // todo eventList.addEvent(new setGenMW(tCurrent, genNumList.get(i), genIDList.get(i),
                                    // todo     genPeSet));
                                }
                                tCurrent += tDataInterval;
                            }
                        }
                    }
                }
                CustomMessageHandler.println("Total of " + genDataTotalNum +
                                             " generator hourly schedule data are loaded!");
            }
            catch (IOException e)
            {
                throw new simException("Error: cannot open loadGenHourlySchd data file at " + genHourlySchdFile);
            }
        }

        // -------------------------- Load load hourly schedule data ----------------// 
        public void loadLoadHourlySchd(File loadHourlySchdFile) //throws simException
        {
            double tStart = 0.0;
            double tDataInterval = 0.0;
            double tCurrent = 0.0;

            List<Integer> schdBusList = new List<Integer>();
            double loadSchdMWSet = 0.0;

            String[] token;
            int secNum = 0; // 0--> time info; 1--> generator data  
            int loadDataTotalNum = 0;


            String dataLine = "";
            try
            {
                BufferedReader din = new BufferedReader(new FileReader(loadHourlySchdFile.getAbsolutePath()));
                while ((dataLine = din.readLine()) != null)
                {
                    dataLine = dataLine.trim();
                    if (!dataLine.startsWith("/") && !dataLine.isEmpty())
                    {
                        // not comments lines 
                        if (dataLine.contains("/"))
                        {
                            dataLine = dataLine.substring(0, dataLine.indexOf("/")).trim();
                        }

                        if (dataLine.startsWith("#"))
                        {
                            secNum++;
                        }
                        else
                        {
                            if (secNum == 1)
                            {
                                // time info 
                                token = dataProcess.getDataFields(dataLine, ",");
                                tStart = Double.Parse(token[0]);
                                tDataInterval = Double.Parse(token[1]);
                                tCurrent = tStart;

                            }
                            else if (secNum == 2)
                            {
                                // load buses 
                                token = dataProcess.getDataFields(dataLine, ",");
                                // todo schdBusList.add(Integer.valueOf(token[0]));
                                loadDataTotalNum++;

                            }
                            else if (secNum == 3)
                            {
                                // sample data 
                                token = dataProcess.getDataFields(dataLine, " ");
                                for (int i = 0; i < loadDataTotalNum; i++)
                                {
                                    loadSchdMWSet = Double.Parse(token[i]);
                                    // todo eventList.addEvent(new schdLoad(tCurrent, schdBusList.get(i), loadSchdMWSet));
                                }
                                tCurrent += tDataInterval;
                            }
                        }
                    }
                }
                CustomMessageHandler.println("Total of " + loadDataTotalNum + " load hourly schedule data are loaded!");
            }
            catch (IOException e)
            {
                throw new simException("Error: cannot open loadLoadHourlySchd data file at " + loadHourlySchdFile);
            }
        }


        //---------------------------- Load hourly Wind Data --------------------------------//
        public void loadWindFile(File windDataFile) //throws simException
        {

            double tStart = 0.0;
            double tWindDataInterval = 0.0;
            double tCurrent = 0.0;
            double iniWPInj = 0.0; // initial power injection 

            //int busNum = 0; 
            int secNum = 0;
            int numSampleWindData = 0;
            bus injBus = null;
            List<bus> windBusList = new List<bus>();
            String[] token;

            String dataLine = "";
            try
            {
                BufferedReader din = new BufferedReader(new FileReader(windDataFile.getAbsolutePath()));
                while ((dataLine = din.readLine()) != null)
                {
                    dataLine = dataLine.trim();
                    if (!dataLine.startsWith("/") && !dataLine.isEmpty())
                    {
                        // not comments lines 
                        if (dataLine.contains("/"))
                        {
                            dataLine = dataLine.substring(0, dataLine.indexOf("/")).trim();
                        }

                        if (dataLine.startsWith("#"))
                        {
                            secNum++;
                        }
                        else
                        {
                            if (secNum == 1)
                            {
                                // General wind data setting 
                                token = dataProcess.getDataFields(dataLine, ",");
                                tStart = Double.Parse(token[0]);
                                tWindDataInterval = Double.Parse(token[1]);
                                tCurrent = tStart;
                            }
                            else if (secNum == 2)
                            {
                                // load wind bus and dynamic time constant
                                token = dataProcess.getDataFields(dataLine, ",");
                                // todo injBus = findBusAt(Integer.valueOf(token[0]));
                                if (injBus != null && injBus.IDE == 1)
                                {
                                    windBusList.add(injBus);
                                    // todo DYNWIND loadedModel = new DYNWIND(injBus, token, numAlge, numState);
                                    // todo numAlge = loadedModel.last_AlgeVar_Pos;
                                    // todo numState = loadedModel.last_StateVar_Pos;
                                    nWindDyn++;
                                    iniWPInj = Double.Parse(token[2]);
                                    CustomMessageHandler.println(
                                        "At t0, PW = " + iniWPInj + " MW is added at Bus " + injBus.I);
                                }
                                else
                                {
                                    CustomMessageHandler.println("ERROR: wind power injected bus is invalid!");
                                }
                            }
                            else if (secNum == 3)
                            {
                                // load wind data, windInj in MW (tread wind power injection at discrete event at specified time instants)
                                token = dataProcess.getDataFields(dataLine, " ");
                                // todo for (int i = 0; i < token.length; i++)
                                {
                                    // todo eventList.addEvent(new dispatchWind(tCurrent, windBusList.get(i).I,
                                    // todo     Double.valueOf(token[i]) / 100));
                                    // todo tCurrent += tWindDataInterval;
                                    numSampleWindData++;
                                }
                            }
                        }
                    }
                }
                CustomMessageHandler.println("Total of " + numSampleWindData + " sampled wind data are loaded!");
            }
            catch (IOException e)
            {
                throw new simException("Error: cannot open wind data file at " + windDataFile);
            }
        }


        //--------------------------- Load Contingency Data---------------------------------//
        public void loadContFile(File contDataFile) //throws simException
        {

            String dataLine = "";

            try
            {
                BufferedReader din = new BufferedReader(new FileReader(contDataFile.getAbsolutePath()));

                while ((dataLine = din.readLine()) != null)
                {
                    dataLine = dataLine.trim();
                    if (!dataLine.startsWith("/") && !dataLine.isEmpty())
                    {
                        // not comments lines 
                        if (dataLine.contains("/"))
                        {
                            dataLine = dataLine.substring(0, dataLine.indexOf("/")).trim();
                        }
                        CustomMessageHandler.println("cont data [" + dataLine + "] is loaded");

                        // todo if (dataLine.contains("trip") && dataLine.contains("generator"))
                        // todo {
                        // todo     tripGen genTripEvent = new tripGen(dataLine);
                        // todo     if (findGenAt(genTripEvent.genBusNum, genTripEvent.genID) != null)
                        // todo     {
                        // todo         // check if generator exists
                        // todo         eventList.addEvent(genTripEvent);
                        // todo         nEvent++;
                        // todo     }
                        // todo }
                        // todo else if (dataLine.contains("MW") && dataLine.contains("generator"))
                        // todo {
                        // todo     setGenMW setGenMWEvent = new setGenMW(dataLine);
                        // todo     if (findGenAt(setGenMWEvent.genBusNum, setGenMWEvent.genID) != null)
                        // todo     {
                        // todo         eventList.addEvent(setGenMWEvent);
                        // todo         nEvent++;
                        // todo     }
                        // todo }
                        // todo else if (dataLine.contains("MW") && dataLine.contains("load") && dataLine.contains("add"))
                        // todo {
                        // todo     setAddLoadMW setLoadMWEvent = new setAddLoadMW(dataLine);
                        // todo     eventList.addEvent(setLoadMWEvent);
                        // todo     nEvent++;
                        // todo }
                        // todo else if (dataLine.contains("fault") && dataLine.contains("bus") && dataLine.contains("cleared"))
                        // todo {
                        // todo     busFault busFaultEvent = new busFault(dataLine);
                        // todo     // convert busFaultEvent 
                        // todo     eventList.addEvent(busFaultEvent.applyFault());
                        // todo     eventList.addEvent(busFaultEvent.clearFault());
                        // todo     foreach (tripBranch tripBranchTemp in busFaultEvent.tripBranchEventList)
                        // todo     {
                        // todo         eventList.addEvent(tripBranchTemp);
                        // todo     }
                        // todo     nEvent++;
                        // todo }
                        // todo else if (dataLine.contains("load") && dataLine.contains("scale"))
                        // todo {
                        // todo     schdLoad schdLoadEvent = new schdLoad(dataLine);
                        // todo     eventList.addEvent(schdLoadEvent);
                        // todo     nEvent++;
                        // todo }
                        // todo // transmission line tripping event
                        // todo else if (dataLine.contains("trip branch"))
                        // todo {
                        // todo     tripBranch tripBranchEvent = new tripBranch(dataLine);
                        // todo     eventList.addEvent(tripBranchEvent);
                        // todo     nEvent++;
                        // todo }
                    }
                }

                din.close();
                CustomMessageHandler.println("Total number of event time is " + // todo  eventList.eventList.size() +
                                             " number of events " + nEvent);

            }
            catch (IOException e)
            {
                throw new simException("Error: cannot open contingency data file at " + contDataFile);
            }
        }


        //-------------------------------------------------------------

        // find generator based on gen bus number and ID 
        public gen findGenAt(int BusNum, String ID)
        {
            foreach (bus busTemp in busList)
            {
                foreach (gen genTemp in busTemp.busGens)
                {
                    if (genTemp.I == BusNum && genTemp.ID.equalsIgnoreCase(ID))
                    {
                        return genTemp;
                    }
                }
            }
            CustomMessageHandler.println("ERROR: generator(" + BusNum + "_" + ID + ") is not found!");
            return null;
        }


        public bus findBusAt(int BusNum)
        {
            foreach (bus busTemp in busList)
            {
                if (busTemp.I == BusNum)
                {
                    return busTemp;
                }
            }
            CustomMessageHandler.println("ERROR: bus(" + BusNum + ") is not found!");
            return null;
        }

        // get branch
        public branch findBranchAt(int frBus, int toBus, String id)
        {
            for (int i = 0; i < branchList.size(); i++)
            {
                branch branchTemp = branchList.get(i);
                if (branchTemp.I == frBus && branchTemp.J == toBus && branchTemp.CKT.equalsIgnoreCase(id))
                {
                    return branchTemp;
                }
                else if (branchTemp.J == frBus && branchTemp.I == toBus && branchTemp.CKT.equalsIgnoreCase(id))
                {
                    return branchTemp;
                }
            }
            return null;
        }


    }
}
