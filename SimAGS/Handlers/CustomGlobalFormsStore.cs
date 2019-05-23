using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimAGS.DynProcessor;
using SimAGS.PfProcessor;

namespace SimAGS.Handlers
{
    public static class CustomGlobalFormsStore
    {
        public static JFile powerFlowCaseFile { get; set; }
        public static PFCase pfProc { get; set; } = new PFCase();
        public static DynCase dynProc { get; set; } = new DynCase();//pfProc); todo 
    }
}
