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
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISINGtrst FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using Bahtinov_Collimator.Custom_Components;
using Bahtinov_Collimator.Image_Processing;
using Bahtinov_Collimator.Voice;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Bahtinov_Collimator.BahtinovLineDataEventArgs;

namespace Bahtinov_Collimator
{
    public partial class Form1 : Form
    {
        #region External Function

        /// <summary>
        /// Sets an attribute for a window using the Desktop Window Manager (DWM) API.
        /// </summary>
        /// <returns>An integer value indicating the result of the operation. A non-zero value indicates failure, while zero indicates success.</returns>
        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        #endregion

        #region Constants

        /// <summary>
        /// Specifies the attribute for enabling or disabling immersive dark mode for a window using the Desktop Window Manager (DWM) API.
        /// </summary>
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 19;

        /// <summary>
        /// Maximum focus channel width (96 DPI logical px) when Tri-Bahtinov + calibrated (widened layout).
        /// </summary>
        private const int MAX_FOCUS_CHAN_SIZE = 270;

        /// <summary>
        /// Default focus channel width (96 DPI logical px). Must match <see cref="Form1"/> designer
        /// (<c>groupBoxRed</c> / <c>analysisGroupBox</c> at 185). Used when restoring after capture stops.
        /// </summary>
        private const int DEFAULT_FOCUS_CHAN_WIDTH_AT96 = 185;

        /// <summary>
        /// Horizontal gap (96 DPI logical px) between the focus column and the image display.
        /// Matches designer: image at X=202, red group at (9,23) with width 185 → 9+185+8=202.
        /// </summary>
        private const int ImageGapAfterFocusColumnAt96 = 8;

        /// <summary>
        /// Right margin (96 DPI logical px) after the image display inside the client area.
        /// Designer: ClientSize 659, image right 652 → 7px.
        /// </summary>
        private const int RightMarginAfterImageAt96 = 7;

        /// <summary>
        /// Gap (96 DPI) between image display and calibration panel when calibration is open.
        /// </summary>
        private const int GapImageToCalibrationAt96 = 8;

        /// Sets testing mode for Calibration 
        private const bool TEST_MODE = true;

        #endregion

        #region Private Fields

        // Calibration User Component
        /// <summary>
        /// Allows for calibration of the focus direction.
        /// </summary>
        private CalibrationComponent calibrationComponent;

        // New image capture, first pass
        /// <summary>
        /// Indicates whether the first pass of image capture has been completed.
        /// </summary>
        private bool firstPassCompleted = false;

        // Image Processing Classes
        /// <summary>
        /// Handles the processing of Bahtinov mask images for focus adjustment.
        /// </summary>
        private readonly BahtinovProcessing bahtinovProcessing;

        /// <summary>
        /// Handles the processing of defocus star images for focus adjustment.
        /// </summary>
        private readonly DefocusStarProcessing defocusStarProcessing;

        // Bahtinov line data
        /// <summary>
        /// Contains the data related to the lines in Bahtinov mask images.
        /// </summary>
        private BahtinovData bahtinovLineData;

        // Voice Generation
        /// <summary>
        /// Manages the voice control for audio notifications and speech synthesis.
        /// </summary>
        private readonly VoiceControl voiceControl;

        // Image Type
        /// <summary>
        /// Specifies the type of image currently being processed:
        /// 0 - No image type,
        /// 1 - Bahtinov,
        /// 2 - Defocus.
        /// </summary>
        private int imageType = 0;

        // Screen Capture Flag
        /// <summary>
        /// Indicates whether the screen capture process is currently running.
        /// </summary>
        private bool screenCaptureRunningFlag = false;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Form1"/> class.
        /// </summary>
        public Form1()
        {
            UITheme.DeviceDpi = DeviceDpi;

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();

            // Initialize the form's components.
            InitializeComponent();
            imageDisplayComponent1.ClearDisplay(); 
            RestoreWindowPosition();
            ApplyLocalization();

            // Initialize the red focus channel component.
            InitializeRedFocusBox();

            // Set up the user interface of the form.
            SetFormUI();

            // Apply the color scheme to the form.
            SetColorScheme();

            // Set up event handlers for the form's controls.
            SetEvents();

            // disable and gray the "what should i do next" menu item if we havent been calibrated
            menuStrip1.Items[4].Enabled = Properties.Settings.Default.CalibrationCompleted;

            // Create instances of the image processing classes.
            bahtinovProcessing = new BahtinovProcessing();
            defocusStarProcessing = new DefocusStarProcessing();

            // Set a custom renderer for the menu strip.
            menuStrip1.Renderer = new CustomToolStripRenderer();

            // Initialize the voice control component for audio notifications.
            voiceControl = new VoiceControl();

            // Set the initial state of the defocus switch based on user settings.
            defocusLabel.ForeColor = UITheme.ImageCaptureGroupBoxDisabledColor;
            bahtinovLabel.ForeColor = UITheme.ImageCaptureGroupBoxColor;
            toggleSwitch1.IsOn = Properties.Settings.Default.DefocusSwitch;

            // Should we be on top?
            if (Properties.Settings.Default.KeepOnTop == true)
                this.TopMost = true;
            else
                this.TopMost = false;
        }
        #endregion

