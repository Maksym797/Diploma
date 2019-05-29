using ikvm.extensions;
using SimAGS.Components;
using SimAGS.DynProcessor;
using SimAGS.Handlers;
using System;

namespace SimAGS.DistEvent
{
    class tripGen: abstractDistEvent
    {

        //public double eventTime = 0.0; 			// event occuring time [inherit from parent class]

        public int genBusNum = 0;               // tripped generator number
        public String genID = "";               // tripped generator ID 

        // class constructor from data line input 
        public tripGen(String dataLine)
        {
            _eventTime = Double.Parse(abstractDistEvent.getDataField(dataLine, 1)[0]);
            genBusNum = Integer.parseInt(abstractDistEvent.getDataField(dataLine, 2)[0]);
            genID = abstractDistEvent.getDataField(dataLine, 2)[1];
            //System.out.println("Event =" + eventTime + " " + genBusNum + " " + genID); 
        }

        // update system model for disturbance event 
        public void applyEvent(DynCase sim)
        {
            CustomMessageHandler.println("----->" + "Gen at bus " + genBusNum + " is tripped ");
            foreach (bus busTemp in sim.busList)
            {
                if (busTemp.bHasGen)
                {
                    foreach (gen genTemp in busTemp.busGens)
                    {
                        if (genTemp.I == genBusNum && genTemp.ID._equalsIgnoreCase(genID))
                        {
                            genTemp.shutDown();
                            return;
                        }
                    }
                }
            }
            CustomMessageHandler.println("### Error: can't shunt down generator <no such generator exists>");
            Environment.Exit(0);
        }
    }
}
