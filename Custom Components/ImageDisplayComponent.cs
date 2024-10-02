﻿using Bahtinov_Collimator.Image_Processing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using static Bahtinov_Collimator.BahtinovLineDataEventArgs;
using Bahtinov_Collimator.Helper;


namespace Bahtinov_Collimator
{
    public partial class ImageDisplayComponent : UserControl
    {
        #region Fields
        // List to hold multiple layers of Bitmap images
        private List<Bitmap> layers;

        // Index to track the currently selected group
        private int selectedGroup = 0;

        // scalable font for the displayed error values
        private Font errorFont;
        #endregion

        #region Constructor
        public ImageDisplayComponent()
        {
            this.AutoScaleMode = AutoScaleMode.None;

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();

            InitializeComponent();
            SetupPictureBox();

            this.layers = new List<Bitmap>();
            AddNewLayers(4);

            SetupSubscriptions();

            int width = pictureBox1.Size.Width;
            int height = pictureBox1.Size.Height;

        }
        #endregion

        #region Setup Methods
        /// <summary>
        /// Sets up event subscriptions for various components and events.
        /// </summary>
        private void SetupSubscriptions()
        {
            // Subscribe to the ImageReceivedEvent
            FocusChannelComponent.ChannelSelectDataEvent += ChannelSelected;
            BahtinovProcessing.BahtinovLineDrawEvent += OnBahtinovLineReceive;
            ImageLostEventProvider.ImageLostEvent += HandleImageLost;
            DefocusStarProcessing.DefocusCircleEvent += OnDefocusCirceReceived;

            this.pictureBox1.Paint += new PaintEventHandler(this.pictureBoxPaint);
        }

        /// <summary>
        /// Configures the PictureBox settings, such as background color.
        /// </summary>
        private void SetupPictureBox()
        {
            pictureBox1.BackColor = UITheme.DisplayBackgroundColor;
        }

        /// <summary>
        /// Handles the form's Load event and adjusts the font size based on the current display's DPI.
        /// This ensures that the font maintains a consistent physical size, regardless of the system's
        /// scaling settings. It retrieves the current DPI, calculates the appropriate font size by
        /// comparing it to the standard DPI (96), and then applies the scaled font to the form.
        /// </summary>
        /// <param name="e">Event data for the Load event.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Get the current DPI of the display
            using (Graphics g = this.CreateGraphics())
            {
                float dpi = g.DpiX; // Horizontal DPI (DpiY can also be used)

                // Set the base font size in points (e.g., 12 points)
                float baseFontSize = 25.0f;

                // Calculate the font size based on DPI (assuming 96 DPI as standard)
                float scaledFontSize = baseFontSize * 96f / dpi;

                // Apply the scaled font to the form or controls
                this.errorFont = new Font(this.Font.FontFamily, scaledFontSize);
            }
        }

        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles the event when an image is lost. Clears the display.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void HandleImageLost(object sender, ImageLostEventArgs e)
        {
            ClearDisplay();
        }

        /// <summary>
        /// Handles the event when a defocus circle is received. Updates the PictureBox with the new image and circle data.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments containing the image and circle data.</param>
        private void OnDefocusCirceReceived(object sender, DefocusCircleEventArgs e)
        {
            Bitmap image = new Bitmap(e.Image);

            if (pictureBox1.InvokeRequired)
                pictureBox1.Invoke(new Action(() => UpdatePictureBox(image, e.InnerCircleCentre, e.InnerCircleRadius, e.OuterCircleCentre, e.OuterCircleRadius, e.Distance, e.Direction)));
            else
                UpdatePictureBox(image, e.InnerCircleCentre, e.InnerCircleRadius, e.OuterCircleCentre, e.OuterCircleRadius, e.Distance, e.Direction);
        }

        /// <summary>
        /// Handles the event when Bahtinov line data is received. Updates the PictureBox with the new image and line data.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments containing the image and line data.</param>
        private void OnBahtinovLineReceive(object sender, BahtinovLineDataEventArgs e)
        {
            Bitmap image = new Bitmap(e.Image);

            if (pictureBox1.InvokeRequired)
                pictureBox1.Invoke(new Action(() => UpdatePictureBox(image, e.Linedata)));
            else
                UpdatePictureBox(image, e.Linedata);
        }

        /// <summary>
        /// Handles the event when the channel selection changes. Updates the selected group index and refreshes the PictureBox.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments containing the channel selection data.</param>
        private void ChannelSelected(object sender, ChannelSelectEventArgs e)
        {
            selectedGroup = 0; // Default value

            for (int i = 0; i < e.ChannelSelected.Length; i++)
            {
                if (e.ChannelSelected[i])
                {
                    selectedGroup = i + 1;
                    break;
                }
            }

            pictureBox1.Invalidate();
        }
        #endregion

