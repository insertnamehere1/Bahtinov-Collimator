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

using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bahtinov_Collimator.AdjustAssistant;
using Bahtinov_Collimator.Voice;
using Bahtinov_Collimator.Image_Processing;
using System.Drawing.Text;
using Bahtinov_Collimator.Custom_Components;
using Bahtinov_Collimator.Helper;
using System.Text;

namespace Bahtinov_Collimator
{
    public partial class Form1 : Form
    {
        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        // Constants
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 19;

        // Focus Group Boxes
        FocusChannelComponent groupBoxRed;
        FocusChannelComponent groupBoxGreen;
        FocusChannelComponent groupBoxBlue;

        // new image capture, first pass
        private bool firstPassCompleted = false;

        // image Processing Class
        private BahtinovProcessing bahtinovProcessing;
        private DefocusStarProcessing defocusStarProcessing;

        // Bahtinove line data
        private BahtinovData bahtinovLineData;

        // Adjust Assistant
        private AdjustAssist adjustAssistDialog;

        // Voice Generation
        private VoiceControl voiceControl;

        private int imageType = 0;  // default - no image type, 1-Bahtinov, 2-Defocus

        public Form1()
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            float scaleX = this.DeviceDpi / this.AutoScaleDimensions.Width;
            float scaleY = this.DeviceDpi / this.AutoScaleDimensions.Height;
            UITheme.Initialize(scaleX, scaleY);

            InitializeComponent();
            InitializeRedFocusBox();
            SetFormUI();
            SetColorScheme();
            SetEvents();

            bahtinovProcessing = new BahtinovProcessing();
            defocusStarProcessing = new DefocusStarProcessing();
            menuStrip1.Renderer = new CustomToolStripRenderer();
            voiceControl = new VoiceControl();

            slideSwitch2.IsOn = Properties.Settings.Default.DefocusSwitch;
        }

        private void InitializeRedFocusBox()
        {
            if (groupBoxRed != null)
            {
                RemoveAndDisposeControls(groupBoxRed);
            }

            groupBoxRed = new FocusChannelComponent(0)
            {
                Location = ScalePoint(new Point(15, 30))
            };
            this.Controls.Add(groupBoxRed);
        }

        private void InitializeGreenFocusBox()
        {
            groupBoxGreen = new FocusChannelComponent(1)
            {
                Location = ScalePoint(new Point(15, 125))
            };
            this.Controls.Add(groupBoxGreen);
        }

        private void InitializeBlueFocusBox()
        {
            groupBoxBlue = new FocusChannelComponent(2)
            {
                Location = ScalePoint(new Point(15, 221))
            };
            this.Controls.Add(groupBoxBlue);
        }

        private Point ScalePoint(Point originalPoint)
        {
            // Get the scaling factor based on the form's AutoScaleFactor
            float scaleX = this.DeviceDpi / this.AutoScaleDimensions.Width;
            float scaleY = this.DeviceDpi / this.AutoScaleDimensions.Height;

            // Scale the original point by the scale factors
            return new Point((int)(originalPoint.X / scaleX), (int)(originalPoint.Y / scaleY));
        }

        private void SetFormUI()
        {
            AdjustAssistToolStripMenuItem.Visible = false;
            SetWindowTitleWithVersion();
        }

        private void SetEvents()
        {
            ImageCapture.ImageReceivedEvent += OnImageReceived;
            ImageLostEventProvider.ImageLostEvent += HandleImageLost;
            slideSwitch2.ToggleChanged += SlideSwitchChanged;
        }

        private void SetColorScheme()
        {
            // main form
            this.ForeColor = UITheme.DarkForeground;
            this.BackColor = UITheme.DarkBackground;

            // menu strip
            menuStrip1.BackColor = UITheme.DarkBackground;
            menuStrip1.ForeColor = UITheme.MenuStripForeground;
            SetMenuItemsColor(menuStrip1.Items, UITheme.MenuDarkBackground, UITheme.MenuDarkForeground);

            // start button
            StartButton.BackColor = UITheme.ButtonDarkBackground;
            StartButton.ForeColor = UITheme.ButtonDarkForeground;
            StartButton.FlatStyle = FlatStyle.Popup;

            // Titlebar
            var color = UITheme.DarkBackground;
            int colorValue = color.R | (color.G << 8) | (color.B << 16);
            DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref colorValue, sizeof(int));

