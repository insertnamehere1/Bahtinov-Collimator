using Bahtinov_Collimator.Properties;
using SkyCal.Custom_Components;
using System;
using System.Drawing;
using System.Security.Policy;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace Bahtinov_Collimator.Custom_Components
{
    /// <summary>
    /// Displays a guided calibration workflow that determines whether moving the primary mirror
    /// toward the secondary causes the measured Bahtinov/Tri-Bahtinov offset(s) to increase or decrease.
    ///
    /// Result:
    /// - If the measured change is negative beyond a threshold, SignSwitch is set to false.
    /// - If the measured change is positive beyond a threshold, SignSwitch is set to true.
    /// </summary>
    public partial class CalibrationComponent : UserControl
    {
        private const float CompletionThresholdPixels = 0.5f;

        private readonly Form1 _parent;

        // Latest measured offsets for (Red, Green, Blue) ids: 0..2
        private readonly float[] _current = new float[3] { float.NaN, float.NaN, float.NaN };

        // Baseline offsets captured once we have enough data
        private readonly float[] _baseline = new float[3] { 0f, 0f, 0f };
        private bool _baselineCaptured;

        // Have we ever seen channels 1/2 update (Tri-Bahtinov indicator)
        private bool _seenGreen;
        private bool _seenBlue;

        private readonly Timer _aggregationTimer;
        private bool _aggregationActive;

        private CalibrationState _state = CalibrationState.WaitingForFirstValidRead;

        private readonly Font labelFont;

        private enum CalibrationState
        {
            WaitingForFirstValidRead,
            CaptureBaseline,
            AskUserToMoveMirror,
            MeasuringDirection,
            Complete
        }

        /// <summary>
        /// Creates a new calibration component hosted on the main form.
        /// </summary>
        /// <param name="parent">Owning main form.</param>
        public CalibrationComponent(Form1 parent)
        {
            InitializeComponent();

            _parent = parent;

            _aggregationTimer = new Timer
            {
                Interval = 1000 // 1 second
            };
            _aggregationTimer.Tick += AggregationTimer_Tick;

            this.AutoScaleMode = AutoScaleMode.None;

            // Get the current DPI of the display
            using (Graphics g = this.CreateGraphics())
            {
                float dpi = g.DpiX; // Horizontal DPI (DpiY can also be used)

                // Set the base font size in points (e.g., 12 points)
                float baseFontSize = 10.0f;

                // Apply the scaled font to the form or controls
                this.labelFont = new Font(this.Font.FontFamily, baseFontSize);
            }

            SetupUI();
            SubscribeToEvents();
            ResetCalibration();
        }

        /// <summary>
        /// Sets fonts and theme styling, then updates the instruction text.
        /// Scales pixel-based sizes/positions using the current DPI so layout stays consistent.
        /// </summary>
        private void SetupUI()
        {
            roundedPanel1.Font = labelFont;
            quitButton.Font = labelFont;
            titledRoundedRichTextBox1.Font = labelFont;

            var titleFont = new Font(this.Font.FontFamily, 12.0f);

            titledRoundedRichTextBox1.TitleFont = titleFont;
            titledRoundedRichTextBox1.TitlePaddingLeft = 140;

            // Scale form size (these were originally designed for 96 DPI).
            this.ClientSize = new Size(390, 624);

            // Bottom panel
            roundedPanel1.Size = new Size(390, 83);
            roundedPanel1.Location = new Point(0, 541);
            roundedPanel1.BackColor = UITheme.DarkBackground;
            roundedPanel1.ForeColor = UITheme.MenuHighlightBackground;

            // Quit button
            quitButton.Size = new Size(160, 44);
            quitButton.Location = new Point(116, 20);
            quitButton.BackColor = UITheme.ButtonDarkBackground;
            quitButton.ForeColor = UITheme.ButtonDarkForeground;
            quitButton.FlatStyle = FlatStyle.Popup;
            quitButton.CornerRadius = 8;
            quitButton.TextOffsetX = 0;
            quitButton.BevelDark = Color.FromArgb(180, 90, 90, 90);
            quitButton.BevelLight = Color.FromArgb(220, 160, 160, 160);

            // RichText box container
            titledRoundedRichTextBox1.Size = new Size(384, 506);
            titledRoundedRichTextBox1.Location = new Point(3, 23);
            titledRoundedRichTextBox1.BackColor = UITheme.MenuDarkBackground;
            titledRoundedRichTextBox1.ForeColor = Color.White;
            titledRoundedRichTextBox1.TitleForeColor = Color.White;
            titledRoundedRichTextBox1.TitleBackColor = UITheme.ButtonDarkBackground;
            titledRoundedRichTextBox1.TitleText = "Calibration";
        }

        /// <summary>
        /// Subscribes to focus data updates required for calibration.
        /// </summary>
        private void SubscribeToEvents()
        {
            BahtinovProcessing.FocusDataEvent += FocusDataEvent;
        }

        /// <summary>
        /// Unsubscribes from events to avoid leaks when closing the component.
        /// </summary>
        private void UnsubscribeToEvents()
        {
            BahtinovProcessing.FocusDataEvent -= FocusDataEvent;
        }

        /// <summary>
        /// Resets internal state for a new calibration run.
        /// </summary>
        private void ResetCalibration()
        {
            _current[0] = _current[1] = _current[2] = float.NaN;
            _baseline[0] = _baseline[1] = _baseline[2] = 0f;

            _baselineCaptured = false;
            _seenGreen = false;
            _seenBlue = false;

            _state = CalibrationState.WaitingForFirstValidRead;

            ImageCapture.ForceImageUpdate();
            UpdateInstructionText();
        }

        /// <summary>
        /// Returns true if we have evidence of Tri-Bahtinov (channels 1 or 2 producing data).
        /// Otherwise, treat as Bahtinov (channel 0 only).
        /// </summary>
        private bool IsTriBahtinov
        {
            get { return _seenGreen || _seenBlue; }
        }

        /// <summary>
        /// Returns true if we have enough readings to proceed:
        /// - Tri-Bahtinov: all three channels must be non-zero
        /// - Bahtinov: only channel 0 must be non-zero
        /// </summary>
        private bool HasEnoughDataToBaseline()
        {
            if (IsTriBahtinov)
                return _current[0] != float.NaN && _current[1] != float.NaN && _current[2] != float.NaN;

            return _current[0] != float.NaN;
        }
        /// <summary>
        /// Handles incoming focus data updates. Aggregates channel updates over a short
        /// time window so calibration state advances only once per measurement cycle.
        /// </summary>
        public void FocusDataEvent(object sender, FocusDataEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object, FocusDataEventArgs>(FocusDataEvent), sender, e);
                return;
            }

            if (e == null || e.FocusData == null)
                return;

            int id = e.FocusData.Id;
            if (id < 0 || id > 2)
                return;

            // Track whether channels 1/2 ever appear (Tri-Bahtinov detection)
            if (id == 1) _seenGreen = true;
            if (id == 2) _seenBlue = true;

            // Update current values
            if (!e.FocusData.ClearDisplay)
                _current[id] = (float)e.FocusData.BahtinovOffset;
            else
                _current[id] = float.NaN;

            // Start aggregation window on first received channel
            if (!_aggregationActive)
            {
                _aggregationActive = true;
                _aggregationTimer.Stop();
                _aggregationTimer.Start();
            }
        }

        /// <summary>
        /// Called after the aggregation window expires. Advances calibration state once
        /// using whatever channels were received during the window.
        /// </summary>
        private void AggregationTimer_Tick(object sender, EventArgs e)
        {
            _aggregationTimer.Stop();
            _aggregationActive = false;

            AdvanceCalibrationState();
        }

        /// <summary>
        /// Advances the calibration state machine based on current readings.
        /// </summary>
        private void AdvanceCalibrationState()
        {
            if (_state == CalibrationState.Complete)
            {
                UpdateInstructionText();
                return;
            }

            // If the display is cleared (or we lose required channels), fall back to waiting state.
            if (!HasEnoughDataToBaseline())
            {
                _state = CalibrationState.WaitingForFirstValidRead;
                _baselineCaptured = false;
                UpdateInstructionText();

                return;
            }

            if (_state == CalibrationState.WaitingForFirstValidRead)
            {
                _state = CalibrationState.CaptureBaseline;
                UpdateInstructionText();
            }

            if (_state == CalibrationState.CaptureBaseline)
            {
                CaptureBaselineIfNeeded();
                _state = CalibrationState.AskUserToMoveMirror;
                UpdateInstructionText();
            }

            if (_state == CalibrationState.AskUserToMoveMirror)
            {
                // As soon as new measurements start changing, we can begin measuring direction.
                _state = CalibrationState.MeasuringDirection;
                UpdateInstructionText();
            }

            if (_state == CalibrationState.MeasuringDirection)
            {
                if (TryCompleteCalibration())
                {
                    _state = CalibrationState.Complete;
                    UpdateInstructionText();
                }
            }
        }

        /// <summary>
        /// Captures baseline values from the current readings (once per run).
        /// </summary>
        private void CaptureBaselineIfNeeded()
        {
            if (_baselineCaptured)
                return;

            _baseline[0] = _current[0];
            _baseline[1] = _current[1];
            _baseline[2] = _current[2];

            _baselineCaptured = true;
        }

        /// <summary>
        /// Computes the signed change from baseline and completes calibration if it exceeds the threshold.
        /// Tri-Bahtinov uses the sum of channels. Bahtinov uses channel 0 only.
        /// </summary>
        /// <returns>True if calibration completed and setting was written.</returns>
        private bool TryCompleteCalibration()
        {
            if (!_baselineCaptured)
                return false;

            float threshold = CompletionThresholdPixels;

            bool signSwitch;

            if (IsTriBahtinov)
            {
                float d0 = _current[0] - _baseline[0];
                float d1 = _current[1] - _baseline[1];
                float d2 = _current[2] - _baseline[2];

                // All channels must exceed the threshold in magnitude
                bool allExceeded =
                    Math.Abs(d0) >= threshold &&
                    Math.Abs(d1) >= threshold &&
                    Math.Abs(d2) >= threshold;

                if (!allExceeded)
                    return false;

                // All channels must move in the same direction
                bool allPositive = (d0 > 0f) && (d1 > 0f) && (d2 > 0f);
                bool allNegative = (d0 < 0f) && (d1 < 0f) && (d2 < 0f);

                if (!allPositive && !allNegative)
                    return false;

                // Rule:
                // Negative movement = correct direction, calibration complete
                // Positive movement = wrong direction, sign switch required
                signSwitch = allPositive;
            }
            else
            {
                float delta = _current[0] - _baseline[0];

                if (Math.Abs(delta) < threshold)
                    return false;

                signSwitch = (delta > 0f);
            }
  
            if(signSwitch)
                Properties.Settings.Default.SignChange = !Properties.Settings.Default.SignChange;
            
            Properties.Settings.Default.CalibrationCompleted = true;
            Properties.Settings.Default.ShowStartupCalibrationPrompt = false;
            Properties.Settings.Default.Save();

            quitButton.Text = "Close";

            // Capture new image to display changed sign.
            ImageCapture.ForceImageUpdate();

            return true;
        }

        private static void BoldLastLine(RichTextBox rtb)
        {
            if (rtb == null || rtb.TextLength == 0)
                return;

            // Save current selection/caret
            int savedStart = rtb.SelectionStart;
            int savedLength = rtb.SelectionLength;

            // Ignore trailing newlines
            int end = rtb.TextLength - 1;
            while (end >= 0 && (rtb.Text[end] == '\n' || rtb.Text[end] == '\r'))
                end--;

            if (end < 0)
                return;

            // Find start of the last line
            int start = rtb.Text.LastIndexOf('\n', end);
            start = (start == -1) ? 0 : start + 1;

            int length = end - start + 1;

            // Bold just that line
            rtb.Select(start, length);
            Font f = rtb.SelectionFont ?? rtb.Font;
            rtb.SelectionFont = new Font(f, f.Style | FontStyle.Bold);

            // Restore selection/caret
            rtb.Select(savedStart, savedLength);
        }


        /// <summary>
        /// Updates the instruction text displayed in the titled rounded rich text box.
        /// </summary>
        private void UpdateInstructionText()
        {
            var rtb = titledRoundedRichTextBox1.InnerRichTextBox;

            string header1 = "\nPurpose - Calibration allows SkyCal to assist with focusing, advise on collimation screw adjustments, and enable the ‘What Should I Do Next ?’ guidance.";

            if (_state == CalibrationState.WaitingForFirstValidRead)
            {
                rtb.Clear();
            
                rtb.AppendText(header1 + "\r\n\r\n Step 1 - Setup\r\n\n");
                BoldLastLine(rtb);

                rtb.SelectionBullet = true;
                rtb.SelectionIndent = 20;
                rtb.SelectionHangingIndent = 10;

                rtb.AppendText("Install a Bahtinov focus mask or Tri-Bahtinov collimation mask.\n"); 
                rtb.AppendText("Capture a star image.\r\n");

                rtb.SelectionBullet = false;

                rtb.AppendText("\nWaiting for a valid Bahtinov image...\r\n");
                BoldLastLine(rtb);

                return;
            }

            if (_state == CalibrationState.CaptureBaseline)
            {
                return;
            }

            if (_state == CalibrationState.AskUserToMoveMirror)
            {
                return;
            }

            if (_state == CalibrationState.MeasuringDirection)
            {
                string header2 = "\nPurpose - This step shows SkyCal how focus adjustments relate to focuser direction and collimation screw guidance.";

                rtb.Clear();

                rtb.AppendText(header2 + "\r\n\r\n\nStep 2 - Moving the focuser.\r\n\n");
                BoldLastLine(rtb);

                rtb.SelectionBullet = true;
                rtb.SelectionIndent = 20;
                rtb.SelectionHangingIndent = 10;

                rtb.AppendText("If the focuser moves the primary mirror, move it INWARD, toward the secondary.\n");

                rtb.SelectionBullet = false;
                rtb.SelectionIndent = 0;
                rtb.SelectionHangingIndent = 0;

                rtb.SelectionBullet = true;
                rtb.SelectionIndent = 20;
                rtb.SelectionHangingIndent = 10;

                rtb.AppendText("If the focuser moves the camera, move it INWARD, toward the telescope body.\n");

                rtb.SelectionBullet = false;
                rtb.SelectionIndent = 0;
                rtb.SelectionHangingIndent = 0;

                rtb.AppendText("\nCalibration completes automatically once enough focus movement is detected.\r\n\r\n");

                rtb.AppendText("\nPlease move the focuser inward now....\r\n");
                BoldLastLine(rtb);

                return;
            }

            rtb.Clear();

            rtb.AppendText("\r\nCalibration complete\r\n\n");
            BoldLastLine(rtb);
            rtb.AppendText("SkyCal has finished calibration and updated its settings.\r\n\nAll changes have been saved.\r\n\n");
        }

        /// <summary>
        /// Cancels calibration and returns control to the main form.
        /// </summary>
        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (_parent != null)
            {
                UnsubscribeToEvents();
                _parent.StopCalibration();
            }
        }
    }
}
