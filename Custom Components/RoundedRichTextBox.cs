using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SkyCal.Custom_Components
{
    /// <summary>
    /// A composite WinForms control that hosts a RichTextBox inside a rounded container
    /// with an optional title bar drawn inside the border.
    /// The outer stroke and inner fill use the same geometry as
    /// <see cref="Bahtinov_Collimator.Custom_Components.RoundedPictureBox"/> (inner radius =
    /// <c>CornerRadius − BorderThickness</c>; default border is 1 px).
    ///
    /// Notes:
    /// - This is the reliable approach for .NET Framework 4.8.1 because RichTextBox is a native control
    ///   and does not support custom painting of rounded borders directly.
    /// - This version is hardened against designer-time/property-change ordering issues
    ///   (e.g., ForeColor/BackColor changes firing before child controls exist).
    /// </summary>
    public sealed partial class TitledRoundedRichTextBox : UserControl
    {
        private RichTextBox richTextBox;

        private int cornerRadius = 12;
        private int borderThickness = 1;
        private Color borderColor = Color.Gray;

        private bool showTitleBar = true;
        private string titleText = "Summary";
        private int titleHeight = 30;
        private int titlePaddingLeft = 10;
        private bool showTitleSeparator = true;

        private Color titleBackColor = Color.FromArgb(245, 245, 245);
        private Color titleForeColor = Color.Black;
        private Font titleFont;

        #region Win32 (Caret suppression)

        [DllImport("user32.dll")]
        private static extern bool HideCaret(IntPtr hWnd);

        #endregion

        /// <summary>
        /// True if the inner RichTextBox has been created and is safe to access.
        /// </summary>
        private bool IsInnerReady
        {
            get { return richTextBox != null && !richTextBox.IsDisposed; }
        }

        /// <summary>
        /// Gets the underlying RichTextBox instance for advanced configuration.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public RichTextBox InnerRichTextBox
        {
            get { return richTextBox; }
        }

        /// <summary>
        /// Gets or sets the content text of the RichTextBox.
        /// </summary>
        public override string Text
        {
            get { return IsInnerReady ? richTextBox.Text : base.Text; }
            set
            {
                if (IsInnerReady)
                    richTextBox.Text = value;
                else
                    base.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the RichTextBox is read-only.
        /// When true, the caret is suppressed to avoid a flashing cursor.
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(false)]
        public bool ReadOnly
        {
            get { return IsInnerReady && richTextBox.ReadOnly; }
            set
            {
                if (!IsInnerReady)
                    return;

                richTextBox.ReadOnly = value;
                SuppressCaretIfNeeded();
            }
        }

        /// <summary>
        /// Radius of the rounded corners in pixels.
        /// </summary>
        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(12)]
        public int CornerRadius
        {
            get { return cornerRadius; }
            set
            {
                cornerRadius = Math.Max(0, value);
                UpdateRegionSafe();
                Invalidate();
            }
        }

        /// <summary>
        /// Thickness of the border in pixels.
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(1)]
        public int BorderThickness
        {
            get { return borderThickness; }
            set
            {
                borderThickness = Math.Max(1, value);
                UpdateLayoutSafe();
                Invalidate();
            }
        }

        /// <summary>
        /// Color of the rounded border.
        /// </summary>
        [Category("Appearance")]
        public Color BorderColor
        {
            get { return borderColor; }
            set
            {
                borderColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Shows or hides the title bar.
        /// </summary>
        [Category("Title Bar")]
        [DefaultValue(true)]
        public bool ShowTitleBar
        {
            get { return showTitleBar; }
            set
            {
                showTitleBar = value;
                UpdateLayoutSafe();
                Invalidate();
            }
        }

        /// <summary>
        /// Title bar text displayed at the top.
        /// </summary>
        [Category("Title Bar")]
        public string TitleText
        {
            get { return titleText; }
            set
            {
                titleText = value ?? string.Empty;
                Invalidate();
            }
        }

        /// <summary>
        /// Height of the title bar in pixels.
        /// </summary>
        [Category("Title Bar")]
        [DefaultValue(30)]
        public int TitleHeight
        {
            get { return titleHeight; }
            set
            {
                titleHeight = Math.Max(18, value);
                UpdateLayoutSafe();
                Invalidate();
            }
        }

        /// <summary>
        /// Left padding for the title text.
        /// </summary>
        [Category("Title Bar")]
        [DefaultValue(10)]
        public int TitlePaddingLeft
        {
            get { return titlePaddingLeft; }
            set
            {
                titlePaddingLeft = Math.Max(0, value);
                Invalidate();
            }
        }

        /// <summary>
        /// Background color of the title bar.
        /// </summary>
        [Category("Title Bar")]
        public Color TitleBackColor
        {
            get { return titleBackColor; }
            set
            {
                titleBackColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Foreground color of the title text.
        /// </summary>
        [Category("Title Bar")]
        public Color TitleForeColor
        {
            get { return titleForeColor; }
            set
            {
                titleForeColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Font used to draw the title text. If null, a bold version of the control Font is used.
        /// </summary>
        [Category("Title Bar")]
        [DefaultValue(null)]
        public Font TitleFont
        {
            get { return titleFont; }
            set
            {
                titleFont = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Shows or hides the separator line between the title bar and content.
        /// </summary>
        [Category("Title Bar")]
        [DefaultValue(true)]
        public bool ShowTitleSeparator
        {
            get { return showTitleSeparator; }
            set
            {
                showTitleSeparator = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Initializes a new titled rounded RichTextBox control.
        /// </summary>
        public TitledRoundedRichTextBox()
        {

            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
            true);

            InitializeComponent();

            DoubleBuffered = true;

            CreateInnerControl();

            UpdateLayoutSafe();
            UpdateRegionSafe();
        }

        /// <summary>
        /// Creates and wires the inner RichTextBox.
        /// </summary>
        private void CreateInnerControl()
        {
            richTextBox = new RichTextBox
            {
                BorderStyle = BorderStyle.None,
                Multiline = true,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                DetectUrls = false,
                HideSelection = true,
                BackColor = BackColor,
                ForeColor = ForeColor,
                Text = base.Text ?? string.Empty
            };

            richTextBox.GotFocus += (s, e) => SuppressCaretIfNeeded();
            richTextBox.MouseDown += (s, e) => SuppressCaretIfNeeded();
            richTextBox.TextChanged += (s, e) => SuppressCaretIfNeeded();
            richTextBox.SelectionChanged += (s, e) => SuppressCaretIfNeeded();

            Controls.Add(richTextBox);
        }

        /// <summary>
        /// Updates the child layout so the RichTextBox sits under the title bar.
        /// Safe to call during designer-time and early initialization.
        /// </summary>
        private void UpdateLayoutSafe()
        {
            if (!IsInnerReady)
                return;

            int inset = borderThickness;
            int topBand = showTitleBar ? titleHeight : 0;

            int contentX = inset + Padding.Left;
            int contentY = inset + Padding.Top + topBand;

            int contentW = Math.Max(0, Width - (inset * 2) - Padding.Left - Padding.Right);
            int contentH = Math.Max(0, Height - (inset * 2) - Padding.Top - Padding.Bottom - topBand);

            richTextBox.Location = new Point(contentX, contentY);
            richTextBox.Size = new Size(contentW, contentH);

            richTextBox.BackColor = BackColor;
            richTextBox.ForeColor = ForeColor;
        }

        /// <summary>
        /// Applies a rounded region to the control so corners clip correctly.
        /// Safe for tiny sizes and during designer initialization.
        /// </summary>
        private void UpdateRegionSafe()
        {
            if (Width < 2 || Height < 2)
                return;

            using (var path = CreateRoundedRectPath(new Rectangle(0, 0, Width, Height), cornerRadius))
            {
                Region = new Region(path);
            }
        }

        /// <summary>
        /// Hides the caret when the RichTextBox is read-only to prevent a flashing cursor.
        /// </summary>
        private void SuppressCaretIfNeeded()
        {
            if (!IsInnerReady)
                return;

            if (richTextBox.ReadOnly && richTextBox.IsHandleCreated)
            {
                HideCaret(richTextBox.Handle);
            }
        }

        /// <summary>
        /// Raised when the control is resized. Updates layout and rounded region.
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateLayoutSafe();
            UpdateRegionSafe();
        }

        /// <summary>
        /// Raised when the padding changes. Updates layout.
        /// </summary>
        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);
            UpdateLayoutSafe();
            Invalidate();
        }

        /// <summary>
        /// Raised when the BackColor changes. Syncs inner RichTextBox color.
        /// Guarded for designer-time ordering.
        /// </summary>
        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);

            if (IsInnerReady)
                richTextBox.BackColor = BackColor;

            Invalidate();
        }

        /// <summary>
        /// Raised when the ForeColor changes. Syncs inner RichTextBox color.
        /// Guarded for designer-time ordering.
        /// </summary>
        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);

            if (IsInnerReady)
                richTextBox.ForeColor = ForeColor;

            Invalidate();
        }

        /// <summary>
        /// Paints the rounded border and (optionally) the title bar inside the border.
        /// Stroke/fill geometry matches <see cref="RoundedPanel"/> / <see cref="RoundedPictureBox"/>:
        /// stroke path inset by half pen width so <see cref="PenAlignment.Center"/> is not clipped on edges.
        /// Window <see cref="Region"/> is cleared for the vector pass then restored in <c>finally</c>
        /// (same pattern as <see cref="Bahtinov_Collimator.Custom_Components.RoundedPanel"/>) so HRGN clipping
        /// does not destroy anti-aliasing on curved corners.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            int cw = Width;
            int ch = Height;
            if (cw <= 0 || ch <= 0)
            {
                Region = null;
                return;
            }

            Region = null;

            try
            {
                int t = borderThickness;
                int strokeInset = t / 2;

                Rectangle strokeBounds = new Rectangle(
                    strokeInset,
                    strokeInset,
                    Math.Max(0, cw - t),
                    Math.Max(0, ch - t));

                Rectangle innerFill = new Rectangle(
                    t,
                    t,
                    Math.Max(0, cw - 2 * t),
                    Math.Max(0, ch - 2 * t));
                int innerRadius = Math.Max(0, cornerRadius - t);

                using (var fillPath = CreateRoundedRectPath(innerFill, innerRadius))
                using (var backBrush = new SolidBrush(BackColor))
                {
                    g.FillPath(backBrush, fillPath);
                }

                if (showTitleBar && titleHeight > 0)
                {
                    Rectangle titleRect = new Rectangle(
                        t,
                        t,
                        Math.Max(0, cw - 2 * t),
                        Math.Max(0, titleHeight));

                    using (var titlePath = CreateTopRoundedRectPath(titleRect, innerRadius))
                    using (var titleBrush = new SolidBrush(titleBackColor))
                    {
                        g.FillPath(titleBrush, titlePath);
                    }

                    if (showTitleSeparator)
                    {
                        int y = t + titleHeight;
                        using (var sepPen = new Pen(borderColor, 1f))
                        {
                            g.DrawLine(sepPen, t, y, cw - t - 1, y);
                        }
                    }

                    using (var textBrush = new SolidBrush(titleForeColor))
                    using (var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    {
                        Font tf = titleFont ?? new Font(Font, FontStyle.Bold);

                        Rectangle textRect = new Rectangle(
                            t,
                            t,
                            Math.Max(0, cw - 2 * t),
                            titleHeight);

                        g.DrawString(titleText ?? string.Empty, tf, textBrush, textRect, fmt);

                        if (titleFont == null)
                            tf.Dispose();
                    }
                }

                using (var borderPath = CreateRoundedRectPath(strokeBounds, cornerRadius))
                using (var pen = new Pen(borderColor, t))
                {
                    pen.Alignment = PenAlignment.Center;
                    g.DrawPath(pen, borderPath);
                }
            }
            finally
            {
                using (GraphicsPath regionPath = CreateRoundedRectPath(new Rectangle(0, 0, cw, ch), cornerRadius))
                    Region = new Region(regionPath);
            }
        }

        /// <summary>
        /// Limits radius so quarter-arcs fit the rectangle.
        /// </summary>
        private static int ClampRadius(Rectangle bounds, int radius)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return 0;
            int max = Math.Min(bounds.Width, bounds.Height) / 2;
            return Math.Max(0, Math.Min(radius, max));
        }

        /// <summary>
        /// Creates a rounded rectangle path for all corners.
        /// </summary>
        private static GraphicsPath CreateRoundedRectPath(Rectangle rect, int radius)
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

        /// <summary>
        /// Creates a path rounded only on the top-left and top-right corners.
        /// Bottom corners remain square.
        /// </summary>
        private static GraphicsPath CreateTopRoundedRectPath(Rectangle rect, int radius)
        {
            int r = ClampRadius(rect, radius);
            var path = new GraphicsPath();

            if (r <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            int d = r * 2;

            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);

            path.AddLine(rect.Right, rect.Y + r, rect.Right, rect.Bottom);
            path.AddLine(rect.Right, rect.Bottom, rect.X, rect.Bottom);
            path.AddLine(rect.X, rect.Bottom, rect.X, rect.Y + r);

            path.CloseFigure();
            return path;
        }
    }
}
