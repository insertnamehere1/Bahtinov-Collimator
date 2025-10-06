
// Sections of the following code are covered by and existing copyright
//
//MIT License

//Copyright (c) 2017 Wytse Jan Posthumus

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Bahtinov_Collimator.BahtinovLineDataEventArgs;

namespace Bahtinov_Collimator
{
    internal class BahtinovProcessing
    {
        #region Delegates
        // Focus Data Listener
        public delegate void FocusDataEventHandler(object sender, FocusDataEventArgs e);
        public static event FocusDataEventHandler FocusDataEvent;

        // LineDrawing Event Listener
        public delegate void BahtinovLineDrawEventHandler(object sender, BahtinovLineDataEventArgs e);
        public static event BahtinovLineDrawEventHandler BahtinovLineDrawEvent;
        #endregion

        #region Private Fields
        // Settings
        private float apertureSetting;
        private float focalLengthSetting;
        private double pixelSizeSetting;

        // to be checked for relevance
        private const float ErrorMarkerScalingValue = 20.0f;

        // single pixel offset
        private const int yOffset = 1;

        // retain last error value
        private float lastFocusErrorValue = 0.0f;
        #endregion

        #region Public Fields
        public static Dictionary<int, double> ErrorValues { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="BahtinovProcessing"/> class. 
        /// Sets up an empty dictionary for storing error values and loads settings from a configuration source.
        /// </summary>
        public BahtinovProcessing()
        {
            ErrorValues = new Dictionary<int, double>();
            LoadSettings();
        }
        #endregion

        #region Methods

        /// <summary>
        /// Loads configuration settings from the application settings.
        /// </summary>
        public void LoadSettings()
        {
            // load settings
            apertureSetting = Properties.Settings.Default.Aperture;
            focalLengthSetting = Properties.Settings.Default.FocalLength;
            pixelSizeSetting = Properties.Settings.Default.PixelSize;
        }

        /// <summary>
        /// Analyzes a bitmap image of a star to identify the brightest lines and their angles.
        /// 
        /// The method performs the following:
        /// 1. Converts the image data into a 2D array and calculates brightness values.
        /// 2. Rotates the image at various angles and detects the brightest lines by summing pixel values along each line.
        /// 3. Smooths the detected line values and selects the top brightest lines.
        /// 
        /// Returns a <see cref="BahtinovData"/> object containing the indices, angles, and values of the brightest lines.
        /// 
        /// <param name="starImage">The bitmap image of the star to be analyzed.</param>
        /// <returns>A <see cref="BahtinovData"/> object with information about the brightest lines.</returns>
        /// </summary>
        public BahtinovData FindBrightestLines(Bitmap starImage)
        {
            const int LINES = 9;
            const int DEGREES180 = 180;
            const float RADIANS_PER_DEGREE = (float)Math.PI / DEGREES180;
            const float DIVISION_FACTOR = 3f / byte.MaxValue;
            const int DEG_5 = 5;

            Rectangle rect = new Rectangle(0, 0, starImage.Width, starImage.Height);
            BahtinovData result = new BahtinovData(LINES, rect);
            BitmapData bitmapData = starImage.LockBits(rect, ImageLockMode.ReadWrite, starImage.PixelFormat);

            IntPtr scan0 = bitmapData.Scan0;
            int stride = bitmapData.Stride;
            int bytesPerPixel = System.Drawing.Image.GetPixelFormatSize(starImage.PixelFormat) / 8;
            int width = starImage.Width;
            int height = starImage.Height;

            byte[] starImageArray = new byte[height * stride];
            Marshal.Copy(scan0, starImageArray, 0, starImageArray.Length);
            starImage.UnlockBits(bitmapData);

            float edgeOffset = (width < height ? 0.5f * (float)Math.Sqrt(2.0) * width : 0.5f * (float)Math.Sqrt(2.0) * height) - 8f;
            int leftEdge = (int)(0.5 * (width - edgeOffset));
            int rightEdge = (int)(0.5 * (width + edgeOffset));
            int topEdge = (int)(0.5 * (height - edgeOffset));
            int bottomEdge = (int)(0.5 * (height + edgeOffset));

            float[,] starArray2D = new float[width, height];
            float starImage_X_Centre = (width + 1.0f) / 2.0f;
            float starImage_Y_Centre = (height + 1.0f) / 2.0f;

            for (int x = leftEdge; x < rightEdge; ++x)
            {
                for (int y = topEdge; y < bottomEdge; ++y)
                {
                    int pixelIndex = y * stride + x * bytesPerPixel;
                    byte greenValue = starImageArray[pixelIndex + 1];

                    starArray2D[x, y] = (float)Math.Sqrt(greenValue * DIVISION_FACTOR);
                }
            }

            float[] lineNumbersOfBrightestLines = new float[DEGREES180];
            float[] valuesOfBrightestLines = new float[DEGREES180];

            Parallel.For(0, DEGREES180, index1 =>
            {
                float[,] rotatedImage = new float[width, height];
                float currentRadians = RADIANS_PER_DEGREE * index1;
                float sin_CurrentRadians = (float)Math.Sin(currentRadians);
                float cos_CurrentRadians = (float)Math.Cos(currentRadians);

                for (int x = leftEdge; x < rightEdge; ++x)
                {
                    for (int y = topEdge; y < bottomEdge; ++y)
                    {
                        float X_DistanceFromCentre = x - starImage_X_Centre;
                        float Y_DistanceFromCentre = y - starImage_Y_Centre;

                        float rotatedXDistance = starImage_X_Centre + X_DistanceFromCentre * cos_CurrentRadians + Y_DistanceFromCentre * sin_CurrentRadians;
                        float rotatedYDistance = starImage_Y_Centre - X_DistanceFromCentre * sin_CurrentRadians + Y_DistanceFromCentre * cos_CurrentRadians;

                        int rotatedXRoundedDown = (int)rotatedXDistance;
                        int rotatedXRoundedUp = rotatedXRoundedDown + 1;
                        int rotatedYRoundedDown = (int)rotatedYDistance;
                        int rotatedYRoundedUp = rotatedYRoundedDown + 1;

                        if (rotatedXRoundedDown >= leftEdge && rotatedXRoundedUp < rightEdge &&
                            rotatedYRoundedDown >= topEdge && rotatedYRoundedUp < bottomEdge)
                        {
                            float rotatedXFraction = rotatedXDistance - rotatedXRoundedDown;
                            float rotatedYFraction = rotatedYDistance - rotatedYRoundedDown;

                            float oneMinusRotatedXFraction = 1.0f - rotatedXFraction;
                            float oneMinusRotatedYFraction = 1.0f - rotatedYFraction;
                            float value1 = starArray2D[rotatedXRoundedDown, rotatedYRoundedDown];
                            float value2 = starArray2D[rotatedXRoundedUp, rotatedYRoundedDown];
                            float value3 = starArray2D[rotatedXRoundedUp, rotatedYRoundedUp];
                            float value4 = starArray2D[rotatedXRoundedDown, rotatedYRoundedUp];

                            rotatedImage[x, y] = value1 * oneMinusRotatedXFraction * oneMinusRotatedYFraction +
                                                 value2 * rotatedXFraction * oneMinusRotatedYFraction +
                                                 value3 * rotatedXFraction * rotatedYFraction +
                                                 value4 * oneMinusRotatedXFraction * rotatedYFraction;
                        }
                    }
                }

                float[] rowTotals = new float[height];
                for (int y = topEdge; y < bottomEdge; ++y)
                {
                    float rowTotal = 0;
                    for (int x = leftEdge; x < rightEdge; ++x)
                    {
                        rowTotal += rotatedImage[x, y];
                    }
                    rowTotals[y] = rowTotal / (rightEdge - leftEdge);
                }

                float[] smoothedTotals = new float[height];
                for (int y = topEdge; y < bottomEdge; ++y)
                {
                    smoothedTotals[y] = rowTotals[y];
                    if (y > topEdge && y < bottomEdge - 1)
                    {
                        smoothedTotals[y] = (rowTotals[y - 1] + rowTotals[y] + rowTotals[y + 1]) / 3;
                    }
                }

                // Old: pick single max row (unstable on thick spikes)
                // New: find peak row, then compute centroid across bright plateau for stability
                int yPeak = -1;
                float bestVal = -1f;
                
                for (int y = topEdge; y < bottomEdge; ++y)
                {
                    if (smoothedTotals[y] > bestVal)
                    {
                        bestVal = smoothedTotals[y];
                        yPeak = y;
                    }
                }
                float centroidY = PeakCentroid(smoothedTotals, topEdge, bottomEdge, yPeak, 0.80f);
                lineNumbersOfBrightestLines[index1] = centroidY;
                valuesOfBrightestLines[index1] = bestVal;
            });

            for (int i = 0; i < LINES; ++i)
            {
                float maxValue = -1f;
                float angleInRadians = -1f;
                float lineNumber = -1f;
                int brightestLineIndex = -1;

                for (int j = 0; j < DEGREES180; ++j)
                {
                    if (valuesOfBrightestLines[j] > maxValue)
                    {
                        maxValue = valuesOfBrightestLines[j];
                        lineNumber = lineNumbersOfBrightestLines[j];
                        brightestLineIndex = j;
                    }
                }

                // Sub-degree angle refinement around brightestLineIndex using 3-point parabolic fit
                int a0 = (brightestLineIndex - 1 + DEGREES180) % DEGREES180;
                int a1 = brightestLineIndex;
                int a2 = (brightestLineIndex + 1) % DEGREES180;
                
                float v0 = valuesOfBrightestLines[a0];
                float v1 = valuesOfBrightestLines[a1];
                float v2 = valuesOfBrightestLines[a2];
                
                // Vertex offset in bins (bin=1 degree): delta = 0.5*(v0 - v2)/(v0 - 2*v1 + v2)
                float denom = (v0 - 2f * v1 + v2);
                float delta = denom != 0f ? 0.5f * (v0 - v2) / denom : 0f;
                
                // Clamp to [-0.5, +0.5] so we do not jump across neighboring bins
                if (delta < -0.5f) delta = -0.5f;
                if (delta > 0.5f) delta = 0.5f;
                
                float refinedAngleDeg = a1 + delta;
                angleInRadians = refinedAngleDeg * RADIANS_PER_DEGREE;
                
                result.LineIndex[i] = lineNumber;
                result.LineAngles[i] = angleInRadians;   // refined sub-degree angle
                result.LineValue[i] = maxValue;

                for (int j = brightestLineIndex - DEG_5; j <= brightestLineIndex + DEG_5; ++j)
                {
                    int index = (j + DEGREES180) % DEGREES180;
                    valuesOfBrightestLines[index] = 0.0f;
                }
            }

            return result;
        }

        /// <summary>
        /// Enhances the accuracy of line detection in a star image by performing subpixel interpolation.
        /// 
        /// The method performs the following steps:
        /// 1. Initializes the bounding box and scales it according to the DPI settings.
        /// 2. Reads and processes the image pixel data into a 2D array.
        /// 3. For each line specified in the <paramref name="bahtinovLines"/> object, applies rotation and interpolation to find the subpixel-accurate position and value of the line.
        /// 4. Updates the provided <paramref name="bahtinovLines"/> object with the refined line indices and values.
        /// 
        /// Returns the updated <see cref="BahtinovData"/> object containing the subpixel-accurate line indices and values.
        /// 
        /// <param name="starImage">The bitmap image of the star to be analyzed.</param>
        /// <param name="lineCount">The number of lines to detect and refine.</param>
        /// <param name="bahtinovLines">The <see cref="BahtinovData"/> object containing initial line data to be refined.</param>
        /// <returns>The updated <see cref="BahtinovData"/> object with refined line indices and values.</returns>
        /// </summary>
        public BahtinovData FindSubpixelLines(Bitmap starImage, int lineCount, BahtinovData bahtinovLines)
        {
            // Define the full rectangle of the image.
            Rectangle rect = new Rectangle(0, 0, starImage.Width, starImage.Height);

            // Lock the bitmap data.
            BitmapData bitmapData = starImage.LockBits(rect, ImageLockMode.ReadWrite, starImage.PixelFormat);
            IntPtr scan0 = bitmapData.Scan0;
            int imagePixelCount = bitmapData.Stride * starImage.Height;
            byte[] starImageArray = new byte[imagePixelCount];

            // Copy pixel data to byte array and back to the image.
            Marshal.Copy(scan0, starImageArray, 0, imagePixelCount);
            Marshal.Copy(starImageArray, 0, scan0, imagePixelCount);
            starImage.UnlockBits(bitmapData);

            int width = starImage.Width;
            int height = starImage.Height;

            // Apply DPI scaling to the edgeOffset
            float edgeOffset = ((width < height ? width : height) * 0.707f - 8f);

            // Calculate bounding box edges with scaling.
            int leftEdge = (int)(0.5f * (width - edgeOffset));
            int rightEdge = (int)(0.5f * (width + edgeOffset));
            int topEdge = (int)(0.5f * (height - edgeOffset));
            int bottomEdge = (int)(0.5f * (height + edgeOffset));

            float starImageXCenter = (width + 1) / 2.0f;
            float starImageYCenter = (height + 1) / 2.0f;
            float divisionFactor = 3f / byte.MaxValue;

            float[,] starArray2D = new float[width, height];
            float pixelValueTotal = 0f;
            float pixelsCounted = 0f;

            // Process the pixels in the bounding box and populate starArray2D.
            for (int x = leftEdge; x < rightEdge; ++x)
            {
                for (int y = topEdge; y < bottomEdge; ++y)
                {
                    int pixelIndex = (x + y * width) * bitmapData.Stride / width;
                    byte redValue = starImageArray[pixelIndex];
                    byte greenValue = starImageArray[pixelIndex + 1];
                    byte blueValue = starImageArray[pixelIndex + 2];

                    float pixelValue = (redValue + greenValue + blueValue) * divisionFactor;
                    starArray2D[x, y] = (float)Math.Sqrt(pixelValue);
                    pixelValueTotal += starArray2D[x, y];
                    ++pixelsCounted;
                }
            }

            float[] subpixelLineNumbers = new float[lineCount];
            float[] brightestLineValues = new float[lineCount];

            Parallel.For(0, lineCount, index1 =>
            {
                float[,] imageArray = new float[width, height];
                float knownAngle = bahtinovLines.GetLineAngle(index1);
                float sinCurrentRadians = (float)Math.Sin(knownAngle);
                float cosCurrentRadians = (float)Math.Cos(knownAngle);

                for (int x = leftEdge; x < rightEdge; ++x)
                {
                    for (int y = topEdge; y < bottomEdge; ++y)
                    {
                        float xDistanceFromCenter = x - starImageXCenter;
                        float yDistanceFromCenter = y - starImageYCenter;

                        float rotatedXDistance = starImageXCenter + xDistanceFromCenter * cosCurrentRadians + yDistanceFromCenter * sinCurrentRadians;
                        float rotatedYDistance = starImageYCenter - xDistanceFromCenter * sinCurrentRadians + yDistanceFromCenter * cosCurrentRadians;

                        int rotatedXRoundedDown = (int)rotatedXDistance;
                        int rotatedYRoundedDown = (int)rotatedYDistance;

                        if (rotatedXRoundedDown < 0 || rotatedXRoundedDown >= width - 1 || rotatedYRoundedDown < 0 || rotatedYRoundedDown >= height - 1)
                            continue;

                        float rotatedXFraction = rotatedXDistance - rotatedXRoundedDown;
                        float rotatedYFraction = rotatedYDistance - rotatedYRoundedDown;

                        float value1 = starArray2D[rotatedXRoundedDown, rotatedYRoundedDown];
                        float value2 = starArray2D[rotatedXRoundedDown + 1, rotatedYRoundedDown];
                        float value3 = starArray2D[rotatedXRoundedDown + 1, rotatedYRoundedDown + 1];
                        float value4 = starArray2D[rotatedXRoundedDown, rotatedYRoundedDown + 1];

                        float interpolatedValue = (float)(
                            value1 * (1 - rotatedXFraction) * (1 - rotatedYFraction) +
                            value2 * rotatedXFraction * (1 - rotatedYFraction) +
                            value3 * rotatedXFraction * rotatedYFraction +
                            value4 * (1 - rotatedXFraction) * rotatedYFraction
                        );

                        imageArray[x, y] = interpolatedValue;
                    }
                }

                float[] rowTotals = new float[height];
                for (int y = topEdge; y < bottomEdge; ++y)
                {
                    float rowTotal = 0f;
                    for (int x = leftEdge; x < rightEdge; ++x)
                    {
                        rowTotal += imageArray[x, y];
                    }
                    rowTotals[y] = rowTotal / (rightEdge - leftEdge);
                }

                float largestValueForThisRotation = -1f;
                float lineNumberOfLargestValue = -1f;

                for (int y = topEdge; y < bottomEdge; ++y)
                {
                    if (rowTotals[y] > largestValueForThisRotation)
                    {
                        largestValueForThisRotation = rowTotals[y];
                        lineNumberOfLargestValue = y;
                    }
                }

                // Seed the subpixel fit at the centroid of the peak plateau for extra stability
                int yPeak = (int)lineNumberOfLargestValue;
                float centroidY = PeakCentroid(rowTotals, topEdge, bottomEdge, yPeak, 0.80f);
                int seed = (int)Math.Round(centroidY);
                
                float subPixelLineNumber = (float)PolynomialCurveFit.FindMaxValueIndex(rowTotals, seed, 2);
                subpixelLineNumbers[index1] = subPixelLineNumber;
                brightestLineValues[index1] = largestValueForThisRotation;
            });

            for (int index = 0; index < lineCount; ++index)
            {
                bahtinovLines.LineIndex[index] = subpixelLineNumbers[index];
            }

            return bahtinovLines;
        }

        /// <summary>
        /// Processes detected Bahtinov diffraction lines and overlays them on a star image.
        /// 
        /// Steps performed:
        /// 1. Validates the number of detected lines (must be 3 or 9).
        /// 2. Converts detected line angles and offsets into XY endpoints.
        /// 3. Builds robust line representations in normal form, avoiding slope infinities.
        /// 4. For each group of 3 lines:
        ///    - Computes the intersection of the outer lines.
        ///    - Projects this intersection onto the middle line.
        ///    - Calculates focus error magnitude and sign.
        ///    - Evaluates whether the error lies within the critical focus zone.
        ///    - Prepares visual markers (error circle, error line, line overlays).
        /// 5. Emits events with processed focus data and line drawing information.
        /// </summary>
        /// <param name="lines">The <see cref="BahtinovData"/> containing detected line angles and offsets.</param>
        /// <param name="starImage">The bitmap image on which lines and markers are drawn.</param>
        /// <returns>True if processing succeeded, false if an error occurred.</returns>
        public bool DisplayLines(BahtinovData lines, Bitmap starImage)
        {
            if (lines.LineAngles.Length != 3 && lines.LineAngles.Length != 9)
            {
                ImageLostEventProvider.OnImageLost("Unable to detect Bahtinov image lines", "DisplayLines", MessageBoxIcon.Warning, MessageBoxButtons.OK);
                return false;
            }

            int numberOfGroups = lines.LineAngles.Length / 3;
            BahtinovLineData displayLines = new BahtinovLineData { NumberOfGroups = numberOfGroups };

            Rectangle rect = new Rectangle(0, 0, starImage.Width, starImage.Height);
            BahtinovData bahtinovLines = new BahtinovData(3, rect);

            int width = starImage.Width;
            int height = starImage.Height;
            float starImage_X_Centre = (float)(width + 1.0) / 2.0f;
            float starImage_Y_Centre = (float)(height + 1.0) / 2.0f;
            float yOffset = 0.0f;

            // Choose a small epsilon relative to image size for robust parallel checks
            double eps = Math.Max(width, height) * 1e-12; // very strict, yet safe for 500x500

            for (int group = 0; group < numberOfGroups; group++)
            {
                BahtinovLineDataEventArgs.LineGroup lineGroup = new BahtinovLineDataEventArgs.LineGroup(group);

                int startIndex = 3 * group;
                Array.Copy(lines.LineAngles, startIndex, bahtinovLines.LineAngles, 0, 3);
                Array.Copy(lines.LineIndex, startIndex, bahtinovLines.LineIndex, 0, 3);

                // Preserve your vertical-swap logic
                if (Math.Abs(bahtinovLines.LineAngles[0] - 1.5708) < 0.0001)
                {
                    float tmp = bahtinovLines.LineAngles[0];
                    float tmp2 = bahtinovLines.LineIndex[0];
                    bahtinovLines.LineAngles[0] = bahtinovLines.LineAngles[2];
                    bahtinovLines.LineIndex[0] = bahtinovLines.LineIndex[2];
                    bahtinovLines.LineAngles[2] = tmp;
                    bahtinovLines.LineIndex[2] = tmp2;
                }

                // Storage for endpoints, and for normal-form lines
                // second line endpoints are still required later for sign and distances
                double secondLineStartX = 0, secondLineStartY = 0;
                double secondLineEndX = 0, secondLineEndY = 0;

                Line2D? firstLine = null;
                Line2D? secondLine = null;
                Line2D? thirdLine = null;

                // Build endpoints from your angle + offset model, then create Line2D from endpoints
                for (int index = 0; index < bahtinovLines.LineAngles.Length; ++index)
                {
                    float centrePoint = Math.Min(starImage_X_Centre, starImage_Y_Centre);
                    float angle = bahtinovLines.LineAngles[index];
                    float sinAngle = (float)Math.Sin(angle);
                    float cosAngle = (float)Math.Cos(angle);
                    float offsetY = bahtinovLines.LineIndex[index] - starImage_Y_Centre;

                    float lineStart_X = starImage_X_Centre + (-centrePoint * cosAngle) + (offsetY * sinAngle);
                    float lineEnd_X = starImage_X_Centre + (centrePoint * cosAngle) + (offsetY * sinAngle);
                    float lineStart_Y = starImage_Y_Centre + (-centrePoint * sinAngle) - (offsetY * cosAngle);
                    float lineEnd_Y = starImage_Y_Centre + (centrePoint * sinAngle) - (offsetY * cosAngle);

                    // Add to group for drawing (screen coordinates with Y flipped as you had)
                    var bahtinovLine = new BahtinovLineDataEventArgs.Line(
                        new Point((int)Math.Round(lineStart_X), (int)Math.Round(height - lineStart_Y + yOffset)),
                        new Point((int)Math.Round(lineEnd_X), (int)Math.Round(height - lineEnd_Y + yOffset)),
                        index
                    );
                    lineGroup.AddLine(bahtinovLine);

                    // Build robust Line2D
                    var L = Line2D.FromPoints(lineStart_X, lineStart_Y, lineEnd_X, lineEnd_Y);

                    switch (index)
                    {
                        case 0:
                            firstLine = L;
                            break;
                        case 1:
                            secondLine = L;
                            secondLineStartX = lineStart_X;
                            secondLineStartY = lineStart_Y;
                            secondLineEndX = lineEnd_X;
                            secondLineEndY = lineEnd_Y;
                            break;
                        case 2:
                            thirdLine = L;
                            break;
                    }
                }

                if (firstLine == null || secondLine == null || thirdLine == null)
                {
                    ImageLostEventProvider.OnImageLost("Image Lost", "Bahtinov Processing: missing lines", MessageBoxIcon.Error, MessageBoxButtons.OK);
                    lastFocusErrorValue = 0.0f;
                    return false;
                }

                // Intersection of first and third lines (no slopes used)
                var inter = Line2D.Intersect(firstLine.Value, thirdLine.Value, eps);
                if (!inter.HasValue)
                {
                    // If these are nearly parallel, you could fall back to midpoints or skip
                    ImageLostEventProvider.OnImageLost("Unable to compute intersection for Bahtinov lines", "DisplayLines", MessageBoxIcon.Warning, MessageBoxButtons.OK);
                    lastFocusErrorValue = 0.0f;
                    return false;
                }
                double xIntersectionD = inter.Value.x;
                double yIntersectionD = inter.Value.y;

                // Project the intersection onto the second line to get the perpendicular foot
                var (perpXD, perpYD) = secondLine.Value.ProjectPoint(xIntersectionD, yIntersectionD);

                // Distances and sign (keep your existing sign convention)
                double dxErr = xIntersectionD - perpXD;
                double dyErr = yIntersectionD - perpYD;

                double dx2 = secondLineEndX - secondLineStartX;
                double dy2 = secondLineEndY - secondLineStartY;

                float errorSign;
                try
                {
                    // cross product z of (error vector) x (second line direction)
                    errorSign = (float)(-Math.Sign(dxErr * dy2 - dyErr * dx2));
                }
                catch
                {
                    ImageLostEventProvider.OnImageLost("Image Lost", "Bahtinov Processing: ", MessageBoxIcon.Error, MessageBoxButtons.OK);
                    lastFocusErrorValue = 0.0f;
                    return false;
                }

                double errorDistanceD = Math.Sqrt(dxErr * dxErr + dyErr * dyErr);
                double bahtinovOffset = errorSign * Math.Floor(errorDistanceD * 10.0) / 10.0;

                float radianPerDegree = (float)Math.PI / 180f;
                float bahtinovAngle = Math.Abs((bahtinovLines.LineAngles[2] - bahtinovLines.LineAngles[0]) / 2.0f);

                float pixelsPerMicron = (float)(9.0f / 32.0f * (apertureSetting / 1000.0f) /
                                        ((focalLengthSetting / 1000.0f) * pixelSizeSetting) *
                                        (1.0f + Math.Cos(45.0f * radianPerDegree) * (1.0f + (float)Math.Tan(bahtinovAngle))));

                double focusErrorInMicrons = errorDistanceD / pixelsPerMicron;

                float criticalFocusValue = 8.99999974990351E-07f * (focalLengthSetting / 1000.0f) /
                                           (apertureSetting / 1000.0f) * (focalLengthSetting / 1000.0f) /
                                           (apertureSetting / 1000.0f);

                bool withinCriticalFocus = Math.Abs(focusErrorInMicrons * 1E-06) < Math.Abs(criticalFocusValue);

                FocusData fd = new FocusData
                {
                    BahtinovOffset = bahtinovOffset,
                    DefocusError = focusErrorInMicrons,
                    InsideFocus = withinCriticalFocus,
                    Id = group
                };

                try
                {
                    FocusDataEvent?.Invoke(null, new FocusDataEventArgs(fd));
                }
                catch { }

                ErrorValues[group] = bahtinovOffset;

                // Error marker point along the perpendicular segment (cast back to float for drawing)
                float xIntersection = (float)xIntersectionD;
                float yIntersection = (float)yIntersectionD;
                float perpendicularX = (float)perpXD;
                float perpendicularY = (float)perpYD;

                float errorMarker_X = xIntersection + (perpendicularX - xIntersection) * ErrorMarkerScalingValue;
                float errorMarker_Y = yIntersection + (perpendicularY - yIntersection) * ErrorMarkerScalingValue;

                int circleRadius = UITheme.ErrorCircleRadius;
                int circle_x = (int)(errorMarker_X - circleRadius);
                int circle_y = (int)(height - errorMarker_Y - circleRadius + yOffset);
                int circle_width = circleRadius * 2;
                int circle_height = circleRadius * 2;
                string errorValue = (errorSign * (Math.Floor((float)errorDistanceD * 10) / 10)).ToString("F1");

                lineGroup.ErrorCircle = new BahtinovLineDataEventArgs.ErrorCircle(
                    new Point(circle_x, circle_y), circle_width, circle_height, withinCriticalFocus, errorValue);

                // Error line is drawn perpendicular to the 2nd line at the error marker location
                float lineX1 = errorMarker_X + circleRadius;
                float lineY1 = height - errorMarker_Y;
                float lineX2 = errorMarker_X - circleRadius;
                float lineY2 = height - errorMarker_Y;

                lineX1 -= errorMarker_X;
                lineY1 -= height - errorMarker_Y;
                lineX2 -= errorMarker_X;
                lineY2 -= height - errorMarker_Y;

                float rotatedX1 = (float)(lineX1 * Math.Cos(-bahtinovLines.LineAngles[1]) - lineY1 * Math.Sin(-bahtinovLines.LineAngles[1]));
                float rotatedY1 = (float)(lineX1 * Math.Sin(-bahtinovLines.LineAngles[1]) + lineY1 * Math.Cos(-bahtinovLines.LineAngles[1]));
                float rotatedX2 = (float)(lineX2 * Math.Cos(-bahtinovLines.LineAngles[1]) - lineY2 * Math.Sin(-bahtinovLines.LineAngles[1]));
                float rotatedY2 = (float)(lineX2 * Math.Sin(-bahtinovLines.LineAngles[1]) + lineY2 * Math.Cos(-bahtinovLines.LineAngles[1]));

                rotatedX1 += errorMarker_X;
                rotatedY1 += height - errorMarker_Y;
                rotatedX2 += errorMarker_X;
                rotatedY2 += height - errorMarker_Y;

                lineGroup.ErrorLine = new BahtinovLineDataEventArgs.ErrorLine(
                    new Point((int)rotatedX1, (int)rotatedY1),
                    new Point((int)rotatedX2, (int)rotatedY2));

                displayLines.AddLineGroup(lineGroup);

                lastFocusErrorValue = xIntersection; // unchanged behavior
            }

            BahtinovLineDrawEvent?.Invoke(null, new BahtinovLineDataEventArgs(displayLines, starImage));
            return true;
        }

        /// <summary>
        /// Stops the image processing and clears any displayed focus data.
        /// 
        /// The method creates a <see cref="FocusData"/> object with a flag to clear the display and an identifier of -1.
        /// It then triggers the <see cref="FocusDataEvent"/> event with this data to update the UI or other components accordingly.
        /// </summary>
        public void StopImageProcessing()
        {
            FocusData fd = new FocusData
            {
                ClearDisplay = true,
                Id = -1
            };

            FocusDataEvent?.Invoke(null, new FocusDataEventArgs(fd));
        }

        // Center of mass inside the contiguous window above a fraction of the peak.
        // Use frac=0.70..0.85 depending on how fat the spikes are.
        private static float PeakCentroid(float[] v, int top, int bottom, int peakIndex, float frac = 0.80f)
        {
            float peak = v[peakIndex];
            float thr = peak * frac;

            int L = peakIndex, R = peakIndex;
            while (L > top && v[L - 1] >= thr) L--;
            while (R < bottom - 1 && v[R + 1] >= thr) R++;

            double num = 0, den = 0;
            for (int y = L; y <= R; y++)
            {
                double w = v[y];
                num += y * w;
                den += w;
            }
            return den > 0 ? (float)(num / den) : peakIndex;
        }
        #endregion
    }

