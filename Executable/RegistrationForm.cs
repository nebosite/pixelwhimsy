using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Xml;
using System.Text.RegularExpressions;

namespace PixelWhimsy
{
    public partial class RegistrationForm : Form
    {
        bool registered = false;
        public bool Registered { get { return registered; } }
        bool shouldClose = false;

        /// ------------------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        /// ------------------------------------------------------------
        public RegistrationForm()
        {
            InitializeComponent();
            if(Settings.Expired)
            {
                labelDays.Text = "The evaluation period has expired.";
            }
            else{
                labelDays.Text = "This is day " +
                    (int)(Settings.EvaluationDays + 1) +
                    " of a 30-day evaluation period.";
            }
        }

        /// ------------------------------------------------------------
        /// <summary>
        /// Go to the registration page
        /// </summary>
        /// ------------------------------------------------------------
        private void buttonRegister_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.pixelwhimsy.com"); 
        }

        /// ------------------------------------------------------------
        /// <summary>
        /// Quite and do nothing
        /// </summary>
        /// ------------------------------------------------------------
        private void buttonLater_Click(object sender, EventArgs e)
        {
            shouldClose = true;
            Close();
        }

        /// ------------------------------------------------------------
        /// <summary>
        /// Try to validate the registration code
        /// </summary>
        /// ------------------------------------------------------------
        private void buttonOk_Click(object sender, EventArgs e)
        {
            string code = textBoxCode.Text.Replace(" ", "").Trim().ToLower();
            bool overMaxCount = false;

            if (!Regex.IsMatch(code, "[0-9a-z]{16}"))
            {
                MessageBox.Show("That registration code is not a valid. \r\nPlease make " +
                    "sure it has been typed in correctly.", "Registration Error");
                return;
            }

            string uriString = "http://aspx.pixelwhimsy.com/pixelwhimsy/reghandler.aspx?" +
                "pc=" + code +"&" +
                "ver=" + AssemblyConstants.Version + "&" +
                "id=" + Utilities.GetMacAddress();

            textBoxCode.Hide();
            buttonOk.Hide();
            labelVerifying.Show();
            Application.DoEvents();

            WebClient myWebClient = new WebClient();

            try
            {
                byte[] responseArray = myWebClient.DownloadData(uriString);
                string result = Encoding.ASCII.GetString(responseArray);

                XmlDocument returnData = new XmlDocument();
                returnData.LoadXml(result);

                XmlNode node = returnData.SelectSingleNode("RegistrationResult");

                bool succeeded = bool.Parse(node.Attributes["succeeded"].Value);
                if (!succeeded)
                {
                    MessageBox.Show("We are sorry, but we could not register this product for the following reason: \r\n\r\n" + node.Attributes["reason"].Value + "\r\n\r\nPlease try again.");
                }
                else
                {
                    overMaxCount = bool.Parse(node.Attributes["overmaxcount"].Value);
                    registered = succeeded;
                }
            }
            catch (Exception)
            {
                registered = true;
            }
            finally { }

            if (registered)
            {
                if(overMaxCount)
                {
                    MessageBox.Show("ALERT:  The product code you have used to register PixelWhimsy " +
                        "has been used more times than the purchased license allows.  It is possible " +
                        "that this key has been pirated.  If you are not the purchaser of this license, " +
                        "please visit http://www.pixelwhimsy.com and purchase a valid registration code.");
                }
                else
                {
                    MessageBox.Show("Congratulations!  You have successfully registered PixelWhimsy.  Thank you for your purchase!");
                }

                shouldClose = true;
                Close();
            }
            textBoxCode.Show();
            buttonOk.Show();
            labelVerifying.Hide();
        }

        private void RegistrationForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!shouldClose)
            {
                e.Cancel = true;
            }
            base.OnClosing(e); 

        }

    }
}