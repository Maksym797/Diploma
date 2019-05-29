using System;
using java.io;
using System.Windows.Forms;
using SimAGS.DynProcessor;
using SimAGS.Handlers;
using SimAGS.PfProcessor;

namespace SimAGS
{
    public partial class LoadForm : Form
    {
        // general setting of power flow computation 
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

        // global data for interface 
        public File powerFlowCaseFile
        //public JFile powerFlowCaseFile
        {
            get => CGS.powerFlowCaseFile;
            set => CGS.powerFlowCaseFile = value;
        }
        public File dynDataCaseFile;
        public File AGCDataCaseFile;
        public File windDataCaseFile;
        public File contDataCaseFile;
        public File genSchdDataFile;
        public File loadSchdDataFie;

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
            OpenFileHandler(windDataCaseFile_textBox);
        }

        private void wind_button_Click(object sender, EventArgs e)
        {
            OpenFileHandler(AGCDataCaseFile_textBox);
        }

        private void conf_button_Click(object sender, EventArgs e)
        {
            OpenFileHandler(contDataCaseFile_textBox);
        }

        private void Submit_Click(object sender, EventArgs e)
        {
            try
            {
                powerFlowCaseFile = string.IsNullOrEmpty(powerFlowCaseFile_textBox.Text) ? null : new File(powerFlowCaseFile_textBox.Text);
                dynDataCaseFile = string.IsNullOrEmpty(dynDataCaseFile_textBox.Text) ? null : new File(dynDataCaseFile_textBox.Text);
                AGCDataCaseFile = string.IsNullOrEmpty(AGCDataCaseFile_textBox.Text) ? null : new File(AGCDataCaseFile_textBox.Text);
                windDataCaseFile = string.IsNullOrEmpty(windDataCaseFile_textBox.Text) ? null : new File(windDataCaseFile_textBox.Text);
                contDataCaseFile = string.IsNullOrEmpty(contDataCaseFile_textBox.Text) ? null : new File(contDataCaseFile_textBox.Text);
                // todo genSchdDataFile = new File(_textBox.Text);
                // todo loadSchdDataFie = new File(_textBox.Text);

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


        private void SubmitHandler()
        {
            var config = SimConfig.GetConfig();
            try
            {
                if (powerFlowCaseFile == null) throw new simException("Error: power flow file must be specified");

                // load power flow data 
                pfProc = new PFCase();
                pfProc.LoadCaseData(powerFlowCaseFile);
                pfProc.ini();
                //*
                //*// build dynamic simulation object so that the data can be loaded
                dynProc = new DynCase(pfProc);
                dynProc.loadCaseFile(dynDataCaseFile, AGCDataCaseFile, windDataCaseFile, contDataCaseFile, genSchdDataFile, loadSchdDataFie);   // load data and assign memories for variables; and create buffer to store simulation results
                //*
                //*//dynamically build model tree 
                CustomTreeViewHandler.loadModelTree();
                //*
                //*// update current table if a new case is loaded 
                CustomTableHandler.updateTableForCurrentTreeNode();

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
