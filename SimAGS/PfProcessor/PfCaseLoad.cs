﻿using java.io;
using SimAGS.Components;
using SimAGS.Handlers;
using System;
using System.Collections.Generic;

namespace SimAGS.PfProcessor
{
    public class PFCaseLoad
    {
        public const double DEFAULT_VMAX = 1.5;
        public const double DEFAULT_VMIN = 0.5;

        public List<bus> busArrayList;
        public List<branch> branchArrayList;
        public List<twoWindTrans> twoWindTransArrayList;
        public List<threeWindTrans> thrWindTransArrayList;
        public List<area> areaArrayList;
        public List<zone> zoneArrayList;
        public List<owner> ownerArrayList;

        public int nGen;
        public int nLoad;
        public int nSWShunt;
        public bool bGoodCase;

        public PFCase mainCase;

        // constructor 
        public PFCaseLoad(PFCase inputCase)
        {
            nGen = 0;
            nLoad = 0;
            nSWShunt = 0;
            bGoodCase = false;

            mainCase = inputCase;
            busArrayList = mainCase.busArrayList;
            branchArrayList = mainCase.branchArrayList;
            twoWindTransArrayList = mainCase.twoWindTransArrayList;
            thrWindTransArrayList = mainCase.thrWindTransArrayList;
            areaArrayList = mainCase.areaArrayList;
            zoneArrayList = mainCase.zoneArrayList;
            ownerArrayList = mainCase.ownerArrayList;
        }

        // load power flow data from raw file 

