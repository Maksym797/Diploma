namespace SimAGS
{
    partial class SettingForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textField_SBASE = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textField_PFTol = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textField_MaxPFItr = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textField_regVoltTol = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.textField_EndingTime = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textField_DynTol = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.textField_MaxDynItr = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.textField_intStep = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.label12 = new System.Windows.Forms.Label();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.textFieldZPComp = new System.Windows.Forms.TextBox();
            this.textFieldZQComp = new System.Windows.Forms.TextBox();
            this.textFieldIPComp = new System.Windows.Forms.TextBox();
            this.textFieldIQComp = new System.Windows.Forms.TextBox();
            this.textFieldPPComp = new System.Windows.Forms.TextBox();
            this.textFieldPQComp = new System.Windows.Forms.TextBox();
            this.textFieldFreqQComp = new System.Windows.Forms.TextBox();
            this.textFieldFreqPComp = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.Save = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textField_SBASE
            // 
            this.textField_SBASE.Location = new System.Drawing.Point(197, 23);
            this.textField_SBASE.Name = "textField_SBASE";
            this.textField_SBASE.Size = new System.Drawing.Size(101, 22);
            this.textField_SBASE.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(32, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(131, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "System Base [MVA]";
            // 
            // textField_PFTol
            // 
            this.textField_PFTol.Location = new System.Drawing.Point(197, 51);
            this.textField_PFTol.Name = "textField_PFTol";
            this.textField_PFTol.Size = new System.Drawing.Size(101, 22);
            this.textField_PFTol.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(32, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(121, 17);
            this.label2.TabIndex = 4;
            this.label2.Text = "PF Tolerance [pu]";
            // 
            // textField_MaxPFItr
            // 
            this.textField_MaxPFItr.Location = new System.Drawing.Point(197, 79);
            this.textField_MaxPFItr.Name = "textField_MaxPFItr";
            this.textField_MaxPFItr.Size = new System.Drawing.Size(101, 22);
            this.textField_MaxPFItr.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(32, 82);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(109, 17);
            this.label3.TabIndex = 6;
            this.label3.Text = "Max PF Iteration";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(32, 110);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(113, 17);
            this.label4.TabIndex = 8;
            this.label4.Text = "PF Voltage Loop";
            // 
            // textField_regVoltTol
            // 
            this.textField_regVoltTol.Location = new System.Drawing.Point(197, 135);
            this.textField_regVoltTol.Name = "textField_regVoltTol";
            this.textField_regVoltTol.Size = new System.Drawing.Size(101, 22);
            this.textField_regVoltTol.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(32, 138);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(166, 17);
            this.label5.TabIndex = 10;
            this.label5.Text = "Reg. Volt. Tolerance [pu]";
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label6.Location = new System.Drawing.Point(5, 173);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(345, 2);
            this.label6.TabIndex = 12;
            // 
            // textField_EndingTime
            // 
            this.textField_EndingTime.Location = new System.Drawing.Point(197, 186);
            this.textField_EndingTime.Name = "textField_EndingTime";
            this.textField_EndingTime.Size = new System.Drawing.Size(101, 22);
            this.textField_EndingTime.TabIndex = 14;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(32, 189);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(106, 17);
            this.label7.TabIndex = 13;
            this.label7.Text = "Ending Time [s]";
            // 
            // textField_DynTol
            // 
            this.textField_DynTol.Location = new System.Drawing.Point(197, 214);
            this.textField_DynTol.Name = "textField_DynTol";
            this.textField_DynTol.Size = new System.Drawing.Size(101, 22);
            this.textField_DynTol.TabIndex = 16;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(32, 217);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(105, 17);
            this.label8.TabIndex = 15;
            this.label8.Text = "Dyn. Tolerance";
            // 
            // textField_MaxDynItr
            // 
            this.textField_MaxDynItr.Location = new System.Drawing.Point(197, 242);
            this.textField_MaxDynItr.Name = "textField_MaxDynItr";
            this.textField_MaxDynItr.Size = new System.Drawing.Size(101, 22);
            this.textField_MaxDynItr.TabIndex = 18;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(32, 245);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(121, 17);
            this.label9.TabIndex = 17;
            this.label9.Text = "Max Dyn. Iteration";
            // 
            // textField_intStep
            // 
            this.textField_intStep.Location = new System.Drawing.Point(197, 270);
            this.textField_intStep.Name = "textField_intStep";
            this.textField_intStep.Size = new System.Drawing.Size(101, 22);
            this.textField_intStep.TabIndex = 20;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(32, 273);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(127, 17);
            this.label10.TabIndex = 19;
            this.label10.Text = "Integration Step [s]";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(32, 301);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(115, 17);
            this.label11.TabIndex = 21;
            this.label11.Text = "Jacobian Update";
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "Detailed NR",
            "Dishonest NR"});
            this.comboBox1.Location = new System.Drawing.Point(197, 301);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 24);
            this.comboBox1.TabIndex = 22;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(197, 109);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(74, 21);
            this.checkBox1.TabIndex = 23;
            this.checkBox1.Text = "Enable";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(197, 330);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(74, 21);
            this.checkBox2.TabIndex = 25;
            this.checkBox2.Text = "Enable";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(32, 331);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(115, 17);
            this.label12.TabIndex = 24;
            this.label12.Text = "Load Conversion";
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Location = new System.Drawing.Point(197, 357);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(74, 21);
            this.checkBox3.TabIndex = 27;
            this.checkBox3.Text = "Enable";
            this.checkBox3.UseVisualStyleBackColor = true;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(32, 358);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(153, 17);
            this.label13.TabIndex = 26;
            this.label13.Text = "Load Freq. Component";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(73, 415);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(39, 17);
            this.label14.TabIndex = 28;
            this.label14.Text = "Z(%)";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(138, 415);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(37, 17);
            this.label15.TabIndex = 29;
            this.label15.Text = "I (%)";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(194, 415);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(43, 17);
            this.label16.TabIndex = 30;
            this.label16.Text = "P (%)";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(38, 443);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(17, 17);
            this.label18.TabIndex = 32;
            this.label18.Text = "P";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(38, 475);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(19, 17);
            this.label19.TabIndex = 33;
            this.label19.Text = "Q";
            // 
            // textFieldZPComp
            // 
            this.textFieldZPComp.Location = new System.Drawing.Point(69, 440);
            this.textFieldZPComp.Name = "textFieldZPComp";
            this.textFieldZPComp.Size = new System.Drawing.Size(51, 22);
            this.textFieldZPComp.TabIndex = 34;
            // 
            // textFieldZQComp
            // 
            this.textFieldZQComp.Location = new System.Drawing.Point(69, 472);
            this.textFieldZQComp.Name = "textFieldZQComp";
            this.textFieldZQComp.Size = new System.Drawing.Size(51, 22);
            this.textFieldZQComp.TabIndex = 35;
            // 
            // textFieldIPComp
            // 
            this.textFieldIPComp.Location = new System.Drawing.Point(132, 440);
            this.textFieldIPComp.Name = "textFieldIPComp";
            this.textFieldIPComp.Size = new System.Drawing.Size(51, 22);
            this.textFieldIPComp.TabIndex = 36;
            // 
            // textFieldIQComp
            // 
            this.textFieldIQComp.Location = new System.Drawing.Point(132, 472);
            this.textFieldIQComp.Name = "textFieldIQComp";
            this.textFieldIQComp.Size = new System.Drawing.Size(51, 22);
            this.textFieldIQComp.TabIndex = 37;
            // 
            // textFieldPPComp
            // 
            this.textFieldPPComp.Location = new System.Drawing.Point(193, 440);
            this.textFieldPPComp.Name = "textFieldPPComp";
            this.textFieldPPComp.Size = new System.Drawing.Size(51, 22);
            this.textFieldPPComp.TabIndex = 38;
            // 
            // textFieldPQComp
            // 
            this.textFieldPQComp.Location = new System.Drawing.Point(193, 472);
            this.textFieldPQComp.Name = "textFieldPQComp";
            this.textFieldPQComp.Size = new System.Drawing.Size(51, 22);
            this.textFieldPQComp.TabIndex = 39;
            // 
            // textFieldFreqQComp
            // 
            this.textFieldFreqQComp.Location = new System.Drawing.Point(255, 472);
            this.textFieldFreqQComp.Name = "textFieldFreqQComp";
            this.textFieldFreqQComp.Size = new System.Drawing.Size(51, 22);
            this.textFieldFreqQComp.TabIndex = 42;
            // 
            // textFieldFreqPComp
            // 
            this.textFieldFreqPComp.Location = new System.Drawing.Point(255, 440);
            this.textFieldFreqPComp.Name = "textFieldFreqPComp";
            this.textFieldFreqPComp.Size = new System.Drawing.Size(51, 22);
            this.textFieldFreqPComp.TabIndex = 41;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(260, 415);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(38, 17);
            this.label17.TabIndex = 40;
            this.label17.Text = "f(pu)";
            // 
            // Save
            // 
            this.Save.Location = new System.Drawing.Point(141, 520);
            this.Save.Name = "Save";
            this.Save.Size = new System.Drawing.Size(75, 23);
            this.Save.TabIndex = 43;
            this.Save.Text = "Save";
            this.Save.UseVisualStyleBackColor = true;
            this.Save.Click += new System.EventHandler(this.Save_Click);
            // 
            // SettingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(350, 567);
            this.Controls.Add(this.Save);
            this.Controls.Add(this.textFieldFreqQComp);
            this.Controls.Add(this.textFieldFreqPComp);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.textFieldPQComp);
            this.Controls.Add(this.textFieldPPComp);
            this.Controls.Add(this.textFieldIQComp);
            this.Controls.Add(this.textFieldIPComp);
            this.Controls.Add(this.textFieldZQComp);
            this.Controls.Add(this.textFieldZPComp);
            this.Controls.Add(this.label19);
            this.Controls.Add(this.label18);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.checkBox3);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.textField_intStep);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.textField_MaxDynItr);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.textField_DynTol);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.textField_EndingTime);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.textField_regVoltTol);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textField_MaxPFItr);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textField_PFTol);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textField_SBASE);
            this.Controls.Add(this.label1);
            this.Name = "SettingForm";
            this.Text = "SettingForm";
            this.Load += new System.EventHandler(this.SettingForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textField_SBASE;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textField_PFTol;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textField_MaxPFItr;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textField_regVoltTol;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textField_EndingTime;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textField_DynTol;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textField_MaxDynItr;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textField_intStep;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TextBox textFieldZPComp;
        private System.Windows.Forms.TextBox textFieldZQComp;
        private System.Windows.Forms.TextBox textFieldIPComp;
        private System.Windows.Forms.TextBox textFieldIQComp;
        private System.Windows.Forms.TextBox textFieldPPComp;
        private System.Windows.Forms.TextBox textFieldPQComp;
        private System.Windows.Forms.TextBox textFieldFreqQComp;
        private System.Windows.Forms.TextBox textFieldFreqPComp;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Button Save;
    }
}