    #region Structures
    public readonly struct Line2D
    {
        // Normal form: a*x + b*y + c = 0, with sqrt(a^2 + b^2) = 1
        public double A { get; }
        public double B { get; }
        public double C { get; }

        // Optional: store points to recover direction if needed
        public double X1 { get; }
        public double Y1 { get; }
        public double X2 { get; }
        public double Y2 { get; }

        private Line2D(double a, double b, double c, double x1, double y1, double x2, double y2)
        {
            double s = Math.Sqrt(a * a + b * b);
            if (s == 0) throw new ArgumentException("Degenerate line from identical points");
            A = a / s; B = b / s; C = c / s;
            X1 = x1; Y1 = y1; X2 = x2; Y2 = y2;
        }

        /// <summary>
        /// Constructs a <see cref="Line2D"/> in normalized normal form (a*x + b*y + c = 0)
        /// from two given points. This avoids slope calculations and remains stable
        /// even for vertical or horizontal lines.
        /// </summary>
        /// <param name="x1">The x-coordinate of the first point.</param>
        /// <param name="y1">The y-coordinate of the first point.</param>
        /// <param name="x2">The x-coordinate of the second point.</param>
        /// <param name="y2">The y-coordinate of the second point.</param>
        /// <returns>A <see cref="Line2D"/> representing the line through the two points.</returns>
        /// <exception cref="ArgumentException">Thrown if the two points are identical.</exception>
        public static Line2D FromPoints(double x1, double y1, double x2, double y2)
        {
            // a = dy, b = -dx, c = dx*y1 - dy*x1
            double dx = x2 - x1;
            double dy = y2 - y1;
            double a = dy;
            double b = -dx;
            double c = dx * y1 - dy * x1;
            return new Line2D(a, b, c, x1, y1, x2, y2);
        }

        /// <summary>
        /// Computes the intersection point of two lines in normalized normal form.
        /// Returns <c>null</c> if the lines are parallel or coincident within the
        /// specified epsilon tolerance.
        /// </summary>
        /// <param name="L1">The first line.</param>
        /// <param name="L2">The second line.</param>
        /// <param name="eps">Numerical tolerance for determinant near zero (default 1e-9).</param>
        /// <returns>
        /// A tuple (x, y) containing the coordinates of the intersection, or
        /// <c>null</c> if no unique intersection exists.
        /// </returns>
        public static (double x, double y)? Intersect(Line2D L1, Line2D L2, double eps = 1e-9)
        {
            double det = L1.A * L2.B - L2.A * L1.B;
            if (Math.Abs(det) < eps) return null; // parallel or coincident
            double x = (L1.B * L2.C - L2.B * L1.C) / det;
            double y = (L1.C * L2.A - L2.C * L1.A) / det;
            return (x, y);
        }

        /// <summary>
        /// Projects a point (x0, y0) orthogonally onto this line,
        /// returning the coordinates of the perpendicular foot.
        /// </summary>
        /// <param name="x0">The x-coordinate of the point to project.</param>
        /// <param name="y0">The y-coordinate of the point to project.</param>
        /// <returns>A tuple (x, y) giving the projection of the point onto the line.</returns>
        public (double x, double y) ProjectPoint(double x0, double y0)
        {
            // Projection of point onto line in normal form
            // p' = p - (a*x0 + b*y0 + c) * (a, b)
            double d = A * x0 + B * y0 + C;
            return (x0 - d * A, y0 - d * B);
        }
    }
        #endregion
}