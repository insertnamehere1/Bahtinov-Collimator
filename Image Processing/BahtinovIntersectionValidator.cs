using System;
using System.Collections.Generic;
using System.Drawing;

namespace Bahtinov_Collimator
{
    public class BahtinovIntersectionValidator
    {
        private readonly int requiredLines;
        private readonly double tolerancePixels;

        public BahtinovIntersectionValidator(int requiredLines = 6, double tolerancePixels = 20.0)
        {
            if (requiredLines < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(requiredLines), "At least two lines are required.");
            }

            if (tolerancePixels <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tolerancePixels), "Tolerance must be positive.");
            }

            this.requiredLines = requiredLines;
            this.tolerancePixels = tolerancePixels;
        }

        public bool HasMinimumIntersectingLines(BahtinovData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.LineAngles.Length != 9 || data.LineIndex.Length != 9)
            {
                return false;
            }

            Rectangle frame = data.ImageFrame;
            if (frame.Width <= 0 || frame.Height <= 0)
            {
                return false;
            }

            List<Line2D> lines = BuildLines(data, frame);
            if (lines.Count < requiredLines)
            {
                return false;
            }

            double eps = Math.Max(frame.Width, frame.Height) * 1e-12;
            List<(double x, double y)> intersections = CollectIntersections(lines, eps);

            foreach (var (x, y) in intersections)
            {
                int count = 0;
                foreach (var line in lines)
                {
                    if (DistanceToLine(line, x, y) <= tolerancePixels)
                    {
                        count++;
                    }
                }

                if (count >= requiredLines)
                {
                    return true;
                }
            }

            return false;
        }

        private static List<Line2D> BuildLines(BahtinovData data, Rectangle frame)
        {
            int width = frame.Width;
            int height = frame.Height;
            float starImage_X_Centre = (float)(width + 1.0) / 2.0f;
            float starImage_Y_Centre = (float)(height + 1.0) / 2.0f;
            float centrePoint = Math.Min(starImage_X_Centre, starImage_Y_Centre);

            List<Line2D> lines = new List<Line2D>(data.LineAngles.Length);

            for (int index = 0; index < data.LineAngles.Length; index++)
            {
                float angle = data.LineAngles[index];
                float sinAngle = (float)Math.Sin(angle);
                float cosAngle = (float)Math.Cos(angle);
                float offsetY = data.LineIndex[index] - starImage_Y_Centre;

                float lineStart_X = starImage_X_Centre + (-centrePoint * cosAngle) + (offsetY * sinAngle);
                float lineEnd_X = starImage_X_Centre + (centrePoint * cosAngle) + (offsetY * sinAngle);
                float lineStart_Y = starImage_Y_Centre + (-centrePoint * sinAngle) - (offsetY * cosAngle);
                float lineEnd_Y = starImage_Y_Centre + (centrePoint * sinAngle) - (offsetY * cosAngle);

                lines.Add(Line2D.FromPoints(lineStart_X, lineStart_Y, lineEnd_X, lineEnd_Y));
            }

            return lines;
        }

        private static List<(double x, double y)> CollectIntersections(List<Line2D> lines, double eps)
        {
            List<(double x, double y)> intersections = new List<(double x, double y)>();

            for (int i = 0; i < lines.Count; i++)
            {
                for (int j = i + 1; j < lines.Count; j++)
                {
                    var intersection = Line2D.Intersect(lines[i], lines[j], eps);
                    if (intersection.HasValue)
                    {
                        intersections.Add(intersection.Value);
                    }
                }
            }

            return intersections;
        }

        private static double DistanceToLine(Line2D line, double x, double y)
        {
            return Math.Abs(line.A * x + line.B * y + line.C);
        }
    }
}