            // Labels
            label1.ForeColor = UITheme.MenuStripForeground;
            Label2.ForeColor = UITheme.MenuStripForeground;

        }

        private void SetMenuItemsColor(ToolStripItemCollection items, Color backColor, Color foreColor)
        {
            foreach (ToolStripMenuItem item in items)
            {
                item.BackColor = backColor;
                item.ForeColor = foreColor;
                if (item.HasDropDownItems)
                {
                    SetMenuItemsColor(item.DropDownItems, backColor, foreColor);
                }
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            ImageCapture.StopImageCapture();
            imageDisplayComponent1.ClearDisplay();
            bahtinovProcessing.StopImageProcessing();
            RemoveAndDisposeControls(groupBoxGreen, groupBoxBlue);

            if (ImageCapture.SelectStar())
            {
                if(Properties.Settings.Default.DefocusSwitch)
                    ImageCapture.TrackingType = 2;
                else
                    ImageCapture.TrackingType = 1;

                firstPassCompleted = false;
                ImageCapture.StartImageCapture();
            }
        }

        private void OnImageReceived(object sender, ImageReceivedEventArgs e)
        {
            // Run the processing on a background thread
            Task.Run(() => ProcessAndDisplay(e.Image));
        }

        private void ProcessAndDisplay(Bitmap original)
        {
            Bitmap image = new Bitmap(original);

            // is this a defocused star?
            if (!firstPassCompleted)
            {
                if(Properties.Settings.Default.DefocusSwitch == true)
                {
                    // this is a defocus star
                    imageType = 2;
                }
                else
                {   // this is a bahtinov image
                    imageType = 1;
                }
           }

            if (imageType == 1)
                RunBahtinovDisplay(image);
            else
                if (imageType == 2)
                RunDefocusStarDisplay(image);
        }


        private void RunDefocusStarDisplay(Bitmap image)
        {
            defocusStarProcessing.DisplayDefocusImage(image);
        }

        private void RunBahtinovDisplay(Bitmap image)
        {
            try
            {
                // Find the Bahtinov lines
                if (!firstPassCompleted)
                {
                    bahtinovLineData = bahtinovProcessing.FindBrightestLines(image);
                    bahtinovLineData.Sort();
                }

                // Second pass to find the fine details
                bahtinovLineData = bahtinovProcessing.FindSubpixelLines(image, bahtinovLineData.LineValue.Length, bahtinovLineData);

                // Update UI based on the number of lines detected
                int numberOfLines = bahtinovLineData.LineAngles.Length;

                if (!firstPassCompleted)
                {
                    UpdateFocusGroup(numberOfLines);
                    firstPassCompleted = true;
                }
            }
            catch(Exception e)
            {
                ImageCapture.StopImageCapture();
                bahtinovProcessing.StopImageProcessing();
                firstPassCompleted = false;
                DarkMessageBox.Show("Failed Bahtinov line detection", "RunBahtinovDisplay", MessageBoxIcon.Error, MessageBoxButtons.OK, this);
                imageDisplayComponent1.ClearDisplay();
            }

            bahtinovProcessing.DisplayLines(bahtinovLineData, image);
        }

        private void ShowWarningMessage(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ShowWarningMessage(message)));
            }
            else
            {
                ImageCapture.StopImageCapture();
                firstPassCompleted = false;
                MessageBox.Show(message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void UpdateFocusGroup(int numberOfLines)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateFocusGroup(numberOfLines)));
            }
            else
            {
                RemoveAndDisposeControls(groupBoxGreen, groupBoxBlue);

                if (numberOfLines == 9)
                {
                    InitializeRedFocusBox();
                    InitializeGreenFocusBox();
                    InitializeBlueFocusBox();
                    AdjustAssistToolStripMenuItem.Visible = true;
                }
                else
                {
                    AdjustAssistToolStripMenuItem.Visible = false;
                }

                firstPassCompleted = true;
            }
        }

        private void SlideSwitchChanged(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                // Marshal the call to the UI thread
                this.Invoke(new Action<object, EventArgs>(SlideSwitchChanged), sender, e);
                return;
            }

            SlideSwitch switchControl = sender as SlideSwitch;
            bool toggle = ((SlideSwitch)sender).IsOn;

            Properties.Settings.Default.DefocusSwitch = toggle;
            Properties.Settings.Default.Save();

            ImageCapture.StopImageCapture();
            imageDisplayComponent1.ClearDisplay();
            bahtinovProcessing.StopImageProcessing();

            if (toggle)
                RemoveAndDisposeControls(groupBoxGreen, groupBoxBlue, groupBoxRed);
            else
            {
                RemoveAndDisposeControls(groupBoxGreen, groupBoxBlue);
                InitializeRedFocusBox();
            }
        }

        private void HandleImageLost(object sender, ImageLostEventArgs e)
        {
            ImageCapture.StopImageCapture();
            bahtinovProcessing.StopImageProcessing();
            firstPassCompleted = false;

            DarkMessageBox.Show(e.Message, e.Title, e.Icon, e.Button, this);
        }

        private void QuitToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MenuStrip1_MouseEnter(object sender, EventArgs e)
        {
            Activate();
        }

        private void AboutToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            PositionDialogInsideMainWindow(aboutBox);
            aboutBox.ShowDialog();
        }

        private void DonateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Donate donate = new Donate();
            PositionDialogInsideMainWindow(donate);
            donate.ShowDialog();
        }

        private void HelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DarkMessageBox.Show("Not yet implemented", "Help", MessageBoxIcon.Asterisk, MessageBoxButtons.OK, this);
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstallUpdateSyncWithInfo();
        }

        private void InstallUpdateSyncWithInfo()
        {
            Helper.Helper.GetLatestUpdate();
        }

        private void cheatSheetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (adjustAssistDialog == null || adjustAssistDialog.IsDisposed)
            {
                adjustAssistDialog = new AdjustAssistant.AdjustAssist(this);
                PositionDialogInsideMainWindow(adjustAssistDialog);
                adjustAssistDialog.Show(this);
                adjustAssistDialog.FormClosed += CheatSheet_FormClosed;
            }
        }

        private void CheatSheet_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Set adjustAssistDialog instance to null when the form is closed
            adjustAssistDialog = null;
        }

        private void RemoveAndDisposeControls(params Control[] controls)
        {
            foreach (var control in controls)
            {
                if (Controls.Contains(control))
                {
                    Controls.Remove(control);
                    control.Dispose();
                }
            }
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

        private void SettingsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // Settings Dialog
            Settings settingsDialog = new Settings();
            PositionDialogInsideMainWindow(settingsDialog);

            DialogResult result = settingsDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                bahtinovProcessing.LoadSettings();
                voiceControl.LoadSettings();

            }
        }

        private void Form1_MouseEnter(object sender, EventArgs e)
        {
            this.Activate();
        }

        private void PositionDialogInsideMainWindow(Form dialog)
        {
            // Get the main window's rectangle
            Rectangle mainWindowRect = this.Bounds;

            // Calculate center position for the dialog within the main window
            int x = mainWindowRect.Left + (mainWindowRect.Width - dialog.Width) / 2;
            int y = mainWindowRect.Top + (mainWindowRect.Height - dialog.Height) / 2;

            // Make sure the dialog is fully within the main window
            x = Math.Max(mainWindowRect.Left, x);
            y = Math.Max(mainWindowRect.Top, y);

            dialog.StartPosition = FormStartPosition.Manual;
            dialog.Location = new Point(x, y);
        }


        // Custom renderer to change the color of selected items
        private class CustomToolStripRenderer : ToolStripProfessionalRenderer
        {
            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                // Handle the background rendering for the menu items
                if (e.Item.Selected || e.Item.Pressed)
                {
                    SolidBrush menuBackgroundBrush = new SolidBrush(UITheme.MenuHighlightBackground);

                    e.Graphics.FillRectangle(menuBackgroundBrush, e.Item.ContentRectangle);
                    e.Item.ForeColor = UITheme.MenuDarkForeground;
                }
                else
                {
                    base.OnRenderMenuItemBackground(e);
                }
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = UITheme.MenuDarkForeground;
                base.OnRenderItemText(e);
            }
        }

    }
}