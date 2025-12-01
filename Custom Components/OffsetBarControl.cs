using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Bahtinov_Collimator.Custom_Components
{
    /// <summary>
    /// Displays a horizontal bar with minimum and maximum labels, a zero tick,
    /// and a circular marker that indicates the current value. The numeric
    /// value is rendered above the marker.
    /// 
    /// Intended for visualizing Bahtinov line offsets or similar signed values.
    /// </summary>
    public partial class OffsetBarControl : Control
    {
        private float minimum = -1.0f;
        private float maximum = 1.0f;
        private float value = 0.0f;

        // Stores the last N values for drawing semi transparent history markers.
        private readonly Queue<float> valueHistory = new Queue<float>();

        /// <summary>
        /// The number of previous values to keep and render as semi-opaque markers.
        /// </summary>
        private const int HistoryCapacity = 5;

        /// <summary>
        /// Gets or sets the minimum value of the scale.
        /// </summary>
        [Category("Behavior")]
        [Description("Minimum value of the scale.")]
        public float Minimum
        {
            get => minimum;
            set
            {
                minimum = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the maximum value of the scale.
        /// </summary>
        [Category("Behavior")]
        [Description("Maximum value of the scale.")]
        public float Maximum
        {
            get => maximum;
            set
            {
                maximum = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the current value displayed by the control.
        /// The marker is clamped to the [Minimum, Maximum] range for drawing,
        /// but the raw value is still used for the numeric label and for deciding
        /// when to show the value in red if it is out of range.
        /// 
        /// Each time the value changes, the previous value is added to the
        /// history buffer so that up to the last five positions are drawn
        /// as semi-opaque markers.
        /// </summary>
        [Category("Behavior")]
        [Description("Current value displayed on the scale.")]
        public float Value
        {
            get => value;
            set
            {
                if (Math.Abs(value - this.value) < float.Epsilon)
                    return; // no change

                // Store the previous value in the history
                AddToHistory(this.value);

                this.value = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the color of the main bar line.
        /// </summary>
        [Category("Appearance")]
        [Description("Color of the main bar line.")]
        public Color BarColor { get; set; } = Color.Gray;

        /// <summary>
        /// Gets or sets the color of the marker (the circular pointer).
        /// </summary>
        [Category("Appearance")]
        [Description("Color of the marker (the circular pointer).")]
        public Color MarkerColor { get; set; } = Color.OrangeRed;

        /// <summary>
        /// Gets or sets the color of the zero tick mark in the center of the bar.
        /// </summary>
        [Category("Appearance")]
        [Description("Color of the zero tick mark.")]
        public Color ZeroTickColor { get; set; } = Color.DarkGray;

        /// <summary>
        /// Gets or sets the base color used for text (labels and value).
        /// The value label will be drawn in red if the raw value is outside
        /// the [Minimum, Maximum] range.
        /// </summary>
        [Category("Appearance")]
        [Description("Base text color for labels and value.")]
        public Color TextColor { get; set; } = Color.White;

        /// <summary>
        /// Initializes a new instance of the <see cref="OffsetBarControl"/> class.
        /// Sets up double buffering and a sensible default size.
        /// </summary>
        public OffsetBarControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint,
                     true);

            DoubleBuffered = true;
            Size = new Size(300, 80);
        }

        /// <summary>
        /// Adds a value to the history list, keeping only the last N entries.
        /// </summary>
        /// <param name="oldValue">The previous value to store for history rendering.</param>
        private void AddToHistory(float oldValue)
        {
            // You may optionally ignore NaN or infinities if that is a concern.
            valueHistory.Enqueue(oldValue);

            while (valueHistory.Count > HistoryCapacity)
                valueHistory.Dequeue();
        }

        /// <summary>
        /// Performs custom painting of the bar, labels, zero tick, history markers,
        /// the current marker, and the numeric value above the marker.
        /// </summary>
        /// <param name="e">Paint event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int margin = 0;
            int left = margin + 40;                 // space for minimum label
            int right = Width - margin - 35;        // space for maximum label
            int centerY = Height / 2;

            // Draw minimum label
            using (var textBrush = new SolidBrush(TextColor))
            {
                g.DrawString(minimum.ToString("0.0"), Font, textBrush,
                    margin, centerY - Font.Height / 2);
            }

            // Draw maximum label
            string maxText = maximum.ToString("0.0");
            SizeF maxSize = g.MeasureString(maxText, Font);

            using (var textBrush = new SolidBrush(TextColor))
            {
                g.DrawString(maxText, Font, textBrush,
                    Width - margin - maxSize.Width,
                    centerY - Font.Height / 2);
            }

            // Draw main bar line
            using (var barPen = new Pen(BarColor, 2f))
            {
                g.DrawLine(barPen, left, centerY, right, centerY);
            }

            // Draw zero tick
            float range = maximum - minimum;
            if (Math.Abs(range) < float.Epsilon)
            {
                // Avoid division by zero, nothing meaningful to draw
                return;
            }

            float zeroT = (0f - minimum) / range;        // position 0..1 along the bar
            float zeroX = left + zeroT * (right - left);

            using (var zeroPen = new Pen(ZeroTickColor, 2f))
            {
                g.DrawLine(zeroPen, zeroX, centerY - 8, zeroX, centerY + 12);
            }

            // Draw history markers (semi opaque, smaller, behind current marker)
            float historyRadius = 7f;
            //            Color historyColor = Color.FromArgb(80, MarkerColor.R, MarkerColor.G, MarkerColor.B);
            Color historyColor = Color.FromArgb(100, Color.White);
            using (var historyBrush = new SolidBrush(historyColor))
            using (var historyPen = new Pen(Color.FromArgb(120, Color.Black)))
            {
                foreach (float histValue in valueHistory)
                {
                    float clampedHist = Math.Max(minimum, Math.Min(maximum, histValue));
                    float ht = (clampedHist - minimum) / range;
                    float hx = left + ht * (right - left);

                    var histRect = new RectangleF(
                        hx - historyRadius,
                        centerY - historyRadius,
                        historyRadius * 2,
                        historyRadius * 2);

                    g.FillEllipse(historyBrush, histRect);
                    g.DrawEllipse(historyPen, histRect);
                }
            }

            // Clamp the current marker position to the bar range
            float clampedValue = Math.Max(minimum, Math.Min(maximum, value));
            float t = (clampedValue - minimum) / range;
            float markerX = left + t * (right - left);
            float markerRadius = 8f;

            var markerRect = new RectangleF(
                markerX - markerRadius,
                centerY - markerRadius,
                markerRadius * 2,
                markerRadius * 2);

            // Draw current marker (●)
            using (var markerBrush = new SolidBrush(MarkerColor))
            using (var markerPen = new Pen(Color.Black, 1f))
            {
                g.FillEllipse(markerBrush, markerRect);
                g.DrawEllipse(markerPen, markerRect);
            }

            // Draw numeric value above marker
            string valueStr = value.ToString("0.0");
            SizeF valSize = g.MeasureString(valueStr, Font);

            Color valueTextColor = TextColor;
            if (value > maximum || value < minimum)
            {
                // Highlight that the raw value is outside the defined range
                valueTextColor = Color.Red;
            }

            using (var textBrush = new SolidBrush(valueTextColor))
            {
                g.DrawString(
                    valueStr,
                    Font,
                    textBrush,
                    markerX - valSize.Width / 2f,           // centered above marker
                    markerRect.Top - valSize.Height - 2f    // small gap above marker
                );
            }
        }
        /// <summary>
        /// Clears the stored history values and refreshes the control.
        /// </summary>
        public void ResetHistory()
        {
            Value = 0.0f;
            valueHistory.Clear();
            Invalidate();         // force redraw without old markers
        }
    }
}
