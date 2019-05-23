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
        private static TreeNode CurrentTreeNode => CustomTreeViewHandler.GetCurrentNode();
        private static PFCase pfProc => CustomGlobalFormsStore.pfProc;

        private static DataGridView _dataGridView { get; set; }

        private CustomTableHandler() { }

        public static CustomTableHandler updateTableForCurrentTreeNode()
        {
            if (CurrentTreeNode.equalsIgnoreCase("BUS"))
            {
                displayBusData(pfProc);

            }
            return new CustomTableHandler();
            //else if (currentTreeNode.equalsIgnoreCase("GENERATOR"))
            //{
            //    table.displayGenData(pfProc);
            //
            //}
            //else if (currentTreeNode.equalsIgnoreCase("LOAD"))
            //{
            //    table.displayLoadData(pfProc);
            //
            //}
            //else if (currentTreeNode.equalsIgnoreCase("BRANCH"))
            //{
            //    table.displayBranchData(pfProc);
            //
            //}
            //else if (currentTreeNode.equalsIgnoreCase("2W TRANSFORMER"))
            //{
            //    table.displayTwoWindTransData(pfProc);
            //
            //}
            //else if (currentTreeNode.equalsIgnoreCase("3W TRANSFORMER"))
            //{
            //    //table.displayThrWindTransData(pfProc);
            //    System.out.println("You select 3-transformer data");
            //
            //}
            //else if (currentTreeNode.equals("SW SHUNT"))
            //{
            //    table.displaySWShuntData(pfProc);
            //
            //}
            //else if (currentTreeNode.equalsIgnoreCase("AREA"))
            //{
            //    table.displayAreaData(pfProc);
            //
            //}
            //else if (currentTreeNode.equalsIgnoreCase("ZONE"))
            //{
            //    table.displayZoneData(pfProc);
            //
            //}
            //else if (currentTreeNode.equalsIgnoreCase("OWNER"))
            //{
            //    table.displayOwnerData(pfProc);
            //}
        }

        private static void displayBusData(PFCase pfCase)
        {
            var _table = new DataTable();
            bus.header.ToList().ForEach(e => _table.Columns.Add(e));

            foreach (var bus in pfCase.busArrayList)
            {
                _table.Rows.AddBus(bus);
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
