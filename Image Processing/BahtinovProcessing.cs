
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
        // to be checked for relevance
        private const float ErrorMarkerScalingValue = 20.0f;

        // retain last error value
        private float lastFocusErrorValue = 0.0f;

        // line validator
        private BahtinovIntersectionValidator lineValidator = new BahtinovIntersectionValidator(6, 2);

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
        /// Attempts to detect very faint Bahtinov / Tri-Bahtinov diffraction spikes by scanning all angles and
        /// selecting row-peaks using a local-contrast (z-score) metric rather than raw brightness.
        ///
        /// The method works as follows:
        /// 1. Converts the bitmap into a float luminance image with a gentle gamma (sqrt) to lift faint structure.
        /// 2. Applies a soft central mask (core + feather) to suppress the saturated star core and halo dominance,
        ///    improving the visibility of faint spikes in the projection profile.
        /// 3. For each angle from 0..179 degrees, rotates the image (bilinear sampling) and computes a 1D profile
        ///    of mean brightness per row.
        /// 4. Smooths the 1D profile with a 5-tap kernel to reduce noise.
        /// 5. Computes a local z-score for each row using a sliding window mean and standard deviation.
        ///    This emphasizes narrow peaks that stand out from their local background.
        /// 6. Picks the strongest local peak for each angle and finally selects the best 9 angles, suppressing
        ///    nearby angles (±5°) to avoid duplicates.
        ///
        /// This function is intended as a fallback when the normal bright-line detector fails, because it is more
        /// sensitive but can be more prone to selecting spurious peaks in high-contrast or unusual images.
        /// </summary>
        public BahtinovData FindFaintLines(Bitmap starImage)
        {
            const int LINES = 9;
            const int DEGREES180 = 180;
            const float RADIANS_PER_DEGREE = (float)Math.PI / DEGREES180;
            const int DEG_5 = 5;

            // Profile processing parameters (tweak if you like)
            const int LOCAL_STATS_WINDOW = 23;     // odd number: 17..31 are typical
            const float EPS_STD = 1e-6f;

            // Core suppression: mask a small central disk (reduces halo/core dominance)
            // 0.06..0.12 are typical depending on star size and crop.
            const float CORE_RADIUS_FRACTION = 0.08f;
            const float CORE_FEATHER_FRACTION = 0.03f;

            Rectangle rect = new Rectangle(0, 0, starImage.Width, starImage.Height);
            BahtinovData result = new BahtinovData(LINES, rect);

            BitmapData bitmapData = starImage.LockBits(rect, ImageLockMode.ReadWrite, starImage.PixelFormat);

            try
            {
                IntPtr scan0 = bitmapData.Scan0;
                int stride = bitmapData.Stride;
                int bytesPerPixel = System.Drawing.Image.GetPixelFormatSize(starImage.PixelFormat) / 8;
                int width = starImage.Width;
                int height = starImage.Height;

                // ------------------------------------------------------------
                // Convert to float luminance image (with gentle gamma), and
                // suppress the saturated core so faint spikes can win.
                // ------------------------------------------------------------
                float[,] starArray2D = new float[width, height];

                float cx = (width - 1) * 0.5f;
                float cy = (height - 1) * 0.5f;
                float minDim = Math.Min(width, height);
                float coreRadius = CORE_RADIUS_FRACTION * minDim;
                float coreFeather = CORE_FEATHER_FRACTION * minDim;

                unsafe
                {
                    byte* basePtr = (byte*)scan0.ToPointer();

                    for (int y = 0; y < height; y++)
                    {
                        byte* rowPtr = basePtr + (y * stride);
                        for (int x = 0; x < width; x++)
                        {
                            byte* px = rowPtr + (x * bytesPerPixel);

                            // Most formats you’ll see here are 24/32bpp (BGR/BGRA).
                            // Fallback: if bytesPerPixel < 3, just treat first as intensity.
                            float r, g, b;
                            if (bytesPerPixel >= 3)
                            {
                                b = px[0];
                                g = px[1];
                                r = px[2];
                            }
                            else
                            {
                                r = g = b = px[0];
                            }

                            // Luminance
                            float lum = (0.299f * r + 0.587f * g + 0.114f * b) / 255f;

                            // Gentle gamma to lift faint structure
                            float v = (float)Math.Sqrt(Math.Max(0.0f, lum));

                            // Core mask (soft)
                            float dx = x - cx;
                            float dy = y - cy;
                            float rr = (float)Math.Sqrt(dx * dx + dy * dy);

                            float w = 1.0f;
                            if (rr <= coreRadius)
                            {
                                w = 0.0f;
                            }
                            else if (rr < coreRadius + coreFeather)
                            {
                                // Linear feather from 0 -> 1
                                w = (rr - coreRadius) / coreFeather;
                            }

                            starArray2D[x, y] = v * w;
                        }
                    }
                }

                // Compute ROI used for row projections (keep your original "safe" margins)
                int leftEdge = 0;
                int rightEdge = width;
                int topEdge = 0;
                int bottomEdge = height;

                // If you have a known crop logic elsewhere, keep it. Otherwise this uses full image.
                // You can tighten edges to reduce border interpolation noise:
                // int margin = Math.Max(1, Math.Min(width, height) / 50);
                // leftEdge += margin; rightEdge -= margin; topEdge += margin; bottomEdge -= margin;

                float[] lineNumbersOfBrightestLines = new float[DEGREES180];
                float[] valuesOfBrightestLines = new float[DEGREES180];

                Parallel.For(0, DEGREES180, angleIndex =>
                {
                    // Rotate (bilinear) into rotatedImage
                    float[,] rotatedImage = new float[width, height];

                    float currentRadians = RADIANS_PER_DEGREE * angleIndex;
                    float sin = (float)Math.Sin(currentRadians);
                    float cos = (float)Math.Cos(currentRadians);

                    // Rotate around center
                    float cxf = (width - 1) * 0.5f;
                    float cyf = (height - 1) * 0.5f;

                    for (int y = topEdge; y < bottomEdge; y++)
                    {
                        float yy = y - cyf;
                        for (int x = leftEdge; x < rightEdge; x++)
                        {
                            float xx = x - cxf;

                            // Inverse rotation to sample source
                            float srcXf = (xx * cos + yy * sin) + cxf;
                            float srcYf = (-xx * sin + yy * cos) + cyf;

                            int srcX0 = (int)srcXf;
                            int srcY0 = (int)srcYf;

                            if (srcX0 >= 0 && srcX0 < width - 1 && srcY0 >= 0 && srcY0 < height - 1)
                            {
                                float fx = srcXf - srcX0;
                                float fy = srcYf - srcY0;
                                float oneMinusFx = 1f - fx;
                                float oneMinusFy = 1f - fy;

                                float v1 = starArray2D[srcX0, srcY0];
                                float v2 = starArray2D[srcX0 + 1, srcY0];
                                float v3 = starArray2D[srcX0, srcY0 + 1];
                                float v4 = starArray2D[srcX0 + 1, srcY0 + 1];

                                rotatedImage[x, y] =
                                    v1 * oneMinusFx * oneMinusFy +
                                    v2 * fx * oneMinusFy +
                                    v3 * oneMinusFx * fy +
                                    v4 * fx * fy;
                            }
                        }
                    }

                    // Build 1D profile: mean brightness per row
                    float[] rowTotals = new float[height];
                    int rowWidth = Math.Max(1, rightEdge - leftEdge);

                    for (int y = topEdge; y < bottomEdge; y++)
                    {
                        float sum = 0f;
                        for (int x = leftEdge; x < rightEdge; x++)
                            sum += rotatedImage[x, y];

                        rowTotals[y] = sum / rowWidth;
                    }

                    // Smooth the profile (Gaussian-ish 5 tap) to reduce noise
                    float[] smoothed = Smooth1D5(rowTotals, topEdge, bottomEdge);

                    // Compute local mean/std quickly using prefix sums, then z-score
                    float[] z = ComputeLocalZScore(smoothed, topEdge, bottomEdge, LOCAL_STATS_WINDOW, EPS_STD);

                    // Pick strongest local maximum by z-score (faint but distinct spikes win here)
                    float bestScore = -1f;
                    int bestY = -1;

                    for (int y = topEdge + 2; y < bottomEdge - 2; y++)
                    {
                        float s = z[y];
                        if (s <= 0f)
                            continue;

                        // Enforce a peak (helps ignore ramps/plateaus)
                        if (smoothed[y] >= smoothed[y - 1] && smoothed[y] >= smoothed[y + 1])
                        {
                            if (s > bestScore)
                            {
                                bestScore = s;
                                bestY = y;
                            }
                        }
                    }

                    // Fallback if nothing qualified (very low contrast)
                    if (bestY < 0)
                    {
                        float largest = -1f;
                        int ly = topEdge;
                        for (int y = topEdge; y < bottomEdge; y++)
                        {
                            if (smoothed[y] > largest)
                            {
                                largest = smoothed[y];
                                ly = y;
                            }
                        }
                        bestY = ly;
                        bestScore = largest;
                    }

                    lineNumbersOfBrightestLines[angleIndex] = bestY;
                    valuesOfBrightestLines[angleIndex] = bestScore;
                });

                // Select top LINES distinct angles (keep your original suppression around peaks)
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
            finally
            {
                starImage.UnlockBits(bitmapData);
            }
        }

        /// <summary>
        /// Smooths a 1D profile using a stable 5-tap kernel [1, 4, 6, 4, 1] / 16.
        ///
        /// The smoothing reduces noise while preserving peak locations reasonably well, which improves the
        /// reliability of subsequent peak detection (especially on faint, noisy profiles).
        ///
        /// Only rows that have 2 valid neighbors on each side are filtered; the edges are copied unchanged.
        /// </summary>
        private static float[] Smooth1D5(float[] src, int topEdge, int bottomEdge)
        {
            // 5-tap kernel: [1 4 6 4 1] / 16
            int n = src.Length;
            float[] dst = new float[n];

            // Copy edges
            for (int i = 0; i < n; i++)
                dst[i] = src[i];

            for (int y = topEdge + 2; y < bottomEdge - 2; y++)
            {
                dst[y] = (src[y - 2] + 4f * src[y - 1] + 6f * src[y] + 4f * src[y + 1] + src[y + 2]) / 16f;
            }

            return dst;
        }

        /// <summary>
        /// Computes a positive-only local z-score for each element of a 1D profile over a sliding window.
        ///
        /// For each y in [topEdge, bottomEdge):
        /// - Computes localMean and localStd over a window centered on y (clamped to the provided bounds),
        ///   using prefix sums for speed.
        /// - Computes z[y] = (profile[y] - localMean) / (localStd + epsStd).
        /// - Negative values are clamped to 0 so only upward deviations (peak-like features) are retained.
        ///
        /// This highlights narrow peaks that rise above the local background and de-emphasizes broad gradients
        /// and slowly-varying structure, which is useful for detecting faint diffraction spikes.
        /// </summary>
        private static float[] ComputeLocalZScore(float[] profile, int topEdge, int bottomEdge, int window, float epsStd)
        {
            // z[y] = (profile[y] - localMean[y]) / (localStd[y] + eps)
            // localMean/localStd over a sliding window using prefix sums for speed.

            if (window < 5) window = 5;
            if ((window & 1) == 0) window++; // force odd

            int n = profile.Length;
            float[] z = new float[n];

            double[] prefix = new double[n + 1];
            double[] prefix2 = new double[n + 1];

            for (int i = 0; i < n; i++)
            {
                double v = profile[i];
                prefix[i + 1] = prefix[i] + v;
                prefix2[i + 1] = prefix2[i] + v * v;
            }

            int half = window / 2;

            for (int y = topEdge; y < bottomEdge; y++)
            {
                int a = y - half;
                int b = y + half;

                if (a < topEdge) a = topEdge;
                if (b > bottomEdge - 1) b = bottomEdge - 1;

                int count = (b - a + 1);
                if (count <= 1)
                {
                    z[y] = 0f;
                    continue;
                }

                double sum = prefix[b + 1] - prefix[a];
                double sum2 = prefix2[b + 1] - prefix2[a];

                double mean = sum / count;
                double var = (sum2 / count) - (mean * mean);
                if (var < 0.0) var = 0.0;

                double std = Math.Sqrt(var);
                double denom = std + epsStd;

                double zz = (profile[y] - mean) / denom;

                // We only care about positive deviations (spikes)
                z[y] = (float)(zz > 0.0 ? zz : 0.0);
            }

            return z;
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

                int yPeak = (int)lineNumberOfLargestValue;
                float centerY = FindSymmetryCenter(rowTotals, yPeak, topEdge, bottomEdge);
                subpixelLineNumbers[index1] = centerY;

                brightestLineValues[index1] = largestValueForThisRotation;
            });

            for (int index = 0; index < lineCount; ++index)
            {
                bahtinovLines.LineIndex[index] = subpixelLineNumbers[index];
            }

            if (lineValidator.HasMinimumIntersectingLines(bahtinovLines) || bahtinovLines.LineValue.Length == 3)
            {
                return bahtinovLines;
            }
            else
            {
                ImageLostEventProvider.OnImageLost("Unable to detect intersection for the Bahtinov lines", "FindSubpixelLines", MessageBoxIcon.Warning, MessageBoxButtons.OK);
                return null;
            }
        }

        /// <summary>
        /// Determines the centre position of a Bahtinov diffraction line using symmetry
        /// analysis of a one-dimensional brightness profile.
        ///
        /// The method performs the following steps:
        /// 1. Estimates the background level from the profile edges.
        /// 2. Determines an adaptive analysis window around an initial peak estimate.
        /// 3. Evaluates left–right symmetry of the profile within this window.
        /// 4. Selects the position that minimises asymmetry and refines it to subpixel
        ///    accuracy.
        ///
        /// Returns the subpixel-accurate centre position of the diffraction line.
        /// 
        /// <param name="profile">One-dimensional brightness profile derived from the rotated image.</param>
        /// <param name="yPeak">Initial estimate of the line position, typically the brightest row.</param>
        /// <param name="yMin">Inclusive lower bound of valid profile indices.</param>
        /// <param name="yMax">Exclusive upper bound of valid profile indices.</param>
        /// <returns>The subpixel-accurate centre position of the diffraction line, or
        /// <paramref name="yPeak"/> if a reliable result cannot be determined.</returns>
        /// </summary>
        private static float FindSymmetryCenter(
            float[] profile,
            int yPeak,
            int yMin,
            int yMax)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (yPeak < yMin || yPeak >= yMax) return yPeak;

            // 1) Background estimate: use a robust-ish edge mean (keeps it fast)
            // We pick the smaller edge mean to avoid bias if one edge contains signal.
            const int edgeN = 8;

            float Mean(int a, int b)
            {
                a = Math.Max(a, yMin);
                b = Math.Min(b, yMax);
                int n = b - a;
                if (n <= 0) return 0f;
                double s = 0;
                for (int i = a; i < b; i++) s += profile[i];
                return (float)(s / n);
            }

            float bgLeft = Mean(yMin, yMin + edgeN);
            float bgRight = Mean(yMax - edgeN, yMax);
            float bg = Math.Min(bgLeft, bgRight);

            float peak = profile[yPeak];
            float amp = peak - bg;
            if (amp <= 1e-6f) return yPeak;

            // 2) Choose a window size automatically from the profile (no fixed line width)
            // Use a low fraction of peak above bg to capture the band wings.
            float level = bg + 0.20f * amp;

            int left = yPeak;
            while (left > yMin && profile[left] > level) left--;
            int right = yPeak;
            while (right < yMax - 1 && profile[right] > level) right++;

            // Width estimate for matching. Clamp to keep it stable.
            int halfW = Math.Max(4, (right - left) / 2);
            halfW = Math.Min(halfW, 35);

            // Search range around the peak: allow small shifts.
            int searchR = Math.Max(3, halfW / 2);
            searchR = Math.Min(searchR, 20);

            double BestScoreForCenter(int c)
            {
                // Compare mirrored samples. Use background-subtracted weights so the band matters.
                int maxI = Math.Min(halfW, Math.Min(c - yMin, (yMax - 1) - c));
                if (maxI < 2) return double.PositiveInfinity;

                double score = 0;
                double wsum = 0;

                for (int i = 1; i <= maxI; i++)
                {
                    float l = profile[c - i] - bg;
                    float r = profile[c + i] - bg;
                    if (l < 0f) l = 0f;
                    if (r < 0f) r = 0f;

                    // Weight by signal so noisy wings do not dominate
                    double w = l + r;
                    if (w <= 1e-9) continue;

                    double d = l - r;
                    score += w * d * d;
                    wsum += w;
                }

                if (wsum <= 1e-9) return double.PositiveInfinity;
                return score / wsum;
            }

            int c0 = yPeak;
            int cMin = Math.Max(yMin + halfW, c0 - searchR);
            int cMax = Math.Min(yMax - 1 - halfW, c0 + searchR);

            double best = double.PositiveInfinity;
            int bestC = c0;

            for (int c = cMin; c <= cMax; c++)
            {
                double s = BestScoreForCenter(c);
                if (s < best)
                {
                    best = s;
                    bestC = c;
                }
            }

            // 3) Subpixel refinement by parabolic fit on score(bestC-1..bestC+1)
            if (bestC > cMin && bestC < cMax)
            {
                double s1 = BestScoreForCenter(bestC - 1);
                double s2 = BestScoreForCenter(bestC);
                double s3 = BestScoreForCenter(bestC + 1);

                double denom = (s1 - 2.0 * s2 + s3);
                if (Math.Abs(denom) > 1e-12)
                {
                    // Vertex of parabola through (-1,s1), (0,s2), (+1,s3)
                    double delta = 0.5 * (s1 - s3) / denom;
                    delta = Math.Max(-0.5, Math.Min(0.5, delta));
                    return (float)(bestC + delta);
                }
            }

            return bestC;
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

                // Compute perpendicular direction of the error vector (radians)
                double errorDir = Math.Atan2(dyErr, dxErr);

                // Normalize to [0, 2π)
                if (errorDir < 0.0)
                    errorDir += Math.PI * 2.0;

                // Invert direction by rotating π radians
                errorDir += Math.PI;
                if (errorDir >= Math.PI * 2.0)
                    errorDir -= Math.PI * 2.0;

                // Determine sign based on radial window per group
                float errorSign;

                switch (group)
                {
                    case 0:
                        if (lines.LineAngles.Length == 9)
                            // 0 < angle < π
                            errorSign = (errorDir > 0.0 && errorDir < Math.PI) ? 1f : -1f;
                        else
                            // Keep sign if this is a simple bahtinov mask
                            errorSign = -1f;
                        break;

                    case 1:
                        // π/3 < angle < 4π/3  (60°–240°) => sign is NEGATIVE there
                        errorSign = (errorDir > Math.PI / 3.0 && errorDir < 4.0 * Math.PI / 3.0) ? -1f : 1f;
                        break;

                    case 2:
                        // 2π/3 < angle < 5π/3  (120°–300°)
                        errorSign = (errorDir > 2.0 * Math.PI / 3.0 && errorDir < 5.0 * Math.PI / 3.0) ? 1f : -1f;
                        break;

                    default:
                        errorSign = -1f;
                        break;
                }

                errorSign *= (Properties.Settings.Default.SignChange ? -1f : 1f);

                double errorDistanceD = Math.Sqrt(dxErr * dxErr + dyErr * dyErr);
                double bahtinovOffset = errorSign * Math.Floor(errorDistanceD * 10.0) / 10.0;
                float bahtinovAngle = Math.Abs((bahtinovLines.LineAngles[2] - bahtinovLines.LineAngles[0]) / 2.0f);

                FocusData fd = new FocusData
                {
                    BahtinovOffset = bahtinovOffset,
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
                    new Point(circle_x, circle_y), circle_width, circle_height, errorValue);

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