using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MathNet.Numerics;

namespace Bahtinov_Collimator
{
    internal class Helper
    {

        public bool AreLineIndicesValid(BahtinovData bahtinovLines, int imageHeight)
        {
            return bahtinovLines.LineIndex.All(lineIndex => lineIndex >= 0 && lineIndex < imageHeight);
        }

        public (float X, float Y) GetImageCentre(Bitmap image)
        {
            return ((image.Width + 1) / 2.0f, (image.Height + 1) / 2.0f);
        }

        public void SwapFirstAndThirdLinesIfNeeded(BahtinovData bahtinovLines)
        {
            if (Math.Abs(bahtinovLines.LineAngles[0] - 1.5708) < 0.0001)
            {
                (bahtinovLines.LineAngles[0], bahtinovLines.LineAngles[2]) = (bahtinovLines.LineAngles[2], bahtinovLines.LineAngles[0]);
                (bahtinovLines.LineIndex[0], bahtinovLines.LineIndex[2]) = (bahtinovLines.LineIndex[2], bahtinovLines.LineIndex[0]);
            }
        }

        public List<Line> CalculateLineCoordinates(BahtinovData bahtinovLines, (float X, float Y) imageCentre)
        {
            var lines = new List<Line>();
            float centrePoint = Math.Min(imageCentre.X, imageCentre.Y);

            for (int i = 0; i < bahtinovLines.LineAngles.Length; i++)
            {
                var angle = bahtinovLines.LineAngles[i];
                var index = bahtinovLines.LineIndex[i];
                var line = CalculateLine(angle, index, imageCentre, centrePoint);
                lines.Add(line);

                if (i == 0)
                {
                    line.Slope = CalculateSlope(line);
                    line.CalculatedYStart = CalculateYIntercept(line);
                }
                else if (i == 2)
                {
                    line.Slope = CalculateSlope(line);
                    line.CalculatedYStart = CalculateYIntercept(line);
                }
            }

            return lines;
        }

        public Line CalculateLine(float angle, float index, (float X, float Y) centre, float centrePoint)
        {
            float startX = (float)(centre.X - centrePoint * Math.Cos(angle) + ((index - centre.Y) * Math.Sin(angle)));
            float endX = (float)(centre.X + centrePoint * Math.Cos(angle) + (index - centre.Y) * Math.Sin(angle));
            float startY = (float)(centre.Y - centrePoint * Math.Sin(angle) - (index - centre.Y) * Math.Cos(angle));
            float endY = (float)(centre.Y + centrePoint * Math.Sin(angle) - (index - centre.Y) * Math.Cos(angle));

            return new Line(startX, startY, endX, endY);
        }

        public float CalculateSlope(Line line)
        {
            return (line.EndY - line.StartY) / (line.EndX - line.StartX);
        }

        public float CalculateYIntercept(Line line)
        {
            return -line.StartX * (line.EndY - line.StartY) / (line.EndX - line.StartX) + line.StartY;
        }

        public void DrawLines(List<Line> lines, int group, Graphics graphics, bool[] showFocus)
        {
            Pen dashPen = CreatePen(group, true);
            Pen largeDashPen = CreatePen(group, false);

            foreach (var line in lines)
            {
                if (showFocus[group])
                {
                    var pen = line.IsFirstOrThird ? dashPen : largeDashPen;
                    graphics.DrawLine(pen, line.StartX, line.StartY, line.EndX, line.EndY);
                }
            }
        }

        public void DisplayCalculatedValues(List<Line> lines, int group, Graphics graphics, GroupBox[] groupBoxes,
                                    float lineAngle1, float lineAngle3,
                                    float apertureSetting, float focalLengthSetting, float pixelSizeSetting)
        {
            if (group < 0 || group >= groupBoxes.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(group), "Group index is out of range.");
            }

            GroupBox selectedGroupBox = groupBoxes[group];
            Label focusErrorLabel = selectedGroupBox.Controls["FocusErrorLabel" + (group + 1)] as Label;
            Label absoluteFocusErrorLabel = selectedGroupBox.Controls["AbsoluteFocusErrorLabel" + (group + 1)] as Label;
            Label withinCriticalFocusLabel = selectedGroupBox.Controls["WithinCriticalFocusLabel" + (group + 1)] as Label;

            PointF? intersection = CalculateIntersection(lines[0], lines[2]);
            var result = ClosestDistanceBetweenPointAndLine(intersection, lines[1]);

            if (focusErrorLabel != null)
            {
                float focusError = CalculateFocusError(result.Distance, intersection.Value, lines[1]);
                focusErrorLabel.Text = focusError.ToString("F1");
                Form1.errorValues[group] = focusError;
            }

            float focusErrorInMicrons = 0;

            if (absoluteFocusErrorLabel != null)
            {
                focusErrorInMicrons = CalculateFocusErrorInMicrons(result.Distance, lineAngle1, lineAngle3, apertureSetting, focalLengthSetting, pixelSizeSetting);
                absoluteFocusErrorLabel.Text = focusErrorInMicrons.ToString("F1");
            }

