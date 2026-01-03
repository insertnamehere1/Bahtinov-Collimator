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
        /// <summary>
        /// Radius in pixels for the rounded corners.
        /// </summary>
        public int CornerRadius { get; set; } = 12;

        /// <summary>
        /// Target width for the drawn image in pixels.
        /// The image is scaled to this width.
        /// </summary>
        public int ImageWidth { get; set; } = 32;

        /// <summary>
        /// Target height for the drawn image in pixels.
        /// The image is scaled to this height.
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
        /// Extra horizontal offset (in pixels) applied to the image position.
        /// Positive values move the image to the right.
        /// </summary>
        public int ImageOffsetX { get; set; } = 60;

        /// <summary>
        /// Extra vertical offset (in pixels) applied to the image position.
        /// Positive values move the image down.
        /// </summary>
        public int ImageOffsetY { get; set; } = 0;

        /// <summary>
        /// Extra horizontal offset (in pixels) applied to the text rectangle.
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

        /// <summary>
        /// True while the mouse is hovering over the button.
        /// </summary>
        private bool isHovered;

        /// <summary>
        /// True while the mouse is pressed down on the button.
        /// </summary>
        private bool isPressed;

        /// <summary>
        /// Initializes the rounded button, configures painting styles,
        /// and hooks mouse events for hover/press state.
        /// </summary>
        public RoundedButton()
        {
            // Remove default button border and use custom painting.
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;

            // Enable user painting and double buffering for smooth rendering.
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.DoubleBuffer, true);

            // Mouse hover state
            MouseEnter += (s, e) => { isHovered = true; Invalidate(); };
            MouseLeave += (s, e) => { isHovered = false; isPressed = false; Invalidate(); };

            // Mouse press state
            MouseDown += (s, e) => { isPressed = true; Invalidate(); };
            MouseUp += (s, e) => { isPressed = false; Invalidate(); };
        }

        /// <summary>
        /// Custom paint routine that draws the rounded background,
        /// hover/pressed overlay, bevel, and content (image + text).
        /// </summary>
        /// <param name="e">Paint event data, including graphics context.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            // Smooth edges for rounded shapes.
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Rectangle for the full button area, reduced by 1 pixel
            // so the border fits within the client rectangle.
            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);

            // Create a rounded path from the button rectangle.
            using (GraphicsPath path = GetRoundPath(rect, CornerRadius))
            {
                // Limit the clickable/hit region to the rounded shape.
                Region = new Region(path);

                // Fill the background with the button BackColor.
                using (SolidBrush sb = new SolidBrush(BackColor))
                    e.Graphics.FillPath(sb, path);

                // Draw overlay for pressed or hovered state.
                if (isPressed)
                {
                    // Darker overlay when pressed.
                    using (SolidBrush sb = new SolidBrush(PressedOverlay))
                        e.Graphics.FillPath(sb, path);
                }
                else if (isHovered)
                {
                    // Lighter overlay when hovered.
                    using (SolidBrush sb = new SolidBrush(HoverOverlay))
                        e.Graphics.FillPath(sb, path);
                }

                // Draw the bevel along the rounded border.
                DrawBevel(e.Graphics, rect, CornerRadius);

                // Draw image and text.
                DrawContent(e.Graphics, rect);
            }
        }

        /// <summary>
        /// Draws a bevel around the outside of the rounded button.
        /// The top and right edges use the light color, left and bottom use the dark color.
        /// </summary>
        /// <param name="g">Graphics surface to draw on.</param>
        /// <param name="rect">Outer rectangle of the button.</param>
        /// <param name="radius">Corner radius in pixels.</param>
        private void DrawBevel(Graphics g, Rectangle rect, int radius)
        {
            // t is the thickness of the bevel line.
            int t = BevelThickness;

            // d is the diameter of the corner arc (2 * radius).
            int d = radius * 2;

            // x and y are the top-left coordinates of the button rectangle.
            int x = rect.X;
            int y = rect.Y;

            // w and h are width and height of the rectangle.
            int w = rect.Width;
            int h = rect.Height;

            // right and bottom are the far edges of the rectangle.
            int right = x + w;
            int bottom = y + h;

            using (Pen lightPen = new Pen(BevelLight, t))
            using (Pen darkPen = new Pen(BevelDark, t))
            {
                // Top edge and top corners use the light color (highlight).
                g.DrawArc(lightPen, x, y, d, d, 180, 90);                      // top-left corner
                g.DrawLine(lightPen, x + radius, y, right - radius, y);       // top edge
                g.DrawArc(lightPen, right - d, y, d, d, 270, 90);             // top-right corner
                g.DrawLine(lightPen, right, y + radius, right, bottom - radius); // right edge

                // Bottom edge and bottom corners use the dark color (shadow).
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
        /// <param name="g">Graphics surface to draw on.</param>
        /// <param name="bounds">Full bounds of the button.</param>
        private void DrawContent(Graphics g, Rectangle bounds)
        {
            // Small pressed offset to simulate a "pushed in" look.
            int offset = isPressed ? 1 : 0;

            // contentRect is the area inside the bevel, slightly inset.
            Rectangle contentRect = Rectangle.Inflate(bounds, -4, -4);

            // 1) Draw image, positioned using ImageAlign and user offsets.
            if (Image != null)
            {
                // imageRect is the area where the image will be aligned
                // before applying user offsets and scaling.
                Rectangle imageRect = GetAlignedRectangle(
                    new Size(ImageWidth, ImageHeight),
                    contentRect,
                    ImageAlign
                );

                // Apply pressed offset.
                imageRect.Offset(offset, offset);

                // Apply user defined offsets.
                imageRect.Offset(ImageOffsetX, ImageOffsetY);

                // scaled is the final rectangle used when drawing the image.
                // It enforces the ImageWidth and ImageHeight.
                // Center the scaled image within the aligned rectangle if imageRect is larger.
                var scaled = new Rectangle(
                    imageRect.X + (imageRect.Width - ImageWidth) / 2,
                    imageRect.Y + (imageRect.Height - ImageHeight) / 2,
                    ImageWidth,
                    ImageHeight
                );

                // Draw the image scaled to the target size.
                g.DrawImage(Image, scaled);
            }

            // 2) Draw text.
            if (!string.IsNullOrEmpty(Text))
            {
                // Start with the content rectangle.
                Rectangle textRect = contentRect;

                // Apply pressed offset and additional horizontal text offset.
                textRect.Offset(offset + TextOffsetX, offset);

                // flags defines text alignment and ellipsis behavior.
                TextFormatFlags flags = TextFormatFlags.WordEllipsis;

                // Map the TextAlign property to TextFormatFlags.
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

                // Draw the text using GDI+ text rendering, with the chosen alignment.
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
        /// <param name="contentSize">Size of the content to align.</param>
        /// <param name="bounds">Outer rectangle within which to align.</param>
        /// <param name="align">Desired alignment.</param>
        /// <returns>A rectangle in which to draw the content.</returns>
        private Rectangle GetAlignedRectangle(Size contentSize, Rectangle bounds, ContentAlignment align)
        {
            // x and y are the top-left coordinates of the aligned content.
            int x;
            int y;

            // Horizontal alignment.
            if (align == ContentAlignment.TopLeft ||
                align == ContentAlignment.MiddleLeft ||
                align == ContentAlignment.BottomLeft)
            {
                // Align to the left.
                x = bounds.Left;
            }
            else if (align == ContentAlignment.TopCenter ||
                     align == ContentAlignment.MiddleCenter ||
                     align == ContentAlignment.BottomCenter)
            {
                // Center horizontally within bounds.
                x = bounds.Left + (bounds.Width - contentSize.Width) / 2;
            }
            else // Right alignment
            {
                // Align to the right edge.
                x = bounds.Right - contentSize.Width;
            }

            // Vertical alignment.
            if (align == ContentAlignment.TopLeft ||
                align == ContentAlignment.TopCenter ||
                align == ContentAlignment.TopRight)
            {
                // Align to the top.
                y = bounds.Top;
            }
            else if (align == ContentAlignment.MiddleLeft ||
                     align == ContentAlignment.MiddleCenter ||
                     align == ContentAlignment.MiddleRight)
            {
                // Center vertically within bounds.
                y = bounds.Top + (bounds.Height - contentSize.Height) / 2;
            }
            else // Bottom alignment
            {
                // Align to the bottom edge.
                y = bounds.Bottom - contentSize.Height;
            }

            return new Rectangle(new Point(x, y), contentSize);
        }

        /// <summary>
        /// Creates a GraphicsPath that represents a rounded rectangle.
        /// </summary>
        /// <param name="r">Rectangle to round.</param>
        /// <param name="radius">Corner radius in pixels.</param>
        /// <returns>GraphicsPath describing the rounded rectangle.</returns>
        private GraphicsPath GetRoundPath(Rectangle r, int radius)
        {
            // d is the diameter of the corner arcs.
            int d = radius * 2;

            // path holds the connected arcs and lines that form the rounded rectangle.
            GraphicsPath path = new GraphicsPath();

            // Top-left corner arc (from 180 to 270 degrees).
            path.AddArc(r.X, r.Y, d, d, 180, 90);

            // Top-right corner arc.
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);

            // Bottom-right corner arc.
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);

            // Bottom-left corner arc.
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);

            // Close the shape.
            path.CloseFigure();

            return path;
        }
    }
}
