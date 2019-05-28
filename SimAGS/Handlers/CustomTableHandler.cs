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
        private static PFCase pfProc => CGS.pfProc;

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
            SetTableViewFor(pfCase.ownerArrayList.Cast<AbstractTableViewing>().ToList());
        }

        private static void displayZoneData(PFCase pfCase)
        {
            SetTableViewFor(pfCase.zoneArrayList.Cast<AbstractTableViewing>().ToList());
        }

        private static void displayAreaData(PFCase pfCase)
        {
            SetTableViewFor(pfCase.areaArrayList.Cast<AbstractTableViewing>().ToList());
        }

        private static void displaySWShuntData(PFCase pfCase)
        {
            SetTableViewFor(pfCase.sortBusArrayList.Cast<AbstractTableViewing>().ToList());
        }

        private static void displayTwoWindTransData(PFCase pfCase)
        {
            SetTableViewFor(pfCase.twoWindTransArrayList.Cast<AbstractTableViewing>().ToList());
        }

        private static void displayBranchData(PFCase pfCase)
        {
            SetTableViewFor(pfCase.branchArrayList.Cast<AbstractTableViewing>().ToList());
        }

        private static void displayLoadData(PFCase pfCase)
        {
            SetTableViewFor(pfCase.sortBusArrayList.Select(busTemp => busTemp.busLoads.Cast<AbstractTableViewing>().ToList()).ToList());
        }

        private static void displayGenData(PFCase pfCase)
        {
            SetTableViewFor(pfCase.sortBusArrayList.Select(busTemp => busTemp.busGens.Cast<AbstractTableViewing>().ToList()).ToList());
        }

        private static void displayBusData(PFCase pfCase)
        {
            SetTableViewFor(pfCase.busArrayList.Cast<AbstractTableViewing>().ToList());
        }

        private static void SetTableViewFor(IList<List<AbstractTableViewing>> source)
        {
            var _table = new DataTable();
            source.FirstOrDefault(e => e.Any())?.FirstOrDefault()?.GetHeaders().ToList().ForEach(e => _table.Columns.Add(e));

            foreach (var elems in source)
            {
                foreach (var elem in elems)
                {
                    _table.Rows.AddElem(elem);

                }
            }
            _dataGridView.DataSource = _table;
        }

        private static void SetTableViewFor(IList<AbstractTableViewing> source)
        {
            var _table = new DataTable();
            source.FirstOrDefault()?.GetHeaders().ToList().ForEach(e => _table.Columns.Add(e));

            foreach (var elem in source)
            {
                _table.Rows.AddElem(elem);
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
