using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Bahtinov_Collimator.Custom_Components
{
    /// <summary>
    /// A custom button with rounded corners, bevel highlight/shadow,
    /// and support for custom sized/positioned images and text.
    /// </summary>
    public partial class RoundedButton : Button
    {
        #region Appearance Properties

        /// <summary>
        /// Radius in pixels for the rounded corners.
        /// </summary>
        public int CornerRadius { get; set; } = 12;

        /// <summary>
        /// Target width for the drawn image in 96-DPI logical pixels (same basis as <see cref="CornerRadius"/>).
        /// The image is scaled to this width after DPI scaling.
        /// </summary>
        public int ImageWidth { get; set; } = 32;

        /// <summary>
        /// Target height for the drawn image in 96-DPI logical pixels.
        /// The image is scaled to this height after DPI scaling.
        /// </summary>
        public int ImageHeight { get; set; } = 32;

        /// <summary>
        /// Semi-transparent overlay color when the mouse is over the button.
        /// </summary>
        public Color HoverOverlay { get; set; } = Color.FromArgb(40, Color.White);

        /// <summary>
        /// Semi-transparent overlay color when the button is pressed.
        /// </summary>
        public Color PressedOverlay { get; set; } = Color.FromArgb(60, Color.Black);

        /// <summary>
        /// Extra horizontal offset in 96-DPI logical pixels applied to the image position.
        /// Positive values move the image to the right.
        /// </summary>
        public int ImageOffsetX { get; set; } = 60;

        /// <summary>
        /// Extra vertical offset in 96-DPI logical pixels applied to the image position.
        /// Positive values move the image down.
        /// </summary>
        public int ImageOffsetY { get; set; } = 0;

        /// <summary>
        /// Extra horizontal offset in 96-DPI logical pixels applied to the text rectangle.
        /// Positive values move the text to the right.
        /// </summary>
        public int TextOffsetX { get; set; } = 10;

        /// <summary>
        /// Bevel highlight color that is drawn on the top and right edges.
        /// </summary>
        public Color BevelLight { get; set; } = Color.FromArgb(220, 255, 255, 255);

        /// <summary>
        /// Bevel shadow color that is drawn on the left and bottom edges.
        /// </summary>
        public Color BevelDark { get; set; } = Color.FromArgb(180, 0, 0, 0);

        /// <summary>
        /// Thickness of the bevel line in pixels.
        /// </summary>
        public int BevelThickness { get; set; } = 2;

        #endregion

        #region Interaction State

        /// <summary>
        /// True while the mouse is hovering over the button.
        /// </summary>
        private bool isHovered;

        /// <summary>
        /// True while the mouse is pressed down on the button.
        /// </summary>
        private bool isPressed;

        #endregion

        #region Lifecycle

        /// <summary>
        /// Initializes the rounded button, configures painting styles,
        /// and hooks mouse events for hover/press state.
        /// </summary>
        public RoundedButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.DoubleBuffer, true);

            MouseEnter += (s, e) => { isHovered = true; Invalidate(); };
            MouseLeave += (s, e) => { isHovered = false; isPressed = false; Invalidate(); };

            MouseDown += (s, e) => { isPressed = true; Invalidate(); };
            MouseUp += (s, e) => { isPressed = false; Invalidate(); };
        }

        #endregion

        #region Layout Helpers

        /// <summary>
        /// Converts 96-DPI design/logical pixel distances to device pixels for the current <see cref="Control.DeviceDpi"/>,
        /// matching <see cref="CornerRadius"/> handling so layout stays consistent when DPI or form scaling changes.
        /// </summary>
        private int S(int logicalPixels)
        {
            float dpiScale = DeviceDpi / 96f;
            return (int)Math.Round(logicalPixels * dpiScale, MidpointRounding.AwayFromZero);
        }

        #endregion

        #region Painting
        /// <summary>
        /// Custom paint routine that draws the rounded background,
        /// hover/pressed overlay, bevel, and content (image + text).
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);

            if (rect.Width <= 0 || rect.Height <= 0)
            {
                Region = null;
                return;
            }

            int scaledRadius = S(CornerRadius);
            // Prevent arcs larger than the rect; avoids odd geometry at small sizes.
            int maxRadius = Math.Max(0, Math.Min(rect.Width, rect.Height) / 2);
            scaledRadius = Math.Max(0, Math.Min(scaledRadius, maxRadius));

            Region = null;
            try
            {
                using (GraphicsPath path = GetRoundPath(rect, scaledRadius))
                {
                    using (SolidBrush sb = new SolidBrush(BackColor))
                        e.Graphics.FillPath(sb, path);

                    if (isPressed)
                    {
                        using (SolidBrush sb = new SolidBrush(PressedOverlay))
                            e.Graphics.FillPath(sb, path);
                    }
                    else if (isHovered)
                    {
                        using (SolidBrush sb = new SolidBrush(HoverOverlay))
                            e.Graphics.FillPath(sb, path);
                    }

                    DrawBevel(e.Graphics, rect, scaledRadius);
                    DrawContent(e.Graphics, rect);
                }
            }
            finally
            {
                using (GraphicsPath path = GetRoundPath(rect, scaledRadius))
                    Region = new Region(path);
            }
        }

        #endregion

        #region Drawing Helpers
        /// <summary>
        /// Draws a bevel around the outside of the rounded button.
        /// The top and right edges use the light color, left and bottom use the dark color.
        /// </summary>
        private void DrawBevel(Graphics g, Rectangle rect, int radius)
        {
            int t = BevelThickness;
            int d = radius * 2;
            int x = rect.X;
            int y = rect.Y;
            int w = rect.Width;
            int h = rect.Height;
            int right = x + w;
            int bottom = y + h;

            using (Pen lightPen = new Pen(BevelLight, t))
            using (Pen darkPen = new Pen(BevelDark, t))
            {
                g.DrawArc(lightPen, x, y, d, d, 180, 90);                      // top-left corner
                g.DrawLine(lightPen, x + radius, y, right - radius, y);       // top edge
                g.DrawArc(lightPen, right - d, y, d, d, 270, 90);             // top-right corner
                g.DrawLine(lightPen, right, y + radius, right, bottom - radius); // right edge

                g.DrawArc(darkPen, right - d, bottom - d, d, d, 0, 90);       // bottom-right corner
                g.DrawLine(darkPen, right - radius, bottom, x + radius, bottom); // bottom edge
                g.DrawArc(darkPen, x, bottom - d, d, d, 90, 90);              // bottom-left corner
                g.DrawLine(darkPen, x, bottom - radius, x, y + radius);       // left edge
            }
        }

        /// <summary>
        /// Draws the button content: image (if any) and text.
        /// Takes into account the pressed offset, alignment, and user offsets.
        /// </summary>
        private void DrawContent(Graphics g, Rectangle bounds)
        {
            int offset = isPressed ? 1 : 0;

            int contentInset = ScaleLogicalPixels(4);
            Rectangle contentRect = Rectangle.Inflate(bounds, -contentInset, -contentInset);

            int imgW = Math.Max(1, S(ImageWidth));
            int imgH = Math.Max(1, S(ImageHeight));
            int imgOffX = S(ImageOffsetX);
            int imgOffY = S(ImageOffsetY);
            int textOffX = S(TextOffsetX);

            if (Image != null)
            {
                Rectangle imageRect = GetAlignedRectangle(
                    new Size(imgW, imgH),
                    contentRect,
                    ImageAlign
                );

                imageRect.Offset(offset, offset);
                imageRect.Offset(imgOffX, imgOffY);
                var scaled = new Rectangle(
                    imageRect.X + (imageRect.Width - imgW) / 2,
                    imageRect.Y + (imageRect.Height - imgH) / 2,
                    imgW,
                    imgH
                );

                g.DrawImage(Image, scaled);
            }

            if (!string.IsNullOrEmpty(Text))
            {
                Rectangle textRect = contentRect;
                textRect.Offset(offset + textOffX, offset);
                TextFormatFlags flags = TextFormatFlags.WordEllipsis;
                switch (TextAlign)
                {
                    case ContentAlignment.TopLeft:
                        flags |= TextFormatFlags.Top | TextFormatFlags.Left;
                        break;
                    case ContentAlignment.TopCenter:
                        flags |= TextFormatFlags.Top | TextFormatFlags.HorizontalCenter;
                        break;
                    case ContentAlignment.TopRight:
                        flags |= TextFormatFlags.Top | TextFormatFlags.Right;
                        break;
                    case ContentAlignment.MiddleLeft:
                        flags |= TextFormatFlags.VerticalCenter | TextFormatFlags.Left;
                        break;
                    case ContentAlignment.MiddleCenter:
                        flags |= TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;
                        break;
                    case ContentAlignment.MiddleRight:
                        flags |= TextFormatFlags.VerticalCenter | TextFormatFlags.Right;
                        break;
                    case ContentAlignment.BottomLeft:
                        flags |= TextFormatFlags.Bottom | TextFormatFlags.Left;
                        break;
                    case ContentAlignment.BottomCenter:
                        flags |= TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter;
                        break;
                    case ContentAlignment.BottomRight:
                        flags |= TextFormatFlags.Bottom | TextFormatFlags.Right;
                        break;
                }

                TextRenderer.DrawText(
                    g,
                    Text,
                    Font,
                    textRect,
                    ForeColor,
                    flags
                );
            }
        }

        /// <summary>
        /// Calculates a rectangle of a given size positioned within bounds
        /// according to the specified ContentAlignment.
        /// </summary>
        /// <returns>A rectangle in which to draw the content.</returns>
        private Rectangle GetAlignedRectangle(Size contentSize, Rectangle bounds, ContentAlignment align)
        {
            int x;
            int y;
            if (align == ContentAlignment.TopLeft ||
                align == ContentAlignment.MiddleLeft ||
                align == ContentAlignment.BottomLeft)
            {
                x = bounds.Left;
            }
            else if (align == ContentAlignment.TopCenter ||
                     align == ContentAlignment.MiddleCenter ||
                     align == ContentAlignment.BottomCenter)
            {
                x = bounds.Left + (bounds.Width - contentSize.Width) / 2;
            }
            else // Right alignment
            {
                x = bounds.Right - contentSize.Width;
            }
            if (align == ContentAlignment.TopLeft ||
                align == ContentAlignment.TopCenter ||
                align == ContentAlignment.TopRight)
            {
                y = bounds.Top;
            }
            else if (align == ContentAlignment.MiddleLeft ||
                     align == ContentAlignment.MiddleCenter ||
                     align == ContentAlignment.MiddleRight)
            {
                y = bounds.Top + (bounds.Height - contentSize.Height) / 2;
            }
            else // Bottom alignment
            {
                y = bounds.Bottom - contentSize.Height;
            }

            return new Rectangle(new Point(x, y), contentSize);
        }

        /// <summary>
        /// Creates a GraphicsPath that represents a rounded rectangle.
        /// </summary>
        /// <returns>GraphicsPath describing the rounded rectangle.</returns>
        private GraphicsPath GetRoundPath(Rectangle r, int radius)
        {
            int d = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();

            return path;
        }

        #endregion
    }
}
