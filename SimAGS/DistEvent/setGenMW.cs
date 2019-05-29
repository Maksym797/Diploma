using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ikvm.extensions;
using SimAGS.Components;
using SimAGS.DynProcessor;
using SimAGS.Handlers;

namespace SimAGS.DistEvent
{
    class setGenMW : abstractDistEvent
    {
        //public double _eventTime = 0.0; 			// event occuring time 
        public int genBusNum = 0;                   // tripped generator number
        public String genID = "";                   // tripped generator ID 
        public double genMWSetVal = 0.0;            // new setting value 

        public setGenMW(String dataLine)
        {
            _eventTime = Double.Parse(abstractDistEvent.getDataField(dataLine, 1)[0]);
            genBusNum = Integer.parseInt(abstractDistEvent.getDataField(dataLine, 2)[0]);
            genID = abstractDistEvent.getDataField(dataLine, 2)[1];
            genMWSetVal = Double.Parse(abstractDistEvent.getDataField(dataLine, 3)[0]);
            //System.out.println("Event =" + _eventTime + " " + genBusNum + " " + genID + " new MW = " + genMWSetVal); 
        }

        public setGenMW(double time, int GenNum, String GenID, double value)
        {
            _eventTime = time;
            genBusNum = GenNum;
            genID = GenID;
            genMWSetVal = value;
        }


        // update system model for disturbance event 
        public void applyEvent(DynCase sim)
        {
           CustomMessageHandler.println("----->" + "Gen at bus " + genBusNum + " is changed to " + _String.format("%1.2f", genMWSetVal));
            foreach (bus busTemp in sim.busList)
            {
                if (busTemp.bHasGen)
                {
                    foreach (gen genTemp in busTemp.busGens)
                    {
                        if (genTemp.I == genBusNum && genTemp.ID._equalsIgnoreCase(genID))
                        {
                            genTemp.changeMWSetting(genMWSetVal);
                            return;
                        }
                    }
                }
            }
            CustomMessageHandler.println("### Error: can't changing the geneator MW output in distEvent");
            Environment.Exit(0);
        }
    }
}
