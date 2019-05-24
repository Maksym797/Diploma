using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimAGS.DynModels.ExcModels;
using SimAGS.DynModels.GenModels;
using SimAGS.DynModels.GovModels;
using SimAGS.Handlers;

namespace SimAGS.Components
{
    public class gen : abstractPfElement
    {
        // data loaded from raw file 
        public int I = 0;       // bus number
        public String ID = "'1'";   // busID 
        public double PG = 0.0;     // P gen in MW 
        public double QG = 0.0;     // Q gen in MVar
        public double QT = 0.0;     // Q max in MVar
        public double QB = 0.0;     // Q min in MVar
        public double VS = 1.0;     // Regulated voltage setpoint in pu 
        public int IREG = 0;        // regulated bus (0 - own bus)
        public double MBASE = 0.0;      // machine base in MW 
        public double ZR = 0.0;     // generator R (corresponding to dynamic generator model)
        public double ZX = 0.0;     // generator X 
        public double RT = 0.0;     // GSU R in pu on MBASE base
        public double XT = 0.0;     // GSU X in pu on MBASE base
        public double GTAP = 1.0;       // transformer off-normal turns ration in pu 
        public int STAT = 0;        // status ( 0 - out-of-service; 1 - in-service)
        public double RMPCT = 100;      // percentage of total MVar to hold regulated voltage
        public double PT = 9999;    // PG max in MW 
        public double PB = -9999;   // PG min in MW
        public int O1 = 0;      // 1st owner number
        public double F1 = 100;     // 1st owner percentage
        public int O2 = 0;      // 2nd owner number
        public double F2 = 0;       // 2nd owner percentage
        public int O3 = 0;      // 3rd owner number
        public double F3 = 0;       // 3rd owner percentage
        public int O4 = 0;      // 4th owner number
        public double F4 = 0;       // 4th owner percentage

        public static int DATALENGTH = 26;      // default data line 

        // extended variables 
        public bus hostBus;

        public double busMWShare = 1.0;                 // calculate the shared MW for multiple generators at the same bus
        public double busMVarShare = 1.0;                   // calculate the shared MVAR for multiple generators at the same bus

        public double realQT = 0.0;                 // QT*RMPCT
        public double realQB = 0.0;                 // QB*RMPCT

        // calculated variables 
        public double calcVt = 0.0;                     // terminal voltage magnitude 
        public double calcAng = 0.0;                        // terminal voltage angle 
        public double calcPgen = 0.0;                       // calculated active power
        public double calcQgen = 0.0;                       // calculated reactive power 
        public int vmagPos = 0;                     // terminal voltage magnitude position
        public int vangPos = 0;                     // terminal voltage angle position 

        // generator dynamic model [dynamic simulation] 
        public GenModel genDyn;                             // generator dynamic model  
        public bool hasGenModel = false;
        public int omega_Pos = 0;
        public int pmech_Pos = 0;                   // output of governor that feeds to generator

        // generator excitation model [dynamic simulation]
        public ExcModel excDyn;                             // exciter model 
        public bool hasExcModel = false;
        public int efd_Pos = 0;

        // generator governor model [dynamic simulation]
        public GovModel govDyn;                             // governor model; 
        public bool hasGovModel = false;

        // generator for AGC control [dynamic simulation]
        public bool hasAGCCtrl = false;              // set AGC control 
        public double AGCSMWhare = 0.0;                     // AGC share percentage 
        public int agcPRefTotal_Pos = 0;                    // total AGC Power reference 

        // generator trip [dynamic simulation]
        public bool InService = false;               // used for tripping generators during dynamic simulation

        // for tabular display 
        public static String[] header = { "Number", "ID", "Status", "gen MW", "gen MVar", "SetVolt", "RegBus", "Min MVar", "Max MVar", "Min MW", "Max MW", "MVABase" };
        public static int tableColNum = 12;


