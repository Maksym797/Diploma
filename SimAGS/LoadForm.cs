using System;
using System.Windows.Forms;

namespace SimAGS
{


    public partial class LoadForm : Form
    {
        public LoadForm()
        {
            InitializeComponent();
        }

        private void pf_button_Click(object sender, EventArgs e)
        {
            OpenFileHandler(textBox1);
        }

        private void dyn_button_Click(object sender, EventArgs e)
        {
            OpenFileHandler(textBox2);
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
                SubmitHandler();
            }
            catch (Exception ex)
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



        }

        private void OpenFileHandler(TextBox textBox, string fileType = "All files (*.*)|*.*")
        {
            openFileDialog1.Filter = fileType;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox.Text = openFileDialog1.FileName;
            }
        }
    }
}
