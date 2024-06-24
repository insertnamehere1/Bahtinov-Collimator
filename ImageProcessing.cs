
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

namespace Bahtinov_Collimator
{
    internal static class ImageProcessing
    {
        public static byte[] ShaHash(this Bitmap image)
        {
            var bytes = new byte[1];
            bytes = (byte[])(new ImageConverter()).ConvertTo(image, bytes.GetType());

            return (new SHA256Managed()).ComputeHash(bytes);
        }
        public static bool Equal(Bitmap imageA, Bitmap imageB)
        {
            if (imageA == null || imageB == null)
                return false;

            if (imageA.Width != imageB.Width) return false;
            if (imageA.Height != imageB.Height) return false;

            var hashA = imageA.ShaHash();
            var hashB = imageB.ShaHash();

            return hashA.SequenceEqual(hashB);
        }

        public static BahtinovData FindBrightestLines(Bitmap starImage, int lineCount)
        {
            Rectangle rect = new Rectangle(0, 0, starImage.Width, starImage.Height);
            BahtinovData result = new BahtinovData(lineCount, rect);
            BitmapData bitmapdata = starImage.LockBits(rect, ImageLockMode.ReadWrite, starImage.PixelFormat);

            IntPtr scan0 = bitmapdata.Scan0;
            int imagePixelCount = bitmapdata.Stride * starImage.Height;
            byte[] starImageArray = new byte[imagePixelCount];

            Marshal.Copy(scan0, starImageArray, 0, imagePixelCount);
            Marshal.Copy(starImageArray, 0, scan0, imagePixelCount);

            starImage.UnlockBits(bitmapdata);

            // calculates the coordinates of a bounding box for the starImage based
            // on its dimensions, which can be used to enclose a specific region of the image.
            // The size will allow it to be rotated without the corners clipping outside the original image
            int width = starImage.Width;
            int height = starImage.Height;

            // calculate the offset from the smaller of the width/height halve it and then multiply by 1.414 and subtract 8
            // this results in an offset from the edge for a square bounding box.
            float edgeOffset = (width < height ? 0.5f * (float)Math.Sqrt(2.0) * (float)width : 0.5f * (float)Math.Sqrt(2.0) * (float)height) - 8f;

            // calculate edges of the bounding box
            int leftEdge = (int)(0.5 * ((double)width - (double)edgeOffset));
            int rightEdge = (int)(0.5 * ((double)width + (double)edgeOffset));
            int topEdge = (int)(0.5 * ((double)height - (double)edgeOffset));
            int bottomEdge = (int)(0.5 * ((double)height + (double)edgeOffset));

            // create a new 2D array of floats the size of the star Image
            float[,] starArray2D = new float[width, height];

            float pixelValueTotal = 0.0f;
            float pixelsCounted = 0.0f;

            // iterate through the pixels enclosed in the bounding box
            // and copy to starArray for later use
            //
            // alse get the total value of all the pixels enclosed, it normalizes the value and then find the average
            // this will create a MEAN pixel value
            float divisionFactor = 3f / byte.MaxValue;

            for (int x = leftEdge; x < rightEdge; ++x)
            {
                for (int y = topEdge; y < bottomEdge; ++y)
                {
                    int pixelIndex = (x + y * width) * bitmapdata.Stride / width;
                    byte redValue = starImageArray[pixelIndex];
                    byte greenValue = starImageArray[pixelIndex + 1];
                    byte blueValue = starImageArray[pixelIndex + 2];

                    starArray2D[x, y] = redValue + greenValue + blueValue;
                    starArray2D[x, y] *= divisionFactor;
                    starArray2D[x, y] = (float)Math.Sqrt(starArray2D[x, y]);
                    pixelValueTotal += starArray2D[x, y];
                    ++pixelsCounted;
                }
            }

            // centre pixels for X and Y in the starImage
            float starImage_X_Centre = (float)(((double)width + 1.0) / 2.0);
            float starImage_Y_Centre = (float)(((double)height + 1.0) / 2.0);

            // initialize the results arrays
            float[] local_BahtinovAngles = new float[3];
            float[] bahtinovLineNumbers = new float[3];

            // number of degrees to rotate?
            int degrees180 = 180;

            float radiansPerDegree = 3.141593f / (float)degrees180;

            // store the brightest line details for all 180 rotations
            float[] lineNumbersOfBrightestLines = new float[degrees180];
            float[] valuesOfBrightestLines = new float[degrees180];

            //
            // loop for 180 degrees in 1 degree steps
            // rotate and smooth the bounding box image
            // 

            // for (int index1 = 0; index1 < degrees180; ++index1)
            Parallel.For(0, degrees180, index1 =>
            {
                float[,] rotatedImage = new float[width, height];

                // work out the angles of the current value of radians.
                float currentRadians = radiansPerDegree * (float)index1;
                float sin_CurrentRadians = (float)Math.Sin((double)currentRadians);
                float cos_CurrentRadians = (float)Math.Cos((double)currentRadians);

                // Rotate the bounding box image by the amount specified and smooth the rotated image.

                int index2 = leftEdge;

                // iterate from left edge to right edge of bounding box
                while (index2 < rightEdge)
                {
                    // iterate from top to bottom of the bounding box
                    int index3 = topEdge;
                    while (index3 < bottomEdge)
                    {
                        float X_DistanceFromCentre = index2 - starImage_X_Centre;
                        float Y_DistanceFromCentre = index3 - starImage_Y_Centre;
                        float cosRadians = cos_CurrentRadians;
                        float sinRadians = sin_CurrentRadians;

                        float rotatedXDistance = starImage_X_Centre + X_DistanceFromCentre * cosRadians + Y_DistanceFromCentre * sinRadians;
                        float rotatedYDistance = starImage_Y_Centre - X_DistanceFromCentre * sinRadians + Y_DistanceFromCentre * cosRadians;

                        int rotatedXRoundedDown = (int)rotatedXDistance;
                        int rotatedXRoundedUp = rotatedXRoundedDown + 1;
                        int rotatedYRoundedDown = (int)rotatedYDistance;
                        int rotatedYRoundedUp = rotatedYRoundedDown + 1;


                        float rotatedXFraction = rotatedXDistance - (float)rotatedXRoundedDown;
                        float rotatedYFraction = rotatedYDistance - (float)rotatedYRoundedDown;

                        // This line calculates the interpolated value at the current position (index2 and rowIndex) using the surrounding values in the starArray2D. It performs bilinear interpolation
                        // to estimate the value based on the fractional parts (rotatedXFraction and rotatedYFraction) and the four surrounding integer indices (rotatedXRoundedDown, rotatedXRoundedUp,
                        // rotatedYRoundedDown, and rotatedYRoundedUp).
                        float oneMinusRotatedXFraction = 1.0f - rotatedXFraction;
                        float oneMinusRotatedYFraction = 1.0f - rotatedYFraction;
                        float value1 = starArray2D[rotatedXRoundedDown, rotatedYRoundedDown];
                        float value2 = starArray2D[rotatedXRoundedUp, rotatedYRoundedDown];
                        float value3 = starArray2D[rotatedXRoundedUp, rotatedYRoundedUp];
                        float value4 = starArray2D[rotatedXRoundedDown, rotatedYRoundedUp];

                        rotatedImage[index2, index3] = value1 * oneMinusRotatedXFraction * oneMinusRotatedYFraction +
                                                    value2 * rotatedXFraction * oneMinusRotatedYFraction +
                                                    value3 * rotatedXFraction * rotatedYFraction +
                                                    value4 * oneMinusRotatedXFraction * rotatedYFraction;

                        index3++; // next column
                    }
                    index2++; // next row
                }

                // create an array for each row in the image
                float[] rowTotals = new float[height];

                // clear the rowTotals 
                for (int rowIndex = 0; rowIndex < height; ++rowIndex)
                    rowTotals[rowIndex] = 0.0f;

                // set index to top of the bounding box
                int rowCount = topEdge;

                for (rowCount = 0; rowCount < bottomEdge; rowCount++)
                {
                    int pixelCount = 0;
                    float rowTotal = 0;

                    for (int columnIndex = leftEdge; columnIndex < rightEdge; columnIndex++)
                    {
                        float pixelValue = rotatedImage[columnIndex, rowCount];
                        rowTotal += pixelValue;
                        pixelCount++;
                    }

                    float averageRowTotal = rowTotal / (float)pixelCount;
                    rowTotals[rowCount] = averageRowTotal;
                }

                // can be changed to effect smoothing of the detected line values
                int constant_1 = 1;

                // create an array for each row in the image
                float[] tempTotals = new float[height];

                // copy rowTotals from the current loop into saved totals
                // dont use the top edge and bottom rows as they are not smoothed
                for (int rowIndex = 0; rowIndex < height; ++rowIndex)
                    tempTotals[rowIndex] = rowTotals[rowIndex];

                // loop from top edge to bottom edge
                for (int rowIndex = topEdge; rowIndex < bottomEdge; ++rowIndex)
                {
                    // clear saved totals only from the rows inside the bounding box
                    tempTotals[rowIndex] = 0.0f;

                    for (int index4 = -(constant_1 - 1) / 2; index4 <= (constant_1 - 1) / 2; ++index4)
                        tempTotals[rowIndex] += rowTotals[rowIndex + index4] / (float)constant_1;
                }

                // copy back to rowTotals
                for (int index3 = 0; index3 < height; ++index3)
                    rowTotals[index3] = tempTotals[index3];

                // largest values found
                float LargestValueForThisRotation = -1f;
                float lineNumberOfLargestValue = -1f;

                // iterate from top row to bottom row 
                for (int index3 = topEdge; index3 < bottomEdge; ++index3)
                {
                    // find the largest row value in the image
                    if ((double)rowTotals[index3] > (double)LargestValueForThisRotation)
                    {
                        lineNumberOfLargestValue = (float)index3;
                        LargestValueForThisRotation = rowTotals[index3];
                    }
                }

                lineNumbersOfBrightestLines[index1] = lineNumberOfLargestValue;
                valuesOfBrightestLines[index1] = LargestValueForThisRotation;

            });

            //
            // find the number of brightest lines required 
            //

            int currentBrightestLineCount = 0;

            // find the number of brightest lines specified in lineCount parameter
            for (int index1 = 0; index1 < lineCount; ++index1)
            {
                // initialize 
                float lineValue = -1f;
                float angleInRadians = -1f;
                float lineNumber = -1f;

                // loop 180 times, 1 for each degree
                for (int index2 = 0; index2 < degrees180; ++index2)
                {
                    // find the brightest line
                    if ((double)valuesOfBrightestLines[index2] > (double)lineValue)
                    {
                        lineValue = valuesOfBrightestLines[index2];
                        lineNumber = lineNumbersOfBrightestLines[index2];
                        angleInRadians = (float)index2 * radiansPerDegree;
                        currentBrightestLineCount = index2;
                    }
                }

                // store the largest values
                result.LineIndex[index1] = lineNumber;
                result.LineAngles[index1] = angleInRadians;
                result.LineValue[index1] = lineValue;

                // computes to 5 degrees, value = 5
                int deg_5 = 5; // (int)(0.0872664600610733 / (double)radiansPerDegree);  // rad/deg 0.0174532925

                // clears any lines within +/- 5 degrees of the current brightest line
                for (int index2 = currentBrightestLineCount - deg_5; index2 <= currentBrightestLineCount + deg_5; ++index2)
                {
                    // set the lines withing 5 degrees to 0.0
                    int index3 = (index2 + degrees180) % degrees180;
                    valuesOfBrightestLines[index3] = 0.0f;
                }
            }

            return result;
        }