        #region Update Methods
        /// <summary>
        /// Updates the PictureBox with the provided image and drawing data, including defocus circles and arrows.
        /// </summary>
        /// <param name="image">The Bitmap image to display.</param>
        /// <param name="innerCentre">The center point of the inner defocus circle.</param>
        /// <param name="innerRadius">The radius of the inner defocus circle.</param>
        /// <param name="outerCentre">The center point of the outer defocus circle.</param>
        /// <param name="outerRadius">The radius of the outer defocus circle.</param>
        /// <param name="distance">The distance value to display with the arrow.</param>
        /// <param name="direction">The direction angle for the arrow.</param>
        private void UpdatePictureBox(Bitmap image, PointD innerCentre, double innerRadius, PointD outerCentre, double outerRadius, double distance, double direction)
        {
            ClearAllLayers();

            DrawImageOnFirstLayer(image);
            DrawDefocusCircles(innerCentre, innerRadius, outerCentre, outerRadius);
            DrawArrow(direction, distance, image.Height, image.Width, outerRadius);
            pictureBox1.Invalidate();
        }

        /// <summary>
        /// Updates the PictureBox with the provided image and drawing data, including bahtinov line data and error values.
        /// </summary>
        /// <param name="image">The Bitmap image to display.</param>
        /// <param name="data">The Bahtinov line information and the bahtinov error values.</param>
        private void UpdatePictureBox(Bitmap image, BahtinovLineDataEventArgs.BahtinovLineData data)
        {
            ClearAllLayers();

            DrawImageOnFirstLayer(image);

            var centerX = UITheme.DisplayWindow.X / 2;
            var centerY = UITheme.DisplayWindow.Y / 2;
            var radius = centerX - 20;

            var clippingRegion = CreateCircularClippingRegion(centerX, centerY, radius);

            foreach (var group in data.LineGroups)
            {
                DrawGroupLines(group, clippingRegion);
            }

            pictureBox1.Invalidate();
        }
        #endregion

        #region Drawing Methods

        /// <summary>
        /// Draws an arrow on the PictureBox indicating the direction and distance.
        /// </summary>
        /// <param name="angleInDegrees">The angle of the arrow in degrees.</param>
        /// <param name="distance">The distance value to display with the arrow.</param>
        /// <param name="imageHeight">The height of the image.</param>
        /// <param name="imageWidth">The width of the image.</param>
        /// <param name="outerRadius">The radius of the outer defocus circle.</param>
        private void DrawArrow(double angleInDegrees, double distance, int imageHeight, int imageWidth, double outerRadius)
        {
            double circleRadius = outerRadius + 100;
            int arrowLength = 70;

            using (var g = Graphics.FromImage(layers[1]))
            {
                // Convert angle to radians
                double angleInRadians = (angleInDegrees + 180) * (Math.PI / 180.0);

                // Get the center of the PictureBox
                Point center = new Point(imageWidth / 2, imageHeight / 2);

                // Calculate the start position on the invisible circle
                Point startPoint = new Point(
                    center.X + (int)(circleRadius * Math.Cos(angleInRadians)),
                    center.Y + (int)(circleRadius * Math.Sin(angleInRadians))
                );

                // Calculate the end position by extending inwards by arrowLength
                Point endPoint = new Point(
                    startPoint.X - (int)(arrowLength * Math.Cos(angleInRadians)),
                    startPoint.Y - (int)(arrowLength * Math.Sin(angleInRadians))
                );

                // Create the arrow
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (Pen pen = new Pen(UITheme.GetGroupBoxTextColor(0), 20))
                {
                    pen.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
                    g.DrawLine(pen, startPoint, endPoint);
                }

                // Draw the distance text
                string distanceText = distance.ToString("F1");
                using (Font font = new Font("Arial", 30, FontStyle.Bold | FontStyle.Italic))
                using (Brush brush = new SolidBrush(UITheme.GetGroupBoxTextColor(0)))
                {
                    PointF textStart = new PointF(20, 20);
                    g.DrawString("Offset: " + distanceText, font, brush, textStart);
                }
            }
        }

        /// <summary>
        /// Draws defocus circles on the specified layer of the PictureBox.
        /// </summary>
        /// <param name="innerCentre">The center point of the inner defocus circle.</param>
        /// <param name="innerRadius">The radius of the inner defocus circle.</param>
        /// <param name="outerCentre">The center point of the outer defocus circle.</param>
        /// <param name="outerRadius">The radius of the outer defocus circle.</param>
        private void DrawDefocusCircles(PointD innerCentre, double innerRadius, PointD outerCentre, double outerRadius)
        {
            using (var g = Graphics.FromImage(layers[1]))
            {
                Pen dashedPen = new Pen(UITheme.GetGroupBoxTextColor(0), 3);
                dashedPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

                double layerCentreX = UITheme.DisplayWindow.X / 2.0f;
                double layerCentreY = UITheme.DisplayWindow.Y / 2.0f;

                // Draw the circle at the average inner radius (centered on the PictureBox1
                g.DrawEllipse(dashedPen, (float)(layerCentreX + innerCentre.X - innerRadius), (float)(layerCentreY + innerCentre.Y - innerRadius), (float)innerRadius * 2, (float)innerRadius * 2);
                g.DrawEllipse(dashedPen, (float)(layerCentreX + outerCentre.X - outerRadius), (float)(layerCentreY + outerCentre.Y - outerRadius), (float)(outerRadius * 2), (float)(outerRadius * 2));
            }
        }

