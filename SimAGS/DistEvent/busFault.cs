using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimAGS.Handlers;

namespace SimAGS.DistEvent
{
    class busFault: abstractDistEvent
    {

        public int busNum = 0;
        public double faultStartTime = 0.0;
        public double faultImpedR = 0.0;    // pu
        public double faultImpedX = 9999;   // pu
        public double faultDuration = 0.0;
        public double faultClearTime = 0.0;
        public List<tripBranch> tripBranchEventList = new List<tripBranch>();


        public busFault(String dataLine)
        {
            faultStartTime = Double.Parse(abstractDistEvent.getDataField(dataLine, 1)[0]);
            busNum = Integer.parseInt(abstractDistEvent.getDataField(dataLine, 2)[0]);
            faultImpedR = Double.Parse(abstractDistEvent.getDataField(dataLine, 3)[0]);
            faultImpedX = Double.Parse(abstractDistEvent.getDataField(dataLine, 3)[1]);
            faultDuration = Double.Parse(abstractDistEvent.getDataField(dataLine, 4)[0]) / 60;        // convert cycle to second
            faultClearTime = faultStartTime + faultDuration;

            // load tripped branches 
            int tripBranchPosIndx = 5;
            while (true)
            {
                String[] loadedBranInfo = abstractDistEvent.getDataField(dataLine, tripBranchPosIndx);
                if (loadedBranInfo != null && loadedBranInfo.length() != 0)
                {
                    int tripBranFrBusNum = Integer.parseInt(abstractDistEvent.getDataField(dataLine, tripBranchPosIndx)[0]);
                    int tripBranToBusNum = Integer.parseInt(abstractDistEvent.getDataField(dataLine, tripBranchPosIndx)[1]);
                    String tripBranchId = abstractDistEvent.getDataField(dataLine, tripBranchPosIndx)[2];
                    tripBranchEventList.add(new tripBranch(faultClearTime, tripBranFrBusNum, tripBranToBusNum, tripBranchId));
                    tripBranchPosIndx++;
                }
                else
                {
                    break;
                }
            }
            CustomMessageHandler.println("Event =" + faultStartTime + " fault at " + busNum + " cleared at " + faultClearTime + " # of tripped branch " + tripBranchEventList.size());
        }

        public addBusImpd applyFault()
        {
            addBusImpd addBusImpdEvent = new addBusImpd(faultStartTime, busNum, faultImpedR, faultImpedX, 1);
            return addBusImpdEvent;
        }

        public addBusImpd clearFault()
        {
            addBusImpd addBusImpdEvent = new addBusImpd(faultClearTime, busNum, -faultImpedR, -faultImpedX, 2);
            return addBusImpdEvent;
        }



        //public tripBranch(double t, int frBus, int toBus, String id) {

    }
}
