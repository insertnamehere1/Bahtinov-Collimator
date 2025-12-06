using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Bahtinov_Collimator.Custom_Components
{
    /// <summary>
    /// A panel with rounded corners and optional border and fill.
    /// Supports transparent backgrounds by repainting the parent behind it.
    /// </summary>
    public partial class RoundedPanel : Panel
    {
        /// <summary>
        /// Radius in pixels for the rounded corners.
        /// </summary>
        public int CornerRadius { get; set; } = 8;

        /// <summary>
        /// Color of the outer border.
        /// </summary>
        public Color BorderColor { get; set; } = Color.Gray;

        /// <summary>
        /// Thickness of the border line in pixels.
        /// </summary>
        public int BorderThickness { get; set; } = 3;

        /// <summary>
        /// Background fill color inside the panel.
        /// Use Transparent to keep the control see-through.
        /// </summary>
        public Color FillColor { get; set; } = Color.Transparent;

        /// <summary>
        /// Enables custom painting, transparency, and double buffering.
        /// </summary>
        public RoundedPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.SupportsTransparentBackColor,
                     true);

            // Allow fully transparent backgrounds so the form shows behind.
            BackColor = Color.Transparent;
        }

        /// <summary>
        /// Paints the parent’s background *inside* this panel,
        /// allowing the panel to appear transparent even with rounded edges.
        /// </summary>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // If we have a parent, paint the parent's background into our surface.
            if (Parent != null)
            {
                // Save current graphics state so we can restore after translation.
                var state = e.Graphics.Save();

                // Translate drawing so the parent's background aligns correctly.
                e.Graphics.TranslateTransform(-Left, -Top);

                // Rectangle covering parent area—used for painting background.
                Rectangle parentRect = new Rectangle(
                    Parent.ClientRectangle.Location,
                    Parent.ClientRectangle.Size);

                // Create a temporary paint event for the parent.
                using (var pe = new PaintEventArgs(e.Graphics, parentRect))
                {
                    // Ask parent to paint its *background* into this panel.
                    this.InvokePaintBackground(Parent, pe);

                    // Ask parent to paint its *foreground* (controls etc.).
                    this.InvokePaint(Parent, pe);
                }

                // Restore original graphics transform.
                e.Graphics.Restore(state);
            }
            else
            {
                // Default fallback if no parent exists.
                base.OnPaintBackground(e);
            }
        }

        /// <summary>
        /// Draws the rounded border, optional fill, and sets the rounded clipping region.
        /// </summary>
        /// <param name="e">Paint event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            // Enable smooth curved edges.
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // rect defines the area inside the border thickness.
            Rectangle rect = new Rectangle(
                BorderThickness / 2,
                BorderThickness / 2,
                Width - BorderThickness,
                Height - BorderThickness
            );

            using (GraphicsPath path = GetRoundPath(rect, CornerRadius))
            using (Pen pen = new Pen(BorderColor, BorderThickness))
            {
                // Fill inside only if FillColor has opacity.
                if (FillColor.A > 0)
                {
                    using (SolidBrush fill = new SolidBrush(FillColor))
                        e.Graphics.FillPath(fill, path);
                }

                // Draw rounded border.
                e.Graphics.DrawPath(pen, path);

                // Set the visible hit test region to rounded shape.
                Region = new Region(path);
            }

            // Draw child controls, text, etc.
            base.OnPaint(e);
        }

        /// <summary>
        /// Produces a rounded rectangle path from a rectangle and a corner radius.
        /// </summary>
        /// <param name="r">Rectangle bounds.</param>
        /// <param name="radius">Corner radius in pixels.</param>
        /// <returns>A GraphicsPath representing the rounded rectangle.</returns>
        private GraphicsPath GetRoundPath(Rectangle r, int radius)
        {
            // Diameter of the arc corners.
            int d = radius * 2;

            // GraphicsPath to hold arcs and lines.
            GraphicsPath path = new GraphicsPath();

            // Add four corner arcs forming a rounded rectangle.
            path.AddArc(r.X, r.Y, d, d, 180, 90);                  // Top-left
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);          // Top-right
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);   // Bottom-right
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);          // Bottom-left

            // Close path to complete the shape.
            path.CloseFigure();

            return path;
        }
    }
}