        public static BahtinovData FindSubpixelLines(Bitmap starImage, int lineCount, BahtinovData bahtinovLines)
        {
            Rectangle rect = new Rectangle(0, 0, starImage.Width, starImage.Height);

            // copy the star image pixels into an array 
            BitmapData bitmapdata = starImage.LockBits(rect, ImageLockMode.ReadWrite, starImage.PixelFormat);

            IntPtr scan0 = bitmapdata.Scan0;
            int imagePixelCount = bitmapdata.Stride * starImage.Height;
            byte[] starImageArray = new byte[imagePixelCount];

            Marshal.Copy(scan0, starImageArray, 0, imagePixelCount);
            Marshal.Copy(starImageArray, 0, scan0, imagePixelCount);

            starImage.UnlockBits(bitmapdata);

            // calculates the coordinates of a bounding box for the starImage based
            // on its dimensions, which can be used to enclose a specific region of the image.
            // The size will allow it to be rotated without the corners clipping outside the original image
            int width = starImage.Width;
            int height = starImage.Height;

            // calculate the offset from the smaller of the width/height halve it and then multiply by 1.414 and subtract 8
            // this results in an offset from the edge for a square bounding box.
            float edgeOffset = (width < height ? 0.5f * (float)Math.Sqrt(2.0) * (float)width : 0.5f * (float)Math.Sqrt(2.0) * (float)height) - 8f;

            // calculate edges of the bounding box
            int leftEdge = (int)(0.5 * ((double)width - (double)edgeOffset));
            int rightEdge = (int)(0.5 * ((double)width + (double)edgeOffset));
            int topEdge = (int)(0.5 * ((double)height - (double)edgeOffset));
            int bottomEdge = (int)(0.5 * ((double)height + (double)edgeOffset));

            // centre pixels for X and Y in the starImage
            float starImage_X_Centre = (float)(((double)width + 1.0) / 2.0);
            float starImage_Y_Centre = (float)(((double)height + 1.0) / 2.0);

            float divisionFactor = 3f / byte.MaxValue;

            // create a new 2D array of floats the size of the star Image
            float[,] starArray2D = new float[width, height];

            float pixelValueTotal = 0.0f;
            float pixelsCounted = 0.0f;

            // iterate through the pixels enclosed in the bounding box
            // and copy to starArray for later use
            //
            // alse get the total value of all the pixels enclosed, it normalizes the value and then find the average
            // this will create a MEAN pixel value

            for (int x = leftEdge; x < rightEdge; ++x)
            {
                for (int y = topEdge; y < bottomEdge; ++y)
                {
                    int pixelIndex = (x + y * width) * bitmapdata.Stride / width;
                    byte redValue = starImageArray[pixelIndex];
                    byte greenValue = starImageArray[pixelIndex + 1];
                    byte blueValue = starImageArray[pixelIndex + 2];

                    starArray2D[x, y] = redValue + greenValue + blueValue;
                    starArray2D[x, y] *= divisionFactor;
                    starArray2D[x, y] = (float)Math.Sqrt(starArray2D[x, y]);
                    pixelValueTotal += starArray2D[x, y];
                    ++pixelsCounted;
                }
            }

            float[] subpixelLineNumbers = new float[lineCount];
            float[] brightestLineValues = new float[lineCount];

            Parallel.For(0, lineCount, index1 =>
            {
                float[,] imageArray = new float[width, height];

                // the angle we are checking
                float knownAngle = bahtinovLines.GetLineAngle(index1);

                // work out the angles of the current lineAngle
                float sin_CurrentRadians = (float)Math.Sin((double)knownAngle);
                float cos_CurrentRadians = (float)Math.Cos((double)knownAngle);

                // Rotate the bounding box image by the amount specified and smooth the rotated image.

                // left edge of the bounding box
                int index2 = leftEdge;

                // loop through from the left edge of the bounding box to the right edge of the bounding box
                while (index2 < rightEdge)
                {
                    // top edge of the bounding box
                    int index3 = topEdge;

                    // loop through from the top edge to the bottom edge of the bounding box
                    while (index3 < bottomEdge)
                    {
                        float X_DistanceFromCentre = (float)index2 - starImage_X_Centre;
                        float Y_DistanceFromCentre = (float)index3 - starImage_Y_Centre;

                        float rotatedXDistance = (float)(starImage_X_Centre + X_DistanceFromCentre * cos_CurrentRadians +
                                                    Y_DistanceFromCentre * sin_CurrentRadians);

                        float rotatedYDistance = (float)(starImage_Y_Centre - X_DistanceFromCentre * sin_CurrentRadians +
                                                Y_DistanceFromCentre * cos_CurrentRadians);

                        // round up and down
                        int rotatedXRoundedDown = (int)rotatedXDistance;
                        int rotatedXRoundedUp = rotatedXRoundedDown + 1;
                        int rotatedYRoundedDown = (int)rotatedYDistance;
                        int rotatedYRoundedUp = rotatedYRoundedDown + 1;

                        float rotatedXFraction = rotatedXDistance - (float)rotatedXRoundedDown;
                        float rotatedYFraction = rotatedYDistance - (float)rotatedYRoundedDown;

                        // This line calculates the interpolated value at the current position (index2 and rowIndex) using the surrounding values in the starArray2D. It performs bilinear interpolation
                        // to estimate the value based on the fractional parts (rotatedXFraction and rotatedYFraction) and the four surrounding integer indices (rotatedXRoundedDown, rotatedXRoundedUp,
                        // rotatedYRoundedDown, and rotatedYRoundedUp).
                        float value1 = starArray2D[rotatedXRoundedDown, rotatedYRoundedDown];
                        float value2 = starArray2D[rotatedXRoundedUp, rotatedYRoundedDown];
                        float value3 = starArray2D[rotatedXRoundedUp, rotatedYRoundedUp];
                        float value4 = starArray2D[rotatedXRoundedDown, rotatedYRoundedUp];
                        double xFraction = rotatedXFraction;
                        double yFraction = rotatedYFraction;
                        double oneMinusXFraction = 1.0 - rotatedXFraction;
                        double oneMinusYFraction = 1.0 - rotatedYFraction;

                        imageArray[index2, index3] = (float)(
                            value1 * oneMinusXFraction * oneMinusYFraction +
                            value2 * xFraction * oneMinusYFraction +
                            value3 * xFraction * yFraction +
                            value4 * oneMinusXFraction * yFraction
                        );

                        index3++;  // next column
                    }
                    index2++; // next line
                }

                // create an array for each row in the image
                float[] rowTotals = new float[height];

                // clear the rowTotals
                for (int rowIndex = 0; rowIndex < height; ++rowIndex)
                    rowTotals[rowIndex] = 0.0f;

                // set index to top of the bounding box
                int rowCount = topEdge;

                // iterate through to rows from top to bottom of the bounding box
                while (rowCount < bottomEdge)
                {
                    int pixelCount = 0;
                    int columnIndex = leftEdge;

                    // loop from left to right of the bounding box
                    while (columnIndex < rightEdge)
                    {
                        // total up all the pixels in a row
                        rowTotals[rowCount] += imageArray[columnIndex, rowCount];
                        ++pixelCount;
                        columnIndex += 1;
                    }

                    // divide row pixel totals by pixel count
                    // to get the average pixel value for that row
                    rowTotals[rowCount] /= (float)pixelCount;
                    rowCount += 1;
                }

                float LargestValueForThisRotation = -1f;
                float lineNumberOfLargestValue = -1f;

                // loop from top to bottom on the bounding box
                for (int rowIndex = topEdge; rowIndex < bottomEdge; ++rowIndex)
                {
                    // find the row with the highest average brightness
                    if ((double)rowTotals[rowIndex] > (double)LargestValueForThisRotation)
                    {
                        lineNumberOfLargestValue = (float)rowIndex;
                        LargestValueForThisRotation = rowTotals[rowIndex];
                    }
                }

                // calculate the sub pixel location of the line
                float subPixelLineNumber = (float)PolynomialCurveFit.FindMaxValueIndex(rowTotals, (int)lineNumberOfLargestValue, 2);

                // save the accurate line number and the line brightness value
                subpixelLineNumbers[index1] = subPixelLineNumber;
                brightestLineValues[index1] = LargestValueForThisRotation;
            });

            // copy final 3 brightest lines and the accurate line number
            for (int index = 0; index < lineCount; ++index)
            {
                // store the largest values
                bahtinovLines.LineIndex[index] = subpixelLineNumbers[index];
            }

            return bahtinovLines;
        }

        public static Bitmap PadAndCenterCircularBitmap(Bitmap originalBitmap, PictureBox pictureBox)
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

        private static Color CalculateCircularBorderAverage(Bitmap bitmap)
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

        private static bool IsOnCircleBorder(int centerX, int centerY, int radius, int x, int y)
        {
            int dx = x - centerX;
            int dy = y - centerY;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            return Math.Abs(distance - radius) <= 1; // Adjust the tolerance as needed
        }

        private static Bitmap SubtractMeanFromImage(Color meanColor, Bitmap bitmap)
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
    }
}