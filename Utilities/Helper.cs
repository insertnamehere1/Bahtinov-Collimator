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
                        DialogResult result = DarkMessageBox.Show(
                            UiText.Current.UpdateAvailableMessage,
                            UiText.Current.UpdateAvailableTitle,
                            MessageBoxIcon.Question,
                            MessageBoxButtons.YesNo
                        );

                        if (result == DialogResult.Yes)
                        {
                            bool success = ad.Update();

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
