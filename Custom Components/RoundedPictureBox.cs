using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;

namespace Bahtinov_Collimator.Custom_Components
{
    /// <summary>
    /// A PictureBox with rounded corners and optional border.
    /// Uses the same rounded-rectangle path as <see cref="RoundedGroupBox"/> (clamped radius + AddArc chain)
    /// so corners stay circular. The image is drawn with <see cref="TextureBrush"/> + <see cref="Graphics.FillPath"/>
    /// for smooth anti-aliased edges (unlike pixel-based <see cref="Region"/> clipping).
    /// <para>
    /// The image is filled using the <em>same</em> rounded rectangle as the border stroke (inset by half the
    /// pen width, then <see cref="CornerRadius"/> on that rect). The border is drawn on top, so the visible
    /// curve matches the <see cref="CornerRadius"/> you set — unlike filling a separate inner path with radius
    /// <c>CornerRadius − BorderThickness</c>, which always looked tighter than the border and than
    /// <see cref="RoundedPanel"/> / titled rich text borders for the same property values.
    /// </para>
    /// </summary>
    public partial class RoundedPictureBox : PictureBox
    {
        #region Reflection Keys

        /// <summary>
        /// Key used by <see cref="Control"/> for the <see cref="Control.Paint"/> event list
        /// (same reference as <c>Control.EventPaint</c>).
        /// </summary>
        private static readonly object EventPaintKey =
            typeof(Control).GetField("EventPaint", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null);

        #endregion

        #region Fields

        private int cornerRadius = 12;
        private Color borderColor = Color.Gray;
        private int borderThickness = 1;

        #endregion

        #region Public Properties

        [Category("Appearance")]
        [DefaultValue(12)]
        public int CornerRadius
        {
            get => cornerRadius;
            set
            {
                cornerRadius = Math.Max(0, value);
                Invalidate();
            }
        }

        [Category("Appearance")]
        public Color BorderColor
        {
            get => borderColor;
            set { borderColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DefaultValue(1)]
        public int BorderThickness
        {
            get => borderThickness;
            set
            {
                borderThickness = Math.Max(1, value);
                Invalidate();
            }
        }

        #endregion

        #region Lifecycle

        /// <summary>
        /// Initializes a new instance of the <see cref="RoundedPictureBox"/> class.
        /// </summary>
        public RoundedPictureBox()
        {
            // Enable custom painting, double buffering, and no flicker.
            this.SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true);

            InitializeComponent();
            Resize += (s, e) => Invalidate();
        }

        #endregion

        #region Geometry Helpers

        /// <summary>
        /// Maximum corner radius so each quarter-circle fits; larger values cause GDI+ to insert
        /// straight segments between arcs (flat spots / squared corners).
        /// </summary>
        private static int ClampRadius(Rectangle bounds, int radius)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return 0;
            int max = Math.Min(bounds.Width, bounds.Height) / 2;
            return Math.Max(0, Math.Min(radius, max));
        }

        /// <summary>
        /// Same construction as <see cref="RoundedGroupBox"/> GetRoundPath (no StartFigure — one continuous figure).
        /// </summary>
        private static GraphicsPath GetRoundPath(Rectangle rect, int radius)
        {
            int r = ClampRadius(rect, radius);
            int d = r * 2;

            var path = new GraphicsPath();

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

        #region Rendering Helpers

        /// <summary>
        /// Maps the image into <paramref name="fillBounds"/> (client coordinates) for the current <see cref="PictureBoxSizeMode"/>.
        /// </summary>
        private static void ConfigureTextureBrushForSizeMode(
            TextureBrush brush, Image image, PictureBoxSizeMode mode, Rectangle fillBounds)
        {
            int iw = image.Width;
            int ih = image.Height;
            if (iw <= 0 || ih <= 0)
            {
                brush.Transform = new Matrix();
                return;
            }

            int cw = fillBounds.Width;
            int ch = fillBounds.Height;
            float ox = fillBounds.X;
            float oy = fillBounds.Y;

            switch (mode)
            {
                case PictureBoxSizeMode.StretchImage:
                    brush.Transform = new Matrix(
                        (float)cw / iw, 0, 0, (float)ch / ih, ox, oy);
                    break;

                case PictureBoxSizeMode.Zoom:
                    {
                        float scale = Math.Min((float)cw / iw, (float)ch / ih);
                        float dw = iw * scale;
                        float dh = ih * scale;
                        float dx = (cw - dw) / 2f + ox;
                        float dy = (ch - dh) / 2f + oy;
                        brush.Transform = new Matrix(scale, 0, 0, scale, dx, dy);
                        break;
                    }

                case PictureBoxSizeMode.CenterImage:
                    {
                        float dx = (cw - iw) / 2f + ox;
                        float dy = (ch - ih) / 2f + oy;
                        brush.Transform = new Matrix(1, 0, 0, 1, dx, dy);
                        break;
                    }

                case PictureBoxSizeMode.AutoSize:
                case PictureBoxSizeMode.Normal:
                default:
                    brush.Transform = new Matrix(1, 0, 0, 1, ox, oy);
                    break;
            }
        }

        #endregion

        #region Painting

        /// <summary>
        /// Paints the rounded image fill and border, then raises subscribed paint handlers.
        /// </summary>
        /// <param name="e">Paint event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;

            // Use client size (same coordinate space as paint DC) for parity with other rounded controls.
            Rectangle client = ClientRectangle;
            int cw = client.Width;
            int ch = client.Height;
            if (cw <= 0 || ch <= 0)
                return;

            int t = borderThickness;
            // Stroke path: inset by half the pen width so PenAlignment.Center does not draw
            // outside the client rect (top/left were clipped → looked thinner than bottom/right).
            int strokeInset = t / 2;
            Rectangle strokeBounds = new Rectangle(
                strokeInset,
                strokeInset,
                Math.Max(0, cw - t),
                Math.Max(0, ch - t));

            // One path for image + border so the image follows the same arc as the stroke (full CornerRadius on
            // strokeBounds). A separate inner path (radius CornerRadius − t) always read as “smaller radius”
            // than RoundedPanel / TitledRoundedRichTextBox borders for the same property values.
            using (GraphicsPath path = GetRoundPath(strokeBounds, cornerRadius))
            {
                if (Image != null)
                {
                    using (TextureBrush brush = new TextureBrush(Image, WrapMode.Clamp))
                    {
                        ConfigureTextureBrushForSizeMode(brush, Image, SizeMode, strokeBounds);
                        g.FillPath(brush, path);
                    }
                }

                using (Pen pen = new Pen(borderColor, t))
                {
                    pen.Alignment = PenAlignment.Center;
                    g.DrawPath(pen, path);
                }
            }

            // Hosts such as ImageDisplayComponent subscribe to Paint and composite layer bitmaps there (they do
            // not use the Image property for live capture). Overriding OnPaint replaces PictureBox.OnPaint, so
            // we must raise the Paint event the same way Control.OnPaint would, or those handlers never run.
            if (EventPaintKey != null)
                RaisePaintEvent(EventPaintKey, e);
        }

        #endregion
    }
}
