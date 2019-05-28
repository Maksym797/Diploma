using ikvm.extensions;
using SimAGS.Components;
using SimAGS.DynProcessor;
using SimAGS.Handlers;
using System;

namespace SimAGS.DistEvent
{
    public class tripBranch : abstractDistEvent
    {

        public int frBusNum = 0;
        public int toBusNum = 0;
        public String branchID = "";
        //public double eventTime = 0.0;			// inherit from main object  

        public tripBranch(String dataLine)
        {
            _eventTime = Double.Parse(abstractDistEvent.getDataField(dataLine, 1)[0]);
            frBusNum = Integer.parseInt(abstractDistEvent.getDataField(dataLine, 2)[0]);
            toBusNum = Integer.parseInt(abstractDistEvent.getDataField(dataLine, 2)[1]);
            branchID = abstractDistEvent.getDataField(dataLine, 2)[2];
            CustomMessageHandler.println("Event =" + _eventTime + " trip branch at [" + frBusNum + "," + toBusNum + "," + branchID + "]");
        }

        public tripBranch(double t, int frBus, int toBus, String id)
        {
            _eventTime = t;
            frBusNum = frBus;
            toBusNum = toBus;
            branchID = id;
        }

        // update system model for disturbance event 
        public void applyEvent(DynCase sim)
        {
            CustomMessageHandler.println("----->" + "branch [" + frBusNum + "," + toBusNum + "," + branchID + "] is tripped");
            foreach (branch branTemp in sim.branchList)
            {
                if (branTemp.frBus.I == frBusNum && branTemp.toBus.I == toBusNum && branTemp.CKT.equalsIgnoreCase(branchID))
                {
                    sim.yMat.removeBranch(branTemp);
                    return;
                }
            }
            CustomMessageHandler.println("### Error: can't trip branch");
            Environment.Exit(0);
        }


    }
}
