using System;
using System.Drawing;
using System.Drawing.Imaging;
using static Bahtinov_Collimator.BahtinovLineDataEventArgs;
using Bahtinov_Collimator.Helper;

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
                double imgCenterX = image.Width / 2;
                double imgCenterY = image.Height / 2;

                // Find the average radius of the inner transition
                var (innerCentre, innerRadius) = FindAverageInnerRadius(image, imgCenterX, imgCenterY);
                var (outerCentre, outerRadius) = FindAverageOuterRadius(image, imgCenterX, imgCenterY);

                var (distance, direction) = CalculateDistanceAndDirection(innerCentre, outerCentre, innerRadius);

                // Send an event with DPI-adjusted values
                DefocusCircleEvent?.Invoke(null, new DefocusCircleEventArgs(image,
                    new PointD(innerCentre.X - imgCenterX, innerCentre.Y - imgCenterY), innerRadius,
                    new PointD(outerCentre.X - imgCenterX, outerCentre.Y - imgCenterY), outerRadius, 
                    distance, direction)); 
            }
        }

        private static (double Distance, double Direction) CalculateDistanceAndDirection(PointD innerCentre, PointD outerCentre, double innerRadius)
        {
            double deltaX = outerCentre.X - innerCentre.X;
            double deltaY = outerCentre.Y - innerCentre.Y;

            double distance = ((innerCentre.DistanceTo(outerCentre))/innerRadius) * 50;
            double directionRadians = innerCentre.DirectionTo(outerCentre);  
            double directionDegrees = directionRadians * (180.0 / Math.PI);

            if (directionDegrees < 0)
                directionDegrees += 360.0;

            return (distance, directionDegrees);
        }

        private (PointD centre, double radius) CalculateAverageRadius(Bitmap image, double centerX, double centerY, bool isInnerRadius)
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
                    double x = centerX + (r * cosValues[angle]);
                    double y = centerY + (r * sinValues[angle]);

                    if (x >= 0 && x < image.Width && y >= 0 && y < image.Height)
                    {
                        int pixelIndex = (int)(y) * stride + (int)(x) * bytesPerPixel;
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
                    double x = centerX + (r * cosValues[angle]);
                    double y = centerY + (r * sinValues[angle]);

                    if (x >= 0 && x < image.Width && y >= 0 && y < image.Height)
                    {
                        int pixelIndex = (int)(y) * stride + (int)(x) * bytesPerPixel;
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

            double radius = transitionCount > 0 ? Math.Round((double)radiusSum / transitionCount) : maxRadius;
            double circleX = transitionXSum / (transitionCount / 2) + centerX;
            double circleY = transitionYSum / (transitionCount / 2) + centerY;

            return (new PointD(circleX, circleY), radius);
        }

        private double GetBrightness(byte r, byte g, byte b)
        {
            return (r * 0.299 + g * 0.587 + b * 0.114) / 255.0;
        }

        private (PointD centre, double radius) FindAverageInnerRadius(Bitmap image, double centerX, double centerY)
        {
            return CalculateAverageRadius(image, centerX, centerY, true);
        }

        private (PointD centre, double radius) FindAverageOuterRadius(Bitmap image, double centerX, double centerY)
        {
            return CalculateAverageRadius(image, centerX, centerY, false);
        }
    }
}

