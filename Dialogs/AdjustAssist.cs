﻿using Bahtinov_Collimator.Voice;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace Bahtinov_Collimator.AdjustAssistant
{
    public enum Arrow
    {
        Right, Left, None
    }

    public partial class AdjustAssist : Form
    {
        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        // Constants
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 19;

        #region Fields

        private int rotationAngle;
        private int circleRadius;

        private double redError = BahtinovProcessing.errorValues[0];
        private double greenError = BahtinovProcessing.errorValues[1];
        private double blueError = BahtinovProcessing.errorValues[2];
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
        /// Initializes a new instance of the <see cref="AdjustAssist"/> class.
        /// </summary>
        /// <param name="parent">The parent form.</param>
        public AdjustAssist(Form1 parent)
        {
            InitializeComponent();
            SetColorScheme();

            BahtinovProcessing.FocusDataEvent += FocusDataEvent;

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
        /// Handles the OnLoad event for the form.
        /// Increases the font size for the form and its controls by 2 points.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            float increasedSize = this.Font.Size + 2.0f;
            Font newFont = new Font(this.Font.FontFamily, increasedSize, this.Font.Style);

            // Adjust fonts
            this.Font = newFont;
            this.swapGreenCheckbox.Font = newFont;
            this.blueReverseBox.Font = newFont;
            this.redReverseBox.Font = newFont;
            this.greenReverseBox.Font = newFont;
            this.groupBox1.Font = newFont;
            this.saveButton.Font = newFont;
            this.closeButton.Font = newFont;
        }

        /// <summary>
        /// Configures the color scheme for the form and its controls.
        /// Applies dark theme colors to the form background, foreground, buttons, 
        /// title bar, and group boxes based on the UITheme settings.
        /// </summary>
        private void SetColorScheme()
        {
            // main form
            this.ForeColor = UITheme.DarkForeground;
            this.BackColor = UITheme.DarkBackground;

            // OK button
            saveButton.BackColor = UITheme.ButtonDarkBackground;
            saveButton.ForeColor = UITheme.ButtonDarkForeground;
            saveButton.FlatStyle = FlatStyle.Popup;

            // Cancel button
            closeButton.BackColor = UITheme.ButtonDarkBackground;
            closeButton.ForeColor = UITheme.ButtonDarkForeground;
            closeButton.FlatStyle = FlatStyle.Popup;

            // Titlebar
            var color = UITheme.DarkBackground;
            int colorValue = color.R | (color.G << 8) | (color.B << 16);
            DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref colorValue, sizeof(int));

            // Group Boxes
            ChangeLabelColors(groupBox1, UITheme.MenuDarkForeground);
            groupBox1.ForeColor = UITheme.MenuDarkForeground;
        }

        /// <summary>
        /// Changes the text color of all Label controls within a specified parent control.
        /// Iterates through the child controls of the parent and sets the ForeColor 
        /// of any Label control to the specified color.
        /// </summary>
        /// <param name="parent">The parent control containing the Label controls to be updated.</param>
        /// <param name="color">The color to apply to the ForeColor property of each Label control.</param>
        private void ChangeLabelColors(Control parent, Color color)
        {
            foreach (Control control in parent.Controls)
            {
                if (control is System.Windows.Forms.Label label)
                {
                    label.ForeColor = color;
                }
            }
        }

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
        /// Handles the focus data event, updating the focus error values for red, green, 
        /// and blue channels based on the incoming data. If the event is not on the UI thread, 
        /// the method invokes itself on the correct thread. Also triggers a redraw of the PictureBox.
        /// </summary>
        /// <param name="sender">The source of the event, typically the object that raised the event.</param>
        /// <param name="e">A <see cref="FocusDataEventArgs"/> object containing the focus data to be processed.</param>
        public void FocusDataEvent(object sender, FocusDataEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object, FocusDataEventArgs>(FocusDataEvent), sender, e);
                return;
            }

             int groupId = e.FocusData.Id;

            // is this a clear event?
            if (groupId != -1)
            {
                if (!e.FocusData.ClearDisplay)
                {
                    if(groupId == 0)
                        redError = (float)e.FocusData.BahtinovOffset;
                    else
                        if (groupId == 1)
                            greenError = (float)e.FocusData.BahtinovOffset;
                    else
                        if (groupId == 2)
                        blueError = (float)e.FocusData.BahtinovOffset;
                }
                else
                {
                    redError = 0.0f;
                    greenError = 0.0f;
                    blueError = 0.0f;
                }
            }
            pictureBox1.Invalidate();
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
            Color startColor = UITheme.AdjustAssistKnobLo;
            Color endColor = UITheme.AdjustAssistKnobHi; ;

            // Create a linear gradient brush to apply shading
            Brush shadingBrush = new LinearGradientBrush(
                new Point(centerX - circleRadius, centerY - circleRadius),
                new Point(centerX + circleRadius, centerY + circleRadius),
                startColor, endColor);

            using (SolidBrush bgBrush = new SolidBrush(UITheme.DarkBackground))
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

                DrawSmallCircle(g, centerX, centerY, circleRadius, -90, UITheme.GetGroupBoxTextColor(0), redArrow, (float)redError);

                if (swapGreenCheckbox.Checked)
                {
                    DrawSmallCircle(g, centerX, centerY, circleRadius, -210, UITheme.GetGroupBoxTextColor(1), greenArrow, greenError);
                    DrawSmallCircle(g, centerX, centerY, circleRadius, -330, UITheme.GetGroupBoxTextColor(2), blueArrow, blueError);
                }
                else
                {
                    DrawSmallCircle(g, centerX, centerY, circleRadius, -210, UITheme.GetGroupBoxTextColor(2), blueArrow, blueError);
                    DrawSmallCircle(g, centerX, centerY, circleRadius, -330, UITheme.GetGroupBoxTextColor(1), greenArrow, greenError);
                }
            }
        }

        /// <summary>
        /// Gets the arrow direction based on the error value and reverse setting.
        /// </summary>
        /// <param name="error">The error value.</param>
        /// <param name="isReverse">Whether the direction is reversed.</param>
        /// <returns>The arrow direction.</returns>
        private Arrow GetArrow(double error, bool isReverse)
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
        private void DrawSmallCircle(Graphics g, int centerX, int centerY, int circleRadi, int angle, Color color, Arrow arrowType, double errorValue)
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
            x = (float.Parse(value) < 0.0) ? x - 22 : x - 17;
            y += 18;

            Font font = new Font("Arial", 12, FontStyle.Bold); // Specify the font for the text
            Brush brush = new SolidBrush(UITheme.AdjustAssistTextColor);

            // Draw the value on the screen at the specified coordinates
            graphics.DrawString(value, font, brush, x, y);
        }

        /// <summary>
        /// Draws an arrow on the specified <see cref="Graphics"/> object at the given center coordinates.
        /// The arrow's direction can be adjusted based on the <see cref="Arrow"/> type, 
        /// and its size is scaled according to the DPI settings.
        /// </summary>
        /// <param name="graphics">The <see cref="Graphics"/> object on which to draw the arrow.</param>
        /// <param name="arrow">The direction of the arrow to be drawn. This can be Right, Left, or None.</param>
        /// <param name="centerX">The X-coordinate of the center point where the arrow should be drawn.</param>
        /// <param name="centerY">The Y-coordinate of the center point where the arrow should be drawn.</param>
        public void DrawArrow(Graphics graphics, Arrow arrow, int centerX, int centerY)
        {
            // Adjust center coordinates for the arrow's placement
            if (arrow == Arrow.Right)
            {
                centerX += 3;
                centerY -= 15;
            }
            else if (arrow == Arrow.Left)
            {
                centerX -= 4;
                centerY -= 15;
            }

            // Load the original arrow image
            Image originalImage = Properties.Resources.arrow;

            // Scale the arrow size based on DPI settings
            Size imageSize = new Size(65, 35);

            // Calculate the top-left corner of the image based on the center coordinates
            int imageX = centerX - imageSize.Width / 2;
            int imageY = centerY - imageSize.Height / 2;

            // Create a new bitmap with the calculated size
            using (Bitmap bitmap = new Bitmap(imageSize.Width, imageSize.Height))
            {
                // Create a Graphics object from the bitmap
                using (Graphics bitmapGraphics = Graphics.FromImage(bitmap))
                {
                    // Reverse the image for the left arrow
                    if (arrow == Arrow.Left)
                    {
                        bitmapGraphics.DrawImage(originalImage,
                            new Rectangle(0, 0, imageSize.Width, imageSize.Height),
                            new Rectangle(originalImage.Width, 0, -originalImage.Width, originalImage.Height),
                            GraphicsUnit.Pixel);
                    }
                    else if (arrow != Arrow.None) // Draw the image for the right arrow
                    {
                        bitmapGraphics.DrawImage(originalImage, new Rectangle(0, 0, imageSize.Width, imageSize.Height));
                    }
                }

                // Draw the bitmap on the specified Graphics object
                graphics.DrawImage(bitmap, imageX, imageY);
            }

            // Dispose of the original image
            originalImage.Dispose();
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
        /// Handles the Click event for the close button. Stops the update timer and closes the form.
        /// </summary>
        /// <param name="sender">The source of the event, typically the close button.</param>
        /// <param name="e">An <see cref="EventArgs"/> object that contains the event data.</param>
        private void closeButton_Click(object sender, EventArgs e)
        {
            updateTimer.Stop();
            this.Close();
        }

        /// <summary>
        /// Handles the CheckedChanged event for checkBox1. Invalidates the pictureBox1, causing it to be redrawn.
        /// </summary>
        /// <param name="sender">The source of the event, typically checkBox1.</param>
        /// <param name="e">An <see cref="EventArgs"/> object that contains the event data.</param>
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        /// <summary>
        /// Handles the ValueChanged event for trackBar1. Updates the rotation angle and refreshes the form.
        /// </summary>
        /// <param name="sender">The source of the event, typically trackBar1.</param>
        /// <param name="e">An <see cref="EventArgs"/> object that contains the event data.</param>
        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            rotationAngle = trackBar1.Value;
            Refresh();
        }

        /// <summary>
        /// Handles the Paint event for pictureBox1. Draws graphics on the PictureBox using the provided Graphics object.
        /// </summary>
        /// <param name="sender">The source of the event, typically pictureBox1.</param>
        /// <param name="e">A <see cref="PaintEventArgs"/> object that contains the event data, including the Graphics object used for drawing.</param>
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            DrawPictureBox(e.Graphics);
        }

        /// <summary>
        /// Handles the Click event for the saveButton. Saves current settings to application settings.
        /// </summary>
        /// <param name="sender">The source of the event, typically the saveButton.</param>
        /// <param name="e">An <see cref="EventArgs"/> object that contains the event data.</param>
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
        /// Handles the Tick event for the updateTimer. Invalidates the PictureBox to trigger a repaint.
        /// </summary>
        /// <param name="sender">The source of the event, typically the updateTimer.</param>
        /// <param name="e">An <see cref="EventArgs"/> object that contains the event data.</param>
        private void updateTimer_Tick(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        /// <summary>
        /// Handles the FormClosed event for the CheatSheet form. Unsubscribes from the FocusDataEvent and stops the updateTimer.
        /// </summary>
        /// <param name="sender">The source of the event, typically the CheatSheet form.</param>
        /// <param name="e">A <see cref="FormClosedEventArgs"/> object that contains the event data.</param>
        private void CheatSheet_FormClosed(object sender, FormClosedEventArgs e)
        {
            BahtinovProcessing.FocusDataEvent += FocusDataEvent;
            updateTimer.Stop();
        }

        /// <summary>
        /// Handles the CheckedChanged event for the reverseBox control. Invalidates the pictureBox1 control to trigger a repaint.
        /// </summary>
        /// <param name="sender">The source of the event, typically the reverseBox control.</param>
        /// <param name="e">An <see cref="EventArgs"/> object that contains the event data.</param>
        private void reverseBox_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        #endregion
    }
}
