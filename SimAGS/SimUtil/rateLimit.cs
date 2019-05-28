﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimAGS.SimUtil
{
    public class rateLimit
    {

        public bool bEnable = false; // enable limiter? 
        public double upLimitVar = 999;     // upper limit value
        public double lowLimitVar = -999;       // lower limit value
        public double dx = 0.0;     // d(output) output after limit 
        public double fy = 0.0;     // d(d_output) w.r.s.t input
        public double tConst = 0.0;
        public String checkMesg = "";

        public rateLimit(double upVal, double lowVal, double tConstVal)
        {
            upLimitVar = upVal;
            lowLimitVar = lowVal;
            tConst = tConstVal;
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
            double dxTemp = input / tConst;
            if (bEnable)
            {
                if (dxTemp < lowLimitVar && dxTemp < 0)
                {
                    dx = dxTemp;
                    fy = 0.0;
                }
                else if (dxTemp > upLimitVar && dxTemp > 0)
                {
                    dx = upLimitVar;
                    fy = 0.0;
                }
                else
                {
                    dx = dxTemp;
                    fy = 1 / tConst;
                }
            }
            else
            {
                dx = dxTemp;
                fy = 1 / tConst;
            }
        }


        public double get_dx() { return dx; }

        public double get_fy() { return fy; }

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
