using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Bahtinov_Collimator.BahtinovLineDataEventArgs;

namespace Bahtinov_Collimator.Image_Processing
{
    internal class DefocusStarProcessing
    {

        public delegate void DefocusCircleEventHandler(object sender, DefocusCircleEventArgs e);

        public static event DefocusCircleEventHandler DefocusCircleEvent;

        public DefocusStarProcessing() { }

        public void DisplayDefocusImage(Bitmap image) 
        {
            if (image == null) 
                return;

            // Calculate center of the image
            int imgCenterX = image.Width / 2;
            int imgCenterY = image.Height / 2;

            // Find the average radius of the inner transition
            var (innerCentre, innerRadius) = FindAverageInnerRadius(image, imgCenterX, imgCenterY);
            var (outerCentre, outerRadius) = FindAverageOuterRadius(image, imgCenterX, imgCenterY);

            var (Distance, Direction) = CalculateDistanceAndDirection(innerCentre, outerCentre);

            // send an event
            DefocusCircleEvent?.Invoke(null, new DefocusCircleEventArgs(image, new Point(innerCentre.X - imgCenterX, innerCentre.Y - imgCenterY), innerRadius, 
                                                                               new Point(outerCentre.X - imgCenterX, outerCentre.Y - imgCenterY), outerRadius));
            return;
        }

        private static (double Distance, double Direction) CalculateDistanceAndDirection(Point firstPoint, Point secondPoint)
        {
            // Calculate the difference in x and y coordinates
            double deltaX = secondPoint.X - firstPoint.X;
            double deltaY = secondPoint.Y - firstPoint.Y;

            // Calculate the distance using the Pythagorean theorem
            double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            // Calculate the direction in radians
            double directionRadians = Math.Atan2(deltaY, deltaX);

            // Convert direction to degrees
            double directionDegrees = directionRadians * (180.0 / Math.PI);

            // Normalize the direction to a range of [0, 360) degrees
            if (directionDegrees < 0)
            {
                directionDegrees += 360.0;
            }

            return (distance, directionDegrees);
        }

        private (Point center, int radius) CalculateAverageRadius(Bitmap image, int centerX, int centerY, bool isInnerRadius)
        {
            int maxRadius = Math.Min(image.Width, image.Height) / 2;
            int radiusSum = 0;
            int transitionCount = 0;
            double transitionXSum = 0;
            double transitionYSum = 0;

            // Iterate over angles from 0 to 359 degrees
            for (double angle = 0; angle < 360; angle += 1)
            {
                double lineSumBrightness = 0;
                int numPoints = 0;

                // Iterate over radii from maxRadius or from 0
                int startRadius = isInnerRadius ? 0 : maxRadius;
                int endRadius = isInnerRadius ? maxRadius : 0;
                int step = isInnerRadius ? 1 : -1;

                for (int r = startRadius; isInnerRadius ? r < endRadius : r > endRadius; r += step)
                {
                    int x = centerX + (int)(r * Math.Cos(angle * Math.PI / 180));
                    int y = centerY + (int)(r * Math.Sin(angle * Math.PI / 180));

                    if (x >= 0 && x < image.Width && y >= 0 && y < image.Height)
                    {
                        Color pixelColor = image.GetPixel(x, y);
                        double brightness = pixelColor.GetBrightness();

                        if (brightness != 0)
                        {
                            lineSumBrightness += brightness;
                            numPoints++;
                        }
                    }
                }

                double averageLineBrightness = numPoints > 0 ? lineSumBrightness / numPoints : 0.0;

                // Detect transitions and compute average radius
                for (int r = startRadius; isInnerRadius ? r < endRadius : r > endRadius; r += step)
                {
                    int x = centerX + (int)(r * Math.Cos(angle * Math.PI / 180));
                    int y = centerY + (int)(r * Math.Sin(angle * Math.PI / 180));

                    if (x >= 0 && x < image.Width && y >= 0 && y < image.Height)
                    {
                        Color pixelColor = image.GetPixel(x, y);
                        double brightness = pixelColor.GetBrightness();

                        if (brightness > averageLineBrightness)
                        {
                            radiusSum += r;
                            transitionCount++;
                            transitionXSum += x - centerX;
                            transitionYSum += y - centerY;
                            break; // Move to the next angle once the transition is found
                        }
                    }
                }
            }

            // Calculate and return the average radius and center of all transitions
            int radius = transitionCount > 0 ? (int)Math.Round((double)radiusSum / transitionCount) : maxRadius;
            Point center = new Point((int)(transitionXSum / transitionCount) + centerX, (int)(transitionYSum / transitionCount) + centerY);

            return (center, radius);
        }

        private (Point center, int radius) FindAverageInnerRadius1(Bitmap image, int centerX, int centerY)
        {
            return CalculateAverageRadius(image, centerX, centerY, true);
        }

        private (Point center, int radius) FindAverageOuterRadius1(Bitmap image, int centerX, int centerY)
        {
            return CalculateAverageRadius(image, centerX, centerY, false);
        }













