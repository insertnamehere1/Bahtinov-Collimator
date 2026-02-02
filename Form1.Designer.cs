namespace Bahtinov_Collimator
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.UpdateTimer = new System.Windows.Forms.Timer(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.generalSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.focusCalibrationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkForUpdatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.pleaseDonateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.whatDoIDoNextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.analysisGroupBox = new Bahtinov_Collimator.Custom_Components.RoundedGroupBox();
            this.toggleSwitch1 = new ToggleSwitch();
            this.bahtinovLabel = new System.Windows.Forms.Label();
            this.defocusLabel = new System.Windows.Forms.Label();
            this.RoundedStartButton = new Bahtinov_Collimator.Custom_Components.RoundedButton();
            this.menuStrip1.SuspendLayout();
            this.analysisGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.settingsToolStripMenuItem1,
            this.aboutToolStripMenuItem,
            this.pleaseDonateToolStripMenuItem,
            this.whatDoIDoNextToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(917, 30);
            this.menuStrip1.TabIndex = 19;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.MouseEnter += new System.EventHandler(this.MenuStrip1_MouseEnter);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.quitToolStripMenuItem2});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 26);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // quitToolStripMenuItem2
            // 
            this.quitToolStripMenuItem2.Name = "quitToolStripMenuItem2";
            this.quitToolStripMenuItem2.Size = new System.Drawing.Size(120, 26);
            this.quitToolStripMenuItem2.Text = "Quit";
            this.quitToolStripMenuItem2.Click += new System.EventHandler(this.QuitToolStripMenuItem2_Click);
            // 
            // settingsToolStripMenuItem1
            // 
            this.settingsToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.generalSettingsToolStripMenuItem,
            this.focusCalibrationToolStripMenuItem});
            this.settingsToolStripMenuItem1.Name = "settingsToolStripMenuItem1";
            this.settingsToolStripMenuItem1.Size = new System.Drawing.Size(61, 26);
            this.settingsToolStripMenuItem1.Text = "Setup";
            // 
            // generalSettingsToolStripMenuItem
            // 
            this.generalSettingsToolStripMenuItem.Name = "generalSettingsToolStripMenuItem";
            this.generalSettingsToolStripMenuItem.Size = new System.Drawing.Size(165, 26);
            this.generalSettingsToolStripMenuItem.Text = "Settings";
            this.generalSettingsToolStripMenuItem.Click += new System.EventHandler(this.GeneralSettingsToolStripMenuItem_Click);
            // 
            // focusCalibrationToolStripMenuItem
            // 
            this.focusCalibrationToolStripMenuItem.Name = "focusCalibrationToolStripMenuItem";
            this.focusCalibrationToolStripMenuItem.Size = new System.Drawing.Size(165, 26);
            this.focusCalibrationToolStripMenuItem.Text = "Calibration";
            this.focusCalibrationToolStripMenuItem.Click += new System.EventHandler(this.FocusCalibrationToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helpToolStripMenuItem,
            this.checkForUpdatesToolStripMenuItem,
            this.aboutToolStripMenuItem2});
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(55, 26);
            this.aboutToolStripMenuItem.Text = "Help";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(213, 26);
            this.helpToolStripMenuItem.Text = "User Manual";
            this.helpToolStripMenuItem.Click += new System.EventHandler(this.HelpToolStripMenuItem_Click);
            // 
            // checkForUpdatesToolStripMenuItem
            // 
            this.checkForUpdatesToolStripMenuItem.Name = "checkForUpdatesToolStripMenuItem";
            this.checkForUpdatesToolStripMenuItem.Size = new System.Drawing.Size(213, 26);
            this.checkForUpdatesToolStripMenuItem.Text = "Check for Updates";
            this.checkForUpdatesToolStripMenuItem.Click += new System.EventHandler(this.CheckForUpdatesToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem2
            // 
            this.aboutToolStripMenuItem2.Name = "aboutToolStripMenuItem2";
            this.aboutToolStripMenuItem2.Size = new System.Drawing.Size(213, 26);
            this.aboutToolStripMenuItem2.Text = "About";
            this.aboutToolStripMenuItem2.Click += new System.EventHandler(this.AboutToolStripMenuItem2_Click);
            // 
            // pleaseDonateToolStripMenuItem
            // 
            this.pleaseDonateToolStripMenuItem.Name = "pleaseDonateToolStripMenuItem";
            this.pleaseDonateToolStripMenuItem.Size = new System.Drawing.Size(123, 26);
            this.pleaseDonateToolStripMenuItem.Text = "Support SkyCal";
            this.pleaseDonateToolStripMenuItem.Click += new System.EventHandler(this.PleaseDonateToolStripMenuItem_Click);
            // 
            // whatDoIDoNextToolStripMenuItem
            // 
            this.whatDoIDoNextToolStripMenuItem.Name = "whatDoIDoNextToolStripMenuItem";
            this.whatDoIDoNextToolStripMenuItem.Size = new System.Drawing.Size(182, 26);
            this.whatDoIDoNextToolStripMenuItem.Text = "What Should I Do Next?";
            this.whatDoIDoNextToolStripMenuItem.Click += new System.EventHandler(this.WhatDoIDoNextToolStripMenuItem_Click);
            // 
            // analysisGroupBox
            // 
            this.analysisGroupBox.BorderColor = System.Drawing.Color.Gray;
            this.analysisGroupBox.BorderThickness = 2;
            this.analysisGroupBox.Controls.Add(this.RoundedStartButton);
            this.analysisGroupBox.Controls.Add(this.toggleSwitch1);
            this.analysisGroupBox.Controls.Add(this.bahtinovLabel);
            this.analysisGroupBox.Controls.Add(this.defocusLabel);
            this.analysisGroupBox.CornerRadius = 12;
            this.analysisGroupBox.Location = new System.Drawing.Point(12, 470);
            this.analysisGroupBox.Name = "analysisGroupBox";
            this.analysisGroupBox.Size = new System.Drawing.Size(251, 174);
            this.analysisGroupBox.TabIndex = 28;
            this.analysisGroupBox.TabStop = false;
            this.analysisGroupBox.Text = "roundedGroupBox1";
            // 
            // toggleSwitch1
            // 
            this.toggleSwitch1.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.toggleSwitch1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.toggleSwitch1.Location = new System.Drawing.Point(29, 34);
            this.toggleSwitch1.Name = "toggleSwitch1";
            this.toggleSwitch1.RotationDegrees = 90;
            this.toggleSwitch1.Size = new System.Drawing.Size(25, 55);
            this.toggleSwitch1.TabIndex = 26;
            this.toggleSwitch1.Text = "toggleSwitch1";
            // 
            // bahtinovLabel
            // 
            this.bahtinovLabel.AutoSize = true;
            this.bahtinovLabel.Location = new System.Drawing.Point(62, 36);
            this.bahtinovLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.bahtinovLabel.Name = "bahtinovLabel";
            this.bahtinovLabel.Size = new System.Drawing.Size(59, 16);
            this.bahtinovLabel.TabIndex = 22;
            this.bahtinovLabel.Text = "Bahtinov";
            this.bahtinovLabel.Click += new System.EventHandler(this.bahtinovLabel_Click);
            // 
            // defocusLabel
            // 
            this.defocusLabel.AutoSize = true;
            this.defocusLabel.Location = new System.Drawing.Point(62, 66);
            this.defocusLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.defocusLabel.Name = "defocusLabel";
            this.defocusLabel.Size = new System.Drawing.Size(57, 16);
            this.defocusLabel.TabIndex = 23;
            this.defocusLabel.Text = "Defocus";
            this.defocusLabel.Click += new System.EventHandler(this.defocusLabel_Click);
            // 
            // RoundedStartButton
            // 
            this.RoundedStartButton.BackColor = System.Drawing.Color.Gray;
            this.RoundedStartButton.BevelDark = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.RoundedStartButton.BevelLight = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
            this.RoundedStartButton.BevelThickness = 4;
            this.RoundedStartButton.CornerRadius = 12;
            this.RoundedStartButton.FlatAppearance.BorderSize = 0;
            this.RoundedStartButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RoundedStartButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.RoundedStartButton.ForeColor = System.Drawing.Color.White;
            this.RoundedStartButton.HoverOverlay = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.RoundedStartButton.Image = global::Bahtinov_Collimator.Properties.Resources.SelectionCircle;
            this.RoundedStartButton.ImageHeight = 32;
            this.RoundedStartButton.ImageOffsetX = -70;
            this.RoundedStartButton.ImageOffsetY = 0;
            this.RoundedStartButton.ImageWidth = 32;
            this.RoundedStartButton.Location = new System.Drawing.Point(29, 107);
            this.RoundedStartButton.Name = "RoundedStartButton";
            this.RoundedStartButton.PressedOverlay = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.RoundedStartButton.Size = new System.Drawing.Size(193, 50);
            this.RoundedStartButton.TabIndex = 25;
            this.RoundedStartButton.Text = "Select Star";
            this.RoundedStartButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.RoundedStartButton.TextOffsetX = 42;
            this.RoundedStartButton.UseVisualStyleBackColor = false;
            this.RoundedStartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(927, 652);
            this.Controls.Add(this.analysisGroupBox);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Padding = new System.Windows.Forms.Padding(0, 0, 10, 10);
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "SkyCal - Focus and Collimation Tool";
            this.MouseEnter += new System.EventHandler(this.Form1_MouseEnter);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.analysisGroupBox.ResumeLayout(false);
            this.analysisGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Timer UpdateTimer;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem2;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem checkForUpdatesToolStripMenuItem;
        private System.Windows.Forms.Label bahtinovLabel;
        private System.Windows.Forms.Label defocusLabel;
        private System.Windows.Forms.ToolStripMenuItem pleaseDonateToolStripMenuItem;
        private Custom_Components.RoundedButton RoundedStartButton;
        private System.Windows.Forms.ToolStripMenuItem whatDoIDoNextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem generalSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem focusCalibrationToolStripMenuItem;
        private ToggleSwitch toggleSwitch1;
        private Custom_Components.RoundedGroupBox analysisGroupBox;
    }
}