        /// <summary>
        /// Draws the provided image on the first layer of the PictureBox.
        /// </summary>
        /// <param name="image">The Bitmap image to draw.</param>
        private void DrawImageOnFirstLayer(Bitmap image)
        {
            using (var g = Graphics.FromImage(layers[0]))
            {
                g.DrawImage(image, new Point(0, 0));
            }
        }

        /// <summary>
        /// Draws a group of lines on the PictureBox based on the provided line group data.
        /// </summary>
        /// <param name="group">The group line data for the bahtinov lines.</param>
        /// <param name="clippingRegion">The clipping circle.</param>
        private void DrawGroupLines(BahtinovLineDataEventArgs.LineGroup group, GraphicsPath clippingRegion)
        {
            using (var g = Graphics.FromImage(layers[group.GroupId + 1]))
            {
                g.SetClip(clippingRegion);

                foreach (var line in group.Lines)
                {
                    var pen = UITheme.GetDisplayLinePen(group.GroupId, line.LineId);
                    g.DrawLine(pen, line.Start, line.End);
                }

                g.ResetClip();

                DrawErrorCircleAndLine(g, group);
                DrawErrorValueText(g, group);
            }
        }

        /// <summary>
        /// Draws an error circle and an error line on the provided graphics object.
        /// </summary>
        /// <param name="g">The <see cref="Graphics"/> object used for drawing.</param>
        /// <param name="group">An instance of <see cref="BahtinovLineDataEventArgs.LineGroup"/> containing the data for the error circle and line.</param>
        private void DrawErrorCircleAndLine(Graphics g, BahtinovLineDataEventArgs.LineGroup group)
        {
            var errorPen = UITheme.GetErrorCirclePen(group.GroupId, group.ErrorCircle.InsideFocus);
            var circleRadius = UITheme.ErrorCircleRadius;

            g.DrawEllipse(errorPen, group.ErrorCircle.Origin.X, group.ErrorCircle.Origin.Y, circleRadius * 2, circleRadius * 2);
            g.DrawLine(errorPen, group.ErrorLine.Start.X, group.ErrorLine.Start.Y, group.ErrorLine.End.X, group.ErrorLine.End.Y);
        }

        /// <summary>
        /// Draws the error value text on the provided graphics object at a calculated location based on the error line data from the specified line group.
        /// </summary>
        /// <param name="g">The <see cref="Graphics"/> object used for drawing.</param>
        /// <param name="group">An instance of <see cref="BahtinovLineDataEventArgs.LineGroup"/> containing the error line data and error circle information.</param>
        private void DrawErrorValueText(Graphics g, BahtinovLineDataEventArgs.LineGroup group)
        {
            var solidBrush = UITheme.GetErrorTextBrush(group.GroupId);

            var radius = (UITheme.DisplayWindow.X / 2) - 50;

            var textStart = group.Lines[1].Start;
            var textEnd = group.Lines[1].End;

            var adjustedPoints = AdjustPointsForRadius(textStart, textEnd, this.Width, this.Height, radius);

            var textLocation = group.GroupId != 1 ? adjustedPoints.end : adjustedPoints.start;

            if (group.GroupId == 1)
                DrawLineWithCenteredText(g, adjustedPoints.start, adjustedPoints.end, group.ErrorCircle.ErrorValue, errorFont, solidBrush);
            else
                DrawLineWithCenteredText(g, adjustedPoints.end, adjustedPoints.start, group.ErrorCircle.ErrorValue, errorFont, solidBrush);
        }

        /// <summary>
        /// Draws a line of text centered at the specified start point on the provided graphics object.
        /// </summary>
        /// <param name="g">The <see cref="Graphics"/> object used for drawing the text.</param>
        /// <param name="start">The <see cref="Point"/> specifying the start position where the text will be centered.</param>
        /// <param name="end">The <see cref="Point"/> specifying the end position of the line (currently not used in this implementation).</param>
        /// <param name="text">The text string to be drawn.</param>
        /// <param name="font">The <see cref="Font"/> used to draw the text.</param>
        /// <param name="brush">The <see cref="Brush"/> used to color the text.</param>
        private void DrawLineWithCenteredText(Graphics g, Point start, Point end, string text, Font font, Brush brush)
        {
            // Measure the size of the text
            SizeF textSize = g.MeasureString(text, font);

            // Calculate the position to center the text at the start point
            PointF textPosition = new PointF(start.X - textSize.Width / 2, start.Y - textSize.Height / 2);

            // draw opaque background
            g.FillRectangle(new SolidBrush(Color.Black), textPosition.X, textPosition.Y, textSize.Width, textSize.Height);

            // Draw the text
            g.DrawString(text, font, brush, textPosition);
        }
        #endregion

