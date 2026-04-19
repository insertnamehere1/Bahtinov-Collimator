using System;
using System.Collections.Generic;
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

                // The outer disk almost always contains the image centre (capture re-centres on the
                // outer circle), so scanning rays from the image centre samples the outer boundary well.
                var (outerCentre, outerRadius) = FindAverageOuterRadius(image, imgCenterX, imgCenterY);

                // For the inner hole, scanning from the image centre is unreliable when the hole is
                // significantly offset: rays may start in the bright donut and trigger immediately,
                // placing the centre near the image centre instead of the hole. Use the outer circle
                // centre as the scan origin – it is far closer to the hole and is normally inside it.
                var (innerCentre, innerRadius) = FindAverageInnerRadius(image, outerCentre.X, outerCentre.Y);

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

            // Use the largest possible search radius (image diagonal) so an off-centre scan origin
            // never artificially clips the boundary on one side. Out-of-bounds samples are skipped.
            int maxRadius = (int)Math.Ceiling(Math.Sqrt(width * width + height * height));

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
            byte[] pixels;
            try
            {
                IntPtr scan0 = bitmapData.Scan0;
                pixels = new byte[stride * height];
                System.Runtime.InteropServices.Marshal.Copy(scan0, pixels, 0, pixels.Length);
            }
            finally
            {
                image.UnlockBits(bitmapData);
            }

            // Collect every detected boundary point and its distance from the scan origin.
            // We later run a least-squares circle fit on these points; this gives an unbiased centre
            // even when the rays are cast from a point that is offset from the true circle centre.
            var boundaryX = new List<double>(360);
            var boundaryY = new List<double>(360);
            var boundaryR = new List<double>(360);

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

                int searchStart = isInnerRadius ? startRadius + 10 : startRadius;

                for (int r = searchStart; isInnerRadius ? r < endRadius : r > endRadius; r += step)
                {
                    double x = centerX + (r * cosValues[angle]);
                    double y = centerY + (r * sinValues[angle]);

                    if (x >= 0 && x < width && y >= 0 && y < height)
                    {
                        int pixelIndex = (int)(y) * stride + (int)(x) * bytesPerPixel;
                        double brightness = GetBrightness(pixels[pixelIndex + 2], pixels[pixelIndex + 1], pixels[pixelIndex]);

                        if (brightness > averageLineBrightness)
                        {
                            boundaryX.Add(x);
                            boundaryY.Add(y);
                            boundaryR.Add(r);
                            break;
                        }
                    }
                }
            }

            if (boundaryR.Count < 8)
            {
                // Not enough transitions to fit a circle; fall back to the scan origin.
                return (new PointD(centerX, centerY), boundaryR.Count > 0 ? Median(boundaryR) : 0);
            }

            // Reject obvious outliers by trimming points whose radius from the scan origin is far
            // from the median. This protects the fit when a few rays latch onto noise or the wrong
            // edge (e.g. inner-hole rays that escape the hole and find the outer rim instead).
            double medianRadius = Median(boundaryR);
            double tolerance = Math.Max(8.0, medianRadius * 0.35);

            var fitX = new List<double>(boundaryR.Count);
            var fitY = new List<double>(boundaryR.Count);
            for (int i = 0; i < boundaryR.Count; i++)
            {
                if (Math.Abs(boundaryR[i] - medianRadius) <= tolerance)
                {
                    fitX.Add(boundaryX[i]);
                    fitY.Add(boundaryY[i]);
                }
            }

            if (fitX.Count < 8)
            {
                fitX = boundaryX;
                fitY = boundaryY;
            }

            if (TryFitCircleKasa(fitX, fitY, out double cx, out double cy, out double rFit))
            {
                return (new PointD(cx, cy), Math.Round(rFit));
            }

            // Fit failed numerically – fall back to the (biased) centroid as a last resort.
            double sumX = 0, sumY = 0;
            for (int i = 0; i < fitX.Count; i++) { sumX += fitX[i]; sumY += fitY[i]; }
            return (new PointD(sumX / fitX.Count, sumY / fitX.Count), Math.Round(medianRadius));
        }

        /// <summary>
        /// Algebraic (Kåsa) least-squares circle fit. Solves the 3x3 normal equations for
        /// (a, b, c) such that a*x + b*y + c = x^2 + y^2, then recovers centre = (a/2, b/2)
        /// and radius = sqrt(c + cx^2 + cy^2).
        /// </summary>
        private static bool TryFitCircleKasa(List<double> xs, List<double> ys, out double cx, out double cy, out double radius)
        {
            cx = 0; cy = 0; radius = 0;

            int n = xs.Count;
            if (n < 3) return false;

            double sumX = 0, sumY = 0, sumX2 = 0, sumY2 = 0, sumXY = 0;
            double sumZ = 0, sumXZ = 0, sumYZ = 0;

            for (int i = 0; i < n; i++)
            {
                double x = xs[i];
                double y = ys[i];
                double z = x * x + y * y;
                sumX += x;
                sumY += y;
                sumX2 += x * x;
                sumY2 += y * y;
                sumXY += x * y;
                sumZ += z;
                sumXZ += x * z;
                sumYZ += y * z;
            }

            // Normal equations:
            // | sumX2 sumXY sumX | |a|   | sumXZ |
            // | sumXY sumY2 sumY | |b| = | sumYZ |
            // | sumX  sumY  n    | |c|   | sumZ  |
            double m11 = sumX2, m12 = sumXY, m13 = sumX;
            double m21 = sumXY, m22 = sumY2, m23 = sumY;
            double m31 = sumX,  m32 = sumY,  m33 = n;

            double det = m11 * (m22 * m33 - m23 * m32)
                       - m12 * (m21 * m33 - m23 * m31)
                       + m13 * (m21 * m32 - m22 * m31);

            if (Math.Abs(det) < 1e-9) return false;

            double detA = sumXZ * (m22 * m33 - m23 * m32)
                        - m12   * (sumYZ * m33 - m23 * sumZ)
                        + m13   * (sumYZ * m32 - m22 * sumZ);

            double detB = m11   * (sumYZ * m33 - m23 * sumZ)
                        - sumXZ * (m21 * m33 - m23 * m31)
                        + m13   * (m21 * sumZ - sumYZ * m31);

            double detC = m11 * (m22 * sumZ - sumYZ * m32)
                        - m12 * (m21 * sumZ - sumYZ * m31)
                        + sumXZ * (m21 * m32 - m22 * m31);

            double a = detA / det;
            double b = detB / det;
            double c = detC / det;

            cx = a / 2.0;
            cy = b / 2.0;
            double rSq = c + cx * cx + cy * cy;
            if (rSq < 0) return false;

            radius = Math.Sqrt(rSq);
            return true;
        }

        /// <summary>
        /// Returns the median of a list of doubles. The input list is copied before sorting.
        /// </summary>
        private static double Median(List<double> values)
        {
            var sorted = new List<double>(values);
            sorted.Sort();
            int n = sorted.Count;
            if (n == 0) return 0;
            if ((n & 1) == 1) return sorted[n / 2];
            return 0.5 * (sorted[n / 2 - 1] + sorted[n / 2]);
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
