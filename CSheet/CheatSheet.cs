using MathNet.Numerics.Random;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace Bahtinov_Collimator.CSheet
{
    public enum Arrow
    {
        Right,
        Left,
        None
    }

    public partial class CheatSheet : Form
    {
        private int rotationAngle;
        private int circleRadius;

        private float redError = 0.0f;
        private float greenError = 0.0f;
        private float blueError = 0.0f;
        private Timer updateTimer;
        private Form1 parentForm;

        // settings
        private int rotationSetting;
        private bool swapColorSetting;
        private bool redReverseSetting;
        private bool greenReverseSetting;
        private bool blueReverseSetting;

        public CheatSheet(Form1 parent)
        {
            InitializeComponent();

            rotationAngle = 0;
            trackBar1.Minimum = 0;
            trackBar1.Maximum = 360;
            trackBar1.TickStyle = TickStyle.None;
            circleRadius = (int)(Math.Min(pictureBox1.Width, pictureBox1.Height) * 0.4);

            updateTimer = new Timer();
            updateTimer.Interval = 100; // 100 milliseconds
            updateTimer.Tick += updateTimer_Tick;

            LoadSettings();

            parentForm = parent;
            updateTimer.Start();
        }

        private void LoadSettings()
        {
            rotationSetting = Properties.Settings.Default.CS_Slider;
            swapColorSetting = Properties.Settings.Default.CS_Swap;
            redReverseSetting = Properties.Settings.Default.CS_RedReverse;
            greenReverseSetting = Properties.Settings.Default.CS_GreenReverse;
            blueReverseSetting = Properties.Settings.Default.CS_BlueReverse;

            trackBar1.Value = rotationSetting;
            swapGreenCheckbox.Checked = swapColorSetting;
            redReverseBox.Checked = redReverseSetting;
            greenReverseBox.Checked = greenReverseSetting;
            blueReverseBox.Checked = blueReverseSetting;
        }

        private void DrawPictureBox(Graphics g)
        {
            Arrow redArrow = Arrow.None;
            Arrow greenArrow = Arrow.None;
            Arrow blueArrow = Arrow.None;

            int centerX = pictureBox1.Width / 2;
            int centerY = pictureBox1.Height / 2;

            using (SolidBrush bgBrush = new SolidBrush(Color.Black))
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

                redArrow = GetArrow(redError, redReverseBox.Checked);
                greenArrow = GetArrow(greenError, greenReverseBox.Checked);
                blueArrow = GetArrow(blueError, blueReverseBox.Checked);

                DrawSmallCircle(g, centerX, centerY, circleRadius, -90, Color.Red, redArrow);
                //DrawArrow(g, centerX, centerY, circleRadius, -90, Color.Red, false);

                if (swapGreenCheckbox.Checked == true)
                {
                    DrawSmallCircle(g, centerX, centerY, circleRadius, -210, Color.Green, greenArrow);
                    DrawSmallCircle(g, centerX, centerY, circleRadius, -330, Color.Blue, blueArrow);
                }
                else
                {
                    DrawSmallCircle(g, centerX, centerY, circleRadius, -210, Color.Blue, blueArrow);
                    DrawSmallCircle(g, centerX, centerY, circleRadius, -330, Color.Green, greenArrow);
                }
            }
        }

        private Arrow GetArrow(float error, bool isReverse)
        {   
            float value = float.Parse(error.ToString("F1"));

            if (value > 0)
                return isReverse ? Arrow.Left : Arrow.Right;
            else if (value < 0)
                return isReverse ? Arrow.Right : Arrow.Left;
            else
                return Arrow.None;
        }

        private void DrawSmallCircle(Graphics g, int centerX, int centerY, int circleRadi, int angle, Color color, Arrow arrowType)
        {
            int circleRadius = (int)(circleRadi * 0.65);
            double radians = (angle + rotationAngle) * Math.PI / 180.0;
            int smallerCircleRadius = (int)(circleRadius * 0.25);
            int x = (int)(centerX + circleRadius * Math.Cos(radians));
            int y = (int)(centerY + circleRadius * Math.Sin(radians));

            using (SolidBrush brush = new SolidBrush(color))
            {
                g.FillEllipse(brush, x - smallerCircleRadius, y - smallerCircleRadius, smallerCircleRadius * 2, smallerCircleRadius * 2);
            }

            DrawArrow(g, arrowType, x, y);
        }


        public void DrawArrow(Graphics graphics, Arrow arrow, int centerX, int centerY)
        {
            // minor corrections for arrow image placement
            if(arrow == Arrow.Right)
            {
                centerX += 4;
                centerY -= 10;
            }
            else
            {
                centerX -= 4;   
                centerY -= 10;
            }

            // Load the original image
            Image originalImage = Properties.Resources.arrow;

            // Calculate the size based on the arrow type
            Size imageSize = arrow == Arrow.None ? originalImage.Size : new Size(70, 40);

            // Calculate the top-left corner of the image based on the center coordinates
            int imageX = centerX - imageSize.Width / 2;
            int imageY = centerY - imageSize.Height / 2;

            // Create a new bitmap with the calculated size
            Bitmap bitmap = new Bitmap(imageSize.Width, imageSize.Height);

            // Create a Graphics object from the bitmap
            using (Graphics bitmapGraphics = Graphics.FromImage(bitmap))
            {
                // If arrow is Left, reverse the image
                if (arrow == Arrow.Left)
                {
                    bitmapGraphics.DrawImage(originalImage, new Rectangle(0, 0, imageSize.Width, imageSize.Height),
                        new Rectangle(originalImage.Width, 0, -originalImage.Width, originalImage.Height), GraphicsUnit.Pixel);
                }
                else if (arrow != Arrow.None)
                {
                    bitmapGraphics.DrawImage(originalImage, new Rectangle(0, 0, imageSize.Width, imageSize.Height));
                }
            }

            // Draw the bitmap on the specified Graphics object
            graphics.DrawImage(bitmap, imageX, imageY);

            // Dispose of the created objects
            originalImage.Dispose();
            bitmap.Dispose();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            updateTimer.Stop();
            this.Close();
        }
        
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            rotationAngle = trackBar1.Value;
            Refresh();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            DrawPictureBox(e.Graphics);
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.CS_Slider = trackBar1.Value;
            Properties.Settings.Default.CS_Swap = swapGreenCheckbox.Checked;
            Properties.Settings.Default.CS_RedReverse = redReverseBox.Checked;
            Properties.Settings.Default.CS_GreenReverse = greenReverseBox.Checked;
            Properties.Settings.Default.CS_BlueReverse = blueReverseBox.Checked;

            Properties.Settings.Default.Save();
        }

        internal void errorValues(float[] errorValues)
        {
            redError = errorValues[0];
            greenError = errorValues[1];    
            blueError = errorValues[2];
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            if (parentForm.isScreenCaptureRunning() == false)
                this.Close();

            float[] errorValues = parentForm.getErrorValues();
            redError = errorValues[0];
            greenError = errorValues[1];
            blueError = errorValues[2];

            redErrorLabel.Text = redError.ToString("F1");
            greenErrorLabel.Text = greenError.ToString("F1");
            blueErrorLabel.Text = blueError.ToString("F1");

            pictureBox1.Invalidate();
        }

        private void CheatSheet_FormClosed(object sender, FormClosedEventArgs e)
        {
            updateTimer.Stop();
        }
    }
}