        #region Configure Form

        /// <summary>
        /// Scales a 96-DPI logical pixel value to the current form DPI.
        /// </summary>
        private int S(int logicalPixelsAt96)
        {
            float dpiScale = DeviceDpi / 96f;
            return (int)Math.Round(logicalPixelsAt96 * dpiScale, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Positions the image display immediately to the right of the focus channel column
        /// using actual control bounds (reactive to narrow/wide channel widths and DPI).
        /// </summary>
        private void ApplyMainWorkspaceLayout()
        {
            if (!IsHandleCreated || imageDisplayComponent1 == null)
                return;

            if (InvokeRequired)
            {
                Invoke(new Action(ApplyMainWorkspaceLayout));
                return;
            }

            int anchorRight;
            if (groupBoxRed != null && groupBoxRed.Visible)
                anchorRight = groupBoxRed.Right;
            else if (analysisGroupBox != null && analysisGroupBox.Visible)
                anchorRight = analysisGroupBox.Right;
            else
                anchorRight = S(9) + S(DEFAULT_FOCUS_CHAN_WIDTH_AT96);

            int left = anchorRight + S(ImageGapAfterFocusColumnAt96);
            int y = imageDisplayComponent1.Location.Y;
            imageDisplayComponent1.Location = new Point(left, y);

            AdjustFormWidthToWorkspace();
        }

        /// <summary>
        /// Expands or contracts the form client width so it matches the image position/size
        /// (and calibration strip when visible). Keeps a consistent right margin.
        /// </summary>
        private void AdjustFormWidthToWorkspace()
        {
            if (imageDisplayComponent1 == null)
                return;

            int minClientWidth = imageDisplayComponent1.Right + S(RightMarginAfterImageAt96);

            if (calibrationComponent != null)
            {
                int calWidth = S(UITheme.CalibrateFrameWidth);
                // Match StartCalibration: calibration left = ClientSize.Width - calWidth - S(3)
                int minForImageAndCalibration =
                    imageDisplayComponent1.Right + S(GapImageToCalibrationAt96) + calWidth + S(3);
                minClientWidth = Math.Max(minClientWidth, minForImageAndCalibration);
            }

            if (ClientSize.Width != minClientWidth)
                ClientSize = new Size(minClientWidth, ClientSize.Height);
        }

        /// <summary>
        /// Invoked when the main form is first shown to the user.
        ///
        /// This override is used to display the optional startup calibration prompt
        /// after the main window has been created and activated. Showing the dialog
        /// at this point avoids focus and z-order issues that can occur if modal
        /// dialogs are displayed earlier in the application startup sequence.
        /// </summary>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ApplyMainWorkspaceLayout();
            ShowStartupCalibrationPromptIfNeeded(this);
        }

        /// <summary>
        /// Keeps the image aligned with the focus column when DPI / scaling changes.
        /// </summary>
        protected override void OnDpiChanged(DpiChangedEventArgs e)
        {
            base.OnDpiChanged(e);
            ApplyMainWorkspaceLayout();
        }

        /// <summary>
        /// Initializes and configures the red focus channel component.
        /// Removes any existing red focus channel component before creating a new one.
        /// </summary>
        private void InitializeRedFocusBox()
        {
            bool isCalibrated = Properties.Settings.Default.CalibrationCompleted;
            bool isTriBahtinov = (bahtinovLineData?.LineValue.Length == 9);
            bool isWidened = isCalibrated && isTriBahtinov;
            groupBoxRed.Visible = true;
            groupBoxRed.ConfigureFocusChannel(0, isWidened);
            groupBoxGreen.Visible = false;
            groupBoxBlue.Visible = false;
        }

        /// <summary>
        /// Initializes and configures the green focus channel component.
        /// Removes any existing green focus channel component before creating a new one.
        /// </summary>
        private void InitializeGreenFocusBox()
        {
            groupBoxGreen.Visible = true;
            groupBoxGreen.ConfigureFocusChannel(1, Properties.Settings.Default.CalibrationCompleted);
        }

        /// <summary>
        /// Initializes and configures the blue focus channel component.
        /// Removes any existing blue focus channel component before creating a new one.
        /// </summary>
        private void InitializeBlueFocusBox()
        {
            groupBoxBlue.Visible = true;
            groupBoxBlue.ConfigureFocusChannel(2, Properties.Settings.Default.CalibrationCompleted);
        }

        /// <summary>
        /// Configures the user interface elements of the form, such as hiding certain items or setting the window title.
        /// </summary>
        private void SetFormUI()
        {
            SetWindowTitleWithVersion();
        }

        /// <summary>
        /// Applies localized UI strings from the active language pack to form controls and menu items.
        /// </summary>
        private void ApplyLocalization()
        {
            var textPack = UiText.Current;

            Text = textPack.AppTitle;
            fileToolStripMenuItem.Text = textPack.MenuFile;
            quitToolStripMenuItem2.Text = textPack.MenuQuit;
            settingsToolStripMenuItem1.Text = textPack.MenuSetup;
            generalSettingsToolStripMenuItem.Text = textPack.MenuSettings;
            focusCalibrationToolStripMenuItem.Text = textPack.MenuCalibration;
            aboutToolStripMenuItem.Text = textPack.MenuHelp;
            helpToolStripMenuItem.Text = textPack.MenuUserManual;
            checkForUpdatesToolStripMenuItem.Text = textPack.MenuCheckUpdates;
            aboutToolStripMenuItem2.Text = textPack.MenuAbout;
            pleaseDonateToolStripMenuItem.Text = textPack.MenuSupportSkyCal;
            whatDoIDoNextToolStripMenuItem.Text = textPack.MenuWhatDoIDoNext;
            RoundedStartButton.Text = textPack.StartButtonSelectStar;
            bahtinovLabel.Text = textPack.LabelBahtinov;
            defocusLabel.Text = textPack.LabelDefocus;
            analysisGroupBox.Text = textPack.AnalysisModeGroupBox;
        }

        /// <summary>
        /// Applies the color scheme to various elements of the form, including the background, foreground, and other visual properties.
        /// </summary>
        private void SetColorScheme()
        {
            // Apply color scheme to the main form
            this.ForeColor = UITheme.DarkForeground;
            this.BackColor = UITheme.DarkBackground;

            // Apply color scheme to the menu strip
            menuStrip1.BackColor = UITheme.DarkBackground;
            menuStrip1.ForeColor = UITheme.MenuStripForeground;
            SetMenuItemsColor(menuStrip1.Items, UITheme.MenuDarkBackground, UITheme.MenuDarkForeground);

            // Apply color scheme to the start button
            RoundedStartButton.BackColor = UITheme.ButtonDarkBackground;
            RoundedStartButton.ForeColor = UITheme.ButtonDarkForeground;
            RoundedStartButton.FlatStyle = FlatStyle.Popup;

            // Apply color scheme to the title bar using DWM API
            var color = UITheme.DarkBackground;
            int colorValue = color.R;
            colorValue |= (color.G << 8);
            colorValue |= (color.B << 16);
            DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref colorValue, sizeof(int));

            // Apply color scheme to labels
            bahtinovLabel.ForeColor = UITheme.MenuStripForeground;
            defocusLabel.ForeColor = UITheme.MenuStripForeground;

            // Apply color scheme to Analysis Group Box
            analysisGroupBox.ForeColor = UITheme.ImageCaptureGroupBoxColor;
            bahtinovLabel.ForeColor = UITheme.ImageCaptureGroupBoxColor;
            defocusLabel.ForeColor = UITheme.ImageCaptureGroupBoxColor;

            //Apply color scheme to Toggle Switch
            toggleSwitch1.BackColor = UITheme.DarkBackground;
            toggleSwitch1.ForeColor = UITheme.DarkForeground;
        }

