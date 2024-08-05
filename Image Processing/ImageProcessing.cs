
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
using System.Security.Cryptography;
using System.Linq;
using System.Drawing.Drawing2D;
using static System.Net.Mime.MediaTypeNames;
using static Bahtinov_Collimator.BahtinovLineDataEventArgs;

namespace Bahtinov_Collimator
{
    internal class ImageProcessing
    {

        // Focus Data Listener
        public delegate void FocusDataEventHandler(object sender, FocusDataEventArgs e);
        public static event FocusDataEventHandler FocusDataEvent;

        // LineDrawing Event Listener
        public delegate void BahtinovLineDrawEventHandler(object sender, BahtinovLineDataEventArgs e);
        public static event BahtinovLineDrawEventHandler BahtinovLineDrawEvent;

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

        public ImageProcessing()
        {
            LoadSettings();
        }

        public void LoadSettings()
        {
            // load settings
            apertureSetting = Properties.Settings.Default.Aperture;
            focalLengthSetting = Properties.Settings.Default.FocalLength;
            pixelSizeSetting = Properties.Settings.Default.PixelSize;
        }

        private byte[] ShaHash(Bitmap image)
        {
            var bytes = new byte[1];
            bytes = (byte[])(new ImageConverter()).ConvertTo(image, bytes.GetType());

            return (new SHA256Managed()).ComputeHash(bytes);
        }

        public bool Equal(Bitmap imageA, Bitmap imageB)
        {
            if (imageA == null || imageB == null)
                return false;

            if (imageA.Width != imageB.Width) return false;
            if (imageA.Height != imageB.Height) return false;

            var hashA = ShaHash(imageA);
            var hashB = ShaHash(imageB);

            return hashA.SequenceEqual(hashB);
        }

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
            float edgeOffset = ((width < height ? width : height) * 0.707f) - 8f;

            // Calculate bounding box edges.
            int leftEdge = (int)(0.5 * (width - edgeOffset));
            int rightEdge = (int)(0.5 * (width + edgeOffset));
            int topEdge = (int)(0.5 * (height - edgeOffset));
            int bottomEdge = (int)(0.5 * (height + edgeOffset));

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

        public Bitmap PadAndCenterCircularBitmap(Bitmap originalBitmap, PictureBox pictureBox)
        {
            Color fillValue;

            // Check if padding is needed
            if ((originalBitmap.Width < pictureBox.Width) || (originalBitmap.Height < pictureBox.Height))
            {
                fillValue = CalculateCircularBorderAverage(originalBitmap);
                originalBitmap = SubtractMeanFromImage(fillValue, originalBitmap);
            }

            fillValue = CalculateCircularBorderAverage(originalBitmap);

            Brush brush = new SolidBrush(fillValue);

            // Create larger bitmap with Mean background
            Bitmap paddedBitmap = new Bitmap(pictureBox.Width, pictureBox.Height);

            using (Graphics graphics = Graphics.FromImage(paddedBitmap))
            {
                // Fill the entire background with the average color
                graphics.FillRectangle(brush, 0, 0, paddedBitmap.Width, paddedBitmap.Height);

                // Calculate padding values
                int horizontalPadding = (paddedBitmap.Width - originalBitmap.Width) / 2;
                int verticalPadding = (paddedBitmap.Height - originalBitmap.Height) / 2;

                // Draw the original circular bitmap onto the larger bitmap with padding
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddEllipse(horizontalPadding, verticalPadding, originalBitmap.Width, originalBitmap.Height);
                    graphics.SetClip(path);
                    graphics.DrawImage(originalBitmap, horizontalPadding, verticalPadding, originalBitmap.Width, originalBitmap.Height);
                    graphics.ResetClip();
                }
            }

            return paddedBitmap;
        }

        private Color CalculateCircularBorderAverage(Bitmap bitmap)
        {
            float totalBrightness = 0;
            int pixelCount = 0;
            int width = bitmap.Width;
            int height = bitmap.Height;

            int centerX = width / 2;
            int centerY = height / 2;
            int radius = Math.Min(centerX, centerY) - 2;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (IsOnCircleBorder(centerX, centerY, radius, x, y))
                    {
                        Color pixelColor = bitmap.GetPixel(x, y);
                        totalBrightness += pixelColor.GetBrightness();
                        pixelCount++;
                    }
                }
            }

            float averageBrightness = totalBrightness / pixelCount;
            int averageGrayValue = (int)(averageBrightness * 255);
            return Color.FromArgb(averageGrayValue, averageGrayValue, averageGrayValue);
        }

        private bool IsOnCircleBorder(int centerX, int centerY, int radius, int x, int y)
        {
            int dx = x - centerX;
            int dy = y - centerY;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            return Math.Abs(distance - radius) <= 1; // Adjust the tolerance as needed
        }

        private Bitmap SubtractMeanFromImage(Color meanColor, Bitmap bitmap)
        {
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            Bitmap newBitmap = new Bitmap(bitmap.Width, bitmap.Height, bitmap.PixelFormat);

            unsafe
            {
                int bytesPerPixel = Bitmap.GetPixelFormatSize(bitmap.PixelFormat) / 8;
                int heightInPixels = bitmapData.Height;
                int widthInBytes = bitmapData.Width * bytesPerPixel;
                byte* ptr = (byte*)bitmapData.Scan0;

                for (int y = 0; y < heightInPixels; y++)
                {
                    byte* currentLine = ptr + (y * bitmapData.Stride);

                    for (int x = 0; x < widthInBytes; x += bytesPerPixel)
                    {
                        int b = currentLine[x];
                        int g = currentLine[x + 1];
                        int r = currentLine[x + 2];

                        // Subtract mean color components
                        r = Math.Max(0, r - meanColor.R);
                        g = Math.Max(0, g - meanColor.G);
                        b = Math.Max(0, b - meanColor.B);

                        currentLine[x] = (byte)b;
                        currentLine[x + 1] = (byte)g;
                        currentLine[x + 2] = (byte)r;
                    }
                }
            }

            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }

        public bool DisplayLines(BahtinovData lines, Bitmap starImage)
        {
            if (lines.LineAngles.Length != 3 && lines.LineAngles.Length != 9)
            {
                DarkMessageBox.Show("Unable to detect Bahtinov image lines", "Display Error", MessageBoxIcon.Error, MessageBoxButtons.OK);
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
                    lastFocusErrorValue = 0.0f;
                    DarkMessageBox.Show("Image has been lost", "Display Error", MessageBoxIcon.Error, MessageBoxButtons.OK);
                    return false;
                }

                double bahtinovOffset = errorSign * errorDistance;

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

                float errorMarker_X = xIntersection + (perpendicularX - xIntersection) * ErrorMarkerScalingValue;
                float errorMarker_Y = yIntersection + (perpendicularY - yIntersection) * ErrorMarkerScalingValue;

                int circleRadius = UITheme.ErrorCircleRadius;
                int circle_x = (int)(errorMarker_X - circleRadius);
                int circle_y = (int)(height - errorMarker_Y - circleRadius + yOffset);
                int circle_width = circleRadius * 2;
                int circle_height = circleRadius * 2;
                string errorValue = (errorSign * errorDistance).ToString("F1");

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
    }
}