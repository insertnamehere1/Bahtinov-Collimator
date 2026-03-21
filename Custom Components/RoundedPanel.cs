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
            InitializeComponent();
            // Reduces stair-stepping on rounded strokes when combined with custom painting.
            DoubleBuffered = true;
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
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            int cw = Width;
            int ch = Height;
            if (cw <= 0 || ch <= 0)
            {
                Region = null;
                base.OnPaint(e);
                return;
            }

            // Clear window region for this paint. A Region from a GraphicsPath is pixel-snapped;
            // if left from the previous frame it clips the DC before FillPath/DrawPath and
            // destroys anti-aliasing (chunky corners). RoundedPictureBox avoids this by not using
            // Control.Region for painting. We restore the HRGN after drawing for child hit-testing.
            Region = null;

            try
            {
                int t = BorderThickness;
                int strokeInset = t / 2;

                // Stroke path inset so PenAlignment.Center is not clipped on top/left.
                Rectangle strokeBounds = new Rectangle(
                    strokeInset,
                    strokeInset,
                    Math.Max(0, cw - t),
                    Math.Max(0, ch - t));

                // Fill inside the border’s inner edge.
                Rectangle fillBounds = new Rectangle(
                    t,
                    t,
                    Math.Max(0, cw - 2 * t),
                    Math.Max(0, ch - 2 * t));
                int innerRadius = Math.Max(0, CornerRadius - t);

                using (GraphicsPath fillPath = GetRoundPath(fillBounds, innerRadius))
                {
                    if (FillColor.A > 0)
                    {
                        using (SolidBrush fill = new SolidBrush(FillColor))
                            g.FillPath(fill, fillPath);
                    }
                }

                using (GraphicsPath strokePath = GetRoundPath(strokeBounds, CornerRadius))
                using (Pen pen = new Pen(BorderColor, t))
                {
                    pen.Alignment = PenAlignment.Center;
                    g.DrawPath(pen, strokePath);
                }
            }
            finally
            {
                using (GraphicsPath regionPath = GetRoundPath(new Rectangle(0, 0, cw, ch), CornerRadius))
                    Region = new Region(regionPath);
            }

            base.OnPaint(e);
        }

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
        /// Produces a rounded rectangle path from a rectangle and a corner radius.
        /// </summary>
        private GraphicsPath GetRoundPath(Rectangle r, int radius)
        {
            int rad = ClampRadius(r, radius);
            int d = rad * 2;

            GraphicsPath path = new GraphicsPath();

            if (rad <= 0)
            {
                path.AddRectangle(r);
                return path;
            }

            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}
