using Bahtinov_Collimator.Properties;
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
        #region Constants and Fields

        private const float CompletionThresholdPixels = 0.5f;

        private readonly Form1 parent;

        // Latest measured offsets for (Red, Green, Blue) ids: 0..2
        private readonly float[] current = new float[3] { float.NaN, float.NaN, float.NaN };

        // Baseline offsets captured once we have enough data
        private readonly float[] baseline = new float[3] { 0f, 0f, 0f };
        private bool baselineCaptured;

        // Have we ever seen channels 1/2 update (Tri-Bahtinov indicator)
        private bool seenGreen;
        private bool seenBlue;

        private readonly Timer aggregationTimer;
        private bool aggregationActive;

        private CalibrationState state = CalibrationState.WaitingForFirstValidRead;

        private enum CalibrationState
        {
            WaitingForFirstValidRead,
            CaptureBaseline,
            AskUserToMoveMirror,
            MeasuringDirection,
            Complete
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new calibration component hosted on the main form.
        /// </summary>
        /// <param name="parent">Owning main form.</param>
        public CalibrationComponent(Form1 parent)
        {
            InitializeComponent();

            this.parent = parent;

            aggregationTimer = new Timer
            {
                Interval = 500 // half second
            };

            aggregationTimer.Tick += AggregationTimer_Tick;

            SetupUI();
            SubscribeToEvents();
            ResetCalibration();
        }

        #endregion

        #region Setup and State

        /// <summary>
        /// Sets and updates the instruction text.
        /// Scales pixel-based sizes/positions using the current DPI so layout stays consistent.
        /// </summary>
        private void SetupUI()
        {
            // internationalize text
            var textPack = NextStepText.Current;
            quitButton.Text = textPack.CalibrationQuitButtonText;
            titledRoundedRichTextBox1.TitleText = textPack.CalibrationTitle;
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
            aggregationTimer.Tick -= AggregationTimer_Tick;
            BahtinovProcessing.FocusDataEvent -= FocusDataEvent;
        }

        /// <summary>
        /// Resets internal state for a new calibration run.
        /// </summary>
        private void ResetCalibration()
        {
            current[0] = current[1] = current[2] = float.NaN;
            baseline[0] = baseline[1] = baseline[2] = 0f;

            baselineCaptured = false;
            seenGreen = false;
            seenBlue = false;

            state = CalibrationState.WaitingForFirstValidRead;

            ImageCapture.ForceImageUpdate();
            UpdateInstructionText();
        }

        /// <summary>
        /// Returns true if we have evidence of Tri-Bahtinov (channels 1 or 2 producing data).
        /// Otherwise, treat as Bahtinov (channel 0 only).
        /// </summary>
        private bool IsTriBahtinov
        {
            get { return seenGreen || seenBlue; }
        }

        /// <summary>
        /// Returns true if we have enough readings to proceed:
        /// - Tri-Bahtinov: all three channels must be non-zero
        /// - Bahtinov: only channel 0 must be non-zero
        /// </summary>
        private bool HasEnoughDataToBaseline()
        {
            if (IsTriBahtinov)
                return !float.IsNaN(current[0]) && !float.IsNaN(current[1]) && !float.IsNaN(current[2]);

            return !float.IsNaN(current[0]);
        }

        #endregion

        #region Event Handling
        /// <summary>
        /// Handles incoming focus data updates. Aggregates channel updates over a short
        /// time window so calibration state advances only once per measurement cycle.
        /// </summary>
        /// <param name="sender">The event source.</param>
        /// <param name="e">Focus data payload for a single color channel update.</param>
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
            if (id == 1) seenGreen = true;
            if (id == 2) seenBlue = true;

            // Update current values
            if (!e.FocusData.ClearDisplay)
                current[id] = (float)e.FocusData.BahtinovOffset;
            else
                current[id] = float.NaN;

            // Start aggregation window on first received channel
            if (!aggregationActive)
            {
                aggregationActive = true;
                aggregationTimer.Stop();
                aggregationTimer.Start();
            }
        }

        /// <summary>
        /// Called after the aggregation window expires. Advances calibration state once
        /// using whatever channels were received during the window.
        /// </summary>
        /// <param name="sender">The timer raising the event.</param>
        /// <param name="e">Event data for the timer tick.</param>
        private void AggregationTimer_Tick(object sender, EventArgs e)
        {
            aggregationTimer.Stop();
            aggregationActive = false;

            AdvanceCalibrationState();
        }

        #endregion

        #region Calibration Logic

        /// <summary>
        /// Advances the calibration state machine based on current readings.
        /// </summary>
        private void AdvanceCalibrationState()
        {
            if (state == CalibrationState.Complete)
            {
                UpdateInstructionText();
                return;
            }

            // If the display is cleared (or we lose required channels), fall back to waiting state.
            if (!HasEnoughDataToBaseline())
            {
                state = CalibrationState.WaitingForFirstValidRead;
                baselineCaptured = false;
                UpdateInstructionText();

                return;
            }

            if (state == CalibrationState.WaitingForFirstValidRead)
            {
                state = CalibrationState.CaptureBaseline;
                UpdateInstructionText();
            }

            if (state == CalibrationState.CaptureBaseline)
            {
                CaptureBaselineIfNeeded();
                state = CalibrationState.AskUserToMoveMirror;
                UpdateInstructionText();
            }

            if (state == CalibrationState.AskUserToMoveMirror)
            {
                // As soon as new measurements start changing, we can begin measuring direction.
                state = CalibrationState.MeasuringDirection;
                UpdateInstructionText();
            }

            if (state == CalibrationState.MeasuringDirection)
            {
                if (TryCompleteCalibration())
                {
                    state = CalibrationState.Complete;
                    UpdateInstructionText();
                }
            }
        }

        /// <summary>
        /// Captures baseline values from the current readings (once per run).
        /// </summary>
        private void CaptureBaselineIfNeeded()
        {
            if (baselineCaptured)
                return;

            baseline[0] = current[0];
            baseline[1] = current[1];
            baseline[2] = current[2];

            baselineCaptured = true;
        }

        /// <summary>
        /// Computes the signed change from baseline and completes calibration if it exceeds the threshold.
        /// Tri-Bahtinov uses the sum of channels. Bahtinov uses channel 0 only.
        /// </summary>
        /// <returns>True if calibration completed and setting was written.</returns>
        private bool TryCompleteCalibration()
        {
            if (!baselineCaptured)
                return false;

            float threshold = CompletionThresholdPixels;

            bool signSwitch;

            if (IsTriBahtinov)
            {
                float d0 = current[0] - baseline[0];
                float d1 = current[1] - baseline[1];
                float d2 = current[2] - baseline[2];

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
                float delta = current[0] - baseline[0];

                if (Math.Abs(delta) < threshold)
                    return false;

                signSwitch = (delta > 0f);
            }

            if (signSwitch)
                Properties.Settings.Default.SignChange = !Properties.Settings.Default.SignChange;

            Properties.Settings.Default.CalibrationCompleted = true;
            Properties.Settings.Default.ShowStartupCalibrationPrompt = false;
            Properties.Settings.Default.Save();

            quitButton.Text = NextStepText.Current.CalibrationCloseButtonText;

            // Capture new image to display changed sign.
            ImageCapture.ForceImageUpdate();

            return true;
        }

        #endregion

        #region UI Rendering

        /// <summary>
        /// Applies bold formatting to the final non-empty line in a rich text box.
        /// </summary>
        /// <param name="rtb">The target rich text box.</param>
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
            var textPack = NextStepText.Current;

            string header1 = textPack.CalibrationObjectiveHeader;

            if (state == CalibrationState.WaitingForFirstValidRead)
            {
                rtb.Clear();

                rtb.AppendText(header1 + "\r\n\r\n" + textPack.CalibrationStep1Title + "\r\n\n");
                BoldLastLine(rtb);

                rtb.SelectionBullet = true;
                rtb.SelectionIndent = 20;
                rtb.SelectionHangingIndent = 10;

                rtb.AppendText(textPack.CalibrationStep1Bullet1 + "\n");
                rtb.AppendText(textPack.CalibrationStep1Bullet2 + "\r\n");

                rtb.SelectionBullet = false;
                rtb.SelectionIndent = 0;

                rtb.AppendText("\n" + textPack.CalibrationStep1Warning + "\r\n");

                rtb.SelectionIndent = 20;
                rtb.AppendText("\n" + textPack.CalibrationWaitingForImage + "\r\n");
                BoldLastLine(rtb);

                return;
            }

            if (state == CalibrationState.CaptureBaseline)
            {
                return;
            }

            if (state == CalibrationState.AskUserToMoveMirror)
            {
                return;
            }

            if (state == CalibrationState.MeasuringDirection)
            {
                string header2 = textPack.CalibrationAimHeader;

                rtb.Clear();

                rtb.AppendText(header2 + "\r\n\r\n" + textPack.CalibrationStep2Title + "\r\n\n");
                BoldLastLine(rtb);

                rtb.SelectionBullet = true;
                rtb.SelectionIndent = 20;
                rtb.SelectionHangingIndent = 10;

                rtb.AppendText(textPack.CalibrationStep2Bullet1 + "\n");

                rtb.SelectionBullet = false;
                rtb.SelectionIndent = 0;
                rtb.SelectionHangingIndent = 0;

                rtb.SelectionBullet = true;
                rtb.SelectionIndent = 20;
                rtb.SelectionHangingIndent = 10;

                rtb.AppendText(textPack.CalibrationStep2Bullet2 + "\n");

                rtb.SelectionBullet = false;
                rtb.SelectionIndent = 0;
                rtb.SelectionHangingIndent = 0;

                rtb.AppendText("\n" + textPack.CalibrationAutoCompleteLine + "\r\n\r\n");

                rtb.SelectionAlignment = HorizontalAlignment.Center;
                rtb.AppendText( textPack.CalibrationMovePrompt + "\r");
                BoldLastLine(rtb);
                rtb.SelectionAlignment = HorizontalAlignment.Left;
                return;
            }

            rtb.Clear();
            rtb.SelectionAlignment = HorizontalAlignment.Center;
            rtb.AppendText("\r\n" + textPack.CalibrationCompleteHeader + "\r\n\n\n\n");
            rtb.SelectionAlignment = HorizontalAlignment.Left;
            BoldLastLine(rtb);

            rtb.SelectionAlignment = HorizontalAlignment.Center;
            rtb.AppendText("\r\n" + textPack.CalibrationCompleteSummary + "\r");
            BoldLastLine(rtb);
            rtb.SelectionAlignment = HorizontalAlignment.Left;
        }

        #endregion

        #region Actions and Cleanup

        /// <summary>
        /// Cancels calibration and returns control to the main form.
        /// </summary>
        /// <param name="sender">The button that initiated cancellation.</param>
        /// <param name="e">Click event data.</param>
        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (parent != null)
            {
                aggregationTimer.Stop();
                UnsubscribeToEvents();
                parent.StopCalibration();
            }
        }

        /// <summary>
        /// Stops background timers and unsubscribes all event handlers owned by this control.
        /// </summary>
        internal void CleanupResources()
        {
            aggregationTimer.Stop();
            UnsubscribeToEvents();
        }

        #endregion
    }
}
