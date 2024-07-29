using System;
using System.Drawing;
using System.Windows.Forms;

namespace ScreenCaptureApp
{
    public class ImageSelect : Form
    {
        // Private fields to manage selection
        private bool selectingArea;
        private Point startPoint;
        public Rectangle SelectedArea { get; private set; }

        /// <summary>
        /// Initializes a new instance of the ImageSelect class.
        /// Configures the form's appearance and registers event handlers.
        /// </summary>
        public ImageSelect()
        {
            InitializeForm();
            RegisterEventHandlers();
        }

        /// <summary>
        /// Configures the form's properties to make it suitable for screen area selection.
        /// Sets the form to be borderless, maximized, semi-transparent, and always on top.
        /// </summary>
        private void InitializeForm()
        {
            FormBorderStyle = FormBorderStyle.None; // Remove window border
            WindowState = FormWindowState.Maximized; // Maximize window to cover entire screen
            BackColor = Color.Black; // Set background color
            Opacity = 0.33; // Set transparency level
            TopMost = true; // Ensure form is above all other windows
            ShowInTaskbar = false; // Hide form from taskbar
        }

        /// <summary>
        /// Registers event handlers for mouse and paint events.
        /// </summary>
        private void RegisterEventHandlers()
        {
            MouseDown += ScreenSelect_MouseDown; // Handle mouse button press
            MouseMove += ScreenSelect_MouseMove; // Handle mouse movement
            MouseUp += ScreenSelect_MouseUp; // Handle mouse button release
            Paint += ScreenSelect_Paint; // Handle paint events for drawing selection
        }

        /// <summary>
        /// Handles the MouseDown event. Initiates the selection process by recording the starting point.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">MouseEventArgs containing details of the mouse event.</param>
        private void ScreenSelect_MouseDown(object sender, MouseEventArgs e)
        {
            selectingArea = true; // Indicate that the user is starting to select an area
            startPoint = e.Location; // Record the initial mouse position
        }

        /// <summary>
        /// Handles the MouseMove event. Updates the selection area as the mouse is moved, if selection is in progress.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">MouseEventArgs containing details of the mouse event.</param>
        private void ScreenSelect_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectingArea)
            {
                UpdateSelectedArea(e.Location); // Update the selection area with current mouse position
                Invalidate(); // Request a repaint to show the updated selection
            }
        }

        /// <summary>
        /// Updates the SelectedArea rectangle based on the current mouse position.
        /// The selection is represented as a circle with the start point as the center.
        /// </summary>
        /// <param name="currentPoint">The current mouse position.</param>
        private void UpdateSelectedArea(Point currentPoint)
        {
            int radius = (int)Math.Sqrt(Math.Pow(currentPoint.X - startPoint.X, 2) + Math.Pow(currentPoint.Y - startPoint.Y, 2));
            SelectedArea = new Rectangle(startPoint.X - radius, startPoint.Y - radius, radius * 2, radius * 2);
        }

        /// <summary>
        /// Handles the MouseUp event. Finalizes the selection process and closes the form.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">MouseEventArgs containing details of the mouse event.</param>
        private void ScreenSelect_MouseUp(object sender, MouseEventArgs e)
        {
            selectingArea = false; // Indicate that the selection process has ended
            DialogResult = DialogResult.OK; // Set the dialog result to OK
            Close(); // Close the form
        }

        /// <summary>
        /// Handles the Paint event. Draws the current selection area on the form.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">PaintEventArgs containing the drawing context.</param>
        private void ScreenSelect_Paint(object sender, PaintEventArgs e)
        {
            if (selectingArea)
                DrawSelection(e.Graphics); // Draw the selection if in selection mode
        }

        /// <summary>
        /// Draws the current selection area as an ellipse on the form.
        /// </summary>
        /// <param name="graphics">The Graphics object used for drawing.</param>
        private void DrawSelection(Graphics graphics)
        {
            using (var brush = new SolidBrush(Color.Gray)) // Brush for filling the ellipse
            using (var pen = new Pen(Color.DarkBlue, 2)) // Pen for drawing the ellipse border
            {
                graphics.Clear(BackColor); // Clear the background with form's back color
                if (SelectedArea.Width > 0 && SelectedArea.Height > 0)
                {
                    graphics.FillEllipse(brush, SelectedArea); // Fill the selection area
                    graphics.DrawEllipse(pen, SelectedArea); // Draw the border around the selection area
                }
            }
        }
    }
}