        /// <summary>
        /// Recursively sets the background and foreground colors for all items in a menu strip.
        /// </summary>
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

        /// <summary>
        /// Sets up event handlers for various form events, such as image capture and control changes.
        /// </summary>
        private void SetEvents()
        {
            ImageCapture.ImageReceivedEvent += OnImageReceived;
            ImageLostEventProvider.ImageLostEvent += HandleImageLost;
            toggleSwitch1.CheckedChanged += SlideSwitchChanged;
        }

        /// <summary>
        /// Restores the main window position from user settings, if a previously saved
        /// position exists and is still visible on one of the connected screens.
        /// Falls back to Windows default placement when no valid saved position is found.
        /// </summary>
        private void RestoreWindowPosition()
        {
            int x = Properties.Settings.Default.MainWindowX;
            int y = Properties.Settings.Default.MainWindowY;

            if (x == -1 && y == -1)
                return;

            // Ensure the top-left corner of the window is still visible on a screen.
            Point savedLocation = new Point(x, y);
            bool isOnScreen = Screen.AllScreens.Any(s => s.WorkingArea.Contains(savedLocation));

            if (isOnScreen)
            {
                this.StartPosition = FormStartPosition.Manual;
                this.Location = savedLocation;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Saves the current window position to user settings when the form is closing,
        /// so it can be restored on the next launch.
        /// </summary>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.MainWindowX = this.Location.X;
            Properties.Settings.Default.MainWindowY = this.Location.Y;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Starts or stops screen capture: select star region then capture, or stop and reset UI.
        /// </summary>
        private void StartButton_Click(object sender, EventArgs e)
        {
            if (screenCaptureRunningFlag)
            {
                StopCaptureSession();
                return;
            }

            // Clean slate before region selection (same as when stopping mid-session).
            ImageCapture.StopImageCapture();
            imageDisplayComponent1.ClearDisplay();
            bahtinovProcessing.StopImageProcessing();
            groupBoxGreen.Visible = false;
            groupBoxBlue.Visible = false;

            MinimizeWindow();
            if (!ImageCapture.SelectStar())
            {
                RestoreWindow();
                return;
            }

            ImageCapture.TrackingType = Properties.Settings.Default.DefocusSwitch ? 2 : 1;
            firstPassCompleted = false;
            ImageCapture.StartImageCapture();

            RestoreWindow();
            screenCaptureRunningFlag = true;
            RoundedStartButton.Text = UiText.Current.StartButtonStop;
            RoundedStartButton.Image = Properties.Resources.Stop;
        }

        /// <summary>
        /// Stops capture, resets Bahtinov/focus UI, and returns the start button to idle.
        /// </summary>
        private void StopCaptureSession()
        {
            ImageCapture.StopImageCapture();
            imageDisplayComponent1.ClearDisplay();
            bahtinovProcessing.StopImageProcessing();
            bahtinovLineData = null;
            imageType = 0;
            groupBoxRed.Visible = false;
            groupBoxGreen.Visible = false;
            groupBoxBlue.Visible = false;

            if (!Properties.Settings.Default.DefocusSwitch)
                InitializeRedFocusBox();

            screenCaptureRunningFlag = false;
            RoundedStartButton.Text = UiText.Current.StartButtonSelectStar;
            RoundedStartButton.Image = Properties.Resources.SelectionCircle;
            DecreaseFocusChannelSize();

            if (Properties.Settings.Default.CalibrationCompleted && calibrationComponent != null)
                StopCalibration();

            firstPassCompleted = false;
        }

        /// <summary>
        /// Processes and displays the received image on a background thread.
        /// </summary>
        private void OnImageReceived(object sender, ImageReceivedEventArgs e)
        {
            // Run the processing on a background thread
            Task.Run(() => ProcessAndDisplay(e.Image));
        }

        /// <summary>
        /// Bahtinov vs Defocus mode: persists <see cref="Properties.Settings.DefocusSwitch"/>, stops capture,
        /// and updates labels, focus UI, and menu availability.
        /// </summary>
        private void SlideSwitchChanged(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SlideSwitchChanged(sender, e)));
                return;
            }

