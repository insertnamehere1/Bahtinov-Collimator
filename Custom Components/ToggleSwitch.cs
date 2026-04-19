using System;
using Bahtinov_Collimator;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

/// <summary>
/// A custom WinForms toggle switch control with smooth animation,
/// strong depth shading, hover/pressed states, keyboard support,
/// and rotation via drawing transforms (0/90/180/270 degrees).
/// </summary>
public class ToggleSwitch : Control
{
    #region Fields

    private bool isOn;
    private float animation; // 0..1
    private readonly Timer animationTimer;
    private bool isHover;
    private bool isPressed;
    private int rotationDegrees;

    #endregion

    #region Public Properties and Events

    /// <summary>
    /// Gets or sets the drawing rotation angle in degrees.
    /// Supported values: 0, 90, 180, 270.
    /// </summary>
    [DefaultValue(0)]
    public int RotationDegrees
    {
        get => rotationDegrees;
        set
        {
            int norm = NormalizeRotation(value);
            if (rotationDegrees == norm) return;
            rotationDegrees = norm;
            Invalidate();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ToggleSwitch"/> control.
    /// </summary>
    public ToggleSwitch()
    {
        DoubleBuffered = true;
        Size = new Size(50, 24);
        Cursor = Cursors.Hand;
        SetStyle(ControlStyles.Selectable, true);
        TabStop = true;

        rotationDegrees = 0;

        animationTimer = new Timer { Interval = 15 };
        animationTimer.Tick += (s, e) =>
        {
            float target = isOn ? 1f : 0f;

            animation += (target - animation) * 0.25f;

            if (Math.Abs(animation - target) < 0.01f)
            {
                animation = target;
                animationTimer.Stop();
            }

            Invalidate();
        };
    }

    /// <summary>
    /// Gets or sets whether the switch is ON.
    /// </summary>
    [DefaultValue(false)]
    public bool IsOn
    {
        get => isOn;
        set
        {
            if (isOn == value) return;

            isOn = value;
            animationTimer.Start();
            Invalidate();
            CheckedChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Raised when <see cref="IsOn"/> changes.
    /// </summary>
    public event EventHandler CheckedChanged;

    #endregion

    #region Lifecycle

    /// <summary>
    /// Disposes managed resources.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            animationTimer?.Dispose();

        base.Dispose(disposing);
    }

    #endregion

    #region Input Handling

    /// <summary>
    /// Enables hover visuals when the mouse enters.
    /// </summary>
    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        if (!Enabled) return;
        isHover = true;
        Invalidate();
    }

    /// <summary>
    /// Clears hover/pressed visuals when the mouse leaves.
    /// </summary>
    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        isHover = false;
        isPressed = false;
        Invalidate();
    }

    /// <summary>
    /// Toggles on left mouse down, shows pressed feedback, and focuses the control.
    /// </summary>
    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (!Enabled) return;

        if (e.Button == MouseButtons.Left && ClientRectangle.Contains(e.Location))
        {
            isPressed = true;
            Focus();
            IsOn = !IsOn;
            Invalidate();
        }
    }

    /// <summary>
    /// Clears pressed feedback on mouse release.
    /// </summary>
    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (!Enabled) return;