        #region Helper Methods

        /// <summary>
        /// Adds a specified number of new layers to the <see cref="layers"/> collection. Each layer is represented as a <see cref="Bitmap"/> with the dimensions of the PictureBox control.
        /// </summary>
        /// <param name="count">The number of layers to add.</param>
        private void AddNewLayers(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var layer = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                layers.Add(layer);
            }
        }

        /// <summary>
        /// Handles the Paint event of the PictureBox control to render the image layers on the control.
        /// </summary>
        /// <param name="sender">The source of the event, typically the PictureBox control.</param>
        /// <param name="e">An <see cref="PaintEventArgs"/> that contains information about the paint event, including the <see cref="Graphics"/> object used for drawing.</param>
        private void pictureBoxPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(layers[0], 0, 0);

            if (selectedGroup == 0)
            {
                foreach (var layer in layers)
                {
                    e.Graphics.DrawImage(layer, 0, 0);
                }
            }
            else if (selectedGroup > 0 && selectedGroup < layers.Count)
            {
                e.Graphics.DrawImage(layers[selectedGroup], 0, 0);
            }
        }

        /// <summary>
        /// Clears all image layers by setting their contents to transparent.
        /// </summary>
        private void ClearAllLayers()
        {
            foreach (var layer in layers)
            {
                using (var g = Graphics.FromImage(layer))
                {
                    g.Clear(Color.Transparent);
                }
            }
        }

        /// <summary>
        /// Creates a circular clipping region centered at a specified point with a given radius.
        /// </summary>
        /// <param name="centerX">The x-coordinate of the center of the circle.</param>
        /// <param name="centerY">The y-coordinate of the center of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <returns>A <see cref="GraphicsPath"/> object representing the circular clipping region.</returns>
        private GraphicsPath CreateCircularClippingRegion(int centerX, int centerY, int radius)
        {
            var path = new GraphicsPath();
            path.AddEllipse(centerX - radius, centerY - radius, radius * 2, radius * 2);
            return path;
        }

        /// <summary>
        /// Adjusts two points to be positioned on a circle centered at the middle of a PictureBox with a specified radius.
        /// </summary>
        /// <param name="start">The initial start point to be adjusted.</param>
        /// <param name="end">The initial end point to be adjusted.</param>
        /// <param name="center_X">The width of the PictureBox, used to calculate the center.</param>
        /// <param name="center_Y">The height of the PictureBox, used to calculate the center.</param>
        /// <param name="radius">The radius of the circle on which the points should be adjusted.</param>
        /// <returns>
        /// A tuple containing the adjusted <paramref name="start"/> and <paramref name="end"/> points that are repositioned on the circle.
        /// </returns>
        public static (Point start, Point end) AdjustPointsForRadius(Point start, Point end, int center_X, int center_Y, float radius)
        {
            // Calculate the center of the PictureBox
            int centerX = center_X / 2;
            int centerY = center_Y / 2;

            // Calculate direction vector
            float dx = end.X - start.X;
            float dy = end.Y - start.Y;
            float length = (float)Math.Sqrt(dx * dx + dy * dy);

            // Normalize direction vector
            float unitDx = dx / length;
            float unitDy = dy / length;

            // Calculate the start point on the circle with the given radius
            start.X = (int)(centerX + radius * unitDx);
            start.Y = (int)(centerY + radius * unitDy);

            // Calculate the end point on the circle with the given radius
            end.X = (int)(centerX - radius * unitDx);
            end.Y = (int)(centerY - radius * unitDy);

            return (start, end);
        }

        /// <summary>
        /// Clears the display in the PictureBox by removing all drawn layers and disposing of the current image.
        /// </summary>
        /// <remarks>
        public void ClearDisplay()
        {
            if (pictureBox1.InvokeRequired)
            {
                // If not on the UI thread, use Invoke to marshal the call to the UI thread
                pictureBox1.Invoke(new Action(ClearDisplay));
            }
            else
            {
                ClearAllLayers();

                // Dispose of the current image if it exists
                if (pictureBox1.Image != null)
                {
                    pictureBox1.Image.Dispose();
                    pictureBox1.Image = null;
                }

                // Force the PictureBox to redraw itself as blank
                pictureBox1.Invalidate();
            }
        }
        #endregion
    }
}