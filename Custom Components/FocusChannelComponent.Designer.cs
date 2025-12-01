using Bahtinov_Collimator.Custom_Components;

namespace Bahtinov_Collimator
{
    partial class FocusChannelComponent
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new Bahtinov_Collimator.Custom_Components.RoundedGroupBox();
            this.offsetBarControl1 = new Bahtinov_Collimator.Custom_Components.OffsetBarControl();
            this.WithinCriticalFocusLabel = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.FocusErrorLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.BorderColor = System.Drawing.Color.Gray;
            this.groupBox1.BorderThickness = 2;
            this.groupBox1.Controls.Add(this.offsetBarControl1);
            this.groupBox1.Controls.Add(this.WithinCriticalFocusLabel);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.FocusErrorLabel);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.CornerRadius = 12;
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(3, 2);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Size = new System.Drawing.Size(234, 146);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "FocusChannel";
            // 
            // offsetBarControl1
            // 
            this.offsetBarControl1.BarColor = System.Drawing.Color.Gray;
            this.offsetBarControl1.Enabled = false;
            this.offsetBarControl1.Location = new System.Drawing.Point(6, 79);
            this.offsetBarControl1.MarkerColor = System.Drawing.Color.OrangeRed;
            this.offsetBarControl1.Maximum = 1F;
            this.offsetBarControl1.Minimum = -1F;
            this.offsetBarControl1.Name = "offsetBarControl1";
            this.offsetBarControl1.Size = new System.Drawing.Size(239, 60);
            this.offsetBarControl1.TabIndex = 8;
            this.offsetBarControl1.Text = "offsetBarControl1";
            this.offsetBarControl1.TextColor = System.Drawing.Color.White;
            this.offsetBarControl1.Value = 0F;
            this.offsetBarControl1.ZeroTickColor = System.Drawing.Color.DarkGray;
            // 
            // WithinCriticalFocusLabel
            // 
            this.WithinCriticalFocusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.WithinCriticalFocusLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.WithinCriticalFocusLabel.Location = new System.Drawing.Point(170, 48);
            this.WithinCriticalFocusLabel.Name = "WithinCriticalFocusLabel";
            this.WithinCriticalFocusLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.WithinCriticalFocusLabel.Size = new System.Drawing.Size(37, 23);
            this.WithinCriticalFocusLabel.TabIndex = 7;
            this.WithinCriticalFocusLabel.Text = "---    ";
            this.WithinCriticalFocusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(12, 50);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(145, 18);
            this.label5.TabIndex = 6;
            this.label5.Text = "Inside Critical Focus:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(170, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(23, 18);
            this.label2.TabIndex = 2;
            this.label2.Text = "px";
            // 
            // FocusErrorLabel
            // 
            this.FocusErrorLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FocusErrorLabel.Location = new System.Drawing.Point(135, 25);
            this.FocusErrorLabel.Name = "FocusErrorLabel";
            this.FocusErrorLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.FocusErrorLabel.Size = new System.Drawing.Size(41, 25);
            this.FocusErrorLabel.TabIndex = 1;
            this.FocusErrorLabel.Text = "0.0";
            this.FocusErrorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Enabled = false;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "Bahtinov Offset:";
            // 
            // FocusChannelComponent
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "FocusChannelComponent";
            this.Size = new System.Drawing.Size(237, 150);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Bahtinov_Collimator.Custom_Components.RoundedGroupBox groupBox1;
        private System.Windows.Forms.Label WithinCriticalFocusLabel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label FocusErrorLabel;
        private System.Windows.Forms.Label label1;
        private OffsetBarControl offsetBarControl1;
    }
}
