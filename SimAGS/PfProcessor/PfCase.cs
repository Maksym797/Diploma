using SimAGS.Components;
using System.Collections.Generic;
using System.IO;

namespace SimAGS.PfProcessor
{
    public class PfCase
    {
        public List<Bus> BusArrayList { get; set; }
        public List<Branch> BranchArrayList { get; set; }
        public List<TwoWindTrans> TwoWindTransArrayList { get; set; }
        public List<ThreeWindTrans> ThrWindTransArrayList { get; set; }
        public List<Area> AreaArrayList { get; set; }
        public List<Zone> ZoneArrayList { get; set; }
        public List<Owner> OwnerArrayList { get; set; }
        public List<Bus> SortBusArrayList { get; set; }

        

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
            new PfCaseLoad(this).Exacute(pfFile);
        }
    }
}
