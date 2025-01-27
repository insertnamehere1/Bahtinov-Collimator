using System;
using System.Drawing.Drawing2D;
using System.Drawing;

namespace Bahtinov_Collimator.AdjustAssistant
{
    /// <summary>
    /// Provides an adjustment assistant specifically designed for Schmidt-Cassegrain telescopes (SCTs).
    /// Inherits functionality from the AdjustAssistantBase class.
    /// </summary>
    internal class AdjustAssistSCT : AdjustAssistBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdjustAssistSCT"/> class with the specified parent form.
        /// </summary>
        /// <param name="parent">The parent form that this assistant operates on.</param>
        public AdjustAssistSCT(Form1 parent) : base(parent)
        {
            this.Text = "Adjustment Assistant for Schmidt–Cassegrain (SCT)";
        }

        /// <summary>
        /// Handles the focus data event, updating the focus error values for red, green, 
        /// and blue channels based on the incoming data. If the event is not on the UI thread, 
        /// the method invokes itself on the correct thread. Also triggers a redraw of the PictureBox.
        /// </summary>
        /// <param name="sender">The source of the event, typically the object that raised the event.</param>
        /// <param name="e">A <see cref="FocusDataEventArgs"/> object containing the focus data to be processed.</param>
        public override void FocusDataEvent(object sender, FocusDataEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object, FocusDataEventArgs>(FocusDataEvent), sender, e);
                return;
            }

            int groupId = e.FocusData.Id;

            // Is this a clear event?
            if (groupId != -1)
            {
                if (!e.FocusData.ClearDisplay)
                {
                    if (groupId == 0)
                        redError = (float)e.FocusData.BahtinovOffset;
                    else if (groupId == 1)
                        greenError = (float)e.FocusData.BahtinovOffset;
                    else if (groupId == 2)
                        blueError = (float)e.FocusData.BahtinovOffset;
                }
                else
                {
                    redError = 0.0f;
                    greenError = 0.0f;
                    blueError = 0.0f;
                }
            }
            pictureBox1.Invalidate();
        }

        /// <summary>
        /// Draws the contents of the PictureBox, including the arrows and circles 
        /// that represent focus adjustment states for each color channel.
        /// </summary>
        /// <param name="g">The <see cref="Graphics"/> object used to perform drawing operations.</param>
        protected override void DrawPictureBox(Graphics g)
        {
            Arrow redArrow = Arrow.None;
            Arrow greenArrow = Arrow.None;
            Arrow blueArrow = Arrow.None;

            int centerX = pictureBox1.Width / 2;
            int centerY = pictureBox1.Height / 2;

            // Define the colors for the shading effect
            Color startColor = UITheme.AdjustAssistKnobLo;
            Color endColor = UITheme.AdjustAssistKnobHi;

            // Create a linear gradient brush to apply shading
            Brush shadingBrush = new LinearGradientBrush(
                new Point(centerX - circleRadius, centerY - circleRadius),
                new Point(centerX + circleRadius, centerY + circleRadius),
                startColor, endColor);

            using (SolidBrush bgBrush = new SolidBrush(UITheme.DarkBackground))
            using (SolidBrush circleBrush = new SolidBrush(Color.DarkGray))
            using (Pen circlePen = new Pen(Color.Black, 2.0f))
            {
                g.FillRectangle(bgBrush, ClientRectangle);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                int circleSize = circleRadius * 2;
                int circleX = centerX - circleRadius;
                int circleY = centerY - circleRadius;
                g.FillEllipse(shadingBrush, circleX, circleY, circleSize, circleSize);
                g.DrawEllipse(circlePen, circleX, circleY, circleSize, circleSize);

                redArrow = GetArrow(redError, redReverseBox.Checked);
                greenArrow = GetArrow(greenError, greenReverseBox.Checked);
                blueArrow = GetArrow(blueError, blueReverseBox.Checked);

                DrawSmallCircle(g, centerX, centerY, circleRadius, -90, UITheme.GetGroupBoxTextColor(0), redArrow, (float)redError);

                if (swapGreenCheckbox.Checked)
                {
                    DrawSmallCircle(g, centerX, centerY, circleRadius, -210, UITheme.GetGroupBoxTextColor(1), greenArrow, greenError);
                    DrawSmallCircle(g, centerX, centerY, circleRadius, -330, UITheme.GetGroupBoxTextColor(2), blueArrow, blueError);
                }
                else
                {
                    DrawSmallCircle(g, centerX, centerY, circleRadius, -210, UITheme.GetGroupBoxTextColor(2), blueArrow, blueError);
                    DrawSmallCircle(g, centerX, centerY, circleRadius, -330, UITheme.GetGroupBoxTextColor(1), greenArrow, greenError);
                }
            }
        }
    }
}

