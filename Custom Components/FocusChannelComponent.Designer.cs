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
            this.offsetBarControl1 = new Bahtinov_Collimator.Custom_Components.HistoryBar();
            this.mirrorDrawingComponent1 = new Bahtinov_Collimator.Custom_Components.MirrorDrawingComponent();
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
            this.groupBox1.Controls.Add(this.mirrorDrawingComponent1);
            this.groupBox1.CornerRadius = 12;
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(3, 2);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Size = new System.Drawing.Size(252, 140);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "FocusChannel";
            // 
            // mirrorDrawingComponent1
            // 
            this.mirrorDrawingComponent1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.mirrorDrawingComponent1.CornerRadius = 3;
            this.mirrorDrawingComponent1.Location = new System.Drawing.Point(12, 24);
            this.mirrorDrawingComponent1.MirrorOutlineColor = System.Drawing.Color.DimGray;
            this.mirrorDrawingComponent1.MirrorType = Bahtinov_Collimator.Custom_Components.MirrorType.SctPrimary;
            this.mirrorDrawingComponent1.Name = "mirrorDrawingComponent1";
            this.mirrorDrawingComponent1.OpticalAxisLength = 100;
            this.mirrorDrawingComponent1.Size = new System.Drawing.Size(100, 100);
            this.mirrorDrawingComponent1.TabIndex = 9;
            // 
            // offsetBarControl1
            // 
            this.offsetBarControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.offsetBarControl1.BarColor = System.Drawing.Color.Gray;
            this.offsetBarControl1.Enabled = false;
            this.offsetBarControl1.Location = new System.Drawing.Point(6, 32);
            this.offsetBarControl1.MarkerColor = System.Drawing.Color.Green;
            this.offsetBarControl1.Maximum = 2F;
            this.offsetBarControl1.Minimum = -2F;
            this.offsetBarControl1.Name = "offsetBarControl1";
            this.offsetBarControl1.Size = new System.Drawing.Size(240, 64);
            this.offsetBarControl1.TabIndex = 8;
            this.offsetBarControl1.Text = "offsetBarControl1";
            this.offsetBarControl1.TextColor = System.Drawing.Color.White;
//            this.offsetBarControl1.Value = 0F;
            this.offsetBarControl1.ZeroTickColor = System.Drawing.Color.LightGray;
            // 
            // FocusChannelComponent
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "FocusChannelComponent";
            this.Size = new System.Drawing.Size(255, 155);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Bahtinov_Collimator.Custom_Components.RoundedGroupBox groupBox1;
        private HistoryBar offsetBarControl1;
        private MirrorDrawingComponent mirrorDrawingComponent1;
    }
}
