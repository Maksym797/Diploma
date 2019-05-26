using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cern.colt.matrix;
using Force.DeepCloner;
using ikvm.extensions;
using java.lang;
using SimAGS.DynProcessor;
using SimAGS.Handlers;
using Math = java.lang.Math;
using Object = System.Object;

namespace SimAGS.DistEvent
{
    public class distEventList
    {
        // class for storing all the _event at a specific time instant
        public class _eventTimeSnaphot : abstractDistEvent
        {
            public double time = 0.0;
            public List<abstractDistEvent> _events;
            public bool storeYVar = false; // indicate if Y variable needs to be stored
            public bool restoreYVar = false; // indicate if Y variable needs to be re-stored

            public _eventTimeSnaphot(abstractDistEvent _event)
            {
                _events = new List<abstractDistEvent>();
                time = _event._eventTime;
                _events.add(_event);
                if (_event.storeYVar == true) storeYVar = true;
                if (_event.restoreYVar == true) restoreYVar = true;
            }

            public bool addEvent(abstractDistEvent _event)
            {
                bool bSuccess = false;
                if (time != _event._eventTime)
                {
                    CustomMessageHandler.println("### Error in distEventList.java");
                }
                else
                {
                    _events.add(_event);
                    CustomMessageHandler.println(" type = " + _event.getClass() + " storeYVar = " + _event.storeYVar +
                                                 " restoreYVar = " + _event.restoreYVar);

                    if (_event.storeYVar == true)
                    {
                        storeYVar = true;
                        CustomMessageHandler.println(" Need to store ");
                    }
                    if (_event.restoreYVar == true)
                    {
                        restoreYVar = true;
                        CustomMessageHandler.println(" Need to restore");
                    }
                    bSuccess = true;
                }
                return bSuccess;
            }

            // apply all _event for a given time instant
            public bool applyEvent(DynCase sim)
            {
                bool bSuccess = false;
                foreach (abstractDistEvent _eventInstance in _events)
                {
                    _eventInstance.applyEvent(sim);
                }
                //
                return bSuccess;
            }

            // for clone purpose in dynamic initialization 
            public Object clone()
            {
                try
                {
                    return (_eventTimeSnaphot)this.DeepClone();
                }
                catch (CloneNotSupportedException e)
                {
                    e.printStackTrace();
                }
                return null;
            }
        }

        // ------ class elements 
        public List<_eventTimeSnaphot> _eventList;

        public int _eventIdx;
        public bool bEventOccur;
        public double hEvent; // time step for _event
        public DynCase mainSim;
        public bool bT0Calc;
        public DoubleMatrix2D yVectorStore;

        // default constructor 
        public distEventList(DynCase MainSim)
        {
            mainSim = MainSim;
            _eventList = new List<distEventList._eventTimeSnaphot>();
            _eventIdx = 0;
            hEvent = 0.0;
            bEventOccur = false;
        }


        // add disturbance _event
        public bool addEvent(abstractDistEvent _event)
        {
            bool bSuccess = false;
            if (_eventList.isEmpty())
            {
                // create new _eventDispatchList if none exists
                _eventList.add(new distEventList._eventTimeSnaphot(_event));
            }
            else
            {
                for (int i = 0; i < _eventList.size(); i++)
                {
                    if (_event._eventTime < _eventList.get(i).time)
                    {
                        // new _event 
                        _eventList.add(i, new _eventTimeSnaphot(_event));
                        bSuccess = true;
                        break;
                    }
                    else if (_event._eventTime == _eventList.get(i).time)
                    {
                        // add _event to existing time instant
                        _eventList.get(i).addEvent(_event);
                        bSuccess = true;
                        break;
                    }
                }

                // new _event is added to the end of the _eventList
                if (bSuccess == false)
                {
                    _eventList.add(new _eventTimeSnaphot(_event));
                    bSuccess = true;
                }
            }
            return bSuccess;
        }


        // get the next h considering _event 
        public double getTimeInc(double h, double t)
        {
            double ret = 999;
            if (!_eventList.isEmpty())
            {
                if (_eventIdx < _eventList.size())
                {
                    hEvent = _eventList.get(_eventIdx).time - t;
                    if (t + h >= _eventList.get(_eventIdx).time)
                    {
                        ret = hEvent;
                    }
                }
            }
            ret = Math.min(h, ret);
            return ret;
        }


        // apply disturbance _event 
        // [1] Load setting: CustomMessageHandler.println("----->" + "Load at bus 7 is added!");
        // 					 add load yMat.modBusFaultZ(7, 1, 0);	
        // [2] AGC setting: AGCCtrl.setMWFLow(14.0);
        //					CustomMessageHandler.println("AGC MW control = 14.0");

        public void applyEvent(double h)
        {
            bT0Calc = false;
            if (!_eventList.isEmpty())
            {
                if (bEventOccur)
                {
                    _eventList.get(_eventIdx).applyEvent(mainSim);
                    bT0Calc = true;

                    // check if the yVar needs to be stored 
                    if (_eventList.get(_eventIdx).storeYVar)
                    {
                        CustomMessageHandler.println(" ######### Will store yVariable");
                        yVectorStore = mainSim.yVector;
                    }

                    if (_eventList.get(_eventIdx).restoreYVar)
                    {
                        CustomMessageHandler.println(" ######### Will restore yVariable");
                        mainSim.yVector = yVectorStore; // yVector is the initial value that for time integration		
                    }
                    // move to the next stored _events
                    _eventIdx++;
                }
                else
                {
                    if (_eventIdx < _eventList.size())
                    {
                        if (h == hEvent)
                        {
                            bEventOccur = true;
                        }
                    }
                }
            }
        }


        // rest _eventList
        public void reset()
        {
            _eventIdx = 0;
            hEvent = 0.0;
            bEventOccur = false;
        }


        /*
        // for clone purpose in dynamic initialization 
        public Object clone() {
            distEventList objList = null;  
            try {
                objList = (distEventList) super.clone(); 	
                return objList;
            } catch (CloneNotSupportedException e) {
                e.printStackTrace();
            }
            return null; 
        }
        */


    }
}