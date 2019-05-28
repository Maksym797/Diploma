using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimAGS.DynProcessor;
using SimAGS.Handlers;

namespace SimAGS.DistEvent
{
    class addBusImpd : abstractDistEvent
    {
        public int busNum = 0; // load bus number 
        public double impdR = 0.0;
        public double impdX = 0.0;

        public addBusImpd(double time, int number, double addR, double addX, int typeNum)
        {
            _eventTime = time;
            busNum = number;
            impdR = addR;
            impdX = addX;
            if (typeNum == 1) storeYVar = true;
            if (typeNum == 2) restoreYVar = true;
        }

        // update system model for disturbance event 
        public void applyEvent(DynCase sim)
        {
            sim.yMat.addBusImpd(busNum, impdR, impdX);
            CustomMessageHandler.println("----->" + "Impedance (" + _String.format("%6.3f, %6.3f", impdR, impdX) +
                                         ") at bus " + busNum + " is added! " + "storeYVar = " + storeYVar +
                                         " restoreYVar = " + restoreYVar);
        }
    }
}