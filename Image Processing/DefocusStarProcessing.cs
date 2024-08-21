using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
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
                // Calculate center of the image with DPI scaling
                int imgCenterX = (int)(image.Width / 2 );
                int imgCenterY = (int)(image.Height / 2);

                // Find the average radius of the inner transition
                var (innerCentre, innerRadius) = FindAverageInnerRadius(image, imgCenterX, imgCenterY);
                var (outerCentre, outerRadius) = FindAverageOuterRadius(image, imgCenterX, imgCenterY);

                var (Distance, Direction) = CalculateDistanceAndDirection(innerCentre, outerCentre);

                // Send an event with DPI-adjusted values
                DefocusCircleEvent?.Invoke(null, new DefocusCircleEventArgs(image,
                    new Point((int)((innerCentre.X - imgCenterX)), (int)((innerCentre.Y - imgCenterY))),
                    (int)(innerRadius),
                    new Point((int)((outerCentre.X - imgCenterX)), (int)((outerCentre.Y - imgCenterY))),
                    (int)(outerRadius)));
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

        private (Point center, int radius) CalculateAverageRadius(Bitmap image, int centerX, int centerY, bool isInnerRadius)
        {
            int maxRadius = Math.Min(image.Width, image.Height) / 2;
            int radiusSum = 0;
            int transitionCount = 0;
            double transitionXSum = 0;
            double transitionYSum = 0;

            // Precompute trigonometric values
            double[] cosValues = new double[360];
            double[] sinValues = new double[360];

            for (int i = 0; i < 360; i++)
            {
                cosValues[i] = Math.Cos(i * Math.PI / 180);
                sinValues[i] = Math.Sin(i * Math.PI / 180);
            }

            // Lock the bitmap for faster pixel access
            BitmapData bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);
            int bytesPerPixel = Image.GetPixelFormatSize(image.PixelFormat) / 8;
            int stride = bitmapData.Stride;
            IntPtr scan0 = bitmapData.Scan0;
            byte[] pixels = new byte[stride * image.Height];
            System.Runtime.InteropServices.Marshal.Copy(scan0, pixels, 0, pixels.Length);

            for (int angle = 0; angle < 360; angle++)
            {
                double lineSumBrightness = 0;
                int numPoints = 0;

                int startRadius = isInnerRadius ? 0 : maxRadius;
                int endRadius = isInnerRadius ? maxRadius : 0;
                int step = isInnerRadius ? 1 : -1;

                for (int r = startRadius; isInnerRadius ? r < endRadius : r > endRadius; r += step)
                {
                    int x = centerX + (int)(r * cosValues[angle]);
                    int y = centerY + (int)(r * sinValues[angle]);

                    if (x >= 0 && x < image.Width && y >= 0 && y < image.Height)
                    {
                        int pixelIndex = y * stride + x * bytesPerPixel;
                        double brightness = GetBrightness(pixels[pixelIndex + 2], pixels[pixelIndex + 1], pixels[pixelIndex]); // assuming RGB format

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
                    int x = centerX + (int)(r * cosValues[angle]);
                    int y = centerY + (int)(r * sinValues[angle]);

                    if (x >= 0 && x < image.Width && y >= 0 && y < image.Height)
                    {
                        int pixelIndex = y * stride + x * bytesPerPixel;
                        double brightness = GetBrightness(pixels[pixelIndex + 2], pixels[pixelIndex + 1], pixels[pixelIndex]);

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

            // Unlock the bitmap
            image.UnlockBits(bitmapData);

            int radius = transitionCount > 0 ? (int)Math.Round((double)radiusSum / transitionCount) : maxRadius;
            Point center = new Point((int)(transitionXSum / transitionCount + centerX), (int)(transitionYSum / transitionCount + centerY));

            return (center, radius);
        }

        // Helper function to calculate brightness from RGB values
        private double GetBrightness(byte r, byte g, byte b)
        {
            return (r * 0.299 + g * 0.587 + b * 0.114) / 255.0;
        }

        private (Point center, int radius) FindAverageInnerRadius(Bitmap image, int centerX, int centerY)
        {
            return CalculateAverageRadius(image, centerX, centerY, true);
        }

        private (Point center, int radius) FindAverageOuterRadius(Bitmap image, int centerX, int centerY)
        {
            return CalculateAverageRadius(image, centerX, centerY, false);
        }
    }
}

