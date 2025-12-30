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
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkForUpdatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.pleaseDonateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.RoundedPanel1 = new Bahtinov_Collimator.Custom_Components.RoundedPanel();
            this.RoundedStartButton = new Bahtinov_Collimator.Custom_Components.RoundedButton();
            this.slideSwitch2 = new Bahtinov_Collimator.Custom_Components.SlideSwitch();
            this.label1 = new System.Windows.Forms.Label();
            this.Label2 = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            this.RoundedPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.settingsToolStripMenuItem1,
            this.aboutToolStripMenuItem,
            this.pleaseDonateToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(920, 28);
            this.menuStrip1.TabIndex = 19;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.MouseEnter += new System.EventHandler(this.MenuStrip1_MouseEnter);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.quitToolStripMenuItem2});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
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
            this.settingsToolStripMenuItem1.Name = "settingsToolStripMenuItem1";
            this.settingsToolStripMenuItem1.Size = new System.Drawing.Size(76, 24);
            this.settingsToolStripMenuItem1.Text = "Settings";
            this.settingsToolStripMenuItem1.Click += new System.EventHandler(this.SettingsToolStripMenuItem1_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helpToolStripMenuItem,
            this.checkForUpdatesToolStripMenuItem,
            this.aboutToolStripMenuItem2});
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(55, 24);
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
            this.pleaseDonateToolStripMenuItem.Size = new System.Drawing.Size(123, 24);
            this.pleaseDonateToolStripMenuItem.Text = "Support SkyCal";
            this.pleaseDonateToolStripMenuItem.Click += new System.EventHandler(this.PleaseDonateToolStripMenuItem_Click);
            // 
            // RoundedPanel1
            // 
            this.RoundedPanel1.BackColor = System.Drawing.Color.Transparent;
            this.RoundedPanel1.BorderColor = System.Drawing.Color.Gray;
            this.RoundedPanel1.BorderThickness = 4;
            this.RoundedPanel1.Controls.Add(this.RoundedStartButton);
            this.RoundedPanel1.Controls.Add(this.slideSwitch2);
            this.RoundedPanel1.Controls.Add(this.label1);
            this.RoundedPanel1.Controls.Add(this.Label2);
            this.RoundedPanel1.CornerRadius = 12;
            this.RoundedPanel1.FillColor = System.Drawing.Color.DimGray;
            this.RoundedPanel1.ForeColor = System.Drawing.Color.White;
            this.RoundedPanel1.Location = new System.Drawing.Point(11, 512);
            this.RoundedPanel1.Name = "RoundedPanel1";
            this.RoundedPanel1.Size = new System.Drawing.Size(254, 134);
            this.RoundedPanel1.TabIndex = 27;
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
            this.RoundedStartButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.RoundedStartButton.ForeColor = System.Drawing.Color.White;
            this.RoundedStartButton.HoverOverlay = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.RoundedStartButton.Image = global::Bahtinov_Collimator.Properties.Resources.SelectionCircle;
            this.RoundedStartButton.ImageHeight = 32;
            this.RoundedStartButton.ImageOffsetX = 50;
            this.RoundedStartButton.ImageOffsetY = 0;
            this.RoundedStartButton.ImageWidth = 32;
            this.RoundedStartButton.Location = new System.Drawing.Point(46, 67);
            this.RoundedStartButton.Name = "RoundedStartButton";
            this.RoundedStartButton.PressedOverlay = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.RoundedStartButton.Size = new System.Drawing.Size(160, 44);
            this.RoundedStartButton.TabIndex = 25;
            this.RoundedStartButton.Text = "    Select Star";
            this.RoundedStartButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.RoundedStartButton.TextOffsetX = 10;
            this.RoundedStartButton.UseVisualStyleBackColor = false;
            this.RoundedStartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // slideSwitch2
            // 
            this.slideSwitch2.IsOn = false;
            this.slideSwitch2.Location = new System.Drawing.Point(104, 22);
            this.slideSwitch2.Margin = new System.Windows.Forms.Padding(2);
            this.slideSwitch2.Name = "slideSwitch2";
            this.slideSwitch2.OffColor = System.Drawing.Color.Gray;
            this.slideSwitch2.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(69)))), ((int)(((byte)(96)))));
            this.slideSwitch2.Size = new System.Drawing.Size(50, 25);
            this.slideSwitch2.TabIndex = 24;
            this.slideSwitch2.ToggleColor = System.Drawing.Color.White;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 25);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 16);
            this.label1.TabIndex = 22;
            this.label1.Text = "Bahtinov";
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Location = new System.Drawing.Point(158, 26);
            this.Label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(57, 16);
            this.Label2.TabIndex = 23;
            this.Label2.Text = "Defocus";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(930, 652);
            this.Controls.Add(this.RoundedPanel1);
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
            this.RoundedPanel1.ResumeLayout(false);
            this.RoundedPanel1.PerformLayout();
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
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label Label2;
        private Custom_Components.SlideSwitch slideSwitch2;
        private System.Windows.Forms.ToolStripMenuItem pleaseDonateToolStripMenuItem;
        private Custom_Components.RoundedButton RoundedStartButton;
        private Custom_Components.RoundedPanel RoundedPanel1;
    }
}

