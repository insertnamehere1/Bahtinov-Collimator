using Bahtinov_Collimator.Custom_Components;

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
            this.okButton = new System.Windows.Forms.Button();
            this.CancelSettingsButton = new System.Windows.Forms.Button();
            this.roundedGroupBox1 = new Bahtinov_Collimator.Custom_Components.RoundedGroupBox();
            this.historyMakersTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ErrorSignGroupBox = new Bahtinov_Collimator.Custom_Components.RoundedGroupBox();
            this.errorSignCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new Bahtinov_Collimator.Custom_Components.RoundedGroupBox();
            this.MakCassRadioButton = new System.Windows.Forms.RadioButton();
            this.SCTRadioButton = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new Bahtinov_Collimator.Custom_Components.RoundedGroupBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.VoiceCheckBox = new System.Windows.Forms.CheckBox();
            this.roundedGroupBox1.SuspendLayout();
            this.ErrorSignGroupBox.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(356, 285);
            this.okButton.Margin = new System.Windows.Forms.Padding(4);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(100, 28);
            this.okButton.TabIndex = 13;
            this.okButton.Text = "Save";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // CancelSettingsButton
            // 
            this.CancelSettingsButton.Location = new System.Drawing.Point(464, 285);
            this.CancelSettingsButton.Margin = new System.Windows.Forms.Padding(4);
            this.CancelSettingsButton.Name = "CancelSettingsButton";
            this.CancelSettingsButton.Size = new System.Drawing.Size(100, 28);
            this.CancelSettingsButton.TabIndex = 14;
            this.CancelSettingsButton.Text = "Cancel";
            this.CancelSettingsButton.UseVisualStyleBackColor = true;
            this.CancelSettingsButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // roundedGroupBox1
            // 
            this.roundedGroupBox1.BorderColor = System.Drawing.Color.Gray;
            this.roundedGroupBox1.BorderThickness = 2;
            this.roundedGroupBox1.Controls.Add(this.historyMakersTextBox);
            this.roundedGroupBox1.Controls.Add(this.label1);
            this.roundedGroupBox1.CornerRadius = 12;
            this.roundedGroupBox1.Location = new System.Drawing.Point(13, 230);
            this.roundedGroupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.roundedGroupBox1.Name = "roundedGroupBox1";
            this.roundedGroupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.roundedGroupBox1.Size = new System.Drawing.Size(300, 83);
            this.roundedGroupBox1.TabIndex = 39;
            this.roundedGroupBox1.TabStop = false;
            this.roundedGroupBox1.Text = "History Markers";
            // 
            // historyMakersTextBox
            // 
            this.historyMakersTextBox.Location = new System.Drawing.Point(212, 34);
            this.historyMakersTextBox.Name = "historyMakersTextBox";
            this.historyMakersTextBox.Size = new System.Drawing.Size(59, 22);
            this.historyMakersTextBox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(196, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "No. of History Makers to display";
            // 
            // ErrorSignGroupBox
            // 
            this.ErrorSignGroupBox.BorderColor = System.Drawing.Color.Gray;
            this.ErrorSignGroupBox.BorderThickness = 2;
            this.ErrorSignGroupBox.Controls.Add(this.errorSignCheckBox);
            this.ErrorSignGroupBox.CornerRadius = 12;
            this.ErrorSignGroupBox.Location = new System.Drawing.Point(13, 152);
            this.ErrorSignGroupBox.Margin = new System.Windows.Forms.Padding(4);
            this.ErrorSignGroupBox.Name = "ErrorSignGroupBox";
            this.ErrorSignGroupBox.Padding = new System.Windows.Forms.Padding(4);
            this.ErrorSignGroupBox.Size = new System.Drawing.Size(300, 70);
            this.ErrorSignGroupBox.TabIndex = 38;
            this.ErrorSignGroupBox.TabStop = false;
            this.ErrorSignGroupBox.Text = "Change Error Sign";
            // 
            // errorSignCheckBox
            // 
            this.errorSignCheckBox.AutoSize = true;
            this.errorSignCheckBox.Location = new System.Drawing.Point(32, 30);
            this.errorSignCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.errorSignCheckBox.Name = "errorSignCheckBox";
            this.errorSignCheckBox.Size = new System.Drawing.Size(97, 20);
            this.errorSignCheckBox.TabIndex = 33;
            this.errorSignCheckBox.Text = "Sign Switch";
            this.errorSignCheckBox.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.BorderColor = System.Drawing.Color.Gray;
            this.groupBox3.BorderThickness = 2;
            this.groupBox3.Controls.Add(this.MakCassRadioButton);
            this.groupBox3.Controls.Add(this.SCTRadioButton);
            this.groupBox3.CornerRadius = 12;
            this.groupBox3.Location = new System.Drawing.Point(329, 13);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(261, 255);
            this.groupBox3.TabIndex = 36;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Adjustment Display";
            // 
            // MakCassRadioButton
            // 
            this.MakCassRadioButton.AutoSize = true;
            this.MakCassRadioButton.Enabled = false;
            this.MakCassRadioButton.Location = new System.Drawing.Point(16, 59);
            this.MakCassRadioButton.Name = "MakCassRadioButton";
            this.MakCassRadioButton.Size = new System.Drawing.Size(199, 20);
            this.MakCassRadioButton.TabIndex = 1;
            this.MakCassRadioButton.TabStop = true;
            this.MakCassRadioButton.Text = "Maksutov-Cassegrain (MCT)";
            this.MakCassRadioButton.UseVisualStyleBackColor = true;
            // 
            // SCTRadioButton
            // 
            this.SCTRadioButton.AutoSize = true;
            this.SCTRadioButton.Location = new System.Drawing.Point(16, 28);
            this.SCTRadioButton.Name = "SCTRadioButton";
            this.SCTRadioButton.Size = new System.Drawing.Size(188, 20);
            this.SCTRadioButton.TabIndex = 0;
            this.SCTRadioButton.TabStop = true;
            this.SCTRadioButton.Text = "Schmidt–Cassegrain (SCT)";
            this.SCTRadioButton.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.BorderColor = System.Drawing.Color.Gray;
            this.groupBox1.BorderThickness = 2;
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.VoiceCheckBox);
            this.groupBox1.CornerRadius = 12;
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(300, 131);
            this.groupBox1.TabIndex = 35;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Voice";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(41, 97);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(78, 16);
            this.label10.TabIndex = 37;
            this.label10.Text = "that channel";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(40, 80);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(219, 16);
            this.label9.TabIndex = 36;
            this.label9.Text = "text box to activate voice callouts for";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(41, 64);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(220, 16);
            this.label8.TabIndex = 35;
            this.label8.Text = "your mouse over the Focus Channel";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(41, 47);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(208, 16);
            this.label5.TabIndex = 34;
            this.label5.Text = "Enable voice guidance and hover";
            // 
            // VoiceCheckBox
            // 
            this.VoiceCheckBox.AutoSize = true;
            this.VoiceCheckBox.Location = new System.Drawing.Point(32, 23);
            this.VoiceCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.VoiceCheckBox.Name = "VoiceCheckBox";
            this.VoiceCheckBox.Size = new System.Drawing.Size(125, 20);
            this.VoiceCheckBox.TabIndex = 33;
            this.VoiceCheckBox.Text = "Voice Guidance";
            this.VoiceCheckBox.UseVisualStyleBackColor = true;
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(603, 329);
            this.Controls.Add(this.roundedGroupBox1);
            this.Controls.Add(this.ErrorSignGroupBox);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.CancelSettingsButton);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Settings";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Settings";
            this.roundedGroupBox1.ResumeLayout(false);
            this.roundedGroupBox1.PerformLayout();
            this.ErrorSignGroupBox.ResumeLayout(false);
            this.ErrorSignGroupBox.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button CancelSettingsButton;
        private System.Windows.Forms.CheckBox VoiceCheckBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.RadioButton MakCassRadioButton;
        private System.Windows.Forms.RadioButton SCTRadioButton;
        private RoundedGroupBox groupBox1;
        private RoundedGroupBox groupBox3;
        private System.Windows.Forms.CheckBox errorSignCheckBox;
        private RoundedGroupBox ErrorSignGroupBox;
        private RoundedGroupBox roundedGroupBox1;
        private System.Windows.Forms.TextBox historyMakersTextBox;
        private System.Windows.Forms.Label label1;
    }
}