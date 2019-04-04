﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimAGS.Components
{
    public class Area : AbstractElement
    {
        // data loaded from raw file
        public int I = 0;           // area number
        public int ISW = 0;         // slack bus for area interchange control 
        public double PDES = 0.0;           // desired net interchange leaving the area in MW 
        public double PTOL = 10.0;      // interchange tolerance bandwidth in MW 
        public String ARNAME = "";          // area name 

        public static int DATALENGTH = 5;   // default data line 

        // data for tabular display 
        public static String[] header = { "Number", "SlackBus", "DesiredMW", "InterChangeTol", "Name" };
        public static int tableColNum = DATALENGTH;

        public Area(string line)
        {
            //String[] dataEntry = dataProcess.getDataFields(line, ",");
            //I = Integer.parseInt(dataEntry[0]);
            //ISW = Integer.parseInt(dataEntry[1]);
            //PDES = Double.parseDouble(dataEntry[2]);
            //PTOL = Double.parseDouble(dataEntry[3]);
            //if (dataEntry[4].startsWith("'"))
            //{
            //    ARNAME = dataEntry[4].substring(1, dataEntry[4].length() - 1).trim();
            //}
            //else
            //{
            //    ARNAME = dataEntry[4].trim();
            //}
        }
    }
}
