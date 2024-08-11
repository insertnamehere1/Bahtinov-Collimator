﻿namespace Bahtinov_Collimator
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
            this.StartButton = new System.Windows.Forms.Button();
            this.UpdateTimer = new System.Windows.Forms.Timer(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.AdjustAssistToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkForUpdatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.donateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.imageDisplayComponent1 = new Bahtinov_Collimator.ImageDisplayComponent();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // StartButton
            // 
            this.StartButton.Location = new System.Drawing.Point(61, 549);
            this.StartButton.Margin = new System.Windows.Forms.Padding(4);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(133, 48);
            this.StartButton.TabIndex = 0;
            this.StartButton.Text = "Select Star";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.AdjustAssistToolStripMenuItem,
            this.settingsToolStripMenuItem1,
            this.aboutToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(880, 30);
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
            // AdjustAssistToolStripMenuItem
            // 
            this.AdjustAssistToolStripMenuItem.Name = "AdjustAssistToolStripMenuItem";
            this.AdjustAssistToolStripMenuItem.Size = new System.Drawing.Size(106, 26);
            this.AdjustAssistToolStripMenuItem.Text = "Adjust Assist";
            this.AdjustAssistToolStripMenuItem.Click += new System.EventHandler(this.cheatSheetToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem1
            // 
            this.settingsToolStripMenuItem1.Name = "settingsToolStripMenuItem1";
            this.settingsToolStripMenuItem1.Size = new System.Drawing.Size(76, 26);
            this.settingsToolStripMenuItem1.Text = "Settings";
            this.settingsToolStripMenuItem1.Click += new System.EventHandler(this.SettingsToolStripMenuItem1_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helpToolStripMenuItem,
            this.checkForUpdatesToolStripMenuItem,
            this.donateToolStripMenuItem,
            this.aboutToolStripMenuItem2});
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(55, 26);
            this.aboutToolStripMenuItem.Text = "Help";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(213, 26);
            this.helpToolStripMenuItem.Text = "Help";
            this.helpToolStripMenuItem.Click += new System.EventHandler(this.HelpToolStripMenuItem_Click);
            // 
            // checkForUpdatesToolStripMenuItem
            // 
            this.checkForUpdatesToolStripMenuItem.Name = "checkForUpdatesToolStripMenuItem";
            this.checkForUpdatesToolStripMenuItem.Size = new System.Drawing.Size(213, 26);
            this.checkForUpdatesToolStripMenuItem.Text = "Check for Updates";
            this.checkForUpdatesToolStripMenuItem.Click += new System.EventHandler(this.checkForUpdatesToolStripMenuItem_Click);
            // 
            // donateToolStripMenuItem
            // 
            this.donateToolStripMenuItem.Name = "donateToolStripMenuItem";
            this.donateToolStripMenuItem.Size = new System.Drawing.Size(213, 26);
            this.donateToolStripMenuItem.Text = "Buy me a Coffee?";
            this.donateToolStripMenuItem.Click += new System.EventHandler(this.DonateToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem2
            // 
            this.aboutToolStripMenuItem2.Name = "aboutToolStripMenuItem2";
            this.aboutToolStripMenuItem2.Size = new System.Drawing.Size(213, 26);
            this.aboutToolStripMenuItem2.Text = "About";
            this.aboutToolStripMenuItem2.Click += new System.EventHandler(this.AboutToolStripMenuItem2_Click);
            // 
            // imageDisplayComponent1
            // 
            this.imageDisplayComponent1.Location = new System.Drawing.Point(269, 38);
            this.imageDisplayComponent1.Name = "imageDisplayComponent1";
            this.imageDisplayComponent1.Size = new System.Drawing.Size(600, 600);
            this.imageDisplayComponent1.TabIndex = 20;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(880, 651);
            this.Controls.Add(this.imageDisplayComponent1);
            this.Controls.Add(this.StartButton);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Bahtinov Collimator";
            this.MouseEnter += new System.EventHandler(this.Form1_MouseEnter);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.Timer UpdateTimer;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem2;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem donateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkForUpdatesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem AdjustAssistToolStripMenuItem;
        private ImageDisplayComponent imageDisplayComponent1;
    }
}

