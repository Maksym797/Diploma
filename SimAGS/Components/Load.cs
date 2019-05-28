using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimAGS.Components
{
    public class load : abstractPfElement
    {

        // raw data section
        public int I = 0;       // bus number
        public String ID = "'1'";   // load ID 
        public int STATUS = 1;      // Status = 1 - in-service
        public int AREA = 0;        // area number
        public int ZONE = 0;        // zone number
        public double PL = 0.0;     // P - Constant Power in MW
        public double QL = 0.0;     // Q - Constant Power in MVar
        public double IP = 0.0;     // P - Constant Current in MW at 1.0 pu voltage 
        public double IQ = 0.0;     // Q - Constant Current in MVar at 1.0 pu voltage  
        public double YP = 0.0;     // P - Constant Impedance in MW at 1.0 pu voltage
        public double YQ = 0.0;     // Q - Constant Impedance in MVar at 1.0 pu voltage (+ for capactive load; - for reactive load)
        public int OWNER = 0;       // owner number
        public double FREQCOF = 0.0;    // frequency dependent coefficient

        public static int DATALENGTH = 13;  // default data line 

        public double SBASE = 100;          // system base

        // calculated variables 
        public double calcGII = 0.0;        // calculate self-impedance for constant impedance component
        public double calcBII = 0.0;        //  

        // presenting data purpose 
        public override String[] header {get; set;} = { "Number", "ID", "Status", "PL", "QL", "IP", "IQ", "YP", "YQ", "area", "zone", "owner" };
        public static int tableColNum = 12;


        // Read data from string line
        public load(String line)
        {
            String[] dataEntry = dataProcess.getDataFields(line, ",");
            I = int.Parse(dataEntry[0]);
            ID = dataEntry[1].Substring(1, dataEntry[1].LastIndexOf("'"));
            STATUS = int.Parse(dataEntry[2]);
            AREA = int.Parse(dataEntry[3]);
            ZONE = int.Parse(dataEntry[4]);
            PL = Double.Parse(dataEntry[5]) / SBASE;
            QL = Double.Parse(dataEntry[6]) / SBASE;
            IP = Double.Parse(dataEntry[7]) / SBASE;
            IQ = Double.Parse(dataEntry[8]) / SBASE;
            YP = Double.Parse(dataEntry[9]) / SBASE;
            YQ = Double.Parse(dataEntry[10]) / SBASE;         // in PSS/E ( + for capacitor; - for inductance) 
            //OWNER = Integer.valueOf(dataEntry[11]);
            OWNER = int.Parse(dataEntry[11]);

            calcPara();
        }

        // calculate parameters 			
        public void calcPara()
        {
            calcGII = YP;
            calcBII = YQ;
        }


        // export data for tabular showing 
        public override string[] AsArrayForRow()
        {
            return new[]
            {
                $"{I}",
                $"{ID}",
                $"{(STATUS == 1 ? "Closed" : "OSS")}",
                $"{PL * SBASE}",
                $"{QL * SBASE}",
                $"{IP * SBASE}",
                $"{IQ * SBASE}",
                $"{YP * SBASE}",
                $"{YQ * SBASE}",
                $"{AREA}",
                $"{ZONE}",
                $"{OWNER}"
            };
        }
    }
}
