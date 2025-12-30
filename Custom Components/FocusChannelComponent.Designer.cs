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
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.offsetBarControl1 = new Bahtinov_Collimator.Custom_Components.HistoryBar();
            this.label5 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
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
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.offsetBarControl1);
            this.groupBox1.CornerRadius = 12;
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(3, 2);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Size = new System.Drawing.Size(234, 165);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "FocusChannel";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Enabled = false;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(178, 105);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 18);
            this.label4.TabIndex = 10;
            this.label4.Text = "Too";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Enabled = false;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(6, 104);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 18);
            this.label3.TabIndex = 9;
            this.label3.Text = "Too";
            // 
            // offsetBarControl1
            // 
            this.offsetBarControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.offsetBarControl1.BarColor = System.Drawing.Color.Gray;
            this.offsetBarControl1.Enabled = false;
            this.offsetBarControl1.Location = new System.Drawing.Point(6, 32);
            this.offsetBarControl1.MarkerColor = System.Drawing.Color.Green;
            this.offsetBarControl1.Maximum = 1F;
            this.offsetBarControl1.Minimum = -1F;
            this.offsetBarControl1.Name = "offsetBarControl1";
            this.offsetBarControl1.Size = new System.Drawing.Size(222, 67);
            this.offsetBarControl1.TabIndex = 8;
            this.offsetBarControl1.Text = "offsetBarControl1";
            this.offsetBarControl1.TextColor = System.Drawing.Color.White;
            this.offsetBarControl1.Value = 0F;
            this.offsetBarControl1.ZeroTickColor = System.Drawing.Color.DarkGray;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(104, 105);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(30, 18);
            this.label5.TabIndex = 12;
            this.label5.Text = "OK";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Enabled = false;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(6, 124);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 18);
            this.label2.TabIndex = 13;
            this.label2.Text = "Close";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Enabled = false;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(160, 124);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(69, 18);
            this.label6.TabIndex = 14;
            this.label6.Text = "Far Away";
            // 
            // FocusChannelComponent
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "FocusChannelComponent";
            this.Size = new System.Drawing.Size(237, 167);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Bahtinov_Collimator.Custom_Components.RoundedGroupBox groupBox1;
        private HistoryBar offsetBarControl1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label6;
    }
}
