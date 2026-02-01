using System;
using System.Windows.Forms;
using System.Deployment.Application;

namespace Bahtinov_Collimator.Helper
{
    internal class Helper
    {
        #region Methods
        /// <summary>
        /// Checks for the latest update and performs the update if available.
        /// </summary>
        public static void GetLatestUpdate()
        {
            try
            {
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;
                    UpdateCheckInfo info = ad.CheckForDetailedUpdate();

                    if (info.UpdateAvailable)
                    {
                        // Ask the user for confirmation before updating
                        DialogResult result = DarkMessageBox.Show(
                            UiText.Current.UpdateAvailableMessage,
                            UiText.Current.UpdateAvailableTitle,
                            MessageBoxIcon.Question,
                            MessageBoxButtons.YesNo
                        );

                        if (result == DialogResult.Yes)
                        {
                            // Perform the update
                            bool success = ad.Update(); // Updates the application asynchronously

                            if (success)
                            {
                                DarkMessageBox.Show(
                                    UiText.Current.UpdateDownloadedMessage,
                                    UiText.Current.UpdateDownloadedTitle,
                                    MessageBoxIcon.Information,
                                    MessageBoxButtons.OK
                                );
                            }
                            else
                            {
                                DarkMessageBox.Show(
                                    UiText.Current.UpdateFailedMessage,
                                    UiText.Current.UpdateFailedTitle,
                                    MessageBoxIcon.Exclamation,
                                    MessageBoxButtons.OK
                                );
                            }
                        }
                        else
                        {
                            DarkMessageBox.Show(
                                UiText.Current.UpdateCanceledMessage,
                                UiText.Current.UpdateCanceledTitle,
                                MessageBoxIcon.Information,
                                MessageBoxButtons.OK
                            );
                        }
                    }
                    else
                    {
                        DarkMessageBox.Show(
                            UiText.Current.UpdateNoneMessage,
                            UiText.Current.UpdateNoneTitle,
                            MessageBoxIcon.Information,
                            MessageBoxButtons.OK
                        );
                    }
                }
            }
            catch (Exception)
            {
                DarkMessageBox.Show(
                    UiText.Current.UpdateNetworkErrorMessage,
                    UiText.Current.UpdateNetworkErrorTitle,
                    MessageBoxIcon.Exclamation,
                    MessageBoxButtons.OK
                );
            }
        }
        #endregion

        #region Line Class

        /// <summary>
        /// Represents a line with start and end points, slope, and intercept.
        /// </summary>
        public class Line
        {
            /// <summary>
            /// Gets the X coordinate of the start point.
            /// </summary>
            public float StartX { get; }

            /// <summary>
            /// Gets the Y coordinate of the start point.
            /// </summary>
            public float StartY { get; }

            /// <summary>
            /// Gets the X coordinate of the end point.
            /// </summary>
            public float EndX { get; }

            /// <summary>
            /// Gets the Y coordinate of the end point.
            /// </summary>
            public float EndY { get; }

            /// <summary>
            /// Gets or sets the slope of the line.
            /// </summary>
            public float Slope { get; set; }

            /// <summary>
            /// Gets or sets the intercept of the line.
            /// </summary>
            public float Intercept { get; private set; }

            /// <summary>
            /// Gets a value indicating whether the line is first or third in some context.
            /// </summary>
            public bool IsFirstOrThird { get; }

            /// <summary>
            /// Gets or sets the calculated Y coordinate of the start point.
            /// </summary>
            public float CalculatedYStart { get; set; }

            /// <summary>
            /// Initializes a new instance of the Line class with the specified start and end points.
            /// </summary>
            /// <param name="startX">The X coordinate of the start point.</param>
            /// <param name="startY">The Y coordinate of the start point.</param>
            /// <param name="endX">The X coordinate of the end point.</param>
            /// <param name="endY">The Y coordinate of the end point.</param>
            /// <param name="isFirstOrThird">A flag indicating whether the line is first or third.</param>
            public Line(float startX, float startY, float endX, float endY, bool isFirstOrThird = false)
            {
                StartX = startX;
                StartY = startY;
                EndX = endX;
                EndY = endY;
                IsFirstOrThird = isFirstOrThird;

                // Calculate Slope and Intercept
                if (EndX != StartX) // Avoid division by zero
                {
                    Slope = (EndY - StartY) / (EndX - StartX);
                    Intercept = StartY - (Slope * StartX);
                }
                else
                {
                    // Handle vertical lines if needed
                    Slope = float.PositiveInfinity;
                    Intercept = float.NaN; // Vertical line does not have a y-intercept
                }
            }
        }

        #endregion
    }

    #region PointD Struct

    /// <summary>
    /// Represents a point in 2D space with double precision.
    /// </summary>
    public struct PointD
    {
        /// <summary>
        /// Gets or sets the X coordinate of the point.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Gets or sets the Y coordinate of the point.
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Initializes a new instance of the PointD struct with the specified coordinates.
        /// </summary>
        /// <param name="x">The X coordinate of the point.</param>
        /// <param name="y">The Y coordinate of the point.</param>
        public PointD(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Calculates the direction from this point to another point.
        /// </summary>
        /// <param name="other">The other point.</param>
        /// <returns>The direction in radians.</returns>
        public double DirectionTo(PointD other)
        {
            return Math.Atan2(other.Y - Y, other.X - X);
        }

        /// <summary>
        /// Calculates the distance from this point to another point.
        /// </summary>
        /// <param name="other">The other point.</param>
        /// <returns>The distance between the points.</returns>
        public double DistanceTo(PointD other)
        {
            return Math.Sqrt(Math.Pow(other.X - X, 2) + Math.Pow(other.Y - Y, 2));
        }
    }

    #endregion
}
