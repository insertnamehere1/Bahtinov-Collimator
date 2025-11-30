using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Bahtinov_Collimator.Custom_Components
{
    /// <summary>
    /// A PictureBox with rounded corners, optional border, and smooth clipping.
    /// The region of the control is updated to match the rounded shape.
    /// </summary>
    public partial class RoundedPictureBox : PictureBox
    {
        // Backing fields for properties.
        private int cornerRadius = 12;
        private Color borderColor = Color.Gray;
        private int borderThickness = 2;

        /// <summary>
        /// Gets or sets the radius used for rounding the four corners.
        /// </summary>
        [Category("Appearance")]
        public int CornerRadius
        {
            get => cornerRadius;
            set
            {
                // Prevent negative values, update hit/clipping region.
                cornerRadius = Math.Max(0, value);
                UpdateRegion();
            }
        }

        /// <summary>
        /// Gets or sets the color of the rounded border.
        /// </summary>
        [Category("Appearance")]
        public Color BorderColor
        {
            get => borderColor;
            set { borderColor = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the thickness of the border in pixels.
        /// </summary>
        [Category("Appearance")]
        public int BorderThickness
        {
            get => borderThickness;
            set
            {
                // Prevent invalid 0 or negative thickness.
                borderThickness = Math.Max(1, value);
                Invalidate();
            }
        }

        /// <summary>
        /// Constructs the rounded picture box and configures double buffering,
        /// transparency, and resizing behavior.
        /// </summary>
        public RoundedPictureBox()
        {
            // Transparent background so parent visuals show through.
            BackColor = Color.Transparent;

            // Default scaling for images.
            SizeMode = PictureBoxSizeMode.Zoom;

            // Enable custom painting, double buffering, and no flicker.
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer,
                     true);

            // When the control is resized, update its rounded shape.
            Resize += (s, e) => UpdateRegion();
        }

        /// <summary>
        /// Recalculates the rounded clipping region based on the current size
        /// and corner radius.
        /// </summary>
        private void UpdateRegion()
        {
            // Avoid errors for invalid sizes.
            if (Width <= 0 || Height <= 0) return;

            // rect defines the full bounds of the picture box.
            using (GraphicsPath path = RoundedRect(new Rectangle(0, 0, Width, Height), cornerRadius))
            {
                // Apply as the control's hit-test region.
                Region = new Region(path);
            }

            // Repaint the control after region change.
            Invalidate();
        }

        /// <summary>
        /// Creates a GraphicsPath describing a rectangle with rounded corners.
        /// </summary>
        /// <param name="rect">Rectangle to round.</param>
        /// <param name="radius">Corner radius.</param>
        /// <returns>A rounded GraphicsPath.</returns>
        private GraphicsPath RoundedRect(Rectangle rect, int radius)
        {
            // Diameter of the arc corners.
            int d = radius * 2;

            GraphicsPath path = new GraphicsPath();

            path.StartFigure();

            // Add arcs for each corner in clockwise order.
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);                    // Top-left
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);            // Top-right
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);     // Bottom-right
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);            // Bottom-left

            // Close the figure to form a complete rounded frame.
            path.CloseFigure();

            return path;
        }

        /// <summary>
        /// Custom painting method to handle rounded clipping on the image
        /// and properly draw the border.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            // Enable anti-aliasing for smoother corners.
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // ---------------------------------------------
            // 1. Clip IMAGE to the rounded region.
            //    Only the image is clipped, NOT the border.
            // ---------------------------------------------
            using (GraphicsPath clipPath = RoundedRect(new Rectangle(0, 0, Width, Height), cornerRadius))
            using (Region clipRegion = new Region(clipPath))
            {
                // Replace the existing clipping region with the rounded one.
                e.Graphics.SetClip(clipRegion, CombineMode.Replace);

                // Draw the PictureBox normally inside the clip.
                base.OnPaint(e);
            }

            // ---------------------------------------------
            // 2. Reset clipping so border draws outside image region.
            // ---------------------------------------------
            e.Graphics.ResetClip();

            // ---------------------------------------------
            // 3. Draw border
            // ---------------------------------------------
            // borderRect shrinks the border so it fits inside the control bounds.
            Rectangle borderRect = new Rectangle(
                borderThickness / 2,
                borderThickness / 2,
                Width - borderThickness,
                Height - borderThickness
            );

            // Create rounded path for the border outline.
            using (GraphicsPath borderPath = RoundedRect(borderRect, cornerRadius))
            using (Pen pen = new Pen(borderColor, borderThickness))
            {
                e.Graphics.DrawPath(pen, borderPath);
            }
        }
    }
}
