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

        /// <summary>
        /// Initializes the rounded group box and enables double-buffering
        /// and redraw-on-resize for smooth rendering.
        /// </summary>
        public RoundedGroupBox()
        {
            // Reduce flicker when repainting.
            DoubleBuffered = true;

            // Force the control to redraw itself when resized.
            ResizeRedraw = true;
        }

        /// <summary>
        /// Custom paint routine for drawing the rounded background,
        /// border, and title text.
        /// </summary>
        /// <param name="e">Paint event data, including graphics context.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            // Smooth edges for rounded corners and curved lines.
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // textHeight is the height of the title text in pixels.
            float textHeight = e.Graphics.MeasureString(Text, Font).Height;

            // rect is the main rounded rectangle area for the group box.
            // It starts halfway down the text height so that the top border
            // passes behind the text.
            Rectangle rect = new Rectangle(
                0,
                (int)(textHeight / 2),
                Width - 1,
                Height - (int)(textHeight / 2) - 1
            );

            // Create the rounded path, border pen, and background brush.
            using (GraphicsPath path = GetRoundPath(rect, CornerRadius))
            using (Pen pen = new Pen(BorderColor, BorderThickness))
            using (SolidBrush brush = new SolidBrush(BackColor))
            {
                // Fill the background inside the rounded region.
                e.Graphics.FillPath(brush, path);

                // Draw the rounded border.
                e.Graphics.DrawPath(pen, path);

                // textSize is the full size of the group box caption text.
                SizeF textSize = e.Graphics.MeasureString(Text, Font);

                // textRect is the rectangle behind the text that we fill
                // with background color so the border line does not appear
                // through the text.
                RectangleF textRect = new RectangleF(
                    10,                     // Left padding for the text area.
                    0,                      // Top at the very top of the control.
                    textSize.Width + 2,     // Slightly wider than the text.
                    textSize.Height         // Height equal to measured text height.
                );

                // backBrush paints over the border behind the text.
                using (SolidBrush backBrush = new SolidBrush(BackColor))
                    e.Graphics.FillRectangle(backBrush, textRect);

                // Draw the group box caption text at the same horizontal offset (10)
                // used above, at the top of the control.
                using (SolidBrush textBrush = new SolidBrush(ForeColor))
                    e.Graphics.DrawString(Text, Font, textBrush, 10, 0);
            }
        }

        /// <summary>
        /// Builds a GraphicsPath that describes a rounded rectangle
        /// for the given bounds and corner radius.
        /// </summary>
        /// <param name="rect">Rectangle that defines the outer bounds.</param>
        /// <param name="radius">Corner radius in pixels.</param>
        /// <returns>A GraphicsPath describing the rounded rectangle.</returns>
        private GraphicsPath GetRoundPath(Rectangle rect, int radius)
        {
            // d is the diameter of each corner arc (2 * radius).
            int d = radius * 2;

            // path will hold the arcs and segments that form the rounded rectangle.
            GraphicsPath path = new GraphicsPath();

            // Top-left corner arc (180 to 270 degrees).
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);                        // top-left

            // Top-right corner arc (270 to 360 degrees).
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);                // top-right

            // Bottom-right corner arc (0 to 90 degrees).
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);         // bottom-right

            // Bottom-left corner arc (90 to 180 degrees).
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);                // bottom-left

            // Close the figure to join all arcs into a continuous rounded rectangle.
            path.CloseFigure();

            return path;
        }
    }
}
