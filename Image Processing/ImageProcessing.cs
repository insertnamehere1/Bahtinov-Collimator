
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

namespace Bahtinov_Collimator
{
    internal class ImageProcessing
    {

        // Focus Data Listener
        public delegate void FocusDataEventHandler(object sender, FocusDataEventArgs e);
        public static event FocusDataEventHandler FocusDataEvent;

        // Settings
        private int apertureSetting;
        private int focalLengthSetting;
        private double pixelSizeSetting;

        // to be checked for relevance
        private const float ErrorMarkerScalingValue = 20.0f;

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
            int imagePixelCount = bitmapData.Stride * starImage.Height;
            byte[] starImageArray = new byte[imagePixelCount];

            Marshal.Copy(scan0, starImageArray, 0, imagePixelCount);
            starImage.UnlockBits(bitmapData);

            int width = starImage.Width;
            int height = starImage.Height;
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
                    int pixelIndex = (x + y * width) * bitmapData.Stride / width;
                    byte redValue = starImageArray[pixelIndex];
                    byte greenValue = starImageArray[pixelIndex + 1];
                    byte blueValue = starImageArray[pixelIndex + 2];

                    starArray2D[x, y] = (float)Math.Sqrt((redValue + greenValue + blueValue) * DIVISION_FACTOR);
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








        private float lastFocusErrorValue = 0.0f;



