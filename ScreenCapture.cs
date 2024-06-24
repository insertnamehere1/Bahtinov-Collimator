
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    public class ScreenCap : Form
    {
        private static Rectangle captureArea;
        private bool isDragging;
        private Point clickPoint;

        public ScreenCap()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.Opaque, true);
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            Opacity = 0.33;

            MouseDown += Program_MouseDown;
            MouseMove += Program_MouseMove;
            MouseUp += Program_MouseUp;

            captureArea = new Rectangle();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using (SolidBrush brush = new SolidBrush(Color.Black))
            {
                e.Graphics.Clear(this.BackColor);
                if (captureArea.Width > 0 && captureArea.Height > 0)
                {
                    e.Graphics.FillEllipse(new SolidBrush(Color.LightBlue), captureArea);
                }
            }

            if (captureArea.Width > 0 && captureArea.Height > 0)
            {
                using (Pen pen = new Pen(Color.DarkBlue, 2))
                {
                    e.Graphics.DrawEllipse(pen, captureArea);
                }
            }
        }

        private void Program_MouseDown(object sender, MouseEventArgs e)
        {
            clickPoint = e.Location;
            captureArea = new Rectangle(clickPoint.X, clickPoint.Y, 0, 0);
            isDragging = true;
        }

        private void Program_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                int radius = (int)Math.Sqrt(Math.Pow(e.X - clickPoint.X, 2) + Math.Pow(e.Y - clickPoint.Y, 2));
                captureArea = new Rectangle(clickPoint.X - radius, clickPoint.Y - radius, radius * 2, radius * 2);

                Invalidate();
            }
        }

        private void Program_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
            Invalidate();
            this.Close();
        }

        public static float GetScalingFactor()
        {
            using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                // Get the current DPI
                float dpiX = graphics.DpiX;
                // Calculate the scaling factor
                return dpiX / 96f;
            }
        }

        public static Bitmap GetScreenImage()
        {
            if (captureArea.Width == 0 || captureArea.Height == 0)
                return null;

            Bitmap picture = new Bitmap(captureArea.Width, captureArea.Height);

            using (Graphics graphics = Graphics.FromImage(picture))
            {
                graphics.CopyFromScreen(new Point(captureArea.X, captureArea.Y), new Point(0, 0), picture.Size);
            }

            Bitmap circularImage = new Bitmap(picture.Width, picture.Height);

            using (Graphics g = Graphics.FromImage(circularImage))
            {
                g.Clear(Color.Transparent);
                using (Brush brush = new TextureBrush(picture))
                {
                    g.FillEllipse(brush, 0, 0, picture.Width, picture.Height);
                }
            }

            return circularImage;
        }

        public Rectangle GetImageCoordinates()
        {
            return captureArea;
        }

        public void SetImageCoordinates(Rectangle area)
        {
            captureArea = area;
        }

        public void Reset()
        {
            captureArea = Rectangle.Empty;
            isDragging = false;
            Invalidate();
        }
    }
}