        if (isPressed && e.Button == MouseButtons.Left)
        {
            isPressed = false;
            Invalidate();
        }
    }

    /// <summary>
    /// Supports Space/Enter for toggle.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!Enabled) return;

        if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
        {
            isPressed = true;
            Invalidate();
            e.Handled = true;
        }
    }

    /// <summary>
    /// Toggles on Space/Enter release.
    /// </summary>
    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        if (!Enabled) return;

        if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
        {
            isPressed = false;
            IsOn = !IsOn;
            e.Handled = true;
        }
    }

    #endregion

    #region Painting

    /// <summary>
    /// Draws the control. Rotation is applied via graphics transforms.
    /// </summary>
    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

        float drawW = Width;
        float drawH = Height;

        if (rotationDegrees != 0)
        {
            g.TranslateTransform(Width / 2f, Height / 2f);
            g.RotateTransform(rotationDegrees);

            if (rotationDegrees == 90 || rotationDegrees == 270)
            {
                g.TranslateTransform(-Height / 2f, -Width / 2f);
                drawW = Height;
                drawH = Width;
            }
            else
            {
                g.TranslateTransform(-Width / 2f, -Height / 2f);
                drawW = Width;
                drawH = Height;
            }
        }

        RectangleF track = new RectangleF(1.2f, 1.2f, drawW - 3.4f, drawH - 3.4f);
        float radius = track.Height / 2f;

        bool hovered = isHover && Enabled;
        bool pressed = isPressed && Enabled;
        bool focused = Focused && ShowFocusCues;

        Color offBase = UITheme.ToggleSwitchOffBase;
        Color offBaseHover = UITheme.ToggleSwitchOffBaseHover;
        Color border = UITheme.ToggleSwitchBorder;
        Color borderHover = UITheme.ToggleSwitchBorderHover;

        Color onRed = UITheme.ToggleSwitchOnRed;
        Color onRedHover = UITheme.ToggleSwitchOnRedHover;

        if (!Enabled)
        {
            offBase = UITheme.ToggleSwitchDisabledOffBase;
            offBaseHover = offBase;
            onRed = UITheme.ToggleSwitchDisabledOn;
            onRedHover = onRed;
            border = UITheme.ToggleSwitchDisabledBorder;
            borderHover = border;
        }

        float shadowYOffset = pressed ? 0.7f : 2.0f;
        int shadowAlpha = pressed ? 70 : 120;

        using (GraphicsPath shadowPath = RoundedRect(
                   new RectangleF(track.X, track.Y + shadowYOffset, track.Width, track.Height),
                   radius))
        using (Brush trackShadow = new SolidBrush(UITheme.WithAlpha(UITheme.Black, shadowAlpha)))
        {
            g.FillPath(trackShadow, shadowPath);
        }

        Color onCol = hovered ? onRedHover : onRed;
        Color offCol = hovered ? offBaseHover : offBase;

        using (GraphicsPath trackPath = RoundedRect(track, radius))
        {
            // Always the same texture path so the last animated frame matches snapped 0/1 (no solid-vs-texture pop).
            FillTrackWithHorizontalBlendTexture(g, trackPath, track, animation, onCol, offCol);

            if (animation > 1e-5f)
            {
                g.SetClip(trackPath);

                RectangleF gloss = new RectangleF(track.X, track.Y, track.Width, track.Height * 0.4f);
                using (Brush glossBrush = new LinearGradientBrush(
                           gloss,
                           UITheme.WithAlpha(UITheme.White, Enabled ? 120 : 50),
                           UITheme.WithAlpha(UITheme.White, 0),
                           LinearGradientMode.Vertical))
                {
                    g.FillRectangle(glossBrush, gloss);
                }

                g.ResetClip();
            }

            using (Pen p = new Pen(hovered ? borderHover : border))
                g.DrawPath(p, trackPath);

            if (focused && Enabled)
            {
                RectangleF focusRect = Inflate(track, 1.0f);
                using (GraphicsPath focusPath = RoundedRect(focusRect, focusRect.Height / 2f))
                using (Pen focusPen = new Pen(UITheme.ToggleSwitchFocusRing, 1.6f))
                {
                    g.DrawPath(focusPen, focusPath);
                }
            }
        }

        float thumbSize = drawH - 7.2f;
        float thumbX = 3.2f + (drawW - drawH) * animation;
        float thumbY = 3.2f + (pressed ? 0.6f : 0f);

        RectangleF thumb = new RectangleF(thumbX, thumbY, thumbSize, thumbSize);

        using (GraphicsPath thumbPath = RoundedRect(thumb, thumbSize / 2f))
        {
            int thumbShadowAlpha = pressed ? 105 : 130;
            float sx = pressed ? 1.2f : 1.8f;
            float sy = pressed ? 1.5f : 2.2f;

            using (Brush sh = new SolidBrush(UITheme.WithAlpha(UITheme.Black, thumbShadowAlpha)))
                g.FillEllipse(sh, thumb.X + sx, thumb.Y + sy, thumb.Width, thumb.Height);

            using (Brush fill = new SolidBrush(UITheme.White))
                g.FillPath(fill, thumbPath);

            RectangleF innerThumb = Inflate(thumb, -1.2f);
            using (GraphicsPath innerThumbPath = RoundedRect(innerThumb, innerThumb.Height / 2f))
            {
                g.SetClip(innerThumbPath);
                using (Brush glow = new LinearGradientBrush(
                           innerThumb,
                           UITheme.WithAlpha(UITheme.White, 110),
                           UITheme.WithAlpha(UITheme.White, 0),
                           LinearGradientMode.Vertical))
                {
                    g.FillRectangle(glow, innerThumb);
                }
                g.ResetClip();
            }

            using (Pen outline = new Pen(UITheme.ToggleSwitchThumbOutline))
                g.DrawPath(outline, thumbPath);

            float cx = thumb.X + thumb.Width / 2f;
            float gy1 = thumb.Y + thumb.Height * 0.25f;
            float gy2 = thumb.Bottom - thumb.Height * 0.25f;

            g.SetClip(thumbPath);
            using (Pen groove = new Pen(UITheme.Black, 1.8f))
            {
                groove.StartCap = LineCap.Round;
                groove.EndCap = LineCap.Round;
                g.DrawLine(groove, cx, gy1, cx, gy2);
            }
            g.ResetClip();
        }
    }

    #endregion

    #region Geometry Helpers

    /// <summary>
    /// Normalizes an arbitrary angle to the nearest supported toggle orientation.
    /// </summary>
    /// <returns>One of 0, 90, 180, or 270 degrees.</returns>
    private static int NormalizeRotation(int degrees)
    {
        int d = degrees % 360;
        if (d < 0) d += 360;

        if (d >= 315 || d < 45) return 0;
        if (d >= 45 && d < 135) return 90;
        if (d >= 135 && d < 225) return 180;
        return 270;
    }

    /// <summary>
    /// Fills the track with a single horizontal red→gray blend (no stacked rectangles).
    /// Uses a 1D texture scaled so the transition stays smooth without a hard vertical edge.
    /// </summary>
    private static void FillTrackWithHorizontalBlendTexture(
        Graphics g,
        GraphicsPath trackPath,
        RectangleF track,
        float animation,
        Color onCol,
        Color offCol)
    {
        const int texW = 512;
        float a = animation;
        if (a < 0f) a = 0f;
        else if (a > 1f) a = 1f;

        using (Bitmap bmp = new Bitmap(texW, 1, PixelFormat.Format32bppArgb))
        {
            if (a <= 0f)
            {
                for (int x = 0; x < texW; x++)
                    bmp.SetPixel(x, 0, offCol);
            }
            else if (a >= 1f)
            {
                for (int x = 0; x < texW; x++)
                    bmp.SetPixel(x, 0, onCol);
            }
            else
            {
                // Blend width shrinks near 0/1 so the ramp does not smear across the whole pill.
                float halfNorm = Math.Max(0.05f, Math.Min(a, 1f - a) * 0.35f);
                halfNorm = Math.Max(halfNorm, 3f / texW);

                float lo = a - halfNorm;
                float hi = a + halfNorm;
                if (lo < 0f) lo = 0f;
                if (hi > 1f) hi = 1f;
                if (lo >= hi)
                    hi = Math.Min(1f, lo + 1f / texW);

                for (int x = 0; x < texW; x++)
                {
                    float u = (x + 0.5f) / texW;
                    float t = SmoothStep(lo, hi, u);
                    float amountOn = 1f - t;
                    bmp.SetPixel(x, 0, UITheme.LerpRgb(offCol, onCol, amountOn));
                }
            }

            using (TextureBrush tb = new TextureBrush(bmp, WrapMode.Clamp))
            {
                tb.TranslateTransform(track.X, track.Y);
                tb.ScaleTransform(track.Width / texW, track.Height);

                InterpolationMode savedI = g.InterpolationMode;
                PixelOffsetMode savedP = g.PixelOffsetMode;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                try
                {
                    g.FillPath(tb, trackPath);
                }
                finally
                {
                    g.InterpolationMode = savedI;
                    g.PixelOffsetMode = savedP;
                }
            }
        }
    }

    private static float SmoothStep(float edge0, float edge1, float x)
    {
        if (x <= edge0) return 0f;
        if (x >= edge1) return 1f;
        float t = (x - edge0) / (edge1 - edge0);
        return t * t * (3f - 2f * t);
    }

    /// <summary>
    /// Returns a rectangle inflated inward or outward by a uniform amount.
    /// </summary>
    /// <returns>The transformed rectangle.</returns>
    private static RectangleF Inflate(RectangleF r, float by)
        => new RectangleF(r.X + by, r.Y + by, r.Width - 2 * by, r.Height - 2 * by);

    /// <summary>
    /// Builds a rounded rectangle path for the supplied bounds and corner radius.
    /// </summary>
    /// <returns>A closed rounded rectangle path.</returns>
    private static GraphicsPath RoundedRect(RectangleF rect, float radius)
    {
        GraphicsPath path = new GraphicsPath();
        float d = radius * 2;

        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();

        return path;
    }

    #endregion
}