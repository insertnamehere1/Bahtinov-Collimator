using System;
using System.Collections.Generic;
using System.ComponentModel;
using Bahtinov_Collimator;
using System.Drawing;
using System.Windows.Forms;

namespace Bahtinov_Collimator.Custom_Components
{
    /// <summary>
    /// Displays a horizontal bar with minimum and maximum labels, a zero tick,
    /// and a circular marker that indicates the current value. The numeric
    /// value is rendered above the marker.
    /// 
    /// Intended for visualizing Bahtinov line offsets or similar signed values.
    /// Now DPI aware by scaling layout and drawing based on DeviceDpi.
    /// </summary>
    public partial class HistoryBar : Control
    {
        #region Win32 DPI Messages

        /// <summary>
        /// Window message sent when the DPI for this window has changed.
        /// </summary>
        private const int WM_DPICHANGED = 0x02E0;

        /// <summary>
        /// Window message sent to a child window before its parent processes a DPI change.
        /// </summary>
        private const int WM_DPICHANGED_BEFOREPARENT = 0x02E2;

        /// <summary>
        /// Window message sent to a child window after its parent processes a DPI change.
        /// </summary>
        private const int WM_DPICHANGED_AFTERPARENT = 0x02E3;

        #endregion

        #region Fields

        private float minimum = -1.0f;
        private float maximum = 1.0f;
        private float value = float.NaN;

        /// <summary>
        /// Stores the last N values for drawing semi-transparent history markers.
        /// </summary>
        private readonly Queue<float> valueHistory = new Queue<float>();

        /// <summary>
        /// Tracks the last observed DPI for this control so DPI transitions can be detected reliably.
        /// </summary>
        private int lastKnownDpi = DesignDpi;

        /// <summary>
        /// The number of previous values to keep and render as semi-opaque markers.
        /// </summary>
        private int historyCapacity = 6;

        /// <summary>
        /// Design-time DPI (WinForms baseline).
        /// All coordinates and sizes in this control are expressed as if at 96 DPI,
        /// then scaled at runtime based on DeviceDpi.
        /// </summary>
        private const int DesignDpi = 96;

        /// <summary>
        /// Gets the current DPI scale factor relative to the design DPI.
        /// </summary>
        private float DpiScale => DeviceDpi / (float)DesignDpi;

