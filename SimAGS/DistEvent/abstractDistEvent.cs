using ikvm.extensions;
using SimAGS.DynProcessor;
using System;

namespace SimAGS.DistEvent
{
    public class  abstractDistEvent
    {
        public double _eventTime;            // event happening time (don't define it in specific realization)
        public bool storeYVar;
        public bool restoreYVar;

        // default constructor
        public abstractDistEvent()
        {

        }

        // read the field in ith () 
        public static String[] getDataField(String dataLine, int posNum)
        {
            String[] ret = { };

            String dataString = "";
            int startingPos = 0;
            int endingPos = -1;         // initial 

            // read the field that may contain ","
            for (int i = 0; i < posNum; i++)
            {
                dataLine = dataLine.substring(endingPos + 1, dataLine.length());
                startingPos = dataLine.indexOf("(");
                endingPos = dataLine.indexOf(")");
                if (startingPos == -1 || endingPos == -1)
                {
                    return null;
                }
            }
            dataString = dataLine.substring(startingPos + 1, endingPos);

            // check if the data field contains ","
            ret = dataProcess.getDataFields(dataString, ",");
            return ret;
        }

        // apply event  
        public void applyEvent(DynCase sim)
        {

        }
    }
}
