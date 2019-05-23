using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using SimAGS.Handlers;

namespace SimAGS
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            CustomMessageHandler.Config(customConsole);
            CustomTreeViewHandler.Config(treeView1);
            CustomTableHandler.Config(dataGridView1);
            //TestInitializeDataTable();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new LoadForm().Show();
        }

        private void TestInitializeDataTable()
        {
            var source = new BindingSource();

            var table = new DataTable();

            table.Columns.Add("ID");
            table.Columns.Add("Name");

            table.Rows.Add("01", "Saman");
            table.Rows.Add("02", "Max");
            table.Rows.Add("02", "Max");
            table.Rows.Add("02", "Max");
            table.Rows.Add("02", "Max");
            table.Rows.Add("02", "Max");
            table.Rows.Add("02", "Max");
            table.Rows.Add("02", "Max");
            table.Rows.Add("02", "Max");
            table.Rows.Add("02", "Max");
            table.Rows.Add("02", "Max");
            table.Rows.Add("02", "Max");
            table.Rows.Add("02", "Max");
            table.Rows.Add("02", "Max");
            table.Rows.Add("02", "Max");
            table.Rows.Add("02", "Max");
            table.Rows.Add("02", "Max");
            table.Rows.Add("02", "Max");
            table.Rows.Add("02", "Max");
            table.Rows.Add("02", "Max");
            table.Rows.Add("02", "Max");
            table.Rows.Add("02", "Max");
            table.Rows.Add("02", "Max");
            table.Rows.Add("03A", "Max");

            dataGridView1.DataSource = table;

            //var list = new List<string> {"s", "s", "s", "s", "s", "s", "s"};
            //
            //CustomMessageHandler.Show(string.Join("\n",list));
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            CustomTableHandler.updateTableForCurrentTreeNode();
        }
    }
}
