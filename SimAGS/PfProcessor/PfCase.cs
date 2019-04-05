using SimAGS.Components;
using SimAGS.SimUtil;
using System;
using System.Collections.Generic;
using System.IO;

namespace SimAGS.PfProcessor
{
    public class PfCase
    {
        #region state
        public List<Bus> BusArrayList { get; set; }
        public List<Branch> BranchArrayList { get; set; }
        public List<TwoWindTrans> TwoWindTransArrayList { get; set; }
        public List<ThreeWindTrans> ThrWindTransArrayList { get; set; }
        public List<Area> AreaArrayList { get; set; }
        public List<Zone> ZoneArrayList { get; set; }
        public List<Owner> OwnerArrayList { get; set; }
        public List<Bus> SortBusArrayList { get; set; }

        // default system parameters (fixed)
        public const double DefaultBlowup = 1E5;           // terminate power flow calculation if difference is too large 
        public const double DefaultVmax = 1.5;             // maximum voltage for PQ bus
        public const double DefaultVmin = 0.2;             // minimum voltage for PQ bus 
        public const int DefaultVcontrolitre = 10;         // maximum iterations for out voltage control loop (to avoid control oscillation)
        public const double Rad2Deg = 180 / Math.PI;

        // computation control variable
        public double setSBASE;                             // system base
        public double setPFTol;                             // 0.1 MW tolerance
        public int setPFMaxItr;                             // maximum iterations for inner power calculation loop
        public bool bEnableVoltRegLoop;                  // enable voltage regulation (generator remote voltage, switchable shunt, tap-change transformers)
        public double setVoltRegLoopTol;                    // tolerance for generator reactive power overshooting to to avoid PV->PQ oscillations

        public int nBus;                                    // total bus number including dead bus 
        public int nYBus;                                   // total active bus number
        public int nLoad;
        public int nGen;
        public int nBranch;
        public int nTwoWindXfrm;
        public int nThrWindXfrm;
        public int nArea;
        public int nSWShunt;
        public int nZone;
        public int nOwner;

        public double sysPLoss = 0.0;                       // system-wide P losses
        public double sysQLoss = 0.0;                       // system-wide Q losses 

        // extended object list

        // system matrix 
        public YMatrix yMat;                                // system y matrix; 

        // array storing all effective bus numbers 
        public double[,] AVsol;                        // Angle and Vmag 
        public double[,] PQsol;                        // P and Q 
        public double[,] PQobj;                        // P and Q objective for PQ and PV bus (Q for PV and P&Q for SW are zeros)
        public double[,] JMat;                         // System Jacobian matrix 
        public Jacob jacobObj;                              // Jacobian matrix 

        // calculation related variables 
        public bool bInLoopSolved = false;          // power flow inner loop converged 
        public bool bConverged = false;         // power flow out loop converged 

        // voltage regulation 
        public PfVoltageHelper voltHelper;
        public List<Bus> loadBusList = new List<Bus>();
        public List<Bus> genBusList = new List<Bus>();
        public int[] loadQPosArray;
        public int[] genQPosArray;

        // case verification 
        public bool bCaseCheck = true;

        #endregion

        public PfCase()
        {
            BusArrayList = new List<Bus>();
            BranchArrayList = new List<Branch>();
            TwoWindTransArrayList = new List<TwoWindTrans>();
            ThrWindTransArrayList = new List<ThreeWindTrans>();
            AreaArrayList = new List<Area>();
            ZoneArrayList = new List<Zone>();
            OwnerArrayList = new List<Owner>();

            SortBusArrayList = new List<Bus>();
        }

        public void LoadCaseDate(FileStream pfFile)
        {
            new PfCaseLoad(this).Execute(pfFile);
        }
    }
}
