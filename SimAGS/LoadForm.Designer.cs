namespace SimAGS
{
    partial class LoadForm
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
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.pf_button = new System.Windows.Forms.Button();
            this.powerFlowCaseFile_textBox = new System.Windows.Forms.TextBox();
            this.dynDataCaseFile_textBox = new System.Windows.Forms.TextBox();
            this.dyn_button = new System.Windows.Forms.Button();
            this.windDataCaseFile_textBox = new System.Windows.Forms.TextBox();
            this.wind_button = new System.Windows.Forms.Button();
            this.AGCDataCaseFile_textBox = new System.Windows.Forms.TextBox();
            this.ags_button = new System.Windows.Forms.Button();
            this.contDataCaseFile_textBox = new System.Windows.Forms.TextBox();
            this.conf_button = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.Submit = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // pf_button
            // 
            this.pf_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.pf_button.Location = new System.Drawing.Point(509, 41);
            this.pf_button.Name = "pf_button";
            this.pf_button.Size = new System.Drawing.Size(75, 26);
            this.pf_button.TabIndex = 0;
            this.pf_button.Text = "...";
            this.pf_button.UseVisualStyleBackColor = true;
            this.pf_button.Click += new System.EventHandler(this.pf_button_Click);
            // 
            // powerFlowCaseFile_textBox
            // 
            this.powerFlowCaseFile_textBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.powerFlowCaseFile_textBox.Location = new System.Drawing.Point(182, 41);
            this.powerFlowCaseFile_textBox.Name = "powerFlowCaseFile_textBox";
            this.powerFlowCaseFile_textBox.Size = new System.Drawing.Size(312, 26);
            this.powerFlowCaseFile_textBox.TabIndex = 1;
            this.powerFlowCaseFile_textBox.Text = "C:\\Users\\Maksym_Misiuchenko\\_Diploma\\SimAGC_03052014_final_ver (1)\\SimAGC\\data\\ku" +
    "nder_case\\new_TwoAreaTest.raw";
            // 
            // dynDataCaseFile_textBox
            // 
            this.dynDataCaseFile_textBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.dynDataCaseFile_textBox.Location = new System.Drawing.Point(182, 90);
            this.dynDataCaseFile_textBox.Name = "dynDataCaseFile_textBox";
            this.dynDataCaseFile_textBox.Size = new System.Drawing.Size(312, 26);
            this.dynDataCaseFile_textBox.TabIndex = 3;
            this.dynDataCaseFile_textBox.Text = "C:\\Users\\Maksym_Misiuchenko\\_Diploma\\SimAGC_03052014_final_ver (1)\\SimAGC\\data\\ku" +
    "nder_case\\DYN_TwoAreaTest.dyr";
            // 
            // dyn_button
            // 
            this.dyn_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.dyn_button.Location = new System.Drawing.Point(509, 90);
            this.dyn_button.Name = "dyn_button";
            this.dyn_button.Size = new System.Drawing.Size(75, 26);
            this.dyn_button.TabIndex = 2;
            this.dyn_button.Text = "...";
            this.dyn_button.UseVisualStyleBackColor = true;
            this.dyn_button.Click += new System.EventHandler(this.dyn_button_Click);
            // 
            // windDataCaseFile_textBox
            // 
            this.windDataCaseFile_textBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.windDataCaseFile_textBox.Location = new System.Drawing.Point(182, 188);
            this.windDataCaseFile_textBox.Name = "windDataCaseFile_textBox";
            this.windDataCaseFile_textBox.Size = new System.Drawing.Size(312, 26);
            this.windDataCaseFile_textBox.TabIndex = 7;
            // 
            // wind_button
            // 
            this.wind_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.wind_button.Location = new System.Drawing.Point(509, 188);
            this.wind_button.Name = "wind_button";
            this.wind_button.Size = new System.Drawing.Size(75, 26);
            this.wind_button.TabIndex = 6;
            this.wind_button.Text = "...";
            this.wind_button.UseVisualStyleBackColor = true;
            this.wind_button.Click += new System.EventHandler(this.wind_button_Click);
            // 
            // AGCDataCaseFile_textBox
            // 
            this.AGCDataCaseFile_textBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.AGCDataCaseFile_textBox.Location = new System.Drawing.Point(182, 139);
            this.AGCDataCaseFile_textBox.Name = "AGCDataCaseFile_textBox";
            this.AGCDataCaseFile_textBox.Size = new System.Drawing.Size(312, 26);
            this.AGCDataCaseFile_textBox.TabIndex = 5;
            // 
            // ags_button
            // 
            this.ags_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.ags_button.Location = new System.Drawing.Point(509, 139);
            this.ags_button.Name = "ags_button";
            this.ags_button.Size = new System.Drawing.Size(75, 26);
            this.ags_button.TabIndex = 4;
            this.ags_button.Text = "...";
            this.ags_button.UseVisualStyleBackColor = true;
            this.ags_button.Click += new System.EventHandler(this.ags_button_Click);
            // 
            // contDataCaseFile_textBox
            // 
            this.contDataCaseFile_textBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.contDataCaseFile_textBox.Location = new System.Drawing.Point(182, 236);
            this.contDataCaseFile_textBox.Name = "contDataCaseFile_textBox";
            this.contDataCaseFile_textBox.Size = new System.Drawing.Size(312, 26);
            this.contDataCaseFile_textBox.TabIndex = 9;
            // 
            // conf_button
            // 
            this.conf_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.conf_button.Location = new System.Drawing.Point(509, 236);
            this.conf_button.Name = "conf_button";
            this.conf_button.Size = new System.Drawing.Size(75, 26);
            this.conf_button.TabIndex = 8;
            this.conf_button.Text = "...";
            this.conf_button.UseVisualStyleBackColor = true;
            this.conf_button.Click += new System.EventHandler(this.conf_button_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label1.Location = new System.Drawing.Point(28, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(118, 20);
            this.label1.TabIndex = 10;
            this.label1.Text = "Power flow file";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label2.Location = new System.Drawing.Point(28, 93);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(112, 20);
            this.label2.TabIndex = 11;
            this.label2.Text = "Dynamic data";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label3.Location = new System.Drawing.Point(28, 139);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(108, 20);
            this.label3.TabIndex = 12;
            this.label3.Text = "AGS data file";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label4.Location = new System.Drawing.Point(28, 191);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(111, 20);
            this.label4.TabIndex = 13;
            this.label4.Text = "Wind data file";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label5.Location = new System.Drawing.Point(29, 239);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(128, 20);
            this.label5.TabIndex = 14;
            this.label5.Text = "Contingency file";
            // 
            // Submit
            // 
            this.Submit.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.Submit.Location = new System.Drawing.Point(182, 289);
            this.Submit.Name = "Submit";
            this.Submit.Size = new System.Drawing.Size(100, 33);
            this.Submit.TabIndex = 15;
            this.Submit.Text = "Submit";
            this.Submit.UseVisualStyleBackColor = true;
            this.Submit.Click += new System.EventHandler(this.Submit_Click);
            // 
            // Cancel
            // 
            this.Cancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.Cancel.Location = new System.Drawing.Point(332, 289);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(100, 33);
            this.Cancel.TabIndex = 16;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // LoadForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(636, 348);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.Submit);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.contDataCaseFile_textBox);
            this.Controls.Add(this.conf_button);
            this.Controls.Add(this.windDataCaseFile_textBox);
            this.Controls.Add(this.wind_button);
            this.Controls.Add(this.AGCDataCaseFile_textBox);
            this.Controls.Add(this.ags_button);
            this.Controls.Add(this.dynDataCaseFile_textBox);
            this.Controls.Add(this.dyn_button);
            this.Controls.Add(this.powerFlowCaseFile_textBox);
            this.Controls.Add(this.pf_button);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "LoadForm";
            this.Text = "Case loading";
            this.Load += new System.EventHandler(this.LoadForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button pf_button;
        private System.Windows.Forms.TextBox powerFlowCaseFile_textBox;
        private System.Windows.Forms.TextBox dynDataCaseFile_textBox;
        private System.Windows.Forms.Button dyn_button;
        private System.Windows.Forms.TextBox windDataCaseFile_textBox;
        private System.Windows.Forms.Button wind_button;
        private System.Windows.Forms.TextBox AGCDataCaseFile_textBox;
        private System.Windows.Forms.Button ags_button;
        private System.Windows.Forms.TextBox contDataCaseFile_textBox;
        private System.Windows.Forms.Button conf_button;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button Submit;
        private System.Windows.Forms.Button Cancel;
    }
}