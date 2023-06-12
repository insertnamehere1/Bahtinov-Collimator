namespace Bahtinov_Collimator.CSheet
{
    partial class CheatSheet
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CheatSheet));
            this.saveButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.swapGreenCheckbox = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.blueReverseBox = new System.Windows.Forms.CheckBox();
            this.greenReverseBox = new System.Windows.Forms.CheckBox();
            this.redReverseBox = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.redErrorLabel = new System.Windows.Forms.Label();
            this.greenErrorLabel = new System.Windows.Forms.Label();
            this.blueErrorLabel = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.SuspendLayout();
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(20, 275);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 23);
            this.saveButton.TabIndex = 0;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // closeButton
            // 
            this.closeButton.Location = new System.Drawing.Point(110, 275);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 1;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Channel";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.IndianRed;
            this.label5.Location = new System.Drawing.Point(27, 48);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(27, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Red";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.Color.ForestGreen;
            this.label6.Location = new System.Drawing.Point(27, 73);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(36, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Green";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.label7.Location = new System.Drawing.Point(27, 99);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(28, 13);
            this.label7.TabIndex = 11;
            this.label7.Text = "Blue";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(102, 25);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(29, 13);
            this.label9.TabIndex = 25;
            this.label9.Text = "Error";
            // 
            // swapGreenCheckbox
            // 
            this.swapGreenCheckbox.AutoSize = true;
            this.swapGreenCheckbox.Location = new System.Drawing.Point(24, 19);
            this.swapGreenCheckbox.Name = "swapGreenCheckbox";
            this.swapGreenCheckbox.Size = new System.Drawing.Size(111, 17);
            this.swapGreenCheckbox.TabIndex = 28;
            this.swapGreenCheckbox.Text = "Swap Green/Blue";
            this.swapGreenCheckbox.UseVisualStyleBackColor = true;
            this.swapGreenCheckbox.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.blueReverseBox);
            this.groupBox1.Controls.Add(this.swapGreenCheckbox);
            this.groupBox1.Controls.Add(this.greenReverseBox);
            this.groupBox1.Controls.Add(this.redReverseBox);
            this.groupBox1.Location = new System.Drawing.Point(12, 145);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(181, 113);
            this.groupBox1.TabIndex = 29;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Configuration";
            // 
            // blueReverseBox
            // 
            this.blueReverseBox.AutoSize = true;
            this.blueReverseBox.Location = new System.Drawing.Point(24, 88);
            this.blueReverseBox.Name = "blueReverseBox";
            this.blueReverseBox.Size = new System.Drawing.Size(90, 17);
            this.blueReverseBox.TabIndex = 33;
            this.blueReverseBox.Text = "Blue Reverse";
            this.blueReverseBox.UseVisualStyleBackColor = true;
            // 
            // greenReverseBox
            // 
            this.greenReverseBox.AutoSize = true;
            this.greenReverseBox.Location = new System.Drawing.Point(24, 65);
            this.greenReverseBox.Name = "greenReverseBox";
            this.greenReverseBox.Size = new System.Drawing.Size(98, 17);
            this.greenReverseBox.TabIndex = 32;
            this.greenReverseBox.Text = "Green Reverse";
            this.greenReverseBox.UseVisualStyleBackColor = true;
            // 
            // redReverseBox
            // 
            this.redReverseBox.AutoSize = true;
            this.redReverseBox.Location = new System.Drawing.Point(24, 42);
            this.redReverseBox.Name = "redReverseBox";
            this.redReverseBox.Size = new System.Drawing.Size(89, 17);
            this.redReverseBox.TabIndex = 31;
            this.redReverseBox.Text = "Red Reverse";
            this.redReverseBox.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.blueErrorLabel);
            this.groupBox2.Controls.Add(this.greenErrorLabel);
            this.groupBox2.Controls.Add(this.redErrorLabel);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Location = new System.Drawing.Point(12, 10);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(181, 129);
            this.groupBox2.TabIndex = 30;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Adjustments";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(203, 13);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(245, 245);
            this.pictureBox1.TabIndex = 31;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
            // 
            // trackBar1
            // 
            this.trackBar1.Location = new System.Drawing.Point(203, 264);
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(247, 45);
            this.trackBar1.TabIndex = 32;
            this.trackBar1.ValueChanged += new System.EventHandler(this.trackBar1_ValueChanged);
            // 
            // redErrorLabel
            // 
            this.redErrorLabel.AutoSize = true;
            this.redErrorLabel.Location = new System.Drawing.Point(104, 48);
            this.redErrorLabel.Name = "redErrorLabel";
            this.redErrorLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.redErrorLabel.Size = new System.Drawing.Size(22, 13);
            this.redErrorLabel.TabIndex = 26;
            this.redErrorLabel.Text = "0.0";
            this.redErrorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // greenErrorLabel
            // 
            this.greenErrorLabel.AutoSize = true;
            this.greenErrorLabel.Location = new System.Drawing.Point(104, 73);
            this.greenErrorLabel.Name = "greenErrorLabel";
            this.greenErrorLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.greenErrorLabel.Size = new System.Drawing.Size(22, 13);
            this.greenErrorLabel.TabIndex = 27;
            this.greenErrorLabel.Text = "0.0";
            this.greenErrorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // blueErrorLabel
            // 
            this.blueErrorLabel.AutoSize = true;
            this.blueErrorLabel.Location = new System.Drawing.Point(104, 99);
            this.blueErrorLabel.Name = "blueErrorLabel";
            this.blueErrorLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.blueErrorLabel.Size = new System.Drawing.Size(22, 13);
            this.blueErrorLabel.TabIndex = 28;
            this.blueErrorLabel.Text = "0.0";
            this.blueErrorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // CheatSheet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(461, 315);
            this.Controls.Add(this.trackBar1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.saveButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "CheatSheet";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Adjustment Assistant";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CheatSheet_FormClosed);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox swapGreenCheckbox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox blueReverseBox;
        private System.Windows.Forms.CheckBox greenReverseBox;
        private System.Windows.Forms.CheckBox redReverseBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.Label blueErrorLabel;
        private System.Windows.Forms.Label greenErrorLabel;
        private System.Windows.Forms.Label redErrorLabel;
    }
}