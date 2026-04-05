using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    public class ImageSelect : Form
    {
        private const int InstructionOuterPad = 20;
        private const int InstructionInnerPad = 18;

        #region Private Fields

        private bool selectingArea;
        private Point startPoint;
        private Rectangle _lastSelectionInvalidateBounds;
        private Rectangle _instructionPanelBounds;
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

        #region Layout

        /// <summary>
        /// Top-left instruction panel size (for invalidation so it stays visible during partial repaints).
        /// </summary>
        private void UpdateInstructionPanelBounds()
        {
            int maxPanelW = Math.Min(800, Math.Max(120, ClientSize.Width - 2 * InstructionOuterPad));
            int maxTextWidth = Math.Max(40, maxPanelW - 2 * InstructionInnerPad);
            string text = UiText.Current.ImageCaptureSelectionOverlayInstructions;
            TextFormatFlags measureFlags = TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl | TextFormatFlags.NoPadding;
            Size textSize = TextRenderer.MeasureText(
                text,
                Font,
                new Size(maxTextWidth, int.MaxValue),
                measureFlags);

            int panelW = textSize.Width + 2 * InstructionInnerPad;
            int panelH = textSize.Height + 2 * InstructionInnerPad;
            int capW = Math.Max(1, ClientSize.Width - 2 * InstructionOuterPad);
            int capH = Math.Max(1, ClientSize.Height - 2 * InstructionOuterPad);
            panelW = Math.Min(panelW, capW);
            panelH = Math.Min(panelH, capH);

            _instructionPanelBounds = new Rectangle(InstructionOuterPad, InstructionOuterPad, panelW, panelH);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateInstructionPanelBounds();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            UpdateInstructionPanelBounds();
            Invalidate();
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
            Font = new Font("Segoe UI", 12.5f, FontStyle.Regular, GraphicsUnit.Point);
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
                if (!_instructionPanelBounds.IsEmpty)
                    invalidateRect = Rectangle.Union(invalidateRect, _instructionPanelBounds);
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
        /// Handles the Paint event: dim overlay, selection circle while dragging, then instructions on top
        /// so the semi-transparent circle does not cover the help panel.
        /// </summary>
        private void ScreenSelect_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            if (selectingArea)
                DrawSelection(e.Graphics);
            DrawInstructionPanel(e.Graphics);
        }

        /// <summary>
        /// Draws localized instructions in the top-left corner with a white border around the text area.
        /// </summary>
        private void DrawInstructionPanel(Graphics graphics)
        {
            if (_instructionPanelBounds.Width <= 0 || _instructionPanelBounds.Height <= 0)
                return;

            string text = UiText.Current.ImageCaptureSelectionOverlayInstructions;
            var textRect = new Rectangle(
                _instructionPanelBounds.X + InstructionInnerPad,
                _instructionPanelBounds.Y + InstructionInnerPad,
                _instructionPanelBounds.Width - 2 * InstructionInnerPad,
                _instructionPanelBounds.Height - 2 * InstructionInnerPad);

            using (var surround = new Pen(UITheme.White, 2))
            {
                graphics.DrawRectangle(
                    surround,
                    _instructionPanelBounds.X,
                    _instructionPanelBounds.Y,
                    _instructionPanelBounds.Width - 1,
                    _instructionPanelBounds.Height - 1);
            }

            TextRenderer.DrawText(
                graphics,
                text,
                Font,
                textRect,
                UITheme.MenuStripForeground,
                TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl | TextFormatFlags.Top | TextFormatFlags.NoPadding);
        }

        /// <summary>
        /// Draws the selection circle with a semi-transparent fill and a radial stroke gradient (transparent inside the band, solid at the outer edge).
        /// </summary>
        private void DrawSelection(Graphics graphics)
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.PixelOffsetMode = PixelOffsetMode.Half;
            if (SelectedArea.Width > 0 && SelectedArea.Height > 0)
            {
                using (var brush = new SolidBrush(UITheme.SelectionCircleFill))
                {
                    graphics.FillEllipse(brush, SelectedArea);
                }
                DrawSelectionStrokeGradient(graphics, SelectedArea);
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
