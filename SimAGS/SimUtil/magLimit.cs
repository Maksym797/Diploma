using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimAGS.SimUtil
{
    public class magLimit
    {
        public bool bEnable = false; // enable limiter? 
        public double upLimitVar = 999;     // upper limit value
        public double lowLimitVar = -999;       // lower limit value
        public double x = 0.0;      // block output
        public double dx_dy = 0.0;      // d(x)/d(y)
        public String checkMesg = "";

        public magLimit(double upVal, double lowVal)
        {
            upLimitVar = upVal;
            lowLimitVar = lowVal;
            bEnable = false;
        }

        // enable upper and lower limit 
        public void enable()
        {
            bEnable = true;
        }

        // disable upper and lower limit 
        public void disable()
        {
            bEnable = false;
        }

        // calculate the block values 
        public void calc(double input)
        {
            if (bEnable)
            {
                // check upper limit 
                if (input >= upLimitVar)
                {
                    x = upLimitVar;
                }
                else if (input <= lowLimitVar)
                {
                    x = lowLimitVar;
                }
                else
                {
                    x = input;
                }
                dx_dy = 1;
            }
            else
            {
                x = input;
                dx_dy = 1;
            }
        }

        public double get_x() { return x; }

        public double get_dx_dy() { return dx_dy; }


        public String checkLimit(double input)
        {           // 
            if (input > upLimitVar)
            {
                return (checkMesg = _String.format("%10.5f violates upLimit = %10.5f", input, upLimitVar));
            }
            else if (input < lowLimitVar)
            {
                return (checkMesg = _String.format("%10.5f violates lowLimit = %10.5f", input, lowLimitVar));
            }
            else
            {
                return (checkMesg = "");
            }
        }

        public String getCheckMesg() { return checkMesg; }

        public double getUpLimiter() { return upLimitVar; }

        public double getLowLimiter() { return lowLimitVar; }


        public void setUpLimiter(double val) { upLimitVar = val; }

        public void setLowLimiter(double val) { lowLimitVar = val; }

    }
}
