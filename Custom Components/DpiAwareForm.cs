using Bahtinov_Collimator;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class DpiAwareForm : Form
{
    private Panel _titleBar;
    private Label _titleLabel;
    private Button _closeButton;
    private Button _minimizeButton;
    private Button _maximizeButton;

    private bool _dragging;
    private Point _dragStart;
    private Point _formStart;

    private const int TitleBarHeight = 45;
    private const int CaptionButtonWidth = 70;

    public int CornerRadius { get; set; } = 10;

    private bool _canMaximize = true;
    public bool CanMaximize
    {
        get { return _canMaximize; }
        set
        {
            _canMaximize = value;
            if (_maximizeButton != null)
            {
                _maximizeButton.ForeColor = value ? Color.White : Color.DimGray;
                _maximizeButton.FlatAppearance.MouseOverBackColor = value
                    ? UITheme.TitleButtonsMouseOver
                    : Color.Transparent;
                _maximizeButton.FlatAppearance.MouseDownBackColor = value
                    ? UITheme.TitleButtonsMouseOver
                    : Color.Transparent;
            }
        }
    }

    private bool _canMinimize = true;
    public bool CanMinimize
    {
        get { return _canMinimize; }
        set
        {
            _canMinimize = value;
            if (_minimizeButton != null)
            {
                _minimizeButton.ForeColor = value ? Color.White : Color.DimGray;
                _minimizeButton.FlatAppearance.MouseOverBackColor = value
                    ? UITheme.TitleButtonsMouseOver
                    : Color.Transparent;
                _minimizeButton.FlatAppearance.MouseDownBackColor = value
                    ? UITheme.TitleButtonsMouseOver
                    : Color.Transparent;
            }
        }
    }

    private bool _showMinimizeMaximize = true;
    public bool ShowMinimizeMaximize
    {
        get { return _showMinimizeMaximize; }
        set
        {
            _showMinimizeMaximize = value;
            if (_minimizeButton != null) _minimizeButton.Visible = value;
            if (_maximizeButton != null) _maximizeButton.Visible = value;
        }
    }

    public DpiAwareForm()
    {
        using (Graphics g = this.CreateGraphics())
        {
            UITheme.DpiValue = g.DpiX;
        }

        InitializeTitleBar();
    }

    private void InitializeTitleBar()
    {
        _titleBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = TitleBarHeight,
            BackColor = Color.FromArgb(0, 120, 215)
        };

        _titleLabel = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(8, 0, 0, 0),
            Font = new Font("Segoe UI", UITheme.LabelFontSize, FontStyle.Regular)
        };

        _minimizeButton = MakeCaptionButton("—");
        _maximizeButton = MakeCaptionButton("\u2610");
        _closeButton = MakeCaptionButton("✕");

        _closeButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(196, 43, 28);
        _closeButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(140, 30, 20);

        _closeButton.Click += (s, e) => Close();
        _minimizeButton.Click += (s, e) => { if (_canMinimize) WindowState = FormWindowState.Minimized; };
        _maximizeButton.Click += (s, e) => { if (_canMaximize) ToggleMaximize(); };

        // Fill must be added first; Right-docked controls added last appear rightmost
        _titleBar.Controls.Add(_titleLabel);
        _titleBar.Controls.Add(_minimizeButton);
        _titleBar.Controls.Add(_maximizeButton);
        _titleBar.Controls.Add(_closeButton);

        foreach (Control c in new Control[] { _titleBar, _titleLabel })
        {
            c.MouseDown += TitleBar_MouseDown;
            c.MouseMove += TitleBar_MouseMove;
            c.MouseUp += (s, e) => _dragging = false;
            c.DoubleClick += (s, e) => { if (_canMaximize) ToggleMaximize(); };
        }

        Controls.Add(_titleBar);
    }

    private Button MakeCaptionButton(string symbol)
    {
        var btn = new Button
        {
            Text = symbol,
            Width = CaptionButtonWidth,
            Dock = DockStyle.Right,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            Font = new Font("Segoe UI", UITheme.GroupBoxFontSize),
            TabStop = false
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = UITheme.TitleButtonsMouseOver;
        btn.FlatAppearance.MouseDownBackColor = UITheme.TitleButtonsMouseOver;
        return btn;
    }

    private void ToggleMaximize() =>
        WindowState = WindowState == FormWindowState.Maximized
            ? FormWindowState.Normal
            : FormWindowState.Maximized;

    private void TitleBar_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left || WindowState == FormWindowState.Maximized) return;
        _dragging = true;
        _dragStart = Cursor.Position;
        _formStart = Location;
    }

    private void TitleBar_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_dragging) return;
        Location = new Point(
            _formStart.X + Cursor.Position.X - _dragStart.X,
            _formStart.Y + Cursor.Position.Y - _dragStart.Y);
    }

    private GraphicsPath GetRoundedPath(Rectangle bounds, int radius)
    {
        int d = radius * 2;
        GraphicsPath path = new GraphicsPath();
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    private void ApplyRoundedCorners()
    {
        if (CornerRadius <= 0)
        {
            Region = null;
            return;
        }
        using (GraphicsPath path = GetRoundedPath(new Rectangle(0, 0, Width, Height), CornerRadius))
        {
            Region = new Region(path);
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        ApplyRoundedCorners();
        _titleBar.SendToBack();
        base.OnLoad(e);
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        ApplyRoundedCorners();
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        if (_titleLabel != null)
            _titleLabel.Text = Text;
    }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        _titleBar.BackColor = UITheme.TitleDarkBackground;
        _titleLabel.ForeColor = Color.White;
        _minimizeButton.ForeColor = _canMinimize ? Color.White : Color.DimGray;
        _maximizeButton.ForeColor = _canMaximize ? Color.White : Color.DimGray;
        _closeButton.ForeColor = Color.White;
    }

    protected override void OnDeactivate(EventArgs e)
    {
        base.OnDeactivate(e);
        _titleBar.BackColor = UITheme.TitleDarkBackground;
        _titleLabel.ForeColor = Color.Gray;
        _minimizeButton.ForeColor = Color.Gray;
        _maximizeButton.ForeColor = Color.Gray;
        _closeButton.ForeColor = Color.Gray;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (CornerRadius <= 0)
        {
            using (Pen pen = new Pen(Color.FromArgb(0, 120, 215)))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
            return;
        }

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using (GraphicsPath path = GetRoundedPath(new Rectangle(0, 0, Width - 1, Height - 1), CornerRadius))
        using (Pen pen = new Pen(UITheme.TitleDarkBackground))
        {
            e.Graphics.DrawPath(pen, path);
        }
    }

    protected override CreateParams CreateParams
    {
        get
        {
            CreateParams cp = base.CreateParams;
            cp.Style &= ~0x00C00000;
            cp.Style &= ~0x00040000;
            return cp;
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == 0x0083 && m.WParam.ToInt32() == 1) // WM_NCCALCSIZE
        {
            m.Result = IntPtr.Zero;
            return;
        }
        if (m.Msg == 0x0085) // WM_NCPAINT
        {
            m.Result = IntPtr.Zero;
            return;
        }
        if (m.Msg == 0x0086) // WM_NCACTIVATE
        {
            m.Result = (IntPtr)1;
            return;
        }
        base.WndProc(ref m);
    }

    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);
        if (_titleBar != null)
            _titleBar.SetBounds(0, 0, ClientSize.Width, TitleBarHeight);
    }
}
