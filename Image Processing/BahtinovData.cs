using System;
using System.Drawing;

namespace Bahtinov_Collimator
{
    public class BahtinovData
    {
        #region Fields

        private float[] lineAngles;
        private float[] lineIndex;
        private float[] lineValue;
        private Rectangle imageFrame;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="BahtinovData"/> class.
        /// </summary>
        /// <param name="size">The size of the arrays.</param>
        /// <param name="imageRect">The rectangle representing the image frame.</param>
        public BahtinovData(int size, Rectangle imageRect)
        {
            lineAngles = new float[size];
            lineIndex = new float[size];
            lineValue = new float[size];

            ClearArrays();

            this.imageFrame = imageRect;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the array of line angles.
        /// </summary>
        public float[] LineAngles
        {
            get { return lineAngles; }
            set { lineAngles = value; }
        }

        /// <summary>
        /// Gets or sets the array of line indices.
        /// </summary>
        public float[] LineIndex
        {
            get { return lineIndex; }
            set { lineIndex = value; }
        }

        /// <summary>
        /// Gets or sets the array of line values.
        /// </summary>
        public float[] LineValue
        {
            get { return lineValue; }
            set { lineValue = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clears the arrays by setting all elements to 0.0f.
        /// </summary>
        private void ClearArrays()
        {
            for (int i = 0; i < lineAngles.Length; i++)
            {
                lineAngles[i] = 0.0f;
                lineIndex[i] = 0.0f;
                lineValue[i] = 0.0f;
            }
        }

        /// <summary>
        /// Gets the line angle at a specified index.
        /// </summary>
        /// <param name="index">The index of the line angle.</param>
        /// <returns>The line angle at the specified index.</returns>
        public float GetLineAngle(int index)
        {
            return lineAngles[index];
        }

        /// <summary>
        /// Sorts the line angles and determines if the data represents a Bahtinov or a Tribahtinov mask.
        /// </summary>
        public void Sort()
        {
            BahtinovData brightest3 = Copy(3);

            ArrangeTribahtinovAngles();

            if (CheckAngleBetween(LineAngles[1], LineAngles[4], 55.0f, 65.0f) &&
                CheckAngleBetween(lineAngles[4], lineAngles[7], 55.0f, 65.0f))
            {
                return; // Tribahtinov mask detected
            }

            lineAngles = new float[3];
            lineIndex = new float[3];
            lineValue = new float[3];

            ClearArrays();

            for (int i = 0; i < lineAngles.Length; i++)
            {
                lineAngles[i] = brightest3.lineAngles[i];
                lineIndex[i] = brightest3.lineIndex[i];
                lineValue[i] = brightest3.lineValue[i];
            }

            ArrangeBahtinovAngles();

            if (CheckAngleDifference(LineAngles[0], LineAngles[1], 35.0f) &&
                CheckAngleDifference(LineAngles[1], LineAngles[2], 35.0f))
            {
                return; // Valid Bahtinov image detected
            }
            else
            {
                // Clear all data to signify failure to identify Bahtinov lines
                LineAngles = new float[0];
                LineIndex = new float[0];
                LineValue = new float[0];
            }
        }

        /// <summary>
        /// Arranges angles for a Tribahtinov pattern.
        /// </summary>
        private void ArrangeTribahtinovAngles()
        {
            BubbleSort();

            if (!CheckAngleDifference(LineAngles[0], lineAngles[2], 40.0f))
            {
                if (CheckAngleDifference(LineAngles[0], LineAngles[1], 25.0f))
                    ReverseAngle(LineAngles.Length - 1);
                else
                {
                    ReverseAngle(LineAngles.Length - 1);
                    ReverseAngle(LineAngles.Length - 2);
                }
                BubbleSort();
            }
        }

        /// <summary>
        /// Arranges angles for a Bahtinov pattern.
        /// </summary>
        private void ArrangeBahtinovAngles()
        {
            BubbleSort();

            if (!CheckAngleDifference(LineAngles[0], LineAngles[2], 65.0f))
            {
                if (CheckAngleDifference(LineAngles[0], LineAngles[1], 25.0f))
                    ReverseAngle(LineAngles.Length - 1);
                else
                {
                    ReverseAngle(LineAngles.Length - 1);
                    ReverseAngle(LineAngles.Length - 2);
                }
                BubbleSort();
            }
        }

        /// <summary>
        /// Reverses the angle at a specified index.
        /// </summary>
        /// <param name="index">The index of the angle to reverse.</param>
        private void ReverseAngle(int index)
        {
            float newAngle = lineAngles[index] - (float)Math.PI;
            lineAngles[index] = newAngle;
            lineIndex[index] = imageFrame.Height - lineIndex[index];
        }

        /// <summary>
        /// Creates a copy of the current <see cref="BahtinovData"/> instance.
        /// </summary>
        /// <param name="size">The size of the copy.</param>
        /// <returns>A new <see cref="BahtinovData"/> instance that is a copy of the current one.</returns>
        private BahtinovData Copy(int size)
        {
            BahtinovData copyArray = new BahtinovData(size, imageFrame);

            if (this.lineAngles.Length >= size)
            {
                for (int i = 0; i < size; i++)
                {
                    copyArray.lineAngles[i] = lineAngles[i];
                    copyArray.lineValue[i] = lineValue[i];
                    copyArray.lineIndex[i] = lineIndex[i];
                }
            }
            else
            {
                throw new Exception("Insufficient data to copy.");
            }
            return copyArray;
        }

        /// <summary>
        /// Checks if the difference between two angles is within a specified range.
        /// </summary>
        /// <param name="angle1Radians">The first angle in radians.</param>
        /// <param name="angle2Radians">The second angle in radians.</param>
        /// <param name="degrees">The range in degrees.</param>
        /// <returns><c>true</c> if the angle difference is within the range; otherwise, <c>false</c>.</returns>
        public static bool CheckAngleDifference(double angle1Radians, double angle2Radians, double degrees)
        {
            double degreesToRadians = Math.PI / 180.0;
            double degreesInRadians = degrees * degreesToRadians;

            double angleDifference = Math.Abs(angle1Radians - angle2Radians);

            return angleDifference < degreesInRadians;
        }

        /// <summary>
        /// Checks if the difference between two angles is between specified lower and upper bounds.
        /// </summary>
        /// <param name="angle1Radians">The first angle in radians.</param>
        /// <param name="angle2Radians">The second angle in radians.</param>
        /// <param name="lowerDegrees">The lower bound in degrees.</param>
        /// <param name="upperDegrees">The upper bound in degrees.</param>
        /// <returns><c>true</c> if the angle difference is within the range; otherwise, <c>false</c>.</returns>
        public static bool CheckAngleBetween(double angle1Radians, double angle2Radians, double lowerDegrees, double upperDegrees)
        {
            double degreesToRadians = Math.PI / 180.0;
            double lowerRadians = lowerDegrees * degreesToRadians;
            double upperRadians = upperDegrees * degreesToRadians;
            double angleDifference = Math.Abs(angle1Radians - angle2Radians);

            return lowerRadians < angleDifference && angleDifference < upperRadians;
        }

        /// <summary>
        /// Sorts the line angles and associated arrays using the Bubble Sort algorithm.
        /// </summary>
        private void BubbleSort()
        {
            int n = lineAngles.Length;
            bool swapped;

            for (int i = 0; i < n - 1; i++)
            {
                swapped = false;

                for (int j = 0; j < n - i - 1; j++)
                {
                    if (lineAngles[j] > lineAngles[j + 1])
                    {
                        // Swap elements
                        (lineAngles[j], lineAngles[j + 1]) = (lineAngles[j + 1], lineAngles[j]);
                        (lineIndex[j], lineIndex[j + 1]) = (lineIndex[j + 1], lineIndex[j]);
                        (lineValue[j], lineValue[j + 1]) = (lineValue[j + 1], lineValue[j]);
                        swapped = true;
                    }
                }

                // If no elements were swapped, the arrays are already sorted
                if (!swapped)
                    break;
            }
        }

        #endregion
    }
}
