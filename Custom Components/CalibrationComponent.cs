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
        private readonly float[] current = new float[3] { float.NaN, float.NaN, float.NaN };
        private readonly float[] baseline = new float[3] { 0f, 0f, 0f };
        private bool baselineCaptured;
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

            // The designer assigns Anchor = T|B|L|R and Bottom|Right to these two children,
            // but we drive their bounds manually from OnResize (see LayoutInnerControls).
            // Pin Anchor to Top|Left so WinForms' default layout engine leaves their Size
            // and Location alone and does not race our manual layout during DPI changes.
            titledRoundedRichTextBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            quitButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            this.parent = parent;

            aggregationTimer = new Timer
            {
                Interval = 500
            };

            aggregationTimer.Tick += AggregationTimer_Tick;

            SetupUI();
            SubscribeToEvents();
            ResetCalibration();
            LayoutInnerControls();
        }

        #endregion

        #region Setup and State

        /// <summary>
        /// Sets and updates the instruction text.
        /// Scales pixel-based sizes/positions using the current DPI so layout stays consistent.
        /// </summary>
        private void SetupUI()
        {
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
        /// Re-applies instruction text and UI strings after the hosting monitor's DPI changes.
        /// WinForms scales the base RichTextBox font for the new DPI, but the inline
        /// <see cref="RichTextBox.SelectionFont"/> values (bold runs), selection bullets, indents,
        /// and alignments were captured at the previous DPI and would otherwise render with
        /// mismatched metrics — which visually manifests as the calibration text collapsing
        /// or disappearing. Regenerating the content here keeps all runs at the current DPI.
        /// </summary>
        /// <param name="e">The DPI-changed event data.</param>
        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);

            SetupUI();

            if (state == CalibrationState.Complete)
                quitButton.Text = NextStepText.Current.CalibrationCloseButtonText;

            UpdateInstructionText();

            // Ensure sibling focus-channel group boxes on the parent form are not left behind
            // the calibration panel after the form rescales.
            if (parent != null && !parent.IsDisposed)
                parent.BeginInvoke(new Action(parent.RefreshFocusChannelsAfterDpiChange));
        }

        /// <summary>
        /// Prevents the framework's DPI scaling pass from mutating our outer bounds.
        ///
        /// WinForms' <see cref="Form.OnDpiChanged(DpiChangedEventArgs)"/> base implementation
        /// walks the control tree and unconditionally calls <see cref="Control.ScaleControl"/>
        /// on every child (independent of <see cref="Control.AutoScaleMode"/>). For this
        /// dynamically-positioned panel that produced a visible mid-DPI-change flicker: after
        /// <see cref="Form1.RepositionCalibrationComponent"/> had set the correct bounds,
        /// WinForms' subsequent Scale pass shrank or grew the panel by the inverse factor
        /// (<c>oldDpi/newDpi</c>) until the final explicit reposition at the end of
        /// <see cref="Form1.OnDpiChanged(DpiChangedEventArgs)"/> corrected it.
        ///
        /// We skip scaling *our own* Size/Location here. Child scaling is additionally
        /// suppressed via <see cref="ScaleChildren"/>.
        /// </summary>
        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            base.ScaleControl(factor, BoundsSpecified.None);
        }

        /// <summary>
        /// Skip recursive DPI scaling of this control's children. See
        /// <see cref="OnResize"/> for how child bounds are instead computed explicitly.
        /// </summary>
        protected override bool ScaleChildren
        {
            get { return false; }
        }

        #region Child layout (replaces Anchor)
        //
        // Why this is done by hand instead of using Anchor:
        //
        // Originally the two children (titledRoundedRichTextBox1, quitButton) relied on
        // WinForms Anchor to track the parent's size. That works fine for ordinary resizes
        // (form drag, Tri-Bahtinov mode switching) but breaks on a monitor-to-monitor DPI
        // change: during Form1.base.OnDpiChanged, WinForms opens a LayoutTransaction that
        // swallows the PerformLayout triggered by Form1.RepositionCalibrationComponent's
        // SuspendLayout/ResumeLayout, and the Scale pass that runs inside the transaction
        // then marks the children's layout as "up-to-date" so the deferred PerformLayout at
        // the end of the transaction does nothing. The result was the outer CalibrationComponent
        // growing from 350x454 (96 DPI) to 613x846 (168 DPI) but the children staying at their
        // 96-DPI size, leaving a large empty band inside the panel.
        //
        // Doing the layout explicitly in OnResize sidesteps all of that — every SetBounds on
        // the CalibrationComponent calls back in here, directly.

        // Design-time layout at 96 DPI (from CalibrationComponent.Designer.cs):
        //   CalibrationComponent.Size = (308, 624)
        //   titledRoundedRichTextBox1 at (4, 22, 300, 526), Anchor T|B|L|R
        //     => distances: left=4, top=22, right=4, bottom=76
        //   quitButton at (55, 562, 161, 42), Anchor Bottom|Right
        //     => right distance=92, bottom distance=20, size=(161, 42)
        private const int TitledLeftAt96 = 4;
        private const int TitledTopAt96 = 22;
        private const int TitledRightDistAt96 = 4;
        private const int TitledBottomDistAt96 = 76;

        private const int QuitRightDistAt96 = 92;
        private const int QuitBottomDistAt96 = 20;
        private const int QuitWidthAt96 = 161;
        private const int QuitHeightAt96 = 42;

        private int ScaleByDpi(int logicalPixels)
        {
            return (int)Math.Round(logicalPixels * (DeviceDpi / 96f), MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Recomputes bounds for the two inner controls based on the current outer size and
        /// the current DPI, replacing the Anchor-based layout that WinForms does not apply
        /// reliably inside a DPI-change LayoutTransaction.
        /// </summary>
        private void LayoutInnerControls()
        {
            if (titledRoundedRichTextBox1 == null || quitButton == null)
                return;

            int titledLeft = ScaleByDpi(TitledLeftAt96);
            int titledTop = ScaleByDpi(TitledTopAt96);
            int titledRightDist = ScaleByDpi(TitledRightDistAt96);
            int titledBottomDist = ScaleByDpi(TitledBottomDistAt96);

            int titledWidth = Math.Max(0, Width - titledLeft - titledRightDist);
            int titledHeight = Math.Max(0, Height - titledTop - titledBottomDist);

            int quitRightDist = ScaleByDpi(QuitRightDistAt96);
            int quitBottomDist = ScaleByDpi(QuitBottomDistAt96);
            int quitWidth = ScaleByDpi(QuitWidthAt96);
            int quitHeight = ScaleByDpi(QuitHeightAt96);

            int quitX = Math.Max(0, Width - quitRightDist - quitWidth);
            int quitY = Math.Max(0, Height - quitBottomDist - quitHeight);

            titledRoundedRichTextBox1.SetBounds(titledLeft, titledTop, titledWidth, titledHeight);
            quitButton.SetBounds(quitX, quitY, quitWidth, quitHeight);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutInnerControls();
        }

        #endregion

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

            if (id == 1) seenGreen = true;
            if (id == 2) seenBlue = true;

            if (!e.FocusData.ClearDisplay)
                current[id] = (float)e.FocusData.BahtinovOffset;
            else
                current[id] = float.NaN;

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

                bool allExceeded =
                    Math.Abs(d0) >= threshold &&
                    Math.Abs(d1) >= threshold &&
                    Math.Abs(d2) >= threshold;

                if (!allExceeded)
                    return false;

                bool allPositive = (d0 > 0f) && (d1 > 0f) && (d2 > 0f);
                bool allNegative = (d0 < 0f) && (d1 < 0f) && (d2 < 0f);

                if (!allPositive && !allNegative)
                    return false;

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

            ImageCapture.ForceImageUpdate();

            return true;
        }

        #endregion

        #region UI Rendering

        /// <summary>
        /// Applies bold formatting to the final non-empty line in a rich text box.
        /// </summary>
        private static void BoldLastLine(RichTextBox rtb)
        {
            if (rtb == null || rtb.TextLength == 0)
                return;

            int savedStart = rtb.SelectionStart;
            int savedLength = rtb.SelectionLength;

            int end = rtb.TextLength - 1;
            while (end >= 0 && (rtb.Text[end] == '\n' || rtb.Text[end] == '\r'))
                end--;

            if (end < 0)
                return;

            int start = rtb.Text.LastIndexOf('\n', end);
            start = (start == -1) ? 0 : start + 1;

            int length = end - start + 1;

            rtb.Select(start, length);
            Font f = rtb.SelectionFont ?? rtb.Font;
            rtb.SelectionFont = new Font(f, f.Style | FontStyle.Bold);

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
