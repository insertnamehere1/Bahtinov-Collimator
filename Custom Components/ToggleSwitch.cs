using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

/// <summary>
/// A custom WinForms toggle switch control with smooth animation,
/// strong depth shading, hover/pressed states, keyboard support,
/// and rotation via drawing transforms (0/90/180/270 degrees).
/// </summary>
public class ToggleSwitch : Control
{
    private bool isOn;
    private float animation; // 0..1
    private readonly Timer animationTimer;

    private bool isHover;
    private bool isPressed;

    private int rotationDegrees;

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

    /// <summary>
    /// Disposes managed resources.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            animationTimer?.Dispose();

        base.Dispose(disposing);
    }

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
    /// Shows pressed feedback and focuses the control.
    /// </summary>
    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (!Enabled) return;

        if (e.Button == MouseButtons.Left)
        {
            isPressed = true;
            Focus();
            Invalidate();
        }
    }

    /// <summary>
    /// Toggles on mouse release when released inside the control bounds.
    /// </summary>
    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (!Enabled) return;

        if (isPressed && e.Button == MouseButtons.Left)
        {
            isPressed = false;

            if (ClientRectangle.Contains(e.Location))
                IsOn = !IsOn;
            else
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

    /// <summary>
    /// Draws the control. Rotation is applied via graphics transforms.
    /// </summary>
    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

        // Apply rotation around control center.
        float drawW = Width;
        float drawH = Height;

        if (rotationDegrees != 0)
        {
            g.TranslateTransform(Width / 2f, Height / 2f);
            g.RotateTransform(rotationDegrees);

            // After rotation, translate so the "draw area" starts at (0,0)
            // for a consistent drawing coordinate system.
            if (rotationDegrees == 90 || rotationDegrees == 270)
            {
                // Swap width/height in the rotated coordinate system.
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

        // Darker palette
        Color offBase = Color.FromArgb(200, 198, 194);
        Color offBaseHover = Color.FromArgb(210, 208, 204);
        Color border = Color.FromArgb(120, 120, 120);
        Color borderHover = Color.FromArgb(100, 100, 100);

        Color onRed = Color.FromArgb(180, 45, 45);
        Color onRedHover = Color.FromArgb(195, 55, 55);

        if (!Enabled)
        {
            offBase = Color.FromArgb(215, 215, 215);
            offBaseHover = offBase;
            onRed = Color.FromArgb(190, 190, 190);
            onRedHover = onRed;
            border = Color.FromArgb(180, 180, 180);
            borderHover = border;
        }

        // Outer shadow
        float shadowYOffset = pressed ? 0.7f : 2.0f;
        int shadowAlpha = pressed ? 70 : 120;

        using (GraphicsPath shadowPath = RoundedRect(
                   new RectangleF(track.X, track.Y + shadowYOffset, track.Width, track.Height),
                   radius))
        using (Brush trackShadow = new SolidBrush(Color.FromArgb(shadowAlpha, 0, 0, 0)))
        {
            g.FillPath(trackShadow, shadowPath);
        }

        using (GraphicsPath trackPath = RoundedRect(track, radius))
        {
            // Track fill
            using (Brush bg = new SolidBrush(hovered ? offBaseHover : offBase))
                g.FillPath(bg, trackPath);

            // ON fill (grows from the LEFT in the drawing coordinate system)
            if (animation > 0f)
            {
                float w = track.Width * animation;
                RectangleF red = new RectangleF(track.X, track.Top, w, track.Height);

                g.SetClip(trackPath);

                using (Brush r = new SolidBrush(hovered ? onRedHover : onRed))
                    g.FillRectangle(r, red);

                // Specular highlight on red
                RectangleF gloss = new RectangleF(red.X, red.Y, red.Width, red.Height * 0.4f);
                using (Brush glossBrush = new LinearGradientBrush(
                           gloss,
                           Color.FromArgb(Enabled ? 120 : 50, Color.White),
                           Color.FromArgb(0, Color.White),
                           LinearGradientMode.Vertical))
                {
                    g.FillRectangle(glossBrush, gloss);
                }

                g.ResetClip();
            }

            // Inner depth
            RectangleF inner = Inflate(track, -1.6f);
            using (GraphicsPath innerPath = RoundedRect(inner, inner.Height / 2f))
            {
                g.SetClip(innerPath);

                using (Brush topGlow = new LinearGradientBrush(
                           inner,
                           Color.FromArgb(60, Color.White),
                           Color.FromArgb(0, Color.White),
                           LinearGradientMode.Vertical))
                {
                    g.FillRectangle(topGlow, inner);
                }

                RectangleF bottom = new RectangleF(inner.X, inner.Bottom - inner.Height * 0.5f, inner.Width, inner.Height * 0.5f);
                using (Brush bottomShade = new LinearGradientBrush(
                           bottom,
                           Color.FromArgb(0, Color.Black),
                           Color.FromArgb(90, Color.Black),
                           LinearGradientMode.Vertical))
                {
                    g.FillRectangle(bottomShade, bottom);
                }

                g.ResetClip();
            }

            // Border
            using (Pen p = new Pen(hovered ? borderHover : border))
                g.DrawPath(p, trackPath);

            // Focus ring
            if (focused && Enabled)
            {
                RectangleF focusRect = Inflate(track, 1.0f);
                using (GraphicsPath focusPath = RoundedRect(focusRect, focusRect.Height / 2f))
                using (Pen focusPen = new Pen(Color.FromArgb(120, 90, 140, 255), 1.6f))
                {
                    g.DrawPath(focusPen, focusPath);
                }
            }
        }

        // Thumb
        float thumbSize = drawH - 7.2f;
        float thumbX = 3.2f + (drawW - drawH) * animation;
        float thumbY = 3.2f + (pressed ? 0.6f : 0f);

        RectangleF thumb = new RectangleF(thumbX, thumbY, thumbSize, thumbSize);

        using (GraphicsPath thumbPath = RoundedRect(thumb, thumbSize / 2f))
        {
            int thumbShadowAlpha = pressed ? 105 : 130;
            float sx = pressed ? 1.2f : 1.8f;
            float sy = pressed ? 1.5f : 2.2f;

            using (Brush sh = new SolidBrush(Color.FromArgb(thumbShadowAlpha, 0, 0, 0)))
                g.FillEllipse(sh, thumb.X + sx, thumb.Y + sy, thumb.Width, thumb.Height);

            using (Brush fill = new SolidBrush(Color.White))
                g.FillPath(fill, thumbPath);

            RectangleF innerThumb = Inflate(thumb, -1.2f);
            using (GraphicsPath innerThumbPath = RoundedRect(innerThumb, innerThumb.Height / 2f))
            {
                g.SetClip(innerThumbPath);
                using (Brush glow = new LinearGradientBrush(
                           innerThumb,
                           Color.FromArgb(110, Color.White),
                           Color.FromArgb(0, Color.White),
                           LinearGradientMode.Vertical))
                {
                    g.FillRectangle(glow, innerThumb);
                }
                g.ResetClip();
            }

            using (Pen outline = new Pen(Color.FromArgb(120, 120, 120)))
                g.DrawPath(outline, thumbPath);

            // Groove
            float cx = thumb.X + thumb.Width / 2f;
            float gy1 = thumb.Y + thumb.Height * 0.25f;
            float gy2 = thumb.Bottom - thumb.Height * 0.25f;

            g.SetClip(thumbPath);
            using (Pen groove = new Pen(Color.Black, 1.8f))
            {
                groove.StartCap = LineCap.Round;
                groove.EndCap = LineCap.Round;
                g.DrawLine(groove, cx, gy1, cx, gy2);
            }
            g.ResetClip();
        }
    }

    private static int NormalizeRotation(int degrees)
    {
        int d = degrees % 360;
        if (d < 0) d += 360;

        // Snap to nearest of 0/90/180/270 (keeps behavior predictable).
        // If you prefer "accept any angle", remove this and just return d.
        if (d >= 315 || d < 45) return 0;
        if (d >= 45 && d < 135) return 90;
        if (d >= 135 && d < 225) return 180;
        return 270;
    }

    private static RectangleF Inflate(RectangleF r, float by)
        => new RectangleF(r.X + by, r.Y + by, r.Width - 2 * by, r.Height - 2 * by);

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
}