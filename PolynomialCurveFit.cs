using System.Linq;
using MathNet.Numerics;

namespace Bahtinov_Collimator
{
    public class PolynomialCurveFit
    {
        public static double FindMaxValueIndex(float[] inputArray, int estimatedPosition, int range)
        {
            if (estimatedPosition <= 0)
                return 0.0f;

            float[] tempArray1 = new float[2 * range + 1];
            float[] tempArray2 = new float[2 * range + 1];

            for (int index = -range; index <= range; ++index)
            {
                tempArray1[index + range] = (float)(index + estimatedPosition);
                tempArray2[index + range] = inputArray[index + estimatedPosition];
            }

            // convert float array to degrees
            double[] indexes = tempArray1.Select(f => (double)f).ToArray();
            double[] values = tempArray2.Select(f => (double)f).ToArray();

            // Perform the least squares regression
            double[] coefficients = Fit.Polynomial(indexes, values, 2); // Fit a polynomial of degree 2

            // Find the x-coordinate of the peak point using the polynomial coefficients
            double peakX = -coefficients[1] / (2 * coefficients[2]); // x = -b / (2a), for a quadratic polynomial

            return peakX;
        }
    }
}
