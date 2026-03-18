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
            this.minimizeGroupBox = new Bahtinov_Collimator.Custom_Components.RoundedGroupBox();
            this.minimizeLabel = new System.Windows.Forms.Label();
            this.minimizeCheckBox = new FixedCheckBox();
            this.keepOnTopGroupBox = new Bahtinov_Collimator.Custom_Components.RoundedGroupBox();
            this.onTopLabel = new System.Windows.Forms.Label();
            this.onTopCheckBox = new FixedCheckBox();
            this.groupBox4 = new Bahtinov_Collimator.Custom_Components.RoundedGroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.historyMakersTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ErrorSignGroupBox = new Bahtinov_Collimator.Custom_Components.RoundedGroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.errorSignCheckBox = new FixedCheckBox();
            this.groupBox3 = new Bahtinov_Collimator.Custom_Components.RoundedGroupBox();
            this.newtonianRadioButton = new FixedRadioButton();
            this.MakCassRadioButton = new FixedRadioButton();
            this.SCTRadioButton = new FixedRadioButton();
            this.groupBox1 = new Bahtinov_Collimator.Custom_Components.RoundedGroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.VoiceCheckBox = new FixedCheckBox();
            this.CancelSettingsButton = new Bahtinov_Collimator.Custom_Components.RoundedButton();
            this.okButton = new Bahtinov_Collimator.Custom_Components.RoundedButton();
            this.minimizeGroupBox.SuspendLayout();
            this.keepOnTopGroupBox.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.ErrorSignGroupBox.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // minimizeGroupBox
            // 
            this.minimizeGroupBox.BorderColor = System.Drawing.Color.Gray;
            this.minimizeGroupBox.BorderThickness = 2;
            this.minimizeGroupBox.Controls.Add(this.minimizeLabel);
            this.minimizeGroupBox.Controls.Add(this.minimizeCheckBox);
            this.minimizeGroupBox.CornerRadius = 12;
            this.minimizeGroupBox.Location = new System.Drawing.Point(373, 422);
            this.minimizeGroupBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.minimizeGroupBox.Name = "minimizeGroupBox";
            this.minimizeGroupBox.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.minimizeGroupBox.Size = new System.Drawing.Size(338, 158);
            this.minimizeGroupBox.TabIndex = 40;
            this.minimizeGroupBox.TabStop = false;
            this.minimizeGroupBox.Text = "Minimize";
            // 
            // minimizeLabel
            // 
            this.minimizeLabel.Location = new System.Drawing.Point(25, 69);
            this.minimizeLabel.Name = "minimizeLabel";
            this.minimizeLabel.Size = new System.Drawing.Size(279, 49);
            this.minimizeLabel.TabIndex = 34;
            this.minimizeLabel.Text = "Minimize SkyCal when you are selecting a star image";
            // 
            // minimizeCheckBox
            // 
            this.minimizeCheckBox.AutoSize = true;
            this.minimizeCheckBox.BoxSize = 13;
            this.minimizeCheckBox.Location = new System.Drawing.Point(36, 34);
            this.minimizeCheckBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.minimizeCheckBox.Name = "minimizeCheckBox";
            this.minimizeCheckBox.Size = new System.Drawing.Size(246, 24);
            this.minimizeCheckBox.TabIndex = 33;
            this.minimizeCheckBox.Text = "Minimize When Selecting Star";
            this.minimizeCheckBox.UseVisualStyleBackColor = true;
            // 
            // keepOnTopGroupBox
            // 
            this.keepOnTopGroupBox.BorderColor = System.Drawing.Color.Gray;
            this.keepOnTopGroupBox.BorderThickness = 2;
            this.keepOnTopGroupBox.Controls.Add(this.onTopLabel);
            this.keepOnTopGroupBox.Controls.Add(this.onTopCheckBox);
            this.keepOnTopGroupBox.CornerRadius = 12;
            this.keepOnTopGroupBox.Location = new System.Drawing.Point(18, 422);
            this.keepOnTopGroupBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.keepOnTopGroupBox.Name = "keepOnTopGroupBox";
            this.keepOnTopGroupBox.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.keepOnTopGroupBox.Size = new System.Drawing.Size(338, 158);
            this.keepOnTopGroupBox.TabIndex = 39;
            this.keepOnTopGroupBox.TabStop = false;
            this.keepOnTopGroupBox.Text = "Keep On Top";
            // 
            // onTopLabel
            // 
            this.onTopLabel.Location = new System.Drawing.Point(25, 69);
            this.onTopLabel.Name = "onTopLabel";
            this.onTopLabel.Size = new System.Drawing.Size(279, 49);
            this.onTopLabel.TabIndex = 34;
            this.onTopLabel.Text = "Make sure SkyCal is always on the top of other application";
            // 
            // onTopCheckBox
            // 
            this.onTopCheckBox.AutoSize = true;
            this.onTopCheckBox.BoxSize = 13;
            this.onTopCheckBox.Location = new System.Drawing.Point(36, 34);
            this.onTopCheckBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.onTopCheckBox.Name = "onTopCheckBox";
            this.onTopCheckBox.Size = new System.Drawing.Size(181, 24);
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
            this.groupBox4.Location = new System.Drawing.Point(373, 257);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox4.Size = new System.Drawing.Size(338, 155);
            this.groupBox4.TabIndex = 39;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "History Markers";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(22, 64);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(291, 86);
            this.label3.TabIndex = 2;
            this.label3.Text = "Sets the number of history markers \r\ndisplayed on the Error Bar";
            // 
            // historyMakersTextBox
            // 
            this.historyMakersTextBox.Location = new System.Drawing.Point(44, 34);
            this.historyMakersTextBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.historyMakersTextBox.MaxLength = 20;
            this.historyMakersTextBox.Name = "historyMakersTextBox";
            this.historyMakersTextBox.Size = new System.Drawing.Size(31, 26);
            this.historyMakersTextBox.TabIndex = 1;
            this.historyMakersTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(87, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 20);
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
            this.ErrorSignGroupBox.Location = new System.Drawing.Point(18, 257);
            this.ErrorSignGroupBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ErrorSignGroupBox.Name = "ErrorSignGroupBox";
            this.ErrorSignGroupBox.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ErrorSignGroupBox.Size = new System.Drawing.Size(338, 155);
            this.ErrorSignGroupBox.TabIndex = 38;
            this.ErrorSignGroupBox.TabStop = false;
            this.ErrorSignGroupBox.Text = "Calibration";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(21, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(294, 86);
            this.label2.TabIndex = 34;
            this.label2.Text = "Change the sign switch if SkyCal’s \r\nguidance appears reversed.";
            // 
            // errorSignCheckBox
            // 
            this.errorSignCheckBox.AutoSize = true;
            this.errorSignCheckBox.BoxSize = 13;
            this.errorSignCheckBox.Location = new System.Drawing.Point(36, 34);
            this.errorSignCheckBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.errorSignCheckBox.Name = "errorSignCheckBox";
            this.errorSignCheckBox.Size = new System.Drawing.Size(118, 24);
            this.errorSignCheckBox.TabIndex = 33;
            this.errorSignCheckBox.Text = "Sign Switch";
            this.errorSignCheckBox.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.BorderColor = System.Drawing.Color.Gray;
            this.groupBox3.BorderThickness = 2;
            this.groupBox3.Controls.Add(this.newtonianRadioButton);
            this.groupBox3.Controls.Add(this.MakCassRadioButton);
            this.groupBox3.Controls.Add(this.SCTRadioButton);
            this.groupBox3.CornerRadius = 12;
            this.groupBox3.Location = new System.Drawing.Point(373, 58);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox3.Size = new System.Drawing.Size(338, 189);
            this.groupBox3.TabIndex = 36;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Guidance Configuration";
            // 
            // newtonianRadioButton
            // 
            this.newtonianRadioButton.AutoSize = true;
            this.newtonianRadioButton.BoxSize = 13;
            this.newtonianRadioButton.Enabled = false;
            this.newtonianRadioButton.Location = new System.Drawing.Point(44, 124);
            this.newtonianRadioButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.newtonianRadioButton.Name = "newtonianRadioButton";
            this.newtonianRadioButton.Size = new System.Drawing.Size(109, 24);
            this.newtonianRadioButton.TabIndex = 2;
            this.newtonianRadioButton.TabStop = true;
            this.newtonianRadioButton.Text = "Newtonian";
            this.newtonianRadioButton.UseVisualStyleBackColor = true;
            // 
            // MakCassRadioButton
            // 
            this.MakCassRadioButton.AutoSize = true;
            this.MakCassRadioButton.BoxSize = 13;
            this.MakCassRadioButton.Location = new System.Drawing.Point(44, 81);
            this.MakCassRadioButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MakCassRadioButton.Name = "MakCassRadioButton";
            this.MakCassRadioButton.Size = new System.Drawing.Size(234, 24);
            this.MakCassRadioButton.TabIndex = 1;
            this.MakCassRadioButton.TabStop = true;
            this.MakCassRadioButton.Text = "Maksutov-Cassegrain (MCT)";
            this.MakCassRadioButton.UseVisualStyleBackColor = true;
            // 
            // SCTRadioButton
            // 
            this.SCTRadioButton.AutoSize = true;
            this.SCTRadioButton.BoxSize = 13;
            this.SCTRadioButton.Location = new System.Drawing.Point(44, 42);
            this.SCTRadioButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.SCTRadioButton.Name = "SCTRadioButton";
            this.SCTRadioButton.Size = new System.Drawing.Size(223, 24);
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
            this.groupBox1.Location = new System.Drawing.Point(18, 58);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Size = new System.Drawing.Size(338, 189);
            this.groupBox1.TabIndex = 35;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Voice";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(21, 60);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(294, 124);
            this.label5.TabIndex = 34;
            this.label5.Text = "Enable voice guidance and hover\r\nyour mouse over the Focus Group \r\ntext box to ac" +
    "tivate voice callouts \r\nfor that channel";
            // 
            // VoiceCheckBox
            // 
            this.VoiceCheckBox.AutoSize = true;
            this.VoiceCheckBox.BoxSize = 13;
            this.VoiceCheckBox.Location = new System.Drawing.Point(36, 29);
            this.VoiceCheckBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.VoiceCheckBox.Name = "VoiceCheckBox";
            this.VoiceCheckBox.Size = new System.Drawing.Size(148, 24);
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
            this.CancelSettingsButton.Location = new System.Drawing.Point(542, 603);
            this.CancelSettingsButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.CancelSettingsButton.Name = "CancelSettingsButton";
            this.CancelSettingsButton.PressedOverlay = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.CancelSettingsButton.Size = new System.Drawing.Size(112, 35);
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
            this.okButton.Location = new System.Drawing.Point(421, 603);
            this.okButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.okButton.Name = "okButton";
            this.okButton.PressedOverlay = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.okButton.Size = new System.Drawing.Size(112, 35);
            this.okButton.TabIndex = 13;
            this.okButton.Text = "Save";
            this.okButton.TextOffsetX = 10;
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(742, 670);
            this.Controls.Add(this.minimizeGroupBox);
            this.Controls.Add(this.keepOnTopGroupBox);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.ErrorSignGroupBox);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.CancelSettingsButton);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Location = new System.Drawing.Point(0, 0);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Settings";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Settings";
            this.Controls.SetChildIndex(this.okButton, 0);
            this.Controls.SetChildIndex(this.CancelSettingsButton, 0);
            this.Controls.SetChildIndex(this.groupBox1, 0);
            this.Controls.SetChildIndex(this.groupBox3, 0);
            this.Controls.SetChildIndex(this.ErrorSignGroupBox, 0);
            this.Controls.SetChildIndex(this.groupBox4, 0);
            this.Controls.SetChildIndex(this.keepOnTopGroupBox, 0);
            this.Controls.SetChildIndex(this.minimizeGroupBox, 0);
            this.minimizeGroupBox.ResumeLayout(false);
            this.minimizeGroupBox.PerformLayout();
            this.keepOnTopGroupBox.ResumeLayout(false);
            this.keepOnTopGroupBox.PerformLayout();
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
        private FixedCheckBox VoiceCheckBox;
        private System.Windows.Forms.Label label5;
        private FixedRadioButton MakCassRadioButton;
        private FixedRadioButton SCTRadioButton;
        private RoundedGroupBox groupBox1;
        private RoundedGroupBox groupBox3;
        private FixedCheckBox errorSignCheckBox;
        private RoundedGroupBox ErrorSignGroupBox;
        private RoundedGroupBox groupBox4;
        private System.Windows.Forms.TextBox historyMakersTextBox;
        private System.Windows.Forms.Label label1;
        private FixedRadioButton newtonianRadioButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private RoundedGroupBox keepOnTopGroupBox;
        private System.Windows.Forms.Label onTopLabel;
        private FixedCheckBox onTopCheckBox;
        private RoundedGroupBox minimizeGroupBox;
        private System.Windows.Forms.Label minimizeLabel;
        private FixedCheckBox minimizeCheckBox;
    }
}