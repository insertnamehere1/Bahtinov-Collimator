using Bahtinov_Collimator.Custom_Components;
using System.Drawing;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    partial class NextStepDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private Panel scrollPanel;
        private FlowLayoutPanel stackLayout;
        private RoundedButton okButton;

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

        /// <summary>
        /// Required method for Designer support.
        /// </summary>
        private void InitializeComponent()
        {
            this.scrollPanel = new System.Windows.Forms.Panel();
            this.stackLayout = new System.Windows.Forms.FlowLayoutPanel();
            this.okButton = new Bahtinov_Collimator.Custom_Components.RoundedButton();
            this.scrollPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // scrollPanel
            // 
            this.scrollPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scrollPanel.AutoScroll = true;
            this.scrollPanel.BackColor = UITheme.DarkBackground;
            this.scrollPanel.Controls.Add(this.stackLayout);
            this.scrollPanel.Location = new System.Drawing.Point(20, 10);
            this.scrollPanel.Name = "scrollPanel";
            this.scrollPanel.Size = new System.Drawing.Size(720, 490);
            this.scrollPanel.TabIndex = 0;
            // 
            // stackLayout
            // 
            this.stackLayout.AutoSize = true;
            this.stackLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.stackLayout.BackColor = UITheme.DarkBackground;
            this.stackLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.stackLayout.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.stackLayout.Location = new System.Drawing.Point(0, 0);
            this.stackLayout.Margin = new System.Windows.Forms.Padding(0);
            this.stackLayout.Name = "stackLayout";
            this.stackLayout.Size = new System.Drawing.Size(720, 0);
            this.stackLayout.TabIndex = 0;
            this.stackLayout.WrapContents = false;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.BackColor = UITheme.ButtonDarkBackground;
            this.okButton.BevelDark = UITheme.RoundedButtonBevelDark;
            this.okButton.BevelLight = UITheme.RoundedButtonBevelLight;
            this.okButton.BevelThickness = 4;
            this.okButton.CornerRadius = 6;
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.okButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.okButton.ForeColor = UITheme.ButtonDarkForeground;
            this.okButton.HoverOverlay = UITheme.RoundedButtonHoverOverlay;
            this.okButton.ImageHeight = 32;
            this.okButton.ImageOffsetX = 60;
            this.okButton.ImageOffsetY = 0;
            this.okButton.ImageWidth = 32;
            this.okButton.Location = new System.Drawing.Point(624, 530);
            this.okButton.Name = "okButton";
            this.okButton.PressedOverlay = UITheme.RoundedButtonPressedOverlay;
            this.okButton.Size = new System.Drawing.Size(120, 35);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "OK";
            this.okButton.TextOffsetX = 0;
            this.okButton.UseVisualStyleBackColor = false;
            // 
            // NextStepDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = UITheme.DarkBackground;
            this.ClientSize = new System.Drawing.Size(760, 580);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.scrollPanel);
            this.ForeColor = UITheme.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NextStepDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "What should I do next?";
            this.scrollPanel.ResumeLayout(false);
            this.scrollPanel.PerformLayout();
            this.ResumeLayout(false);

        }
    }
}