        // Read data from string line 
        public gen(String line)
        {
            String[] dataEntry = dataProcess.getDataFields(line, ",");
            I = int.Parse(dataEntry[0]);
            ID = dataEntry[1].Substring(1, dataEntry[1].LastIndexOf("'")).Trim();
            PG = Double.Parse(dataEntry[2]) / SBASE;
            QG = Double.Parse(dataEntry[3]) / SBASE;
            QT = Double.Parse(dataEntry[4]) / SBASE;
            QB = Double.Parse(dataEntry[5]) / SBASE;
            VS = Double.Parse(dataEntry[6]);
            IREG = int.Parse(dataEntry[7]);
            MBASE = Double.Parse(dataEntry[8]);
            ZR = Double.Parse(dataEntry[9]) / MBASE *  SBASE;
            ZX = Double.Parse(dataEntry[10]) / MBASE * SBASE;
            RT = Double.Parse(dataEntry[11]) / MBASE * SBASE;
            XT = Double.Parse(dataEntry[12]) / MBASE * SBASE;
            GTAP = Double.Parse(dataEntry[13]);
            STAT = int.Parse(dataEntry[14]);
            RMPCT = Double.Parse(dataEntry[15]) / 100;
            PT = Double.Parse(dataEntry[16]) / SBASE;
            PB = Double.Parse(dataEntry[17]) / SBASE;
            O1 = int.Parse(dataEntry[18]);
            F1 = Double.Parse(dataEntry[19]);
            if (dataEntry.Length > 20)
            {
                O2 = int.Parse(dataEntry[20]);
                F2 = Double.Parse(dataEntry[21]);
                if (dataEntry.Length > 22)
                {
                    O3 = int.Parse(dataEntry[22]);
                    F3 = Double.Parse(dataEntry[23]);
                    if (dataEntry.Length > 24)
                    {
                        O4 = int.Parse(dataEntry[24]);
                        F4 = Double.Parse(dataEntry[25]);
                    }
                }
            }

            // extended variables 
            InService = STAT == 1 ? true : false;
            realQT = QT * RMPCT;
            realQB = QB * RMPCT;

            // initialize computed variables 
            calcPgen = PG;
            calcQgen = QG;
            calcVt = VS;
        }

        //set the host bus to generators 
        public void setHostBus(bus busTemp)
        {
            hostBus = busTemp;
        }

        // update states
        public void updateState()
        {
            vangPos = hostBus.vangPos;
            vmagPos = hostBus.vmagPos;
            calcPgen = busMWShare * hostBus.aggPGen;
            calcQgen = busMVarShare * hostBus.aggQGen;
            calcVt = hostBus.volt;
            calcAng = hostBus.ang;
        }

        public int getVmPos()
        {
            return vmagPos;
        }

        public double getVm()
        {
            return calcVt;
        }

        // change generator MW setting 
        public void changeMWSetting(double PSetVal)
        {
            if (hasGovModel == true)
            {
                govDyn.setMWRef(PSetVal / SBASE);
                CustomMessageHandler.Show("genertor govDyn setting is changed");
            }
            else
            {
                if (hasGenModel == true)
                {
                    genDyn.setPmRef(PSetVal / SBASE);
                    CustomMessageHandler.Show("genertor genDyn setting is changed");
                }
                else
                {
                    CustomMessageHandler.Show("### Error: generator at " + I + "_" + ID + " MW can't be changed as no GEN or GOV model is defined");
                    Environment.Exit(0);
                }
            }
        }

        //get generator MW setting value 
        public double getMWSetting()
        {
            if (hasGovModel == true)
            {
                return govDyn.getTotalMWRef();
            }
            else
            {
                if (hasGenModel == true)
                {
                    return genDyn.getPm0();
                }
                else
                {
                    CustomMessageHandler.Show("### Error: generator at " + I + "_" + ID + " MW setting can't be loaded");
                    Environment.Exit(0);
                }
            }
            return -9999;
        }

        // shut down generator 
        public void shutDown()
        {
            InService = false;
        }

        // export data for tabular showing 
        public override string[] AsArrayForRow()
        {
            return new []
            {
                I.ToString(),
                ID,
                STAT == 1 ? "Closed" : "OSS",
                $"{calcPgen * SBASE}",
                $"{calcQgen * SBASE}",
                $"{VS}",
                $"{IREG}",
                $"{QB * SBASE}",
                $"{QT * SBASE}",
                $"{PB * SBASE}",
                $"{PT * SBASE}",
                $"{MBASE}",
            };
        }

    }
}
