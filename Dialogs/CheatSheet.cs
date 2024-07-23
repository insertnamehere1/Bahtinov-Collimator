using Bahtinov_Collimator.Sound;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

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
        #region Fields

        private int rotationAngle;
        private int circleRadius;

        private float redError = 0.0f;
        private float greenError = 0.0f;
        private float blueError = 0.0f;
        private Timer updateTimer;
        private Form1 parentForm;
        private int lastImageCount = 0;

        // settings
        private int rotationSetting;
        private bool swapColorSetting;
        private bool redReverseSetting;
        private bool greenReverseSetting;
        private bool blueReverseSetting;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CheatSheet"/> class.
        /// </summary>
        /// <param name="parent">The parent form.</param>
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

        #endregion

        #region Methods

        /// <summary>
        /// Loads the settings from the properties.
        /// </summary>
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

        /// <summary>
        /// Draws the contents of the picture box.
        /// </summary>
        /// <param name="g">The graphics object.</param>
        private void DrawPictureBox(Graphics g)
        {
            Arrow redArrow = Arrow.None;
            Arrow greenArrow = Arrow.None;
            Arrow blueArrow = Arrow.None;

            int centerX = pictureBox1.Width / 2;
            int centerY = pictureBox1.Height / 2;

            // Define the colors for the shading effect
            Color startColor = Color.White;
            Color endColor = Color.Gray;

            // Create a linear gradient brush to apply shading
            Brush shadingBrush = new LinearGradientBrush(
                new Point(centerX - circleRadius, centerY - circleRadius),
                new Point(centerX + circleRadius, centerY + circleRadius),
                startColor, endColor);

            using (SolidBrush bgBrush = new SolidBrush(SystemColors.Control))
            using (SolidBrush circleBrush = new SolidBrush(Color.DarkGray))
            using (Pen circlePen = new Pen(Color.Black, 2.0f))
            {
                g.FillRectangle(bgBrush, ClientRectangle);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                int circleSize = circleRadius * 2;
                int circleX = centerX - circleRadius;
                int circleY = centerY - circleRadius;
                g.FillEllipse(shadingBrush, circleX, circleY, circleSize, circleSize);
                g.DrawEllipse(circlePen, circleX, circleY, circleSize, circleSize);

                redArrow = GetArrow(redError, redReverseBox.Checked);
                greenArrow = GetArrow(greenError, greenReverseBox.Checked);
                blueArrow = GetArrow(blueError, blueReverseBox.Checked);

                DrawSmallCircle(g, centerX, centerY, circleRadius, -90, Color.Red, redArrow, redError);

                if (swapGreenCheckbox.Checked)
                {
                    DrawSmallCircle(g, centerX, centerY, circleRadius, -210, Color.Green, greenArrow, greenError);
                    DrawSmallCircle(g, centerX, centerY, circleRadius, -330, Color.Blue, blueArrow, blueError);
                }
                else
                {
                    DrawSmallCircle(g, centerX, centerY, circleRadius, -210, Color.Blue, blueArrow, blueError);
                    DrawSmallCircle(g, centerX, centerY, circleRadius, -330, Color.Green, greenArrow, greenError);
                }
            }
        }

        /// <summary>
        /// Gets the arrow direction based on the error value and reverse setting.
        /// </summary>
        /// <param name="error">The error value.</param>
        /// <param name="isReverse">Whether the direction is reversed.</param>
        /// <returns>The arrow direction.</returns>
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

        /// <summary>
        /// Draws a small circle with an arrow and error value.
        /// </summary>
        /// <param name="g">The graphics object.</param>
        /// <param name="centerX">The center X coordinate.</param>
        /// <param name="centerY">The center Y coordinate.</param>
        /// <param name="circleRadi">The circle radius.</param>
        /// <param name="angle">The angle for positioning.</param>
        /// <param name="color">The circle color.</param>
        /// <param name="arrowType">The arrow type.</param>
        /// <param name="errorValue">The error value.</param>
        private void DrawSmallCircle(Graphics g, int centerX, int centerY, int circleRadi, int angle, Color color, Arrow arrowType, float errorValue)
        {
            int circleRadius = (int)(circleRadi * 0.65);
            double radians = (angle + rotationAngle) * Math.PI / 180.0;
            int smallerCircleRadius = (int)(circleRadius * 0.25);
            int x = (int)(centerX + circleRadius * Math.Cos(radians));
            int y = (int)(centerY + circleRadius * Math.Sin(radians));

            int hexSize = 7;

            using (Pen circlePen = new Pen(Color.Black, 1.0f))
            using (SolidBrush brush = new SolidBrush(color))
            {
                g.FillEllipse(brush, x - smallerCircleRadius, y - smallerCircleRadius, smallerCircleRadius * 2, smallerCircleRadius * 2);
                g.DrawEllipse(circlePen, x - smallerCircleRadius, y - smallerCircleRadius, smallerCircleRadius * 2, smallerCircleRadius * 2);

                // Define the points of the hexagon
                Point[] hexagonPoints = GetHexagonPoints(x, y, hexSize);

                // Draw the hexagon
                Brush fillBrush = Brushes.Black;
                g.FillPolygon(fillBrush, hexagonPoints);
            }

            DrawArrow(g, arrowType, x, y);
            WriteValue(g, x, y, errorValue.ToString("F1"));
        }

        /// <summary>
        /// Writes a value near the specified coordinates.
        /// </summary>
        /// <param name="graphics">The graphics object.</param>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteValue(Graphics graphics, int x, int y, string value)
        {
            // Adjust position of the text
            x = (float.Parse(value) < 0.0) ? x - 17 : x - 13;
            y += 15;

            Font font = new Font("Arial", 12, FontStyle.Bold); // Specify the font for the text
            Brush brush = Brushes.Black;

            // Draw the value on the screen at the specified coordinates
            graphics.DrawString(value, font, brush, x, y);
        }

        /// <summary>
        /// Draws an arrow at the specified coordinates.
        /// </summary>
        /// <param name="graphics">The graphics object.</param>
        /// <param name="arrow">The arrow type.</param>
        /// <param name="centerX">The center X coordinate.</param>
        /// <param name="centerY">The center Y coordinate.</param>
        public void DrawArrow(Graphics graphics, Arrow arrow, int centerX, int centerY)
        {
            // Minor corrections for arrow image placement
            if (arrow == Arrow.Right)
            {
                centerX += 2;
                centerY -= 10;
            }
            else
            {
                centerX -= 3;
                centerY -= 10;
            }

            // Load the original image
            Image originalImage = Properties.Resources.arrow;

            // Calculate the size based on the arrow type
            Size imageSize = arrow == Arrow.None ? originalImage.Size : new Size(57, 32);

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

        /// <summary>
        /// Gets the points of a hexagon centered at the specified coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="size">The size of the hexagon.</param>
        /// <returns>The points of the hexagon.</returns>
        private Point[] GetHexagonPoints(int x, int y, int size)
        {
            Point[] points = new Point[6];

            points[0] = new Point(x + size, y); // Top-right
            points[1] = new Point(x + size / 2, y + (int)(size * Math.Sqrt(3) / 2)); // Bottom-right
            points[2] = new Point(x - size / 2, y + (int)(size * Math.Sqrt(3) / 2)); // Bottom-left
            points[3] = new Point(x - size, y); // Top-left
            points[4] = new Point(x - size / 2, y - (int)(size * Math.Sqrt(3) / 2)); // Top-bottom
            points[5] = new Point(x + size / 2, y - (int)(size * Math.Sqrt(3) / 2)); // Top-right

            return points;
        }

        /// <summary>
        /// Handles the Click event of the closeButton control.
        /// </summary>
        private void closeButton_Click(object sender, EventArgs e)
        {
            updateTimer.Stop();
            this.Close();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the swapGreenCheckbox control.
        /// </summary>
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        /// <summary>
        /// Handles the ValueChanged event of the trackBar1 control.
        /// </summary>
        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            rotationAngle = trackBar1.Value;
            Refresh();
        }

        /// <summary>
        /// Handles the Paint event of the pictureBox1 control.
        /// </summary>
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            DrawPictureBox(e.Graphics);
        }

        /// <summary>
        /// Handles the Click event of the saveButton control.
        /// </summary>
        private void saveButton_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.CS_Slider = trackBar1.Value;
            Properties.Settings.Default.CS_Swap = swapGreenCheckbox.Checked;
            Properties.Settings.Default.CS_RedReverse = redReverseBox.Checked;
            Properties.Settings.Default.CS_GreenReverse = greenReverseBox.Checked;
            Properties.Settings.Default.CS_BlueReverse = blueReverseBox.Checked;

            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Updates the error values.
        /// </summary>
        /// <param name="errorValues">The error values array.</param>
        internal void errorValues(float[] errorValues)
        {
            redError = errorValues[0];
            greenError = errorValues[1];
            blueError = errorValues[2];
        }

        /// <summary>
        /// Handles the Tick event of the updateTimer control.
        /// </summary>
        private void updateTimer_Tick(object sender, EventArgs e)
        {
            if (parentForm.isScreenCaptureRunning() == false)
                this.Close();

            int imageCount = parentForm.getImageCount();

            // Only update if we have a new image
            if (imageCount > lastImageCount)
            {
                float[] errorValues = parentForm.getErrorValues();
                redError = errorValues[0];
                greenError = errorValues[1];
                blueError = errorValues[2];

                pictureBox1.Invalidate();
                lastImageCount = imageCount;
            }
        }

        /// <summary>
        /// Handles the FormClosed event of the CheatSheet form.
        /// </summary>
        private void CheatSheet_FormClosed(object sender, FormClosedEventArgs e)
        {
            updateTimer.Stop();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the reverseBox controls.
        /// </summary>
        private void reverseBox_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        #endregion
    }
}
