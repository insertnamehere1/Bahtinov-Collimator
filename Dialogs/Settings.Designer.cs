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
            this.groupBox4 = new Bahtinov_Collimator.Custom_Components.RoundedGroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.historyMakersTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ErrorSignGroupBox = new Bahtinov_Collimator.Custom_Components.RoundedGroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.errorSignCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new Bahtinov_Collimator.Custom_Components.RoundedGroupBox();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.MakCassRadioButton = new System.Windows.Forms.RadioButton();
            this.SCTRadioButton = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new Bahtinov_Collimator.Custom_Components.RoundedGroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.VoiceCheckBox = new System.Windows.Forms.CheckBox();
            this.CancelSettingsButton = new Bahtinov_Collimator.Custom_Components.RoundedButton();
            this.okButton = new Bahtinov_Collimator.Custom_Components.RoundedButton();
            this.groupBox4.SuspendLayout();
            this.ErrorSignGroupBox.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox4
            // 
            this.groupBox4.BorderColor = System.Drawing.Color.Gray;
            this.groupBox4.BorderThickness = 2;
            this.groupBox4.Controls.Add(this.label3);
            this.groupBox4.Controls.Add(this.historyMakersTextBox);
            this.groupBox4.Controls.Add(this.label1);
            this.groupBox4.CornerRadius = 12;
            this.groupBox4.Location = new System.Drawing.Point(329, 172);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox4.Size = new System.Drawing.Size(300, 124);
            this.groupBox4.TabIndex = 39;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "History Markers";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(20, 51);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(259, 69);
            this.label3.TabIndex = 2;
            this.label3.Text = "Sets the number of history markers \r\ndisplayed on the Error Bar";
            // 
            // historyMakersTextBox
            // 
            this.historyMakersTextBox.Location = new System.Drawing.Point(39, 27);
            this.historyMakersTextBox.MaxLength = 20;
            this.historyMakersTextBox.Name = "historyMakersTextBox";
            this.historyMakersTextBox.Size = new System.Drawing.Size(28, 22);
            this.historyMakersTextBox.TabIndex = 1;
            this.historyMakersTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(77, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "History Makers";
            // 
            // ErrorSignGroupBox
            // 
            this.ErrorSignGroupBox.BorderColor = System.Drawing.Color.Gray;
            this.ErrorSignGroupBox.BorderThickness = 2;
            this.ErrorSignGroupBox.Controls.Add(this.label2);
            this.ErrorSignGroupBox.Controls.Add(this.errorSignCheckBox);
            this.ErrorSignGroupBox.CornerRadius = 12;
            this.ErrorSignGroupBox.Location = new System.Drawing.Point(13, 172);
            this.ErrorSignGroupBox.Margin = new System.Windows.Forms.Padding(4);
            this.ErrorSignGroupBox.Name = "ErrorSignGroupBox";
            this.ErrorSignGroupBox.Padding = new System.Windows.Forms.Padding(4);
            this.ErrorSignGroupBox.Size = new System.Drawing.Size(300, 124);
            this.ErrorSignGroupBox.TabIndex = 38;
            this.ErrorSignGroupBox.TabStop = false;
            this.ErrorSignGroupBox.Text = "Calibration";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(19, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(261, 69);
            this.label2.TabIndex = 34;
            this.label2.Text = "Change the sign switch if SkyCal’s \r\nguidance appears reversed.";
            // 
            // errorSignCheckBox
            // 
            this.errorSignCheckBox.AutoSize = true;
            this.errorSignCheckBox.Location = new System.Drawing.Point(32, 27);
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
            this.groupBox3.Controls.Add(this.radioButton1);
            this.groupBox3.Controls.Add(this.MakCassRadioButton);
            this.groupBox3.Controls.Add(this.SCTRadioButton);
            this.groupBox3.CornerRadius = 12;
            this.groupBox3.Location = new System.Drawing.Point(329, 13);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(300, 151);
            this.groupBox3.TabIndex = 36;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Guidance Configuration";
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Enabled = false;
            this.radioButton1.Location = new System.Drawing.Point(39, 99);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(91, 20);
            this.radioButton1.TabIndex = 2;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "Newtonian";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // MakCassRadioButton
            // 
            this.MakCassRadioButton.AutoSize = true;
            this.MakCassRadioButton.Location = new System.Drawing.Point(39, 65);
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
            this.SCTRadioButton.Location = new System.Drawing.Point(39, 34);
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
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.VoiceCheckBox);
            this.groupBox1.CornerRadius = 12;
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(300, 151);
            this.groupBox1.TabIndex = 35;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Voice";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(19, 48);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(261, 99);
            this.label5.TabIndex = 34;
            this.label5.Text = "Enable voice guidance and hover\r\nyour mouse over the Focus Group \r\ntext box to ac" +
    "tivate voice callouts \r\nfor that channel";
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
            // CancelSettingsButton
            // 
            this.CancelSettingsButton.BevelDark = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.CancelSettingsButton.BevelLight = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.CancelSettingsButton.BevelThickness = 2;
            this.CancelSettingsButton.CornerRadius = 12;
            this.CancelSettingsButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CancelSettingsButton.HoverOverlay = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.CancelSettingsButton.ImageHeight = 32;
            this.CancelSettingsButton.ImageOffsetX = 60;
            this.CancelSettingsButton.ImageOffsetY = 0;
            this.CancelSettingsButton.ImageWidth = 32;
            this.CancelSettingsButton.Location = new System.Drawing.Point(517, 314);
            this.CancelSettingsButton.Margin = new System.Windows.Forms.Padding(4);
            this.CancelSettingsButton.Name = "CancelSettingsButton";
            this.CancelSettingsButton.PressedOverlay = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.CancelSettingsButton.Size = new System.Drawing.Size(100, 28);
            this.CancelSettingsButton.TabIndex = 14;
            this.CancelSettingsButton.Text = "Cancel";
            this.CancelSettingsButton.TextOffsetX = 10;
            this.CancelSettingsButton.UseVisualStyleBackColor = true;
            this.CancelSettingsButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // okButton
            // 
            this.okButton.BevelDark = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.okButton.BevelLight = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.okButton.BevelThickness = 2;
            this.okButton.CornerRadius = 12;
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.okButton.HoverOverlay = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.okButton.ImageHeight = 32;
            this.okButton.ImageOffsetX = 60;
            this.okButton.ImageOffsetY = 0;
            this.okButton.ImageWidth = 32;
            this.okButton.Location = new System.Drawing.Point(409, 314);
            this.okButton.Margin = new System.Windows.Forms.Padding(4);
            this.okButton.Name = "okButton";
            this.okButton.PressedOverlay = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.okButton.Size = new System.Drawing.Size(100, 28);
            this.okButton.TabIndex = 13;
            this.okButton.Text = "Save";
            this.okButton.TextOffsetX = 10;
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(644, 357);
            this.Controls.Add(this.groupBox4);
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
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ErrorSignGroupBox.ResumeLayout(false);
            this.ErrorSignGroupBox.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
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
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }
}