            if (withinCriticalFocusLabel != null)
            {
                bool withinCriticalFocus = IsWithinCriticalFocus(focusErrorInMicrons, apertureSetting, focalLengthSetting);
                withinCriticalFocusLabel.ForeColor = withinCriticalFocus ? Color.Green : Color.Red;
                withinCriticalFocusLabel.Text = withinCriticalFocus ? "Yes" : "No";
            }
        }

        private float CalculateFocusError(float errorDistance, PointF intersection, Line line)
        {
            float errorDistanceX = intersection.X - line.StartX;
            float errorDistanceY = intersection.Y - line.StartY;
            float line2XDistance = line.EndX - line.StartX;
            float line2YDistance = line.EndY - line.StartY;
            float errorSign = (float)-Math.Sign(errorDistanceX * line2YDistance - errorDistanceY * line2XDistance);
            return errorSign * errorDistance;
        }

        private float CalculateFocusErrorInMicrons(float errorDistance, float lineAngle1, float lineAngle3,
                                                   float apertureSetting, float focalLengthSetting, float pixelSizeSetting)
        {
            float bahtinovAngle = Math.Abs((lineAngle3 - lineAngle1) / 2.0f);
            float radianPerDegree = (float)Math.PI / 180f;
            float pixelsPerMicron = (float)(9.0f / 32.0f * (apertureSetting / 1000.0f) / (focalLengthSetting / 1000.0f * pixelSizeSetting) *
                                     (1.0f + Math.Cos(45.0f * radianPerDegree) * (1.0f + Math.Tan(bahtinovAngle))));
            return errorDistance / pixelsPerMicron;
        }

        private bool IsWithinCriticalFocus(float focusErrorInMicrons, float apertureSetting, float focalLengthSetting)
        {
            float criticalFocusValue = (float)(8.99999974990351E-07 * (focalLengthSetting / 1000.0f) / (apertureSetting / 1000.0f) *
                                               (focalLengthSetting / 1000.0f) / (apertureSetting / 1000.0f));
            return Math.Abs(focusErrorInMicrons * 1E-06) < Math.Abs(criticalFocusValue);
        }

        public static (float Distance, Line ClosestLine) ClosestDistanceBetweenPointAndLine(PointF? intersection, Line line)
        {
            PointF closestPoint;
            float distance = PerpendicularDistanceToLine(intersection.Value.X, intersection.Value.Y, line, out closestPoint);

            // Define the line segment for the closest distance
            Line closestLine = new Line(closestPoint.X, closestPoint.Y, intersection.Value.X, intersection.Value.Y);

            return (distance, closestLine);
        }

        public static float PerpendicularDistanceToLine(float px, float py, Line line, out PointF closestPoint)
        {
            // Line equation: Ax + By + C = 0
            float A = line.StartY - line.EndY;
            float B = line.EndX - line.StartX;
            float C = line.StartX * line.EndY - line.EndX * line.StartY;

            // Calculate the perpendicular distance
            float distance = Math.Abs(A * px + B * py + C) / (float)Math.Sqrt(A * A + B * B);

            // Calculate the closest point on the line
            float x = (B * (B * px - A * py) - A * C) / (A * A + B * B);
            float y = (A * (-B * px + A * py) - B * C) / (A * A + B * B);

            closestPoint = new PointF(x, y);

            return distance;
        }

        public static PointF? CalculateIntersection(Line line1, Line line2)
        {
            if (line1.Slope == line2.Slope)
            {
                // Parallel lines
                return null;
            }

            float xIntersection = (line2.Intercept - line1.Intercept) / (line1.Slope - line2.Slope);
            float yIntersection = line1.Slope * xIntersection + line1.Intercept;

            return new PointF(xIntersection, yIntersection);
        }

        public Pen CreatePen(int group, bool isDash)
        {
            Color color;
            if (group == 0)
                color = Color.Red;
            else if (group == 1)
                color = Color.Green;
            else if (group == 2)
                color = Color.Blue;
            else
                color = Color.White;

            return new Pen(color, 2) { DashStyle = isDash ? DashStyle.Dash : DashStyle.Solid };
        }

        public void DrawErrorCircle(List<Line> lines, int group, Graphics graphics)
        {
            // Implement focus label updating logic
        }

        public void UpdateFocusLabels(List<Line> lines, int group)
        {
            // Implement focus label updating logic
        }

        public class Line
        {
            public float StartX { get; }
            public float StartY { get; }
            public float EndX { get; }
            public float EndY { get; }
            public float Slope { get; set; }
            public float Intercept { get; private set; }
            public bool IsFirstOrThird { get; }
            public float CalculatedYStart {  get; set; }

            public Line(float startX, float startY, float endX, float endY, bool isFirstOrThird = false)
            {
                StartX = startX;
                StartY = startY;
                EndX = endX;
                EndY = endY;
                IsFirstOrThird = isFirstOrThird;

                // Calculate Slope and Intercept
                if (EndX != StartX) // Avoid division by zero
                {
                    Slope = (EndY - StartY) / (EndX - StartX);
                    Intercept = StartY - (Slope * StartX);
                }
                else
                {
                    // Handle vertical lines if needed
                    Slope = float.PositiveInfinity;
                    Intercept = float.NaN; // Vertical line does not have a y-intercept
                }
            }
        }
    }
}
