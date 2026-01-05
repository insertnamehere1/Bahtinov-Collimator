namespace Bahtinov_Collimator.Custom_Components
{
    partial class CalibrationComponent
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.saveButton = new Bahtinov_Collimator.Custom_Components.RoundedButton();
            this.cancelButton = new Bahtinov_Collimator.Custom_Components.RoundedButton();
            this.titledRoundedRichTextBox1 = new SkyCal.Custom_Components.TitledRoundedRichTextBox();
            this.roundedPanel1 = new Bahtinov_Collimator.Custom_Components.RoundedPanel();
            this.roundedPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // saveButton
            // 
            this.saveButton.BevelDark = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.saveButton.BevelLight = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.saveButton.BevelThickness = 2;
            this.saveButton.CornerRadius = 12;
            this.saveButton.FlatAppearance.BorderSize = 0;
            this.saveButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.saveButton.HoverOverlay = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.saveButton.ImageHeight = 32;
            this.saveButton.ImageOffsetX = 60;
            this.saveButton.ImageOffsetY = 0;
            this.saveButton.ImageWidth = 32;
            this.saveButton.Location = new System.Drawing.Point(29, 20);
            this.saveButton.Name = "saveButton";
            this.saveButton.PressedOverlay = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.saveButton.Size = new System.Drawing.Size(160, 44);
            this.saveButton.TabIndex = 1;
            this.saveButton.Text = "Save";
            this.saveButton.TextOffsetX = 10;
            this.saveButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.BackColor = System.Drawing.Color.Gray;
            this.cancelButton.BevelDark = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.cancelButton.BevelLight = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.cancelButton.BevelThickness = 2;
            this.cancelButton.CornerRadius = 12;
            this.cancelButton.FlatAppearance.BorderSize = 0;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cancelButton.HoverOverlay = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.cancelButton.ImageHeight = 32;
            this.cancelButton.ImageOffsetX = 60;
            this.cancelButton.ImageOffsetY = 0;
            this.cancelButton.ImageWidth = 32;
            this.cancelButton.Location = new System.Drawing.Point(197, 20);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.PressedOverlay = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.cancelButton.Size = new System.Drawing.Size(160, 44);
            this.cancelButton.TabIndex = 0;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.TextOffsetX = 10;
            this.cancelButton.UseVisualStyleBackColor = false;
            this.cancelButton.Click += new System.EventHandler(this.roundedButton1_Click);
            // 
            // titledRoundedRichTextBox1
            // 
            this.titledRoundedRichTextBox1.BackColor = System.Drawing.Color.White;
            this.titledRoundedRichTextBox1.BorderColor = System.Drawing.Color.Gray;
            this.titledRoundedRichTextBox1.ForeColor = System.Drawing.Color.Black;
            // 
            // 
            // 
            this.titledRoundedRichTextBox1.InnerRichTextBox.BackColor = System.Drawing.Color.White;
            this.titledRoundedRichTextBox1.InnerRichTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.titledRoundedRichTextBox1.InnerRichTextBox.DetectUrls = false;
            this.titledRoundedRichTextBox1.InnerRichTextBox.ForeColor = System.Drawing.Color.Black;
            this.titledRoundedRichTextBox1.InnerRichTextBox.Location = new System.Drawing.Point(11, 41);
            this.titledRoundedRichTextBox1.InnerRichTextBox.Name = "";
            this.titledRoundedRichTextBox1.InnerRichTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.titledRoundedRichTextBox1.InnerRichTextBox.Size = new System.Drawing.Size(365, 417);
            this.titledRoundedRichTextBox1.InnerRichTextBox.TabIndex = 0;
            this.titledRoundedRichTextBox1.Location = new System.Drawing.Point(3, 23);
            this.titledRoundedRichTextBox1.Name = "titledRoundedRichTextBox1";
            this.titledRoundedRichTextBox1.Padding = new System.Windows.Forms.Padding(10);
            this.titledRoundedRichTextBox1.Size = new System.Drawing.Size(384, 506);
            this.titledRoundedRichTextBox1.TabIndex = 0;
            this.titledRoundedRichTextBox1.TitleBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(245)))));
            this.titledRoundedRichTextBox1.TitleForeColor = System.Drawing.Color.Black;
            this.titledRoundedRichTextBox1.TitleText = "Summary";
            // 
            // roundedPanel1
            // 
            this.roundedPanel1.BackColor = System.Drawing.Color.Transparent;
            this.roundedPanel1.BorderColor = System.Drawing.Color.Gray;
            this.roundedPanel1.BorderThickness = 3;
            this.roundedPanel1.Controls.Add(this.cancelButton);
            this.roundedPanel1.Controls.Add(this.saveButton);
            this.roundedPanel1.CornerRadius = 8;
            this.roundedPanel1.FillColor = System.Drawing.Color.Transparent;
            this.roundedPanel1.Location = new System.Drawing.Point(0, 541);
            this.roundedPanel1.Name = "roundedPanel1";
            this.roundedPanel1.Size = new System.Drawing.Size(390, 83);
            this.roundedPanel1.TabIndex = 3;
            // 
            // CalibrationComponent
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.roundedPanel1);
            this.Controls.Add(this.titledRoundedRichTextBox1);
            this.Name = "CalibrationComponent";
            this.Size = new System.Drawing.Size(390, 623);
            this.roundedPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private RoundedButton cancelButton;
        private RoundedButton saveButton;
        private SkyCal.Custom_Components.TitledRoundedRichTextBox titledRoundedRichTextBox1;
        private RoundedPanel roundedPanel1;
    }
}
