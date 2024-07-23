using System.Linq;
using MathNet.Numerics;

namespace Bahtinov_Collimator
{
    public class PolynomialCurveFit
    {
        #region Methods

        /// <summary>
        /// Finds the x-coordinate of the maximum value in a specified range around an estimated position.
        /// </summary>
        /// <param name="inputArray">Array of float values representing the data to be analyzed.</param>
        /// <param name="estimatedPosition">The estimated position of the maximum value.</param>
        /// <param name="range">The range around the estimated position to consider for fitting.</param>
        /// <returns>The x-coordinate of the maximum value in the specified range.</returns>
        public static double FindMaxValueIndex(float[] inputArray, int estimatedPosition, int range)
        {
            // Return 0 if estimated position is non-positive
            if (estimatedPosition <= 0)
                return 0.0;

            // Define temporary arrays to hold the range of x-values and corresponding y-values
            float[] tempArray1 = new float[2 * range + 1];
            float[] tempArray2 = new float[2 * range + 1];

            // Populate temporary arrays with x-values and y-values
            for (int index = -range; index <= range; ++index)
            {
                tempArray1[index + range] = (float)(index + estimatedPosition);
                tempArray2[index + range] = inputArray[index + estimatedPosition];
            }

            // Convert float arrays to double arrays for polynomial fitting
            double[] indexes = tempArray1.Select(f => (double)f).ToArray();
            double[] values = tempArray2.Select(f => (double)f).ToArray();

            // Perform polynomial fitting with degree 2 (quadratic polynomial)
            double[] coefficients = Fit.Polynomial(indexes, values, 2);

            // Calculate the x-coordinate of the peak point (vertex) of the quadratic polynomial
            double peakX = -coefficients[1] / (2 * coefficients[2]); // x = -b / (2a)

            return peakX;
        }

        #endregion
    }
}
