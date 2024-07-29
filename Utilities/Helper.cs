using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MathNet.Numerics;
using System.Deployment.Application;

namespace Bahtinov_Collimator
{
    internal class Helper
    {
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
                        bool success = ad.Update();//Updates the application asynchronously

                        if (ad.Update())
                            MessageBox.Show("New updates have been downloaded. Restart the application to install", "Update Available", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        else
                            MessageBox.Show("The update download failed, please try again", "Update Available", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                    else
                        MessageBox.Show("No new updates are available", "No Updates", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to the update server", "Network Problem", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
