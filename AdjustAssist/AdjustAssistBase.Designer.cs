namespace Bahtinov_Collimator.AdjustAssistant
{
    partial class AdjustAssistBase
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdjustAssistBase));
            this.saveButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.swapGreenCheckbox = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.blueReverseBox = new System.Windows.Forms.CheckBox();
            this.greenReverseBox = new System.Windows.Forms.CheckBox();
            this.redReverseBox = new System.Windows.Forms.CheckBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.SuspendLayout();
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(21, 493);
            this.saveButton.Margin = new System.Windows.Forms.Padding(4);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(100, 28);
            this.saveButton.TabIndex = 0;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // closeButton
            // 
            this.closeButton.Location = new System.Drawing.Point(141, 493);
            this.closeButton.Margin = new System.Windows.Forms.Padding(4);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(100, 28);
            this.closeButton.TabIndex = 1;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // swapGreenCheckbox
            // 
            this.swapGreenCheckbox.AutoSize = true;
            this.swapGreenCheckbox.Location = new System.Drawing.Point(8, 23);
            this.swapGreenCheckbox.Margin = new System.Windows.Forms.Padding(4);
            this.swapGreenCheckbox.Name = "swapGreenCheckbox";
            this.swapGreenCheckbox.Size = new System.Drawing.Size(134, 20);
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
            this.groupBox1.Location = new System.Drawing.Point(13, 15);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(161, 139);
            this.groupBox1.TabIndex = 29;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Configuration";
            // 
            // blueReverseBox
            // 
            this.blueReverseBox.AutoSize = true;
            this.blueReverseBox.Location = new System.Drawing.Point(8, 108);
            this.blueReverseBox.Margin = new System.Windows.Forms.Padding(4);
            this.blueReverseBox.Name = "blueReverseBox";
            this.blueReverseBox.Size = new System.Drawing.Size(111, 20);
            this.blueReverseBox.TabIndex = 33;
            this.blueReverseBox.Text = "Blue Reverse";
            this.blueReverseBox.UseVisualStyleBackColor = true;
            this.blueReverseBox.CheckedChanged += new System.EventHandler(this.reverseBox_CheckedChanged);
            // 
            // greenReverseBox
            // 
            this.greenReverseBox.AutoSize = true;
            this.greenReverseBox.Location = new System.Drawing.Point(8, 80);
            this.greenReverseBox.Margin = new System.Windows.Forms.Padding(4);
            this.greenReverseBox.Name = "greenReverseBox";
            this.greenReverseBox.Size = new System.Drawing.Size(121, 20);
            this.greenReverseBox.TabIndex = 32;
            this.greenReverseBox.Text = "Green Reverse";
            this.greenReverseBox.UseVisualStyleBackColor = true;
            this.greenReverseBox.CheckedChanged += new System.EventHandler(this.reverseBox_CheckedChanged);
            // 
            // redReverseBox
            // 
            this.redReverseBox.AutoSize = true;
            this.redReverseBox.Location = new System.Drawing.Point(8, 52);
            this.redReverseBox.Margin = new System.Windows.Forms.Padding(4);
            this.redReverseBox.Name = "redReverseBox";
            this.redReverseBox.Size = new System.Drawing.Size(110, 20);
            this.redReverseBox.TabIndex = 31;
            this.redReverseBox.Text = "Red Reverse";
            this.redReverseBox.UseVisualStyleBackColor = true;
            this.redReverseBox.CheckedChanged += new System.EventHandler(this.reverseBox_CheckedChanged);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(182, 25);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(435, 387);
            this.pictureBox1.TabIndex = 31;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
            // 
            // trackBar1
            // 
            this.trackBar1.Location = new System.Drawing.Point(266, 465);
            this.trackBar1.Margin = new System.Windows.Forms.Padding(4);
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(329, 56);
            this.trackBar1.TabIndex = 32;
            this.trackBar1.ValueChanged += new System.EventHandler(this.trackBar1_ValueChanged);
            // 
            // AdjustAssistBase
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(630, 544);
            this.Controls.Add(this.trackBar1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.saveButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "AdjustAssistBase";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Adjustment Assistant";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CheatSheet_FormClosed);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button closeButton;
        protected System.Windows.Forms.CheckBox swapGreenCheckbox;
        private System.Windows.Forms.GroupBox groupBox1;
        protected System.Windows.Forms.CheckBox blueReverseBox;
        protected System.Windows.Forms.CheckBox greenReverseBox;
        protected System.Windows.Forms.CheckBox redReverseBox;
        protected System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TrackBar trackBar1;
    }
}