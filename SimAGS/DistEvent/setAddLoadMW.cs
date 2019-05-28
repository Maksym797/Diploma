using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimAGS.DynProcessor;
using SimAGS.Handlers;

namespace SimAGS.DistEvent
{
    class setAddLoadMW: abstractDistEvent
    {

        //public double eventTime = 0;		// defined in parent class
        public int addBusNum = 0;           // load bus number 
        public double addMWVal = 0.0;       // add load 
        //public double loadBusVolt = 0.0;	// load bus voltage

        // constructor for string data input
        public setAddLoadMW(String dataLine)
        {
            _eventTime = Double.Parse(abstractDistEvent.getDataField(dataLine, 1)[0]);
            addBusNum = Integer.parseInt(abstractDistEvent.getDataField(dataLine, 2)[0]);
            addMWVal = Double.Parse(abstractDistEvent.getDataField(dataLine, 3)[0]) / 100;
        }

        // constructor for numeric data input
        public setAddLoadMW(double time, int busNum, double addMW)
        {
            _eventTime = time;
            addBusNum = busNum;
            addMWVal = addMW;
        }

        // update system model for disturbance event 
        public void applyEvent(DynCase sim)
        {
            CustomMessageHandler.println(" load adding is disabled");
            CustomMessageHandler.println("-----> " + addMWVal * 100 + " MW load at bus " + addBusNum + " is added!");
            //sim.addLoadMW(addBusNum, addMWVal); 


            // when the load is constant-impednace
            //sim.yMat.addBusImpd(loadBusNum, addMWVal/100, 0);
        }
    }
}