        public bool DisplayLines(BahtinovData lines, Bitmap starImage)
        {
            int numberOfLines = lines.LineAngles.Length;

            if(numberOfLines != 3 && numberOfLines != 9 )
            {
 //               ShowWarningMessage("Unable to detect Bahtinov image lines");
            }

            int numberOfGroups = numberOfLines / 3;

            Rectangle rect = new Rectangle(0, 0, starImage.Width, starImage.Height);
            BahtinovData bahtinovLines = new BahtinovData(3, rect);

            for (int group = 0; group < numberOfGroups; group++)
            {
                int startIndex = 3 * group;
                Array.Copy(lines.LineAngles, startIndex, bahtinovLines.LineAngles, 0, 3);
                Array.Copy(lines.LineIndex, startIndex, bahtinovLines.LineIndex, 0, 3);

                int width = starImage.Width;
                int height = starImage.Height;

                // check for stray LineIndex values
                // this can happen when the image moves quickly or is occulted by another window
                foreach (float lineIndex in bahtinovLines.LineIndex)
                {
                    // Check if x-coordinate is outside the image bounds
                    if (lineIndex < 0 || lineIndex >= height)
                    {
                        return false; // Line angle is outside x-axis range
                    }
                }

                // centre pixels for X and Y in the starImage
                float starImage_X_Centre = (float)(((double)width + 1.0) / 2.0);
                float starImage_Y_Centre = (float)(((double)height + 1.0) / 2.0);

                float first_lineSlope = 0.0f;
                float third_lineSlope = 0.0f;
                float first_calculatedY_for_XStart = 0.0f;
                float third_calculatedY_for_XStart = 0.0f;

                float second_lineStart_X = 0.0f;
                float second_lineStart_Y = 0.0f;
                float second_lineEnd_X = 0.0f;
                float second_lineEnd_Y = 0.0f;

                int penWidth = 2;
                Pen dashPen = new Pen(Color.Yellow, (float)penWidth) { DashStyle = DashStyle.Dash };

                Pen largeDashPen = new Pen(Color.Black, penWidth) { DashStyle = DashStyle.Dash };
                Pen largeSolidPen = new Pen(Color.Yellow, (float)penWidth) { DashStyle = DashStyle.Solid };

                //            Graphics graphics = Graphics.FromImage(pictureBox.Image);

                // check first line is not vertical this will cause computation issues later as the slope is aproaching infinity
                if (bahtinovLines.LineAngles[0] < 1.5708 && bahtinovLines.LineAngles[0] > 1.5707)
                {
                    //reverse the lines
                    float tmp = bahtinovLines.LineAngles[0];
                    float tmp2 = bahtinovLines.LineIndex[0];
                    bahtinovLines.LineAngles[0] = bahtinovLines.LineAngles[2];
                    bahtinovLines.LineIndex[0] = bahtinovLines.LineIndex[2];
                    bahtinovLines.LineAngles[2] = tmp;
                    bahtinovLines.LineIndex[2] = tmp2;
                }

                // for each Bahtinov angle in this line group draw the line on the image
                for (int index = 0; index < bahtinovLines.LineAngles.Length; ++index)
                {
                    // set to the smaller value of the centre point, either x or y
                    float CentrePoint = Math.Min(starImage_X_Centre, starImage_Y_Centre);

                    // calculate the start and end X/Y point for the line
                    float lineStart_X = starImage_X_Centre + -CentrePoint * (float)Math.Cos((double)bahtinovLines.LineAngles[index]) +
                                      (bahtinovLines.LineIndex[index] - starImage_Y_Centre) * (float)Math.Sin((double)bahtinovLines.LineAngles[index]);

                    float lineEnd_X = starImage_X_Centre + CentrePoint * (float)Math.Cos((double)bahtinovLines.LineAngles[index]) +
                                      (bahtinovLines.LineIndex[index] - starImage_Y_Centre) * (float)Math.Sin((double)bahtinovLines.LineAngles[index]);

                    float lineStart_Y = starImage_Y_Centre + -CentrePoint * (float)Math.Sin((double)bahtinovLines.LineAngles[index]) + (float)-((double)bahtinovLines.LineIndex[index] -
                                      (double)starImage_Y_Centre) * (float)Math.Cos((double)bahtinovLines.LineAngles[index]);

                    float lineEnd_Y = starImage_Y_Centre + CentrePoint * (float)Math.Sin((double)bahtinovLines.LineAngles[index]) + (float)-((double)bahtinovLines.LineIndex[index] -
                                      (double)starImage_Y_Centre) * (float)Math.Cos((double)bahtinovLines.LineAngles[index]);

                    // if this is the first line
                    if (index == 0)
                    {
                        float first_lineStart_X = lineStart_X;
                        float first_lineEnd_X = lineEnd_X;
                        float first_lineStart_Y = lineStart_Y;
                        float first_lineEnd_Y = lineEnd_Y;

                        first_lineSlope = (float)(((double)first_lineEnd_Y - (double)first_lineStart_Y) / ((double)first_lineEnd_X - (double)first_lineStart_X));
                        first_calculatedY_for_XStart = (float)(-(double)first_lineStart_X * (((double)first_lineEnd_Y - (double)first_lineStart_Y) /
                                                       ((double)first_lineEnd_X - (double)first_lineStart_X))) + first_lineStart_Y;
                    }

                    // if this is the middle line
                    else if (index == 1)
                    {
                        second_lineStart_X = lineStart_X;
                        second_lineEnd_X = lineEnd_X;
                        second_lineStart_Y = lineStart_Y;
                        second_lineEnd_Y = lineEnd_Y;
                    }

                    // if this is the last line
                    else if (index == 2)
                    {
                        float third_lineStart_X = lineStart_X;
                        float third_lineEnd_X = lineEnd_X;
                        float third_lineStart_Y = lineStart_Y;
                        float third_lineEnd_Y = lineEnd_Y;

                        third_lineSlope = (float)(((double)third_lineEnd_Y - (double)third_lineStart_Y) / ((double)third_lineEnd_X - (double)third_lineStart_X));
                        third_calculatedY_for_XStart = (float)(-(double)third_lineStart_X * (((double)third_lineEnd_Y -
                                                       (double)third_lineStart_Y) / ((double)third_lineEnd_X - (double)third_lineStart_X))) + third_lineStart_Y;
                    }

                    // select pen colour for the lines
                    switch (group)
                    {
                        case 0:
                            dashPen.Color = Color.Red;
                            largeDashPen.Color = Color.Red;
                            break;
                        case 1:
                            dashPen.Color = Color.Green;
                            largeDashPen.Color = Color.Green;
                            break;
                        case 2:
                            dashPen.Color = Color.Blue;
                            largeDashPen.Color = Color.Blue;
                            break;
                    }
                }


                // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>> send line info for line 0, line1 and line2 from here.




                // calculate the line intersections

                // this code calculates the x-coordinate of a point of intersection between two lines. line1 and line3
                float x_intersectionOfLines1and3 = (float)(-((double)first_calculatedY_for_XStart - (double)third_calculatedY_for_XStart) / ((double)first_lineSlope - (double)third_lineSlope));

                // this code calculates the y-coordinate of a point of intersection between two lines. line1 and line3
                float y_intersectionOfLines1and3 = first_lineSlope * x_intersectionOfLines1and3 + first_calculatedY_for_XStart;

                // this code calculates the point on line 2 that is perpendicular to the intersection point
                // we start by finding the length along the line 2 where the perpendicular intersects 
                float line2_PerpendicularDistance = (float)(((double)x_intersectionOfLines1and3 - (double)second_lineStart_X) *
                                                    ((double)second_lineEnd_X - (double)second_lineStart_X) + ((double)y_intersectionOfLines1and3 -
                                                    (double)second_lineStart_Y) * ((double)second_lineEnd_Y - (double)second_lineStart_Y)) /
                                                    (float)(((double)second_lineEnd_X - (double)second_lineStart_X) * ((double)second_lineEnd_X -
                                                    (double)second_lineStart_X) + ((double)second_lineEnd_Y - (double)second_lineStart_Y) *
                                                    ((double)second_lineEnd_Y - (double)second_lineStart_Y));

                // then using this distance calculate the X and Y location on line 2
                float line2_X_Perpendicular = second_lineStart_X + line2_PerpendicularDistance * (second_lineEnd_X - second_lineStart_X);
                float line2_Y_Perpendicular = second_lineStart_Y + line2_PerpendicularDistance * (second_lineEnd_Y - second_lineStart_Y);

                // we then use the line2 perpendicular point and the intersect point and calculate the distance between them 
                float line2ToIntersectDistance = (float)Math.Sqrt(((double)x_intersectionOfLines1and3 - (double)line2_X_Perpendicular) *
                                                 ((double)x_intersectionOfLines1and3 - (double)line2_X_Perpendicular) + ((double)y_intersectionOfLines1and3 -
                                                 (double)line2_Y_Perpendicular) * ((double)y_intersectionOfLines1and3 - (double)line2_Y_Perpendicular));

                // calculate x and y distances of the error line 
                float X_ErrorDistance = x_intersectionOfLines1and3 - line2_X_Perpendicular;
                float Y_ErrorDistance = y_intersectionOfLines1and3 - line2_Y_Perpendicular;

                // calculate the x and y lengths of line2
                float line2_X_Distance = second_lineEnd_X - second_lineStart_X;
                float line2_Y_Distance = second_lineEnd_Y - second_lineStart_Y;

                float errorSign;

                try
                {
                    // determines if the error value is positive or negative
                    errorSign = (float)-Math.Sign((float)((double)X_ErrorDistance * (double)line2_Y_Distance - (double)Y_ErrorDistance * (double)line2_X_Distance));
                }
                catch
                {
                    lastFocusErrorValue = 0.0f;
                    //                LoadImageLost();
                    return false;
                }








                // Calculate the Bahtinov Mask Image Offset in Pixels
                double bahtinovOffset = errorSign * line2ToIntersectDistance;

                // calculate the bahtinov mask angle in degrees
                float radianPerDegree = (float)Math.PI / 180f;

                float bahtinovAngle = Math.Abs((float)(((double)bahtinovLines.LineAngles[2] - (double)bahtinovLines.LineAngles[0]) / 2.0));

                // calculate focus error in microns
                float pixelsPerMicron = (float)(9.0 / 32.0 * (((double)(float)apertureSetting / 1000.0) /
                                          ((double)((float)focalLengthSetting / 1000) *
                                          (double)(float)pixelSizeSetting)) *
                                          (1.0 + Math.Cos(45.0 * (double)radianPerDegree) *
                                          (1.0 + Math.Tan((double)bahtinovAngle))));

                //           float focusErrorInMicrons = errorSign * line2ToIntersectDistance / pixelsPerMicron;
                double focusErrorInMicrons = line2ToIntersectDistance / pixelsPerMicron;



                // calculate if Within Critical Focus 
                float criticalFocusValue = (float)(8.99999974990351E-07 * ((double)((float)focalLengthSetting / 1000) /
                                           (double)((float)apertureSetting / 1000)) * ((double)((float)focalLengthSetting / 1000) /
                                           (double)((float)apertureSetting / 1000)));

                bool withinCriticalFocus = Math.Abs((double)focusErrorInMicrons * 1E-06) < (double)Math.Abs(criticalFocusValue);

                FocusData fd = new FocusData
                {
                    BahtinovOffset = bahtinovOffset,
                    DefocusError = focusErrorInMicrons,
                    InsideFocus = withinCriticalFocus,
                    Id = group
                };

                FocusDataEvent?.Invoke(null, new FocusDataEventArgs(fd));







                // the error marker x,y position
                float errorMarker_X = x_intersectionOfLines1and3 + (float)(((double)line2_X_Perpendicular - (double)x_intersectionOfLines1and3) * ErrorMarkerScalingValue);
                float errorMarker_Y = y_intersectionOfLines1and3 + (float)(((double)line2_Y_Perpendicular - (double)y_intersectionOfLines1and3) * ErrorMarkerScalingValue);

                Pen errorPen;
                if (withinCriticalFocus)
                    errorPen = largeSolidPen;
                else
                    errorPen = dashPen;

                // select pen colour
                switch (group)
                {
                    case 0:
                        errorPen.Color = Color.Red;
                        break;
                    case 1:
                        errorPen.Color = Color.Green;
                        break;
                    case 2:
                        errorPen.Color = Color.Blue;
                        break;
                    default:
                        errorPen.Color = Color.White;
                        break;
                }

                // draw the circular error marker
                errorPen.Width = 5;
                int circleRadius = 64;


                // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>> send Error Cirle from here.



                //            graphics.DrawEllipse(errorPen, errorMarker_X - (float)circleRadius, (float)height - errorMarker_Y - (float)circleRadius + (float)yOffset, (float)(circleRadius * 2), (float)(circleRadius * 2));

                // Calculate the line coordinates
                float lineX1 = errorMarker_X + circleRadius;
                float lineY1 = height - errorMarker_Y;
                float lineX2 = errorMarker_X - circleRadius;
                float lineY2 = height - errorMarker_Y;

                // Error circle internal line

                // Translate the line to the origin
                lineX1 -= errorMarker_X;
                lineY1 -= height - errorMarker_Y;
                lineX2 -= errorMarker_X;
                lineY2 -= height - errorMarker_Y;

                // Rotate the line
                float rotatedX1 = (float)(lineX1 * Math.Cos(-bahtinovLines.LineAngles[1]) - lineY1 * Math.Sin(-bahtinovLines.LineAngles[1]));
                float rotatedY1 = (float)(lineX1 * Math.Sin(-bahtinovLines.LineAngles[1]) + lineY1 * Math.Cos(-bahtinovLines.LineAngles[1]));
                float rotatedX2 = (float)(lineX2 * Math.Cos(-bahtinovLines.LineAngles[1]) - lineY2 * Math.Sin(-bahtinovLines.LineAngles[1]));
                float rotatedY2 = (float)(lineX2 * Math.Sin(-bahtinovLines.LineAngles[1]) + lineY2 * Math.Cos(-bahtinovLines.LineAngles[1]));

                // Translate the line back to its original position
                rotatedX1 += errorMarker_X;
                rotatedY1 += height - errorMarker_Y;
                rotatedX2 += errorMarker_X;
                rotatedY2 += height - errorMarker_Y;

                // Draw the rotated error line in the error circle
                //               graphics.DrawLine(errorPen, rotatedX1, rotatedY1, rotatedX2, rotatedY2);

                float CentrePoint1 = Math.Min(starImage_X_Centre, starImage_Y_Centre);

                float lineStart_X1 = starImage_X_Centre + -CentrePoint1 * (float)Math.Cos((double)bahtinovLines.LineAngles[1]) +
                        (bahtinovLines.LineIndex[1] - starImage_Y_Centre) * (float)Math.Sin((double)bahtinovLines.LineAngles[1]);

                float lineEnd_X1 = starImage_X_Centre + CentrePoint1 * (float)Math.Cos((double)bahtinovLines.LineAngles[1]) +
                                    (bahtinovLines.LineIndex[1] - starImage_Y_Centre) * (float)Math.Sin((double)bahtinovLines.LineAngles[1]);

                float lineStart_Y1 = starImage_Y_Centre + -CentrePoint1 * (float)Math.Sin((double)bahtinovLines.LineAngles[1]) + (float)-((double)bahtinovLines.LineIndex[1] -
                                    (double)starImage_Y_Centre) * (float)Math.Cos((double)bahtinovLines.LineAngles[1]);

                float lineEnd_Y1 = starImage_Y_Centre + CentrePoint1 * (float)Math.Sin((double)bahtinovLines.LineAngles[1]) + (float)-((double)bahtinovLines.LineIndex[1] -
                                    (double)starImage_Y_Centre) * (float)Math.Cos((double)bahtinovLines.LineAngles[1]);

                // display the error value in the pictureBox
                Font font2 = new Font("Arial", 16f);
                SolidBrush solidBrush = new SolidBrush(Color.White);

                //                           Point textStartPoint = new Point((int)lineStart_X1, (int)((float)height - lineStart_Y1 + (float)yOffset));
                //                           Point textEndPoint = new Point((int)lineEnd_X1, (int)((float)height - lineEnd_Y1 + (float)yOffset));

                //            if (textStartPoint.X > pictureBox.Width - 50 || textStartPoint.Y > pictureBox.Height - 50)
                //           {
                //                textStartPoint.X = Math.Min(textStartPoint.X, pictureBox.Width - 50);
                //                textStartPoint.Y = Math.Min(textStartPoint.Y, pictureBox.Height - 50);
                //            }

                //            if (textEndPoint.X > pictureBox.Width - 50 || textEndPoint.Y > pictureBox.Height - 50)
                //            {
                //                textEndPoint.X = Math.Min(textEndPoint.X, pictureBox.Width - 50);
                //                textEndPoint.Y = Math.Min(textEndPoint.Y, pictureBox.Height - 50);
                //            }

                //            Point textLocation = group == 1 ? textEndPoint : textStartPoint;

                //            if (showFocus[group] == true)
                //                graphics.DrawString((errorSign * line2ToIntersectDistance).ToString("F1"), font2, (Brush)solidBrush, (PointF)textLocation);

                // check if the images has moved substantially indicating the image has been lost
                if (lastFocusErrorValue != 0.0f && Math.Abs(lastFocusErrorValue - x_intersectionOfLines1and3) > 200)
                {
                    lastFocusErrorValue = 0.0f;
                    return false;
                }
                lastFocusErrorValue = x_intersectionOfLines1and3;
            }
            return true;
        }
    }
}