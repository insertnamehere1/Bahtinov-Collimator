using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    public class ImageSelect : Form
    {
        #region Private Fields

        private bool selectingArea;
        private Point startPoint;
        private Rectangle _lastSelectionInvalidateBounds;
        public Rectangle SelectedArea { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageSelect"/> class.
        /// Configures the form's appearance and registers event handlers.
        /// </summary>
        public ImageSelect()
        {
            InitializeForm();
            RegisterEventHandlers();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Configures the form's properties to make it suitable for screen area selection.
        /// Sets the form to be borderless, maximized, semi-transparent, and always on top.
        /// </summary>
        private void InitializeForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            BackColor = UITheme.SelectionBackground;
            Opacity = UITheme.SelectionBackgroundTransparency;
            TopMost = true;
            ShowInTaskbar = false;
            DoubleBuffered = true;
        }

        /// <summary>
        /// Registers event handlers for mouse and paint events.
        /// </summary>
        private void RegisterEventHandlers()
        {
            MouseDown += ScreenSelect_MouseDown;
            MouseMove += ScreenSelect_MouseMove;
            MouseUp += ScreenSelect_MouseUp;
            Paint += ScreenSelect_Paint;
        }

        /// <summary>
        /// Handles the MouseDown event. Initiates the selection process by recording the starting point.
        /// </summary>
        private void ScreenSelect_MouseDown(object sender, MouseEventArgs e)
        {
            selectingArea = true;
            startPoint = e.Location;
            _lastSelectionInvalidateBounds = Rectangle.Empty;
        }

        /// <summary>
        /// Handles the MouseMove event. Updates the selection area as the mouse is moved, if selection is in progress.
        /// </summary>
        private void ScreenSelect_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectingArea)
            {
                UpdateSelectedArea(e.Location);
                Rectangle newBounds = GetSelectionInvalidateBounds(SelectedArea);
                if (newBounds.IsEmpty)
                    return;
                Rectangle invalidateRect = _lastSelectionInvalidateBounds.IsEmpty
                    ? newBounds
                    : Rectangle.Union(_lastSelectionInvalidateBounds, newBounds);
                Invalidate(invalidateRect);
                _lastSelectionInvalidateBounds = newBounds;
            }
        }

        /// <summary>
        /// Updates the SelectedArea rectangle based on the current mouse position.
        /// The selection is represented as a circle with the start point as the center.
        /// </summary>
        private void UpdateSelectedArea(Point currentPoint)
        {
            int radius = (int)Math.Sqrt(Math.Pow(currentPoint.X - startPoint.X, 2) + Math.Pow(currentPoint.Y - startPoint.Y, 2));
            SelectedArea = new Rectangle(startPoint.X - radius, startPoint.Y - radius, radius * 2, radius * 2);
        }

        private static Rectangle GetSelectionInvalidateBounds(Rectangle selectedArea)
        {
            if (selectedArea.Width <= 0 || selectedArea.Height <= 0)
                return Rectangle.Empty;
            int pad = (int)Math.Ceiling(UITheme.SelectionBoarderWidth / 2f) + 3;
            Rectangle r = selectedArea;
            r.Inflate(pad, pad);
            return r;
        }

        /// <summary>
        /// Handles the MouseUp event. Finalizes the selection process and closes the form.
        /// </summary>
        private void ScreenSelect_MouseUp(object sender, MouseEventArgs e)
        {
            selectingArea = false;
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Handles the Paint event. Draws the current selection area on the form.
        /// </summary>
        private void ScreenSelect_Paint(object sender, PaintEventArgs e)
        {
            if (selectingArea)
                DrawSelection(e.Graphics);
        }

        /// <summary>
        /// Draws the selection circle with a semi-transparent fill and a radial stroke gradient (transparent inside the band, solid at the outer edge).
        /// </summary>
        private void DrawSelection(Graphics graphics)
        {
            using (var brush = new SolidBrush(UITheme.SelectionCircleFill))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.PixelOffsetMode = PixelOffsetMode.Half;
                graphics.Clear(BackColor);
                if (SelectedArea.Width > 0 && SelectedArea.Height > 0)
                {
                    graphics.FillEllipse(brush, SelectedArea);
                    DrawSelectionStrokeGradient(graphics, SelectedArea);
                }
            }
        }

        private static void DrawSelectionStrokeGradient(Graphics graphics, Rectangle bounds)
        {
            Color border = UITheme.SelectionCircleBoarder;
            float halfStroke = UITheme.SelectionBoarderWidth / 2f;
            float cx = bounds.X + bounds.Width / 2f;
            float cy = bounds.Y + bounds.Height / 2f;
            float w = bounds.Width;
            float h = bounds.Height;

            float innerW = Math.Max(0.5f, w - 2f * halfStroke);
            float innerH = Math.Max(0.5f, h - 2f * halfStroke);
            float outerW = w + 2f * halfStroke;
            float outerH = h + 2f * halfStroke;

            int steps = Math.Max(2, UITheme.SelectionCircleStrokeGradientSteps);
            float penWidth = Math.Max(1f, UITheme.SelectionBoarderWidth / (float)steps);

            for (int i = 0; i < steps; i++)
            {
                float u = (i + 0.5f) / steps;
                float tw = innerW + (outerW - innerW) * u;
                float th = innerH + (outerH - innerH) * u;
                int alpha = (int)(255 * u);
                using (var pen = new Pen(Color.FromArgb(alpha, border.R, border.G, border.B), penWidth))
                {
                    var rect = new RectangleF(cx - tw / 2f, cy - th / 2f, tw, th);
                    graphics.DrawEllipse(pen, rect);
                }
            }
        }

        #endregion
    }
}
