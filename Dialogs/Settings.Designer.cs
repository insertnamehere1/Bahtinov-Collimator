using Bahtinov_Collimator;
using Bahtinov_Collimator.Custom_Components;
using System.Drawing;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace Bahtinov_Collimator
{
    partial class Settings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Settings));
            this.groupBox3 = new Bahtinov_Collimator.Custom_Components.RoundedGroupBox();
            this.guidanceOffRadioButton = new System.Windows.Forms.RadioButton();
            this.newtonianRadioButton = new System.Windows.Forms.RadioButton();
            this.MakCassRadioButton = new System.Windows.Forms.RadioButton();
            this.SCTRadioButton = new System.Windows.Forms.RadioButton();
            this.minimizeGroupBox = new Bahtinov_Collimator.Custom_Components.RoundedGroupBox();
            this.minimizeLabel = new System.Windows.Forms.Label();
            this.minimizeCheckBox = new System.Windows.Forms.CheckBox();
            this.ErrorSignGroupBox = new Bahtinov_Collimator.Custom_Components.RoundedGroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.errorSignCheckBox = new System.Windows.Forms.CheckBox();
            this.keepOnTopGroupBox = new Bahtinov_Collimator.Custom_Components.RoundedGroupBox();
            this.onTopLabel = new System.Windows.Forms.Label();
            this.onTopCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new Bahtinov_Collimator.Custom_Components.RoundedGroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.historyMakersTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new Bahtinov_Collimator.Custom_Components.RoundedGroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.VoiceCheckBox = new System.Windows.Forms.CheckBox();
            this.CancelSettingsButton = new Bahtinov_Collimator.Custom_Components.RoundedButton();
            this.okButton = new Bahtinov_Collimator.Custom_Components.RoundedButton();
            this.groupBox3.SuspendLayout();
            this.minimizeGroupBox.SuspendLayout();
            this.ErrorSignGroupBox.SuspendLayout();
            this.keepOnTopGroupBox.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox3
            // 
            this.groupBox3.BorderColor = System.Drawing.Color.Gray;
            this.groupBox3.BorderThickness = 2;
            this.groupBox3.Controls.Add(this.guidanceOffRadioButton);
            this.groupBox3.Controls.Add(this.newtonianRadioButton);
            this.groupBox3.Controls.Add(this.MakCassRadioButton);
            this.groupBox3.Controls.Add(this.SCTRadioButton);
            this.groupBox3.CornerRadius = 12;
            this.groupBox3.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.groupBox3.ForeColor = System.Drawing.Color.LightGray;
            this.groupBox3.Location = new System.Drawing.Point(284, 14);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox3.Size = new System.Drawing.Size(267, 124);
            this.groupBox3.TabIndex = 36;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Guidance Configuration";
            // 
            // guidanceOffRadioButton
            // 
            this.guidanceOffRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.guidanceOffRadioButton.Location = new System.Drawing.Point(19, 91);
            this.guidanceOffRadioButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.guidanceOffRadioButton.Name = "guidanceOffRadioButton";
            this.guidanceOffRadioButton.Size = new System.Drawing.Size(110, 23);
            this.guidanceOffRadioButton.TabIndex = 3;
            this.guidanceOffRadioButton.TabStop = true;
            this.guidanceOffRadioButton.Text = "Guidance Off";
            this.guidanceOffRadioButton.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.guidanceOffRadioButton.UseVisualStyleBackColor = true;
            // 
            // newtonianRadioButton
            // 
            this.newtonianRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.newtonianRadioButton.Enabled = false;
            this.newtonianRadioButton.Location = new System.Drawing.Point(19, 68);
            this.newtonianRadioButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.newtonianRadioButton.Name = "newtonianRadioButton";
            this.newtonianRadioButton.Size = new System.Drawing.Size(93, 23);
            this.newtonianRadioButton.TabIndex = 2;
            this.newtonianRadioButton.TabStop = true;
            this.newtonianRadioButton.Text = "Newtonian";
            this.newtonianRadioButton.UseVisualStyleBackColor = true;
            // 
            // MakCassRadioButton
            // 
            this.MakCassRadioButton.Location = new System.Drawing.Point(19, 44);
            this.MakCassRadioButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MakCassRadioButton.Name = "MakCassRadioButton";
            this.MakCassRadioButton.Size = new System.Drawing.Size(201, 23);
            this.MakCassRadioButton.TabIndex = 1;
            this.MakCassRadioButton.TabStop = true;
            this.MakCassRadioButton.Text = "Maksutov-Cassegrain (MCT)";
            this.MakCassRadioButton.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.MakCassRadioButton.UseVisualStyleBackColor = true;
            // 
            // SCTRadioButton
            // 
            this.SCTRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SCTRadioButton.Location = new System.Drawing.Point(19, 23);
            this.SCTRadioButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.SCTRadioButton.Name = "SCTRadioButton";
            this.SCTRadioButton.Size = new System.Drawing.Size(184, 23);
            this.SCTRadioButton.TabIndex = 0;
            this.SCTRadioButton.TabStop = true;
            this.SCTRadioButton.Text = "Schmidt–Cassegrain (SCT)";
            this.SCTRadioButton.UseVisualStyleBackColor = true;
            // 
            // minimizeGroupBox
            // 
            this.minimizeGroupBox.BorderColor = System.Drawing.Color.Gray;
            this.minimizeGroupBox.BorderThickness = 2;
            this.minimizeGroupBox.Controls.Add(this.minimizeLabel);
            this.minimizeGroupBox.Controls.Add(this.minimizeCheckBox);
            this.minimizeGroupBox.CornerRadius = 12;
            this.minimizeGroupBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.minimizeGroupBox.ForeColor = System.Drawing.Color.LightGray;
            this.minimizeGroupBox.Location = new System.Drawing.Point(283, 278);
            this.minimizeGroupBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.minimizeGroupBox.Name = "minimizeGroupBox";
            this.minimizeGroupBox.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.minimizeGroupBox.Size = new System.Drawing.Size(268, 120);
            this.minimizeGroupBox.TabIndex = 40;
            this.minimizeGroupBox.TabStop = false;
            this.minimizeGroupBox.Text = "Minimize";
            // 
            // minimizeLabel
            // 
            this.minimizeLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.minimizeLabel.Location = new System.Drawing.Point(12, 51);
            this.minimizeLabel.Name = "minimizeLabel";
            this.minimizeLabel.Size = new System.Drawing.Size(219, 49);
            this.minimizeLabel.TabIndex = 34;
            this.minimizeLabel.Text = "Minimize SkyCal when you are selecting a star image";
            // 
            // minimizeCheckBox
            // 
            this.minimizeCheckBox.Location = new System.Drawing.Point(20, 25);
            this.minimizeCheckBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.minimizeCheckBox.Name = "minimizeCheckBox";
            this.minimizeCheckBox.Size = new System.Drawing.Size(209, 23);
            this.minimizeCheckBox.TabIndex = 33;
            this.minimizeCheckBox.Text = "Minimize When Selecting Star";
            this.minimizeCheckBox.UseVisualStyleBackColor = true;
            // 
            // ErrorSignGroupBox
            // 
            this.ErrorSignGroupBox.BorderColor = System.Drawing.Color.Gray;
            this.ErrorSignGroupBox.BorderThickness = 2;
            this.ErrorSignGroupBox.Controls.Add(this.label2);
            this.ErrorSignGroupBox.Controls.Add(this.errorSignCheckBox);
            this.ErrorSignGroupBox.CornerRadius = 12;
            this.ErrorSignGroupBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.ErrorSignGroupBox.ForeColor = System.Drawing.Color.LightGray;
            this.ErrorSignGroupBox.Location = new System.Drawing.Point(12, 148);
            this.ErrorSignGroupBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ErrorSignGroupBox.Name = "ErrorSignGroupBox";
            this.ErrorSignGroupBox.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ErrorSignGroupBox.Size = new System.Drawing.Size(263, 120);
            this.ErrorSignGroupBox.TabIndex = 38;
            this.ErrorSignGroupBox.TabStop = false;
            this.ErrorSignGroupBox.Text = "Calibration";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.label2.Location = new System.Drawing.Point(13, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(235, 57);
            this.label2.TabIndex = 34;
            this.label2.Text = "Change the sign switch if SkyCal’s \r\nguidance appears reversed.";
            // 
            // errorSignCheckBox
            // 
            this.errorSignCheckBox.Location = new System.Drawing.Point(27, 26);
            this.errorSignCheckBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.errorSignCheckBox.Name = "errorSignCheckBox";
            this.errorSignCheckBox.Size = new System.Drawing.Size(97, 23);
            this.errorSignCheckBox.TabIndex = 33;
            this.errorSignCheckBox.Text = "Sign Switch";
            this.errorSignCheckBox.UseVisualStyleBackColor = true;
            // 
            // keepOnTopGroupBox
            // 
            this.keepOnTopGroupBox.BorderColor = System.Drawing.Color.Gray;
            this.keepOnTopGroupBox.BorderThickness = 2;
            this.keepOnTopGroupBox.Controls.Add(this.onTopLabel);
            this.keepOnTopGroupBox.Controls.Add(this.onTopCheckBox);
            this.keepOnTopGroupBox.CornerRadius = 12;
            this.keepOnTopGroupBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.keepOnTopGroupBox.ForeColor = System.Drawing.Color.LightGray;
            this.keepOnTopGroupBox.Location = new System.Drawing.Point(12, 278);
            this.keepOnTopGroupBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.keepOnTopGroupBox.Name = "keepOnTopGroupBox";
            this.keepOnTopGroupBox.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.keepOnTopGroupBox.Size = new System.Drawing.Size(263, 120);
            this.keepOnTopGroupBox.TabIndex = 39;
            this.keepOnTopGroupBox.TabStop = false;
            this.keepOnTopGroupBox.Text = "Keep On Top";
            // 
            // onTopLabel
            // 
            this.onTopLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.onTopLabel.Location = new System.Drawing.Point(16, 52);
            this.onTopLabel.Name = "onTopLabel";
            this.onTopLabel.Size = new System.Drawing.Size(221, 61);
            this.onTopLabel.TabIndex = 34;
            this.onTopLabel.Text = "Make sure SkyCal is always on the top of other application";
            // 
            // onTopCheckBox
            // 
            this.onTopCheckBox.Location = new System.Drawing.Point(27, 25);
            this.onTopCheckBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.onTopCheckBox.Name = "onTopCheckBox";
            this.onTopCheckBox.Size = new System.Drawing.Size(151, 23);
            this.onTopCheckBox.TabIndex = 33;
            this.onTopCheckBox.Text = "Keep SkyCal On Top";
            this.onTopCheckBox.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.BorderColor = System.Drawing.Color.Gray;
            this.groupBox4.BorderThickness = 2;
            this.groupBox4.Controls.Add(this.label3);
            this.groupBox4.Controls.Add(this.historyMakersTextBox);
            this.groupBox4.Controls.Add(this.label1);
            this.groupBox4.CornerRadius = 12;
            this.groupBox4.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.groupBox4.ForeColor = System.Drawing.Color.LightGray;
            this.groupBox4.Location = new System.Drawing.Point(283, 147);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox4.Size = new System.Drawing.Size(268, 121);
            this.groupBox4.TabIndex = 39;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "History Markers";
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.label3.Location = new System.Drawing.Point(12, 57);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(222, 57);
            this.label3.TabIndex = 2;
            this.label3.Text = "Sets the number of history markers \r\ndisplayed on the Error Bar";
            // 
            // historyMakersTextBox
            // 
            this.historyMakersTextBox.BackColor = System.Drawing.Color.LightGray;
            this.historyMakersTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.historyMakersTextBox.Location = new System.Drawing.Point(22, 30);
            this.historyMakersTextBox.Margin = new System.Windows.Forms.Padding(3, 4, 4, 4);
            this.historyMakersTextBox.MaxLength = 20;
            this.historyMakersTextBox.Name = "historyMakersTextBox";
            this.historyMakersTextBox.Size = new System.Drawing.Size(25, 18);
            this.historyMakersTextBox.TabIndex = 1;
            this.historyMakersTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(54, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(207, 19);
            this.label1.TabIndex = 0;
            this.label1.Text = "History Makers";
            // 
            // groupBox1
            // 
            this.groupBox1.BorderColor = System.Drawing.Color.Gray;
            this.groupBox1.BorderThickness = 2;
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.VoiceCheckBox);
            this.groupBox1.CornerRadius = 12;
            this.groupBox1.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.groupBox1.ForeColor = System.Drawing.Color.LightGray;
            this.groupBox1.Location = new System.Drawing.Point(12, 14);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Size = new System.Drawing.Size(263, 124);
            this.groupBox1.TabIndex = 35;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Voice";
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.label5.Location = new System.Drawing.Point(13, 51);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(225, 67);
            this.label5.TabIndex = 34;
            this.label5.Text = "Enable voice guidance and hover\r\nyour mouse over the Focus Group \r\ntext box to ac" +
    "tivate voice callouts \r\nfor that channel";
            // 
            // VoiceCheckBox
            // 
            this.VoiceCheckBox.Location = new System.Drawing.Point(27, 25);
            this.VoiceCheckBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.VoiceCheckBox.Name = "VoiceCheckBox";
            this.VoiceCheckBox.Size = new System.Drawing.Size(121, 23);
            this.VoiceCheckBox.TabIndex = 33;
            this.VoiceCheckBox.Text = "Voice Guidance";
            this.VoiceCheckBox.UseVisualStyleBackColor = true;
            // 
            // CancelSettingsButton
            // 
            this.CancelSettingsButton.BackColor = System.Drawing.Color.DimGray;
            this.CancelSettingsButton.BevelDark = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.CancelSettingsButton.BevelLight = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
            this.CancelSettingsButton.BevelThickness = 4;
            this.CancelSettingsButton.CornerRadius = 6;
            this.CancelSettingsButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.CancelSettingsButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.CancelSettingsButton.ForeColor = System.Drawing.Color.LightGray;
            this.CancelSettingsButton.HoverOverlay = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.CancelSettingsButton.ImageHeight = 32;
            this.CancelSettingsButton.ImageOffsetX = 60;
            this.CancelSettingsButton.ImageOffsetY = 0;
            this.CancelSettingsButton.ImageWidth = 32;
            this.CancelSettingsButton.Location = new System.Drawing.Point(424, 415);
            this.CancelSettingsButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.CancelSettingsButton.Name = "CancelSettingsButton";
            this.CancelSettingsButton.PressedOverlay = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.CancelSettingsButton.Size = new System.Drawing.Size(120, 35);
            this.CancelSettingsButton.TabIndex = 14;
            this.CancelSettingsButton.Text = "Cancel";
            this.CancelSettingsButton.TextOffsetX = 0;
            this.CancelSettingsButton.UseVisualStyleBackColor = true;
            this.CancelSettingsButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // okButton
            // 
            this.okButton.BackColor = System.Drawing.Color.DimGray;
            this.okButton.BevelDark = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.okButton.BevelLight = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
            this.okButton.BevelThickness = 4;
            this.okButton.CornerRadius = 6;
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.okButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.okButton.ForeColor = System.Drawing.Color.LightGray;
            this.okButton.HoverOverlay = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.okButton.ImageHeight = 32;
            this.okButton.ImageOffsetX = 60;
            this.okButton.ImageOffsetY = 0;
            this.okButton.ImageWidth = 32;
            this.okButton.Location = new System.Drawing.Point(290, 415);
            this.okButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.okButton.Name = "okButton";
            this.okButton.PressedOverlay = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.okButton.Size = new System.Drawing.Size(120, 35);
            this.okButton.TabIndex = 13;
            this.okButton.Text = "Save";
            this.okButton.TextOffsetX = 0;
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(17)))), ((int)(((byte)(23)))));
            this.ClientSize = new System.Drawing.Size(563, 468);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.minimizeGroupBox);
            this.Controls.Add(this.ErrorSignGroupBox);
            this.Controls.Add(this.keepOnTopGroupBox);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.CancelSettingsButton);
            this.Controls.Add(this.okButton);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(80)))), ((int)(((byte)(90)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Settings";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Settings";
            this.groupBox3.ResumeLayout(false);
            this.minimizeGroupBox.ResumeLayout(false);
            this.ErrorSignGroupBox.ResumeLayout(false);
            this.keepOnTopGroupBox.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private RoundedButton okButton;
        private RoundedButton CancelSettingsButton;
        private System.Windows.Forms.CheckBox VoiceCheckBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.RadioButton MakCassRadioButton;
        private System.Windows.Forms.RadioButton SCTRadioButton;
        private RoundedGroupBox groupBox1;
        private RoundedGroupBox groupBox3;
        private System.Windows.Forms.CheckBox errorSignCheckBox;
        private RoundedGroupBox ErrorSignGroupBox;
        private RoundedGroupBox groupBox4;
        private System.Windows.Forms.TextBox historyMakersTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton newtonianRadioButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private RoundedGroupBox keepOnTopGroupBox;
        private System.Windows.Forms.Label onTopLabel;
        private System.Windows.Forms.CheckBox onTopCheckBox;
        private RoundedGroupBox minimizeGroupBox;
        private System.Windows.Forms.Label minimizeLabel;
        private System.Windows.Forms.CheckBox minimizeCheckBox;
        private System.Windows.Forms.RadioButton guidanceOffRadioButton;
    }
}