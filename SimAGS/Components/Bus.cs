using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using cern.colt.matrix;
using SimAGS.Components.ExtendOption;
using SimAGS.DynModels.DynLoadModels;
using SimAGS.DynModels.MonModels;
using SimAGS.DynModels.WindModels;
using SimAGS.Handlers;

namespace SimAGS.Components
{
    public class bus : abstractPfElement
    {
        // data loaded from raw file 
        public int I = 0;       // bus number 
        public String NAME = "";        // bus name 
        public double BASKV = 0;        // bus base voltage in kV
        public int IDE = 0;     // bus type (1 - load bus; 2 - gen bus; 3 - slack bus; 4- dis-connected bus)
        public double GL = 0.0;         // P component of shunt admittance to ground in MW at per unit 
        public double BL = 0.0;     // Q component of shunt admittance to ground in MVar (+ capacitor; - reactor)
        public int AREA = 0;        // area number
        public int ZONE = 0;        // zone number
        public double VM = 1.0;     // bus voltage magnitude [pu] 
        public double VA = 0.0;     // bus phase angle [deg]
        public int OWNER = 0;       // owner number

        public static int DATALENGTH = 11;  // default data line 

        // extended variables 
        public const double DEFAULT_VMAX = 1.5;                     // maximum voltage for PQ bus
        public const double DEFAULT_VMIN = 0.2;                     // minimum voltage for PQ bus

        // computed data section 
        public double volt = 1.0;                               // computed bus voltage after the inter-PQ loop is finished
        public double ang = 0.0;                                // computed phase angle after the inter-PQ loop is finished
        public double Pset = 0.0;                               // power injection = PG - PL 
        public double Qset = 0.0;                               // power injection = QG - QL 
        public int calcType = 0;                                // 1--> PQ bus; 2--> PV bus for PV <-> PQ conversion if necessary 

        public int vmagPos = 0;                                     // index for bus voltage in the pf solution vector
        public int vangPos = 0;                                     // index for bus angle in the pf solution vector
        public int yMatIndx = 0;                                    // position in the sortedList; 

        public List<bus> adjBusList = new List<bus>();    // store adjacent buses connected by activated branches 

        // add generators to bus (for multiple generators at the same bus)
        // assumption: all of the generator at the same bus regulating the same bus (either local or remote bus)
        public bool bHasGen = false;
        public List<gen> busGens = new List<gen>();       // all generators connected
        public double aggPGen = 0.0;                        // total power P gen 
        public double aggQGen = 0.0;                        // total power Q gen 
        public double aggQMax = 1E4;                        // Total Q Max only valid for PV bus; for PQ bus, it is set to  10000
        public double aggQMin = -1E4;                       // Total Q Min only valid for PV bus; for PQ bus, it is set to -10000
        public double aggQUpperMargin = 0.0;                        // total Qmax - Qgen
        public double aggQBottomMargin = 0.0;                       // total Qgen - Qmin 

        public bool bBusHasRegGen = false;
        public List<int> genRegBusNumList;                 // regulating bus list
        public List<double> regBusVoltSetList;             // regulating sitting list
        public double regBusVoltSet = 0.0;                          // targeting voltage magnitude
        public int genRegBusNum = 0;                            // generator regulated bus (all generators should regulate the same bus)
        public double genBusVoltSetCalc = 0.0;                      // generator terminal voltage setting (=1 regulating voltage is the terminal bus is regulated)		


        // add loads to bus (for multiple loads at the same bus)
        public bool bHasLoad = false;
        public List<load> busLoads = new List<load>();
        public double aggCPLoadP = 0.0;                         // total pure P load [pu]
        public double aggCPLoadQ = 0.0;                         // total pure Q load [pu]
        public double aggCCLoadP = 0.0;                         // total P of constant current load [pu]
        public double aggCCLoadQ = 0.0;                         // total Q of constant current load [pu] 
        public double aggCYLoadP = 0.0;                         // total P of constant impedance load [pu]
        public double aggCYLoadQ = 0.0;                         // total Q of constant impedance load [pu]

        public double PLoadTotalSet = 0.0;                          // total MW load at 1.0 [pu]
        public double QLoadTotalSet = 0.0;                          // total MVAR load at 1.0 [pu] 

        public double aggTotalPLoad = 0.0;                          // total MW load after power flow converged 
        public double aggTotalQLoad = 0.0;                          // total MWAR load after power flow converged

