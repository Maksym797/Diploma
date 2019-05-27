using SimAGS.DynModels.AgcModel;
using SimAGS.DynProcessor;
using SimAGS.Handlers;

namespace SimAGS.DistEvent
{
    class actAGC : abstractDistEvent
    {
        //public double eventTime = 0.0; 		// event occurring time [inherit from parent class]	
        public agcModel agcObj;

        // class constructor from data line input 
        public actAGC(double time, agcModel agc)
        {
            _eventTime = time;                   // must be defined 
            agcObj = agc;
        }

        // update system model for disturbance event 
        public void applyEvent(DynCase sim)
        {
            CustomMessageHandler.println("----> AGC is activated!");
            agcObj.activate();
        }
    }
}