            if (!(sender is ToggleSwitch toggle))
                return;

            bool defocusMode = toggle.IsOn;

            defocusLabel.ForeColor = defocusMode ? UITheme.ImageCaptureGroupBoxColor : UITheme.ImageCaptureGroupBoxDisabledColor;
            bahtinovLabel.ForeColor = defocusMode ? UITheme.ImageCaptureGroupBoxDisabledColor : UITheme.ImageCaptureGroupBoxColor;

            Properties.Settings.Default.DefocusSwitch = defocusMode;
            Properties.Settings.Default.Save();

            ImageCapture.StopImageCapture();
            imageDisplayComponent1.ClearDisplay();
            bahtinovProcessing.StopImageProcessing();

            screenCaptureRunningFlag = false;
            RoundedStartButton.Text = UiText.Current.StartButtonSelectStar;
            RoundedStartButton.Image = Properties.Resources.SelectionCircle;
            groupBoxRed.Visible = false;
            groupBoxGreen.Visible = false;
            groupBoxBlue.Visible = false;

            if (defocusMode)
            {
                DecreaseFocusChannelSize();
                whatDoIDoNextToolStripMenuItem.Enabled = false;
                focusCalibrationToolStripMenuItem.Enabled = false;
                StopCalibration();
            }
            else
            {
                bahtinovLineData = null;
                InitializeRedFocusBox();
                whatDoIDoNextToolStripMenuItem.Enabled = Properties.Settings.Default.CalibrationCompleted;
                focusCalibrationToolStripMenuItem.Enabled = true;
            }
        }

        /// <summary>
        /// Stops capture/processing when an image is lost, then (on the UI thread) resets the capture UI
        /// and shows an error message.
        /// </summary>
        private void HandleImageLost(object sender, ImageLostEventArgs e)
        {
            ImageCapture.StopImageCapture();
            bahtinovProcessing.StopImageProcessing();
            firstPassCompleted = false;
            screenCaptureRunningFlag = false;

            if (InvokeRequired)
                Invoke(new Action(() => HandleImageLostUi(e)));
            else
                HandleImageLostUi(e);
        }