        //public bool bIncludeLoadInYMat = true;					// include constant-impedance component into yMatrix 

        // external PQ injection to bus (gen, load)
        public double extPInj = 0.0;
        public double extQInj = 0.0;

        // add switched shunts (for multiple shunts at the same bus)
        public bool bHasSwShunt = false;
        public List<swshunt> busSwShunts = new List<swshunt>();
        public int swshuntRegBusNum = 0;
        public double aggSWshuntBusBMax = 0;                        // total shunt BMax [pu]
        public double aggSWshuntBusBMin = 0;                        // total shunt BMin [pu]
        public double aggSWshuntBusBMargin = 0;                     // shunt margin [pu]
        public double swshuntCalcB = 0;

        // extended voltage regulation option 
        public BaseExtOption voltExtOption;
        public int GGIndx = 0;                                  // index in GG sub-matrix (for gen bus only) 
        public int LLIndx = 0;                                  // index in LL sub-matrix (for load bus and switchable shunt bus only)
        public bool bVoltRegulated = false;                      // bool - if this bus is regulated by others 
        public double VoltRegulatedSet = 0.0;                       // the target voltage 

        // for dynamic load [dynamic simulation]
        public bool bHasDynLoad = false;                         // has dynamic load 
        public DynLoadModel dynLoad;                                // aggregated load models  


        // for bus frequency [dynamic simulation]
        public BusFreq busFreqCalc;                                 // store if the bus's frequency is measured (for forming jacobian matrix)
        public bool bHasFreqMeasure = false;
        public double busFreq = 0.0;                                // bus frequency deviation (in pu) 
        public int busFreq_Pos = 0;

        // for ACE in AGC simulation [dynamic simulation]
        public double areaFreqWeight = 0.0;

        // for wind data injection modeling [dynamic simulation]
        public DynWind windModel;
        public bool bWindMWEnable = false;
        public double windMWInj = 0.0;

        // neigbring buses 
        public List<bus> neighborBusList = new List<bus>();

        // 
        //public bool bDynSimMon = false;						// decide if it will be stored


        // presenting data purpose 
        public static String[] header = { "Number", "Name", "Type", "Norm kV", "PU Volt", "Angle", "G", "B", "area", "zone", "owner" };
        public static int tableColNum = 11;

        // Read data from string line 
        public bus(String line)
        {
            String[] dataEntry = dataProcess.getDataFields(line, ",");
            I = int.Parse(dataEntry[0]);
            NAME = dataEntry[1].Substring(1, dataEntry[1].LastIndexOf("'"));
            BASKV = Double.Parse(dataEntry[2]);
            IDE = int.Parse(dataEntry[3]);
            GL = Double.Parse(dataEntry[4]) / SBASE;
            BL = Double.Parse(dataEntry[5]) / SBASE;
            AREA = int.Parse(dataEntry[6]);
            ZONE = int.Parse(dataEntry[7]);
            VM = Double.Parse(dataEntry[8]);
            VA = Double.Parse(dataEntry[9]) * Deg2Rad;
            OWNER = int.Parse(dataEntry[10]);

            // initialize the attached bus devices 
            adjBusList = new List<bus>();
            busGens = new List<gen>();
            busLoads = new List<load>();
            busSwShunts = new List<swshunt>();

            // aggregated bus Q
            aggQMax = 0.0;
            aggQMin = 0.0;
            aggPGen = 0.0;

            // for generator voltage regulation 
            genRegBusNumList = new List<int>();
            regBusVoltSetList = new List<Double>();

            // update calculated variables 
            calcType = IDE;
            volt = VM;
            ang = VA;
        }



        // add power flow load model 
        public void addLoad(load loadModel)
        {
            bHasLoad = true;
            //bDynSimMon = true; 
            busLoads.Add(loadModel);
        }

        // add power flow generator model 
        public void addGen(gen genModel)
        {
            bHasGen = true;
            // set the network variable position in yVector - power flow solution vector
            genModel.setHostBus(this);
            busGens.Add(genModel);

            // store the bus generator regulation info
            genRegBusNumList.Add(genModel.IREG);
            regBusVoltSetList.Add(genModel.VS);
        }

        // add power flow switchable shunt model
        public void addSwShunt(swshunt swshuntModel)
        {
            bHasSwShunt = true;
            busSwShunts.Add(swshuntModel);
            CustomMessageHandler.Show("[defiend in bus.java] bus " + I + " switchable shunt regualte bus = " + swshuntModel.SWREM + " to Volt = " + swshuntModel.VSWLO);
        }


