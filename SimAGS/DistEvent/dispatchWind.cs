using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimAGS.DynProcessor;
using SimAGS.Handlers;

namespace SimAGS.DistEvent
{
    class dispatchWind : abstractDistEvent
    {

        //public double eventTime = 0;		// defined in parent class
        public int windBusNum = 0; // load bus number 

        public double dispMWVal = 0.0; // dispatched wind MW  

        // constructor for numeric data input
        public dispatchWind(double time, int busNum, double addMW)
        {
            _eventTime = time;
            windBusNum = busNum;
            dispMWVal = addMW;
        }

        // update system model for disturbance event 
        public void applyEvent(DynCase sim)
        {
            sim.findBusAt(windBusNum).windModel.schdMW(dispMWVal);
            CustomMessageHandler.println(_String.format("-----> wind MW injection at bus %3d is dispatched to %5.2f",
                windBusNum, dispMWVal));
        }
    }
}