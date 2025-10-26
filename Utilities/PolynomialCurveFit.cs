using System;

namespace Bahtinov_Collimator
{
    public static class PolynomialCurveFit
    {
        /// <summary>
        /// Finds the x-coordinate of the maximum value near an estimated position by
        /// fitting a quadratic y = a x^2 + b x + c to samples in a window and returning
        /// the vertex x = -b / (2a). Falls back to the discrete max index if needed.
        /// </summary>
        /// <param name="inputArray">Array of float values representing the data to analyze.</param>
        /// <param name="estimatedPosition">Estimated index of the peak.</param>
        /// <param name="range">Half window size on each side of the estimate.</param>
        /// <returns>Subpixel x-coordinate of the peak. Returns a discrete index if fitting is not possible.</returns>
        public static double FindMaxValueIndex(float[] inputArray, int estimatedPosition, int range)
        {
            if (inputArray == null || inputArray.Length == 0) return 0.0;
            if (estimatedPosition < 0) estimatedPosition = 0;
            if (estimatedPosition >= inputArray.Length) estimatedPosition = inputArray.Length - 1;
            if (range < 1) range = 1;

            // Clamp the fitting window to valid bounds
            int left = Math.Max(0, estimatedPosition - range);
            int right = Math.Min(inputArray.Length - 1, estimatedPosition + range);
            int n = right - left + 1;

            // Need at least 3 points for a quadratic fit
            if (n < 3)
                return DiscreteMaxIndex(inputArray, left, right);

            // Accumulate sums for normal equations
            // A * [a b c]^T = y, where:
            // [Sx4 Sx3 Sx2][a]   [Sx2y]
            // [Sx3 Sx2 Sx ][b] = [Sxy ]
            // [Sx2 Sx  N  ][c]   [Sy  ]
            double Sx = 0.0, Sx2 = 0.0, Sx3 = 0.0, Sx4 = 0.0;
            double Sy = 0.0, Sxy = 0.0, Sx2y = 0.0;

            for (int i = left; i <= right; ++i)
            {
                double x = i;
                double y = inputArray[i];
                double x2 = x * x;
                double x3 = x2 * x;
                double x4 = x3 * x;

                Sx += x;
                Sx2 += x2;
                Sx3 += x3;
                Sx4 += x4;

                Sy += y;
                Sxy += x * y;
                Sx2y += x2 * y;
            }

            double[,] A = new double[3, 3]
            {
                { Sx4, Sx3, Sx2 },
                { Sx3, Sx2, Sx  },
                { Sx2, Sx,  n   }
            };
            double[] rhs = new double[3] { Sx2y, Sxy, Sy };
            if (!Solve3x3(A, rhs, out double a, out double b, out _))
            {
                // Singular or ill conditioned system, fall back
                return DiscreteMaxIndex(inputArray, left, right);
            }

            // If a is very close to zero or opens upward, take the discrete max
            const double eps = 1e-12;
            if (Math.Abs(a) < eps || a >= 0.0)
                return DiscreteMaxIndex(inputArray, left, right);

            // Vertex of the fitted parabola
            double peakX = -b / (2.0 * a);

            // Clamp to the window in case of slight extrapolation
            if (peakX < left) peakX = left;
            if (peakX > right) peakX = right;

            return peakX;
        }

        private static int DiscreteMaxIndex(float[] arr, int left, int right)
        {
            int maxIdx = left;
            float maxVal = arr[left];
            for (int i = left + 1; i <= right; ++i)
            {
                if (arr[i] > maxVal)
                {
                    maxVal = arr[i];
                    maxIdx = i;
                }
            }
            return maxIdx;
        }

        // Simple Gaussian elimination with partial pivoting for a 3x3 system
        private static bool Solve3x3(double[,] A, double[] b, out double a, out double d, out double c)
        {
            // We solve A * x = b, x = [a d c]
            // Copy to local augmented matrix for row ops: [A | b]
            double[,] M = new double[3, 4]
            {
                { A[0,0], A[0,1], A[0,2], b[0] },
                { A[1,0], A[1,1], A[1,2], b[1] },
                { A[2,0], A[2,1], A[2,2], b[2] }
            };

            // Forward elimination with partial pivoting
            for (int col = 0; col < 3; ++col)
            {
                // Pivot row
                int pivot = col;
                double best = Math.Abs(M[col, col]);
                for (int r = col + 1; r < 3; ++r)
                {
                    double val = Math.Abs(M[r, col]);
                    if (val > best)
                    {
                        best = val;
                        pivot = r;
                    }
                }

                if (best < 1e-18)
                {
                    a = d = c = 0.0;
                    return false; // Singular
                }

                // Swap if needed
                if (pivot != col)
                {
                    for (int k = col; k < 4; ++k)
                    {
                        (M[pivot, k], M[col, k]) = (M[col, k], M[pivot, k]);
                    }
                }

                // Normalize pivot row
                double pivVal = M[col, col];
                for (int k = col; k < 4; ++k)
                    M[col, k] /= pivVal;

                // Eliminate below
                for (int r = col + 1; r < 3; ++r)
                {
                    double factor = M[r, col];
                    if (factor == 0.0) continue;
                    for (int k = col; k < 4; ++k)
                        M[r, k] -= factor * M[col, k];
                }
            }

            // Back substitution
            double x2 = M[2, 3];                        // row 2: 0 0 1 | x2
            double x1 = M[1, 3] - M[1, 2] * x2;         // row 1: 0 1 * | rhs
            double x0 = M[0, 3] - M[0, 2] * x2 - M[0, 1] * x1; // row 0: 1 * * | rhs

            a = x0; // quadratic term
            d = x1; // linear term (named d to avoid clash with local 'double d')
            c = x2; // constant term
            return true;
        }
    }
}