        // pre-process bus data 
        public String setup()
        {

            // [0] check if PV bus has generator model 
            if (IDE == 2 || IDE == 3)
            {
                if (!bHasGen)
                {
                    return ("#ERROR: PF bus at " + I + " has no valid generator data!");
                }
            }

            // [1] generator regulation 
            if (bHasGen)
            {
                genRegBusNum = genRegBusNumList[0];                         // will be the regulated bus if no conflicts  
                for (int i = 1; i < genRegBusNumList.Count; i++)
                {
                    if (genRegBusNumList[i] != genRegBusNum)
                    {
                        return ("#ERROR: PF gen at bus " + I + " have different regulated buses");
                    }
                }

                regBusVoltSet = regBusVoltSetList[0];                       // will be the regulated voltage magnitude if no conflicts 
                for (int i = 1; i < regBusVoltSetList.Count; i++)
                {
                    if (regBusVoltSetList[i] != regBusVoltSet)
                    {
                        return ("#ERROR: PF gen at bus " + I + " have different regulated voltage magnitude");
                    }
                }

                bBusHasRegGen = (genRegBusNum != 0) ? true : false;
                genBusVoltSetCalc = regBusVoltSet;                              // use generator section voltage is initialize volt magnitude
                if (bBusHasRegGen)
                {
                    CustomMessageHandler.Show("[definde in bus.java] bus " + I + " regulates " + genRegBusNum + " setVal = " + regBusVoltSet);
                    voltExtOption = new genRegOption(this);
                }

                // calculate the aggregated Q limt 
                foreach (var genTemp in busGens)
                {
                    if (genTemp.STAT == 1)
                    {
                        aggPGen += genTemp.PG;
                    }
                    if (calcType == 2 || calcType == 3)
                    {               // for PV->PQ conversion [don't limit Q for Swing bus] 
                        aggQMax += genTemp.realQT;
                        aggQMin += genTemp.realQB;
                    }
                }

                // calculate the MW and MVar share based on the ratio of individual Qmax to total Qmax
                foreach (var genTemp in busGens)
                {
                    if (calcType == 2 || calcType == 3)
                    {
                        genTemp.busMWShare = genTemp.PG / aggPGen;
                        genTemp.busMVarShare = genTemp.realQT / aggQMax;
                    }
                }
                //CustomMessageHandler.Show("At bus " + I + " pgen = " + aggPGen + " qmax = " + aggQMax + " qmin =" + aggQMin); 
            }

            // [2] switchable shunt regulation 
            if (bHasSwShunt)
            {
                foreach (var swshuntTemp in busSwShunts)
                {
                    regBusVoltSet = swshuntTemp.VSWLO;
                    aggSWshuntBusBMin += swshuntTemp.calcBMin;
                    aggSWshuntBusBMax += swshuntTemp.calcBMax;
                    swshuntCalcB += swshuntTemp.BINIT;
                    swshuntRegBusNum = (swshuntTemp.SWREM == 0) ? I : swshuntTemp.SWREM;
                }
                voltExtOption = new SwShuntRegOption(this);
            }

            // aggregate bus loads 
            foreach (var loadTemp in busLoads)
            {
                if (loadTemp.STATUS == 1)
                {
                    aggCPLoadP += loadTemp.PL;              // for multiple loads 
                    aggCPLoadQ += loadTemp.QL;
                    aggCCLoadP += loadTemp.IP;
                    aggCCLoadQ += loadTemp.IQ;
                    aggCYLoadP += loadTemp.YP;
                    aggCYLoadQ += loadTemp.YQ;              // + for capactive load; - for reactive load
                }
            }

            //if (!busLoads.isEmpty()) {					// base for load schedule in dynamic simulation 
            //	PLoadTotalSet = aggCPLoadP + aggCCLoadP + aggCYLoadP; 
            //	QLoadTotalSet = aggCPLoadQ + aggCCLoadQ - aggCYLoadQ; 
            //}

            return null;
        }

        // calculate the external power injection to the bus (gen - load) 
        public void calcExtPQInj(double voltMag)
        {

            extPInj = 0.0;
            extQInj = 0.0;

            // take into accounts the generation
            if (bHasGen)
            {
                //extPInj += aggPGen + windMWInj;			// calculate additional power injection from wind turbine
                extPInj += aggPGen;
                extQInj += aggQGen;
            }

            // take into accounts the load consumption 
            if (bHasLoad)
            {
                extPInj += -(aggCPLoadP + voltMag * aggCCLoadP + voltMag * voltMag * aggCYLoadP);
                extQInj += -(aggCPLoadQ + voltMag * aggCCLoadQ - voltMag * voltMag * aggCYLoadQ);
            }
        }