        /// <summary>
        /// Logical pixels at 96 DPI; scaled at runtime via <see cref="DpiScale"/>.
        /// </summary>
        private float designLeftPadding = 10f;
        private float designRightPadding = 10f;
        private float designZeroTickUp = 10f;
        private float designZeroTickDown = 10f;
        private float designHistoryMarkerRadius = 8f;
        private float designMarkerRadius = 7f;
        private float designBarThickness = 3f;
        private float designZeroTickThickness = 2f;
        private float designAlertOutlineThickness = 1f;
        private float designValueTextGap = 2f;
        private float designAlertOutlineInset = 3f;
        private int horizontalMargin;
        private float designContentPadding = 2f;
        private float designVerticalEdgePadding = 2f;
        private int preferredWidthDesign = 300;
        private Color historyMarkerColor = UITheme.ErrorBarHistoryColor;
        private Color outOfRangeColor = UITheme.ErrorBarMarkerColorInRange;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the minimum value of the scale.
        /// </summary>
        [Category("Behavior")]
        [Description("Minimum value of the scale.")]
        public float Minimum
        {
            get => minimum;
            set
            {
                minimum = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the maximum value of the scale.
        /// </summary>
        [Category("Behavior")]
        [Description("Maximum value of the scale.")]
        public float Maximum
        {
            get => maximum;
            set
            {
                maximum = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the current value displayed by the control.
        /// The marker is clamped to the [Minimum, Maximum] range for drawing,
        /// but the raw value is still used for the numeric label and for deciding
        /// when to show the value in red if it is out of range.
        /// 
        /// Each time the value changes, the previous value is added to the
        /// history buffer so that up to the last five positions are drawn
        /// as semi-opaque markers.
        /// </summary>
        [Category("Behavior")]
        [Description("Current value displayed on the scale.")]
        public float Value
        {
            get => value;
            set
            {
                historyCapacity = Properties.Settings.Default.historyCount;

                if (!float.IsNaN(this.value))
                {
                    AddToHistory(this.value);
                }

                this.value = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the color of the main bar line.
        /// </summary>
        [Category("Appearance")]
        [Description("Color of the main bar line.")]
        public Color BarColor { get; set; } = UITheme.ErrorBarColor;

        /// <summary>
        /// Gets or sets the color of the marker (the circular pointer).
        /// </summary>
        [Category("Appearance")]
        [Description("Color of the marker (the circular pointer).")]
        public Color MarkerColor { get; set; } = UITheme.ErrorBarMarkerColorInRange;

        /// <summary>
        /// Gets or sets the color of the zero tick mark in the center of the bar.
        /// </summary>
        [Category("Appearance")]
        [Description("Color of the zero tick mark.")]
        public Color ZeroTickColor { get; set; } = UITheme.ZeroTickColor;

        /// <summary>
        /// Gets or sets the base color used for text (labels and value).
        /// The value label will be drawn in red if the raw value is outside
        /// the [Minimum, Maximum] range.
        /// </summary>
        [Category("Appearance")]
        [Description("Base text color for labels and value.")]
        public Color TextColor { get; set; } = UITheme.ErrorBarTextColor;

        /// <summary>
        /// Horizontal space reserved at the left for the minimum label (logical px at 96 DPI).
        /// </summary>
        [Category("Layout")]
        [Description("Left inset for the minimum label, in logical pixels at 96 DPI.")]
        [DefaultValue(10f)]
        public float DesignLeftPadding
        {
            get => designLeftPadding;
            set
            {
                designLeftPadding = value;
                OnLayoutMetricsChanged();
            }
        }

        /// <summary>
        /// Horizontal space reserved at the right for the maximum label (logical px at 96 DPI).
        /// </summary>
        [Category("Layout")]
        [Description("Right inset for the maximum label, in logical pixels at 96 DPI.")]
        [DefaultValue(10f)]
        public float DesignRightPadding
        {
            get => designRightPadding;
            set
            {
                designRightPadding = value;
                OnLayoutMetricsChanged();
            }
        }

        /// <summary>
        /// Length of the center zero tick above the bar (logical px at 96 DPI).
        /// </summary>
        [Category("Layout")]
        [Description("Zero tick length upward from the bar, in logical pixels at 96 DPI.")]
        [DefaultValue(15f)]
        public float DesignZeroTickUp
        {
            get => designZeroTickUp;
            set
            {
                designZeroTickUp = value;
                OnLayoutMetricsChanged();
            }
        }

        /// <summary>
        /// Length of the center zero tick below the bar (logical px at 96 DPI).
        /// </summary>
        [Category("Layout")]
        [Description("Zero tick length downward from the bar, in logical pixels at 96 DPI.")]
        [DefaultValue(15f)]
        public float DesignZeroTickDown
        {
            get => designZeroTickDown;
            set
            {
                designZeroTickDown = value;
                OnLayoutMetricsChanged();
            }
        }

        /// <summary>
        /// Radius of each history marker circle (logical px at 96 DPI).
        /// </summary>
        [Category("Layout")]
        [Description("Radius of semi-transparent history markers, in logical pixels at 96 DPI.")]
        [DefaultValue(8f)]
        public float DesignHistoryMarkerRadius
        {
            get => designHistoryMarkerRadius;
            set
            {
                designHistoryMarkerRadius = value;
                OnLayoutMetricsChanged();
            }
        }

        /// <summary>
        /// Radius of the current-value marker (logical px at 96 DPI).
        /// </summary>
        [Category("Layout")]
        [Description("Radius of the main value marker, in logical pixels at 96 DPI.")]
        [DefaultValue(7f)]
        public float DesignMarkerRadius
        {
            get => designMarkerRadius;
            set
            {
                designMarkerRadius = value;
                OnLayoutMetricsChanged();
            }
        }

        /// <summary>
        /// Stroke width of the horizontal bar (logical px at 96 DPI).
        /// </summary>
        [Category("Layout")]
        [Description("Thickness of the main horizontal line, in logical pixels at 96 DPI.")]
        [DefaultValue(2f)]
        public float DesignBarThickness
        {
            get => designBarThickness;
            set
            {
                designBarThickness = value;
                OnLayoutMetricsChanged();
            }
        }

        /// <summary>
        /// Stroke width of tick lines (logical px at 96 DPI).
        /// </summary>
        [Category("Layout")]
        [Description("Thickness of zero and end tick lines, in logical pixels at 96 DPI.")]
        [DefaultValue(2f)]
        public float DesignZeroTickThickness
        {
            get => designZeroTickThickness;
            set
            {
                designZeroTickThickness = value;
                OnLayoutMetricsChanged();
            }
        }

        /// <summary>
        /// Stroke width of the out-of-range alert ellipse (logical px at 96 DPI).
        /// </summary>
        [Category("Layout")]
        [Description("Thickness of the alert outline when value is out of range, in logical pixels at 96 DPI.")]
        [DefaultValue(1f)]
        public float DesignAlertOutlineThickness
        {
            get => designAlertOutlineThickness;
            set
            {
                designAlertOutlineThickness = value;
                OnLayoutMetricsChanged();
            }
        }

        /// <summary>
        /// Gap between the value text and the marker (logical px at 96 DPI).
        /// </summary>
        [Category("Layout")]
        [Description("Space between the numeric label and the marker, in logical pixels at 96 DPI.")]
        [DefaultValue(2f)]
        public float DesignValueTextGap
        {
            get => designValueTextGap;
            set
            {
                designValueTextGap = value;
                OnLayoutMetricsChanged();
            }
        }

        /// <summary>
        /// Inset of the out-of-range alert ellipse beyond the marker (logical px at 96 DPI).
        /// </summary>
        [Category("Layout")]
        [Description("Extra padding for the out-of-range ellipse beyond the marker bounds, in logical pixels at 96 DPI.")]
        [DefaultValue(3f)]
        public float DesignAlertOutlineInset
        {
            get => designAlertOutlineInset;
            set
            {
                designAlertOutlineInset = value;
                OnLayoutMetricsChanged();
            }
        }

        /// <summary>
        /// Extra horizontal margin inside the control client area (logical px, scaled at runtime).
        /// </summary>
        [Category("Layout")]
        [Description("Additional horizontal margin inside the client area (device-independent).")]
        [DefaultValue(0)]
        public int HorizontalMargin
        {
            get => horizontalMargin;
            set
            {
                horizontalMargin = value;
                OnLayoutMetricsChanged();
            }
        }

        /// <summary>
        /// Minimum vertical padding used when centering content (logical px at 96 DPI).
        /// </summary>
        [Category("Layout")]
        [Description("Minimum vertical padding when centering the bar in the client area, in logical pixels at 96 DPI.")]
        [DefaultValue(2f)]
        public float DesignContentPadding
        {
            get => designContentPadding;
            set
            {
                designContentPadding = value;
                OnLayoutMetricsChanged();
            }
        }

        /// <summary>
        /// Vertical padding included in minimum height, per top and bottom edge (logical px at 96 DPI).
        /// </summary>
        [Category("Layout")]
        [Description("Padding added to preferred height (each of top and bottom), in logical pixels at 96 DPI.")]
        [DefaultValue(2f)]
        public float DesignVerticalEdgePadding
        {
            get => designVerticalEdgePadding;
            set
            {
                designVerticalEdgePadding = value;
                OnLayoutMetricsChanged();
            }
        }

        /// <summary>
        /// Default preferred width used by the control and <see cref="GetPreferredSize"/> (logical px at 96 DPI).
        /// </summary>
        [Category("Layout")]
        [Description("Preferred width in logical pixels at 96 DPI (used for default size and GetPreferredSize width).")]
        [DefaultValue(300)]
        public int PreferredWidthDesign
        {
            get => preferredWidthDesign;
            set
            {
                preferredWidthDesign = value;
                OnLayoutMetricsChanged();
            }
        }

        /// <summary>
        /// Color used for history marker circles (include alpha in the color).
        /// </summary>
        [Category("Layout")]
        [Description("Fill color for past value markers (set alpha for transparency).")]
        public Color HistoryMarkerColor
        {
            get => historyMarkerColor;
            set
            {
                historyMarkerColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Color for out-of-range value text and alert ellipse outline.
        /// </summary>
        [Category("Layout")]
        [Description("Color for out-of-range value text and alert outline.")]
        public Color OutOfRangeColor
        {
            get => outOfRangeColor;
            set
            {
                outOfRangeColor = value;
                Invalidate();
            }
        }

        #endregion

        #region Lifecycle

        private void OnLayoutMetricsChanged()
        {
            if (IsHandleCreated)
                Size = new Size(Width, GetPreferredHeight());
            Invalidate();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryBar"/> class.
        /// Sets up double buffering and a sensible default size, scaled for the current DPI.
        /// </summary>
        public HistoryBar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint,
                     true);

            DoubleBuffered = true;

            Size = new Size((int)(preferredWidthDesign * DpiScale), GetPreferredHeight());
        }

        #endregion

        #region Sizing and DPI

        /// <summary>
        /// Ensures preferred size is also DPI aware when the designer or layout engine asks.
        /// </summary>
        /// <param name="proposedSize">The requested size proposed by the layout engine.</param>
        /// <returns>The preferred size for this control at the current DPI.</returns>
        public override Size GetPreferredSize(Size proposedSize)
        {
            var scale = DpiScale;
            return new Size(
                (int)(preferredWidthDesign * scale),
                GetPreferredHeight());
        }

        /// <summary>
        /// Adds a value to the history list, keeping only the last N entries.
        /// </summary>
        private void AddToHistory(float oldValue)
        {
            valueHistory.Enqueue(oldValue);

            while (valueHistory.Count > historyCapacity)
                valueHistory.Dequeue();
        }

        /// <summary>
        /// Captures the initial DPI once the underlying window handle is created.
        /// </summary>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            lastKnownDpi = DeviceDpi > 0 ? DeviceDpi : DesignDpi;
            UpdateLayoutForDpiChange();
        }

        /// <summary>
        /// Updates layout when the control is reparented (which can occur during DPI transitions).
        /// </summary>
        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            UpdateLayoutForDpiChange();
        }

        /// <summary>
        /// Intercepts DPI-change related window messages and refreshes layout when the DPI changes.
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            int oldDpi = lastKnownDpi;

            base.WndProc(ref m);

            if (m.Msg == WM_DPICHANGED ||
                m.Msg == WM_DPICHANGED_BEFOREPARENT ||
                m.Msg == WM_DPICHANGED_AFTERPARENT)
            {
                int newDpi = DeviceDpi > 0 ? DeviceDpi : DesignDpi;
                if (newDpi != oldDpi)
                {
                    lastKnownDpi = newDpi;
                    UpdateLayoutForDpiChange();
                }
            }
        }

        /// <summary>
        /// Updates size/layout in response to a DPI change.
        /// </summary>
        private void UpdateLayoutForDpiChange()
        {
            if (IsDisposed)
                return;

            int preferredHeight = GetPreferredHeight();

            if (Height < preferredHeight)
                Height = preferredHeight;

            Invalidate();
        }

        #endregion

        #region Painting

        /// <summary>
        /// Performs custom painting of the bar, labels, zero tick, history markers,
        /// the current marker, and the numeric value above the marker.
        /// Layout and stroke sizes are scaled according to the current DPI.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            float scale = DpiScale;

            int margin = horizontalMargin;
            int left = margin + (int)(designLeftPadding * scale);
            int right = Width - margin - (int)(designRightPadding * scale);

            float zeroTickUp = designZeroTickUp * scale;
            float zeroTickDown = designZeroTickDown * scale;
            float historyRadius = designHistoryMarkerRadius * scale;
            float markerRadius = designMarkerRadius * scale;

            float barThickness = designBarThickness * scale;
            float zeroThickness = designZeroTickThickness * scale;
            float alertOutlineThickness = designAlertOutlineThickness * scale;
            float valueGap = designValueTextGap * scale;

            float textHeight = Font.Height;
            float aboveCenter = textHeight + valueGap + markerRadius;
            float belowCenter = Math.Max(zeroTickDown, historyRadius);
            float contentHeight = aboveCenter + belowCenter;
            float padding = designContentPadding * scale;
            float top = Math.Max(padding, (Height - contentHeight) / 2f);
            float centerY = top + aboveCenter;

            using (var barPen = new Pen(BarColor, barThickness))
            {
                g.DrawLine(barPen, left, centerY, right, centerY);
            }

            float range = maximum - minimum;
            if (Math.Abs(range) < float.Epsilon)
            {
                return;
            }

            float zeroT = (0f - minimum) / range;        // position 0..1 along the bar
            float zeroX = left + zeroT * (right - left);

            using (var zeroPen = new Pen(ZeroTickColor, zeroThickness))
            {
                g.DrawLine(zeroPen, zeroX, centerY - zeroTickUp, zeroX, centerY + zeroTickDown);
            }

            using (var tickPen = new Pen(ZeroTickColor, zeroThickness))
            {
                g.DrawLine(tickPen, left, centerY - zeroTickUp / 2, left, centerY + zeroTickDown / 2);
                g.DrawLine(tickPen, right, centerY - zeroTickUp / 2, right, centerY + zeroTickDown / 2);
            }

            using (var historyBrush = new SolidBrush(historyMarkerColor))
            {
                foreach (float histValue in valueHistory)
                {
                    if (!float.IsNaN(histValue))
                    {
                        float clampedHist = Math.Max(minimum, Math.Min(maximum, histValue));
                        float ht = (clampedHist - minimum) / range;
                        float hx = left + ht * (right - left);

                        var histRect = new RectangleF(
                            hx - historyRadius,
                            centerY - historyRadius,
                            historyRadius * 2,
                            historyRadius * 2);

                        g.FillEllipse(historyBrush, histRect);
                    }
                }
            }

            float clampedValue = Math.Max(minimum, Math.Min(maximum, (float.IsNaN(value))?0.0f:value));
            float t = (clampedValue - minimum) / range;
            float markerX = left + t * (right - left);

            var markerRect = new RectangleF(
                markerX - markerRadius,
                centerY - markerRadius,
                markerRadius * 2,
                markerRadius * 2);

            using (var markerBrush = new SolidBrush(MarkerColor))
            {
                g.FillEllipse(markerBrush, markerRect);
            }

            string valueStr = (float.IsNaN(value) ? 0.0f : value).ToString("0.0");
            SizeF valSize = g.MeasureString(valueStr, Font);

            bool outOfRange = value < minimum || value > maximum;
            Color valueTextColor = outOfRange ? outOfRangeColor : TextColor;

            if (outOfRange)
            {
                float inset = designAlertOutlineInset * scale;
                using (var alertPen = new Pen(outOfRangeColor, alertOutlineThickness))
                {
                    g.DrawEllipse(alertPen,
                        markerRect.X - inset,
                        markerRect.Y - inset,
                        markerRect.Width + 2 * inset,
                        markerRect.Height + 2 * inset);
                }
            }

            using (var textBrush = new SolidBrush(valueTextColor))
            {
                g.DrawString(
                    valueStr,
                    Font,
                    textBrush,
                    markerX - valSize.Width / 2f,
                    markerRect.Top - valSize.Height - valueGap);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Calculates the minimum height needed to render the marker, ticks, and value text without clipping.
        /// </summary>
        /// <returns>The minimum control height in pixels for the current DPI and font.</returns>
        private int GetPreferredHeight()
        {
            float scale = DpiScale;

            float aboveCenter = Font.Height + (designValueTextGap + designMarkerRadius) * scale;

            float zeroDown = designZeroTickDown * scale;
            float historyR = designHistoryMarkerRadius * scale;
            float belowCenter = Math.Max(zeroDown, historyR);

            return (int)Math.Ceiling(
                aboveCenter + belowCenter + 2f * designVerticalEdgePadding * scale);
        }

        /// <summary>
        /// Updates the preferred height when the font changes, since text height affects layout.
        /// </summary>
        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            Size = new Size(Width, GetPreferredHeight());
        }

        /// <summary>
        /// Clears the stored history values and refreshes the control.
        /// </summary>
        public void ResetHistory()
        {
            valueHistory.Clear();
            Invalidate();         // force redraw without old markers
        }

        /// <summary>
        /// Clears history and resets the current value to idle (<see cref="float.NaN"/>) without
        /// pushing the previous value into history — used when capture stops or display is cleared.
        /// </summary>
        public void ResetValueAndHistory()
        {
            valueHistory.Clear();
            value = float.NaN;
            Invalidate();
        }

        #endregion
    }
}
