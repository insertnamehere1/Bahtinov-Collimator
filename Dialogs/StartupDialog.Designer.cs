using Bahtinov_Collimator;
using System.Drawing;
using System.Windows.Forms;

namespace SkyCal
{
    partial class StartupDialog
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
            this.notNowButton = new Bahtinov_Collimator.Custom_Components.RoundedButton();
            this.runButton = new Bahtinov_Collimator.Custom_Components.RoundedButton();
            this.titleLabel = new System.Windows.Forms.Label();
            this.bodyLabel = new System.Windows.Forms.Label();
            this.dontShowAgainCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // notNowButton
            // 
            this.notNowButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.notNowButton.BackColor = System.Drawing.Color.DimGray;
            this.notNowButton.BevelDark = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.notNowButton.BevelLight = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
            this.notNowButton.BevelThickness = 4;
            this.notNowButton.CornerRadius = 6;
            this.notNowButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.notNowButton.FlatAppearance.BorderSize = 0;
            this.notNowButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.notNowButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.notNowButton.ForeColor = System.Drawing.Color.LightGray;
            this.notNowButton.HoverOverlay = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.notNowButton.ImageHeight = 32;
            this.notNowButton.ImageOffsetX = 60;
            this.notNowButton.ImageOffsetY = 0;
            this.notNowButton.ImageWidth = 32;
            this.notNowButton.Location = new System.Drawing.Point(388, 161);
            this.notNowButton.Name = "notNowButton";
            this.notNowButton.PressedOverlay = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.notNowButton.Size = new System.Drawing.Size(120, 35);
            this.notNowButton.TabIndex = 1;
            this.notNowButton.Text = "Not Now";
            this.notNowButton.TextOffsetX = 0;
            this.notNowButton.UseVisualStyleBackColor = true;
            // 
            // runButton
            // 
            this.runButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.runButton.BackColor = System.Drawing.Color.DimGray;
            this.runButton.BevelDark = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.runButton.BevelLight = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
            this.runButton.BevelThickness = 4;
            this.runButton.CornerRadius = 6;
            this.runButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.runButton.FlatAppearance.BorderSize = 0;
            this.runButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.runButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.runButton.ForeColor = System.Drawing.Color.LightGray;
            this.runButton.HoverOverlay = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.runButton.ImageHeight = 32;
            this.runButton.ImageOffsetX = 60;
            this.runButton.ImageOffsetY = 0;
            this.runButton.ImageWidth = 32;
            this.runButton.Location = new System.Drawing.Point(254, 161);
            this.runButton.Name = "runButton";
            this.runButton.PressedOverlay = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.runButton.Size = new System.Drawing.Size(120, 35);
            this.runButton.TabIndex = 0;
            this.runButton.Text = "Run";
            this.runButton.TextOffsetX = 0;
            this.runButton.UseVisualStyleBackColor = true;
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.titleLabel.Location = new System.Drawing.Point(14, 11);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(47, 15);
            this.titleLabel.TabIndex = 2;
            this.titleLabel.Text = "label1";
            // 
            // bodyLabel
            // 
            this.bodyLabel.AutoSize = true;
            this.bodyLabel.Location = new System.Drawing.Point(17, 44);
            this.bodyLabel.MaximumSize = new System.Drawing.Size(500, 100);
            this.bodyLabel.Name = "bodyLabel";
            this.bodyLabel.Size = new System.Drawing.Size(30, 15);
            this.bodyLabel.TabIndex = 3;
            this.bodyLabel.Text = "Test";
            // 
            // dontShowAgainCheckBox
            // 
            this.dontShowAgainCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.dontShowAgainCheckBox.AutoSize = true;
            this.dontShowAgainCheckBox.Location = new System.Drawing.Point(25, 170);
            this.dontShowAgainCheckBox.Name = "dontShowAgainCheckBox";
            this.dontShowAgainCheckBox.Size = new System.Drawing.Size(86, 19);
            this.dontShowAgainCheckBox.TabIndex = 4;
            this.dontShowAgainCheckBox.Text = "checkBox1";
            this.dontShowAgainCheckBox.UseVisualStyleBackColor = true;
            // 
            // StartupDialog
            // 
            this.AcceptButton = this.runButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(54)))), ((int)(((byte)(64)))));
            this.CancelButton = this.notNowButton;
            this.ClientSize = new System.Drawing.Size(525, 212);
            this.Controls.Add(this.dontShowAgainCheckBox);
            this.Controls.Add(this.bodyLabel);
            this.Controls.Add(this.titleLabel);
            this.Controls.Add(this.notNowButton);
            this.Controls.Add(this.runButton);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "StartupDialog";
            this.Padding = new System.Windows.Forms.Padding(14, 0, 0, 0);
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "StartupDialog";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Bahtinov_Collimator.Custom_Components.RoundedButton runButton;
        private Bahtinov_Collimator.Custom_Components.RoundedButton notNowButton;
        private Label titleLabel;
        private Label bodyLabel;
        private CheckBox dontShowAgainCheckBox;
    }
}