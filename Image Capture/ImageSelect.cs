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
        /// Draws the current selection as an unfilled circle (stroke only) on the form.
        /// </summary>
        private void DrawSelection(Graphics graphics)
        {
            using (var pen = new Pen(UITheme.SelectionCircleBoarder, UITheme.SelectionBoarderWidth))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.Clear(BackColor);
                if (SelectedArea.Width > 0 && SelectedArea.Height > 0)
                    graphics.DrawEllipse(pen, SelectedArea);
            }
        }

        #endregion
    }
}
