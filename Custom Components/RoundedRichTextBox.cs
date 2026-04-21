using System;
using Bahtinov_Collimator;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Text;
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
        private Color borderColor = UITheme.BorderDefaultGray;

        private bool showTitleBar = true;
        private string titleText = "Summary";
        private int titleHeight = 30;
        private int titlePaddingLeft = 10;
        private bool showTitleSeparator = true;

        private Color titleBackColor = UITheme.RoundedRichTextTitleBack;
        private Color titleForeColor = UITheme.TextBoxForeground;
        private Font titleFont;

        // Progress bar (optional band drawn at the bottom, inside the rounded container).
        // All pixel sizes are logical values at 96 DPI; scaled via ScaleByDpi().
        private bool showProgressBar = false;
        private float progressValue = 0f;
        private int progressBarHeight = 14;
        private int progressBarSideMargin = 16;
        private int progressBarBottomMargin = 10;
        private int progressBarTopGap = 4;
        private int progressBarDivisions = 5;
        private Color progressBarFillColor = Color.FromArgb(72, 187, 92);
        private Color progressBarTrackColor = Color.FromArgb(30, 34, 40);
        private Color progressBarBorderColor = Color.FromArgb(150, 150, 150);
        private Color progressBarDivisionColor = Color.FromArgb(220, 220, 220);

        #region Win32 (Caret suppression + scrollbar chrome)

        [DllImport("user32.dll")]
        private static extern bool HideCaret(IntPtr hWnd);

        /// <summary>Applies dark-style scrollbar / edit chrome (Windows 10 2004+); no-op on failure.</summary>
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hwnd, string pszSubAppName, string pszSubIdList);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool EnumChildWindows(IntPtr hWndParent, EnumChildProc lpEnumFunc, IntPtr lParam);

        private delegate bool EnumChildProc(IntPtr hWnd, IntPtr lParam);

        /// <summary>Keep delegate alive for native callback lifetime.</summary>
        private static readonly EnumChildProc ScrollBarThemeEnumCallback = ThemeScrollBarChildrenCallback;

        private static bool ThemeScrollBarChildrenCallback(IntPtr hWnd, IntPtr lParam)
        {
            var sb = new StringBuilder(256);
            if (GetClassName(hWnd, sb, sb.Capacity) > 0 && sb.ToString() == "ScrollBar")
                SetWindowTheme(hWnd, "DarkMode_Explorer", null);
            return true;
        }

        /// <summary>
        /// RichEdit scrollbars are sometimes separate HWNDs; theme the control and enumerate children.
        /// Re-run after layout/text changes because the vertical scrollbar may be created later.
        /// </summary>
        private static void ApplyDarkScrollBarTheme(RichTextBox rtb)
        {
            if (rtb == null || !rtb.IsHandleCreated)
                return;

            IntPtr h = rtb.Handle;
            try
            {
                SetWindowTheme(h, "DarkMode_Explorer", null);
                EnumChildWindows(h, ScrollBarThemeEnumCallback, IntPtr.Zero);
            }
            catch
            {
                // Best-effort; OS may not support DarkMode_Explorer.
            }
        }

        #endregion

        #region DPI scaling
        //
        // The pixel-based properties on this control (TitleHeight, BorderThickness,
        // CornerRadius, TitlePaddingLeft) and TitleFont are interpreted as "logical"
        // values at 96 DPI. This control lives inside CalibrationComponent, which
        // sets ScaleChildren = false to keep its hand-rolled layout pass authoritative,
        // so WinForms never auto-scales these values when the form is dragged between
        // monitors or launched on a high-DPI display. We therefore apply the scale
        // factor ourselves at paint/layout time.

        private float DpiScale
        {
            get { return DeviceDpi / 96f; }
        }

        private int ScaleByDpi(int logicalPixels)
        {
            return (int)Math.Round(logicalPixels * DpiScale, MidpointRounding.AwayFromZero);
        }

        private int ScaledTitleHeight
        {
            get { return ScaleByDpi(titleHeight); }
        }

        private int ScaledBorderThickness
        {
            get { return Math.Max(1, ScaleByDpi(borderThickness)); }
        }

        private int ScaledCornerRadius
        {
            get { return ScaleByDpi(cornerRadius); }
        }

        private int ScaledProgressBarHeight
        {
            get { return ScaleByDpi(progressBarHeight); }
        }

        private int ScaledProgressBarSideMargin
        {
            get { return ScaleByDpi(progressBarSideMargin); }
        }

        private int ScaledProgressBarBottomMargin
        {
            get { return ScaleByDpi(progressBarBottomMargin); }
        }

        private int ScaledProgressBarTopGap
        {
            get { return ScaleByDpi(progressBarTopGap); }
        }

        /// <summary>
        /// Total vertical space reserved for the progress bar band (bar + gap above + margin below),
        /// measured in scaled device pixels. Zero when <see cref="ShowProgressBar"/> is false.
        /// </summary>
        private int ScaledProgressBarBandHeight
        {
            get
            {
                if (!showProgressBar)
                    return 0;
                return ScaledProgressBarHeight + ScaledProgressBarTopGap + ScaledProgressBarBottomMargin;
            }
        }
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
        /// Shows or hides the progress bar band at the bottom of the container.
        /// When visible, the inner RichTextBox shrinks vertically to make room.
        /// </summary>
        [Category("Progress Bar")]
        [DefaultValue(false)]
        public bool ShowProgressBar
        {
            get { return showProgressBar; }
            set
            {
                if (showProgressBar == value)
                    return;
                showProgressBar = value;
                UpdateLayoutSafe();
                Invalidate();
            }
        }

        /// <summary>
        /// Current progress value in the range [0, 1] (values outside are clamped).
        /// Drawn as a green fill left-to-right across the bar.
        /// </summary>
        [Category("Progress Bar")]
        [DefaultValue(0f)]
        public float ProgressValue
        {
            get { return progressValue; }
            set
            {
                float clamped = value;
                if (clamped < 0f) clamped = 0f;
                else if (clamped > 1f) clamped = 1f;

                if (Math.Abs(progressValue - clamped) < 0.0001f)
                    return;

                progressValue = clamped;
                if (showProgressBar)
                    Invalidate();
            }
        }

        /// <summary>
        /// Number of equal divisions drawn on the progress bar (tick segments).
        /// A value of 5 draws 4 internal dividers (0.2, 0.4, 0.6, 0.8 of the bar width).
        /// </summary>
        [Category("Progress Bar")]
        [DefaultValue(5)]
        public int ProgressBarDivisions
        {
            get { return progressBarDivisions; }
            set
            {
                progressBarDivisions = Math.Max(1, value);
                if (showProgressBar)
                    Invalidate();
            }
        }

        /// <summary>
        /// Height of the progress bar in logical pixels at 96 DPI.
        /// </summary>
        [Category("Progress Bar")]
        [DefaultValue(14)]
        public int ProgressBarHeight
        {
            get { return progressBarHeight; }
            set
            {
                progressBarHeight = Math.Max(4, value);
                UpdateLayoutSafe();
                Invalidate();
            }
        }

        /// <summary>
        /// Fill color of the progress bar.
        /// </summary>
        [Category("Progress Bar")]
        public Color ProgressBarFillColor
        {
            get { return progressBarFillColor; }
            set
            {
                progressBarFillColor = value;
                if (showProgressBar)
                    Invalidate();
            }
        }

        /// <summary>
        /// Track (empty) color of the progress bar.
        /// </summary>
        [Category("Progress Bar")]
        public Color ProgressBarTrackColor
        {
            get { return progressBarTrackColor; }
            set
            {
                progressBarTrackColor = value;
                if (showProgressBar)
                    Invalidate();
            }
        }

        /// <summary>
        /// Border color of the progress bar.
        /// </summary>
        [Category("Progress Bar")]
        public Color ProgressBarBorderColor
        {
            get { return progressBarBorderColor; }
            set
            {
                progressBarBorderColor = value;
                if (showProgressBar)
                    Invalidate();
            }
        }

        /// <summary>
        /// Color of the internal division tick lines drawn across the bar.
        /// </summary>
        [Category("Progress Bar")]
        public Color ProgressBarDivisionColor
        {
            get { return progressBarDivisionColor; }
            set
            {
                progressBarDivisionColor = value;
                if (showProgressBar)
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
            richTextBox.TextChanged += (s, e) =>
            {
                SuppressCaretIfNeeded();
                if (!richTextBox.IsDisposed)
                    ApplyDarkScrollBarTheme(richTextBox);
            };
            richTextBox.SelectionChanged += (s, e) => SuppressCaretIfNeeded();
            richTextBox.SizeChanged += (s, e) =>
            {
                if (!richTextBox.IsDisposed)
                    ApplyDarkScrollBarTheme(richTextBox);
            };
            richTextBox.HandleCreated += (s, e) =>
            {
                if (!richTextBox.IsHandleCreated)
                    return;
                richTextBox.BeginInvoke(new Action(() =>
                {
                    if (!richTextBox.IsDisposed)
                        ApplyDarkScrollBarTheme(richTextBox);
                }));
            };

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

            int inset = ScaledBorderThickness;
            int topBand = showTitleBar ? ScaledTitleHeight : 0;
            int bottomBand = ScaledProgressBarBandHeight;

            int contentX = inset + Padding.Left;
            int contentY = inset + Padding.Top + topBand;

            int contentW = Math.Max(0, Width - (inset * 2) - Padding.Left - Padding.Right);
            int contentH = Math.Max(0, Height - (inset * 2) - Padding.Top - Padding.Bottom - topBand - bottomBand);

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

            using (var path = CreateRoundedRectPath(new Rectangle(0, 0, Width, Height), ScaledCornerRadius))
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
        /// Raised after the parent finishes handling a DPI change. The inner layout,
        /// rounded region, and painted title bar are all derived from <see cref="DeviceDpi"/>,
        /// so recompute them and force a repaint. Without this the title bar keeps its
        /// 96-DPI pixel size/font when the form is dragged to a higher-DPI monitor, which
        /// leaves the title text looking too small relative to the rescaled panel.
        /// </summary>
        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);
            UpdateLayoutSafe();
            UpdateRegionSafe();
            Invalidate();
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
                int t = ScaledBorderThickness;
                int scaledTitleHeight = ScaledTitleHeight;
                int scaledCornerRadius = ScaledCornerRadius;
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
                int innerRadius = Math.Max(0, scaledCornerRadius - t);

                using (var fillPath = CreateRoundedRectPath(innerFill, innerRadius))
                using (var backBrush = new SolidBrush(BackColor))
                {
                    g.FillPath(backBrush, fillPath);
                }

                if (showTitleBar && scaledTitleHeight > 0)
                {
                    Rectangle titleRect = new Rectangle(
                        t,
                        t,
                        Math.Max(0, cw - 2 * t),
                        Math.Max(0, scaledTitleHeight));

                    using (var titlePath = CreateTopRoundedRectPath(titleRect, innerRadius))
                    using (var titleBrush = new SolidBrush(titleBackColor))
                    {
                        g.FillPath(titleBrush, titlePath);
                    }

                    if (showTitleSeparator)
                    {
                        int y = t + scaledTitleHeight;
                        using (var sepPen = new Pen(borderColor, 1f))
                        {
                            g.DrawLine(sepPen, t, y, cw - t - 1, y);
                        }
                    }

                    // Fonts measured in Point (the WinForms default) are DPI-independent:
                    // GDI+ converts points to pixels using the current Graphics DpiY at draw
                    // time (pixels = points * DpiY / 72), so a 9pt font automatically renders
                    // at 2x the pixel size on a 192-DPI monitor without any manual scaling.
                    // Only pixel-unit fonts need the DPI factor applied by hand, because they
                    // are interpreted literally in device pixels regardless of monitor DPI
                    // and — since CalibrationComponent disables child scaling — WinForms will
                    // not resize them for us when the form moves to a higher-DPI monitor.
                    Font sourceFont = titleFont ?? new Font(Font, FontStyle.Bold);
                    Font tf;
                    if (sourceFont.Unit == GraphicsUnit.Pixel)
                    {
                        tf = new Font(
                            sourceFont.FontFamily,
                            sourceFont.Size * DpiScale,
                            sourceFont.Style,
                            sourceFont.Unit);
                    }
                    else
                    {
                        tf = sourceFont;
                    }

                    using (var textBrush = new SolidBrush(titleForeColor))
                    using (var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    {
                        Rectangle textRect = new Rectangle(
                            t,
                            t,
                            Math.Max(0, cw - 2 * t),
                            scaledTitleHeight);

                        g.DrawString(titleText ?? string.Empty, tf, textBrush, textRect, fmt);
                    }

                    // Dispose only the fonts we allocated. Never dispose the user-owned
                    // titleFont property — its lifetime belongs to the caller.
                    if (tf != sourceFont)
                        tf.Dispose();
                    if (sourceFont != titleFont)
                        sourceFont.Dispose();
                }

                if (showProgressBar)
                {
                    DrawProgressBar(g, t, cw, ch);
                }

                using (var borderPath = CreateRoundedRectPath(strokeBounds, scaledCornerRadius))
                using (var pen = new Pen(borderColor, t))
                {
                    pen.Alignment = PenAlignment.Center;
                    g.DrawPath(pen, borderPath);
                }
            }
            finally
            {
                using (GraphicsPath regionPath = CreateRoundedRectPath(new Rectangle(0, 0, cw, ch), ScaledCornerRadius))
                    Region = new Region(regionPath);
            }
        }

        /// <summary>
        /// Draws the bottom progress bar band: a rounded track filled left-to-right based on
        /// <see cref="ProgressValue"/>, with internal tick dividers at each division boundary.
        /// The bar sits inside the outer border, horizontally inset by the configured side margin.
        /// </summary>
        private void DrawProgressBar(Graphics g, int borderThicknessPx, int cw, int ch)
        {
            int side = ScaledProgressBarSideMargin;
            int bottomMargin = ScaledProgressBarBottomMargin;
            int barHeight = ScaledProgressBarHeight;

            int barLeft = borderThicknessPx + side;
            int barRight = cw - borderThicknessPx - side;
            int barWidth = barRight - barLeft;
            if (barWidth <= 2 || barHeight <= 2)
                return;

            int barTop = ch - borderThicknessPx - bottomMargin - barHeight;
            if (barTop < borderThicknessPx)
                return;

            Rectangle barRect = new Rectangle(barLeft, barTop, barWidth, barHeight);
            int barCorner = Math.Min(barHeight / 2, ScaleByDpi(4));

            using (var trackPath = CreateRoundedRectPath(barRect, barCorner))
            using (var trackBrush = new SolidBrush(progressBarTrackColor))
            {
                g.FillPath(trackBrush, trackPath);
            }

            if (progressValue > 0f)
            {
                int fillWidth = (int)Math.Round(barWidth * progressValue, MidpointRounding.AwayFromZero);
                if (fillWidth > 0)
                {
                    Rectangle fillRect = new Rectangle(barLeft, barTop, fillWidth, barHeight);

                    Region prevClip = g.Clip;
                    using (var clipPath = CreateRoundedRectPath(barRect, barCorner))
                    {
                        g.SetClip(clipPath, CombineMode.Intersect);
                        using (var fillBrush = new SolidBrush(progressBarFillColor))
                        {
                            g.FillRectangle(fillBrush, fillRect);
                        }
                        g.Clip = prevClip;
                    }
                }
            }

            if (progressBarDivisions > 1)
            {
                using (var divPen = new Pen(progressBarDivisionColor, 1f))
                {
                    int tickTop = barTop + 2;
                    int tickBottom = barTop + barHeight - 2;
                    for (int i = 1; i < progressBarDivisions; i++)
                    {
                        int x = barLeft + (int)Math.Round(barWidth * (i / (float)progressBarDivisions));
                        g.DrawLine(divPen, x, tickTop, x, tickBottom);
                    }
                }
            }

            using (var borderPath = CreateRoundedRectPath(barRect, barCorner))
            using (var borderPen = new Pen(progressBarBorderColor, 1f))
            {
                borderPen.Alignment = PenAlignment.Inset;
                g.DrawPath(borderPen, borderPath);
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
