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
            this.groupBoxBlue = new Bahtinov_Collimator.FocusChannelComponent();
            this.groupBoxGreen = new Bahtinov_Collimator.FocusChannelComponent();
            this.groupBoxRed = new Bahtinov_Collimator.FocusChannelComponent();
            this.imageDisplayComponent1 = new Bahtinov_Collimator.ImageDisplayComponent();
            this.analysisGroupBox = new Bahtinov_Collimator.Custom_Components.RoundedGroupBox();
            this.RoundedStartButton = new Bahtinov_Collimator.Custom_Components.RoundedButton();
            this.toggleSwitch1 = new ToggleSwitch();
            this.bahtinovLabel = new System.Windows.Forms.Label();
            this.defocusLabel = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            this.analysisGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.settingsToolStripMenuItem1,
            this.aboutToolStripMenuItem,
            this.pleaseDonateToolStripMenuItem,
            this.whatDoIDoNextToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 0, 14);
            this.menuStrip1.Size = new System.Drawing.Size(584, 37);
            this.menuStrip1.TabIndex = 19;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.MouseEnter += new System.EventHandler(this.MenuStrip1_MouseEnter);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.quitToolStripMenuItem2});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(41, 23);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // quitToolStripMenuItem2
            // 
            this.quitToolStripMenuItem2.Name = "quitToolStripMenuItem2";
            this.quitToolStripMenuItem2.Size = new System.Drawing.Size(105, 24);
            this.quitToolStripMenuItem2.Text = "Quit";
            this.quitToolStripMenuItem2.Click += new System.EventHandler(this.QuitToolStripMenuItem2_Click);
            // 
            // settingsToolStripMenuItem1
            // 
            this.settingsToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.generalSettingsToolStripMenuItem,
            this.focusCalibrationToolStripMenuItem});
            this.settingsToolStripMenuItem1.Name = "settingsToolStripMenuItem1";
            this.settingsToolStripMenuItem1.Size = new System.Drawing.Size(56, 23);
            this.settingsToolStripMenuItem1.Text = "Setup";
            // 
            // generalSettingsToolStripMenuItem
            // 
            this.generalSettingsToolStripMenuItem.Name = "generalSettingsToolStripMenuItem";
            this.generalSettingsToolStripMenuItem.Size = new System.Drawing.Size(144, 24);
            this.generalSettingsToolStripMenuItem.Text = "Settings";
            this.generalSettingsToolStripMenuItem.Click += new System.EventHandler(this.GeneralSettingsToolStripMenuItem_Click);
            // 
            // focusCalibrationToolStripMenuItem
            // 
            this.focusCalibrationToolStripMenuItem.Name = "focusCalibrationToolStripMenuItem";
            this.focusCalibrationToolStripMenuItem.Size = new System.Drawing.Size(144, 24);
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
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(49, 23);
            this.aboutToolStripMenuItem.Text = "Help";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(191, 24);
            this.helpToolStripMenuItem.Text = "User Manual";
            this.helpToolStripMenuItem.Click += new System.EventHandler(this.HelpToolStripMenuItem_Click);
            // 
            // checkForUpdatesToolStripMenuItem
            // 
            this.checkForUpdatesToolStripMenuItem.Name = "checkForUpdatesToolStripMenuItem";
            this.checkForUpdatesToolStripMenuItem.Size = new System.Drawing.Size(191, 24);
            this.checkForUpdatesToolStripMenuItem.Text = "Check for Updates";
            this.checkForUpdatesToolStripMenuItem.Click += new System.EventHandler(this.CheckForUpdatesToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem2
            // 
            this.aboutToolStripMenuItem2.Name = "aboutToolStripMenuItem2";
            this.aboutToolStripMenuItem2.Size = new System.Drawing.Size(191, 24);
            this.aboutToolStripMenuItem2.Text = "About";
            this.aboutToolStripMenuItem2.Click += new System.EventHandler(this.AboutToolStripMenuItem2_Click);
            // 
            // pleaseDonateToolStripMenuItem
            // 
            this.pleaseDonateToolStripMenuItem.Name = "pleaseDonateToolStripMenuItem";
            this.pleaseDonateToolStripMenuItem.Size = new System.Drawing.Size(114, 23);
            this.pleaseDonateToolStripMenuItem.Text = "Support SkyCal";
            this.pleaseDonateToolStripMenuItem.Click += new System.EventHandler(this.PleaseDonateToolStripMenuItem_Click);
            // 
            // whatDoIDoNextToolStripMenuItem
            // 
            this.whatDoIDoNextToolStripMenuItem.Name = "whatDoIDoNextToolStripMenuItem";
            this.whatDoIDoNextToolStripMenuItem.Size = new System.Drawing.Size(168, 23);
            this.whatDoIDoNextToolStripMenuItem.Text = "What Should I Do Next?";
            this.whatDoIDoNextToolStripMenuItem.Click += new System.EventHandler(this.WhatDoIDoNextToolStripMenuItem_Click);
            // 
            // groupBoxBlue
            // 
            this.groupBoxBlue.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.groupBoxBlue.Location = new System.Drawing.Point(9, 239);
            this.groupBoxBlue.Margin = new System.Windows.Forms.Padding(21, 14, 21, 14);
            this.groupBoxBlue.MirrorType = Bahtinov_Collimator.Custom_Components.MirrorType.SctPrimary;
            this.groupBoxBlue.Name = "groupBoxBlue";
            this.groupBoxBlue.Size = new System.Drawing.Size(185, 105);
            this.groupBoxBlue.TabIndex = 36;
            // 
            // groupBoxGreen
            // 
            this.groupBoxGreen.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.groupBoxGreen.Location = new System.Drawing.Point(9, 131);
            this.groupBoxGreen.Margin = new System.Windows.Forms.Padding(21, 14, 21, 14);
            this.groupBoxGreen.MirrorType = Bahtinov_Collimator.Custom_Components.MirrorType.SctPrimary;
            this.groupBoxGreen.Name = "groupBoxGreen";
            this.groupBoxGreen.Size = new System.Drawing.Size(185, 105);
            this.groupBoxGreen.TabIndex = 35;
            // 
            // groupBoxRed
            // 
            this.groupBoxRed.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.groupBoxRed.Location = new System.Drawing.Point(9, 22);
            this.groupBoxRed.Margin = new System.Windows.Forms.Padding(21, 14, 21, 14);
            this.groupBoxRed.MirrorType = Bahtinov_Collimator.Custom_Components.MirrorType.SctPrimary;
            this.groupBoxRed.Name = "groupBoxRed";
            this.groupBoxRed.Size = new System.Drawing.Size(185, 105);
            this.groupBoxRed.TabIndex = 34;
            // 
            // imageDisplayComponent1
            // 
            this.imageDisplayComponent1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.imageDisplayComponent1.Location = new System.Drawing.Point(202, 30);
            this.imageDisplayComponent1.Margin = new System.Windows.Forms.Padding(21, 14, 21, 14);
            this.imageDisplayComponent1.Name = "imageDisplayComponent1";
            this.imageDisplayComponent1.Size = new System.Drawing.Size(450, 450);
            this.imageDisplayComponent1.TabIndex = 29;
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
            this.analysisGroupBox.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.analysisGroupBox.Location = new System.Drawing.Point(9, 348);
            this.analysisGroupBox.Margin = new System.Windows.Forms.Padding(32);
            this.analysisGroupBox.Name = "analysisGroupBox";
            this.analysisGroupBox.Padding = new System.Windows.Forms.Padding(32);
            this.analysisGroupBox.Size = new System.Drawing.Size(185, 131);
            this.analysisGroupBox.TabIndex = 28;
            this.analysisGroupBox.TabStop = false;
            this.analysisGroupBox.Text = "roundedGroupBox1";
            // 
            // RoundedStartButton
            // 
            this.RoundedStartButton.BackColor = System.Drawing.Color.Gray;
            this.RoundedStartButton.BevelDark = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.RoundedStartButton.BevelLight = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
            this.RoundedStartButton.BevelThickness = 4;
            this.RoundedStartButton.CornerRadius = 8;
            this.RoundedStartButton.FlatAppearance.BorderSize = 0;
            this.RoundedStartButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RoundedStartButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.RoundedStartButton.ForeColor = System.Drawing.Color.White;
            this.RoundedStartButton.HoverOverlay = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.RoundedStartButton.Image = ((System.Drawing.Image)(resources.GetObject("RoundedStartButton.Image")));
            this.RoundedStartButton.ImageHeight = 32;
            this.RoundedStartButton.ImageOffsetX = -57;
            this.RoundedStartButton.ImageOffsetY = 0;
            this.RoundedStartButton.ImageWidth = 32;
            this.RoundedStartButton.Location = new System.Drawing.Point(12, 77);
            this.RoundedStartButton.Margin = new System.Windows.Forms.Padding(32);
            this.RoundedStartButton.Name = "RoundedStartButton";
            this.RoundedStartButton.PressedOverlay = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.RoundedStartButton.Size = new System.Drawing.Size(161, 42);
            this.RoundedStartButton.TabIndex = 25;
            this.RoundedStartButton.Text = "Select Star";
            this.RoundedStartButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.RoundedStartButton.TextOffsetX = 37;
            this.RoundedStartButton.UseVisualStyleBackColor = false;
            this.RoundedStartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // toggleSwitch1
            // 
            this.toggleSwitch1.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.toggleSwitch1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.toggleSwitch1.Location = new System.Drawing.Point(14, 25);
            this.toggleSwitch1.Margin = new System.Windows.Forms.Padding(32);
            this.toggleSwitch1.Name = "toggleSwitch1";
            this.toggleSwitch1.RotationDegrees = 90;
            this.toggleSwitch1.Size = new System.Drawing.Size(20, 40);
            this.toggleSwitch1.TabIndex = 26;
            this.toggleSwitch1.Text = "toggleSwitch1";
            // 
            // bahtinovLabel
            // 
            this.bahtinovLabel.AutoSize = true;
            this.bahtinovLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bahtinovLabel.Location = new System.Drawing.Point(36, 26);
            this.bahtinovLabel.Margin = new System.Windows.Forms.Padding(14, 0, 14, 0);
            this.bahtinovLabel.Name = "bahtinovLabel";
            this.bahtinovLabel.Size = new System.Drawing.Size(54, 15);
            this.bahtinovLabel.TabIndex = 22;
            this.bahtinovLabel.Text = "Bahtinov";
            this.bahtinovLabel.Click += new System.EventHandler(this.BahtinovLabel_Click);
            // 
            // defocusLabel
            // 
            this.defocusLabel.AutoSize = true;
            this.defocusLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.defocusLabel.Location = new System.Drawing.Point(36, 48);
            this.defocusLabel.Margin = new System.Windows.Forms.Padding(14, 0, 14, 0);
            this.defocusLabel.Name = "defocusLabel";
            this.defocusLabel.Size = new System.Drawing.Size(50, 15);
            this.defocusLabel.TabIndex = 23;
            this.defocusLabel.Text = "Defocus";
            this.defocusLabel.Click += new System.EventHandler(this.DefocusLabel_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(659, 487);
            this.Controls.Add(this.groupBoxBlue);
            this.Controls.Add(this.groupBoxGreen);
            this.Controls.Add(this.groupBoxRed);
            this.Controls.Add(this.imageDisplayComponent1);
            this.Controls.Add(this.analysisGroupBox);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(40, 40, 40, 40);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Padding = new System.Windows.Forms.Padding(0, 0, 75, 75);
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "SkyCal - Focus and Collimation Tool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
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
        private ImageDisplayComponent imageDisplayComponent1;
        private FocusChannelComponent groupBoxRed;
        private FocusChannelComponent groupBoxGreen;
        private FocusChannelComponent groupBoxBlue;
    }
}

