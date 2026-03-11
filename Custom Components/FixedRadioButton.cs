using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class FixedRadioButton : RadioButton
{
    private bool _hovered = false;

    public int BoxSize { get; set; } = 20;

    public FixedRadioButton()
    {
        SetStyle(ControlStyles.UserPaint |
                 ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(BackColor);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        // Circle centered vertically
        int circleTop = (Height - BoxSize) / 2;
        Rectangle circle = new Rectangle(0, circleTop, BoxSize, BoxSize);

        // Circle background
        Color boxBack = !Enabled
            ? SystemColors.Control
            : _hovered ? Color.FromArgb(230, 240, 255) : Color.White;

        using (SolidBrush brush = new SolidBrush(boxBack))
            e.Graphics.FillEllipse(brush, circle);

        // Circle border
        Color borderColor = !Enabled
            ? Color.FromArgb(180, 180, 180)
            : _hovered ? Color.FromArgb(0, 120, 215) : Color.FromArgb(100, 100, 100);

        using (Pen pen = new Pen(borderColor))
            e.Graphics.DrawEllipse(pen, circle);

        // Inner filled circle when checked
        if (Checked)
        {
            int innerPadding = 3;
            Rectangle innerCircle = new Rectangle(
                circle.X + innerPadding,
                circle.Y + innerPadding,
                circle.Width - innerPadding * 2,
                circle.Height - innerPadding * 2);

            Color fillColor = Enabled ? Color.FromArgb(0, 120, 215) : Color.FromArgb(150, 150, 150);
            using (SolidBrush brush = new SolidBrush(fillColor))
                e.Graphics.FillEllipse(brush, innerCircle);
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

