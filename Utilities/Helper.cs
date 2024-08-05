using System;
using System.Windows.Forms;
using System.Deployment.Application;

namespace Bahtinov_Collimator
{
    internal class Helper
    {
        public static void GetLatestUpdate()
        {
            try
            {
                if (!ApplicationDeployment.IsNetworkDeployed)
                {
                    ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;
                    UpdateCheckInfo info = ad.CheckForDetailedUpdate();

                    if (info.UpdateAvailable)
                    {
                        // Ask the user for confirmation before updating
                        DialogResult result = DarkMessageBox.Show(
                            "A new update is available. Do you want to download and install it now?",
                            "Update Available",
                            MessageBoxIcon.Question,
                            MessageBoxButtons.YesNo
                        );

                        if (result == DialogResult.OK)
                        {
                            // Perform the update
                            bool success = ad.Update(); // Updates the application asynchronously
     
                            if (success)
                            {
                                DarkMessageBox.Show(
                                    "New updates have been downloaded. Restart the application to install.",
                                    "Update Completed",
                                    MessageBoxIcon.Information,
                                    MessageBoxButtons.YesNo
                                );
                            }
                            else
                            {
                                DarkMessageBox.Show(
                                    "The update download failed. Please try again.",
                                    "Update Failed",
                                    MessageBoxIcon.Exclamation,
                                    MessageBoxButtons.OK
                                );
                            }
                        }
                        else
                        {
                            DarkMessageBox.Show(
                                "Update canceled.",
                                "Update Canceled",
                                MessageBoxIcon.Information,
                                MessageBoxButtons.OK
                            );
                        }
                    }
                    else
                    {
                        DarkMessageBox.Show(
                            "No new updates are available.",
                            "No Updates",
                            MessageBoxIcon.Information,
                            MessageBoxButtons.OK
                        );
                    }
                }
            }
            catch (Exception)
            {
                DarkMessageBox.Show(
                    "Unable to connect to the update server.",
                    "Network Problem",
                    MessageBoxIcon.Exclamation,
                    MessageBoxButtons.OK
                );
            }
        }

        public class Line
        {
            public float StartX { get; }
            public float StartY { get; }
            public float EndX { get; }
            public float EndY { get; }
            public float Slope { get; set; }
            public float Intercept { get; private set; }
            public bool IsFirstOrThird { get; }
            public float CalculatedYStart {  get; set; }

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
    }
}
