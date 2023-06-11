// Sections of the following code are covered by and existing copyright
//
//MIT License

//Copyright (c) 2017 Wytse Jan Posthumus

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using Microsoft.Win32;
using System;
using System.Deployment.Application;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace Bahtinov_Collimator
{
    public partial class Form1 : Form
    {
        private const float ErrorMarkerScalingValue = 20.0f;

        private const int yOffset = 1;
        private const int updateinterval = 100;
        private Bitmap bufferedPicture;
        private Bitmap currentPicture;
        private readonly ScreenCap screenCapture;
        private bool screenCaptureRunningFlag = false;
        private bool showRedFocus = true;
        private bool showGreenFocus = true;
        private bool showBlueFocus = true;
        private bool forceUpdate = false;
        private readonly System.Windows.Forms.GroupBox[] groupBoxes;
        private const int lineCount = 9;          // lines for a tri-bahtinov mask
        private BahtinovData lineData;
        private float lastFocusErrorValue = 0.0f;
        private readonly Timer mousePositionTimer;
        private readonly Rectangle RedGroupBoxScreenBounds;
        private readonly Rectangle GreenGroupBoxScreenBounds;
        private Rectangle BlueGroupBoxScreenBounds;
        private bool menuActive = false;
        private bool nightEnable = false;

        // settings
        private readonly Settings settingsDialog;
        private int apertureSetting;
        private int focalLengthSetting;
        private double pixelSizeSetting;

        public Form1()
        {
            InitializeComponent();
            screenCapture = new ScreenCap();
            settingsDialog = new Settings();

            UpdateTimer.Interval = updateinterval;

            mousePositionTimer = new Timer() { Interval = 100 };
            mousePositionTimer.Tick += MousePositionTimer_Tick;
            mousePositionTimer.Start();

            RedGroupBoxScreenBounds = RedGroupBox.Bounds;
            GreenGroupBoxScreenBounds = GreenGroupBox.Bounds;
            BlueGroupBoxScreenBounds = BlueGroupBox.Bounds;

            LoadSettings();

            // turn off Green and Blue GroupBoxes
            GreenGroupBox.Visible = false;
            BlueGroupBox.Visible = false;

            // set red Group Box to Black text
            RedGroupBox.ForeColor = Color.Black;

            groupBoxes = new System.Windows.Forms.GroupBox[] { RedGroupBox, GreenGroupBox, BlueGroupBox };
        }

        private void LoadSettings()
        {
            // load settings
            apertureSetting = Properties.Settings.Default.Aperture;
            focalLengthSetting = Properties.Settings.Default.FocalLength;
            pixelSizeSetting = Properties.Settings.Default.PixelSize;
        }

        private void Reset()
        {
            UpdateTimer.Stop();
            LoadSettings();
            bufferedPicture = null;
            screenCaptureRunningFlag = false;
            StartButton.Text = "Start";
        }

        private static bool IsNightLightEnabled()
        { 
            const string BlueLightReductionStateValue = @"SOFTWARE\Microsoft\Windows\CurrentVersion\CloudStore\Store\DefaultAccount\Current\default$windows.data.bluelightreduction.settings\windows.data.bluelightreduction.settings";
            const string BlueLightReductionStateKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\CloudStore\Store\DefaultAccount\Current\default$windows.data.bluelightreduction.bluelightreductionstate\windows.data.bluelightreduction.bluelightreductionstate";

            using (var key = Registry.CurrentUser.OpenSubKey(BlueLightReductionStateKey))
            {
                var data = key?.GetValue("Data");
        
                if (data is null)
                    return false;

                var byteData = (byte[])data;

                if (byteData.Length > 24 && byteData[23] == 0x10 && byteData[24] == 0x00)
                {
                    using (var key2 = Registry.CurrentUser.OpenSubKey(BlueLightReductionStateValue))
                    {
                        var data2 = key2?.GetValue("Data");

                        if (data2 is null)
                            return false;

                        var byteData2 = (byte[])data2;

                        // is the blue reduction > 50%
                        if (byteData2.Length > 36 && byteData2[36] < 0x32)
                            return true;
                    }
                }
                return false;
            }
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            // check for nightlight settings in windows
            if (!nightEnable && IsNightLightEnabled())
            {
                forceUpdate = true;
                nightEnable = true;
            }
            else if (nightEnable && !IsNightLightEnabled())
            {
                nightEnable = false;
                forceUpdate = true;
            }

            ProcessAndDisplay();
        }

        private void ProcessAndDisplay()
        {
            Bitmap paddedImage;

            currentPicture = new Bitmap(ScreenCap.GetScreenImage());

            if (!forceUpdate && ImageProcessing.Equal(currentPicture, bufferedPicture))
            {
                return;
            }

            // is this the first pass?
            if (bufferedPicture == null)
            {
                bufferedPicture = new Bitmap(currentPicture);
                paddedImage = new Bitmap(ImageProcessing.PadAndCenterBitmap(currentPicture, pictureBox));
                lineData = ImageProcessing.FindBrightestLines(paddedImage, lineCount);
                lineData.Sort();
            }
            else
            {
                bufferedPicture = new Bitmap(currentPicture);
                paddedImage = new Bitmap(ImageProcessing.PadAndCenterBitmap(currentPicture, pictureBox));
                lineData = ImageProcessing.FindSubpixelLines(paddedImage, lineData.LineValue.Length, lineData);
            }

            if (lineData.LineAngles.Length > 3)
            {
                GreenGroupBox.Visible = true;
                BlueGroupBox.Visible = true;
                RedGroupBox.ForeColor = nightEnable ?  Color.Black : Color.Red;
            }
            else
            {
                GreenGroupBox.Visible = false;
                BlueGroupBox.Visible = false;
                RedGroupBox.ForeColor = Color.Black;
            }

            pictureBox.Image = new Bitmap(paddedImage);

            if (lineData.LineAngles.Length > 3)
            {
                Rectangle rect = new Rectangle(0, 0, paddedImage.Width, paddedImage.Height);

                BahtinovData data1 = new BahtinovData(3, rect);
                BahtinovData data2 = new BahtinovData(3, rect);
                BahtinovData data3 = new BahtinovData(3, rect);

                for (int i = 0; i < 3; i++)
                {
                    data1.LineAngles[i] = lineData.LineAngles[i];
                    data2.LineAngles[i] = lineData.LineAngles[i + 3];
                    data3.LineAngles[i] = lineData.LineAngles[i + 6];

                    data1.LineIndex[i] = lineData.LineIndex[i];
                    data2.LineIndex[i] = lineData.LineIndex[i + 3];
                    data3.LineIndex[i] = lineData.LineIndex[i + 6];
                }

                if (showRedFocus && bufferedPicture != null)
                    DisplayLines(data1, paddedImage, 0);
                if (showGreenFocus && bufferedPicture != null)
                    DisplayLines(data2, paddedImage, 1);
                if (showBlueFocus && bufferedPicture != null)
                    DisplayLines(data3, paddedImage, 2);
            }
            else
            {
                if (lineData.LineAngles.Length == 3)
                    DisplayLines(lineData, paddedImage, 0);
                else
                {
                    Reset();
                    MessageBox.Show("Unable to detect Bahtinov image lines", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }

            forceUpdate = false;
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (screenCaptureRunningFlag == false)
            {
                bufferedPicture = null;
                currentPicture = null;
                pictureBox.Image = null;

                screenCapture.Reset();
                screenCapture.ShowDialog(this);

                Rectangle imageArea = screenCapture.GetImageCoordinates();

                if (imageArea.Width == 0 || imageArea.Height == 0)
                {
                    MessageBox.Show("Invalid Selection Area, Press Start and try again", "Note",
                                     MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                screenCaptureRunningFlag = true;
                UpdateTimer.Start();
                StartButton.Text = "Stop";
            }
            else
            {
                UpdateTimer.Stop();
                screenCaptureRunningFlag = false;
                StartButton.Text = "Start";
                pictureBox.Image = null;
            }
        }

        private void DisplayLines(BahtinovData bahtinovLines, Bitmap starImage, int group)
        {
            int width = starImage.Width;
            int height = starImage.Height;

            // centre pixels for X and Y in the starImage
            float starImage_X_Centre = (float)(((double)width + 1.0) / 2.0);
            float starImage_Y_Centre = (float)(((double)height + 1.0) / 2.0);

            float first_lineSlope = 0.0f;
            float third_lineSlope = 0.0f;
            float first_calculatedY_for_XStart = 0.0f;
            float third_calculatedY_for_XStart = 0.0f;

            float second_lineStart_X = 0.0f;
            float second_lineStart_Y = 0.0f;
            float second_lineEnd_X = 0.0f;
            float second_lineEnd_Y = 0.0f;

            int penWidth = 1;
            Pen dashPen = new Pen(Color.Yellow, (float)penWidth) { DashStyle = DashStyle.Dash };

            penWidth = 2;
            Pen largeDashPen = new Pen(Color.Black, penWidth) { DashStyle = DashStyle.Dash };
            Pen largeSolidPen = new Pen(Color.Yellow, (float)penWidth) { DashStyle = DashStyle.Solid };

            Graphics graphics = Graphics.FromImage(pictureBox.Image);

            // check first line is not vertical this will cause computation issues later as the slope is aproaching infinity
            if (bahtinovLines.LineAngles[0] < 1.5708 && bahtinovLines.LineAngles[0] > 1.5707)
            {
                //reverse the lines
                float tmp = bahtinovLines.LineAngles[0];
                float tmp2 = bahtinovLines.LineIndex[0];
                bahtinovLines.LineAngles[0] = bahtinovLines.LineAngles[2];
                bahtinovLines.LineIndex[0] = bahtinovLines.LineIndex[2];
                bahtinovLines.LineAngles[2] = tmp;
                bahtinovLines.LineIndex[2] = tmp2;
            }

            // for each Bahtinov angle in this line group draw the line on the image
            for (int index = 0; index < bahtinovLines.LineAngles.Length; ++index)
            {
                // set to the smaller value of the centre point, either x or y
                float CentrePoint = Math.Min(starImage_X_Centre, starImage_Y_Centre);

                // calculate the start and end X/Y point for the line
                float lineStart_X = starImage_X_Centre + -CentrePoint * (float)Math.Cos((double)bahtinovLines.LineAngles[index]) +
                                  (bahtinovLines.LineIndex[index] - starImage_Y_Centre) * (float)Math.Sin((double)bahtinovLines.LineAngles[index]);

                float lineEnd_X = starImage_X_Centre + CentrePoint * (float)Math.Cos((double)bahtinovLines.LineAngles[index]) +
                                  (bahtinovLines.LineIndex[index] - starImage_Y_Centre) * (float)Math.Sin((double)bahtinovLines.LineAngles[index]);

                float lineStart_Y = starImage_Y_Centre + -CentrePoint * (float)Math.Sin((double)bahtinovLines.LineAngles[index]) + (float)-((double)bahtinovLines.LineIndex[index] -
                                  (double)starImage_Y_Centre) * (float)Math.Cos((double)bahtinovLines.LineAngles[index]);

                float lineEnd_Y = starImage_Y_Centre + CentrePoint * (float)Math.Sin((double)bahtinovLines.LineAngles[index]) + (float)-((double)bahtinovLines.LineIndex[index] -
                                  (double)starImage_Y_Centre) * (float)Math.Cos((double)bahtinovLines.LineAngles[index]);

                // if this is the first line
                if (index == 0)
                {
                    float first_lineStart_X = lineStart_X;
                    float first_lineEnd_X = lineEnd_X;
                    float first_lineStart_Y = lineStart_Y;
                    float first_lineEnd_Y = lineEnd_Y;

                    first_lineSlope = (float)(((double)first_lineEnd_Y - (double)first_lineStart_Y) / ((double)first_lineEnd_X - (double)first_lineStart_X));
                    first_calculatedY_for_XStart = (float)(-(double)first_lineStart_X * (((double)first_lineEnd_Y - (double)first_lineStart_Y) /
                                                   ((double)first_lineEnd_X - (double)first_lineStart_X))) + first_lineStart_Y;
                }

                // if this is the middle line
                else if (index == 1)
                {
                    second_lineStart_X = lineStart_X;
                    second_lineEnd_X = lineEnd_X;
                    second_lineStart_Y = lineStart_Y;
                    second_lineEnd_Y = lineEnd_Y;
                }

                // if this is the last line
                else if (index == 2)
                {
                    float third_lineStart_X = lineStart_X;
                    float third_lineEnd_X = lineEnd_X;
                    float third_lineStart_Y = lineStart_Y;
                    float third_lineEnd_Y = lineEnd_Y;

                    third_lineSlope = (float)(((double)third_lineEnd_Y - (double)third_lineStart_Y) / ((double)third_lineEnd_X - (double)third_lineStart_X));
                    third_calculatedY_for_XStart = (float)(-(double)third_lineStart_X * (((double)third_lineEnd_Y -
                                                   (double)third_lineStart_Y) / ((double)third_lineEnd_X - (double)third_lineStart_X))) + third_lineStart_Y;
                }

                // select pen colour for the lines
                switch (group)
                {
                    case 0:
                        dashPen.Color = nightEnable ? Color.Red : Color.Red;
                        largeDashPen.Color = nightEnable ? Color.Red : Color.Red;
                        break;
                    case 1:
                        dashPen.Color = nightEnable ? Color.Red : Color.Green;
                        largeDashPen.Color = nightEnable ? Color.Red : Color.Green;
                        break;
                    case 2:
                        dashPen.Color = nightEnable ? Color.Red : Color.Blue;
                        largeDashPen.Color = nightEnable ? Color.Red : Color.Blue;
                        break;
                }

                // draw line on image
                if (index == 0 || index == 2)
                    graphics.DrawLine(dashPen, lineStart_X, (float)height - lineStart_Y + (float)yOffset, lineEnd_X, (float)height - lineEnd_Y + (float)yOffset);
                else
                    graphics.DrawLine(largeDashPen, lineStart_X, (float)height - lineStart_Y + (float)yOffset, lineEnd_X, (float)height - lineEnd_Y + (float)yOffset);
            }

            // calculate the line intersections

            // this code calculates the x-coordinate of a point of intersection between two lines. line1 and line3
            float x_intersectionOfLines1and3 = (float)(-((double)first_calculatedY_for_XStart - (double)third_calculatedY_for_XStart) / ((double)first_lineSlope - (double)third_lineSlope));

            // this code calculates the y-coordinate of a point of intersection between two lines. line1 and line3
            float y_intersectionOfLines1and3 = first_lineSlope * x_intersectionOfLines1and3 + first_calculatedY_for_XStart;

            // this code calculates the point on line 2 that is perpendicular to the intersection point
            // we start by finding the length along the line 2 where the perpendicular intersects 
            float line2_PerpendicularDistance = (float)(((double)x_intersectionOfLines1and3 - (double)second_lineStart_X) *
                                                ((double)second_lineEnd_X - (double)second_lineStart_X) + ((double)y_intersectionOfLines1and3 -
                                                (double)second_lineStart_Y) * ((double)second_lineEnd_Y - (double)second_lineStart_Y)) /
                                                (float)(((double)second_lineEnd_X - (double)second_lineStart_X) * ((double)second_lineEnd_X -
                                                (double)second_lineStart_X) + ((double)second_lineEnd_Y - (double)second_lineStart_Y) *
                                                ((double)second_lineEnd_Y - (double)second_lineStart_Y));

            // then using this distance calculate the X and Y location on line 2
            float line2_X_Perpendicular = second_lineStart_X + line2_PerpendicularDistance * (second_lineEnd_X - second_lineStart_X);
            float line2_Y_Perpendicular = second_lineStart_Y + line2_PerpendicularDistance * (second_lineEnd_Y - second_lineStart_Y);

            // we then use the line2 perpendicular point and the intersect point and calculate the distance between them 
            float line2ToIntersectDistance = (float)Math.Sqrt(((double)x_intersectionOfLines1and3 - (double)line2_X_Perpendicular) *
                                             ((double)x_intersectionOfLines1and3 - (double)line2_X_Perpendicular) + ((double)y_intersectionOfLines1and3 -
                                             (double)line2_Y_Perpendicular) * ((double)y_intersectionOfLines1and3 - (double)line2_Y_Perpendicular));

            // calculate x and y distances of the error line 
            float X_ErrorDistance = x_intersectionOfLines1and3 - line2_X_Perpendicular;
            float Y_ErrorDistance = y_intersectionOfLines1and3 - line2_Y_Perpendicular;

            // calculate the x and y lengths of line2
            float line2_X_Distance = second_lineEnd_X - second_lineStart_X;
            float line2_Y_Distance = second_lineEnd_Y - second_lineStart_Y;

            float errorSign;

            try
            {
                // determines if the error value is positive or negative
                errorSign = (float)-Math.Sign((float)((double)X_ErrorDistance * (double)line2_Y_Distance - (double)Y_ErrorDistance * (double)line2_X_Distance));
            }
            catch
            {
                lastFocusErrorValue = 0.0f;
                LoadImageLost();
                Reset();
                return;
            }

            // selected groupbox
            System.Windows.Forms.GroupBox selectedGroupBox = groupBoxes[group];

            Label focusError = selectedGroupBox.Controls["FocusErrorLabel" + (group + 1).ToString()] as Label;
            Label absoluteFocusErrorLabel = selectedGroupBox.Controls["AbsoluteFocusErrorLabel" + (group + 1).ToString()] as Label;
            Label withinCriticalFocusLabel = selectedGroupBox.Controls["WithinCriticalFocusLabel" + (group + 1).ToString()] as Label;

            // display Focus Error value
            string str1 = (errorSign * line2ToIntersectDistance).ToString("F1");
            focusError.Text = str1;

            // calculate the bahtinov mask angle in degrees
            float radianPerDegree = (float)Math.PI / 180f;

            float bahtinovAngle = Math.Abs((float)(((double)bahtinovLines.LineAngles[2] - (double)bahtinovLines.LineAngles[0]) / 2.0));

            // calculate focus error in microns
            float pixelsPerMicron = (float)(9.0 / 32.0 * (((double)(float)apertureSetting / 1000.0) /
                                      ((double)((float)focalLengthSetting / 1000) *
                                      (double)(float)pixelSizeSetting)) *
                                      (1.0 + Math.Cos(45.0 * (double)radianPerDegree) *
                                      (1.0 + Math.Tan((double)bahtinovAngle))));

            //           float focusErrorInMicrons = errorSign * line2ToIntersectDistance / pixelsPerMicron;
            float focusErrorInMicrons = line2ToIntersectDistance / pixelsPerMicron;
            string str4 = focusErrorInMicrons.ToString("F1");

            absoluteFocusErrorLabel.Text = str4;

            // display within critical focus values
            float criticalFocusValue = (float)(8.99999974990351E-07 * ((double)((float)focalLengthSetting / 1000) /
                                       (double)((float)apertureSetting / 1000)) * ((double)((float)focalLengthSetting / 1000) /
                                       (double)((float)apertureSetting / 1000)));

            bool withinCriticalFocus = Math.Abs((double)focusErrorInMicrons * 1E-06) < (double)Math.Abs(criticalFocusValue);

            if (withinCriticalFocus)
            {
                withinCriticalFocusLabel.ForeColor = nightEnable ? Color.Black : Color.Green; ;
                withinCriticalFocusLabel.Text = "Yes";
            }
            else
            {
                withinCriticalFocusLabel.ForeColor = nightEnable ? Color.Black : Color.Red;
                withinCriticalFocusLabel.Text = "No";
            }

            // the error marker x,y position
            float errorMarker_X = x_intersectionOfLines1and3 + (float)(((double)line2_X_Perpendicular - (double)x_intersectionOfLines1and3) * ErrorMarkerScalingValue);
            float errorMarker_Y = y_intersectionOfLines1and3 + (float)(((double)line2_Y_Perpendicular - (double)y_intersectionOfLines1and3) * ErrorMarkerScalingValue);

            Pen errorPen;
            if (withinCriticalFocus)
                errorPen = largeSolidPen;
            else
                errorPen = dashPen;

            // select pen colour
            switch (group)
            {
                case 0:
                    errorPen.Color = nightEnable ? Color.Red : Color.Red;
                    break;
                case 1:
                    errorPen.Color = nightEnable ? Color.Red : Color.Green;
                    break;
                case 2:
                    errorPen.Color = nightEnable ? Color.Red : Color.Blue;
                    break;
                default:
                    errorPen.Color = nightEnable ? Color.White : Color.Green;
                    break;
            }

            // draw the circular error marker
            errorPen.Width = (float)penWidth * 2;
            int circleRadius = penWidth * 32;
            graphics.DrawEllipse(errorPen, errorMarker_X - (float)circleRadius, (float)height - errorMarker_Y - (float)circleRadius + (float)yOffset, (float)(circleRadius * 2), (float)(circleRadius * 2));

            // Calculate the line coordinates
            float lineX1 = errorMarker_X + circleRadius;
            float lineY1 = height - errorMarker_Y;
            float lineX2 = errorMarker_X - circleRadius;
            float lineY2 = height - errorMarker_Y;

            // Error circle internal line

            // Translate the line to the origin
            lineX1 -= errorMarker_X;
            lineY1 -= height - errorMarker_Y;
            lineX2 -= errorMarker_X;
            lineY2 -= height - errorMarker_Y;

            // Rotate the line
            float rotatedX1 = (float)(lineX1 * Math.Cos(-bahtinovLines.LineAngles[1]) - lineY1 * Math.Sin(-bahtinovLines.LineAngles[1]));
            float rotatedY1 = (float)(lineX1 * Math.Sin(-bahtinovLines.LineAngles[1]) + lineY1 * Math.Cos(-bahtinovLines.LineAngles[1]));
            float rotatedX2 = (float)(lineX2 * Math.Cos(-bahtinovLines.LineAngles[1]) - lineY2 * Math.Sin(-bahtinovLines.LineAngles[1]));
            float rotatedY2 = (float)(lineX2 * Math.Sin(-bahtinovLines.LineAngles[1]) + lineY2 * Math.Cos(-bahtinovLines.LineAngles[1]));

            // Translate the line back to its original position
            rotatedX1 += errorMarker_X;
            rotatedY1 += height - errorMarker_Y;
            rotatedX2 += errorMarker_X;
            rotatedY2 += height - errorMarker_Y;

            // Draw the rotated error line in the error circle
            graphics.DrawLine(errorPen, rotatedX1, rotatedY1, rotatedX2, rotatedY2);

            float CentrePoint1 = Math.Min(starImage_X_Centre, starImage_Y_Centre);

            float lineStart_X1 = starImage_X_Centre + -CentrePoint1 * (float)Math.Cos((double)bahtinovLines.LineAngles[1]) +
                    (bahtinovLines.LineIndex[1] - starImage_Y_Centre) * (float)Math.Sin((double)bahtinovLines.LineAngles[1]);

            float lineEnd_X1 = starImage_X_Centre + CentrePoint1 * (float)Math.Cos((double)bahtinovLines.LineAngles[1]) +
                                (bahtinovLines.LineIndex[1] - starImage_Y_Centre) * (float)Math.Sin((double)bahtinovLines.LineAngles[1]);

            float lineStart_Y1 = starImage_Y_Centre + -CentrePoint1 * (float)Math.Sin((double)bahtinovLines.LineAngles[1]) + (float)-((double)bahtinovLines.LineIndex[1] -
                                (double)starImage_Y_Centre) * (float)Math.Cos((double)bahtinovLines.LineAngles[1]);

            float lineEnd_Y1 = starImage_Y_Centre + CentrePoint1 * (float)Math.Sin((double)bahtinovLines.LineAngles[1]) + (float)-((double)bahtinovLines.LineIndex[1] -
                                (double)starImage_Y_Centre) * (float)Math.Cos((double)bahtinovLines.LineAngles[1]);

            // display the error value in the pictureBox
            Font font2 = new Font("Arial", 16f);
            SolidBrush solidBrush = new SolidBrush(Color.White);

            Point textStartPoint = new Point((int)lineStart_X1, (int)((float)height - lineStart_Y1 + (float)yOffset));
            Point textEndPoint = new Point((int)lineEnd_X1, (int)((float)height - lineEnd_Y1 + (float)yOffset));

            if (textStartPoint.X > pictureBox.Width - 50 || textStartPoint.Y > pictureBox.Height - 50)
            {
                textStartPoint.X = Math.Min(textStartPoint.X, pictureBox.Width - 50);
                textStartPoint.Y = Math.Min(textStartPoint.Y, pictureBox.Height - 50);
            }

            if (textEndPoint.X > pictureBox.Width - 50 || textEndPoint.Y > pictureBox.Height - 50)
            {
                textEndPoint.X = Math.Min(textEndPoint.X, pictureBox.Width - 50);
                textEndPoint.Y = Math.Min(textEndPoint.Y, pictureBox.Height - 50);
            }

            Point textLocation = group == 1 ? textEndPoint : textStartPoint;
            graphics.DrawString((errorSign * line2ToIntersectDistance).ToString("F1"), font2, (Brush)solidBrush, (PointF)textLocation);

            // check if the images has moved substantially indicating the image has been lost
            if (lastFocusErrorValue != 0.0f && Math.Abs(lastFocusErrorValue - x_intersectionOfLines1and3) > 100)
            {
                lastFocusErrorValue = 0.0f;
                LoadImageLost();
                Reset();
                return;
            }
            lastFocusErrorValue = x_intersectionOfLines1and3;

            // centre the image around the intersection of line group 0
            if (group == 0)
            {
                // recentre image using the X,Y location of the intersection
                Rectangle imageArea = screenCapture.GetImageCoordinates();

                imageArea.X -= (int)((((double)(starImage.Width / 2) - ((double)x_intersectionOfLines1and3 + (double)line2_X_Perpendicular) / 2.0)));
                imageArea.Y += (int)((((double)(starImage.Height / 2) - ((double)y_intersectionOfLines1and3 + (double)line2_Y_Perpendicular) / 2.0)));

                screenCapture.SetImageCoordinates(imageArea);
            }
            return;
        }

        private void LoadImageLost()
        {
            Bitmap imageLost = new Bitmap(Properties.Resources.imageLost);
            pictureBox.Image = imageLost;
        }

        private void QuitToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MenuStrip1_MouseEnter(object sender, EventArgs e)
        {
            Activate();
        }

        private void CapturedImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentPicture != null)
            {
                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "Bitmap Image|*.bmp";
                    saveDialog.Title = "Save Bitmap Image";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = saveDialog.FileName;
                        currentPicture.Save(filePath, System.Drawing.Imaging.ImageFormat.Bmp);
                    }
                }
            }
            else
            {
                MessageBox.Show("No image available", "Warning",
                                 MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void AnnotatedImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox.Image != null)
            {
                Bitmap temp = new Bitmap(pictureBox.Image);

                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "Bitmap Image|*.bmp";
                    saveDialog.Title = "Save Bitmap Image";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = saveDialog.FileName;
                        temp.Save(filePath, System.Drawing.Imaging.ImageFormat.Bmp);
                    }
                }
            }
            else
            {
                MessageBox.Show("No image available", "Warning",
                                 MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SettingsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // Settings
            DialogResult result = settingsDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                LoadSettings();
                forceUpdate = true;
            }
        }

        private void MousePositionTimer_Tick(object sender, EventArgs e)
        {
            if (this.ContainsFocus && !menuActive)
            {
                Point mousePosition = PointToClient(Control.MousePosition);

                bool isRedFocused = RedGroupBoxScreenBounds.Contains(mousePosition);
                bool isGreenFocused = GreenGroupBoxScreenBounds.Contains(mousePosition);
                bool isBlueFocused = BlueGroupBoxScreenBounds.Contains(mousePosition);

                RedGroupBox.BackColor = isRedFocused ? Color.LightGray : SystemColors.Control;
                GreenGroupBox.BackColor = isGreenFocused ? Color.LightGray : SystemColors.Control;
                BlueGroupBox.BackColor = isBlueFocused ? Color.LightGray : SystemColors.Control;

                showRedFocus = isRedFocused || !isRedFocused && !isGreenFocused && !isBlueFocused;
                showGreenFocus = isGreenFocused || !isRedFocused && !isGreenFocused && !isBlueFocused;
                showBlueFocus = isBlueFocused || !isRedFocused && !isGreenFocused && !isBlueFocused;
                forceUpdate = true;
            }
        }

        private void MenuStrip1_MenuActivate(object sender, EventArgs e)
        {
            menuActive = true;
        }

        private void MenuStrip1_MenuDeactivate(object sender, EventArgs e)
        {
            menuActive = false;
        }

        private void AboutToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();

        }

        private void DonateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Donate donate = new Donate();
            donate.ShowDialog();
        }

        private void HelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Not yet implemented");
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstallUpdateSyncWithInfo();
        }

        private void InstallUpdateSyncWithInfo()
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
    }
}



