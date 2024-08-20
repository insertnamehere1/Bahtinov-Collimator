using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static Bahtinov_Collimator.BahtinovLineDataEventArgs;

namespace Bahtinov_Collimator.Image_Processing
{
    internal class DefocusStarProcessing
    {
        public delegate void DefocusCircleEventHandler(object sender, DefocusCircleEventArgs e);
        public static event DefocusCircleEventHandler DefocusCircleEvent;

        public DefocusStarProcessing() { }

        public void DisplayDefocusImage(Bitmap image)
        {
            if (image == null)
                return;

            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                // Get the DPI scaling factor
                float dpiX = 1 / UITheme.DpiScaleX;
                float dpiY = 1 / UITheme.DpiScaleY;

                // Calculate center of the image with DPI scaling
                int imgCenterX = (int)(image.Width / 2 * dpiX);
                int imgCenterY = (int)(image.Height / 2 * dpiY);

                // Find the average radius of the inner transition
                var (innerCentre, innerRadius) = FindAverageInnerRadius(image, imgCenterX, imgCenterY, dpiX, dpiY);
                var (outerCentre, outerRadius) = FindAverageOuterRadius(image, imgCenterX, imgCenterY, dpiX, dpiY);

                var (Distance, Direction) = CalculateDistanceAndDirection(innerCentre, outerCentre);

                // Send an event with DPI-adjusted values
                DefocusCircleEvent?.Invoke(null, new DefocusCircleEventArgs(image,
                    new Point((int)((innerCentre.X - imgCenterX) / dpiX), (int)((innerCentre.Y - imgCenterY) / dpiY)),
                    (int)(innerRadius / dpiX),
                    new Point((int)((outerCentre.X - imgCenterX) / dpiX), (int)((outerCentre.Y - imgCenterY) / dpiY)),
                    (int)(outerRadius / dpiX)));
            }
        }

        private static (double Distance, double Direction) CalculateDistanceAndDirection(Point firstPoint, Point secondPoint)
        {
            double deltaX = secondPoint.X - firstPoint.X;
            double deltaY = secondPoint.Y - firstPoint.Y;

            double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
            double directionRadians = Math.Atan2(deltaY, deltaX);
            double directionDegrees = directionRadians * (180.0 / Math.PI);

            if (directionDegrees < 0)
                directionDegrees += 360.0;

            return (distance, directionDegrees);
        }

        private (Point center, int radius) CalculateAverageRadius(Bitmap image, int centerX, int centerY, bool isInnerRadius, float dpiX, float dpiY)
        {
            int maxRadius = (int)(Math.Min(image.Width, image.Height) / 2 * Math.Min(dpiX, dpiY));
            int radiusSum = 0;
            int transitionCount = 0;
            double transitionXSum = 0;
            double transitionYSum = 0;

            for (double angle = 0; angle < 360; angle += 1)
            {
                double lineSumBrightness = 0;
                int numPoints = 0;

                int startRadius = isInnerRadius ? 0 : maxRadius;
                int endRadius = isInnerRadius ? maxRadius : 0;
                int step = isInnerRadius ? 1 : -1;

                for (int r = startRadius; isInnerRadius ? r < endRadius : r > endRadius; r += step)
                {
                    int x = centerX + (int)(r * Math.Cos(angle * Math.PI / 180) * dpiX);
                    int y = centerY + (int)(r * Math.Sin(angle * Math.PI / 180) * dpiY);

                    if (x >= 0 && x < image.Width && y >= 0 && y < image.Height)
                    {
                        Color pixelColor = image.GetPixel(x, y);
                        double brightness = pixelColor.GetBrightness();

                        if (brightness != 0)
                        {
                            lineSumBrightness += brightness;
                            numPoints++;
                        }
                    }
                }

                double averageLineBrightness = numPoints > 0 ? lineSumBrightness / numPoints : 0.0;

                for (int r = startRadius; isInnerRadius ? r < endRadius : r > endRadius; r += step)
                {
                    int x = centerX + (int)(r * Math.Cos(angle * Math.PI / 180) * dpiX);
                    int y = centerY + (int)(r * Math.Sin(angle * Math.PI / 180) * dpiY);

                    if (x >= 0 && x < image.Width && y >= 0 && y < image.Height)
                    {
                        Color pixelColor = image.GetPixel(x, y);
                        double brightness = pixelColor.GetBrightness();

                        if (brightness > averageLineBrightness)
                        {
                            radiusSum += r;
                            transitionCount++;
                            transitionXSum += x - centerX;
                            transitionYSum += y - centerY;
                            break;
                        }
                    }
                }
            }

            int radius = transitionCount > 0 ? (int)Math.Round((double)radiusSum / transitionCount) : maxRadius;
            Point center = new Point((int)(transitionXSum / transitionCount + centerX), (int)(transitionYSum / transitionCount + centerY));

            return (center, radius);
        }

        private (Point center, int radius) FindAverageInnerRadius(Bitmap image, int centerX, int centerY, float dpiX, float dpiY)
        {
            return CalculateAverageRadius(image, centerX, centerY, true, dpiX, dpiY);
        }

        private (Point center, int radius) FindAverageOuterRadius(Bitmap image, int centerX, int centerY, float dpiX, float dpiY)
        {
            return CalculateAverageRadius(image, centerX, centerY, false, dpiX, dpiY);
        }
    }
}

