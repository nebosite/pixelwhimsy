namespace PixelWhimsy
{
    partial class SettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.buttonOK = new System.Windows.Forms.Button();
            this.groupBoxPixelCount = new System.Windows.Forms.GroupBox();
            this.radioButtonPixelCountHigh = new System.Windows.Forms.RadioButton();
            this.radioButtonPixelCountMedium = new System.Windows.Forms.RadioButton();
            this.radioButtonPixelCountLow = new System.Windows.Forms.RadioButton();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxExitCodeHint = new System.Windows.Forms.TextBox();
            this.textBoxExitCode = new System.Windows.Forms.TextBox();
            this.checkBoxKidSafe = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBoxReportErrors = new System.Windows.Forms.CheckBox();
            this.checkBoxWindowed = new System.Windows.Forms.CheckBox();
            this.trackBarVolume = new System.Windows.Forms.TrackBar();
            this.labelVolume = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.checkBoxScreenSaverMute = new System.Windows.Forms.CheckBox();
            this.checkBoxPlayableScreensaver = new System.Windows.Forms.CheckBox();
            this.checkBoxShowSettings = new System.Windows.Forms.CheckBox();
            this.groupBoxPixelCount.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarVolume)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.BackColor = System.Drawing.Color.Lime;
            this.buttonOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonOK.Location = new System.Drawing.Point(438, 209);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(104, 39);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "Save Settings";
            this.buttonOK.UseVisualStyleBackColor = false;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // groupBoxPixelCount
            // 
            this.groupBoxPixelCount.Controls.Add(this.radioButtonPixelCountHigh);
            this.groupBoxPixelCount.Controls.Add(this.radioButtonPixelCountMedium);
            this.groupBoxPixelCount.Controls.Add(this.radioButtonPixelCountLow);
            this.groupBoxPixelCount.Location = new System.Drawing.Point(12, 12);
            this.groupBoxPixelCount.Name = "groupBoxPixelCount";
            this.groupBoxPixelCount.Size = new System.Drawing.Size(147, 116);
            this.groupBoxPixelCount.TabIndex = 2;
            this.groupBoxPixelCount.TabStop = false;
            this.groupBoxPixelCount.Text = "Pixel Count";
            // 
            // radioButtonPixelCountHigh
            // 
            this.radioButtonPixelCountHigh.AutoSize = true;
            this.radioButtonPixelCountHigh.Location = new System.Drawing.Point(18, 65);
            this.radioButtonPixelCountHigh.Name = "radioButtonPixelCountHigh";
            this.radioButtonPixelCountHigh.Size = new System.Drawing.Size(116, 17);
            this.radioButtonPixelCountHigh.TabIndex = 4;
            this.radioButtonPixelCountHigh.TabStop = true;
            this.radioButtonPixelCountHigh.Text = "High (better quality)";
            this.radioButtonPixelCountHigh.UseVisualStyleBackColor = true;
            // 
            // radioButtonPixelCountMedium
            // 
            this.radioButtonPixelCountMedium.AutoSize = true;
            this.radioButtonPixelCountMedium.Location = new System.Drawing.Point(18, 42);
            this.radioButtonPixelCountMedium.Name = "radioButtonPixelCountMedium";
            this.radioButtonPixelCountMedium.Size = new System.Drawing.Size(62, 17);
            this.radioButtonPixelCountMedium.TabIndex = 4;
            this.radioButtonPixelCountMedium.TabStop = true;
            this.radioButtonPixelCountMedium.Text = "Medium";
            this.radioButtonPixelCountMedium.UseVisualStyleBackColor = true;
            // 
            // radioButtonPixelCountLow
            // 
            this.radioButtonPixelCountLow.AutoSize = true;
            this.radioButtonPixelCountLow.Location = new System.Drawing.Point(18, 19);
            this.radioButtonPixelCountLow.Name = "radioButtonPixelCountLow";
            this.radioButtonPixelCountLow.Size = new System.Drawing.Size(80, 17);
            this.radioButtonPixelCountLow.TabIndex = 4;
            this.radioButtonPixelCountLow.TabStop = true;
            this.radioButtonPixelCountLow.Text = "Low (faster)";
            this.radioButtonPixelCountLow.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.BackColor = System.Drawing.Color.Red;
            this.buttonCancel.Location = new System.Drawing.Point(438, 261);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(104, 28);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Never Mind";
            this.buttonCancel.UseVisualStyleBackColor = false;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.textBoxExitCodeHint);
            this.groupBox1.Controls.Add(this.textBoxExitCode);
            this.groupBox1.Controls.Add(this.checkBoxKidSafe);
            this.groupBox1.Location = new System.Drawing.Point(12, 134);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(411, 155);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Kid Safety";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(40, 124);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Hint:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.SystemColors.ControlLight;
            this.label4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label4.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.label4.Location = new System.Drawing.Point(8, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(391, 41);
            this.label4.TabIndex = 2;
            this.label4.Text = resources.GetString("label4.Text");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 101);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Exit Code:";
            // 
            // textBoxExitCodeHint
            // 
            this.textBoxExitCodeHint.Location = new System.Drawing.Point(72, 121);
            this.textBoxExitCodeHint.Name = "textBoxExitCodeHint";
            this.textBoxExitCodeHint.Size = new System.Drawing.Size(324, 20);
            this.textBoxExitCodeHint.TabIndex = 1;
            // 
            // textBoxExitCode
            // 
            this.textBoxExitCode.Location = new System.Drawing.Point(72, 98);
            this.textBoxExitCode.Name = "textBoxExitCode";
            this.textBoxExitCode.Size = new System.Drawing.Size(100, 20);
            this.textBoxExitCode.TabIndex = 1;
            // 
            // checkBoxKidSafe
            // 
            this.checkBoxKidSafe.AutoSize = true;
            this.checkBoxKidSafe.Location = new System.Drawing.Point(18, 75);
            this.checkBoxKidSafe.Name = "checkBoxKidSafe";
            this.checkBoxKidSafe.Size = new System.Drawing.Size(142, 17);
            this.checkBoxKidSafe.TabIndex = 0;
            this.checkBoxKidSafe.Text = "Enable \"Kid Safe\" Mode";
            this.checkBoxKidSafe.UseVisualStyleBackColor = true;
            this.checkBoxKidSafe.CheckedChanged += new System.EventHandler(this.checkBoxKidSafe_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBoxReportErrors);
            this.groupBox2.Controls.Add(this.checkBoxWindowed);
            this.groupBox2.Controls.Add(this.trackBarVolume);
            this.groupBox2.Controls.Add(this.labelVolume);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Location = new System.Drawing.Point(301, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(244, 116);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "General Settings";
            // 
            // checkBoxReportErrors
            // 
            this.checkBoxReportErrors.AutoSize = true;
            this.checkBoxReportErrors.Location = new System.Drawing.Point(116, 50);
            this.checkBoxReportErrors.Name = "checkBoxReportErrors";
            this.checkBoxReportErrors.Size = new System.Drawing.Size(88, 17);
            this.checkBoxReportErrors.TabIndex = 4;
            this.checkBoxReportErrors.Text = "Report Errors";
            this.checkBoxReportErrors.UseVisualStyleBackColor = true;
            // 
            // checkBoxWindowed
            // 
            this.checkBoxWindowed.AutoSize = true;
            this.checkBoxWindowed.Location = new System.Drawing.Point(116, 27);
            this.checkBoxWindowed.Name = "checkBoxWindowed";
            this.checkBoxWindowed.Size = new System.Drawing.Size(77, 17);
            this.checkBoxWindowed.TabIndex = 4;
            this.checkBoxWindowed.Text = "Windowed";
            this.checkBoxWindowed.UseVisualStyleBackColor = true;
            // 
            // trackBarVolume
            // 
            this.trackBarVolume.LargeChange = 10;
            this.trackBarVolume.Location = new System.Drawing.Point(59, 21);
            this.trackBarVolume.Maximum = 200;
            this.trackBarVolume.Name = "trackBarVolume";
            this.trackBarVolume.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBarVolume.Size = new System.Drawing.Size(45, 89);
            this.trackBarVolume.TabIndex = 3;
            this.trackBarVolume.TickFrequency = 10;
            this.trackBarVolume.ValueChanged += new System.EventHandler(this.trackBarVolume_ValueChanged);
            this.trackBarVolume.Scroll += new System.EventHandler(this.trackBarVolume_Scroll);
            this.trackBarVolume.MouseUp += new System.Windows.Forms.MouseEventHandler(this.trackBarVolume_MouseUp);
            // 
            // labelVolume
            // 
            this.labelVolume.AutoSize = true;
            this.labelVolume.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelVolume.Location = new System.Drawing.Point(14, 49);
            this.labelVolume.Name = "labelVolume";
            this.labelVolume.Size = new System.Drawing.Size(33, 15);
            this.labelVolume.TabIndex = 2;
            this.labelVolume.Text = "Loud";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 27);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(45, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Volume:";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.checkBoxScreenSaverMute);
            this.groupBox3.Controls.Add(this.checkBoxPlayableScreensaver);
            this.groupBox3.Location = new System.Drawing.Point(165, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(130, 116);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Screen Saver Settings";
            // 
            // checkBoxScreenSaverMute
            // 
            this.checkBoxScreenSaverMute.AutoSize = true;
            this.checkBoxScreenSaverMute.Location = new System.Drawing.Point(7, 43);
            this.checkBoxScreenSaverMute.Name = "checkBoxScreenSaverMute";
            this.checkBoxScreenSaverMute.Size = new System.Drawing.Size(88, 17);
            this.checkBoxScreenSaverMute.TabIndex = 1;
            this.checkBoxScreenSaverMute.Text = "Mute Volume";
            this.checkBoxScreenSaverMute.UseVisualStyleBackColor = true;
            // 
            // checkBoxPlayableScreensaver
            // 
            this.checkBoxPlayableScreensaver.AutoSize = true;
            this.checkBoxPlayableScreensaver.Location = new System.Drawing.Point(7, 20);
            this.checkBoxPlayableScreensaver.Name = "checkBoxPlayableScreensaver";
            this.checkBoxPlayableScreensaver.Size = new System.Drawing.Size(66, 17);
            this.checkBoxPlayableScreensaver.TabIndex = 0;
            this.checkBoxPlayableScreensaver.Text = "Playable";
            this.checkBoxPlayableScreensaver.UseVisualStyleBackColor = true;
            // 
            // checkBoxShowSettings
            // 
            this.checkBoxShowSettings.AutoSize = true;
            this.checkBoxShowSettings.Location = new System.Drawing.Point(12, 295);
            this.checkBoxShowSettings.Name = "checkBoxShowSettings";
            this.checkBoxShowSettings.Size = new System.Drawing.Size(279, 17);
            this.checkBoxShowSettings.TabIndex = 0;
            this.checkBoxShowSettings.Text = "Show this settings dialog every time I run PixelWhimsy";
            this.checkBoxShowSettings.UseVisualStyleBackColor = true;
            this.checkBoxShowSettings.CheckedChanged += new System.EventHandler(this.checkBoxKidSafe_CheckedChanged);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(554, 322);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.groupBoxPixelCount);
            this.Controls.Add(this.checkBoxShowSettings);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.ShowIcon = false;
            this.Text = "PixelWhimsy Settings";
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SettingsForm_MouseDown);
            this.groupBoxPixelCount.ResumeLayout(false);
            this.groupBoxPixelCount.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarVolume)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.GroupBox groupBoxPixelCount;
        private System.Windows.Forms.RadioButton radioButtonPixelCountHigh;
        private System.Windows.Forms.RadioButton radioButtonPixelCountMedium;
        private System.Windows.Forms.RadioButton radioButtonPixelCountLow;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxExitCodeHint;
        private System.Windows.Forms.TextBox textBoxExitCode;
        private System.Windows.Forms.CheckBox checkBoxKidSafe;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TrackBar trackBarVolume;
        private System.Windows.Forms.CheckBox checkBoxWindowed;
        private System.Windows.Forms.Label labelVolume;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox checkBoxScreenSaverMute;
        private System.Windows.Forms.CheckBox checkBoxPlayableScreensaver;
        private System.Windows.Forms.CheckBox checkBoxReportErrors;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox checkBoxShowSettings;
    }
}