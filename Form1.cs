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
using Bahtinov_Collimator.Custom_Components;
using System.Diagnostics;
using System.IO;

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

        /// <summary>
        /// Retrieves a handle to the display monitor that is closest to the specified window.
        /// </summary>
        /// <param name="hwnd">A handle to the window of interest.</param>
        /// <param name="dwFlags">Determines the return value if the window does not intersect any display monitor. Use MONITOR_DEFAULTTONEAREST to get the handle to the monitor closest to the window.</param>
        /// <returns>A handle to the monitor closest to the specified window.</returns>
        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        /// <summary>
        /// Retrieves the DPI (Dots Per Inch) for a specified monitor.
        /// </summary>
        /// <param name="hmonitor">A handle to the monitor of interest.</param>
        /// <param name="dpiType">The DPI type to retrieve. Use MDT_EFFECTIVE_DPI to get the effective DPI for scaling.</param>
        /// <param name="dpiX">Outputs the DPI value for the X (horizontal) dimension of the monitor.</param>
        /// <param name="dpiY">Outputs the DPI value for the Y (vertical) dimension of the monitor.</param>
        /// <returns>An HRESULT indicating success or failure.</returns>
        [DllImport("shcore.dll")]
        private static extern int GetDpiForMonitor(IntPtr hmonitor, int dpiType, out uint dpiX, out uint dpiY);
  
        #endregion

        #region Constants

        /// <summary>
        /// Specifies the attribute for enabling or disabling immersive dark mode for a window using the Desktop Window Manager (DWM) API.
        /// </summary>
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 19;
        
        /// <summary>
        /// Flag that specifies the function should return the handle to the monitor closest to a specified window.
        /// Used with the MonitorFromWindow function.
        /// </summary>
        private const uint MONITOR_DEFAULTTONEAREST = 2;

        /// <summary>
        /// Specifies that the effective DPI (Dots Per Inch) of a monitor should be retrieved.
        /// Used with the GetDpiForMonitor function.
        /// </summary>
        private const int MDT_EFFECTIVE_DPI = 0;

        #endregion

        #region Private Fields

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
        private BahtinovProcessing bahtinovProcessing;

        /// <summary>
        /// Handles the processing of defocus star images for focus adjustment.
        /// </summary>
        private DefocusStarProcessing defocusStarProcessing;

        // Bahtinov line data
        /// <summary>
        /// Contains the data related to the lines in Bahtinov mask images.
        /// </summary>
        private BahtinovData bahtinovLineData;

        // Adjust Assistant
        /// <summary>
        /// Represents the dialog for adjusting assistive features.
        /// </summary>
        private AdjustAssist adjustAssistDialog;

        // Voice Generation
        /// <summary>
        /// Manages the voice control for audio notifications and speech synthesis.
        /// </summary>
        private VoiceControl voiceControl;

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
            // Set the form to not automatically scale based on its container.
            this.AutoScaleMode = AutoScaleMode.None;

            // Disable automatic resizing and set the form to grow and shrink based on its content.
            this.AutoSize = false;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            // Initialize the form's components.
            InitializeComponent();

            // Initialize the red focus channel component.
            InitializeRedFocusBox();

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

            // Create instances of the image processing classes.
            bahtinovProcessing = new BahtinovProcessing();
            defocusStarProcessing = new DefocusStarProcessing();

            // Set a custom renderer for the menu strip.
            menuStrip1.Renderer = new CustomToolStripRenderer();

            // Initialize the voice control component for audio notifications.
            voiceControl = new VoiceControl();

            // Set the initial state of the defocus switch based on user settings.
            slideSwitch2.IsOn = Properties.Settings.Default.DefocusSwitch;
        }

        #endregion

        #region Configure Form

        /// <summary>
        /// Initializes and configures the <see cref="ImageDisplayComponent"/> control, setting its location, size, and other properties.
        /// </summary>
        private void InitializeDisplayComponent()
        {
            this.imageDisplayComponent1 = new Bahtinov_Collimator.ImageDisplayComponent();
            this.imageDisplayComponent1.Location = new System.Drawing.Point(256, 45);
            this.imageDisplayComponent1.Margin = new System.Windows.Forms.Padding(2);
            this.imageDisplayComponent1.Name = "imageDisplayComponent1";
            this.imageDisplayComponent1.Size = new System.Drawing.Size(600, 600);
            this.imageDisplayComponent1.TabIndex = 25;

            this.Controls.Add(this.imageDisplayComponent1);
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
            label1.Font = newFont;
            Label2.Font = newFont;
            StartButton.Font = newFont;
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

            groupBoxRed = new FocusChannelComponent(0);
            groupBoxRed.Size = new Size(230, 114);
            groupBoxRed.Location = new Point(12, 35);
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

            groupBoxGreen = new FocusChannelComponent(1);
            groupBoxGreen.Size = new Size(230, 114);
            groupBoxGreen.Location = new Point(12, 150);
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

            groupBoxBlue = new FocusChannelComponent(2);
            groupBoxBlue.Size = new Size(230, 114);
            groupBoxBlue.Location = new Point(12, 265);
            this.Controls.Add(groupBoxBlue);
        }

        /// <summary>
        /// Configures the user interface elements of the form, such as hiding certain items or setting the window title.
        /// </summary>
        private void SetFormUI()
        {
            AdjustAssistToolStripMenuItem.Visible = false;
            SetWindowTitleWithVersion();
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
            StartButton.BackColor = UITheme.ButtonDarkBackground;
            StartButton.ForeColor = UITheme.ButtonDarkForeground;
            StartButton.FlatStyle = FlatStyle.Popup;

            // Apply color scheme to the title bar using DWM API
            var color = UITheme.DarkBackground;
            int colorValue = color.R | (color.G << 8) | (color.B << 16);
            DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref colorValue, sizeof(int));

            // Apply color scheme to labels
            label1.ForeColor = UITheme.MenuStripForeground;
            Label2.ForeColor = UITheme.MenuStripForeground;
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
            slideSwitch2.ToggleChanged += SlideSwitchChanged;
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

                // Start image capture if star selection is successful
                if (ImageCapture.SelectStar())
                {
                    ImageCapture.TrackingType = Properties.Settings.Default.DefocusSwitch ? 2 : 1;
                    firstPassCompleted = false;
                    ImageCapture.StartImageCapture();
                }

                screenCaptureRunningFlag = true;
                StartButton.Text = "Stop";
            }
            else
            {
                // Stop image capture and processing, clear the display, and reset button text
                ImageCapture.StopImageCapture();
                imageDisplayComponent1.ClearDisplay();
                bahtinovProcessing.StopImageProcessing();
                RemoveAndDisposeControls(groupBoxGreen, groupBoxBlue);
                screenCaptureRunningFlag = false;
                StartButton.Text = "Select Star";
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

            SlideSwitch switchControl = sender as SlideSwitch;
            bool toggle = switchControl.IsOn;

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
                StartButton.Text = "Select Star";
                RemoveAndDisposeControls(groupBoxGreen, groupBoxBlue, groupBoxRed);
                adjustAssistDialog?.Close();
                AdjustAssistToolStripMenuItem.Visible = false;
            }
            else
            {
                screenCaptureRunningFlag = false;
                StartButton.Text = "Select Star";
                RemoveAndDisposeControls(groupBoxGreen, groupBoxBlue);
                InitializeRedFocusBox();
            }
        }

        /// <summary>
        /// Handles the event when an image is lost during capture, displaying an error message.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data containing the error message.</param>
        private void HandleImageLost(object sender, ImageLostEventArgs e)
        {
            ImageCapture.StopImageCapture();
            bahtinovProcessing.StopImageProcessing();
            firstPassCompleted = false;
            screenCaptureRunningFlag = false;
            StartButton.Text = "Select Star";

            DarkMessageBox.Show(e.Message, e.Title, e.Icon, e.Button, this);
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
                MessageBox.Show("Unable to open the help file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles the click event for the Check for Updates menu item, initiating an update check.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstallUpdateSyncWithInfo();
        }

        /// <summary>
        /// Handles the FormClosed event for the CheatSheet form, setting the adjust assist dialog reference to null.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void CheatSheet_FormClosed(object sender, FormClosedEventArgs e)
        {
            adjustAssistDialog = null;
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
        /// Handles the click event for the Settings menu item, showing the Settings dialog and loading updated settings if dialog result is OK.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
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

        /// <summary>
        /// Activates the form when the mouse enters the form area.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void Form1_MouseEnter(object sender, EventArgs e)
        {
            this.Activate();
        }

        /// <summary>
        /// Handles the click event for the Cheat Sheet menu item, showing the AdjustAssist dialog if it's not already open.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
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
                }

                // Perform a second pass to find fine details of Bahtinov lines
                bahtinovLineData = bahtinovProcessing.FindSubpixelLines(image, bahtinovLineData.LineValue.Length, bahtinovLineData);

                // Update the UI based on the number of detected lines
                int numberOfLines = bahtinovLineData.LineAngles.Length;

                if (!firstPassCompleted)
                {
                    UpdateFocusGroup(numberOfLines);
                    firstPassCompleted = true;
                }
            }
            catch (Exception e)
            {
                // Handle exceptions by stopping image capture and processing, and display an error message
                ImageCapture.StopImageCapture();
                bahtinovProcessing.StopImageProcessing();
                firstPassCompleted = false;
                DarkMessageBox.Show("Failed Bahtinov line detection", "RunBahtinovDisplay", MessageBoxIcon.Error, MessageBoxButtons.OK, this);
                imageDisplayComponent1.ClearDisplay();
            }

            // Display the detected Bahtinov lines on the image
            bahtinovProcessing.DisplayLines(bahtinovLineData, image);
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
                    AdjustAssistToolStripMenuItem.Visible = true;
                }
                else
                {
                    AdjustAssistToolStripMenuItem.Visible = false;
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
            string versionString = $"V {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";

            // Set the form's title
            this.Text = this.Text + $" - {versionString}";
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
        private class CustomToolStripRenderer : ToolStripProfessionalRenderer
        {
            /// <summary>
            /// Customizes the rendering of the menu item background.
            /// </summary>
            /// <param name="e">Event arguments containing information about the menu item to render.</param>
            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                // Render background with custom color if the item is selected or pressed
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

            /// <summary>
            /// Customizes the rendering of the menu item text.
            /// </summary>
            /// <param name="e">Event arguments containing information about the text to render.</param>
            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                // Set custom text color and font style for the "Buy me a Coffee?" menu item
                if (e.Item.Text == "Buy me a Coffee?")
                {
                    e.TextColor = Color.Red;
                    // Set the font to bold
                    e.TextFont = new Font(e.TextFont, FontStyle.Bold);
                }
                else
                {
                    // Use the default text color for other items
                    e.TextColor = UITheme.MenuDarkForeground;
                }

                base.OnRenderItemText(e);
            }
        }

        #endregion
    }
}