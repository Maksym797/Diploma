using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimAGS.DynProcessor;
using SimAGS.PfProcessor;

namespace SimAGS.Handlers
{
    public static class CGS //CustomGlobalFormsStore
    {
        public static double setSBASE {get;set;} = 100.0;            // system base 
        public static double setPFTol {get;set;} = 1e-4;         // PF tolerance 
        public static int setPFMaxItr {get;set;} = 20;               // maximum iteration number
        public static bool bEnableVoltRegLoop {get;set;} = false; // enable voltage regulation 
        public static double setVoltRegLoopTol {get;set;} = 1e-3;    // voltage regulation mismatch tolerance 
        public static double setEndTime {get;set;} = 5.0;            // ending time of simulation
        public static double setDyntol {get;set;} = 1e-4;            // dynamic tolerance 
        public static int setDynMaxItr {get;set;} = 20;              // maximum iteration number 
        public static double setIntStep {get;set;} = 0.02;           //
        public static int NRType {get;set;} = 0;             // 0 --> detailed NR method; 1 --> dishonest NR method 
        public static bool bEnableLoadConv {get;set;} = true;     // convert load to specified ZIP model (by default constant impedance model of P and Q will be applied by default)? 
        public static bool bEnableLoadReq {get;set;} = false; // enable model frequency component in load modeling?
        public static double loadConvZP_Pct {get;set;} = 100.0;      // percentage of constant impedance MW load
        public static double loadConvIP_Pct {get;set;} = 0.0;        // percentage of constant current MW load 
        public static double loadConvPP_Pct {get;set;} = 0.0;        // percentage of constant power MW load
        public static double loadConvZQ_Pct {get;set;} = 100.0;      // percentage of constant impedance MVar load
        public static double loadConvIQ_Pct {get;set;} = 0.0;        // percentage of constant current MVar load 
        public static double loadConvPQ_Pct {get;set;} = 0.0;        // percentage of constant power MVar load
        public static double loadP_FreqCoef {get;set;} = 0.0;        // frequent component coefficient for MW load
        public static double loadQ_FreqCoef {get;set;} = 0.0;		// frequent component coefficient for MVar load


        public static JFile powerFlowCaseFile { get; set; }
        public static PFCase pfProc { get; set; } = new PFCase();
        public static DynCase dynProc { get; set; } = new DynCase(pfProc);//pfProc); todo 
    }
}