        /// <summary>
        /// Resets capture UI and shows the error; must run on the UI thread.
        /// </summary>
        private void HandleImageLostUi(ImageLostEventArgs e)
        {
            bahtinovLineData = null;
            DecreaseFocusChannelSize();
            StopCalibration();
            groupBoxRed.Visible = false;
            groupBoxGreen.Visible = false;
            groupBoxBlue.Visible = false;
            InitializeRedFocusBox();
            RoundedStartButton.Text = UiText.Current.StartButtonSelectStar;
            RoundedStartButton.Image = Properties.Resources.SelectionCircle;
            ApplyMainWorkspaceLayout();

            DarkMessageBox.Show(e.Message, e.Title, e.Icon, e.Button, this);
        }

        /// <summary>
        /// Handles the click event for the Quit menu item, closing the form.
        /// </summary>
        private void QuitToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Activates the form when the mouse enters the menu strip area.
        /// </summary>
        private void MenuStrip1_MouseEnter(object sender, EventArgs e)
        {
            Activate();
        }

        /// <summary>
        /// Handles the click event for the About menu item, showing the About dialog.
        /// </summary>
        private void AboutToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            PositionDialogInsideMainWindow(aboutBox);
            aboutBox.TopMost = this.TopMost;
            aboutBox.ShowDialog(this);
        }

        /// <summary>
        /// Handles the click event for the Donate menu item, showing the Donate dialog.
        /// </summary>
        private void PleaseDonateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Donate donate = new Donate();
            PositionDialogInsideMainWindow(donate);
            donate.TopMost = this.TopMost;
            donate.ShowDialog(this);
        }

        /// <summary>
        /// Handles the click event for the Help menu item, displaying a help message.
        /// </summary>
        private void HelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Extract the PDF from resources and save it to a temporary location
                string tempPath = Path.Combine(Path.GetTempPath(), "help.pdf");

                // Write the embedded PDF file to the temporary location
                File.WriteAllBytes(tempPath, Properties.Resources.help);

                // Open the PDF file with the default PDF viewer
                Process.Start(tempPath);
            }
            catch (Exception ex)
            {
                // Handle exceptions if the file cannot be opened
                MessageBox.Show(string.Format(UiText.Current.HelpOpenErrorMessageFormat, ex.Message), UiText.Current.HelpOpenErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles the click event for the Check for Updates menu item, initiating an update check.
        /// </summary>
        private void CheckForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstallUpdateSyncWithInfo();
        }

        /// <summary>
        /// Activates the form when the mouse enters the form area.
        /// </summary>
        private void Form1_MouseEnter(object sender, EventArgs e)
        {
            this.Activate();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Processes the provided image and displays it based on whether it's a Bahtinov mask or a defocus star.
        /// </summary>
        private void ProcessAndDisplay(Bitmap original)
        {
            Bitmap image = new Bitmap(original);

            // Determine image type if first pass is not completed
            if (!firstPassCompleted)
            {
                if (Properties.Settings.Default.DefocusSwitch)
                {
                    // This is a defocus star
                    imageType = 2;
                }
                else
                {
                    // This is a Bahtinov image
                    imageType = 1;
                }
            }

            // Run the appropriate display method based on the image type
            if (imageType == 1)
                RunBahtinovDisplay(image);
            else if (imageType == 2)
                RunDefocusStarDisplay(image);
        }

        /// <summary>
        /// Displays the image if it is identified as a defocus star.
        /// </summary>
        private void RunDefocusStarDisplay(Bitmap image)
        {
            defocusStarProcessing.DisplayDefocusImage(image);
        }

        /// <summary>
        /// Runs the Bahtinov display process, including finding and displaying Bahtinov lines.
        /// </summary>
        private void RunBahtinovDisplay(Bitmap image)
        {
            try
            {
                // Find the brightest Bahtinov lines if first pass is not completed
                if (!firstPassCompleted)
                {
                    bahtinovLineData = bahtinovProcessing.FindBrightestLines(image);
                    bahtinovLineData.Sort();

                    if (screenCaptureRunningFlag == false)
                        return;

                    // validate the line groups
                    if (bahtinovLineData.ValidateBahtinovLines() == false)
                    {
                        // try again
                        bahtinovLineData = bahtinovProcessing.FindFaintLines(image);
                        bahtinovLineData.Sort();

                        // validate the line groups
                        if (bahtinovLineData.ValidateBahtinovLines() == false)
                            throw new InvalidOperationException("Invalid Bahtinov line group.");
                    }
                }

                // Perform a second pass to find fine details of Bahtinov lines
                bahtinovLineData = bahtinovProcessing.FindSubpixelLines(image, bahtinovLineData.LineValue.Length, bahtinovLineData);

                if (bahtinovLineData == null)
                    return;

                // Update the UI based on the number of detected lines
                int numberOfLines = bahtinovLineData.LineAngles.Length;

                if (!firstPassCompleted)
                {
                    UpdateFocusGroup(numberOfLines);
                    firstPassCompleted = true;
                }

                // Display the detected Bahtinov lines on the image
                bahtinovProcessing.DisplayLines(bahtinovLineData, image);
            }
            catch (Exception)
            {
                screenCaptureRunningFlag = false;
                ImageLostEventProvider.OnImageLost(UiText.Current.BahtinovLineDetectionFailedMessage, UiText.Current.BahtinovLineDetectionFailedTitle, MessageBoxIcon.Error, MessageBoxButtons.OK);
            }
        }

        /// <summary>
        /// Updates the focus group UI based on the number of Bahtinov lines detected.
        /// </summary>
        private void UpdateFocusGroup(int numberOfLines)
        {
            if (InvokeRequired)
            {
                // Marshal the call to the UI thread
                Invoke(new Action(() => UpdateFocusGroup(numberOfLines)));
            }
            else
            {
                // Remove existing controls and initialize focus controls based on the number of lines
                groupBoxGreen.Visible = false;
                groupBoxBlue.Visible = false;

                if (numberOfLines == 9)
                {
                    InitializeRedFocusBox();
                    InitializeGreenFocusBox();
                    InitializeBlueFocusBox();

                    if(Properties.Settings.Default.CalibrationCompleted)
                        IncreaseFocusChannelSize();
                }

                firstPassCompleted = true;
            }
        }

        /// <summary>
        /// Expands focus channel controls to the widened layout width for calibrated Tri-Bahtinov mode.
        /// </summary>
        private void IncreaseFocusChannelSize()
        {
            if (InvokeRequired)
            {
                // Marshal the call to the UI thread
                Invoke(new Action(IncreaseFocusChannelSize));
            }
            else
            {
                int maxWidth = S(MAX_FOCUS_CHAN_SIZE);

                if (groupBoxRed != null && groupBoxRed.Size.Width < maxWidth)
                {
                    groupBoxRed.Size = new Size(maxWidth, groupBoxRed.Size.Height);
                    groupBoxGreen.Size = new Size(maxWidth, groupBoxGreen.Size.Height);
                    groupBoxBlue.Size = new Size(maxWidth, groupBoxBlue.Size.Height);
                }
                    ApplyMainWorkspaceLayout();
            }
        }

        /// <summary>
        /// Restores focus channel controls to the default designer width.
        /// </summary>
        private void DecreaseFocusChannelSize()
        {
            if (InvokeRequired)
            {
                // Marshal the call to the UI thread
                Invoke(new Action(DecreaseFocusChannelSize));
            }
            else
            {
                // Restore to designer default (185), not the old 170 "min" — otherwise the image shifts
                // left over <c>analysisGroupBox</c> (still 185 wide). Also fixes any stale 170px width.
                int defaultWidth = S(DEFAULT_FOCUS_CHAN_WIDTH_AT96);

                if (groupBoxRed != null && groupBoxRed.Size.Width != defaultWidth)
                {
                    groupBoxRed.Size = new Size(defaultWidth, groupBoxRed.Size.Height);
                    groupBoxGreen.Size = new Size(defaultWidth, groupBoxGreen.Size.Height);
                    groupBoxBlue.Size = new Size(defaultWidth, groupBoxBlue.Size.Height);
                }

                ApplyMainWorkspaceLayout();
            }
        }

        /// <summary>
        /// Checks for and installs updates, synchronizing with information about the update.
        /// </summary>
        private void InstallUpdateSyncWithInfo()
        {
            Helper.Helper.GetLatestUpdate();
        }

        /// <summary>
        /// Sets the form's title to include the current version number of the application.
        /// </summary>
        private void SetWindowTitleWithVersion()
        {
            // Get the version number of the assembly
            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            // Create the version string
            string versionString = string.Format(UiText.Current.AppVersionFormat, version.Major, version.Minor, version.Build, version.Revision);

            // Use compound assignment to append the version string to the form title
            this.Text += $" - {versionString}";
        }

        /// <summary>
        /// Positions a dialog form inside the main window, centering it within the main window's bounds.
        /// </summary>
        private void PositionDialogInsideMainWindow(Form dialog)
        {
            // Get the main window's rectangle
            Rectangle mainWindowRect = this.Bounds;

            // Calculate the center position for the dialog within the main window
            int x = mainWindowRect.Left + (mainWindowRect.Width - dialog.Width) / 2;
            int y = mainWindowRect.Top + (mainWindowRect.Height - dialog.Height) / 2;

            // Ensure the dialog is fully within the main window's bounds
            x = Math.Max(mainWindowRect.Left, x);
            y = Math.Max(mainWindowRect.Top, y);

            dialog.StartPosition = FormStartPosition.Manual;
            dialog.Location = new Point(x, y);
        }

        #endregion

        #region CustomToolStripRenderer

        /// <summary>
        /// Custom renderer for the ToolStrip to customize the appearance of menu items, including background and text color.
        /// </summary>
        private sealed class CustomToolStripRenderer : ToolStripProfessionalRenderer
        {
            /// <summary>
            /// Initializes a renderer with the custom dark color table.
            /// </summary>
            public CustomToolStripRenderer() : base(new DarkColorTable())
            {
                RoundedEdges = false; // avoids odd light corners
            }

            /// <summary>
            /// Draws the border around dropdown toolstrip surfaces.
            /// </summary>
            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                // This draws the border around the whole dropdown
                Rectangle r = new Rectangle(0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);

                Color border = UITheme.DarkBackground;

                using (var pen = new Pen(border))
                {
                    e.Graphics.DrawRectangle(pen, r);
                }
            }

            /// <summary>
            /// Paints menu item backgrounds for dropdown and top-level menu states.
            /// </summary>
            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                bool isDropDownItem = e.ToolStrip is ToolStripDropDown;
                Rectangle rect = new Rectangle(Point.Empty, e.Item.Size);

                if (isDropDownItem)
                {
                    // Always paint a dark background for dropdown items, even if disabled
                    using (var back = new SolidBrush(UITheme.DarkBackground))
                        e.Graphics.FillRectangle(back, rect);

                    // Only draw highlight + border for enabled hot items
                    if (e.Item.Enabled && (e.Item.Selected || e.Item.Pressed))
                    {
                        using (var hi = new SolidBrush(UITheme.MenuHighlightBackground))
                            e.Graphics.FillRectangle(hi, rect);

                        using (var pen = new Pen(Color.Black))
                        {
                            rect.Width -= 1;
                            rect.Height -= 1;
                            e.Graphics.DrawRectangle(pen, rect);
                        }
                    }

                    return;
                }

                // Top-level menu strip items can use your existing logic
                if (e.Item.Enabled && (e.Item.Selected || e.Item.Pressed))
                {
                    using (var hi = new SolidBrush(UITheme.MenuHighlightBackground))
                        e.Graphics.FillRectangle(hi, e.Item.ContentRectangle);
                    return;
                }
            }

            /// <summary>
            /// Applies themed text colors for rendered menu items.
            /// </summary>
            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = (e.Item.Text == UiText.Current.MenuSupportSkyCal)
                    ? UITheme.GetGroupBoxTextColor(0)
                    : UITheme.MenuDarkForeground;

                base.OnRenderItemText(e);
            }

            private sealed class DarkColorTable : ProfessionalColorTable
            {
                public override Color ToolStripDropDownBackground => UITheme.DarkBackground;
                public override Color MenuBorder => UITheme.DarkBackground;
                public override Color ToolStripBorder => UITheme.DarkBackground;

                // These prevent the default light image-margin / gradient strip
                public override Color ImageMarginGradientBegin => UITheme.DarkBackground;
                public override Color ImageMarginGradientMiddle => UITheme.DarkBackground;
                public override Color ImageMarginGradientEnd => UITheme.DarkBackground;
            }
        }

        #endregion

        #region Guidance and Calibration

        /// <summary>
        /// Builds current guidance context and opens the next-step dialog.
        /// </summary>
        private void WhatDoIDoNextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Reload the correct JSON based on current MCT/SCT setting
            string model = Properties.Settings.Default.MCTSelected ? "MCT" : "SCT";
            LanguageLoader.LoadFromSystemCulture(AppDomain.CurrentDomain.BaseDirectory, model);

            var nextStepText = NextStepText.Current;
            var screwMap = new Dictionary<string, string>
            {
                [nextStepText.ChannelRed] = UiText.Current.ScrewRedLabel,
                [nextStepText.ChannelGreen] = UiText.Current.ScrewGreenLabel,
                [nextStepText.ChannelBlue] = UiText.Current.ScrewBlueLabel
            };

            double redError = groupBoxRed?.ErrorOffset ?? 0.0;
            double greenError = groupBoxGreen?.ErrorOffset ?? 0.0;
            double blueError = groupBoxBlue?.ErrorOffset ?? 0.0;

            try
            {
                NextStepGuidance guidance = NextStep.GetNextStepGuidance(
                    capturing: imageType == 1,
                    triBahtinovVisible: (bahtinovLineData?.LineAngles.Length == 9),
                    red: redError,
                    green: greenError,
                    blue: blueError,
                    tolerance: 0.15,
                    groupToScrewLabel: screwMap);

                using (var dlg = new NextStepDialog(guidance, this.Icon))
                {
                    dlg.TopMost = this.TopMost;
                    dlg.ShowDialog(this);
                }
                // enable "what should i do next" menu item if we have been calibrated
                menuStrip1.Items[4].Enabled = Properties.Settings.Default.CalibrationCompleted;
            }
            catch(FileNotFoundException err)
            {
                DarkMessageBox.Show(string.Format(UiText.Current.LanguageFileMissingMessageFormat, err.Message), UiText.Current.LanguageFileMissingTitle, MessageBoxIcon.Error, MessageBoxButtons.OK);
            }
        }

        /// <summary>
        /// Handles the click event for the Settings menu item, showing the Settings dialog and loading updated settings if dialog result is OK.
        /// </summary>
        private void GeneralSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Settings Dialog
            Settings settingsDialog = new Settings();
            PositionDialogInsideMainWindow(settingsDialog);
            settingsDialog.TopMost = this.TopMost;
            DialogResult result = settingsDialog.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                bahtinovProcessing.LoadSettings();
                voiceControl.LoadSettings();

                if(Properties.Settings.Default.KeepOnTop == true)
                    this.TopMost = true;
                else
                    this.TopMost = false;   

                // stop any running capture so that new settings will take effect, then restart if we were previously capturing
                if (screenCaptureRunningFlag == true && settingsDialog.RestartCapture)
                    StartButton_Click(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Opens the focus calibration workflow from the settings menu action.
        /// </summary>
        private void FocusCalibrationToolStripMenuItem_Click(object sender, EventArgs e)
        { 
            StartCalibration();
        }

        /// <summary>
        /// Creates and displays the calibration component anchored to the right side of the form.
        /// </summary>
        private void StartCalibration()
        {
            if (calibrationComponent != null)
                return;

            calibrationComponent = new CalibrationComponent(this)
            { 
                Width = S(UITheme.CalibrateFrameWidth),
                Height = this.ClientSize.Height,
                Location = new Point(this.ClientSize.Width - S(UITheme.CalibrateFrameWidth) - S(6), S(8)),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right
            };

            this.Controls.Add(calibrationComponent);
            calibrationComponent.BringToFront();
            ApplyMainWorkspaceLayout();
        }

        /// <summary>
        /// Stops and disposes the active calibration component, then reapplies workspace layout.
        /// </summary>
        public void StopCalibration()
        {
            if (calibrationComponent == null)
                return;
  
            this.Controls.Remove(calibrationComponent);
            calibrationComponent.Dispose();
            calibrationComponent = null;
            ApplyMainWorkspaceLayout();

            if(screenCaptureRunningFlag == true)
                StartButton_Click(this, EventArgs.Empty);

            // enable "what should i do next" menu item if we have been calibrated
            menuStrip1.Items[4].Enabled = Properties.Settings.Default.CalibrationCompleted;
        }

        /// <summary>
        /// Displays the startup calibration prompt if the user has not disabled it.
        ///
        /// The dialog recommends running the Settings Calibration workflow to ensure
        /// SkyCal is correctly tuned for the user's optical setup. If the user selects
        /// "Do not show again", the preference is persisted to user-scoped settings
        /// and the prompt will be suppressed on future application launches.
        ///
        /// If the user chooses to proceed, the Settings Calibration workflow is
        /// launched using the application's existing calibration entry point.
        /// </summary>
        private void ShowStartupCalibrationPromptIfNeeded(IWin32Window owner)
        {
#pragma warning disable CS0162

            if (TEST_MODE)
            {
                Properties.Settings.Default.ShowStartupCalibrationPrompt = true;
                Properties.Settings.Default.CalibrationCompleted = false;
                Properties.Settings.Default.Save();
            }
#pragma warning restore CS0162

            if (!Properties.Settings.Default.ShowStartupCalibrationPrompt)
                return;

            using (var dlg = new SkyCal.StartupDialog())
            {
                dlg.TopMost = this.TopMost;
                DialogResult result = dlg.ShowDialog(owner);

                if (dlg.DontShowAgain)
                {
                    Properties.Settings.Default.ShowStartupCalibrationPrompt = false;
                    Properties.Settings.Default.Save();
                }

                if (result == DialogResult.OK)
                {
                    StartCalibration();
                }
            }
        }

        /// <summary>
        /// Switches analysis mode to Bahtinov when the Bahtinov label is clicked.
        /// </summary>
        private void BahtinovLabel_Click(object sender, EventArgs e)
        {
            toggleSwitch1.IsOn = false;
        }

        /// <summary>
        /// Switches analysis mode to Defocus when the Defocus label is clicked.
        /// </summary>
        private void DefocusLabel_Click(object sender, EventArgs e)
        {
            toggleSwitch1.IsOn = true;
        }

        /// <summary>
        /// Minimizes the main window when minimize-on-capture is enabled in settings.
        /// </summary>
        public void MinimizeWindow()
        {
            if(Properties.Settings.Default.Minimize == true)
                this.WindowState = FormWindowState.Minimized;
        }

        /// <summary>
        /// Restores the main window from minimized state and activates it.
        /// </summary>
        public void RestoreWindow()
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
                this.BringToFront(); // Optional: bring to front
                this.Activate();     // Optional: give it focus
            }
        }

        #endregion
    }
}