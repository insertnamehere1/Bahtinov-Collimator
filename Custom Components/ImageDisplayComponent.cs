﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

using System.Windows.Forms;

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
        }

        private void SetupSubscriptions()
        {
            // Subscribe to the ImageReceivedEvent
            FocusChannelComponent.ChannelSelectDataEvent += ChannelSelected;
            ImageProcessing.BahtinovLineDrawEvent += OnBahtinovLineReceive;
            this.pictureBox1.Paint += new PaintEventHandler(this.pictureBoxPaint);
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

        private void UpdatePictureBox(Bitmap image, BahtinovLineDataEventArgs.BahtinovLineData data)
        {
            ClearAllLayers();

            DrawImageOnFirstLayer(image);

            var centerX = 300;
            var centerY = 300;
            var radius = 290;

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

                DrawErrorCircleAndLine(g, group);
                DrawErrorValueText(g, group);

                g.ResetClip();
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

            var textStart = group.Lines[1].Start;
            var textEnd = group.Lines[1].End;

            int margin = 50;

            var adjustedPoints = AdjustPointsForMargin(textStart, textEnd, pictureBox1.Width, pictureBox1.Height, margin);

            var textLocation = group.GroupId == 1 ? adjustedPoints.end : adjustedPoints.start;

            if (group.GroupId == 1)
                DrawLineWithCenteredText(g, adjustedPoints.end, adjustedPoints.start, group.ErrorCircle.ErrorValue, textFont, solidBrush);
            else
                DrawLineWithCenteredText(g, adjustedPoints.start, adjustedPoints.end, group.ErrorCircle.ErrorValue, textFont, solidBrush);
        }

        private void DrawLineWithCenteredText(Graphics g, Point start, Point end, string text, Font font, Brush brush)
        {
            // Measure the size of the text
            SizeF textSize = g.MeasureString(text, font);

            // Calculate the position to center the text at the start point
            PointF textPosition = new PointF(start.X - textSize.Width / 2, start.Y - textSize.Height / 2);

            // draw opaque background
            g.FillRectangle(new SolidBrush(Color.Black), textPosition.X, textPosition.Y, textSize.Width, textSize.Height);

            g.FillRectangle(new SolidBrush(Color.Transparent), textPosition.X, textPosition.Y, textSize.Width, textSize.Height);


            // Draw the text
            g.DrawString(text, font, brush, textPosition);

        }

        public static (Point start, Point end) AdjustPointsForMargin(Point start, Point end, int width, int height, int margin)
        {
            // Calculate direction vector
            float dx = end.X - start.X;
            float dy = end.Y - start.Y;
            float length = (float)Math.Sqrt(dx * dx + dy * dy);

            // Normalize direction vector
            float unitDx = dx / length;
            float unitDy = dy / length;

            // Calculate margin length
            float marginLength = (float)Math.Sqrt(margin * margin + margin * margin);

            // Adjust start point if within margin
            if (start.X < margin || start.X > width - margin || start.Y < margin || start.Y > height - margin)
            {
                start.X += (int)(marginLength * unitDx);
                start.Y += (int)(marginLength * unitDy);
            }

            // Adjust end point if within margin
            if (end.X < margin || end.X > width - margin || end.Y < margin || end.Y > height - margin)
            {
                end.X -= (int)(marginLength * unitDx);
                end.Y -= (int)(marginLength * unitDy);
            }

            return (start, end);
        }

        public void ClearDisplay()
        {
            pictureBox1.Image?.Dispose();
            pictureBox1.Image = null;
        }
    }
}