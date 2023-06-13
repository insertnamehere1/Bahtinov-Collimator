using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace Bahtinov_Collimator.CSheet
{
    public partial class CustomPicture : UserControl
    {
        private int rotationAngle;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.TrackBar trackBar;
        private System.ComponentModel.Container components = null;
        private int circleRadius;

        public CustomPicture()
        {
            InitializeComponent();
            InitializeCustomComponent();
            rotationAngle = 0;
            trackBar.Minimum = 0;
            trackBar.Maximum = 360;
            trackBar.TickStyle = TickStyle.None;
            trackBar.ValueChanged += TrackBar_ValueChanged;
            circleRadius = (int)(Math.Min(Width, Height) * 0.4);
        }

        private void InitializeCustomComponent()
        {
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.trackBar = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox
            // 
            this.pictureBox.BackColor = System.Drawing.Color.LightGray;
            this.pictureBox.Location = new System.Drawing.Point(220, 10);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(225, 250);
            this.pictureBox.TabIndex = 0;
            this.pictureBox.TabStop = false;
            // 
            // trackBar
            // 
            this.trackBar.Location = new System.Drawing.Point(0, 200);
            this.trackBar.Name = "trackBar";
            this.trackBar.Size = new System.Drawing.Size(222, 45);
            this.trackBar.TabIndex = 1;
            this.trackBar.Visible = true;
            // 
            // CustomPicture
            // 
            this.Controls.Add(this.trackBar);
            this.Controls.Add(this.pictureBox);
            this.Name = "CustomPicture";
            this.Size = new System.Drawing.Size(200, 200);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        public int TrackBarValue
        {
            get { return rotationAngle; }
            set
            {
                rotationAngle = value % 360;
                trackBar.Value = rotationAngle;
                Refresh();
            }
        }

        private void TrackBar_ValueChanged(object sender, EventArgs e)
        {
            rotationAngle = trackBar.Value;
            Refresh();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawPictureBox(e.Graphics);
        }

        private void DrawPictureBox(Graphics g)
        {
            int centerX = Width / 2;
            int centerY = Height / 2;

            using (SolidBrush bgBrush = new SolidBrush(Color.LightGray))
            using (SolidBrush circleBrush = new SolidBrush(Color.DarkGray))
            using (Pen circlePen = new Pen(Color.DarkGray))
            {
                g.FillRectangle(bgBrush, ClientRectangle);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                int circleSize = circleRadius * 2;
                int circleX = centerX - circleRadius;
                int circleY = centerY - circleRadius;
                g.FillEllipse(circleBrush, circleX, circleY, circleSize, circleSize);
                g.DrawEllipse(circlePen, circleX, circleY, circleSize, circleSize);

                DrawSmallCircle(g, centerX, centerY, circleRadius, 0, Color.Red);
                DrawSmallCircle(g, centerX, centerY, circleRadius, 120, Color.Green);
                DrawSmallCircle(g, centerX, centerY, circleRadius, 240, Color.Blue);
            }
        }

        private void DrawSmallCircle(Graphics g, int centerX, int centerY, int circleRadi, int angle, Color color)
        {
            int circleRadius = (int)(circleRadi * 0.8);    
            double radians = (angle + rotationAngle) * Math.PI / 180.0;
            int smallerCircleRadius = (int)(circleRadius * 0.2);
            int x = (int)(centerX + circleRadius * Math.Cos(radians));
            int y = (int)(centerY + circleRadius * Math.Sin(radians));

            using (SolidBrush brush = new SolidBrush(color))
            {
                g.FillEllipse(brush, x - smallerCircleRadius, y - smallerCircleRadius, smallerCircleRadius * 2, smallerCircleRadius * 2);
            }
        }
    
    }
}
