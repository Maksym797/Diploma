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
            
            //TODO REMOVE
            //CallLoadForm();
            CallSettingForm();
        }

        private void CallSettingForm()
        {
            new SettingForm().Show();
            //settingForm.Close();
        }

        #region props

        public double setSBASE
        {
            get => CGS.setSBASE;
            set => CGS.setSBASE = value;
        }             // system base 
        public double setPFTol
        {
            get => CGS.setPFTol;
            set => CGS.setPFTol = value;
        }                   // pf tolerance 
        public int setPFMaxItr
        {
            get => CGS.setPFMaxItr;
            set => CGS.setPFMaxItr = value;
        }                   // maximum iteration number
        public bool bEnableVoltRegLoop
        {
            get => CGS.bEnableVoltRegLoop;
            set => CGS.bEnableVoltRegLoop = value;
        }      // enable voltage regulation loop 
        public double setVoltRegLoopTol
        {
            get => CGS.setSBASE;
            set => CGS.setSBASE = value;
        }          // voltage regulation loop tolerance 

        #endregion

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CallLoadForm();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            CustomTableHandler.updateTableForCurrentTreeNode();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            runPFItemActionPerformed();
        }

        public void CallLoadForm()
        {
            new LoadForm().Show();
        }

        public void runPFItemActionPerformed()
        {
            var startTime = DateTime.Now;

            try
            {
                CGS.pfProc.loadComputePara(setSBASE, setPFTol, setPFMaxItr, bEnableVoltRegLoop, setVoltRegLoopTol);
                CGS.pfProc.buildYMatrix();          // build Y matrix 
                CGS.pfProc.solvePQ();               // calculate power flow 

                CustomMessageHandler.println("Power flow calculation is completed after " + _String.format("%1.5f", (DateTime.Now - startTime).Milliseconds / 1E3) + " seconds");

                // update table element 
                CustomTableHandler.updateTableForCurrentTreeNode();
            }
            catch (simException e)
            {
                //JOptionPane.showMessageDialog(new JFrame(), e.toString(), "Dialog", JOptionPane.ERROR_MESSAGE);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            new LoadForm().Show();
        }
    }
}
