using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using com.sun.corba.se.impl.encoding;
using javax.swing.table;
using SimAGS.Components;
using SimAGS.PfProcessor;

namespace SimAGS.Handlers
{
    public class CustomTableHandler
    {
        private static TreeNode currentTreeNode => CustomTreeViewHandler.GetCurrentNode();
        private static PFCase pfProc => CustomGlobalFormsStore.pfProc;

        private static DataGridView _dataGridView { get; set; }

        private CustomTableHandler() { }

        public static CustomTableHandler updateTableForCurrentTreeNode()
        {
            if (currentTreeNode.equalsIgnoreCase("BUS"))
            {
                displayBusData(pfProc);

            }
            else if (currentTreeNode.equalsIgnoreCase("GENERATOR"))
            {
                displayGenData(pfProc);

            }
            else if (currentTreeNode.equalsIgnoreCase("LOAD"))
            {
                displayLoadData(pfProc);

            }
            else if (currentTreeNode.equalsIgnoreCase("BRANCH"))
            {
                displayBranchData(pfProc);
            }
            else if (currentTreeNode.equalsIgnoreCase("2W TRANSFORMER"))
            {
                displayTwoWindTransData(pfProc);

            }
            else if (currentTreeNode.equalsIgnoreCase("3W TRANSFORMER"))
            {
                //table.displayThrWindTransData(pfProc);
                CustomMessageHandler.println("You select 3-transformer data")
                    .MessBox();
            }
            else if (currentTreeNode.equalsIgnoreCase("SW SHUNT"))
            {
                displaySWShuntData(pfProc);
            
            }
            else if (currentTreeNode.equalsIgnoreCase("AREA"))
            {
                displayAreaData(pfProc);
            
            }
            else if (currentTreeNode.equalsIgnoreCase("ZONE"))
            {
                displayZoneData(pfProc);
            
            }
            else if (currentTreeNode.equalsIgnoreCase("OWNER"))
            {
                displayOwnerData(pfProc);
            }
            else return null;
            return new CustomTableHandler();
        }

        private static void displayOwnerData(PFCase pfCase)
        {
            var _table = new DataTable();
            owner.header.ToList().ForEach(e => _table.Columns.Add(e));

            pfCase.ownerArrayList.ForEach(e => _table.Rows.AddElem(e));
            _dataGridView.DataSource = _table;
        }

        private static void displayZoneData(PFCase pfCase)
        {
            var _table = new DataTable();
            zone.header.ToList().ForEach(e => _table.Columns.Add(e));

            pfCase.zoneArrayList.ForEach(e => _table.Rows.AddElem(e));
            _dataGridView.DataSource = _table;
        }

        private static void displayAreaData(PFCase pfCase)
        {
            var _table = new DataTable();
            area.header.ToList().ForEach(e => _table.Columns.Add(e));

            pfCase.areaArrayList.ForEach(e => _table.Rows.AddElem(e));
            _dataGridView.DataSource = _table;
        }

        private static void displaySWShuntData(PFCase pfCase)
        {
            var _table = new DataTable();
            swshunt.header.ToList().ForEach(e => _table.Columns.Add(e));

            pfCase.sortBusArrayList.ForEach(bt => bt.busSwShunts.ForEach(e => _table.Rows.AddElem(e)));
            _dataGridView.DataSource = _table;
        }

        private static void displayTwoWindTransData(PFCase pfCase)
        {
            var _table = new DataTable();
            twoWindTrans.header.ToList().ForEach(e => _table.Columns.Add(e));

            pfCase.twoWindTransArrayList.ForEach(e => _table.Rows.AddElem(e));
            _dataGridView.DataSource = _table;
        }

        private static void displayBranchData(PFCase pfCase)
        {
            var _table = new DataTable();
            branch.header.ToList().ForEach(e => _table.Columns.Add(e));

            foreach (var elem in pfCase.branchArrayList)
            {
                _table.Rows.AddElem(elem);
            }
            _dataGridView.DataSource = _table;
        }

        private static void displayLoadData(PFCase pfCase)
        {
            var _table = new DataTable();
            load.header.ToList().ForEach(e => _table.Columns.Add(e));

            foreach (var busTemp in pfCase.sortBusArrayList)
            {
                foreach (var load in busTemp.busLoads)
                {
                    _table.Rows.AddElem(load);
                }
            }
            _dataGridView.DataSource = _table;
        }

        private static void displayGenData(PFCase pfCase)
        {
            var _table = new DataTable();
            gen.header.ToList().ForEach(e => _table.Columns.Add(e));

            foreach (var busTemp in pfCase.sortBusArrayList)
            {
                foreach (var gen in busTemp.busGens)
                {
                    _table.Rows.AddElem(gen);
                }
            }
            _dataGridView.DataSource = _table;
        }

        private static void displayBusData(PFCase pfCase)
        {
            var _table = new DataTable();
            bus.header.ToList().ForEach(e => _table.Columns.Add(e));

            foreach (var bus in pfCase.busArrayList)
            {
                _table.Rows.AddElem(bus);
            }
            _dataGridView.DataSource = _table;
        }

        public static CustomTableHandler Config(DataGridView table)
        {
            _dataGridView = table;
            return new CustomTableHandler();
        }
    }
}
