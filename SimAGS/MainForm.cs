using System;
using System.Data;
using System.Windows.Forms;

namespace SimAGS
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            TestInitializeDataTable();
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
        }
    }
}
