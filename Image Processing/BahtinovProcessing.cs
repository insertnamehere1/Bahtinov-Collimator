
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

using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Bahtinov_Collimator.BahtinovLineDataEventArgs;
using System.Collections.Generic;

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
        private int apertureSetting;
        private int focalLengthSetting;
        private double pixelSizeSetting;

        // to be checked for relevance
        private const float ErrorMarkerScalingValue = 20.0f;

        // single pixel offset
        private const int yOffset = 1;

        // retain last error value
        private float lastFocusErrorValue = 0.0f;
        #endregion

        #region Public Fields
        public static Dictionary<int, double> errorValues { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="BahtinovProcessing"/> class. 
        /// Sets up an empty dictionary for storing error values and loads settings from a configuration source.
        /// </summary>
        public BahtinovProcessing()
        {
            errorValues = new Dictionary<int, double>();
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

                float largestValue = -1f;
                float lineNumber = -1f;
                for (int y = topEdge; y < bottomEdge; ++y)
                {
                    if (smoothedTotals[y] > largestValue)
                    {
                        largestValue = smoothedTotals[y];
                        lineNumber = y;
                    }
                }

                lineNumbersOfBrightestLines[index1] = lineNumber;
                valuesOfBrightestLines[index1] = largestValue;
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
                        angleInRadians = j * RADIANS_PER_DEGREE;
                        brightestLineIndex = j;
                    }
                }

                result.LineIndex[i] = lineNumber;
                result.LineAngles[i] = angleInRadians;
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

                float subPixelLineNumber = (float)PolynomialCurveFit.FindMaxValueIndex(rowTotals, (int)lineNumberOfLargestValue, 2);
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
        /// Processes and displays detected Bahtinov lines on the provided star image.
        /// 
        /// The method performs the following steps:
        /// 1. Validates the number of detected lines, ensuring it is either 3 or 9. Logs an error and returns false if the number is incorrect.
        /// 2. Initializes data structures for processing and calculates image center coordinates.
        /// 3. For each group of lines:
        ///    - Reorders lines if necessary.
        ///    - Computes the start and end points of each line.
        ///    - Calculates intersections and error distances related to focus.
        ///    - Determines if the focus error is within an acceptable range.
        ///    - Prepares visual markers for displaying the error and lines.
        /// 4. Invokes events to update the user interface or other components with the processed line data and error information.
        /// 
        /// Returns true if the lines were successfully processed and displayed; otherwise, false.
        /// 
        /// <param name="lines">The <see cref="BahtinovData"/> object containing the detected line angles and indices.</param>
        /// <param name="starImage">The bitmap image of the star on which the lines are to be displayed.</param>
        /// <returns>True if the lines were successfully processed and displayed; otherwise, false.</returns>
        /// </summary>
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

            for (int group = 0; group < numberOfGroups; group++)
            {
                BahtinovLineDataEventArgs.LineGroup lineGroup = new BahtinovLineDataEventArgs.LineGroup(group);

                int startIndex = 3 * group;
                Array.Copy(lines.LineAngles, startIndex, bahtinovLines.LineAngles, 0, 3);
                Array.Copy(lines.LineIndex, startIndex, bahtinovLines.LineIndex, 0, 3);

                if (Math.Abs(bahtinovLines.LineAngles[0] - 1.5708) < 0.0001)
                {
                    float tmp = bahtinovLines.LineAngles[0];
                    float tmp2 = bahtinovLines.LineIndex[0];
                    bahtinovLines.LineAngles[0] = bahtinovLines.LineAngles[2];
                    bahtinovLines.LineIndex[0] = bahtinovLines.LineIndex[2];
                    bahtinovLines.LineAngles[2] = tmp;
                    bahtinovLines.LineIndex[2] = tmp2;
                }

                float firstLineSlope = 0.0f, thirdLineSlope = 0.0f;
                float firstYForXStart = 0.0f, thirdYForXStart = 0.0f;
                float secondLineStartX = 0.0f, secondLineStartY = 0.0f;
                float secondLineEndX = 0.0f, secondLineEndY = 0.0f;

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

                    BahtinovLineDataEventArgs.Line bahtinovLine = new BahtinovLineDataEventArgs.Line(
                        new Point((int)Math.Round(lineStart_X), (int)Math.Round(height - lineStart_Y + yOffset)),
                        new Point((int)Math.Round(lineEnd_X), (int)Math.Round(height - lineEnd_Y + yOffset)), index
                    );
                    lineGroup.AddLine(bahtinovLine);

                    switch (index)
                    {
                        case 0:
                            firstLineSlope = (lineEnd_Y - lineStart_Y) / (lineEnd_X - lineStart_X);
                            firstYForXStart = -(lineStart_X * (lineEnd_Y - lineStart_Y) / (lineEnd_X - lineStart_X)) + lineStart_Y;
                            break;
                        case 1:
                            secondLineStartX = lineStart_X;
                            secondLineEndX = lineEnd_X;
                            secondLineStartY = lineStart_Y;
                            secondLineEndY = lineEnd_Y;
                            break;
                        case 2:
                            thirdLineSlope = (lineEnd_Y - lineStart_Y) / (lineEnd_X - lineStart_X);
                            thirdYForXStart = -(lineStart_X * (lineEnd_Y - lineStart_Y) / (lineEnd_X - lineStart_X)) + lineStart_Y;
                            break;
                    }
                }

                float xIntersection = -(firstYForXStart - thirdYForXStart) / (firstLineSlope - thirdLineSlope);
                float yIntersection = firstLineSlope * xIntersection + firstYForXStart;

                float perpendicularDistance = ((xIntersection - secondLineStartX) * (secondLineEndX - secondLineStartX) +
                                               (yIntersection - secondLineStartY) * (secondLineEndY - secondLineStartY)) /
                                              ((secondLineEndX - secondLineStartX) * (secondLineEndX - secondLineStartX) +
                                               (secondLineEndY - secondLineStartY) * (secondLineEndY - secondLineStartY));

                float perpendicularX = secondLineStartX + perpendicularDistance * (secondLineEndX - secondLineStartX);
                float perpendicularY = secondLineStartY + perpendicularDistance * (secondLineEndY - secondLineStartY);

                float errorDistance = (float)Math.Sqrt((xIntersection - perpendicularX) * (xIntersection - perpendicularX) +
                                                       (yIntersection - perpendicularY) * (yIntersection - perpendicularY));

                float xErrorDistance = xIntersection - perpendicularX;
                float yErrorDistance = yIntersection - perpendicularY;
                float xDistance = secondLineEndX - secondLineStartX;
                float yDistance = secondLineEndY - secondLineStartY;

                float errorSign;
                try
                {
                    errorSign = -Math.Sign(xErrorDistance * yDistance - yErrorDistance * xDistance);
                }
                catch
                {
                    ImageLostEventProvider.OnImageLost("Invalid Error calculation", "Bahtinov Processing", MessageBoxIcon.Error, MessageBoxButtons.OK);
                    lastFocusErrorValue = 0.0f;
                    return false;
                }

                double bahtinovOffset = errorSign * Math.Floor(errorDistance * 10) / 10 ;

                float radianPerDegree = (float)Math.PI / 180f;
                float bahtinovAngle = Math.Abs((bahtinovLines.LineAngles[2] - bahtinovLines.LineAngles[0]) / 2.0f);

                float pixelsPerMicron = (float)(9.0f / 32.0f * (apertureSetting / 1000.0f) /
                                        ((focalLengthSetting / 1000.0f) * pixelSizeSetting) *
                                        (1.0f + Math.Cos(45.0f * radianPerDegree) * (1.0f + (float)Math.Tan(bahtinovAngle))));

                double focusErrorInMicrons = errorDistance / pixelsPerMicron;

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

                FocusDataEvent?.Invoke(null, new FocusDataEventArgs(fd));
                errorValues[group] = bahtinovOffset;

                float errorMarker_X = xIntersection + (perpendicularX - xIntersection) * ErrorMarkerScalingValue;
                float errorMarker_Y = yIntersection + (perpendicularY - yIntersection) * ErrorMarkerScalingValue;

                int circleRadius = UITheme.ErrorCircleRadius;
                int circle_x = (int)(errorMarker_X - circleRadius);
                int circle_y = (int)(height - errorMarker_Y - circleRadius + yOffset);
                int circle_width = circleRadius * 2;
                int circle_height = circleRadius * 2;
                string errorValue = (errorSign * (Math.Floor(errorDistance * 10)/10)).ToString("F1");

                lineGroup.ErrorCircle = new BahtinovLineDataEventArgs.ErrorCircle(new Point(circle_x, circle_y), circle_width, circle_height, withinCriticalFocus, errorValue);

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

                lineGroup.ErrorLine = new BahtinovLineDataEventArgs.ErrorLine(new Point((int)rotatedX1, (int)rotatedY1), new Point((int)rotatedX2, (int)rotatedY2));
                displayLines.AddLineGroup(lineGroup);

                lastFocusErrorValue = xIntersection;
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
        #endregion
    }
}