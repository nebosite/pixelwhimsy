using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PixelWhimsy
{
    public partial class PrivacyForm : Form
    {
        public bool ReportErrors
        {
            get { return this.checkBoxErrorReports.Checked; }
        }

        public PrivacyForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}