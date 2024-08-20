using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bahtinov_Collimator.Custom_Components
{
    public partial class SlideSwitch : UserControl
    {
        private bool isOn;
        private Rectangle toggleRectangle;
        private Color onColor = UITheme.GetGroupBoxTextColor(0);
        private Color offColor = Color.Gray;
        private Color toggleColor = Color.White;

        public SlideSwitch()
        {
            InitializeComponent();
            this.Size = new Size(50, 25);
            this.toggleRectangle = new Rectangle(2, 2, this.Height - 4, this.Height - 4);
            this.isOn = false;
            this.DoubleBuffered = true;
            this.Click += SlideSwitch_Click;
        }

        [Category("Appearance")]
        public Color OnColor
        {
            get { return onColor; }
            set { onColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color OffColor
        {
            get { return offColor; }
            set { offColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color ToggleColor
        {
            get { return toggleColor; }
            set { toggleColor = value; Invalidate(); }
        }

        [Category("Behavior")]
        public bool IsOn
        {
            get { return isOn; }
            set
            {
                isOn = value;
                Invalidate(); // Redraw control
                OnToggleChanged(EventArgs.Empty);
            }
        }

        public event EventHandler ToggleChanged;

        protected virtual void OnToggleChanged(EventArgs e)
        {
            ToggleChanged?.Invoke(this, e);
        }

        private void SlideSwitch_Click(object sender, EventArgs e)
        {
            IsOn = !IsOn;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Background
            int backgroundCornerRadius = this.Height / 2;
            using (GraphicsPath backgroundPath = new GraphicsPath())
            {
                backgroundPath.AddArc(0, 0, backgroundCornerRadius, backgroundCornerRadius, 180, 90);
                backgroundPath.AddArc(this.Width - backgroundCornerRadius, 0, backgroundCornerRadius, backgroundCornerRadius, 270, 90);
                backgroundPath.AddArc(this.Width - backgroundCornerRadius, this.Height - backgroundCornerRadius, backgroundCornerRadius, backgroundCornerRadius, 0, 90);
                backgroundPath.AddArc(0, this.Height - backgroundCornerRadius, backgroundCornerRadius, backgroundCornerRadius, 90, 90);
                backgroundPath.CloseFigure();

                using (SolidBrush brush = new SolidBrush(isOn ? onColor : offColor))
                {
                    g.FillPath(brush, backgroundPath);
                }
            }

            // Toggle
            int toggleX = isOn ? this.Width - this.Height + 2 : 2;
            this.toggleRectangle = new Rectangle(toggleX, 2, this.Height - 4, this.Height - 4);

            int toggleCornerRadius = this.toggleRectangle.Height / 2;

            using (GraphicsPath togglePath = new GraphicsPath())
            {
                togglePath.AddArc(this.toggleRectangle.X, this.toggleRectangle.Y, toggleCornerRadius, toggleCornerRadius, 180, 90);
                togglePath.AddArc(this.toggleRectangle.Right - toggleCornerRadius, this.toggleRectangle.Y, toggleCornerRadius, toggleCornerRadius, 270, 90);
                togglePath.AddArc(this.toggleRectangle.Right - toggleCornerRadius, this.toggleRectangle.Bottom - toggleCornerRadius, toggleCornerRadius, toggleCornerRadius, 0, 90);
                togglePath.AddArc(this.toggleRectangle.X, this.toggleRectangle.Bottom - toggleCornerRadius, toggleCornerRadius, toggleCornerRadius, 90, 90);
                togglePath.CloseFigure();

                using (SolidBrush brush = new SolidBrush(toggleColor))
                {
                    g.FillPath(brush, togglePath);
                }
            }
        }
    }
}
