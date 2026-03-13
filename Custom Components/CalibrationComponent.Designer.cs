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
            this.quitButton = new Bahtinov_Collimator.Custom_Components.RoundedButton();
            this.titledRoundedRichTextBox1 = new SkyCal.Custom_Components.TitledRoundedRichTextBox();
            this.roundedPanel1 = new Bahtinov_Collimator.Custom_Components.RoundedPanel();
            this.roundedPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // quitButton
            // 
            this.quitButton.BackColor = System.Drawing.Color.Gray;
            this.quitButton.BevelDark = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.quitButton.BevelLight = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.quitButton.BevelThickness = 4;
            this.quitButton.CornerRadius = 12;
            this.quitButton.FlatAppearance.BorderSize = 0;
            this.quitButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.quitButton.HoverOverlay = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.quitButton.ImageHeight = 32;
            this.quitButton.ImageOffsetX = 60;
            this.quitButton.ImageOffsetY = 0;
            this.quitButton.ImageWidth = 32;
            this.quitButton.Location = new System.Drawing.Point(122, 25);
            this.quitButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.quitButton.Name = "quitButton";
            this.quitButton.PressedOverlay = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.quitButton.Size = new System.Drawing.Size(180, 55);
            this.quitButton.TabIndex = 0;
            this.quitButton.Text = "Quit";
            this.quitButton.TextOffsetX = 10;
            this.quitButton.UseVisualStyleBackColor = false;
            this.quitButton.Click += new System.EventHandler(this.CancelButton_Click);
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
            this.titledRoundedRichTextBox1.InnerRichTextBox.Location = new System.Drawing.Point(12, 63);
            this.titledRoundedRichTextBox1.InnerRichTextBox.Name = "";
            this.titledRoundedRichTextBox1.InnerRichTextBox.ReadOnly = true;
            this.titledRoundedRichTextBox1.InnerRichTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.titledRoundedRichTextBox1.InnerRichTextBox.Size = new System.Drawing.Size(408, 556);
            this.titledRoundedRichTextBox1.InnerRichTextBox.TabIndex = 0;
            this.titledRoundedRichTextBox1.Location = new System.Drawing.Point(3, 29);
            this.titledRoundedRichTextBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.titledRoundedRichTextBox1.Name = "titledRoundedRichTextBox1";
            this.titledRoundedRichTextBox1.Padding = new System.Windows.Forms.Padding(11, 12, 11, 12);
            this.titledRoundedRichTextBox1.ReadOnly = true;
            this.titledRoundedRichTextBox1.Size = new System.Drawing.Size(432, 632);
            this.titledRoundedRichTextBox1.TabIndex = 0;
            this.titledRoundedRichTextBox1.TitleBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(245)))));
            this.titledRoundedRichTextBox1.TitleForeColor = System.Drawing.Color.Black;
            this.titledRoundedRichTextBox1.TitleHeight = 50;
            this.titledRoundedRichTextBox1.TitleText = "Summary";
            // 
            // roundedPanel1
            // 
            this.roundedPanel1.BackColor = System.Drawing.Color.Transparent;
            this.roundedPanel1.BorderColor = System.Drawing.Color.Gray;
            this.roundedPanel1.BorderThickness = 3;
            this.roundedPanel1.Controls.Add(this.quitButton);
            this.roundedPanel1.CornerRadius = 8;
            this.roundedPanel1.FillColor = System.Drawing.Color.Transparent;
            this.roundedPanel1.Location = new System.Drawing.Point(0, 676);
            this.roundedPanel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.roundedPanel1.Name = "roundedPanel1";
            this.roundedPanel1.Size = new System.Drawing.Size(439, 104);
            this.roundedPanel1.TabIndex = 3;
            // 
            // CalibrationComponent
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.roundedPanel1);
            this.Controls.Add(this.titledRoundedRichTextBox1);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "CalibrationComponent";
            this.Size = new System.Drawing.Size(439, 779);
            this.roundedPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private RoundedButton quitButton;
        private SkyCal.Custom_Components.TitledRoundedRichTextBox titledRoundedRichTextBox1;
        private RoundedPanel roundedPanel1;
    }
}
