using SkyCal.Custom_Components;
using System;
using System.Drawing;
using System.Windows.Forms;

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

            SetupUI();
            SubscribeToEvents();

            ResetCalibration();
        }

        /// <summary>
        /// Sets fonts and theme styling, then updates the instruction text.
        /// </summary>
        private void SetupUI()
        {
            float increasedSize = this.Font.Size + 2.0f;
            Font newFont = new Font(this.Font.FontFamily, increasedSize, this.Font.Style);

            // Adjust fonts
            roundedPanel1.Font = newFont;
            quitButton.Font = newFont;
            titledRoundedRichTextBox1.Font = newFont;
            titledRoundedRichTextBox1.TitleFont = new Font(this.Font.FontFamily, increasedSize + 2.0f, FontStyle.Bold);

            roundedPanel1.BackColor = UITheme.DarkBackground;
            roundedPanel1.ForeColor = UITheme.MenuHighlightBackground;

            // Cancel Button Styling
            quitButton.BackColor = UITheme.ButtonDarkBackground;
            quitButton.ForeColor = UITheme.ButtonDarkForeground;
            quitButton.FlatStyle = FlatStyle.Popup;
            quitButton.CornerRadius = 6;
            quitButton.TextOffsetX = 0;
            quitButton.BevelDark = Color.FromArgb(180, 90, 90, 90);
            quitButton.BevelLight = Color.FromArgb(220, 160, 160, 160);

            // RichTextBox Styling
            titledRoundedRichTextBox1.BackColor = UITheme.ButtonDarkBackground;
            titledRoundedRichTextBox1.ForeColor = Color.White;
            titledRoundedRichTextBox1.TitleForeColor = Color.White;
            titledRoundedRichTextBox1.TitleBackColor = UITheme.DarkBackground;
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
            Properties.Settings.Default.Save();

            // Capture new image to display changed sign.
            ImageCapture.ForceImageUpdate();

            return true;
        }

        /// <summary>
        /// Updates the instruction text displayed in the titled rounded rich text box.
        /// </summary>
        private void UpdateInstructionText()
        {
            string mode = IsTriBahtinov ? "Tri-Bahtinov (3 values)" : "Bahtinov (1 value)";
            string header = "This tells SkyCal which way the focus values move when you turn the focuser.";

            if (_state == CalibrationState.WaitingForFirstValidRead)
            { 
                var rtb = titledRoundedRichTextBox1.InnerRichTextBox;

                rtb.Clear();

                rtb.AppendText(header + "\r\n\r\n Step 1 - Setup\r\n");

                rtb.SelectionBullet = true;
                rtb.SelectionIndent = 20;
                rtb.SelectionHangingIndent = 10;

                rtb.AppendText("Install a Bahtinov focus mask or Tri-Bahtinov collimation mask.\n"); 
                rtb.AppendText("Capture a star image.\r\n");

                rtb.SelectionBullet = false;

                rtb.AppendText("\nWaiting for a valid Bahtinov image...\r\n");

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

                var rtb = titledRoundedRichTextBox1.InnerRichTextBox;

                rtb.Clear();


                rtb.AppendText("Step 2 - Moving the focuser.\r\n\n");

                rtb.AppendText("If you telescope focuses by moving the primary mirror:\n");

                rtb.SelectionBullet = true;
                rtb.SelectionIndent = 20;
                rtb.SelectionHangingIndent = 10;

                rtb.AppendText("Turn the focuser so that the primary mirror moves IN toward the secondary mirror.\n");

                rtb.SelectionBullet = false;
                rtb.SelectionIndent = 0;
                rtb.SelectionHangingIndent = 0;
                rtb.AppendText("\n If your telescope focuses by moving the camera (for example, a Crayford focuser):\n");

                rtb.SelectionBullet = true;
                rtb.SelectionIndent = 20;
                rtb.SelectionHangingIndent = 10;

                rtb.AppendText("Turn the focuser to move the camera IN towards the telescope body.\n");

                rtb.SelectionBullet = false;
                rtb.SelectionIndent = 0;
                rtb.SelectionHangingIndent = 0;

                rtb.AppendText("\nCalibration will complete automatically once the change exceeds " + CompletionThresholdPixels.ToString("0.0") + " px.\r\n\r\n");

                rtb.AppendText("Measuring direction...\r\n");
                rtb.AppendText("Mode: " + mode);

                return;
            }

            // Complete
            titledRoundedRichTextBox1.Text =
                "Calibration complete.\r\n\r\n" +
                "The SignSwitch setting has been updated and saved.\r\n" +
                "You can now continue with normal focus/collimation.\r\n\r\n";
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
