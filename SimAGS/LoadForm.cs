using System;
using System.IO;
using System.Windows.Forms;
using SimAGS.DynProcessor;
using SimAGS.Handlers;
using SimAGS.PfProcessor;

namespace SimAGS
{
    public partial class LoadForm : Form
    {
        // general setting of power flow computation 
        public double setSBASE{
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

        // general setting of dynamic simulation 
        public double setEndTime;           // ending time of simulation
        public double setDyntol;            // dynamic tolerance 
        public double setIntStep;           // time step 
        public double AGCStep;              // AGC time step 
        public int setDynMaxItr;            // maximum iteration number 
        public int NRType;                  // 0 --> detailed NR method; 1 --> dishonest NR method 

        // dynamic load conversion related 
        public bool bEnableLoadConv;     // convert load to specified ZIP model (if enabled, constant impedance model of P and Q will be applied by default)? 
        public bool bEnableLoadReq;      // enable model frequency component in load modeling?
        public double loadConvZP_Pct;       // percentage of constant impedance MW load
        public double loadConvIP_Pct;       // percentage of constant current MW load 
        public double loadConvPP_Pct;       // percentage of constant power MW load
        public double loadConvZQ_Pct;       // percentage of constant impedance MVar load
        public double loadConvIQ_Pct;       // percentage of constant current MVar load 
        public double loadConvPQ_Pct;       // percentage of constant power MVar load
        public double loadP_FreqCoef;       // frequent component coefficient for MW load
        public double loadQ_FreqCoef;       // frequent component coefficient for MVar load

        // case data 
        public PFCase pfProc
        {
            get => CGS.pfProc;
            set => CGS.pfProc = value;
        }
        public DynCase dynProc
        {
            get => CGS.dynProc;
            set => CGS.dynProc = value;
        }

        // global data for interface 
        public JFile powerFlowCaseFile
        {
            get => CGS.powerFlowCaseFile;
            set => CGS.powerFlowCaseFile = value;
        }
        public JFile dynDataCaseFile;
        public JFile AGCDataCaseFile;
        public JFile windDataCaseFile;
        public JFile contDataCaseFile;
        public JFile genSchdDataFile;
        public JFile loadSchdDataFie;

        // frame elements
        //*private JPanel contentPanel;
        //*private modelTable table;
        //*private JTree modelTree;
        //*private String currentTreeNode = "";                // update current table element 

        //*private caseLoadPanel caseLoadPaneWindow;
        //*private settingPanel simSetParWindow;
        //*private resultViewer resultViewWindow;
        //*private helpPanel helpWindow;
        public LoadForm()
        {
            InitializeComponent();
        }

        private void pf_button_Click(object sender, EventArgs e)
        {
            OpenFileHandler(powerFlowCaseFile_textBox);
        }

        private void dyn_button_Click(object sender, EventArgs e)
        {
            OpenFileHandler(dynDataCaseFile_textBox);
        }

        private void ags_button_Click(object sender, EventArgs e)
        {
            OpenFileHandler(textBox3);
        }

        private void wind_button_Click(object sender, EventArgs e)
        {
            OpenFileHandler(textBox4);
        }

        private void conf_button_Click(object sender, EventArgs e)
        {
            OpenFileHandler(textBox5);
        }

        private void Submit_Click(object sender, EventArgs e)
        {
            try
            {
                powerFlowCaseFile = new JFile(powerFlowCaseFile_textBox.Text);

                SubmitHandler();
                Close();
            }
            catch
            {
                throw;
            }
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        //private void Validate()
        //{
        // TODO
        //}

        private void SubmitHandler()
        {
            var config = SimConfig.GetConfig();
            try
            {
                //*if (powerFlowCaseFile == null) throw new simException("Error: power flow file must be specified");

                // load power flow data 
                pfProc = new PFCase();
                pfProc.LoadCaseData(powerFlowCaseFile);
                pfProc.ini();
                //*
                //*// build dynamic simulation object so that the data can be loaded
                dynProc = new DynCase(pfProc);
                //*dynProc.loadCaseFile(dynDataCaseFile, AGCDataCaseFile, windDataCaseFile, contDataCaseFile, genSchdDataFile, loadSchdDataFie);   // load data and assign memories for variables; and create buffer to store simulation results
                //*
                //*//dynamically build model tree 
                CustomTreeViewHandler.loadModelTree();
                //*
                //*// update current table if a new case is loaded 
                //CustomTableHandler.updateTableForCurrentTreeNode();

            }
            catch (Exception e)
            {
                throw;
            }
        }

        private void OpenFileHandler(TextBox textBox, string fileType = "All files (*.*)|*.*")
        {
            openFileDialog1.Filter = fileType;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox.Text = openFileDialog1.FileName;
            }
        }

        private void LoadForm_Load(object sender, EventArgs e)
        {
            //TODO remove
            Submit_Click(sender, e);
        }
    }
}
