
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
        public delegate void FocusDataEventHandler(object sender, FocusDataEventArgs e);
        public static event FocusDataEventHandler FocusDataEvent;

        public delegate void BahtinovLineDrawEventHandler(object sender, BahtinovLineDataEventArgs e);
        public static event BahtinovLineDrawEventHandler BahtinovLineDrawEvent;
        #endregion

        #region Private Fields
        private const float ErrorMarkerScalingValue = 20.0f;
        private float lastFocusErrorValue = 0.0f;


        #endregion

        #region Nested Types

        /// <summary>
        /// Per-worker-thread scratch buffers reused across <see cref="Parallel.For"/> iterations in
        /// <see cref="FindBrightestLines"/>, <see cref="FindFaintLines"/>, and <see cref="FindSubpixelLines"/>.
        /// Allocating once per thread (instead of once per angle/line) avoids ~180 large
        /// <c>float[width,height]</c> allocations per call and the associated GC pressure.
        /// </summary>
        private sealed class AngleScanWorkspace
        {
            public float[,] RotatedImage { get; }
            public float[] RowTotals { get; }
            public float[] SmoothedTotals { get; }
            public float[] ZScore { get; }
            public double[] Prefix { get; }
            public double[] Prefix2 { get; }

            public AngleScanWorkspace(int width, int height)
            {
                RotatedImage = new float[width, height];
                RowTotals = new float[height];
                SmoothedTotals = new float[height];
                ZScore = new float[height];
                Prefix = new double[height + 1];
                Prefix2 = new double[height + 1];
            }
        }

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
            const float DIVISION_FACTOR = 3f / byte.MaxValue;

            // Two-pass angle search:
            //   Pass 1 sweeps the full [0, 180) range at a coarse step (2°) to
            //          quickly locate candidate diffraction-line angles.
            //   Pass 2 refines each candidate with a fine step (0.1°) over a
            //          narrow window centred on the coarse pick. Total angle
            //          evaluations: COARSE_STEPS + LINES * FINE_STEPS_PER_CANDIDATE
            //          which is far fewer than a flat 0.1° sweep would require.
            const float COARSE_STEP_DEGREES = 2.0f;
            const int   COARSE_STEPS = (int)(180f / COARSE_STEP_DEGREES); // 90
            const float COARSE_RADIANS_PER_STEP = (float)Math.PI / COARSE_STEPS;
            const float COARSE_SUPPRESSION_DEGREES = 5.0f;
            int   COARSE_SUPPRESSION_STEPS =
                (int)Math.Ceiling(COARSE_SUPPRESSION_DEGREES / COARSE_STEP_DEGREES); // 3

            const float FINE_STEP_DEGREES = 0.1f;
            const float FINE_HALF_WINDOW_DEGREES = COARSE_STEP_DEGREES / 2f; // ±1°
            const int   FINE_STEPS_PER_CANDIDATE =
                (int)(2f * FINE_HALF_WINDOW_DEGREES / FINE_STEP_DEGREES) + 1; // 21
            const float FINE_RADIANS_PER_STEP = (float)(Math.PI * FINE_STEP_DEGREES / 180.0);
            const int   FINE_HALF_INDEX = FINE_STEPS_PER_CANDIDATE / 2;

            Rectangle rect = new Rectangle(0, 0, starImage.Width, starImage.Height);
            BahtinovData result = new BahtinovData(LINES, rect);
            BitmapData bitmapData = starImage.LockBits(rect, ImageLockMode.ReadWrite, starImage.PixelFormat);

            int stride = bitmapData.Stride;
            int bytesPerPixel = System.Drawing.Image.GetPixelFormatSize(starImage.PixelFormat) / 8;
            int width = starImage.Width;
            int height = starImage.Height;
            byte[] starImageArray;
            try
            {
                IntPtr scan0 = bitmapData.Scan0;
                starImageArray = new byte[height * stride];
                Marshal.Copy(scan0, starImageArray, 0, starImageArray.Length);
            }
            finally
            {
                starImage.UnlockBits(bitmapData);
            }

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

            // -----------------------------------------------------------------
            // Pass 1: coarse 2° sweep over the full [0, 180) range.
            // -----------------------------------------------------------------
            float[] coarseLineNumbers = new float[COARSE_STEPS];
            float[] coarseValues = new float[COARSE_STEPS];

            Parallel.For(
                0,
                COARSE_STEPS,
                () => new AngleScanWorkspace(width, height),
                (idx, state, workspace) =>
                {
                    float angleRadians = idx * COARSE_RADIANS_PER_STEP;
                    ScanAngleBright(
                        workspace, starArray2D,
                        leftEdge, rightEdge, topEdge, bottomEdge,
                        starImage_X_Centre, starImage_Y_Centre,
                        angleRadians,
                        out float lineNumber, out float largestValue);

                    coarseLineNumbers[idx] = lineNumber;
                    coarseValues[idx] = largestValue;
                    return workspace;
                },
                _ => { });

            // Pick the top LINES coarse candidates with ±5° non-maximum suppression.
            float[] candidateAngles = new float[LINES];
            {
                float[] coarseScratch = (float[])coarseValues.Clone();
                for (int i = 0; i < LINES; i++)
                {
                    float maxValue = -1f;
                    int bestIdx = -1;
                    for (int j = 0; j < COARSE_STEPS; j++)
                    {
                        if (coarseScratch[j] > maxValue)
                        {
                            maxValue = coarseScratch[j];
                            bestIdx = j;
                        }
                    }

                    if (bestIdx < 0)
                    {
                        candidateAngles[i] = 0f;
                        continue;
                    }

                    candidateAngles[i] = bestIdx * COARSE_RADIANS_PER_STEP;

                    for (int j = bestIdx - COARSE_SUPPRESSION_STEPS; j <= bestIdx + COARSE_SUPPRESSION_STEPS; j++)
                    {
                        int wrapped = (j + COARSE_STEPS) % COARSE_STEPS;
                        coarseScratch[wrapped] = 0f;
                    }
                }
            }

            // -----------------------------------------------------------------
            // Pass 2: refine each candidate at 0.1° over a ±1° window.
            // All LINES * FINE_STEPS_PER_CANDIDATE evaluations are run in a
            // single Parallel.For so they share workspace pools efficiently.
            // -----------------------------------------------------------------
            int totalFineEvals = LINES * FINE_STEPS_PER_CANDIDATE;
            float[] fineLineNumbers = new float[totalFineEvals];
            float[] fineValues = new float[totalFineEvals];

            Parallel.For(
                0,
                totalFineEvals,
                () => new AngleScanWorkspace(width, height),
                (flatIdx, state, workspace) =>
                {
                    int candidateIdx = flatIdx / FINE_STEPS_PER_CANDIDATE;
                    int stepIdx = flatIdx % FINE_STEPS_PER_CANDIDATE;

                    int delta = stepIdx - FINE_HALF_INDEX;
                    float angleRadians = candidateAngles[candidateIdx] + delta * FINE_RADIANS_PER_STEP;

                    ScanAngleBright(
                        workspace, starArray2D,
                        leftEdge, rightEdge, topEdge, bottomEdge,
                        starImage_X_Centre, starImage_Y_Centre,
                        angleRadians,
                        out float lineNumber, out float largestValue);

                    fineLineNumbers[flatIdx] = lineNumber;
                    fineValues[flatIdx] = largestValue;
                    return workspace;
                },
                _ => { });

            // Pick the best fine sample per candidate and write to result.
            for (int c = 0; c < LINES; c++)
            {
                int baseIdx = c * FINE_STEPS_PER_CANDIDATE;
                float maxValue = -1f;
                int bestStep = FINE_HALF_INDEX;
                for (int s = 0; s < FINE_STEPS_PER_CANDIDATE; s++)
                {
                    float v = fineValues[baseIdx + s];
                    if (v > maxValue)
                    {
                        maxValue = v;
                        bestStep = s;
                    }
                }

                int delta = bestStep - FINE_HALF_INDEX;
                float refinedAngle = candidateAngles[c] + delta * FINE_RADIANS_PER_STEP;

                result.LineIndex[c] = fineLineNumbers[baseIdx + bestStep];
                result.LineAngles[c] = refinedAngle;
                result.LineValue[c] = fineValues[baseIdx + bestStep];
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

            // Two-pass angle search; see FindBrightestLines for rationale.
            const float COARSE_STEP_DEGREES = 2.0f;
            const int   COARSE_STEPS = (int)(180f / COARSE_STEP_DEGREES); // 90
            const float COARSE_RADIANS_PER_STEP = (float)Math.PI / COARSE_STEPS;
            const float COARSE_SUPPRESSION_DEGREES = 5.0f;
            int   COARSE_SUPPRESSION_STEPS =
                (int)Math.Ceiling(COARSE_SUPPRESSION_DEGREES / COARSE_STEP_DEGREES); // 3

            const float FINE_STEP_DEGREES = 0.1f;
            const float FINE_HALF_WINDOW_DEGREES = COARSE_STEP_DEGREES / 2f; // ±1°
            const int   FINE_STEPS_PER_CANDIDATE =
                (int)(2f * FINE_HALF_WINDOW_DEGREES / FINE_STEP_DEGREES) + 1; // 21
            const float FINE_RADIANS_PER_STEP = (float)(Math.PI * FINE_STEP_DEGREES / 180.0);
            const int   FINE_HALF_INDEX = FINE_STEPS_PER_CANDIDATE / 2;

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

                float cxf = (width + 1) * 0.5f;
                float cyf = (height + 1) * 0.5f;
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

                            float lum = (0.299f * r + 0.587f * g + 0.114f * b) / 255f;
                            float v = (float)Math.Sqrt(Math.Max(0.0f, lum));

                            float dx = x - cxf;
                            float dy = y - cyf;
                            float rr = (float)Math.Sqrt(dx * dx + dy * dy);

                            float w = 1.0f;
                            if (rr <= coreRadius)
                            {
                                w = 0.0f;
                            }
                            else if (rr < coreRadius + coreFeather)
                            {
                                w = (rr - coreRadius) / coreFeather;
                            }

                            starArray2D[x, y] = v * w;
                        }
                    }
                }

                int leftEdge = 0;
                int rightEdge = width;
                int topEdge = 0;
                int bottomEdge = height;

                // -----------------------------------------------------------------
                // Pass 1: coarse 2° sweep over the full [0, 180) range.
                // -----------------------------------------------------------------
                float[] coarseLineNumbers = new float[COARSE_STEPS];
                float[] coarseValues = new float[COARSE_STEPS];

                Parallel.For(
                    0,
                    COARSE_STEPS,
                    () => new AngleScanWorkspace(width, height),
                    (idx, state, workspace) =>
                    {
                        float angleRadians = idx * COARSE_RADIANS_PER_STEP;
                        ScanAngleFaint(
                            workspace, starArray2D,
                            width, height,
                            leftEdge, rightEdge, topEdge, bottomEdge,
                            cxf, cyf,
                            angleRadians,
                            LOCAL_STATS_WINDOW, EPS_STD,
                            out int bestY, out float bestScore);

                        coarseLineNumbers[idx] = bestY;
                        coarseValues[idx] = bestScore;
                        return workspace;
                    },
                    _ => { });

                // Pick the top LINES coarse candidates with ±5° non-maximum suppression.
                float[] candidateAngles = new float[LINES];
                {
                    float[] coarseScratch = (float[])coarseValues.Clone();
                    for (int i = 0; i < LINES; i++)
                    {
                        float maxValue = -1f;
                        int bestIdx = -1;
                        for (int j = 0; j < COARSE_STEPS; j++)
                        {
                            if (coarseScratch[j] > maxValue)
                            {
                                maxValue = coarseScratch[j];
                                bestIdx = j;
                            }
                        }

                        if (bestIdx < 0)
                        {
                            candidateAngles[i] = 0f;
                            continue;
                        }

                        candidateAngles[i] = bestIdx * COARSE_RADIANS_PER_STEP;

                        for (int j = bestIdx - COARSE_SUPPRESSION_STEPS; j <= bestIdx + COARSE_SUPPRESSION_STEPS; j++)
                        {
                            int wrapped = (j + COARSE_STEPS) % COARSE_STEPS;
                            coarseScratch[wrapped] = 0f;
                        }
                    }
                }

                // -----------------------------------------------------------------
                // Pass 2: refine each candidate at 0.1° over a ±1° window.
                // -----------------------------------------------------------------
                int totalFineEvals = LINES * FINE_STEPS_PER_CANDIDATE;
                float[] fineLineNumbers = new float[totalFineEvals];
                float[] fineValues = new float[totalFineEvals];

                Parallel.For(
                    0,
                    totalFineEvals,
                    () => new AngleScanWorkspace(width, height),
                    (flatIdx, state, workspace) =>
                    {
                        int candidateIdx = flatIdx / FINE_STEPS_PER_CANDIDATE;
                        int stepIdx = flatIdx % FINE_STEPS_PER_CANDIDATE;

                        int delta = stepIdx - FINE_HALF_INDEX;
                        float angleRadians = candidateAngles[candidateIdx] + delta * FINE_RADIANS_PER_STEP;

                        ScanAngleFaint(
                            workspace, starArray2D,
                            width, height,
                            leftEdge, rightEdge, topEdge, bottomEdge,
                            cxf, cyf,
                            angleRadians,
                            LOCAL_STATS_WINDOW, EPS_STD,
                            out int bestY, out float bestScore);

                        fineLineNumbers[flatIdx] = bestY;
                        fineValues[flatIdx] = bestScore;
                        return workspace;
                    },
                    _ => { });

                for (int c = 0; c < LINES; c++)
                {
                    int baseIdx = c * FINE_STEPS_PER_CANDIDATE;
                    float maxValue = -1f;
                    int bestStep = FINE_HALF_INDEX;
                    for (int s = 0; s < FINE_STEPS_PER_CANDIDATE; s++)
                    {
                        float v = fineValues[baseIdx + s];
                        if (v > maxValue)
                        {
                            maxValue = v;
                            bestStep = s;
                        }
                    }

                    int delta = bestStep - FINE_HALF_INDEX;
                    float refinedAngle = candidateAngles[c] + delta * FINE_RADIANS_PER_STEP;

                    result.LineIndex[c] = fineLineNumbers[baseIdx + bestStep];
                    result.LineAngles[c] = refinedAngle;
                    result.LineValue[c] = fineValues[baseIdx + bestStep];
                }

                return result;
            }
            finally
            {
                starImage.UnlockBits(bitmapData);
            }
        }

        /// <summary>
        /// Bilinearly rotates the ROI of <paramref name="starArray2D"/> by
        /// <paramref name="angleRadians"/>, sums per row, applies 3-tap row smoothing,
        /// and returns the brightest row index and its smoothed value.
        ///
        /// Used by both passes of <see cref="FindBrightestLines"/> so the same kernel
        /// is shared between the coarse and fine angle sweeps.
        /// </summary>
        private static void ScanAngleBright(
            AngleScanWorkspace workspace,
            float[,] starArray2D,
            int leftEdge, int rightEdge, int topEdge, int bottomEdge,
            float starImage_X_Centre, float starImage_Y_Centre,
            float angleRadians,
            out float lineNumber, out float largestValue)
        {
            float[,] rotatedImage = workspace.RotatedImage;
            float[] rowTotals = workspace.RowTotals;
            float[] smoothedTotals = workspace.SmoothedTotals;

            // Pixels outside the rotated source map are left at zero (Array.Clear).
            Array.Clear(rotatedImage, 0, rotatedImage.Length);

            float sin_CurrentRadians = (float)Math.Sin(angleRadians);
            float cos_CurrentRadians = (float)Math.Cos(angleRadians);

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

            int rowWidth = Math.Max(1, rightEdge - leftEdge);
            for (int y = topEdge; y < bottomEdge; ++y)
            {
                float rowTotal = 0;
                for (int x = leftEdge; x < rightEdge; ++x)
                {
                    rowTotal += rotatedImage[x, y];
                }
                rowTotals[y] = rowTotal / rowWidth;
            }

            for (int y = topEdge; y < bottomEdge; ++y)
            {
                smoothedTotals[y] = rowTotals[y];
                if (y > topEdge && y < bottomEdge - 1)
                {
                    smoothedTotals[y] = (rowTotals[y - 1] + rowTotals[y] + rowTotals[y + 1]) / 3f;
                }
            }

            largestValue = -1f;
            lineNumber = -1f;
            for (int y = topEdge; y < bottomEdge; ++y)
            {
                if (smoothedTotals[y] > largestValue)
                {
                    largestValue = smoothedTotals[y];
                    lineNumber = y;
                }
            }
        }

        /// <summary>
        /// Bilinearly rotates the full image stored in <paramref name="starArray2D"/> by
        /// <paramref name="angleRadians"/>, computes a 1D row-mean profile, smooths it
        /// with a 5-tap kernel, applies a local z-score detector, and returns the
        /// strongest local-maximum row index and its z-score.
        ///
        /// Used by both passes of <see cref="FindFaintLines"/> so the same kernel is
        /// shared between the coarse and fine angle sweeps.
        /// </summary>
        private static void ScanAngleFaint(
            AngleScanWorkspace workspace,
            float[,] starArray2D,
            int width, int height,
            int leftEdge, int rightEdge, int topEdge, int bottomEdge,
            float cxf, float cyf,
            float angleRadians,
            int statsWindow, float epsStd,
            out int bestY, out float bestScore)
        {
            float[,] rotatedImage = workspace.RotatedImage;
            float[] rowTotals = workspace.RowTotals;
            float[] smoothed = workspace.SmoothedTotals;
            float[] z = workspace.ZScore;
            double[] prefix = workspace.Prefix;
            double[] prefix2 = workspace.Prefix2;

            Array.Clear(rotatedImage, 0, rotatedImage.Length);

            float sin = (float)Math.Sin(angleRadians);
            float cos = (float)Math.Cos(angleRadians);

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

            int rowWidth = Math.Max(1, rightEdge - leftEdge);
            for (int y = topEdge; y < bottomEdge; y++)
            {
                float sum = 0f;
                for (int x = leftEdge; x < rightEdge; x++)
                    sum += rotatedImage[x, y];

                rowTotals[y] = sum / rowWidth;
            }

            Smooth1D5(rowTotals, smoothed, topEdge, bottomEdge);
            ComputeLocalZScore(smoothed, z, prefix, prefix2, topEdge, bottomEdge, statsWindow, epsStd);

            bestScore = -1f;
            bestY = -1;

            for (int y = topEdge + 2; y < bottomEdge - 2; y++)
            {
                float s = z[y];
                if (s <= 0f)
                    continue;

                if (smoothed[y] >= smoothed[y - 1] && smoothed[y] >= smoothed[y + 1])
                {
                    if (s > bestScore)
                    {
                        bestScore = s;
                        bestY = y;
                    }
                }
            }

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
        }

        /// <summary>
        /// Smooths a 1D profile using a stable 5-tap kernel [1, 4, 6, 4, 1] / 16, writing
        /// the result into a caller-supplied <paramref name="dst"/> buffer.
        ///
        /// The smoothing reduces noise while preserving peak locations reasonably well, which improves the
        /// reliability of subsequent peak detection (especially on faint, noisy profiles).
        ///
        /// Only rows that have 2 valid neighbors on each side are filtered; the edges are copied unchanged.
        /// </summary>
        private static void Smooth1D5(float[] src, float[] dst, int topEdge, int bottomEdge)
        {
            int n = src.Length;
            for (int i = 0; i < n; i++)
                dst[i] = src[i];

            for (int y = topEdge + 2; y < bottomEdge - 2; y++)
            {
                dst[y] = (src[y - 2] + 4f * src[y - 1] + 6f * src[y] + 4f * src[y + 1] + src[y + 2]) / 16f;
            }
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
        private static void ComputeLocalZScore(
            float[] profile,
            float[] z,
            double[] prefix,
            double[] prefix2,
            int topEdge,
            int bottomEdge,
            int window,
            float epsStd)
        {
            // z[y] = (profile[y] - localMean[y]) / (localStd[y] + eps)
            // localMean/localStd over a sliding window using prefix sums for speed.
            // The prefix and prefix2 buffers are caller-supplied (pre-allocated and
            // reused across angles in the parallel scan) to avoid per-angle GC churn.

            if (window < 5) window = 5;
            if ((window & 1) == 0) window++; // force odd

            int n = profile.Length;

            // prefix[0] / prefix2[0] are conventionally 0; we never write to index 0 below
            // so reused buffers retain that 0 from initial allocation.
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
        }

        /// <summary>
        /// Refines line positions using the same green-channel sqrt model as <see cref="FindBrightestLines"/>,
        /// 3-tap row smoothing, parabolic subpixel peak fit on the 1D profile, with symmetry-based fallback.
        /// </summary>
        /// <param name="starImage">The bitmap image of the star to be analyzed.</param>
        /// <param name="lineCount">The number of lines to detect and refine.</param>
        /// <param name="bahtinovLines">The <see cref="BahtinovData"/> object containing initial line data to be refined.</param>
        /// <returns>The updated <see cref="BahtinovData"/> object with refined line indices and values.</returns>
        public BahtinovData FindSubpixelLines(Bitmap starImage, int lineCount, BahtinovData bahtinovLines)
        {
            // Define the full rectangle of the image.
            Rectangle rect = new Rectangle(0, 0, starImage.Width, starImage.Height);

            // Lock the bitmap data.
            BitmapData bitmapData = starImage.LockBits(rect, ImageLockMode.ReadWrite, starImage.PixelFormat);
            int stride = bitmapData.Stride;
            int bytesPerPixel = Image.GetPixelFormatSize(starImage.PixelFormat) / 8;
            int imagePixelCount = stride * starImage.Height;
            byte[] starImageArray;
            try
            {
                IntPtr scan0 = bitmapData.Scan0;
                starImageArray = new byte[imagePixelCount];
                // Copy pixel data to byte array; caller reads from the byte buffer after UnlockBits.
                Marshal.Copy(scan0, starImageArray, 0, imagePixelCount);
            }
            finally
            {
                starImage.UnlockBits(bitmapData);
            }

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

            // Green channel + sqrt, matching FindBrightestLines for consistent geometry vs angle pass.
            for (int x = leftEdge; x < rightEdge; ++x)
            {
                for (int y = topEdge; y < bottomEdge; ++y)
                {
                    int pixelIndex = y * stride + x * bytesPerPixel;
                    if (pixelIndex < 0 || pixelIndex + 1 >= starImageArray.Length)
                        continue;

                    byte greenValue = starImageArray[pixelIndex + 1];
                    starArray2D[x, y] = (float)Math.Sqrt(greenValue * divisionFactor);
                }
            }

            float[] subpixelLineNumbers = new float[lineCount];
            float[] brightestLineValues = new float[lineCount];

            // Per-thread workspace: avoid allocating lineCount large float[,] buffers per call.
            Parallel.For(
                0,
                lineCount,
                () => new AngleScanWorkspace(width, height),
                (index1, state, workspace) =>
                {
                    float[,] imageArray = workspace.RotatedImage;
                    float[] rowTotals = workspace.RowTotals;
                    float[] smoothedTotals = workspace.SmoothedTotals;

                    // Clear stale values from the previous line processed by this thread.
                    Array.Clear(imageArray, 0, imageArray.Length);

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

                    for (int y = topEdge; y < bottomEdge; ++y)
                    {
                        float rowTotal = 0f;
                        for (int x = leftEdge; x < rightEdge; ++x)
                        {
                            rowTotal += imageArray[x, y];
                        }
                        rowTotals[y] = rowTotal / (rightEdge - leftEdge);
                    }

                    // Same 3-tap smoothing as FindBrightestLines before peak search
                    for (int y = topEdge; y < bottomEdge; ++y)
                    {
                        smoothedTotals[y] = rowTotals[y];
                        if (y > topEdge && y < bottomEdge - 1)
                            smoothedTotals[y] = (rowTotals[y - 1] + rowTotals[y] + rowTotals[y + 1]) / 3f;
                    }

                    float largestValueForThisRotation = -1f;
                    int yPeak = topEdge;

                    for (int y = topEdge; y < bottomEdge; ++y)
                    {
                        if (smoothedTotals[y] > largestValueForThisRotation)
                        {
                            largestValueForThisRotation = smoothedTotals[y];
                            yPeak = y;
                        }
                    }

                    float centerY = TryRefinePeakSubpixelParabola(smoothedTotals, yPeak, topEdge, bottomEdge, out float parabolicY)
                        ? parabolicY
                        : FindSymmetryCenter(smoothedTotals, yPeak, topEdge, bottomEdge);
                    subpixelLineNumbers[index1] = centerY;

                    brightestLineValues[index1] = largestValueForThisRotation;

                    return workspace;
                },
                _ => { });

            for (int index = 0; index < lineCount; ++index)
            {
                bahtinovLines.LineIndex[index] = subpixelLineNumbers[index];
            }
            return bahtinovLines;
        }

        /// <summary>
        /// Subpixel peak via parabolic fit to three samples around the integer maximum.
        /// Returns false if the fit is unreliable (edge index, flat peak, or wrong curvature).
        /// </summary>
        private static bool TryRefinePeakSubpixelParabola(
            float[] profile,
            int peak,
            int yMin,
            int yMax,
            out float centerY)
        {
            centerY = peak;
            if (peak <= yMin || peak >= yMax - 1)
                return false;

            double a = profile[peak - 1];
            double b = profile[peak];
            double c = profile[peak + 1];
            double denom = a - 2.0 * b + c;
            if (Math.Abs(denom) < 1e-12 || denom > 0.0)
                return false;

            double delta = 0.5 * (a - c) / denom;
            if (double.IsNaN(delta) || double.IsInfinity(delta))
                return false;

            delta = Math.Max(-0.5, Math.Min(0.5, delta));
            centerY = (float)(peak + delta);
            return true;
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
                ImageLostEventProvider.OnImageLost(UiText.Current.BahtinovLinesNotDetectedMessage, UiText.Current.BahtinovLinesNotDetectedTitle, MessageBoxIcon.Warning, MessageBoxButtons.OK);
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
            // Empirical +1px Y nudge so overlays align with the captured image (legacy height−Y screen mapping).
            const float BahtinovOverlayScreenYOffset = 1f;

            // Choose a small epsilon relative to image size for robust parallel checks
            double eps = Math.Max(width, height) * 1e-12; // very strict, yet safe for 500x500

            for (int group = 0; group < numberOfGroups; group++)
            {
                BahtinovLineDataEventArgs.LineGroup lineGroup = new BahtinovLineDataEventArgs.LineGroup(group);

                int startIndex = 3 * group;
                Array.Copy(lines.LineAngles, startIndex, bahtinovLines.LineAngles, 0, 3);
                Array.Copy(lines.LineIndex, startIndex, bahtinovLines.LineIndex, 0, 3);

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
                        new Point((int)Math.Round(lineStart_X), (int)Math.Round(height - lineStart_Y + yOffset + BahtinovOverlayScreenYOffset)),
                        new Point((int)Math.Round(lineEnd_X), (int)Math.Round(height - lineEnd_Y + yOffset + BahtinovOverlayScreenYOffset)),
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
                            break;
                        case 2:
                            thirdLine = L;
                            break;
                    }
                }

                if (firstLine == null || secondLine == null || thirdLine == null)
                {
                    ImageLostEventProvider.OnImageLost(UiText.Current.BahtinovImageLostMessage, UiText.Current.BahtinovImageLostTitle, MessageBoxIcon.Error, MessageBoxButtons.OK);
                    lastFocusErrorValue = 0.0f;
                    return false;
                }

                // Intersection of first and third lines (no slopes used)
                var inter = Line2D.Intersect(firstLine.Value, thirdLine.Value, eps);
                if (!inter.HasValue)
                {
                    // If these are nearly parallel, you could fall back to midpoints or skip
                    ImageLostEventProvider.OnImageLost(UiText.Current.BahtinovIntersectionComputeFailedMessage, UiText.Current.BahtinovIntersectionComputeFailedTitle, MessageBoxIcon.Warning, MessageBoxButtons.OK);
                    lastFocusErrorValue = 0.0f;
                    return false;
                }
                double xIntersectionD = inter.Value.x;
                double yIntersectionD = inter.Value.y;

                // Project the intersection onto the second line to get the perpendicular foot
                var (perpXD, perpYD) = secondLine.Value.ProjectPoint(xIntersectionD, yIntersectionD);

                double dxErr = xIntersectionD - perpXD;
                double dyErr = yIntersectionD - perpYD;

                // Robust sign: which side of the middle line is the intersection on?
                // The line is stored in normalized normal form (a*x + b*y + c = 0 with
                // sqrt(a^2 + b^2) = 1), so this is a true signed perpendicular distance
                // and it is continuous at every orientation, including exactly 90°
                // (where Atan2-based sector tests collapse onto a boundary and stop
                // flipping with focus direction).
                double signedDist =
                    secondLine.Value.A * xIntersectionD +
                    secondLine.Value.B * yIntersectionD +
                    secondLine.Value.C;

                // Per-group polarity preserves the original sign convention from the
                // previous Atan2 sector test for non-degenerate orientations:
                // groups 0/2 reported "+" on the positive-normal side, group 1 was
                // inverted (sign was negative inside its sector).
                float polarity;
                switch (group)
                {
                    case 0: polarity = +1f; break;
                    case 1: polarity = -1f; break;
                    case 2: polarity = +1f; break;
                    default: polarity = -1f; break;
                }

                float errorSign = (signedDist >= 0.0 ? +1f : -1f) * polarity;

                errorSign *= (Properties.Settings.Default.SignChange ? -1f : 1f);

                double errorDistanceD = Math.Sqrt(dxErr * dxErr + dyErr * dyErr);
                double bahtinovOffset = errorSign * Math.Round(errorDistanceD, 1, MidpointRounding.AwayFromZero);
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
                int circle_y = (int)(height - errorMarker_Y - circleRadius + yOffset + BahtinovOverlayScreenYOffset);
                int circle_width = circleRadius * 2;
                int circle_height = circleRadius * 2;
                string errorValue = bahtinovOffset.ToString("F1");

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
                rotatedY1 += height - errorMarker_Y + BahtinovOverlayScreenYOffset;
                rotatedX2 += errorMarker_X;
                rotatedY2 += height - errorMarker_Y + BahtinovOverlayScreenYOffset;

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
