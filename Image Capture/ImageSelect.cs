using System;
using System.Drawing;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    public class ImageSelect : Form
    {
        #region Private Fields

        private bool selectingArea;
        private Point startPoint;
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
        }

        /// <summary>
        /// Handles the MouseMove event. Updates the selection area as the mouse is moved, if selection is in progress.
        /// </summary>
        private void ScreenSelect_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectingArea)
            {
                UpdateSelectedArea(e.Location);
                Invalidate();
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
        /// Draws the current selection area as an ellipse on the form.
        /// </summary>
        private void DrawSelection(Graphics graphics)
        {
            using (var brush = new SolidBrush(UITheme.SelectionCircleInfill))
            using (var pen = new Pen(UITheme.SelectionCircleBoarder, UITheme.SelectionBoarderWidth))
            {
                graphics.Clear(BackColor);
                if (SelectedArea.Width > 0 && SelectedArea.Height > 0)
                {
                    graphics.FillEllipse(brush, SelectedArea);
                    graphics.DrawEllipse(pen, SelectedArea);
                }
            }
        }

        #endregion
    }
}
