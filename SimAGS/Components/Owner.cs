using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ikvm.extensions;

namespace SimAGS.Components
{
    public class owner
    {

        public int I = 0;       // zone number
        public String OWNAME = "";      // zone name 

        public static int DATALENGTH = 2;   // default data line 

        // presenting data purpose 
        public static String[] header = { "Number", "Name" };
        public static int tableColNum = DATALENGTH;

        // Read data from string line 
        public owner(String line)
        {
            String[] dataEntry = dataProcess.getDataFields(line, ",");
            I = Integer.parseInt(dataEntry[0]);
            if (dataEntry[1].startsWith("'"))
            {
                OWNAME = dataEntry[1].substring(1, dataEntry[1].length() - 1).trim();
            }
            else
            {
                OWNAME = dataEntry[1].trim();
            }
        }

        // export data for tabular showing 
        public Object[] setTable()
        {
            Object[] ret = new Object[tableColNum];
            ret[0] = I.ToString();
            ret[1] = OWNAME;
            return ret;
        }


    }
}
