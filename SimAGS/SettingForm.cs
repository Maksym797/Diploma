using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimAGS.Handlers;

namespace SimAGS
{
    public partial class SettingForm : Form
    {
        #region props
        private double setSBASE
        {
            get => CGS.setSBASE;
            set => CGS.setSBASE = value;
        } // system base 
        private double setPFTol
        {
            get => CGS.setPFTol;
            set => CGS.setPFTol = value;
        } // system base          // PF tolerance 
        private int setPFMaxItr
        {
            get => CGS.setPFMaxItr;
            set => CGS.setPFMaxItr = value;
        } // system base ;               // maximum iteration number
        private bool bEnableVoltRegLoop
        {
            get => CGS.bEnableVoltRegLoop;
            set => CGS.bEnableVoltRegLoop = value;
        } // system base  // enable voltage regulation 
        private double setVoltRegLoopTol
        {
            get => CGS.setVoltRegLoopTol;
            set => CGS.setVoltRegLoopTol = value;
        } // system base     // voltage regulation mismatch tolerance 

        private double setEndTime
        {
            get => CGS.setEndTime;
            set => CGS.setEndTime = value;
        } // system base             // ending time of simulation
        private double setDyntol
        {
            get => CGS.setDyntol;
            set => CGS.setDyntol = value;
        } // system base             // dynamic tolerance 
        private int setDynMaxItr
        {
            get => CGS.setDynMaxItr;
            set => CGS.setDynMaxItr = value;
        } // system base               // maximum iteration number 
        private double setIntStep
        {
            get => CGS.setIntStep;
            set => CGS.setIntStep = value;
        } // system base            //
        private int NRType
        {
            get => CGS.NRType;
            set => CGS.NRType = value;
        } // system base // 0 --> detailed NR method; 1 --> dishonest NR method 

        private bool bEnableLoadConv
        {
            get => CGS.bEnableLoadConv;
            set => CGS.bEnableLoadConv = value;
        } // system base      // convert load to specified ZIP model (by default constant impedance model of P and Q will be applied by default)? 
        private bool bEnableLoadReq
        {
            get => CGS.bEnableLoadReq;
            set => CGS.bEnableLoadReq = value;
        } // system base ; // enable model frequency component in load modeling?

        private double loadConvZP_Pct
        {
            get => CGS.loadConvZP_Pct;
            set => CGS.loadConvZP_Pct = value;
        } // system base       // percentage of constant impedance MW load
        private double loadConvIP_Pct
        {
            get => CGS.loadConvIP_Pct;
            set => CGS.loadConvIP_Pct = value;
        } // system base         // percentage of constant current MW load 
        private double loadConvPP_Pct
        {
            get => CGS.loadConvPP_Pct;
            set => CGS.loadConvPP_Pct = value;
        } // system base         // percentage of constant power MW load

        private double loadConvZQ_Pct
        {
            get => CGS.loadConvZQ_Pct;
            set => CGS.loadConvZQ_Pct = value;
        } // system base  // percentage of constant impedance MVar load
        private double loadConvIQ_Pct
        {
            get => CGS.loadConvIQ_Pct;
            set => CGS.loadConvIQ_Pct = value;
        } // system base         // percentage of constant current MVar load 
        private double loadConvPQ_Pct
        {
            get => CGS.loadConvPQ_Pct;
            set => CGS.loadConvPQ_Pct = value;
        } // system base         // percentage of constant power MVar load

        private double loadP_FreqCoef
        {
            get => CGS.loadP_FreqCoef;
            set => CGS.loadP_FreqCoef = value;
        } // system base        // frequent component coefficient for MW load
        private double loadQ_FreqCoef
        {
            get => CGS.loadQ_FreqCoef;
            set => CGS.loadQ_FreqCoef = value;
        } // system base 		// frequent component coefficient for MVar load

        #endregion

        public SettingForm()
        {
            InitializeComponent();
        }

        private void SettingForm_Load(object sender, EventArgs e)
        {
            textField_SBASE.Text = setSBASE.ToString();
            textField_PFTol.Text = setPFTol.ToString();
            textField_MaxPFItr.Text = setPFMaxItr.ToString();
            textField_regVoltTol.Text = setVoltRegLoopTol.ToString();

            textField_EndingTime.Text = setEndTime.ToString();
            textField_DynTol.Text = setDyntol.ToString();
            textField_MaxDynItr.Text = setDynMaxItr.ToString();
            textField_intStep.Text = setIntStep.ToString();

            textFieldZPComp.Text =    loadConvZP_Pct.ToString();
            textFieldIPComp.Text =    loadConvIP_Pct.ToString();
            textFieldPPComp.Text =    loadConvPP_Pct.ToString();
            textFieldZQComp.Text =    loadConvZQ_Pct.ToString();
            textFieldIQComp.Text =    loadConvIQ_Pct.ToString();
            textFieldPQComp.Text = loadConvPQ_Pct.ToString();

            textFieldFreqPComp.Text = loadP_FreqCoef.ToString(); 
            textFieldFreqQComp.Text = loadQ_FreqCoef.ToString();  
        }

        public void saveSettingItemActionPerformed()
        {
            setSBASE = Double.Parse(textField_SBASE.Text);
            setPFTol = Double.Parse(textField_PFTol.Text);
            setPFMaxItr = Integer.parseInt(textField_MaxPFItr.Text);
            setVoltRegLoopTol = Double.Parse(textField_regVoltTol.Text);

            setEndTime = Double.Parse(textField_EndingTime.Text);
            setDyntol = Double.Parse(textField_DynTol.Text);
            setDynMaxItr = Integer.parseInt(textField_MaxDynItr.Text);
            setIntStep = Double.Parse(textField_intStep.Text);

            loadConvZP_Pct = Double.Parse(textFieldZPComp.Text);
            loadConvIP_Pct = Double.Parse(textFieldIPComp.Text);
            loadConvPP_Pct = Double.Parse(textFieldPPComp.Text);
            loadConvZQ_Pct = Double.Parse(textFieldZQComp.Text);
            loadConvIQ_Pct = Double.Parse(textFieldIQComp.Text);
            loadConvPQ_Pct = Double.Parse(textFieldPQComp.Text);

            loadP_FreqCoef = Double.Parse(textFieldFreqPComp.Text);
            loadQ_FreqCoef = Double.Parse(textFieldFreqQComp.Text);

            //mainFrame.updatePFPara(setSBASE, setPFTol, setPFMaxItr, bEnableVoltRegLoop, setVoltRegLoopTol);
            //mainFrame.updateSimPara(setEndTime, setDyntol, setDynMaxItr, setIntStep, NRType);
            //mainFrame.updateDynLoadConv(bEnableLoadConv, bEnableLoadReq, loadConvZP_Pct, loadConvIP_Pct, loadConvPP_Pct, loadConvZQ_Pct, loadConvIQ_Pct, loadConvPQ_Pct, loadP_FreqCoef, loadQ_FreqCoef);

            CustomMessageHandler.println("[info] Parameter setting is saved!");
            Close();
        }

        private void Save_Click(object sender, EventArgs e)
        {
            saveSettingItemActionPerformed();
        }
    }
}
