using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class FixedCheckBox : CheckBox
{
    private bool _hovered = false;

    public int BoxSize { get; set; } = 13;

    public FixedCheckBox()
    {
        SetStyle(ControlStyles.UserPaint |
                 ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(BackColor);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        // Box centered vertically
        int boxTop = (Height - BoxSize) / 2;
        Rectangle box = new Rectangle(0, boxTop, BoxSize, BoxSize);

        // Box background
        Color boxBack = !Enabled
            ? SystemColors.Control
            : _hovered ? Color.FromArgb(230, 240, 255) : Color.White;

        using (SolidBrush brush = new SolidBrush(boxBack))
            e.Graphics.FillRectangle(brush, box);

        // Box border
        Color borderColor = !Enabled
            ? Color.FromArgb(180, 180, 180)
            : _hovered ? Color.FromArgb(0, 120, 215) : Color.FromArgb(100, 100, 100);

        using (Pen pen = new Pen(borderColor))
            e.Graphics.DrawRectangle(pen, box);

        // Checkmark
        if (Checked)
        {
            Color tickColor = Enabled ? Color.FromArgb(0, 120, 215) : Color.FromArgb(150, 150, 150);
            using (Pen pen = new Pen(tickColor, 2f))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;

                Point p1 = new Point(box.Left + 2, box.Top + box.Height / 2);
                Point p2 = new Point(box.Left + box.Width / 2 - 1, box.Bottom - 3);
                Point p3 = new Point(box.Right - 2, box.Top + 2);

                e.Graphics.DrawLine(pen, p1, p2);
                e.Graphics.DrawLine(pen, p2, p3);
            }
        }

        // Text
        if (!string.IsNullOrEmpty(Text))
        {
            Rectangle textRect = new Rectangle(BoxSize + 4, 0, Width - BoxSize - 4, Height);
            Color textColor = Enabled ? ForeColor : SystemColors.GrayText;
            TextRenderer.DrawText(e.Graphics, Text, Font, textRect, textColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
        }

        // Focus rectangle
        if (Focused && ShowFocusCues)
        {
            Rectangle focusRect = new Rectangle(BoxSize + 2, 1, Width - BoxSize - 4, Height - 2);
            ControlPaint.DrawFocusRectangle(e.Graphics, focusRect);
        }
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _hovered = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hovered = false;
        Invalidate();
    }

    protected override void OnCheckedChanged(EventArgs e)
    {
        base.OnCheckedChanged(e);
        Invalidate();
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        Invalidate();
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }
}

