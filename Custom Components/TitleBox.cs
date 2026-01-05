using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Bahtinov_Collimator.Custom_Components
{
    /// <summary>
    /// A lightweight header control that draws a rounded rectangle "pill" around its text.
    /// Intended for section titles to improve readability in dark-themed dialogs.
    /// </summary>
    public sealed class TitleBox : Control
    {
        private int _cornerRadius = 10;
        private Padding _innerPadding = new Padding(10, 6, 10, 6);

        /// <summary>
        /// Gets or sets the corner radius used for the rounded rectangle border.
        /// </summary>
        [DefaultValue(10)]
        public int CornerRadius
        {
            get => _cornerRadius;
            set
            {
                _cornerRadius = Math.Max(0, value);
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the padding applied between the box border and the title text.
        /// </summary>
        public Padding InnerPadding
        {
            get => _innerPadding;
            set
            {
                _innerPadding = value;
                Invalidate();
                PerformLayout();
            }
        }

        /// <summary>
        /// Gets or sets the border color used to draw the title box outline.
        /// </summary>
        public Color BorderColor { get; set; } = Color.White;

        /// <summary>
        /// Gets or sets the border width used to draw the title box outline.
        /// </summary>
        public float BorderWidth { get; set; } = 1f;

        /// <summary>
        /// Gets or sets the fill color used to paint the title box background.
        /// </summary>
        public Color FillColor { get; set; } = Color.FromArgb(35, 255, 255, 255);

        /// <summary>
        /// Initializes a new instance of the <see cref="TitleBox"/> control with sensible
        /// defaults for dark UI themes and reduced flicker.
        /// </summary>
        public TitleBox()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);

            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            ForeColor = Color.White;
            BackColor = UITheme.DarkBackground;
            TabStop = false;
        }

        /// <summary>
        /// Returns the preferred size for the control based on its text, font and padding.
        /// This is used so the title box can auto-size to fit the rendered text.
        /// </summary>
        /// <param name="proposedSize">The proposed size (ignored for the measurement logic).</param>
        /// <returns>The preferred size that fully contains the title text plus padding.</returns>
        public override Size GetPreferredSize(Size proposedSize)
        {
            using (var g = CreateGraphics())
            {
                var textSize = TextRenderer.MeasureText(
                    g,
                    Text ?? string.Empty,
                    Font,
                    new Size(int.MaxValue, int.MaxValue),
                    TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);

                return new Size(
                    _innerPadding.Left + textSize.Width + _innerPadding.Right,
                    _innerPadding.Top + textSize.Height + _innerPadding.Bottom);
            }
        }

        /// <summary>
        /// Paints the rounded rectangle box and the title text.
        /// </summary>
        /// <param name="e">Paint arguments containing the target graphics.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;

            using (var path = RoundedRect(rect, _cornerRadius))
            using (var fill = new SolidBrush(FillColor))
            using (var pen = new Pen(BorderColor, BorderWidth))
            {
                e.Graphics.FillPath(fill, path);
                e.Graphics.DrawPath(pen, path);
            }

            Rectangle textRect = new Rectangle(
                _innerPadding.Left,
                _innerPadding.Top,
                Width - _innerPadding.Left - _innerPadding.Right,
                Height - _innerPadding.Top - _innerPadding.Bottom);

            TextRenderer.DrawText(
                e.Graphics,
                Text ?? string.Empty,
                Font,
                textRect,
                ForeColor,
                TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter);
        }

        /// <summary>
        /// Creates a <see cref="GraphicsPath"/> describing a rounded rectangle suitable for
        /// drawing or filling with GDI+.
        /// </summary>
        /// <param name="bounds">The rectangle bounds.</param>
        /// <param name="radius">The corner radius in pixels.</param>
        /// <returns>A rounded-rectangle graphics path.</returns>
        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int r = Math.Max(1, radius);
            int d = r * 2;

            var path = new GraphicsPath();
            path.StartFigure();

            path.AddArc(bounds.Left, bounds.Top, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Top, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - d, d, d, 90, 90);

            path.CloseFigure();
            return path;
        }
    }
}
