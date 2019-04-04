using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimAGS.PfProcessor
{
    public class PfCaseLoad
    {
        private readonly PfCase _pfCase;

        public PfCaseLoad(PfCase pfCase)
        {
            _pfCase = pfCase;
        }

        public void Execute(FileStream pfFile)
        {
        }
    }
}
