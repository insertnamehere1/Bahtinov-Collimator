using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Bahtinov_Collimator.Custom_Components
{
    /// <summary>
    /// A GroupBox with rounded corners and a continuous border,
    /// with the text rendered over a cleared area at the top.
    /// </summary>
    public partial class RoundedGroupBox : GroupBox
    {
        #region Appearance Properties

        /// <summary>
        /// Radius of the rounded corners in pixels.
        /// </summary>
        public int CornerRadius { get; set; } = 12;

        /// <summary>
        /// Color of the rounded border.
        /// </summary>
        public Color BorderColor { get; set; } = Color.Gray;

        /// <summary>
        /// Thickness (in pixels) of the border line.
        /// </summary>
        public int BorderThickness { get; set; } = 2;

        #endregion

        #region Lifecycle

        /// <summary>
        /// Initializes the rounded group box and enables double-buffering
        /// and redraw-on-resize for smooth rendering.
        /// </summary>
        public RoundedGroupBox()
        {
            DoubleBuffered = true;

            ResizeRedraw = true;
        }

        #endregion

        #region Painting

        /// <summary>
        /// Custom paint routine for drawing the rounded background,
        /// border, and title text.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            float textHeight = e.Graphics.MeasureString(Text, Font).Height;
            int y0 = (int)(textHeight / 2);

            int bodyW = Math.Max(0, Width - 1);
            int bodyH = Math.Max(0, Height - y0 - 1);
            Rectangle outerBody = new Rectangle(0, y0, bodyW, bodyH);

            int t = BorderThickness;
            int strokeInset = t / 2;

            Rectangle strokeBounds = new Rectangle(
                outerBody.X + strokeInset,
                outerBody.Y + strokeInset,
                Math.Max(0, outerBody.Width - t),
                Math.Max(0, outerBody.Height - t));

            Rectangle fillBounds = new Rectangle(
                outerBody.X + t,
                outerBody.Y + t,
                Math.Max(0, outerBody.Width - 2 * t),
                Math.Max(0, outerBody.Height - 2 * t));
            int innerRadius = Math.Max(0, CornerRadius - t);

            using (GraphicsPath fillPath = GetRoundPath(fillBounds, innerRadius))
            using (SolidBrush brush = new SolidBrush(BackColor))
            {
                e.Graphics.FillPath(brush, fillPath);
            }

            using (GraphicsPath strokePath = GetRoundPath(strokeBounds, CornerRadius))
            using (Pen pen = new Pen(BorderColor, t))
            {
                pen.Alignment = PenAlignment.Center;
                e.Graphics.DrawPath(pen, strokePath);
            }

            SizeF textSize = e.Graphics.MeasureString(Text, Font);

            RectangleF textRect = new RectangleF(
                10,
                0,
                textSize.Width + 2,
                textSize.Height);

            using (SolidBrush backBrush = new SolidBrush(BackColor))
                e.Graphics.FillRectangle(backBrush, textRect);

            using (SolidBrush textBrush = new SolidBrush(ForeColor))
                e.Graphics.DrawString(Text, Font, textBrush, 10, 0);
        }

        #endregion

        #region Geometry Helpers

        /// <summary>
        /// Limits radius so quarter-arcs fit; avoids straight segments between arcs.
        /// </summary>
        private static int ClampRadius(Rectangle bounds, int radius)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return 0;
            int max = Math.Min(bounds.Width, bounds.Height) / 2;
            return Math.Max(0, Math.Min(radius, max));
        }

        /// <summary>
        /// Builds a rounded-rectangle path for the given bounds and corner radius.
        /// </summary>
        private GraphicsPath GetRoundPath(Rectangle rect, int radius)
        {
            int r = ClampRadius(rect, radius);
            int d = r * 2;

            GraphicsPath path = new GraphicsPath();

            if (r <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();

            return path;
        }

        #endregion
    }
}
