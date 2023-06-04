
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
                e.Graphics.FillRectangle(new SolidBrush(Color.LightBlue), captureArea);
            }

            if (captureArea != Rectangle.Empty)
            {
                using (Pen pen = new Pen(Color.DarkBlue, 2))
                {
                    e.Graphics.DrawRectangle(pen, captureArea);
                }
            }
        }

        private void Program_MouseDown(object sender, MouseEventArgs e)
        {
            captureArea = new Rectangle(e.Location, Size.Empty);
            clickPoint = new Point(e.X, e.Y);
            isDragging = true;
        }

        private void Program_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                int deltaX = e.Location.X - clickPoint.X;
                int deltaY = e.Location.Y - clickPoint.Y;

                if (deltaX != 0 || deltaY != 0)
                {
                    int distance = Math.Max(Math.Abs(deltaX), Math.Abs(deltaY));

                    captureArea.X = clickPoint.X - distance;
                    captureArea.Y = clickPoint.Y - distance;
                    captureArea.Width = 2 * distance;
                    captureArea.Height = 2 * distance;

                    Invalidate();
                }
            }
        }

        private void Program_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
            Invalidate();
            this.Close();
        }

        public static Bitmap GetScreenImage()
        {
            if(captureArea.Width == 0 || captureArea.Height == 0) 
                return null;

            Bitmap picture = new Bitmap(captureArea.Width, captureArea.Height);

            using (Graphics graphics = Graphics.FromImage((Image)picture))
                graphics.CopyFromScreen(new Point(captureArea.X, captureArea.Y), new Point(0, 0), picture.Size);

            return picture;
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