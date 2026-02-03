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

using Bahtinov_Collimator.Custom_Components;
using Bahtinov_Collimator.Image_Processing;
using Bahtinov_Collimator.Voice;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    public partial class Form1 : Form
    {
        #region External Function

        /// <summary>
        /// Sets an attribute for a window using the Desktop Window Manager (DWM) API.
        /// </summary>
        /// <param name="hwnd">A handle to the window for which the attribute is being set.</param>
        /// <param name="attr">The attribute to be set, represented by an integer value.</param>
        /// <param name="attrValue">The value to set for the specified attribute. This is passed by reference to allow modification by the external function.</param>
        /// <param name="attrSize">The size of the attribute value in bytes.</param>
        /// <returns>An integer value indicating the result of the operation. A non-zero value indicates failure, while zero indicates success.</returns>
        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        #endregion

        #region Constants

        /// <summary>
        /// Specifies the attribute for enabling or disabling immersive dark mode for a window using the Desktop Window Manager (DWM) API.
        /// </summary>
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 19;

        #endregion

        #region Private Fields

        // Calibration User Component
        /// <summary>
        /// Allows for calibration of the focus direction.
        /// </summary>
        private CalibrationComponent calibrationComponent;

        // Focus Group Boxes
        /// <summary>
        /// Represents the red focus channel component for visualizing focus adjustments.
        /// </summary>
        private FocusChannelComponent groupBoxRed;

        /// <summary>
        /// Represents the green focus channel component for visualizing focus adjustments.
        /// </summary>
        private FocusChannelComponent groupBoxGreen;

        /// <summary>
        /// Represents the blue focus channel component for visualizing focus adjustments.
        /// </summary>
        private FocusChannelComponent groupBoxBlue;

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

        // Image Display Component
        /// <summary>
        /// Component responsible for displaying images with overlays and custom graphics.
        /// </summary>
        private ImageDisplayComponent imageDisplayComponent1;

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
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();

            // Set the form to not automatically scale based on its container.
            this.AutoScaleMode = AutoScaleMode.None;

            // Disable automatic resizing and set the form to grow and shrink based on its content.
            this.AutoSize = false;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            // Initialize the form's components.
            InitializeComponent();
            ApplyLocalization();

            // Initialize the red focus channel component.
            InitializeRedFocusBox();
            groupBoxRed.SetLabelsVisible(false);

            // Set up the user interface of the form.
            SetFormUI();

            // Apply the color scheme to the form.
            SetColorScheme();

            // Set up event handlers for the form's controls.
            SetEvents();

            // Initialize the image display component.
            InitializeDisplayComponent();

            // Increase the font size slightly for better readability.
            float increasedSize = this.Font.Size + 2.0f;
            Font largerFont = new Font(this.Font.FontFamily, increasedSize, this.Font.Style);

            // Apply the new font size to the form's controls.
            SetFontSize(largerFont);

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
        }
        #endregion

        #region Configure Form

        /// <summary>
        /// Invoked when the main form is first shown to the user.
        ///
        /// This override is used to display the optional startup calibration prompt
        /// after the main window has been created and activated. Showing the dialog
        /// at this point avoids focus and z-order issues that can occur if modal
        /// dialogs are displayed earlier in the application startup sequence.
        /// </summary>
        /// <param name="e">
        /// Event data associated with the form being shown.
        /// </param>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ShowStartupCalibrationPromptIfNeeded(this);
        }

        /// <summary>
        /// Initializes and configures the <see cref="ImageDisplayComponent"/> control, setting its location, size, and other properties.
        /// </summary>
        private void InitializeDisplayComponent()
        {
            this.imageDisplayComponent1 = new Bahtinov_Collimator.ImageDisplayComponent
            {
                Location = new System.Drawing.Point(276, 45),
                Margin = new System.Windows.Forms.Padding(2),
                Name = "imageDisplayComponent1",
                Size = new System.Drawing.Size(600, 600),
                TabIndex = 25
            };

            this.Controls.Add(this.imageDisplayComponent1);
            imageDisplayComponent1.ClearDisplay();
        }

        /// <summary>
        /// Sets the font size for various controls on the form.
        /// </summary>
        /// <param name="newFont">The new font to be applied to the controls.</param>
        private void SetFontSize(Font newFont)
        {
            // Apply the larger font to the MenuStrip
            menuStrip1.Font = newFont;

            // Apply the larger font to any labels
            bahtinovLabel.Font = newFont;
            defocusLabel.Font = newFont;
            RoundedStartButton.Font = newFont;
            analysisGroupBox.Font = newFont;
        }

        /// <summary>
        /// Initializes and configures the red focus channel component.
        /// Removes any existing red focus channel component before creating a new one.
        /// </summary>
        private void InitializeRedFocusBox()
        {
            if (groupBoxRed != null)
            {
                RemoveAndDisposeControls(groupBoxRed);
            }

            groupBoxRed = new FocusChannelComponent(0)
            {
                Size = new Size(255, 144),
                Location = new Point(8, 34)
            };
            this.Controls.Add(groupBoxRed);
        }

        /// <summary>
        /// Initializes and configures the green focus channel component.
        /// Removes any existing green focus channel component before creating a new one.
        /// </summary>
        private void InitializeGreenFocusBox()
        {
            if (groupBoxGreen != null)
            {
                RemoveAndDisposeControls(groupBoxGreen);
            }

            groupBoxGreen = new FocusChannelComponent(1)
            {
                Size = new Size(255, 144),
                Location = new Point(8, 172)
            };
            this.Controls.Add(groupBoxGreen);
        }

        /// <summary>
        /// Initializes and configures the blue focus channel component.
        /// Removes any existing blue focus channel component before creating a new one.
        /// </summary>
        private void InitializeBlueFocusBox()
        {
            if (groupBoxBlue != null)
            {
                RemoveAndDisposeControls(groupBoxBlue);
            }

            groupBoxBlue = new FocusChannelComponent(2)
            {
                Size = new Size(255, 144),
                Location = new Point(8, 310)
            };
            this.Controls.Add(groupBoxBlue);
        }

        /// <summary>
        /// Configures the user interface elements of the form, such as hiding certain items or setting the window title.
        /// </summary>
        private void SetFormUI()
        {
            SetWindowTitleWithVersion();
        }

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
        /// <param name="items">The collection of <see cref="ToolStripItem"/>s to update.</param>
        /// <param name="backColor">The background color to apply to the menu items.</param>
        /// <param name="foreColor">The foreground (text) color to apply to the menu items.</param>
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

        #endregion

        #region Events

        /// <summary>
        /// Handles the click event for the start button. Toggles the image capture process and updates button text accordingly.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void StartButton_Click(object sender, EventArgs e)
        {
            if (screenCaptureRunningFlag == false)
            {
                // Stop image capture and processing, clear the display, and remove focus channel components
                ImageCapture.StopImageCapture();
                imageDisplayComponent1.ClearDisplay();
                bahtinovProcessing.StopImageProcessing();
                RemoveAndDisposeControls(groupBoxGreen, groupBoxBlue);
                groupBoxRed?.SetLabelsVisible(false);

                // Start image capture if star selection is successful
                if (ImageCapture.SelectStar())
                {
                    ImageCapture.TrackingType = Properties.Settings.Default.DefocusSwitch ? 2 : 1;
                    firstPassCompleted = false;
                    ImageCapture.StartImageCapture();
                }

                screenCaptureRunningFlag = true;
                RoundedStartButton.Text = UiText.Current.StartButtonStop;
                RoundedStartButton.Image = Properties.Resources.Stop;
            }
            else
            {
                // Stop image capture and processing, clear the display, and reset button text
                ImageCapture.StopImageCapture();
                imageDisplayComponent1.ClearDisplay();
                bahtinovProcessing.StopImageProcessing();
                RemoveAndDisposeControls(groupBoxGreen, groupBoxBlue);
                groupBoxRed?.SetLabelsVisible(false);
                screenCaptureRunningFlag = false;
                RoundedStartButton.Text = UiText.Current.StartButtonSelectStar;
                RoundedStartButton.Image = Properties.Resources.SelectionCircle;
                firstPassCompleted = false;
            }
        }

        /// <summary>
        /// Processes and displays the received image on a background thread.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data containing the received image.</param>
        private void OnImageReceived(object sender, ImageReceivedEventArgs e)
        {
            // Run the processing on a background thread
            Task.Run(() => ProcessAndDisplay(e.Image));
        }

        /// <summary>
        /// Handles changes in the slide switch control, updating settings and UI accordingly.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void SlideSwitchChanged(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                // Marshal the call to the UI thread
                this.Invoke(new Action<object, EventArgs>(SlideSwitchChanged), sender, e);
                return;
            }

            ToggleSwitch toggleControl = sender as ToggleSwitch;
            bool toggle = toggleControl.IsOn;

            if(toggle)
            {
                defocusLabel.ForeColor = UITheme.ImageCaptureGroupBoxColor;
                bahtinovLabel.ForeColor = UITheme.ImageCaptureGroupBoxDisabledColor;
            }
            else
            {
                defocusLabel.ForeColor = UITheme.ImageCaptureGroupBoxDisabledColor;
                bahtinovLabel.ForeColor = UITheme.ImageCaptureGroupBoxColor;
            }

            // Update settings based on slide switch state
            Properties.Settings.Default.DefocusSwitch = toggle;
            Properties.Settings.Default.Save();

            // Stop image capture and processing, clear the display, and update UI
            ImageCapture.StopImageCapture();
            imageDisplayComponent1.ClearDisplay();
            bahtinovProcessing.StopImageProcessing();

            if (toggle)
            {
                screenCaptureRunningFlag = false;
                RoundedStartButton.Text = UiText.Current.StartButtonSelectStar;
                RoundedStartButton.Image = Properties.Resources.SelectionCircle;
                RemoveAndDisposeControls(groupBoxGreen, groupBoxBlue, groupBoxRed);
                whatDoIDoNextToolStripMenuItem.Enabled = false;
                focusCalibrationToolStripMenuItem.Enabled = false;
                
               StopCalibration();
            }
            else
            {
                screenCaptureRunningFlag = false;
                RoundedStartButton.Text = UiText.Current.StartButtonSelectStar;
                RoundedStartButton.Image = Properties.Resources.SelectionCircle;
                RemoveAndDisposeControls(groupBoxGreen, groupBoxBlue);
                InitializeRedFocusBox();
                groupBoxRed.SetLabelsVisible(false);
                whatDoIDoNextToolStripMenuItem.Enabled = Properties.Settings.Default.CalibrationCompleted;
                focusCalibrationToolStripMenuItem.Enabled = true;
            }
        }

        /// <summary>
        /// Handles the event when an image is lost during capture, displaying an error message.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data containing the error message.</param>
        private void HandleImageLost(object sender, ImageLostEventArgs e)
        {
            // Stop any image capture and processing
            ImageCapture.StopImageCapture();
            bahtinovProcessing.StopImageProcessing();
            firstPassCompleted = false;
            screenCaptureRunningFlag = false;

            // Ensure UI updates are performed on the UI thread
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    RemoveAndDisposeControls(groupBoxGreen, groupBoxBlue);
                    groupBoxRed?.SetLabelsVisible(false);
                    RoundedStartButton.Text = UiText.Current.StartButtonSelectStar;
                    RoundedStartButton.Image = Properties.Resources.SelectionCircle;
                    DarkMessageBox.Show(e.Message, e.Title, e.Icon, e.Button, this);
                }));
            }
            else
            {
                RemoveAndDisposeControls(groupBoxGreen, groupBoxBlue);
                groupBoxRed?.SetLabelsVisible(false);
                RoundedStartButton.Text = UiText.Current.StartButtonSelectStar;
                RoundedStartButton.Image = Properties.Resources.SelectionCircle;
                DarkMessageBox.Show(e.Message, e.Title, e.Icon, e.Button, this);
            }
        }

        /// <summary>
        /// Handles the click event for the Quit menu item, closing the form.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void QuitToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Activates the form when the mouse enters the menu strip area.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void MenuStrip1_MouseEnter(object sender, EventArgs e)
        {
            Activate();
        }

        /// <summary>
        /// Handles the click event for the About menu item, showing the About dialog.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void AboutToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            PositionDialogInsideMainWindow(aboutBox);
            aboutBox.ShowDialog();
        }

        /// <summary>
        /// Handles the click event for the Donate menu item, showing the Donate dialog.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void DonateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Donate donate = new Donate();
            PositionDialogInsideMainWindow(donate);
            donate.ShowDialog();
        }

        /// <summary>
        /// Handles the click event for the Donate menu item, showing the Donate dialog.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void PleaseDonateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Donate donate = new Donate();
            PositionDialogInsideMainWindow(donate);
            donate.ShowDialog();
        }

        /// <summary>
        /// Handles the click event for the Help menu item, displaying a help message.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
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
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void CheckForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstallUpdateSyncWithInfo();
        }

        /// <summary>
        /// Removes and disposes of specified controls from the form.
        /// </summary>
        /// <param name="controls">The controls to be removed and disposed of.</param>
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

        /// <summary>
        /// Activates the form when the mouse enters the form area.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void Form1_MouseEnter(object sender, EventArgs e)
        {
            this.Activate();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Processes the provided image and displays it based on whether it's a Bahtinov mask or a defocus star.
        /// </summary>
        /// <param name="original">The original bitmap image to process.</param>
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
        /// <param name="image">The bitmap image to display.</param>
        private void RunDefocusStarDisplay(Bitmap image)
        {
            defocusStarProcessing.DisplayDefocusImage(image);
        }

        /// <summary>
        /// Runs the Bahtinov display process, including finding and displaying Bahtinov lines.
        /// </summary>
        /// <param name="image">The bitmap image to process.</param>
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
        /// <param name="numberOfLines">The number of Bahtinov lines detected.</param>
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
                RemoveAndDisposeControls(groupBoxGreen, groupBoxBlue);

                if (numberOfLines == 9)
                {
                    InitializeRedFocusBox();
                    InitializeGreenFocusBox();
                    InitializeBlueFocusBox();
                    groupBoxRed.SetLabelsVisible(true);
                    groupBoxGreen.SetLabelsVisible(true);
                    groupBoxBlue.SetLabelsVisible(true);
                }

                firstPassCompleted = true;
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
        /// <param name="dialog">The dialog form to position.</param>
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
            public CustomToolStripRenderer() : base(new DarkColorTable())
            {
                RoundedEdges = false; // avoids odd light corners
            }

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

        private void WhatDoIDoNextToolStripMenuItem_Click(object sender, EventArgs e)
        {
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
                    dlg.ShowDialog(this);

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
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void GeneralSettingsToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void FocusCalibrationToolStripMenuItem_Click(object sender, EventArgs e)
        { 
            StartCalibration();
        }

        private void StartCalibration()
        {
            if (calibrationComponent != null)
                return;

            this.AutoSizeMode = AutoSizeMode.GrowOnly;
            this.Width += UITheme.CalibrateFrameWidth;

            int calWidth = UITheme.CalibrateFrameWidth;

            calibrationComponent = new CalibrationComponent(this)
            { 
                Width = UITheme.CalibrateFrameWidth,
                Location = new Point(this.Width - calWidth - 19, 22),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            this.Controls.Add(calibrationComponent);
        }

        public void StopCalibration()
        {
            if (calibrationComponent == null)
                return;
            this.Controls.Remove(calibrationComponent);
            calibrationComponent.Dispose();
            calibrationComponent = null;
            this.Width -= UITheme.CalibrateFrameWidth;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;

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
        /// <param name="owner">
        /// The window that will own the modal dialog, typically the main application form.
        /// </param>
        private void ShowStartupCalibrationPromptIfNeeded(IWin32Window owner)
        {

            Properties.Settings.Default.ShowStartupCalibrationPrompt = true;
            Properties.Settings.Default.CalibrationCompleted = false;
            Properties.Settings.Default.Save();

            if (!Properties.Settings.Default.ShowStartupCalibrationPrompt)
                return;

            using (var dlg = new SkyCal.StartupDialog())
            {
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

        private void bahtinovLabel_Click(object sender, EventArgs e)
        {
            toggleSwitch1.IsOn = false;
        }

        private void defocusLabel_Click(object sender, EventArgs e)
        {
            toggleSwitch1.IsOn = true;
        }
    }
}