        private (Point center, int radius) FindAverageInnerRadius(Bitmap image, int centerX, int centerY)
        {
            int maxRadius = Math.Min(image.Width, image.Height) / 2;
            int radiusSum = 0;
            int transitionCount = 0;
            double transitionXSum = 0;
            double transitionYSum = 0;

            // Store brightness values for each angle
            Dictionary<double, List<double>> angleBrightness = new Dictionary<double, List<double>>();

            // Iterate over angles from 0 to 359 degrees
            for (double angle = 0; angle < 360; angle += 1)
            {
                angleBrightness[angle] = new List<double>();

                // Check the radius along this angle, from center outwards
                for (int r = 0; r < maxRadius; r++)
                {
                    int x = centerX + (int)(r * Math.Cos(angle * Math.PI / 180));
                    int y = centerY + (int)(r * Math.Sin(angle * Math.PI / 180));

                    if (x >= 0 && x < image.Width && y >= 0 && y < image.Height)
                    {
                        Color pixelColor = image.GetPixel(x, y);
                        double brightness = pixelColor.GetBrightness();

                        if(brightness > 0.0)
                            angleBrightness[angle].Add(brightness);
                    }
                }
            }

            // Detect average brightness and transition for each angle
            for (double angle = 0; angle < 360; angle += 1)
            {
                double lineSumBrightness = angleBrightness[angle].Sum();
                int numPoints = angleBrightness[angle].Count;
                double averageLineBrightness = numPoints > 0 ? lineSumBrightness / numPoints : 0.0;

                // Check radius along this angle for transition (dark to bright)
                for (int r = 0; r < maxRadius; r++)
                {
                    int x = centerX + (int)(r * Math.Cos(angle * Math.PI / 180));
                    int y = centerY + (int)(r * Math.Sin(angle * Math.PI / 180));

                    if (x >= 0 && x < image.Width && y >= 0 && y < image.Height)
                    {
                        Color pixelColor = image.GetPixel(x, y);
                        double brightness = pixelColor.GetBrightness();

                        // Detect transition based on average brightness
                        if (brightness > averageLineBrightness)
                        {
                            radiusSum += r;
                            transitionCount++;
                            transitionXSum += x - centerX;
                            transitionYSum += y - centerY;
                            break; // Move to the next angle once the transition is found
                        }
                    }
                }
            }

            // Calculate and return the average radius and center of all inner transitions
            int radius = transitionCount > 0 ? (int)Math.Round((double)radiusSum / (double)transitionCount) : maxRadius;

//            Point centre = new Point((int)transitionXSum / 180, (int)transitionYSum / 180);


//            Point centre = new Point((int)(transitionXSum / transitionCount) + centerX, (int)(transitionYSum / transitionCount) + centerY);
            Point centre = new Point((int)(transitionXSum / 180) + centerX, (int)(transitionYSum / 180) + centerY);


            return (centre, radius);
        }

        private (Point center, int radius) FindAverageOuterRadius(Bitmap image, int centerX, int centerY)
        {
            int maxRadius = Math.Min(image.Width, image.Height) / 2;
            int radiusSum = 0;
            int transitionCount = 0;
            double transitionXSum = 0;
            double transitionYSum = 0;

            // Iterate over angles from 0 to 359 degrees
            for (double angle = 0; angle < 360; angle += 1)
            {
                double lineSumBrightness = 0;
                int numPoints = 0;

                // Calculate the average brightness along this line
                for (int r = maxRadius; r > 0; r--)
                {
                    int x = centerX + (int)(r * Math.Cos(angle * Math.PI / 180));
                    int y = centerY + (int)(r * Math.Sin(angle * Math.PI / 180));

                    if (x >= 0 && x < image.Width && y >= 0 && y < image.Height)
                    {
                        Color pixelColor = image.GetPixel(x, y);
                        double brightness = pixelColor.GetBrightness();

                        if (brightness > 0.0)
                        {
                            lineSumBrightness += brightness;
                            numPoints++;
                        }
                    }
                }

                double averageLineBrightness = numPoints > 0 ? lineSumBrightness / numPoints : 0.0;

                // Now search for the transition based on the average brightness
                for (int r = maxRadius; r > 0; r--)
                {
                    int x = centerX + (int)(r * Math.Cos(angle * Math.PI / 180));
                    int y = centerY + (int)(r * Math.Sin(angle * Math.PI / 180));

                    if (x >= 0 && x < image.Width && y >= 0 && y < image.Height)
                    {
                        Color pixelColor = image.GetPixel(x, y);
                        double brightness = pixelColor.GetBrightness();

                        // Detect transition (dark to bright) using the average brightness
                        if (brightness > averageLineBrightness)
                        {
                            radiusSum += r;
                            transitionCount++;
                            transitionXSum += x - centerX;
                            transitionYSum += y - centerY;
                            break; // Move to the next angle once the transition is found
                        }
                    }
                }
            }

            // Calculate and return the average radius of all inner transitions
            int radius = transitionCount > 0 ? (int)Math.Round((double)radiusSum / (double)transitionCount) : maxRadius;
            //            Point centre = new Point((int)(transitionXSum / transitionCount) + centerX, (int)(transitionYSum / transitionCount) + centerY);
            Point centre = new Point((int)(transitionXSum / 180) + centerX, (int)(transitionYSum / 180) + centerY);

            return (centre, radius);
        }




















    }
}
