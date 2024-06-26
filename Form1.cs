﻿// Sections of the following code are covered by and existing copyright
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

using Bahtinov_Collimator.CSheet;
using Bahtinov_Collimator.Sound;
using Microsoft.Win32;
using System;
using System.Deployment.Application;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

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
        private bool[] showFocus = new bool[3];
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
        private float[] errorValues = new float[3];
        private int imageCount;
        private CSheet.CheatSheet cheatSheet;
        private Point currentCentre;
        private bool lastRedFocus = false;

        private Timer soundTimer;
        private int lastSoundOut = 0;
        private SoundControl soundOut;

        private bool redSoundPlayed = false;
        private bool greenSoundPlayed = false;
        private bool blueSoundPlayed = false;

        // settings
        private readonly Settings settingsDialog;
        private int apertureSetting;
        private int focalLengthSetting;
        private double pixelSizeSetting;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool SetProcessDPIAware();

        public Form1()
        {
            SetProcessDPIAware();
            InitializeComponent();

            screenCapture = new ScreenCap();
            settingsDialog = new Settings();

            UpdateTimer.Interval = updateinterval;

            mousePositionTimer = new Timer() { Interval = 100 };
            mousePositionTimer.Tick += MousePositionTimer_Tick;
            mousePositionTimer.Start();

            // sound setup
            soundOut = new SoundControl();
            soundTimer = new Timer();
            soundTimer.Interval = 2000; // 2000 milliseconds
            soundTimer.Tick += SoundTimer_Tick;
            
            RedGroupBoxScreenBounds = RedGroupBox.Bounds;
            GreenGroupBoxScreenBounds = GreenGroupBox.Bounds;
            BlueGroupBoxScreenBounds = BlueGroupBox.Bounds; 

            LoadSettings();

            // turn off Green and Blue GroupBoxes
            GreenGroupBox.Visible = false;
            BlueGroupBox.Visible = false;

            cheatSheetToolStripMenuItem.Visible = false;

            // set red Group Box to Black text
            RedGroupBox.ForeColor = Color.Black;

            groupBoxes = new System.Windows.Forms.GroupBox[] { RedGroupBox, GreenGroupBox, BlueGroupBox };

            // initialize showFocus to true
            showFocus = Enumerable.Repeat(true, showFocus.Length).ToArray();
            imageCount = 0;

            // rescale form for different windows scaling values
            float scalingFactor = ScreenCap.GetScalingFactor();

            int newHeight = pictureBox.Location.Y + pictureBox.Height + (int)(50 * scalingFactor);
            int newWidth = pictureBox.Location.X + pictureBox.Width + (int)(25 * scalingFactor);
            this.Size = new System.Drawing.Size(newWidth, newHeight);

            SetWindowTitleWithVersion();
        }

        private void LoadSettings()
        {
            // load settings
            apertureSetting = Properties.Settings.Default.Aperture;
            focalLengthSetting = Properties.Settings.Default.FocalLength;
            pixelSizeSetting = Properties.Settings.Default.PixelSize;

            if (Properties.Settings.Default.VoiceEnabled)
            {
                SoundOnOff(true);
            }
            else
                SoundOnOff(false);
        }

        private void Reset()
        {
            UpdateTimer.Stop();
            LoadSettings();
            imageCount = 0;
            bufferedPicture = null;
            screenCaptureRunningFlag = false;
            cheatSheetToolStripMenuItem.Visible = false;
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

            // count accumulating images
            imageCount++;

            paddedImage = new Bitmap(ImageProcessing.PadAndCenterCircularBitmap(currentPicture, pictureBox));

            if (bufferedPicture == null)
            {
                lineData = ImageProcessing.FindBrightestLines(paddedImage, lineCount);
                lineData.Sort();
            }
            
            lineData = ImageProcessing.FindSubpixelLines(paddedImage, lineData.LineValue.Length, lineData);
            bufferedPicture = new Bitmap(currentPicture);

            if (lineData.LineAngles.Length > 3)
            {
                GreenGroupBox.Visible = true;
                BlueGroupBox.Visible = true;
                RedGroupBox.ForeColor = nightEnable ? Color.Black : Color.Red;
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
                cheatSheetToolStripMenuItem.Visible = true;

                Rectangle rect = new Rectangle(0, 0, paddedImage.Width, paddedImage.Height);
                BahtinovData data = new BahtinovData(3, rect);

                for (int i = 0; i < 3; i++)
                {
                    int startIndex = 3 * i;
                    Array.Copy(lineData.LineAngles, startIndex, data.LineAngles, 0, 3);
                    Array.Copy(lineData.LineIndex, startIndex, data.LineIndex, 0, 3);

                    if (!DisplayLines(data, paddedImage, i, showFocus[1]))
                        break;
                }
            }
            else
            {
                if (lineData.LineAngles.Length == 3)
                {
                    DisplayLines(lineData, paddedImage, 0, true);
                    cheatSheetToolStripMenuItem.Visible = false;
                }
                else
                {
                    Reset();
                    MessageBox.Show("Unable to detect Bahtinov image lines", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }

            // re centre the image if the star has moved
            currentCentre = RecentreImage(currentPicture);

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

                soundTimer.Start();
                imageCount = 0;
                lastSoundOut = 0;
                screenCaptureRunningFlag = true;
                UpdateTimer.Start();
                StartButton.Text = "Stop";
            }
            else
            {
                soundTimer.Stop();
                soundOut.StopAndFlush();
                UpdateTimer.Stop();
                cheatSheetToolStripMenuItem.Visible = false;
                screenCaptureRunningFlag = false;
                StartButton.Text = "Start";
                pictureBox.Image = null;
            }
        }

        private bool DisplayLines(BahtinovData bahtinovLines, Bitmap starImage, int group, bool calculateOnly)
        {
            int width = starImage.Width;
            int height = starImage.Height;

            // check for stray LineIndex values
            // this can happen when the image moves quickly or is occulted by another window
            foreach (float lineIndex in bahtinovLines.LineIndex)
            {
                // Check if x-coordinate is outside the image bounds
                if (lineIndex < 0 || lineIndex >= height)
                {
                    return false; // Line angle is outside x-axis range
                }
            }

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

            int penWidth = 2;
            Pen dashPen = new Pen(Color.Yellow, (float)penWidth) { DashStyle = DashStyle.Dash };

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
                if (showFocus[group] == true)
                {
                    if (index == 0 || index == 2)
                        graphics.DrawLine(dashPen, lineStart_X, (float)height - lineStart_Y + (float)yOffset, lineEnd_X, (float)height - lineEnd_Y + (float)yOffset);
                    else
                        graphics.DrawLine(largeDashPen, lineStart_X, (float)height - lineStart_Y + (float)yOffset, lineEnd_X, (float)height - lineEnd_Y + (float)yOffset);
                }
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
                return false;
            }

            // selected groupbox
            System.Windows.Forms.GroupBox selectedGroupBox = groupBoxes[group];

            Label focusError = selectedGroupBox.Controls["FocusErrorLabel" + (group + 1).ToString()] as Label;
            Label absoluteFocusErrorLabel = selectedGroupBox.Controls["AbsoluteFocusErrorLabel" + (group + 1).ToString()] as Label;
            Label withinCriticalFocusLabel = selectedGroupBox.Controls["WithinCriticalFocusLabel" + (group + 1).ToString()] as Label;

            // display Focus Error value
            string str1 = (errorSign * line2ToIntersectDistance).ToString("F1");
            focusError.Text = str1;

            // store error values for Cheat Sheet display
            errorValues[group] = errorSign * line2ToIntersectDistance;

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
                    errorPen.Color = nightEnable ? Color.Green : Color.Green;
                    break;
                case 2:
                    errorPen.Color = nightEnable ? Color.Blue : Color.Blue;
                    break;
                default:
                    errorPen.Color = nightEnable ? Color.White : Color.White;
                    break;
            }

            // draw the circular error marker
            errorPen.Width = 5;
            int circleRadius = 64;

            if (showFocus[group] == true)
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
            if (showFocus[group] == true)
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

            if (showFocus[group] == true)
                graphics.DrawString((errorSign * line2ToIntersectDistance).ToString("F1"), font2, (Brush)solidBrush, (PointF)textLocation);

            // check if the images has moved substantially indicating the image has been lost
            if (lastFocusErrorValue != 0.0f && Math.Abs(lastFocusErrorValue - x_intersectionOfLines1and3) > 200)
            {
                lastFocusErrorValue = 0.0f;
                return false;
            }
            lastFocusErrorValue = x_intersectionOfLines1and3;

            return true;
        }

        private Point RecentreImage(Bitmap image)
        {
            // centre the image around the intersection of line group 0
            // recentre image using the X,Y location of the intersection
            Rectangle imageArea = screenCapture.GetImageCoordinates();

            Point boxPoint = FindBrightestAreaCenter(image, 5);
            int delta_X = (int)(image.Width / 2.0) - boxPoint.X;
            int delta_Y = (int)(image.Height / 2.0) - boxPoint.Y;

            if (delta_X > 10 || delta_Y > 10 || delta_X < -10 || delta_Y < -10)
            {

                imageArea.X -= delta_X;
                imageArea.Y -= delta_Y;

                screenCapture.SetImageCoordinates(imageArea);
            }

            return boxPoint;
        }

        public Point FindBrightestAreaCenter(Bitmap bitmap, int blockSize = 10)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            int bestX = 0, bestY = 0;
            float maxIntensity = float.MinValue;

            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            int stride = bitmapData.Stride;
            IntPtr scan0 = bitmapData.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)scan0;

                for (int y = 0; y < height; y += blockSize)
                {
                    for (int x = 0; x < width; x += blockSize)
                    {
                        float intensity = CalculateBlockIntensity(p, x, y, blockSize, width, height, stride);
                        if (intensity > maxIntensity)
                        {
                            maxIntensity = intensity;
                            bestX = x;
                            bestY = y;
                        }
                    }
                }
            }

            bitmap.UnlockBits(bitmapData);

            // Calculate the center point of the brightest block
            int centerX = bestX + blockSize / 2;
            int centerY = bestY + blockSize / 2;

            return new Point(centerX, centerY);
        }

        private static unsafe float CalculateBlockIntensity(byte* p, int startX, int startY, int blockSize, int width, int height, int stride)
        {
            float totalIntensity = 0;
            int count = 0;

            for (int y = startY; y < startY + blockSize && y < height; y++)
            {
                for (int x = startX; x < startX + blockSize && x < width; x++)
                {
                    byte* pixel = p + y * stride + x * 3;
                    float intensity = (pixel[2] + pixel[1] + pixel[0]) / 3.0f; // RGB order
                    totalIntensity += intensity;
                    count++;
                }
            }

            return totalIntensity / count;
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
            if (this.ContainsFocus && !menuActive && lineData != null)
            {
                Point mousePosition = PointToClient(Control.MousePosition);

                bool isRedFocused = RedGroupBoxScreenBounds.Contains(mousePosition);
                bool isGreenFocused = GreenGroupBoxScreenBounds.Contains(mousePosition);
                bool isBlueFocused = BlueGroupBoxScreenBounds.Contains(mousePosition);

                // tri-bahtinov mask
                if (lineData.LineAngles.Length != 3)
                {
                    // Store the previous showFocus states
                    bool prevRedFocus = showFocus[0];
                    bool prevGreenFocus = showFocus[1];
                    bool prevBlueFocus = showFocus[2];

                    RedGroupBox.BackColor = isRedFocused ? Color.LightGray : SystemColors.Control;
                    GreenGroupBox.BackColor = isGreenFocused ? Color.LightGray : SystemColors.Control;
                    BlueGroupBox.BackColor = isBlueFocused ? Color.LightGray : SystemColors.Control;

                    // this is a tri-bahtinov mask
                    showFocus[0] = isRedFocused || (!isRedFocused && !isGreenFocused && !isBlueFocused);
                    showFocus[1] = isGreenFocused || (!isRedFocused && !isGreenFocused && !isBlueFocused);
                    showFocus[2] = isBlueFocused || (!isRedFocused && !isGreenFocused && !isBlueFocused);

                    // Check if the state of showFocus has changed for any group box
                    if ((showFocus[0] != prevRedFocus) || (showFocus[1] != prevGreenFocus) || (showFocus[2] != prevBlueFocus))
                        forceUpdate = true;
                }
                // bahtinov mask
                else
                {
                    RedGroupBox.BackColor = isRedFocused ? Color.LightGray : SystemColors.Control;
                    showFocus[0] = true;

                    if (isRedFocused && !lastRedFocus)
                        forceUpdate = true;

                    lastRedFocus = isRedFocused;
                }
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

        private void cheatSheetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (cheatSheet == null || cheatSheet.IsDisposed)
            {
                cheatSheet = new CSheet.CheatSheet(this);
                cheatSheet.errorValues(errorValues);
                cheatSheet.Show(this);
                cheatSheet.FormClosed += CheatSheet_FormClosed;
            }
        }

        private void CheatSheet_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Set cheatSheet instance to null when the form is closed
            cheatSheet = null;
        }

        public float[] getErrorValues()
        {
            return errorValues;
        }

        public bool isScreenCaptureRunning()
        {
            return screenCaptureRunningFlag;
        }

        public int getImageCount()
        {
            return imageCount;
        }

        private void SoundOnOff(bool enable)
        {
            if (enable)
            {
                soundOut.Play("voice enabled");
                soundTimer.Start();
            }
            else
            {
                soundOut.StopAndFlush();
                soundTimer.Stop();
            }
        }

        private void SoundTimer_Tick(object sender, EventArgs e)
        {
            if (imageCount > lastSoundOut)
            {
                Point mousePosition = PointToClient(Control.MousePosition);
                bool tribatMask = lineData.LineAngles.Length > 3;

                if (RedGroupBoxScreenBounds.Contains(mousePosition))
                {
                    if (!redSoundPlayed)
                    {
                        soundOut.Play("Red");
                        redSoundPlayed = true;
                        greenSoundPlayed = false; // Reset other flags to allow re-triggering if needed
                        blueSoundPlayed = false;
                    }
                    soundOut.Play(errorValues[0].ToString("F1"));
                }
                else if (GreenGroupBoxScreenBounds.Contains(mousePosition) && tribatMask)
                {
                    if (!greenSoundPlayed)
                    {
                        soundOut.Play("Green");
                        greenSoundPlayed = true;
                        redSoundPlayed = false; // Reset other flags to allow re-triggering if needed
                        blueSoundPlayed = false;
                    }
                    soundOut.Play(errorValues[1].ToString("F1"));
                }
                else if (BlueGroupBoxScreenBounds.Contains(mousePosition) && tribatMask)
                {
                    if (!blueSoundPlayed)
                    {
                        soundOut.Play("Blue");
                        blueSoundPlayed = true;
                        redSoundPlayed = false; // Reset other flags to allow re-triggering if needed
                        greenSoundPlayed = false;
                    }
                    soundOut.Play(errorValues[2].ToString("F1"));
                }

                lastSoundOut = imageCount;
            }
        }

        private void Form1_MouseEnter(object sender, EventArgs e)
        {
            this.Activate();
        }

        private void SetWindowTitleWithVersion()
        {
            // Get the version number of the assembly
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            // Create the version string
            string versionString = $"V {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            // Set the form's title
            this.Text = $"Bahtinov Collimator - {versionString}";
        }
    }
}
