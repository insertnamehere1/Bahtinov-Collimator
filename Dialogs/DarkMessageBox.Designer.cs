using Bahtinov_Collimator;
using Bahtinov_Collimator.Custom_Components;
using System.Drawing;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    partial class DarkMessageBox
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
            this.messageLabel = new System.Windows.Forms.Label();
            this.iconBox = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.cancelButton = new Bahtinov_Collimator.Custom_Components.RoundedButton();
            this.okButton = new Bahtinov_Collimator.Custom_Components.RoundedButton();
            ((System.ComponentModel.ISupportInitialize)(this.iconBox)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // messageLabel
            // 
            this.messageLabel.AutoSize = true;
            this.messageLabel.ForeColor = UITheme.MessageBoxTextColor;
            this.messageLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.messageLabel.Location = new System.Drawing.Point(78, 40);
            this.messageLabel.Name = "messageLabel";
            this.messageLabel.Size = new System.Drawing.Size(41, 15);
            this.messageLabel.TabIndex = 0;
            this.messageLabel.Text = "label1";
            this.messageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // iconBox
            // 
            this.iconBox.Location = new System.Drawing.Point(26, 25);
            this.iconBox.Name = "iconBox";
            this.iconBox.Size = new System.Drawing.Size(40, 40);
            this.iconBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.iconBox.TabIndex = 2;
            this.iconBox.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = UITheme.MessageBoxPanelBackground;
            this.panel1.Controls.Add(this.cancelButton);
            this.panel1.Controls.Add(this.okButton);
            this.panel1.Location = new System.Drawing.Point(5, 196);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(499, 53);
            this.panel1.TabIndex = 4;
            // 
            // cancelButton
            // 

            this.cancelButton.BackColor = UITheme.ButtonDarkBackground;
            this.cancelButton.ForeColor = UITheme.ButtonDarkForeground;
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.BevelDark = Color.FromArgb(180, 90, 90, 90);
            this.cancelButton.BevelLight = Color.FromArgb(220, 160, 160, 160);
            this.cancelButton.BevelThickness = 2;
            this.cancelButton.CornerRadius = 4;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.cancelButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.cancelButton.HoverOverlay = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.cancelButton.ImageHeight = 32;
            this.cancelButton.ImageOffsetX = 60;
            this.cancelButton.ImageOffsetY = 0;
            this.cancelButton.ImageWidth = 32;
            this.cancelButton.Location = new System.Drawing.Point(277, 8);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.PressedOverlay = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.cancelButton.Size = new System.Drawing.Size(82, 30);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.TextOffsetX = 0;
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // okButton
            // 


            this.okButton.BackColor = UITheme.ButtonDarkBackground;
            this.okButton.ForeColor = UITheme.ButtonDarkForeground;
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.BevelDark = Color.FromArgb(180, 90, 90, 90);
            this.okButton.BevelLight = Color.FromArgb(220, 160, 160, 160);
            this.okButton.BevelThickness = 2;
            this.okButton.CornerRadius = 4;
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.okButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.okButton.HoverOverlay = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.okButton.ImageHeight = 32;
            this.okButton.ImageOffsetX = 60;
            this.okButton.ImageOffsetY = 0;
            this.okButton.ImageWidth = 32;
            this.okButton.Location = new System.Drawing.Point(394, 12);
            this.okButton.Name = "okButton";
            this.okButton.PressedOverlay = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.okButton.Size = new System.Drawing.Size(82, 30);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "OK";
            this.okButton.TextOffsetX = 0;
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // DarkMessageBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = UITheme.DarkBackground;
            this.ClientSize = new System.Drawing.Size(472, 273);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.iconBox);
            this.Controls.Add(this.messageLabel);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DarkMessageBox";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Error Message";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.iconBox)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label messageLabel;
        private System.Windows.Forms.PictureBox iconBox;
        private System.Windows.Forms.Panel panel1;
        private RoundedButton okButton;
        private RoundedButton cancelButton;
    }
}



