using System;
using System.Drawing;
using System.Drawing.Imaging;
using static Bahtinov_Collimator.BahtinovLineDataEventArgs;
using Bahtinov_Collimator.Helper;
using System.Windows.Forms;

namespace Bahtinov_Collimator.Image_Processing
{
    /// <summary>
    /// Processes defocused star images and publishes inner/outer circle measurements.
    /// </summary>
    internal class DefocusStarProcessing
    {
        #region Events

        /// <summary>
        /// Delegate for handling defocus circle events.
        /// </summary>
        public delegate void DefocusCircleEventHandler(object sender, DefocusCircleEventArgs e);

        /// <summary>
        /// Event triggered when defocus circle data is available.
        /// </summary>
        public static event DefocusCircleEventHandler DefocusCircleEvent;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DefocusStarProcessing"/> class.
        /// </summary>
        public DefocusStarProcessing() { }

        #endregion

        #region Public Methods

        /// <summary>
        /// Processes the given defocus image to find the inner and outer circles and calculate their properties.
        /// 
        /// The method calculates the center, inner radius, and outer radius of the defocus image, and then computes
        /// the distance and direction between the inner and outer circles. The results are sent through the
        /// <see cref="DefocusCircleEvent"/>.
        /// 
        /// If the image is null, an error message is displayed.
        /// </summary>
        /// <param name="image">The bitmap image of the defocus star.</param>
        public void DisplayDefocusImage(Bitmap image)
        {
            if (image == null)
            {
                DarkMessageBox.Show(UiText.Current.DefocusNoImageMessage, UiText.Current.DefocusNoImageTitle, MessageBoxIcon.Error, MessageBoxButtons.OK);
                return;
            }

            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                double imgCenterX = image.Width / 2;
                double imgCenterY = image.Height / 2;

                var (innerCentre, innerRadius) = FindAverageInnerRadius(image, imgCenterX, imgCenterY);
                var (outerCentre, outerRadius) = FindAverageOuterRadius(image, imgCenterX, imgCenterY);

                var (distance, direction) = CalculateDistanceAndDirection(innerCentre, outerCentre, innerRadius);

                DefocusCircleEvent?.Invoke(null, new DefocusCircleEventArgs(image,
                    new PointD(innerCentre.X - imgCenterX, innerCentre.Y - imgCenterY), innerRadius,
                    new PointD(outerCentre.X - imgCenterX, outerCentre.Y - imgCenterY), outerRadius,
                    distance, direction));
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Calculates the distance and direction between the inner and outer circles.
        /// 
        /// The distance is scaled relative to the inner radius, and the direction is converted from radians to degrees.
        /// The direction is adjusted to be within the range of 0 to 360 degrees.
        /// </summary>
        /// <returns>A tuple containing the calculated distance and direction (in degrees).</returns>
        private static (double Distance, double Direction) CalculateDistanceAndDirection(PointD innerCentre, PointD outerCentre, double innerRadius)
        {
            double distance = ((innerCentre.DistanceTo(outerCentre)) / innerRadius) * 50;
            double directionRadians = innerCentre.DirectionTo(outerCentre);
            double directionDegrees = directionRadians * (180.0 / Math.PI);

            if (directionDegrees < 0)
                directionDegrees += 360.0;

            return (distance, directionDegrees);
        }

        /// <summary>
        /// Calculates the average radius and center of a circle in the image based on brightness transitions.
        /// 
        /// The method iterates over different angles to find the radius where the brightness transition occurs. It uses
        /// trigonometric precomputations for efficiency and processes the image's pixel data to determine the average radius
        /// and center point. The calculation is different for inner and outer circles based on the <paramref name="isInnerRadius"/> flag.
        /// 
        /// The bitmap is locked for faster access, and pixel brightness is computed using a predefined formula.
        /// </summary>
        /// <returns>A tuple containing the center point and radius of the circle.</returns>
        private (PointD centre, double radius) CalculateAverageRadius(Bitmap image, double centerX, double centerY, bool isInnerRadius)
        {
            int width = image.Width;
            int height = image.Height;

            int maxRadius = Math.Min(image.Width, image.Height) / 2;
            int radiusSum = 0;
            int transitionCount = 0;
            double transitionXSum = 0;
            double transitionYSum = 0;

            double[] cosValues = new double[360];
            double[] sinValues = new double[360];

            for (int i = 0; i < 360; i++)
            {
                cosValues[i] = Math.Cos(i * Math.PI / 180);
                sinValues[i] = Math.Sin(i * Math.PI / 180);
            }

            BitmapData bitmapData = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, image.PixelFormat);
            int bytesPerPixel = Image.GetPixelFormatSize(image.PixelFormat) / 8;
            int stride = bitmapData.Stride;
            IntPtr scan0 = bitmapData.Scan0;
            byte[] pixels = new byte[stride * height];
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

                    if (x >= 0 && x < width && y >= 0 && y < height)
                    {
                        int pixelIndex = (int)(y) * stride + (int)(x) * bytesPerPixel;
       
                        double brightness = GetBrightness(pixels[pixelIndex + 2], pixels[pixelIndex + 1], pixels[pixelIndex]);

                        if (brightness != 0)
                        {
                            lineSumBrightness += brightness;
                            numPoints++;
                        }
                    }
                }

                double averageLineBrightness = numPoints > 0 ? lineSumBrightness / numPoints : 0.0;

                for (int r = startRadius + 10; isInnerRadius ? r < endRadius : r > endRadius; r += step)
                {
                    double x = centerX + (r * cosValues[angle]);
                    double y = centerY + (r * sinValues[angle]);

                    if (x >= 0 && x < width && y >= 0 && y < height)
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

            image.UnlockBits(bitmapData);

            double radius = transitionCount > 0 ? Math.Round((double)radiusSum / transitionCount) : maxRadius;
            double circleX = transitionXSum / (transitionCount / 2) + centerX;
            double circleY = transitionYSum / (transitionCount / 2) + centerY;

            return (new PointD(circleX, circleY), radius);
        }

        /// <summary>
        /// Calculates the brightness of a pixel based on its RGB values.
        /// 
        /// The brightness is computed using a weighted average of the red, green, and blue components. This formula is
        /// commonly used for converting RGB values to grayscale brightness.
        /// 
        /// The result is normalized to a range of 0 to 1.
        /// </summary>
        /// <returns>The calculated brightness of the pixel.</returns>
        private double GetBrightness(byte r, byte g, byte b)
        {
            return (r * 0.299 + g * 0.587 + b * 0.114) / 255.0;
        }

        /// <summary>
        /// Finds the average radius and center of the inner circle in the defocus image.
        /// 
        /// This method calls the <see cref="CalculateAverageRadius"/> method with the <paramref name="isInnerRadius"/> flag
        /// set to true to find the average inner radius and center.
        /// </summary>
        /// <returns>A tuple containing the center point and radius of the inner circle.</returns>
        private (PointD centre, double radius) FindAverageInnerRadius(Bitmap image, double centerX, double centerY)
        {
            return CalculateAverageRadius(image, centerX, centerY, true);
        }

        /// <summary>
        /// Finds the average radius and center of the outer circle in the defocus image.
        /// 
        /// This method calls the <see cref="CalculateAverageRadius"/> method with the <paramref name="isInnerRadius"/> flag
        /// set to false to find the average outer radius and center.
        /// </summary>
        /// <returns>A tuple containing the center point and radius of the outer circle.</returns>
        private (PointD centre, double radius) FindAverageOuterRadius(Bitmap image, double centerX, double centerY)
        {
            return CalculateAverageRadius(image, centerX, centerY, false);
        }

        #endregion
    }
}
