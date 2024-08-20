using Bahtinov_Collimator.Image_Processing;
using MathNet.Numerics.RootFinding;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using static Bahtinov_Collimator.BahtinovLineDataEventArgs;
using System.Runtime.InteropServices;


namespace Bahtinov_Collimator
{
    public partial class ImageDisplayComponent : UserControl
    {
        private List<Bitmap> layers;
        private int selectedGroup = 0;

        public ImageDisplayComponent()
        {
            InitializeComponent();
            SetupPictureBox();

            this.layers = new List<Bitmap>();
            AddNewLayers(4);

            SetupSubscriptions();

            int width = pictureBox1.Size.Width;
            int height = pictureBox1.Size.Height;

        }

        private void SetupSubscriptions()
        {
            // Subscribe to the ImageReceivedEvent
            FocusChannelComponent.ChannelSelectDataEvent += ChannelSelected;
            BahtinovProcessing.BahtinovLineDrawEvent += OnBahtinovLineReceive;
            ImageLostEventProvider.ImageLostEvent += HandleImageLost;
            DefocusStarProcessing.DefocusCircleEvent += OnDefocusCirceReceived;

            this.pictureBox1.Paint += new PaintEventHandler(this.pictureBoxPaint);
        }

        private void HandleImageLost(object sender, ImageLostEventArgs e)
        {
            ClearDisplay();
        }

        private void SetupPictureBox()
        {
            pictureBox1.BackColor = UITheme.DisplayBackgroundColor;
        }

        private void AddNewLayers(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var layer = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                layers.Add(layer);
            }
        }

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

        private void OnDefocusCirceReceived(object sender, DefocusCircleEventArgs e)
        {
            Bitmap image = new Bitmap(e.Image);

            if (pictureBox1.InvokeRequired)
                pictureBox1.Invoke(new Action(() => UpdatePictureBox(image, e.InnerCircleCentre, e.InnerCircleRadius, e.OuterCircleCentre, e.OuterCircleRadius)));
            else
                UpdatePictureBox(image, e.InnerCircleCentre, e.InnerCircleRadius, e.OuterCircleCentre, e.OuterCircleRadius);
        }


        private void OnBahtinovLineReceive(object sender, BahtinovLineDataEventArgs e)
        {
            Bitmap image = new Bitmap(e.Image);

            if (pictureBox1.InvokeRequired)
                pictureBox1.Invoke(new Action(() => UpdatePictureBox(image, e.Linedata)));
            else
                UpdatePictureBox(image, e.Linedata);
        }

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

        private void UpdatePictureBox(Bitmap image, Point innerCentre, int innerRadius, Point outerCentre, int outerRadius)
        {
            ClearAllLayers();

            DrawImageOnFirstLayer(image);
            DrawDefocusCircles(innerCentre, innerRadius, outerCentre, outerRadius);
            pictureBox1.Invalidate();
        }

        private void DrawDefocusCircles(Point innerCentre, int innerRadius, Point outerCentre, int outerRadius)
        {

            using (var g = Graphics.FromImage(layers[1]))
            {
                Pen dashedPen = new Pen(Color.Red, 3);
                dashedPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

                int layerCentreX = layers[1].Width/2;
                int layerCenterY = layers[1].Height/2;

                // Draw the circle at the average inner radius (centered on the PictureBox1
                g.DrawEllipse(dashedPen, layerCentreX + innerCentre.X - innerRadius, layerCenterY + innerCentre.Y - innerRadius, innerRadius * 2, innerRadius * 2);
                g.DrawEllipse(dashedPen, layerCentreX + outerCentre.X - outerRadius, layerCenterY + outerCentre.Y - outerRadius, outerRadius * 2, outerRadius * 2);
            }
        }

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

        private void DrawImageOnFirstLayer(Bitmap image)
        {
            using (var g = Graphics.FromImage(layers[0]))
            {
                g.DrawImage(image, new Point(0, 0));
            }
        }

        private GraphicsPath CreateCircularClippingRegion(int centerX, int centerY, int radius)
        {
            var path = new GraphicsPath();
            path.AddEllipse(centerX - radius, centerY - radius, radius * 2, radius * 2);
            return path;
        }

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

        private void DrawErrorCircleAndLine(Graphics g, BahtinovLineDataEventArgs.LineGroup group)
        {
            var errorPen = UITheme.GetErrorCirclePen(group.GroupId, group.ErrorCircle.InsideFocus);
            var circleRadius = UITheme.ErrorCircleRadius;

            g.DrawEllipse(errorPen, group.ErrorCircle.Origin.X, group.ErrorCircle.Origin.Y, circleRadius * 2, circleRadius * 2);
            g.DrawLine(errorPen, group.ErrorLine.Start.X, group.ErrorLine.Start.Y, group.ErrorLine.End.X, group.ErrorLine.End.Y);
        }

        private void DrawErrorValueText(Graphics g, BahtinovLineDataEventArgs.LineGroup group)
        {
            var textFont = UITheme.GetErrorTextFont(group.GroupId);
            var solidBrush = UITheme.GetErrorTextBrush(group.GroupId);

            var radius = (UITheme.DisplayWindow.X / 2) - 50;

            var textStart = group.Lines[1].Start;
            var textEnd = group.Lines[1].End;

            var adjustedPoints = AdjustPointsForRadius(textStart, textEnd, this.Width, this.Height, radius);

            var textLocation = group.GroupId != 1 ? adjustedPoints.end : adjustedPoints.start;

            if (group.GroupId == 1)
                DrawLineWithCenteredText(g, adjustedPoints.start, adjustedPoints.end, group.ErrorCircle.ErrorValue, textFont, solidBrush);
            else
                DrawLineWithCenteredText(g, adjustedPoints.end, adjustedPoints.start, group.ErrorCircle.ErrorValue, textFont, solidBrush);
        }

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
    }
}