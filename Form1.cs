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
using System.Threading;
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

        /// <summary>Queries the DPI of the monitor hosting the given window.</summary>
        [DllImport("user32.dll")]
        private static extern uint GetDpiForWindow(IntPtr hwnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private const int WM_DPICHANGED = 0x02E0;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

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

        /// <summary>
        /// Horizontal offset from <see cref="RoundedStartButton"/>.Left to <see cref="toggleSwitch1"/>.Left (96 DPI).
        /// Matches designer: button 18, switch 36.
        /// </summary>
        private const int AnalysisToggleSwitchLeftOffsetFromButtonLeftAt96 = 5;

        /// <summary>
        /// Horizontal offset from <see cref="RoundedStartButton"/>.Left to Bahtinov/Defocus labels (96 DPI).
        /// Matches designer: button 18, labels 68.
        /// </summary>
        private const int AnalysisModeLabelsLeftOffsetFromButtonLeftAt96 = 30;

        /// Sets testing mode for Calibration 
        private const bool TEST_MODE = false;

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
        /// Marked volatile: read/written from both the UI thread and the background
        /// processing Task spawned in <see cref="OnImageReceived"/>.
        /// </summary>
        private volatile bool firstPassCompleted = false;

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
        /// Marked volatile because the reference is published from the background
        /// processing Task and read from the UI thread (menu gating, hover rendering).
        /// </summary>
        private volatile BahtinovData bahtinovLineData;

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
        /// Volatile: written by the background processing Task and the UI thread.
        /// </summary>
        private volatile int imageType = 0;

        // Screen Capture Flag
        /// <summary>
        /// Indicates whether the screen capture process is currently running.
        /// Volatile: written from the UI thread and read from the background processing Task.
        /// </summary>
        private volatile bool screenCaptureRunningFlag = false;

        /// <summary>
        /// Single-flight gate for <see cref="ProcessAndDisplay"/>. 0 = idle, 1 = a frame is being
        /// processed. New frames that arrive while processing is in flight are dropped so heavy
        /// Bahtinov work cannot pile up behind the 10 ms capture timer during tracking.
        /// </summary>
        private int processingFrameFlag;
        private const string AutoLanguagePreference = "auto";

        /// <summary>
        /// Target position to move the form to after it first becomes visible
        /// (see <see cref="RestoreWindowPosition"/>). Moving an invisible window
        /// across a DPI boundary does not refresh the HWND's DC DPI context,
        /// so we defer the move to OnShown where the form is Windows-visible
        /// (kept invisible to the user via Opacity).
        /// </summary>
        private Point? _pendingMoveTarget;

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

            // Keep the form visually invisible on first show. We need Windows
            // to treat it as a visible window when we move it to the saved
            // (potentially high-DPI) monitor so the HWND's DC DPI context
            // actually refreshes - an INVISIBLE move doesn't refresh the DC,
            // which leaves 10pt fonts rendering at 13px on a 168-DPI display
            // ("fonts too small"). By running the move while Opacity == 0,
            // the window is visible to Windows but not the user; after the
            // move completes we restore Opacity to 1 in OnShown.
            this.Opacity = 0d;

            // Initialize the form's components.
            InitializeComponent();

            imageDisplayComponent1.ClearDisplay(); 
            RestoreWindowPosition();
            ApplyLocalization();
            ApplyLayoutDirectionForLanguage();
            PopulateLanguageMenu();

            // Initialize the red focus channel component.
            InitializeRedFocusBox();

            // Set up the user interface of the form.
            SetFormUI();

            // Apply the color scheme to the form.
            SetColorScheme();

            // Set up event handlers for the form's controls.
            SetEvents();

            // disable and gray the "what should i do next" menu item if we havent been calibrated
            menuStrip1.Items[4].Enabled = Properties.Settings.Default.CalibrationCompleted && !Properties.Settings.Default.NoneSelected;

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
            {
                ClientSize = new Size(minClientWidth, ClientSize.Height);

                // In RTL/RightToLeftLayout, a ClientSize.Width change exposes new
                // pixels on the form's left edge (mirrored), and Windows does not
                // automatically paint that newly invalidated strip with the form
                // BackColor. Force a full repaint so the dark background fills the
                // entire client area instead of leaving a black strip on the left
                // when calibration opens/closes.
                Invalidate();
            }
        }

        /// <summary>
        /// Keeps the bottom-left analysis <see cref="analysisGroupBox"/> the same width as the focus channel
        /// column when Tri-Bahtinov mode widens or narrows those controls, centers
        /// <see cref="RoundedStartButton"/> horizontally (size unchanged), and moves the mode switch and
        /// labels by the same delta so their layout relative to the button is unchanged.
        /// </summary>
        private void SyncAnalysisGroupBoxWidthToFocusColumn()
        {
            if (analysisGroupBox == null || groupBoxRed == null)
                return;

            int w = groupBoxRed.Width;
            analysisGroupBox.Size = new Size(w, analysisGroupBox.Height);

            if (RoundedStartButton == null)
                return;

            int buttonX = Math.Max(0, (analysisGroupBox.ClientSize.Width - RoundedStartButton.Width) / 2);
            RoundedStartButton.Location = new Point(buttonX, RoundedStartButton.Location.Y);

            int toggleX = buttonX + S(AnalysisToggleSwitchLeftOffsetFromButtonLeftAt96);
            int labelsX = buttonX + S(AnalysisModeLabelsLeftOffsetFromButtonLeftAt96);

            if (toggleSwitch1 != null)
                toggleSwitch1.Location = new Point(toggleX, toggleSwitch1.Location.Y);
            if (bahtinovLabel != null)
                bahtinovLabel.Location = new Point(labelsX, bahtinovLabel.Location.Y);
            if (defocusLabel != null)
                defocusLabel.Location = new Point(labelsX, defocusLabel.Location.Y);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Safety net: if the process ended up running in PerMonitor v1
            // (so WinForms' AutoScaleMode.Dpi did not fire because DeviceDpi
            // is stuck at 96 despite being on a higher-DPI monitor), manually
            // apply the correct scale. When PerMonitorV2 is active this is a
            // no-op because GetDpiForWindow will equal DeviceDpi.
            ApplyInitialDpiScaleIfNeeded();
        }

        private bool initialDpiScaleApplied;

        /// <summary>
        /// If the monitor hosting this form reports a different DPI than
        /// <see cref="Control.DeviceDpi"/>, applies a one-shot
        /// <see cref="Control.Scale(SizeF)"/> plus font resize so controls and
        /// text render at the correct physical size. Safe no-op when DPI
        /// already matches.
        /// </summary>
        private void ApplyInitialDpiScaleIfNeeded()
        {
            if (initialDpiScaleApplied) return;
            initialDpiScaleApplied = true;

            if (!IsHandleCreated) return;

            int monitorDpi;
            try
            {
                monitorDpi = (int)GetDpiForWindow(Handle);
            }
            catch (DllNotFoundException) { return; }
            catch (EntryPointNotFoundException) { return; }

            int formDpi = DeviceDpi > 0 ? DeviceDpi : 96;
            if (monitorDpi <= 0 || Math.Abs(monitorDpi - formDpi) <= 1)
            {
                UITheme.DeviceDpi = formDpi;
                return;
            }

            float factor = (float)monitorDpi / formDpi;

            SuspendLayout();
            try
            {
                // Scale control sizes and positions (does not touch Font).
                Scale(new SizeF(factor, factor));

                // Recursively scale every Font in the tree: controls (most of
                // which have a designer-set explicit Font that does NOT inherit
                // from the form) and ToolStrip / ToolStripItems (which live in
                // a parallel hierarchy that Control.Controls doesn't expose).
                ScaleFontsRecursive(this, factor);
            }
            finally
            {
                ResumeLayout(true);
            }

            UITheme.DeviceDpi = monitorDpi;
        }

        /// <summary>
        /// Recursively multiplies every control and ToolStripItem Font size by
        /// <paramref name="factor"/>. Ambient-font controls are still touched
        /// explicitly because we cannot reliably tell which are ambient in all
        /// framework versions; setting Font to a new instance at the same
        /// effective size for ambient children is harmless.
        /// </summary>
        private static void ScaleFontsRecursive(Control control, float factor)
        {
            Font f = control.Font;
            if (f != null)
            {
                control.Font = new Font(
                    f.FontFamily,
                    f.Size * factor,
                    f.Style,
                    f.Unit,
                    f.GdiCharSet,
                    f.GdiVerticalFont);
            }

            if (control is ToolStrip strip)
            {
                foreach (ToolStripItem item in strip.Items)
                    ScaleToolStripItemFont(item, factor);
            }

            foreach (Control child in control.Controls)
                ScaleFontsRecursive(child, factor);
        }

        private static void ScaleToolStripItemFont(ToolStripItem item, float factor)
        {
            Font f = item.Font;
            if (f != null)
            {
                item.Font = new Font(
                    f.FontFamily,
                    f.Size * factor,
                    f.Style,
                    f.Unit,
                    f.GdiCharSet,
                    f.GdiVerticalFont);
            }
            if (item is ToolStripDropDownItem dd)
            {
                foreach (ToolStripItem child in dd.DropDownItems)
                    ScaleToolStripItemFont(child, factor);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // If we deferred moving the form to a saved (potentially
            // different-DPI) position, do it now. At this point the form is
            // Windows-visible (though user-invisible thanks to Opacity == 0
            // set in the constructor), so crossing into a higher-DPI monitor
            // here refreshes the HWND's DC DPI context, fires WM_DPICHANGED,
            // and lets WinForms scale both controls AND fonts correctly for
            // the first paint. As a belt-and-braces, if WinForms' DeviceDpi
            // still doesn't match the monitor after the move, synthesize
            // WM_DPICHANGED ourselves.
            if (_pendingMoveTarget.HasValue)
            {
                Location = _pendingMoveTarget.Value;
                _pendingMoveTarget = null;
                SynthesizeDpiChangedIfNeeded();
            }

            // Now make the form actually visible to the user.
            if (Opacity < 1d)
                Opacity = 1d;

            SyncAnalysisGroupBoxWidthToFocusColumn();
            ApplyMainWorkspaceLayout();

            ShowStartupCalibrationPromptIfNeeded(this);
        }

        /// <summary>
        /// Keeps the image aligned with the focus column when DPI / scaling changes.
        /// </summary>
        protected override void OnDpiChanged(DpiChangedEventArgs e)
        {
            base.OnDpiChanged(e);

            SyncAnalysisGroupBoxWidthToFocusColumn();
            ApplyMainWorkspaceLayout();
            RepositionCalibrationComponent();
            RefreshFocusChannelsAfterDpiChange();
        }

        /// <summary>
        /// Re-applies the calibration component's size and location using the current DPI.
        ///
        /// Historical note: this control used to be anchored Top|Bottom|Right and relied on
        /// <see cref="Control.AutoScaleMode"/>=Dpi. On a monitor-to-monitor DPI change
        /// (verified with logging at 96->168), WinForms re-ran PerformAutoScale every time
        /// the bounds were touched, compounding scale factors — the bottom edge stayed pinned
        /// to the designer height 624 while Top was pushed negative (e.g. Y=-1792, H=2416).
        /// The mixed Top|Bottom|Right anchor (no Left) then moved X on top of that.
        ///
        /// Fix: <see cref="CalibrationComponent"/> is now <see cref="AutoScaleMode.None"/>
        /// and <see cref="AnchorStyles.None"/>, so Form1 is the single authority for its
        /// bounds. We use <see cref="Control.SetBounds(int,int,int,int)"/> inside a
        /// <see cref="Control.SuspendLayout"/> / <see cref="Control.ResumeLayout(bool)"/>
        /// block so size and location change atomically and no intermediate layout pass
        /// can see a half-updated state.
        /// </summary>
        private void RepositionCalibrationComponent()
        {
            if (calibrationComponent == null || calibrationComponent.IsDisposed)
                return;

            int calWidth = S(UITheme.CalibrateFrameWidth);
            int topOffset = S(8);
            int rightGap = S(6);

            int newX = ClientSize.Width - calWidth - rightGap;
            int newY = topOffset;
            int newHeight = Math.Max(0, ClientSize.Height - topOffset);

            calibrationComponent.SuspendLayout();
            try
            {
                calibrationComponent.SetBounds(newX, newY, calWidth, newHeight);
            }
            finally
            {
                calibrationComponent.ResumeLayout(true);
            }
        }

        /// <summary>
        /// The calibration panel has no anchor (see <see cref="RepositionCalibrationComponent"/>),
        /// so when the user resizes the form we manually keep it glued to the right edge.
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (calibrationComponent != null && !calibrationComponent.IsDisposed)
                RepositionCalibrationComponent();
        }

        /// <summary>
        /// After a DPI change the focus channel group boxes can end up hidden behind the
        /// <see cref="calibrationComponent"/> (which is always BringToFront'd) or be left
        /// without an up-to-date internal layout. This restores their z-order so they remain
        /// visible alongside calibration, and forces a redraw so the inner history bar and
        /// mirror drawings are present.
        /// </summary>
        public void RefreshFocusChannelsAfterDpiChange()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(RefreshFocusChannelsAfterDpiChange));
                return;
            }

            FocusChannelComponent[] channels = { groupBoxRed, groupBoxGreen, groupBoxBlue };
            foreach (var channel in channels)
            {
                if (channel == null)
                    continue;

                if (channel.Visible)
                    channel.BringToFront();

                channel.Invalidate(true);
            }

            if (imageDisplayComponent1 != null)
                imageDisplayComponent1.Invalidate(true);
        }

        /// <summary>
        /// Initializes and configures the red focus channel component.
        /// Removes any existing red focus channel component before creating a new one.
        /// </summary>
        private void InitializeRedFocusBox(bool soloRedChannel = true)
        {
            bool isCalibrated = Properties.Settings.Default.CalibrationCompleted;
            bool isTriBahtinov = (bahtinovLineData?.LineValue.Length == 9);
            bool isWidened = isCalibrated && isTriBahtinov && !Properties.Settings.Default.NoneSelected;
            groupBoxRed.Visible = true;
            groupBoxRed.ConfigureFocusChannel(0, isWidened, soloRedChannel);

            groupBoxRed.HoverHighlightEnabled = isTriBahtinov;
            groupBoxGreen.HoverHighlightEnabled = isTriBahtinov;
            groupBoxBlue.HoverHighlightEnabled = isTriBahtinov;

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
            groupBoxGreen.ConfigureFocusChannel(1, Properties.Settings.Default.CalibrationCompleted && !Properties.Settings.Default.NoneSelected);
        }

        /// <summary>
        /// Initializes and configures the blue focus channel component.
        /// Removes any existing blue focus channel component before creating a new one.
        /// </summary>
        private void InitializeBlueFocusBox()
        {
            groupBoxBlue.Visible = true;
            groupBoxBlue.ConfigureFocusChannel(2, Properties.Settings.Default.CalibrationCompleted && !Properties.Settings.Default.NoneSelected);
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
            languageToolStripMenuItem.Text = textPack.MenuLanguage;
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
        /// Applies right-to-left layout when the current language requires it.
        /// </summary>
        private void ApplyLayoutDirectionForLanguage()
        {
            bool isRtl = LanguageLoader.IsCurrentLanguageRightToLeft();
            RightToLeft = isRtl ? RightToLeft.Yes : RightToLeft.No;
            RightToLeftLayout = isRtl;
        }

        /// <summary>
        /// Rebuilds the language submenu from valid language packs on disk.
        /// </summary>
        private void PopulateLanguageMenu()
        {
            languageToolStripMenuItem.DropDownItems.Clear();

            string preference = NormalizeLanguagePreference(Properties.Settings.Default.LanguagePreference);
            var autoItem = new ToolStripMenuItem(UiText.Current.LanguageAutoOption)
            {
                Tag = AutoLanguagePreference,
                Checked = string.Equals(preference, AutoLanguagePreference, StringComparison.OrdinalIgnoreCase)
            };
            autoItem.Click += LanguageSelectionToolStripMenuItem_Click;
            languageToolStripMenuItem.DropDownItems.Add(autoItem);
            languageToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());

            foreach (var option in LanguageLoader.GetAvailableLanguages(AppDomain.CurrentDomain.BaseDirectory))
            {
                var languageItem = new ToolStripMenuItem(option.DisplayName)
                {
                    Tag = option.Code,
                    Checked = string.Equals(preference, option.Code, StringComparison.OrdinalIgnoreCase)
                };
                languageItem.Click += LanguageSelectionToolStripMenuItem_Click;
                languageToolStripMenuItem.DropDownItems.Add(languageItem);
            }
        }

        private static string NormalizeLanguagePreference(string preference)
        {
            if (string.IsNullOrWhiteSpace(preference))
                return AutoLanguagePreference;

            return preference.Trim();
        }

        /// <summary>
        /// Persists a language preference and prompts the user to restart in the selected language pack.
        /// </summary>
        private void LanguageSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(sender is ToolStripMenuItem selectedItem))
                return;

            string selectedPreference = NormalizeLanguagePreference(selectedItem.Tag as string);
            string previousPreference = NormalizeLanguagePreference(Properties.Settings.Default.LanguagePreference);
            if (string.Equals(selectedPreference, previousPreference, StringComparison.OrdinalIgnoreCase))
                return;

            UiTextPack promptPack = UiText.Current;
            bool unavailable = false;
            string resolvedCode = LanguageLoader.ResolveLanguageCodeForPreference(AppDomain.CurrentDomain.BaseDirectory, selectedPreference);
            try
            {
                promptPack = LanguageLoader.ReadUiTextPackForLanguage(AppDomain.CurrentDomain.BaseDirectory, resolvedCode);
            }
            catch
            {
                resolvedCode = LanguageLoader.DefaultLanguageCode;
                unavailable = true;
                promptPack = LanguageLoader.ReadUiTextPackForLanguage(AppDomain.CurrentDomain.BaseDirectory, LanguageLoader.DefaultLanguageCode);
            }

            if (!string.Equals(selectedPreference, AutoLanguagePreference, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(selectedPreference, resolvedCode, StringComparison.OrdinalIgnoreCase))
            {
                unavailable = true;
            }

            // Use the *new* language's directionality for the prompt dialogs so the
            // buttons aren't laid out in the previously loaded language's mirror order
            // (e.g., switching away from Arabic should not leave the prompt RTL-mirrored).
            bool promptIsRtl = LanguageLoader.IsLanguageRightToLeft(resolvedCode);

            if (unavailable)
            {
                DarkMessageBox.Show(
                    string.Format(promptPack.LanguageUnavailableMessageFormat, selectedPreference, resolvedCode),
                    promptPack.LanguageUnavailableTitle,
                    MessageBoxIcon.Warning,
                    MessageBoxButtons.OK,
                    promptPack,
                    this,
                    promptIsRtl);
            }

            DialogResult restartResult = DarkMessageBox.Show(
                promptPack.LanguageRestartRequiredMessage,
                promptPack.LanguageRestartRequiredTitle,
                MessageBoxIcon.Question,
                MessageBoxButtons.OKCancel,
                promptPack,
                this,
                promptIsRtl);

            if (restartResult == DialogResult.Yes)
            {
                Properties.Settings.Default.LanguagePreference = selectedPreference;
                Properties.Settings.Default.Save();
                Application.Restart();
                Close();
                return;
            }

            // Cancel/No means keep the prior language choice.
            Properties.Settings.Default.LanguagePreference = previousPreference;
            Properties.Settings.Default.Save();
            PopulateLanguageMenu();
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
            menuStrip1.BackColor = UITheme.MenuStripDarkBackground;
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
            foreach (ToolStripItem item in items)
            {
                if (!(item is ToolStripMenuItem menuItem))
                    continue;

                menuItem.BackColor = backColor;
                menuItem.ForeColor = foreColor;
                if (menuItem.HasDropDownItems)
                    SetMenuItemsColor(menuItem.DropDownItems, backColor, foreColor);
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
            // Determine where we ultimately want the window to appear.
            int x = Properties.Settings.Default.MainWindowX;
            int y = Properties.Settings.Default.MainWindowY;

            Point? target = null;
            if (!(x == -1 && y == -1))
            {
                Point savedLocation = new Point(x, y);
                if (Screen.AllScreens.Any(s => s.WorkingArea.Contains(savedLocation)))
                    target = savedLocation;
            }

            // ALWAYS force handle creation on the primary monitor first, then
            // (if necessary) move to the final position. This sidesteps a
            // .NET Framework WinForms quirk where a top-level form whose
            // handle is created directly on a non-primary (different-DPI)
            // monitor gets stuck with Form.DeviceDpi == 96 even though the
            // underlying HWND and child controls are at the real monitor DPI
            // - a hybrid state that breaks every layout calculation downstream.
            //
            // Creating the handle on the primary monitor and THEN moving to
            // the final position causes Windows to fire WM_DPICHANGED on the
            // monitor crossing, which drives WinForms' built-in AutoScaleMode.Dpi
            // path and rescales the form cleanly. The window is still invisible
            // during this dance, so there is no visible flash.
            Screen primary = Screen.PrimaryScreen;
            Point primaryStart = new Point(
                primary.WorkingArea.Left + 50,
                primary.WorkingArea.Top + 50);

            this.StartPosition = FormStartPosition.Manual;
            this.Location = primaryStart;

            // Force handle creation NOW at primaryStart. Accessing Handle
            // triggers CreateHandle with the current Location. Form is still
            // invisible at this point.
            var _ = this.Handle;

            if (target == null)
                return; // first run or invalid saved pos - keep primary default

            // Defer the move to OnShown instead of doing it here. Moving an
            // INVISIBLE top-level window across a DPI boundary does not
            // refresh the HWND's device context DPI - WinForms may scale
            // control sizes (via WM_DPICHANGED or our synthesize), but the
            // DC stays at the original 96 DPI for the first paint, so 10pt
            // fonts render at 13px on a 168-DPI display ("fonts too small").
            // Doing the move after the form has been shown (while still
            // user-invisible via Opacity = 0) causes Windows to refresh the
            // DC DPI, fire WM_DPICHANGED naturally, and paint fonts at the
            // correct size. See OnShown.
            _pendingMoveTarget = target;
        }

        /// <summary>
        /// If the form's cached DPI does not match the DPI of the monitor its
        /// HWND is currently on, post a WM_DPICHANGED message to the form so
        /// WinForms' built-in handler rescales controls, fonts, and bounds.
        /// No-op when DPIs already match. Retained as a defensive fallback.
        /// </summary>
        private void SynthesizeDpiChangedIfNeeded()
        {
            if (!IsHandleCreated) return;

            int monitorDpi;
            try
            {
                monitorDpi = (int)GetDpiForWindow(Handle);
            }
            catch (DllNotFoundException) { return; }
            catch (EntryPointNotFoundException) { return; }

            int formDpi = DeviceDpi > 0 ? DeviceDpi : 96;
            if (monitorDpi <= 0 || monitorDpi == formDpi)
                return;

            // Suggested new bounds: same top-left, size scaled by DPI ratio.
            float factor = (float)monitorDpi / formDpi;
            Rectangle b = Bounds;
            RECT suggested = new RECT
            {
                left = b.Left,
                top = b.Top,
                right = b.Left + (int)Math.Round(b.Width * factor),
                bottom = b.Top + (int)Math.Round(b.Height * factor)
            };

            IntPtr pRect = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(RECT)));
            try
            {
                Marshal.StructureToPtr(suggested, pRect, false);
                // wParam: HIWORD = Y DPI, LOWORD = X DPI (both == monitorDpi).
                IntPtr wParam = (IntPtr)((monitorDpi << 16) | (monitorDpi & 0xFFFF));
                SendMessage(Handle, WM_DPICHANGED, wParam, pRect);
            }
            finally
            {
                Marshal.FreeHGlobal(pRect);
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
        /// If a previous frame is still being processed, this frame is dropped (disposed) so that
        /// expensive Bahtinov work does not queue up behind the capture timer. Latest-arriving
        /// frames are intentionally preferred over a backlog of stale ones.
        /// </summary>
        private void OnImageReceived(object sender, ImageReceivedEventArgs e)
        {
            if (Interlocked.CompareExchange(ref processingFrameFlag, 1, 0) != 0)
            {
                e.Image?.Dispose();
                return;
            }

            Bitmap frame = e.Image;
            Task.Run(() =>
            {
                try
                {
                    ProcessAndDisplay(frame);
                }
                catch (Exception ex)
                {
                    // Never let exceptions from the processing path escape as unobserved task
                    // exceptions (which can tear down the process on some runtime configurations).
                    // ProcessAndDisplay owns and disposes `frame` itself, but guard against the
                    // case where it throws before the try/finally inside it runs.
                    System.Diagnostics.Debug.WriteLine("ProcessAndDisplay failed: " + ex);
                }
                finally
                {
                    Interlocked.Exchange(ref processingFrameFlag, 0);
                }
            });
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
                whatDoIDoNextToolStripMenuItem.Enabled = Properties.Settings.Default.CalibrationCompleted && !Properties.Settings.Default.NoneSelected;
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
            if (!Properties.Settings.Default.DefocusSwitch)
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
            aboutBox.StartPosition = FormStartPosition.CenterParent;
            aboutBox.TopMost = this.TopMost;
            aboutBox.ShowDialogDpiAware(this);
        }

        /// <summary>
        /// Handles the click event for the Donate menu item, showing the Donate dialog.
        /// </summary>
        private void PleaseDonateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Donate donate = new Donate();
            donate.StartPosition = FormStartPosition.CenterParent;
            donate.TopMost = this.TopMost;
            donate.ShowDialogDpiAware(this);
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
                DarkMessageBox.Show(
                    string.Format(UiText.Current.HelpOpenErrorMessageFormat, ex.Message),
                    UiText.Current.HelpOpenErrorTitle,
                    MessageBoxIcon.Error,
                    MessageBoxButtons.OK,
                    this);
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
        /// Takes ownership of <paramref name="original"/> and disposes it on exit. Downstream consumers
        /// (event handlers) borrow the bitmap for the duration of their synchronous handler and must
        /// make their own copy if they need to retain it.
        /// </summary>
        private void ProcessAndDisplay(Bitmap original)
        {
            if (original == null)
                return;

            try
            {
                if (!firstPassCompleted)
                {
                    imageType = Properties.Settings.Default.DefocusSwitch ? 2 : 1;
                }

                if (imageType == 1)
                    RunBahtinovDisplay(original);
                else if (imageType == 2)
                    RunDefocusStarDisplay(original);
            }
            finally
            {
                original.Dispose();
            }
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
                    InitializeRedFocusBox(soloRedChannel: false);
                    InitializeGreenFocusBox();
                    InitializeBlueFocusBox();

                    if(Properties.Settings.Default.CalibrationCompleted && !Properties.Settings.Default.NoneSelected)
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

                SyncAnalysisGroupBoxWidthToFocusColumn();
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

                SyncAnalysisGroupBoxWidthToFocusColumn();
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
                // Only border popup/dropdown menus, not the top menu bar.
                if (!(e.ToolStrip is ToolStripDropDown))
                    return;
                Rectangle r = new Rectangle(0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
                using (var pen = new Pen(UITheme.MenuBorderColor)) // add this in UITheme
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

                        using (var pen = new Pen(UITheme.Black))
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

            /// <summary>
            /// Applies themed arrow color for submenu indicators.
            /// </summary>
            protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
            {
                e.ArrowColor = UITheme.MenuDarkForeground;
                base.OnRenderArrow(e);
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
            LanguageLoader.LoadFromPreference(
                AppDomain.CurrentDomain.BaseDirectory,
                model,
                Properties.Settings.Default.LanguagePreference,
                out _);

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
                    dlg.ShowDialogDpiAware(this);
                }
                // enable "what should i do next" menu item if we have been calibrated
                menuStrip1.Items[4].Enabled = Properties.Settings.Default.CalibrationCompleted && !Properties.Settings.Default.NoneSelected;
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
            settingsDialog.StartPosition = FormStartPosition.CenterParent;
            settingsDialog.TopMost = this.TopMost;
            DialogResult result = settingsDialog.ShowDialogDpiAware(this);

            if (result == DialogResult.OK)
            {
                bahtinovProcessing.LoadSettings();
                voiceControl.LoadSettings();
                menuStrip1.Items[4].Enabled = Properties.Settings.Default.CalibrationCompleted && !Properties.Settings.Default.NoneSelected;

                if (Properties.Settings.Default.KeepOnTop == true)
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

            // Anchor must be Top | Left (not None). AnchorStyles.None in WinForms means
            // "float with the center of the parent" — each parent resize shifts X by half the
            // parent-width delta, producing a visible jump on DPI changes and form resizes.
            // RepositionCalibrationComponent is the sole authority for this panel's bounds.
            //
            // The panel is created invisible to suppress a brief flash at (0,0) between
            // Controls.Add and the first reposition.
            calibrationComponent = new CalibrationComponent(this)
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Visible = false
            };

            this.Controls.Add(calibrationComponent);
            RepositionCalibrationComponent();
            calibrationComponent.BringToFront();
            calibrationComponent.Visible = true;
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
            menuStrip1.Items[4].Enabled = Properties.Settings.Default.CalibrationCompleted && !Properties.Settings.Default.NoneSelected;
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
                DialogResult result = dlg.ShowDialogDpiAware(owner);

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