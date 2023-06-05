using System;
using System.Drawing;

namespace Bahtinov_Collimator
{
    public class BahtinovData
    {
        private float[] lineAngles;
        private float[] lineIndex;
        private float[] lineValue;
        private Rectangle imageFrame;

        public BahtinovData(int size, Rectangle imageRect)
        {
            lineAngles = new float[size];
            lineIndex = new float[size];
            lineValue = new float[size];

            ClearArrays();

            this.imageFrame = imageRect;
        }

        private void ClearArrays()
        {
            // Initialize arrays to 0.0f
            for (int i = 0; i < lineAngles.Length; i++)
            {
                lineAngles[i] = 0.0f;
                lineIndex[i] = 0.0f;
                lineValue[i] = 0.0f;
            }
        }

        public float[] LineAngles
        {
            get { return lineAngles; }
            set { lineAngles = value; }
        }

        public float[] LineIndex
        {
            get { return lineIndex; }
            set { lineIndex = value; }
        }

        public float[] LineValue
        {
            get { return lineValue; }
            set { lineValue = value; }
        }

        public float GetLineAngle(int index)
        {
            return lineAngles[index];
        }

        public void Sort()
        {
            // save first 3 brightest
            BahtinovData brightest3 = Copy(3);

            ArrangeTribahtinovAngles();

            // check if this is a single or triple bahtinov pattern
            if (CheckAngleBetween(LineAngles[1], this.LineAngles[4], 55.0f, 65.0f) &&
                CheckAngleBetween(lineAngles[4], lineAngles[7], 55.0f, 65.0f))
            {
                // it is a tribahtinov mask just return, nothing else to do
                return;
            }

            // treat this as a bahtinov mask
            lineAngles = new float[3];
            lineIndex = new float[3];
            lineValue = new float[3];

            ClearArrays();

            // copy back the three brightest lines
            for (int i = 0; i < lineAngles.Length; i++)
            {
                lineAngles[i] = brightest3.lineAngles[i];
                lineIndex[i] = brightest3.lineIndex[i];
                lineValue[i] = brightest3.lineValue[i];
            }

            ArrangeBahtinovAngles();

            if (CheckAngleDifference(LineAngles[0], LineAngles[1], 35.0f) && CheckAngleDifference(LineAngles[1], LineAngles[2], 35.0f))
            {
                // valid bahtinov image
                return;
            }
            else
            {
                // clear out all data to signify a failure to identify bahtinov lines
                LineAngles = new float[0];
                LineIndex = new float[0];
                LineValue = new float[0];

                return;
            }
        }

        private void ArrangeTribahtinovAngles()
        {
            // initial sort
            this.BubbleSort();

            // check first lines and reverse if needed 
            if (!CheckAngleDifference(this.LineAngles[0], this.lineAngles[2], 50.0f))
            {
                if (CheckAngleDifference(this.LineAngles[0], this.lineAngles[1], 25.0f))
                    ReverseAngle(this.LineAngles.Length - 1);
                else
                {
                    ReverseAngle(this.LineAngles.Length - 1);
                    ReverseAngle(this.LineAngles.Length - 2);
                }
                this.BubbleSort();
            }
        }

        private void ArrangeBahtinovAngles()
        {
            // initial sort
            this.BubbleSort();

            // check first lines and reverse if needed 
            if (!CheckAngleDifference(this.LineAngles[0], this.lineAngles[2], 65.0f))
            {
                if (CheckAngleDifference(this.LineAngles[0], this.lineAngles[1], 25.0f))
                    ReverseAngle(this.LineAngles.Length - 1);
                else
                {
                    ReverseAngle(this.LineAngles.Length - 1);
                    ReverseAngle(this.LineAngles.Length - 2);
                }
                this.BubbleSort();
            }
        }

        private void ReverseAngle(int index)
        {
            float newAngle = lineAngles[index] - 3.141593f;
            lineAngles[index] = newAngle;
            lineIndex[index] = imageFrame.Height - lineIndex[index];
        }

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
                throw new Exception();
            }
            return copyArray;
        }

        public static bool CheckAngleDifference(double angle1Radians, double angle2Radians, double degrees)
        {
            double degreesToRadians = Math.PI / 180.0;
            double degreesInRadians = degrees * degreesToRadians;

            double angleDifference = Math.Abs(angle1Radians - angle2Radians);

            if (angleDifference < degreesInRadians)
            {
                return true;
            }

            return false;
        }

        public static bool CheckAngleBetween(double angle1Radians, double angle2Radians, double lowerDegrees, double upperDegrees)
        {
            double degreesToRadians = Math.PI / 180.0;
            double lowerRadians = lowerDegrees * degreesToRadians;
            double upperRadians = upperDegrees * degreesToRadians;
            double angleDifference = Math.Abs(angle1Radians - angle2Radians);

            if (lowerRadians < angleDifference && angleDifference < upperRadians)
            {
                return true;
            }
            return false;
        }

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

                // If no elements were swapped in the inner loop, the arrays are already sorted
                if (!swapped)
                    break;
            }
        }
    }
}