        // initialize all of the dynamic models attached to a given bus 
        public void dynIni(DoubleMatrix2D yVector, DoubleMatrix2D xVector)
        {
            // initialize the network variable  
            yVector.setQuick(vangPos, 0, ang);
            yVector.setQuick(vmagPos, 0, volt);

            // <1> regular dynamic model 
            foreach (var genTemp in busGens)
            {
                genTemp.genDyn.ini(yVector, xVector);
                if (genTemp.hasExcModel) genTemp.excDyn.ini(yVector, xVector);
                if (genTemp.hasGovModel) genTemp.govDyn.ini(yVector, xVector);
            }

            // <2> initialize bus frequency 
            if (bHasFreqMeasure) busFreqCalc.ini(yVector, xVector);

            // <3> initialize dynamic load 
            if (bHasLoad) dynLoad.ini(yVector, xVector);

            // <4> initialize wind data
            if (bWindMWEnable) windModel.ini(yVector, xVector);
        }

        // update generators results if they are connected to the same bus 
        public void updateResults()
        {
            foreach (var genTemp in busGens)
            {
                if (calcType == 2 || calcType == 3)
                {
                    genTemp.updateState();              // assign the bus voltage and angle to generators and calculate power injections  
                }
            }
        }

        // get voltage for dynamic simulation 
        public double getVolt(DoubleMatrix2D yVector)
        {
            return yVector.getQuick(vmagPos, 0);
        }

        // get angle for dynamic simulation 
        public double getAngle(DoubleMatrix2D yVector)
        {
            return yVector.getQuick(vangPos, 0);
        }

        // get MW load 
        public double getRealPLoad(DoubleMatrix2D yVector)
        {
            //return yVector.getQuick(dynRealPLoad_Pos, 0); 
            return -9999;
        }

        // get MVAr load
        public double getRealQLoad(DoubleMatrix2D yVector)
        {
            //return yVector.getQuick(dynRealQLoad_Pos,0); 
            return -9999;
        }

        // return bus extended option for voltage regulation

        public BaseExtOption getVoltExtOption()
        {
            return voltExtOption;
        }

        // update yMatrix 
        public void updateYMat(DoubleMatrix2D yMatRe, DoubleMatrix2D yMatIm)
        {
            if (this.IDE != 4)
            {                                                       //active bus 
                // [1] bus self shunt
                yMatRe.setQuick(yMatIndx, yMatIndx, GL + yMatRe.getQuick(yMatIndx, yMatIndx));
                yMatIm.setQuick(yMatIndx, yMatIndx, BL + yMatIm.getQuick(yMatIndx, yMatIndx));


                // ### [2] add constant impedance load model 
                // ### if (bHasLoad && bIncludeLoadInYMat){
                // ###	for (load loadTemp: busLoads){
                // ###		if (loadTemp.STATUS == 1) {
                // ###			yMatRe.setQuick(yMatIndx, yMatIndx, loadTemp.calcGII + yMatRe.getQuick(yMatIndx, yMatIndx)); 
                // ###			yMatIm.setQuick(yMatIndx, yMatIndx, loadTemp.calcBII + yMatIm.getQuick(yMatIndx, yMatIndx)); 
                // ###		}
                // ###	}
                // ###}

                // [2] add switchable shunts 
                if (bHasSwShunt)
                {
                    yMatIm.set(yMatIndx, yMatIndx, swshuntCalcB + yMatIm.getQuick(yMatIndx, yMatIndx));
                }
            }
        }



        // add neighboring bus 
        public void addNeighborBus(bus busTemp)
        {
            if (!neighborBusList.Contains(busTemp))
            {
                neighborBusList.Add(busTemp);
            }
        }


        // export data for tabular showing 
        public override string[] AsArrayForRow()
        {
            return new []
            {
                $"{I}",
                $"{NAME}",
                $"{IDE}",
                $"{BASKV}",
                $"{volt}",
                $"{ang / Deg2Rad}",
                $"{GL * SBASE}",
                $"{BL * SBASE}",
                $"{AREA}",
                $"{ZONE}",
                $"{OWNER}"
            };
        }


    }
}
