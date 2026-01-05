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
    ///
    /// Notes:
    /// - This is the reliable approach for .NET Framework 4.8.1 because RichTextBox is a native control
    ///   and does not support custom painting of rounded borders directly.
    /// - This version is hardened against designer-time/property-change ordering issues
    ///   (e.g., ForeColor/BackColor changes firing before child controls exist).
    /// </summary>
    public sealed class TitledRoundedRichTextBox : UserControl
    {
        private RichTextBox _richTextBox;

        private int _cornerRadius = 12;
        private int _borderThickness = 1;
        private Color _borderColor = Color.Gray;

        private bool _showTitleBar = true;
        private string _titleText = "Summary";
        private int _titleHeight = 30;
        private int _titlePaddingLeft = 10;
        private bool _showTitleSeparator = true;

        private Color _titleBackColor = Color.FromArgb(245, 245, 245);
        private Color _titleForeColor = Color.Black;
        private Font _titleFont;

        #region Win32 (Caret suppression)

        [DllImport("user32.dll")]
        private static extern bool HideCaret(IntPtr hWnd);

        #endregion

        /// <summary>
        /// True if the inner RichTextBox has been created and is safe to access.
        /// </summary>
        private bool IsInnerReady
        {
            get { return _richTextBox != null && !_richTextBox.IsDisposed; }
        }

        /// <summary>
        /// Gets the underlying RichTextBox instance for advanced configuration.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public RichTextBox InnerRichTextBox
        {
            get { return _richTextBox; }
        }

        /// <summary>
        /// Gets or sets the content text of the RichTextBox.
        /// </summary>
        public override string Text
        {
            get { return IsInnerReady ? _richTextBox.Text : base.Text; }
            set
            {
                if (IsInnerReady)
                    _richTextBox.Text = value;
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
            get { return IsInnerReady && _richTextBox.ReadOnly; }
            set
            {
                if (!IsInnerReady)
                    return;

                _richTextBox.ReadOnly = value;
                SuppressCaretIfNeeded();
            }
        }

        /// <summary>
        /// Radius of the rounded corners in pixels.
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(12)]
        public int CornerRadius
        {
            get { return _cornerRadius; }
            set
            {
                _cornerRadius = Math.Max(0, value);
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
            get { return _borderThickness; }
            set
            {
                _borderThickness = Math.Max(1, value);
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
            get { return _borderColor; }
            set
            {
                _borderColor = value;
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
            get { return _showTitleBar; }
            set
            {
                _showTitleBar = value;
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
            get { return _titleText; }
            set
            {
                _titleText = value ?? string.Empty;
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
            get { return _titleHeight; }
            set
            {
                _titleHeight = Math.Max(18, value);
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
            get { return _titlePaddingLeft; }
            set
            {
                _titlePaddingLeft = Math.Max(0, value);
                Invalidate();
            }
        }

        /// <summary>
        /// Background color of the title bar.
        /// </summary>
        [Category("Title Bar")]
        public Color TitleBackColor
        {
            get { return _titleBackColor; }
            set
            {
                _titleBackColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Foreground color of the title text.
        /// </summary>
        [Category("Title Bar")]
        public Color TitleForeColor
        {
            get { return _titleForeColor; }
            set
            {
                _titleForeColor = value;
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
            get { return _titleFont; }
            set
            {
                _titleFont = value;
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
            get { return _showTitleSeparator; }
            set
            {
                _showTitleSeparator = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Initializes a new titled rounded RichTextBox control.
        /// </summary>
        public TitledRoundedRichTextBox()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);

            AutoScaleMode = AutoScaleMode.Dpi;

            // Defaults
            base.BackColor = Color.White;
            base.ForeColor = Color.Black;
            Padding = new Padding(10);

            CreateInnerControl();

            // Apply initial layout/region
            UpdateLayoutSafe();
            UpdateRegionSafe();
        }

        /// <summary>
        /// Creates and wires the inner RichTextBox.
        /// </summary>
        private void CreateInnerControl()
        {
            _richTextBox = new RichTextBox
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

            // Caret suppression hooks
            _richTextBox.GotFocus += (s, e) => SuppressCaretIfNeeded();
            _richTextBox.MouseDown += (s, e) => SuppressCaretIfNeeded();
            _richTextBox.TextChanged += (s, e) => SuppressCaretIfNeeded();
            _richTextBox.SelectionChanged += (s, e) => SuppressCaretIfNeeded();

            Controls.Add(_richTextBox);
        }

        /// <summary>
        /// Updates the child layout so the RichTextBox sits under the title bar.
        /// Safe to call during designer-time and early initialization.
        /// </summary>
        private void UpdateLayoutSafe()
        {
            if (!IsInnerReady)
                return;

            int inset = _borderThickness;
            int topBand = _showTitleBar ? _titleHeight : 0;

            int contentX = inset + Padding.Left;
            int contentY = inset + Padding.Top + topBand;

            int contentW = Math.Max(0, Width - (inset * 2) - Padding.Left - Padding.Right);
            int contentH = Math.Max(0, Height - (inset * 2) - Padding.Top - Padding.Bottom - topBand);

            _richTextBox.Location = new Point(contentX, contentY);
            _richTextBox.Size = new Size(contentW, contentH);

            _richTextBox.BackColor = BackColor;
            _richTextBox.ForeColor = ForeColor;
        }

        /// <summary>
        /// Applies a rounded region to the control so corners clip correctly.
        /// Safe for tiny sizes and during designer initialization.
        /// </summary>
        private void UpdateRegionSafe()
        {
            if (Width < 2 || Height < 2)
                return;

            using (var path = CreateRoundedRectPath(new Rectangle(0, 0, Width, Height), _cornerRadius))
            {
                // Assigning a new Region disposes the old one automatically in WinForms.
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

            if (_richTextBox.ReadOnly && _richTextBox.IsHandleCreated)
            {
                HideCaret(_richTextBox.Handle);
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
                _richTextBox.BackColor = BackColor;

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
                _richTextBox.ForeColor = ForeColor;

            Invalidate();
        }

        /// <summary>
        /// Paints the rounded border and (optionally) the title bar inside the border.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Outer border bounds (pen is inset aligned)
            Rectangle outer = new Rectangle(0, 0, Width - 1, Height - 1);

            // Fill bounds inside the border
            Rectangle innerFill = new Rectangle(
                _borderThickness,
                _borderThickness,
                Math.Max(0, Width - 1 - (_borderThickness * 2)),
                Math.Max(0, Height - 1 - (_borderThickness * 2)));

            int innerRadius = Math.Max(0, _cornerRadius - _borderThickness);

            // Background fill
            using (var fillPath = CreateRoundedRectPath(innerFill, innerRadius))
            using (var backBrush = new SolidBrush(BackColor))
            {
                e.Graphics.FillPath(backBrush, fillPath);
            }

            // Title bar fill (rounded only at top corners)
            if (_showTitleBar && _titleHeight > 0)
            {
                Rectangle titleRect = new Rectangle(
                    _borderThickness,
                    _borderThickness,
                    Math.Max(0, Width - (_borderThickness * 2)),
                    Math.Max(0, _titleHeight));

                using (var titlePath = CreateTopRoundedRectPath(titleRect, innerRadius))
                using (var titleBrush = new SolidBrush(_titleBackColor))
                {
                    e.Graphics.FillPath(titleBrush, titlePath);
                }

                if (_showTitleSeparator)
                {
                    int y = _borderThickness + _titleHeight;
                    using (var sepPen = new Pen(_borderColor, 1f))
                    {
                        e.Graphics.DrawLine(
                            sepPen,
                            _borderThickness,
                            y,
                            Width - _borderThickness - 1,
                            y);
                    }
                }

                // Title text
                using (var textBrush = new SolidBrush(_titleForeColor))
                using (var fmt = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center })
                {
                    Font tf = _titleFont ?? new Font(Font, FontStyle.Bold);

                    Rectangle textRect = new Rectangle(
                        _borderThickness + _titlePaddingLeft,
                        _borderThickness,
                        Math.Max(0, Width - (_borderThickness * 2) - _titlePaddingLeft - 6),
                        _titleHeight);

                    e.Graphics.DrawString(_titleText ?? string.Empty, tf, textBrush, textRect, fmt);

                    if (_titleFont == null)
                        tf.Dispose();
                }
            }

            // Border
            using (var borderPath = CreateRoundedRectPath(outer, _cornerRadius))
            using (var pen = new Pen(_borderColor, _borderThickness))
            {
                pen.Alignment = PenAlignment.Inset;
                e.Graphics.DrawPath(pen, borderPath);
            }
        }

        /// <summary>
        /// Creates a rounded rectangle path for all corners.
        /// </summary>
        private static GraphicsPath CreateRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();

            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            int d = radius * 2;

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
            var path = new GraphicsPath();

            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            int d = radius * 2;

            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);

            // Square bottom
            path.AddLine(rect.Right, rect.Y + radius, rect.Right, rect.Bottom);
            path.AddLine(rect.Right, rect.Bottom, rect.X, rect.Bottom);
            path.AddLine(rect.X, rect.Bottom, rect.X, rect.Y + radius);

            path.CloseFigure();
            return path;
        }
    }
}
