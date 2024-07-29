using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    public partial class FocusChannelComponent : UserControl
    {
        [DllImport("user32.dll")]
        public static extern bool SetProcessDPIAware();

        public delegate void ChannelSelectEventHandler(object sender, ChannelSelectEventArgs e);
        public static event ChannelSelectEventHandler ChannelSelectDataEvent;

        private static int focusChannelCount = 0;
        private static bool[] mouseOver = new bool[3] { false, false, false };

        private bool disposed = false; // To detect redundant calls
        private int groupID;
        public bool IsHighlighted { get; private set; } = false;
        private Color insideCriticalFocusColor;

        public FocusChannelComponent()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FocusChannelComponent"/> class.
        /// </summary>
        /// <param name="groupID">The group ID for the component.</param>
        public FocusChannelComponent(int groupID)
        {
            this.groupID = groupID;
            insideCriticalFocusColor = UITheme.GetGroupBoxTextColor(groupID);

            SetProcessDPIAware();
            InitializeComponent();
            SetLabelProperties();
            ApplyTheme();
            SubscribeToEvents();

            groupBox1.Text = $"Channel {groupID + 1}";
            groupBox1.Tag = groupID;

            focusChannelCount++;
        }

        /// <summary>
        /// Subscribes to necessary events for the component.
        /// </summary>
        private void SubscribeToEvents()
        {
            ImageProcessing.FocusDataEvent += FocusDataEvent;
            groupBox1.MouseEnter += FocusChannelGroupBox_MouseEnter;
            groupBox1.MouseLeave += FocusChannelGroupBox_MouseLeave;

            foreach (var label in GetAllLabels())
            {
                label.Paint += Label_Paint;
            }
        }

        /// <summary>
        /// Unsubscribes from events to avoid memory leaks.
        /// </summary>
        private void UnsubscribeToEvents()
        {
            ImageProcessing.FocusDataEvent -= FocusDataEvent;
            groupBox1.MouseEnter -= FocusChannelGroupBox_MouseEnter;
            groupBox1.MouseLeave -= FocusChannelGroupBox_MouseLeave;
            
            foreach (var label in GetAllLabels())
            {
                label.Paint -= Label_Paint;
            }
        }

        /// <summary>
        /// Initializes the properties of the labels.
        /// </summary>
        private void SetLabelProperties()
        {
            DisableLabels();
            SetLabelTextAlignment();
            SetLabelTextDirection();
            SetLabelAutoSize(false);
            SetLabelText();
        }

        /// <summary>
        /// Sets the initial text of the labels.
        /// </summary>
        private void SetLabelText()
        {
            FocusErrorLabel.Text = "0.0";
            AbsoluteFocusErrorLabel.Text = "0.0";
            WithinCriticalFocusLabel.Text = "---";
        }

        /// <summary>
        /// Sets the text alignment of the labels.
        /// </summary>
        private void SetLabelTextAlignment()
        {
            foreach (var label in GetAllLabels())
            {
                label.TextAlign = ContentAlignment.MiddleRight;
            }
        }

        /// <summary>
        /// Sets the text direction of the labels.
        /// </summary>
        private void SetLabelTextDirection()
        {
            foreach (var label in GetAllLabels())
            {
                label.RightToLeft = RightToLeft.No;
            }
        }

        /// <summary>
        /// Sets the auto-size property of the labels.
        /// </summary>
        /// <param name="autoSize">If set to <c>true</c>, the labels will auto-size.</param>
        private void SetLabelAutoSize(bool autoSize)
        {
            FocusErrorLabel.AutoSize = autoSize;
            AbsoluteFocusErrorLabel.AutoSize = autoSize;
            WithinCriticalFocusLabel.AutoSize = autoSize;
        }

        /// <summary>
        /// Applies the theme to the component.
        /// </summary>
        private void ApplyTheme()
        {
            groupBox1.ForeColor = UITheme.GetGroupBoxTextColor(groupID);
        }

        /// <summary>
        /// Retrieves all labels that require painting events.
        /// </summary>
        /// <returns>An array of labels.</returns>
        private Label[] GetAllLabels()
        {
            return new[]
            {
                label1, label2, label3, label4, label5,
                FocusErrorLabel, AbsoluteFocusErrorLabel, WithinCriticalFocusLabel
            };
        }

        /// <summary>
        /// Disables the labels initially.
        /// </summary>
        private void DisableLabels()
        {
            foreach (var label in GetAllLabels())
            {
                label.Enabled = false;
            }
        }

        /// <summary>
        /// Handles the focus data event to update label values.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The <see cref="FocusDataEventArgs"/> instance containing the event data.</param>
        public void FocusDataEvent(object sender, FocusDataEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object, FocusDataEventArgs>(FocusDataEvent), sender, e);
                return;
            }

            if (e.focusData.Id == groupID || e.focusData.Id == -1)
            {
                if (!e.focusData.ClearDisplay)
                {
                    FocusErrorLabel.Text = e.focusData.BahtinovOffset.ToString("F1");
                    AbsoluteFocusErrorLabel.Text = e.focusData.DefocusError.ToString("F1");
                    WithinCriticalFocusLabel.Text = e.focusData.InsideFocus ? "YES" : "NO";
                    insideCriticalFocusColor = e.focusData.InsideFocus ? UITheme.GetGroupBoxTextColor(groupID) : Color.Red;
                }
                else
                {
                    insideCriticalFocusColor = UITheme.GetGroupBoxTextColor(groupID);
                    SetLabelText();
                }
            }
        }

        /// <summary>
        /// Custom paint event handler for labels.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
        private void Label_Paint(object sender, PaintEventArgs e)
        {
            if (sender is Label label && label.Parent is GroupBox parentGroupBox && parentGroupBox.Tag is int groupId)
            {
                Color textColor = UITheme.GetGroupBoxTextColor(groupId);
                TextFormatFlags flags = TextFormatFlags.Default;

                // Check label name to set text format flags
                if (label.Name == "AbsoluteFocusErrorLabel" ||
                    label.Name == "FocusErrorLabel" ||
                    label.Name == "WithinCriticalFocusLabel")
                {
                    flags = TextFormatFlags.VerticalCenter | TextFormatFlags.Right;
                }

                if(label.Name == "WithinCriticalFocusLabel")
                {
                    TextRenderer.DrawText(e.Graphics, label.Text, label.Font, label.ClientRectangle, insideCriticalFocusColor, flags);
                }
                else
                {
                    // Draw the text with the specified color and format
                    TextRenderer.DrawText(e.Graphics, label.Text, label.Font, label.ClientRectangle, textColor, flags);
                }

            }
        }

        /// <summary>
        /// Handles the MouseEnter event for the group box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void FocusChannelGroupBox_MouseEnter(object sender, EventArgs e)
        {
            groupBox1.BackColor = UITheme.GetGroupBoxBackgroundColor(groupID);
            mouseOver[groupID] = true;
            ChannelSelectDataEvent?.Invoke(null, new ChannelSelectEventArgs(mouseOver, focusChannelCount));
        }

        /// <summary>
        /// Handles the MouseLeave event for the group box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void FocusChannelGroupBox_MouseLeave(object sender, EventArgs e)
        {
            groupBox1.BackColor = UITheme.DarkBackground;
            mouseOver[groupID] = false;
            ChannelSelectDataEvent?.Invoke(null, new ChannelSelectEventArgs(mouseOver, focusChannelCount));
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">If set to <c>true</c> release both managed and unmanaged resources; otherwise, only release unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                UnsubscribeToEvents();
                focusChannelCount--;
                mouseOver[groupID] = false;
                components?.Dispose();
            }

            base.Dispose(disposing);
            disposed = true;
        }
    }
}