        public void exec(File pfFile)
        {
            bus busTemp;
            String dataLine = "";

            int sectionNum = 0;

            try
            {
                BufferedReader din = new BufferedReader(new FileReader(pfFile.getAbsolutePath()));

                // Case information (Line 1-3) 
                for (int i = 0; i < 3; i++)
                {
                    din.readLine();
                }

                //valid data section 
                while ((dataLine = din.readLine()) != null)
                {
                    dataLine = dataLine.trim();

                    if (dataLine.startsWith("0"))
                    {                       // beginning of new data section 
                        sectionNum++;

                    }
                    else if (!dataLine.startsWith("//") && !dataLine.isEmpty())
                    {

                        if (sectionNum == 0)
                        {                           // bus data section	 	    		
                            busArrayList.add(new bus(dataLine));

                        }
                        else if (sectionNum == 1)
                        {                   // load data section (Stored to corresponding buses) 
                            load loadModel = new load(dataLine);
                            if ((busTemp = dataProcess.getBusAt(loadModel.I, busArrayList)) != null)
                            {
                                busTemp.addLoad(loadModel);
                                nLoad++;
                            }
                            else
                            {                                   // load bus doesn't exist
                                CustomMessageHandler.Show("Warning: Load [" + loadModel.I + "_" + loadModel.ID + "] can't be loaded in PF case!");
                            }

                        }
                        else if (sectionNum == 2)
                        {                   // generator data section (stored to corresponding buses)
                            gen genModel = new gen(dataLine);
                            if ((busTemp = dataProcess.getBusAt(genModel.I, busArrayList)) != null)
                            {
                                busTemp.addGen(genModel);
                                nGen++;
                            }
                            else
                            {
                                CustomMessageHandler.Show("Warning: generator [" + genModel.I + "_" + genModel.ID + "] can't be loaded in PF case!");
                            }

                        }
                        else if (sectionNum == 3)
                        {                   // branch data section 
                            branchArrayList.add(new branch(dataLine));

                        }
                        else if (sectionNum == 4)
                        {                   // transformer data section
                            String Line1, Line2, Line3, Line4, Line5;

                            String[] dataEntry = dataProcess.getDataFields(dataLine, ",");
                            if (Integer.parseInt(dataEntry[2]) == 0)
                            {   // two-winding transformers
                                Line1 = dataLine;
                                Line2 = din.readLine();
                                Line3 = din.readLine();
                                Line4 = din.readLine();
                                twoWindTransArrayList.add(new twoWindTrans(Line1, Line2, Line3, Line4));

                            }
                            else
                            {                                   // three-winding transformers
                                Line1 = dataLine;
                                Line2 = din.readLine();
                                Line3 = din.readLine();
                                Line4 = din.readLine();
                                Line5 = din.readLine();
                                thrWindTransArrayList.add(new threeWindTrans(Line1, Line2, Line3, Line4, Line5));
                            }

                        }
                        else if (sectionNum == 5)
                        {                   // area data section
                            areaArrayList.add(new area(dataLine));

                        }
                        else if (sectionNum == 6)
                        {                   // two-terminal DC data section
                                            // DO NOTHING 

                        }
                        else if (sectionNum == 7)
                        {                   // vsc-hvdc data section
                                            // DO NOTHING  

                        }
                        else if (sectionNum == 8)
                        {                   // switchable shunt data section
                            swshunt swshuntModel = new swshunt(dataLine);
                            if ((busTemp = dataProcess.getBusAt(swshuntModel.I, busArrayList)) != null)
                            {
                                busTemp.addSwShunt(swshuntModel);
                                nSWShunt++;
                            }
                            else
                            {
                                CustomMessageHandler.Show("Warning:SW Shunt [" + swshuntModel.I + "] can't be loaded in PF case!");
                            }

                        }
                        else if (sectionNum == 9)
                        {                   // transformer impedance correction data
                                            // DO NOTHING

                        }
                        else if (sectionNum == 10)
                        {                   // multi-terminal DC data section
                                            // DO NOTHING  

                        }
                        else if (sectionNum == 11)
                        {                   // multi-segement line data section
                                            // DO NOTHING  

                        }
                        else if (sectionNum == 12)
                        {                   // zone data section
                            zoneArrayList.add(new zone(dataLine));

                        }
                        else if (sectionNum == 13)
                        {                   // interchange transfer data section
                                            // DO NOTHING 

                        }
                        else if (sectionNum == 14)
                        {                   // owner data section 
                            ownerArrayList.add(new owner(dataLine));

                        }
                        else if (sectionNum == 15)
                        {                   // FACT data section 
                                            // DO NOTHING 
                        }
                    }
                }
                din.close();

                mainCase.bCaseCheck = checkDataConsistency();

                // --------------- load case ---------------------//
                mainCase.nBus = busArrayList.size();
                mainCase.nBranch = branchArrayList.size();
                mainCase.nGen = nGen;
                mainCase.nLoad = nLoad;
                mainCase.nTwoWindXfrm = twoWindTransArrayList.size();
                mainCase.nThrWindXfrm = thrWindTransArrayList.size();
                mainCase.nArea = areaArrayList.size();
                mainCase.nSWShunt = nSWShunt;
                mainCase.nZone = zoneArrayList.size();
                mainCase.nOwner = ownerArrayList.size();

                CustomMessageHandler.Show(">>>>>>>> Process.... Case data is loaded!");

                #region Data results

                var dataList = new List<string>();

                dataList.Add("           Bus = " + mainCase.nBus);
                dataList.Add("          Load = " + mainCase.nLoad);
                dataList.Add("           Gen = " + mainCase.nGen);
                dataList.Add("        Branch = " + mainCase.nBranch);
                dataList.Add("2W Transformer = " + mainCase.nTwoWindXfrm);
                dataList.Add("3W Transformer = " + mainCase.nThrWindXfrm);
                dataList.Add("          Area = " + mainCase.nArea);
                dataList.Add("      SW Shunt = " + mainCase.nSWShunt);
                dataList.Add("          Zone = " + mainCase.nZone);
                dataList.Add("         Owner = " + mainCase.nOwner);

                CustomMessageHandler.Show(string.Join("\n", dataList));
                #endregion
            }
            catch (IOException e)
            {
                throw new simException("Error: check erros in power flow file", e);
            }
        }

        public bool checkDataConsistency()
        {
            String errMsg = "";

            // [1] aggregate generators connected to the same bus 
            foreach (var busTemp in busArrayList)
            {
                if ((errMsg = busTemp.setup()) != null)
                {
                    throw new Exception(errMsg);
                }
            }

            // check other data for inconsistency (pending)
            return true;
        }




    }
}