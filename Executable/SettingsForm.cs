using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DirectVarmint;

namespace PixelWhimsy
{
    /// <summary>
    /// Settings for the PixelCount
    /// </summary>
    public enum PixelCount
    {
        Low,
        Medium,
        High
    }


    /// ---------------------------------------------------
    /// <summary>
    /// Class for settings
    /// </summary>
    /// ---------------------------------------------------
    public partial class SettingsForm : Form
    {
        SoundPlayer soundPlayer;

        public PixelCount PixelCount
        {
            get
            {
                if (this.radioButtonPixelCountLow.Checked) return PixelCount.Low;
                if (this.radioButtonPixelCountHigh.Checked) return PixelCount.High;
                return PixelCount.Medium;
            }

        }

        public SettingsForm()
        {
            InitializeComponent();

            switch (Settings.PixelCount)
            {
                case PixelCount.Low: radioButtonPixelCountLow.Checked = true; break;
                case PixelCount.Medium: radioButtonPixelCountMedium.Checked = true; break;
                case PixelCount.High: radioButtonPixelCountHigh.Checked = true; break;
            }

            if (Settings.PlayableScreensaver) checkBoxPlayableScreensaver.Checked = true;
            if (Settings.MuteScreenSaverVolume) checkBoxScreenSaverMute.Checked = true;
            if (Settings.Windowed) checkBoxWindowed.Checked = true;
            if (Settings.KidSafe) checkBoxKidSafe.Checked = true;
            if (Settings.ReportErrors != null) checkBoxReportErrors.Checked = (bool)Settings.ReportErrors;
            textBoxExitCodeHint.Text = Settings.ExitHint;
            textBoxExitCode.Text = Settings.ExitCode;
            checkBoxShowSettings.Checked = Settings.ShowSettings;
            trackBarVolume.Value = (int)(Settings.Volume * 100);

            soundPlayer = new SoundPlayer(this);
            soundPlayer.MasterVolume = Settings.Volume;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (textBoxExitCode.Text == "")
            {
                MessageBox.Show("You must enter an exit code.");
                return;
            }

            Close();
            Settings.PixelCount = this.PixelCount;
            Settings.PlayableScreensaver = this.checkBoxPlayableScreensaver.Checked;
            Settings.MuteScreenSaverVolume = checkBoxScreenSaverMute.Checked;
            Settings.Windowed = checkBoxWindowed.Checked;
            Settings.Volume = trackBarVolume.Value / 100.0;
            Settings.ReportErrors = checkBoxReportErrors.Checked;
            Settings.KidSafe = checkBoxKidSafe.Checked;
            Settings.ExitCode = textBoxExitCode.Text;
            Settings.ExitHint = textBoxExitCodeHint.Text;
            Settings.ShowSettings = checkBoxShowSettings.Checked;

            Settings.SaveSettings();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void trackBarVolume_Scroll(object sender, EventArgs e)
        {
        }

        private void trackBarVolume_ValueChanged(object sender, EventArgs e)
        {
            if (trackBarVolume.Value == 0) labelVolume.Text = "Silent";
            else if (trackBarVolume.Value < 80) labelVolume.Text = "Quiet";
            else if (trackBarVolume.Value < 120) labelVolume.Text = "Normal";
            else labelVolume.Text = "Loud";
        }

        private void trackBarVolume_MouseUp(object sender, MouseEventArgs e)
        {
            //if (soundPlayer != null)
            //{
            //    soundPlayer.MasterVolume = trackBarVolume.Value / 100.0;
            //    MediaBag.Play(SoundID.Dot18);
            //}
        }

        private void SettingsForm_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void checkBoxKidSafe_CheckedChanged(object sender, EventArgs e)
        {
        }
    }
}