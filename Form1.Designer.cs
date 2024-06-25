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
            this.StartButton = new System.Windows.Forms.Button();
            this.FocusErrorLabel1 = new System.Windows.Forms.Label();
            this.WithinCriticalFocusLabel1 = new System.Windows.Forms.Label();
            this.UpdateTimer = new System.Windows.Forms.Timer(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.capturedImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.annotatedImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.cheatSheetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkForUpdatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.donateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.AbsoluteFocusErrorLabel1 = new System.Windows.Forms.Label();
            this.RedGroupBox = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.GreenGroupBox = new System.Windows.Forms.GroupBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.WithinCriticalFocusLabel2 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.AbsoluteFocusErrorLabel2 = new System.Windows.Forms.Label();
            this.FocusErrorLabel2 = new System.Windows.Forms.Label();
            this.BlueGroupBox = new System.Windows.Forms.GroupBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.WithinCriticalFocusLabel3 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.AbsoluteFocusErrorLabel3 = new System.Windows.Forms.Label();
            this.FocusErrorLabel3 = new System.Windows.Forms.Label();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.menuStrip1.SuspendLayout();
            this.RedGroupBox.SuspendLayout();
            this.GreenGroupBox.SuspendLayout();
            this.BlueGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // StartButton
            // 
            this.StartButton.Location = new System.Drawing.Point(55, 446);
            this.StartButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(133, 48);
            this.StartButton.TabIndex = 0;
            this.StartButton.Text = "Start";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // FocusErrorLabel1
            // 
            this.FocusErrorLabel1.AutoSize = true;
            this.FocusErrorLabel1.Location = new System.Drawing.Point(116, 36);
            this.FocusErrorLabel1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FocusErrorLabel1.MinimumSize = new System.Drawing.Size(53, 0);
            this.FocusErrorLabel1.Name = "FocusErrorLabel1";
            this.FocusErrorLabel1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.FocusErrorLabel1.Size = new System.Drawing.Size(53, 16);
            this.FocusErrorLabel1.TabIndex = 9;
            this.FocusErrorLabel1.Text = "0.0";
            this.FocusErrorLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // WithinCriticalFocusLabel1
            // 
            this.WithinCriticalFocusLabel1.AutoSize = true;
            this.WithinCriticalFocusLabel1.ForeColor = System.Drawing.Color.Black;
            this.WithinCriticalFocusLabel1.Location = new System.Drawing.Point(159, 80);
            this.WithinCriticalFocusLabel1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.WithinCriticalFocusLabel1.Name = "WithinCriticalFocusLabel1";
            this.WithinCriticalFocusLabel1.Size = new System.Drawing.Size(19, 16);
            this.WithinCriticalFocusLabel1.TabIndex = 16;
            this.WithinCriticalFocusLabel1.Text = "---";
            // 
            // UpdateTimer
            // 
            this.UpdateTimer.Tick += new System.EventHandler(this.UpdateTimer_Tick);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.cheatSheetToolStripMenuItem,
            this.settingsToolStripMenuItem1,
            this.aboutToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(785, 28);
            this.menuStrip1.TabIndex = 19;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.MenuActivate += new System.EventHandler(this.MenuStrip1_MenuActivate);
            this.menuStrip1.MenuDeactivate += new System.EventHandler(this.MenuStrip1_MenuDeactivate);
            this.menuStrip1.MouseEnter += new System.EventHandler(this.MenuStrip1_MouseEnter);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.quitToolStripMenuItem,
            this.quitToolStripMenuItem2});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // quitToolStripMenuItem
            // 
            this.quitToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.capturedImageToolStripMenuItem,
            this.annotatedImageToolStripMenuItem});
            this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.Size = new System.Drawing.Size(169, 26);
            this.quitToolStripMenuItem.Text = "Save Image";
            // 
            // capturedImageToolStripMenuItem
            // 
            this.capturedImageToolStripMenuItem.Name = "capturedImageToolStripMenuItem";
            this.capturedImageToolStripMenuItem.Size = new System.Drawing.Size(208, 26);
            this.capturedImageToolStripMenuItem.Text = "Captured Image";
            this.capturedImageToolStripMenuItem.Click += new System.EventHandler(this.CapturedImageToolStripMenuItem_Click);
            // 
            // annotatedImageToolStripMenuItem
            // 
            this.annotatedImageToolStripMenuItem.Name = "annotatedImageToolStripMenuItem";
            this.annotatedImageToolStripMenuItem.Size = new System.Drawing.Size(208, 26);
            this.annotatedImageToolStripMenuItem.Text = "Annotated Image";
            this.annotatedImageToolStripMenuItem.Click += new System.EventHandler(this.AnnotatedImageToolStripMenuItem_Click);
            // 
            // quitToolStripMenuItem2
            // 
            this.quitToolStripMenuItem2.Name = "quitToolStripMenuItem2";
            this.quitToolStripMenuItem2.Size = new System.Drawing.Size(169, 26);
            this.quitToolStripMenuItem2.Text = "Quit";
            this.quitToolStripMenuItem2.Click += new System.EventHandler(this.QuitToolStripMenuItem2_Click);
            // 
            // cheatSheetToolStripMenuItem
            // 
            this.cheatSheetToolStripMenuItem.Name = "cheatSheetToolStripMenuItem";
            this.cheatSheetToolStripMenuItem.Size = new System.Drawing.Size(106, 24);
            this.cheatSheetToolStripMenuItem.Text = "Adjust Assist";
            this.cheatSheetToolStripMenuItem.Click += new System.EventHandler(this.cheatSheetToolStripMenuItem_Click);
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
            this.donateToolStripMenuItem,
            this.aboutToolStripMenuItem2});
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(55, 24);
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
            // AbsoluteFocusErrorLabel1
            // 
            this.AbsoluteFocusErrorLabel1.AutoSize = true;
            this.AbsoluteFocusErrorLabel1.Location = new System.Drawing.Point(116, 58);
            this.AbsoluteFocusErrorLabel1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.AbsoluteFocusErrorLabel1.MaximumSize = new System.Drawing.Size(53, 0);
            this.AbsoluteFocusErrorLabel1.MinimumSize = new System.Drawing.Size(53, 0);
            this.AbsoluteFocusErrorLabel1.Name = "AbsoluteFocusErrorLabel1";
            this.AbsoluteFocusErrorLabel1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.AbsoluteFocusErrorLabel1.Size = new System.Drawing.Size(53, 16);
            this.AbsoluteFocusErrorLabel1.TabIndex = 20;
            this.AbsoluteFocusErrorLabel1.Text = "0.0";
            this.AbsoluteFocusErrorLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // RedGroupBox
            // 
            this.RedGroupBox.Controls.Add(this.label7);
            this.RedGroupBox.Controls.Add(this.label4);
            this.RedGroupBox.Controls.Add(this.label3);
            this.RedGroupBox.Controls.Add(this.WithinCriticalFocusLabel1);
            this.RedGroupBox.Controls.Add(this.label2);
            this.RedGroupBox.Controls.Add(this.label1);
            this.RedGroupBox.Controls.Add(this.AbsoluteFocusErrorLabel1);
            this.RedGroupBox.Controls.Add(this.FocusErrorLabel1);
            this.RedGroupBox.ForeColor = System.Drawing.Color.Brown;
            this.RedGroupBox.Location = new System.Drawing.Point(16, 33);
            this.RedGroupBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.RedGroupBox.Name = "RedGroupBox";
            this.RedGroupBox.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.RedGroupBox.Size = new System.Drawing.Size(221, 114);
            this.RedGroupBox.TabIndex = 21;
            this.RedGroupBox.TabStop = false;
            this.RedGroupBox.Text = "Focus Channel 1";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 58);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(103, 16);
            this.label7.TabIndex = 26;
            this.label7.Text = "Defocused Star:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 80);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(129, 16);
            this.label4.TabIndex = 25;
            this.label4.Text = "Inside Critical Focus:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(164, 58);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(25, 16);
            this.label3.TabIndex = 24;
            this.label3.Text = "µm";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(164, 36);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 16);
            this.label2.TabIndex = 23;
            this.label2.Text = "pixels";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 36);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 16);
            this.label1.TabIndex = 22;
            this.label1.Text = "Bahtinov Error:";
            // 
            // GreenGroupBox
            // 
            this.GreenGroupBox.Controls.Add(this.label10);
            this.GreenGroupBox.Controls.Add(this.label5);
            this.GreenGroupBox.Controls.Add(this.label6);
            this.GreenGroupBox.Controls.Add(this.WithinCriticalFocusLabel2);
            this.GreenGroupBox.Controls.Add(this.label8);
            this.GreenGroupBox.Controls.Add(this.label9);
            this.GreenGroupBox.Controls.Add(this.AbsoluteFocusErrorLabel2);
            this.GreenGroupBox.Controls.Add(this.FocusErrorLabel2);
            this.GreenGroupBox.ForeColor = System.Drawing.Color.DarkGreen;
            this.GreenGroupBox.Location = new System.Drawing.Point(16, 160);
            this.GreenGroupBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.GreenGroupBox.Name = "GreenGroupBox";
            this.GreenGroupBox.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.GreenGroupBox.Size = new System.Drawing.Size(221, 114);
            this.GreenGroupBox.TabIndex = 26;
            this.GreenGroupBox.TabStop = false;
            this.GreenGroupBox.Text = "Focus Channel 2";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(12, 58);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(103, 16);
            this.label10.TabIndex = 26;
            this.label10.Text = "Defocused Star:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 80);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(129, 16);
            this.label5.TabIndex = 25;
            this.label5.Text = "Inside Critical Focus:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(164, 58);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(25, 16);
            this.label6.TabIndex = 24;
            this.label6.Text = "µm";
            // 
            // WithinCriticalFocusLabel2
            // 
            this.WithinCriticalFocusLabel2.AutoSize = true;
            this.WithinCriticalFocusLabel2.ForeColor = System.Drawing.Color.Black;
            this.WithinCriticalFocusLabel2.Location = new System.Drawing.Point(159, 80);
            this.WithinCriticalFocusLabel2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.WithinCriticalFocusLabel2.Name = "WithinCriticalFocusLabel2";
            this.WithinCriticalFocusLabel2.Size = new System.Drawing.Size(19, 16);
            this.WithinCriticalFocusLabel2.TabIndex = 16;
            this.WithinCriticalFocusLabel2.Text = "---";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(164, 36);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(42, 16);
            this.label8.TabIndex = 23;
            this.label8.Text = "pixels";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(12, 36);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(94, 16);
            this.label9.TabIndex = 22;
            this.label9.Text = "Bahtinov Error:";
            // 
            // AbsoluteFocusErrorLabel2
            // 
            this.AbsoluteFocusErrorLabel2.AutoSize = true;
            this.AbsoluteFocusErrorLabel2.Location = new System.Drawing.Point(116, 58);
            this.AbsoluteFocusErrorLabel2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.AbsoluteFocusErrorLabel2.MaximumSize = new System.Drawing.Size(53, 0);
            this.AbsoluteFocusErrorLabel2.MinimumSize = new System.Drawing.Size(53, 0);
            this.AbsoluteFocusErrorLabel2.Name = "AbsoluteFocusErrorLabel2";
            this.AbsoluteFocusErrorLabel2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.AbsoluteFocusErrorLabel2.Size = new System.Drawing.Size(53, 16);
            this.AbsoluteFocusErrorLabel2.TabIndex = 20;
            this.AbsoluteFocusErrorLabel2.Text = "0.0";
            this.AbsoluteFocusErrorLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // FocusErrorLabel2
            // 
            this.FocusErrorLabel2.AutoSize = true;
            this.FocusErrorLabel2.Location = new System.Drawing.Point(116, 36);
            this.FocusErrorLabel2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FocusErrorLabel2.MinimumSize = new System.Drawing.Size(53, 0);
            this.FocusErrorLabel2.Name = "FocusErrorLabel2";
            this.FocusErrorLabel2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.FocusErrorLabel2.Size = new System.Drawing.Size(53, 16);
            this.FocusErrorLabel2.TabIndex = 9;
            this.FocusErrorLabel2.Text = "0.0";
            this.FocusErrorLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // BlueGroupBox
            // 
            this.BlueGroupBox.Controls.Add(this.label11);
            this.BlueGroupBox.Controls.Add(this.label12);
            this.BlueGroupBox.Controls.Add(this.label13);
            this.BlueGroupBox.Controls.Add(this.WithinCriticalFocusLabel3);
            this.BlueGroupBox.Controls.Add(this.label15);
            this.BlueGroupBox.Controls.Add(this.label16);
            this.BlueGroupBox.Controls.Add(this.AbsoluteFocusErrorLabel3);
            this.BlueGroupBox.Controls.Add(this.FocusErrorLabel3);
            this.BlueGroupBox.ForeColor = System.Drawing.Color.DarkBlue;
            this.BlueGroupBox.Location = new System.Drawing.Point(16, 287);
            this.BlueGroupBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.BlueGroupBox.Name = "BlueGroupBox";
            this.BlueGroupBox.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.BlueGroupBox.Size = new System.Drawing.Size(221, 114);
            this.BlueGroupBox.TabIndex = 26;
            this.BlueGroupBox.TabStop = false;
            this.BlueGroupBox.Text = "Focus Channel 3";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(12, 58);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(103, 16);
            this.label11.TabIndex = 26;
            this.label11.Text = "Defocused Star:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(12, 80);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(129, 16);
            this.label12.TabIndex = 25;
            this.label12.Text = "Inside Critical Focus:";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(164, 58);
            this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(25, 16);
            this.label13.TabIndex = 24;
            this.label13.Text = "µm";
            // 
            // WithinCriticalFocusLabel3
            // 
            this.WithinCriticalFocusLabel3.AutoSize = true;
            this.WithinCriticalFocusLabel3.ForeColor = System.Drawing.Color.Black;
            this.WithinCriticalFocusLabel3.Location = new System.Drawing.Point(159, 80);
            this.WithinCriticalFocusLabel3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.WithinCriticalFocusLabel3.Name = "WithinCriticalFocusLabel3";
            this.WithinCriticalFocusLabel3.Size = new System.Drawing.Size(19, 16);
            this.WithinCriticalFocusLabel3.TabIndex = 16;
            this.WithinCriticalFocusLabel3.Text = "---";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(164, 36);
            this.label15.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(42, 16);
            this.label15.TabIndex = 23;
            this.label15.Text = "pixels";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(12, 36);
            this.label16.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(94, 16);
            this.label16.TabIndex = 22;
            this.label16.Text = "Bahtinov Error:";
            // 
            // AbsoluteFocusErrorLabel3
            // 
            this.AbsoluteFocusErrorLabel3.AutoSize = true;
            this.AbsoluteFocusErrorLabel3.Location = new System.Drawing.Point(116, 58);
            this.AbsoluteFocusErrorLabel3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.AbsoluteFocusErrorLabel3.MaximumSize = new System.Drawing.Size(53, 0);
            this.AbsoluteFocusErrorLabel3.MinimumSize = new System.Drawing.Size(53, 0);
            this.AbsoluteFocusErrorLabel3.Name = "AbsoluteFocusErrorLabel3";
            this.AbsoluteFocusErrorLabel3.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.AbsoluteFocusErrorLabel3.Size = new System.Drawing.Size(53, 16);
            this.AbsoluteFocusErrorLabel3.TabIndex = 20;
            this.AbsoluteFocusErrorLabel3.Text = "0.0";
            this.AbsoluteFocusErrorLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // FocusErrorLabel3
            // 
            this.FocusErrorLabel3.AutoSize = true;
            this.FocusErrorLabel3.Location = new System.Drawing.Point(116, 36);
            this.FocusErrorLabel3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FocusErrorLabel3.MinimumSize = new System.Drawing.Size(53, 0);
            this.FocusErrorLabel3.Name = "FocusErrorLabel3";
            this.FocusErrorLabel3.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.FocusErrorLabel3.Size = new System.Drawing.Size(53, 16);
            this.FocusErrorLabel3.TabIndex = 9;
            this.FocusErrorLabel3.Text = "0.0";
            this.FocusErrorLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pictureBox
            // 
            this.pictureBox.BackColor = System.Drawing.Color.Black;
            this.pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox.Location = new System.Drawing.Point(245, 38);
            this.pictureBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(533, 492);
            this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox.TabIndex = 6;
            this.pictureBox.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(785, 537);
            this.Controls.Add(this.BlueGroupBox);
            this.Controls.Add(this.GreenGroupBox);
            this.Controls.Add(this.RedGroupBox);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.StartButton);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Bahtinov Collimator";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.RedGroupBox.ResumeLayout(false);
            this.RedGroupBox.PerformLayout();
            this.GreenGroupBox.ResumeLayout(false);
            this.GreenGroupBox.PerformLayout();
            this.BlueGroupBox.ResumeLayout(false);
            this.BlueGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.Label FocusErrorLabel1;
        private System.Windows.Forms.Label WithinCriticalFocusLabel1;
        private System.Windows.Forms.Timer UpdateTimer;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem capturedImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem annotatedImageToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.Label AbsoluteFocusErrorLabel1;
        private System.Windows.Forms.GroupBox RedGroupBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox GreenGroupBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label WithinCriticalFocusLabel2;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label AbsoluteFocusErrorLabel2;
        private System.Windows.Forms.Label FocusErrorLabel2;
        private System.Windows.Forms.GroupBox BlueGroupBox;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label WithinCriticalFocusLabel3;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label AbsoluteFocusErrorLabel3;
        private System.Windows.Forms.Label FocusErrorLabel3;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ToolStripMenuItem donateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkForUpdatesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cheatSheetToolStripMenuItem;
    }
}

