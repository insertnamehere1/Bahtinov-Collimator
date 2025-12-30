using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Bahtinov_Collimator.Custom_Components
{
    /// <summary>
    /// Displays a horizontal bar with minimum and maximum labels, a zero tick,
    /// and a circular marker that indicates the current value. The numeric
    /// value is rendered above the marker.
    ///
    /// Intended for visualizing Bahtinov line offsets or similar signed values.
    /// DPI aware by scaling layout and drawing based on DeviceDpi.
    /// </summary>
    public partial class HistoryBar : Control
    {
        private float minimum = -1.0f;
        private float maximum = 1.0f;
        private float value = 0.0f;

        private readonly Queue<float> valueHistory = new Queue<float>();
        public int HistoryCapacity { get; set; } = 6;

        private const int DesignDpi = 96;
        private float DpiScale => DeviceDpi / (float)DesignDpi;

        [Category("Behavior")]
        [Description("Minimum value of the scale.")]
        public float Minimum
        {
            get => minimum;
            set { minimum = value; Invalidate(); }
        }

        [Category("Behavior")]
        [Description("Maximum value of the scale.")]
        public float Maximum
        {
            get => maximum;
            set { maximum = value; Invalidate(); }
        }

        [Category("Behavior")]
        [Description("Current value displayed on the scale.")]
        public float Value
        {
            get => value;
            set
            {
                AddToHistory(this.value);
                this.value = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Color of the main bar line.")]
        public Color BarColor { get; set; } = UITheme.ErrorBarColor;

        [Category("Appearance")]
        [Description("Color of the marker (the circular pointer).")]
        public Color MarkerColor { get; set; } = UITheme.ErrorBarMarkerColorInRange;

        [Category("Appearance")]
        [Description("Color of the zero tick mark in the center of the bar.")]
        public Color ZeroTickColor { get; set; } = UITheme.ZeroTickColor;

        [Category("Appearance")]
        [Description("Base text color for labels and value.")]
        public Color TextColor { get; set; } = UITheme.ErrorBarTextColor;

        public HistoryBar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            DoubleBuffered = true;

            // Keep a stable default size. Let WinForms DPI scale handle it.
            Size = new Size(300, 60);

            HistoryCapacity = Properties.Settings.Default.historyCount;
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            return new Size(300, 60);
        }

        private void AddToHistory(float oldValue)
        {
            valueHistory.Enqueue(oldValue);
            while (valueHistory.Count > HistoryCapacity)
                valueHistory.Dequeue();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            float scale = DpiScale;

            // Layout (design units at 96 DPI)
            float leftPad = 35f * scale;
            float rightPad = 35f * scale;

            float trackHeight = 6f * scale;      // nicer than a 2px line
            float tickUp = 10f * scale;
            float tickDown = 10f * scale;

            float histBaseRadius = 8.5f * scale; // slightly smaller than main marker
            float markerRadius = 8.5f * scale;

            float valueGap = 8f * scale;

            // Value "window" (badge)
            float badgePadX = 6f * scale;
            float badgePadY = 3f * scale;
            float badgeCornerRadius = 6f * scale;
            float badgeBorderThickness = Math.Max(1f, 1.0f * scale);
            Color badgeBackColor = Color.FromArgb(200, UITheme.DarkBackground);     // gray background
            Color badgeBorderColor = Color.FromArgb(140, Color.White);  // subtle border

            int left = (int)Math.Round(leftPad);
            int right = Width - (int)Math.Round(rightPad);
            int centerY = (int)Math.Round(Height * 0.60f);

            float range = maximum - minimum;
            if (Math.Abs(range) < float.Epsilon || right <= left)
                return;

            // Text: use TextRenderer for consistency and crispness
            string minText = minimum.ToString("0.0");
            string maxText = maximum.ToString("0.0");

            int textY = centerY - (Font.Height / 2);
            TextRenderer.DrawText(g, minText, Font, new Point(0, textY), TextColor, TextFormatFlags.NoPadding);

            Size maxSize = TextRenderer.MeasureText(maxText, Font, Size.Empty, TextFormatFlags.NoPadding);
            TextRenderer.DrawText(g, maxText, Font, new Point(Width - maxSize.Width + 3, textY), TextColor, TextFormatFlags.NoPadding);

            // Track (rounded pill)
            RectangleF trackRect = new RectangleF(left, centerY - trackHeight / 2f, right - left, trackHeight);
            using (SolidBrush trackBrush = new SolidBrush(BarColor))
            {
                FillRoundedRect(g, trackBrush, trackRect, trackHeight / 2f);
            }

            // Zero tick
            float zeroT = (0f - minimum) / range;
            float zeroX = left + zeroT * (right - left);
            using (Pen zeroPen = new Pen(ZeroTickColor, Math.Max(1f, 1.6f * scale)))
            {
                g.DrawLine(zeroPen, zeroX, centerY - tickUp, zeroX, centerY + tickDown);
            }

            // History markers: fade + shrink with age (older = smaller + more transparent)
            float[] hist = valueHistory.ToArray();
            int n = hist.Length;
            for (int i = 0; i < n; i++)
            {
                float hv = hist[i];
                float clampedHist = Math.Max(minimum, Math.Min(maximum, hv));
                float ht = (clampedHist - minimum) / range;
                float hx = left + ht * (right - left);

                float age01 = (n <= 1) ? 1f : (i / (float)(n - 1)); // 0 oldest -> 1 newest
                int alpha = (int)(30 + 110 * age01);                 // newest more visible
                float r = histBaseRadius * (0.70f + 0.30f * age01);  // newest slightly larger

                Color c = Color.FromArgb(alpha, UITheme.ErrorBarHistoryColor);
                using (SolidBrush b = new SolidBrush(c))
                {
                    g.FillEllipse(b, hx - r, centerY - r, 2f * r, 2f * r);
                }
            }

            // Current marker
            float clampedValue = Math.Max(minimum, Math.Min(maximum, value));
            float t = (clampedValue - minimum) / range;
            float markerX = left + t * (right - left);

            bool outOfRange = (value < minimum || value > maximum);
            Color markerFill = outOfRange ? UITheme.ErrorBarMarkerColorOutOfRange : UITheme.ErrorBarMarkerColorInRange;

            RectangleF markerRect = new RectangleF(
                markerX - markerRadius,
                centerY - markerRadius,
                2f * markerRadius,
                2f * markerRadius);

            // Subtle shadow
            using (SolidBrush shadow = new SolidBrush(Color.FromArgb(20, Color.Black)))
            {
                RectangleF s = markerRect;
                s.Offset(1.0f * scale, 1.2f * scale);
                g.FillEllipse(shadow, s);
            }

            // Fill + outline
            using (SolidBrush mb = new SolidBrush(markerFill))
                g.FillEllipse(mb, markerRect);

            using (Pen outline = new Pen(Color.FromArgb(150, Color.White), Math.Max(1f, 1.2f * scale)))
                g.DrawEllipse(outline, markerRect);

            // Out-of-range ring (thin)
            if (outOfRange)
            {
                using (Pen ring = new Pen(UITheme.ErrorBarMarkerColorOutOfRange, Math.Max(1f, 1.2f * scale)))
                {
                    RectangleF r = markerRect;
                    r.Inflate(4f * scale, 4f * scale);
                    g.DrawEllipse(ring, r);
                }
            }

            // Value label above marker with "window" (gray badge)
            string valueStr = value.ToString("0.0");
            Size valSize = TextRenderer.MeasureText(valueStr, Font, Size.Empty, TextFormatFlags.NoPadding);

            float badgeW = valSize.Width + (badgePadX * 2f);
            float badgeH = valSize.Height + (badgePadY * 2f);

            float badgeX = markerX - (badgeW / 2f);
            float badgeY = markerRect.Top - badgeH - valueGap;

            RectangleF badgeRect = new RectangleF(badgeX, badgeY, badgeW, badgeH);

            // Optional: subtle badge shadow
            using (SolidBrush badgeShadow = new SolidBrush(Color.FromArgb(40, Color.Black)))
            {
                RectangleF s = badgeRect;
                s.Offset(1.0f * scale, 1.2f * scale);
                FillRoundedRect(g, badgeShadow, s, badgeCornerRadius);
            }

            using (SolidBrush badgeBrush = new SolidBrush(badgeBackColor))
            {
                FillRoundedRect(g, badgeBrush, badgeRect, badgeCornerRadius);
            }

            using (Pen badgePen = new Pen(badgeBorderColor, badgeBorderThickness))
            {
                DrawRoundedRect(g, badgePen, badgeRect, badgeCornerRadius);
            }

            Color valueTextColor = Color.LightGray;

            TextRenderer.DrawText(
                g,
                valueStr,
                Font,
                Rectangle.Round(badgeRect),
                valueTextColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }

        private static void FillRoundedRect(Graphics g, Brush brush, RectangleF rect, float radius)
        {
            using (GraphicsPath path = RoundedRectPath(rect, radius))
                g.FillPath(brush, path);
        }

        private static void DrawRoundedRect(Graphics g, Pen pen, RectangleF rect, float radius)
        {
            using (GraphicsPath path = RoundedRectPath(rect, radius))
                g.DrawPath(pen, path);
        }

        private static GraphicsPath RoundedRectPath(RectangleF rect, float radius)
        { 
            float r = Math.Max(0f, radius);
            float d = r * 2f;

            GraphicsPath path = new GraphicsPath();
            if (r <= 0.1f)
            {
                path.AddRectangle(rect);
                path.CloseFigure();
                return path;
            }

            path.AddArc(rect.Left, rect.Top, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Top, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.Left, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        public void ResetHistory()
        {
            value = 0.0f;
            valueHistory.Clear();
            Invalidate();
        }

    }
}

