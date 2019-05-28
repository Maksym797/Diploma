using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimAGS.Handlers
{
    public abstract class AbstractTableViewing : IHasTableView
    {
        public abstract String[] header { get; set; }

        public abstract string[] AsArrayForRow();

        public string[] GetHeaders()
        {
            return header;
        }
    }
}
