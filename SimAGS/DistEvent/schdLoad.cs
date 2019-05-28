using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimAGS.Components;
using SimAGS.DynProcessor;
using SimAGS.Handlers;

namespace SimAGS.DistEvent
{
    public class schdLoad:abstractDistEvent
    {
        public int loadBusNum = 0;              // load bus number 
        public double loadMWSchdVal = 0.0;      // load setting value 

        public schdLoad(String dataLine)
        {
            _eventTime = Double.Parse(abstractDistEvent.getDataField(dataLine, 1)[0]);
            loadBusNum = Integer.parseInt(abstractDistEvent.getDataField(dataLine, 2)[0]);
            loadMWSchdVal = Double.Parse(abstractDistEvent.getDataField(dataLine, 3)[0]);
        }

        public schdLoad(double time, int busNum, double value)
        {
            _eventTime = time;
            loadBusNum = busNum;
            loadMWSchdVal = value;
        }

        // update system model for disturbance event 
        public void applyEvent(DynCase sim)
        {
           CustomMessageHandler.println("----->" + "load at bus " + loadBusNum + " is scaled to " + _String.format("%1.2f", loadMWSchdVal) + " MW");
            foreach (bus busTemp in sim.busList)
            {
                if (busTemp.bHasLoad && busTemp.I == loadBusNum)
                {
                    //busTemp.dynLoad.schdMW(loadMWSchdVal/100);
                    return;
                }
            }
            CustomMessageHandler.println("### Error: can't changing the geneator MW output in distEvent");
            Environment.Exit(0);
        }

    }
}
