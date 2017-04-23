namespace PixelWhimsy
{
    partial class Slate
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Slate));
            this.panelGame = new System.Windows.Forms.Panel();
            this.labelFps = new System.Windows.Forms.Label();
            this.panelBrushes = new System.Windows.Forms.Panel();
            this.panelAnimations = new System.Windows.Forms.Panel();
            this.labelPleaseWait = new System.Windows.Forms.Label();
            this.labelDebug = new System.Windows.Forms.Label();
            this.buttonExit = new System.Windows.Forms.Button();
            this.panelModulators = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // panelGame
            // 
            this.panelGame.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panelGame.Location = new System.Drawing.Point(65, 164);
            this.panelGame.Name = "panelGame";
            this.panelGame.Size = new System.Drawing.Size(416, 231);
            this.panelGame.TabIndex = 0;
            // 
            // labelFps
            // 
            this.labelFps.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelFps.AutoSize = true;
            this.labelFps.ForeColor = System.Drawing.Color.Gray;
            this.labelFps.Location = new System.Drawing.Point(12, 500);
            this.labelFps.Name = "labelFps";
            this.labelFps.Size = new System.Drawing.Size(22, 13);
            this.labelFps.TabIndex = 1;
            this.labelFps.Text = "0.0";
            // 
            // panelBrushes
            // 
            this.panelBrushes.Location = new System.Drawing.Point(15, 164);
            this.panelBrushes.Name = "panelBrushes";
            this.panelBrushes.Size = new System.Drawing.Size(28, 199);
            this.panelBrushes.TabIndex = 2;
            this.panelBrushes.MouseLeave += new System.EventHandler(this.toolbar_MouseLeave);
            this.panelBrushes.Paint += new System.Windows.Forms.PaintEventHandler(this.panelBrushes_Paint);
            this.panelBrushes.MouseEnter += new System.EventHandler(this.toolbar_MouseEnter);
            // 
            // panelAnimations
            // 
            this.panelAnimations.Location = new System.Drawing.Point(12, 134);
            this.panelAnimations.Name = "panelAnimations";
            this.panelAnimations.Size = new System.Drawing.Size(168, 24);
            this.panelAnimations.TabIndex = 3;
            this.panelAnimations.MouseLeave += new System.EventHandler(this.toolbar_MouseLeave);
            this.panelAnimations.Paint += new System.Windows.Forms.PaintEventHandler(this.panelAnimations_Paint);
            this.panelAnimations.MouseEnter += new System.EventHandler(this.toolbar_MouseEnter);
            // 
            // labelPleaseWait
            // 
            this.labelPleaseWait.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelPleaseWait.AutoSize = true;
            this.labelPleaseWait.Location = new System.Drawing.Point(80, 181);
            this.labelPleaseWait.Name = "labelPleaseWait";
            this.labelPleaseWait.Size = new System.Drawing.Size(254, 13);
            this.labelPleaseWait.TabIndex = 4;
            this.labelPleaseWait.Text = "Please wait while I start up your graphics hardware...";
            this.labelPleaseWait.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelDebug
            // 
            this.labelDebug.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelDebug.AutoSize = true;
            this.labelDebug.ForeColor = System.Drawing.Color.Gray;
            this.labelDebug.Location = new System.Drawing.Point(12, 487);
            this.labelDebug.Name = "labelDebug";
            this.labelDebug.Size = new System.Drawing.Size(0, 13);
            this.labelDebug.TabIndex = 1;
            // 
            // buttonExit
            // 
            this.buttonExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExit.Font = new System.Drawing.Font("Wingdings 2", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.buttonExit.ForeColor = System.Drawing.Color.Red;
            this.buttonExit.Location = new System.Drawing.Point(583, 4);
            this.buttonExit.Name = "buttonExit";
            this.buttonExit.Size = new System.Drawing.Size(25, 23);
            this.buttonExit.TabIndex = 5;
            this.buttonExit.Text = "Ó";
            this.buttonExit.UseVisualStyleBackColor = true;
            this.buttonExit.Click += new System.EventHandler(this.buttonExit_Click);
            this.buttonExit.MouseEnter += new System.EventHandler(this.buttonExit_MouseEnter);
            // 
            // panelModulators
            // 
            this.panelModulators.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panelModulators.Location = new System.Drawing.Point(512, 181);
            this.panelModulators.Name = "panelModulators";
            this.panelModulators.Size = new System.Drawing.Size(28, 199);
            this.panelModulators.TabIndex = 3;
            this.panelModulators.MouseLeave += new System.EventHandler(this.toolbar_MouseLeave);
            this.panelModulators.Paint += new System.Windows.Forms.PaintEventHandler(this.panel_Modulators_Paint);
            this.panelModulators.Click += new System.EventHandler(this.panelModulators_Click);
            this.panelModulators.MouseEnter += new System.EventHandler(this.toolbar_MouseEnter);
            // 
            // Slate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(613, 522);
            this.ControlBox = false;
            this.Controls.Add(this.panelModulators);
            this.Controls.Add(this.buttonExit);
            this.Controls.Add(this.labelPleaseWait);
            this.Controls.Add(this.panelAnimations);
            this.Controls.Add(this.panelBrushes);
            this.Controls.Add(this.labelDebug);
            this.Controls.Add(this.labelFps);
            this.Controls.Add(this.panelGame);
            this.ForeColor = System.Drawing.Color.Coral;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Slate";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "PixelWhimsy";
            this.TopMost = true;
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Slate_Paint);
            this.SizeChanged += new System.EventHandler(this.Slate_SizeChanged);
            this.VisibleChanged += new System.EventHandler(this.Slate_VisibleChanged);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Slate_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelGame;
        private System.Windows.Forms.Label labelFps;
        private System.Windows.Forms.Panel panelBrushes;
        private System.Windows.Forms.Panel panelAnimations;
        private System.Windows.Forms.Label labelPleaseWait;
        private System.Windows.Forms.Label labelDebug;
        private System.Windows.Forms.Button buttonExit;
        private System.Windows.Forms.Panel panelModulators;



    }
}