using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    public partial class FocusChannelComponent : UserControl
    {
        #region Fields

        [DllImport("user32.dll")]
        public static extern bool SetProcessDPIAware();

        public delegate void ChannelSelectEventHandler(object sender, ChannelSelectEventArgs e);
        public static event ChannelSelectEventHandler ChannelSelectDataEvent;

        private static int focusChannelCount = 0;
        private static bool[] mouseOver = new bool[3] { false, false, false };

        private bool disposed = false; // To detect redundant calls
        private int groupID;
        private Color insideCriticalFocusColor;
        private Font labelFont;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="FocusChannelComponent"/> class.
        /// </summary>
        /// <param name="groupID">The group ID for the component.</param>
        public FocusChannelComponent(int groupID)
        {
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();

            this.groupID = groupID;
            insideCriticalFocusColor = UITheme.GetGroupBoxTextColor(groupID);

            SetProcessDPIAware();
            InitializeComponent();

            this.AutoScaleMode = AutoScaleMode.None;


            // Get the current DPI of the display
            using (Graphics g = this.CreateGraphics())
            {
                float dpi = g.DpiX; // Horizontal DPI (DpiY can also be used)

                // Set the base font size in points (e.g., 12 points)
                float baseFontSize = 12.0f;

                // Calculate the font size based on DPI (assuming 96 DPI as standard)
                float scaledFontSize = baseFontSize * 96f / dpi;

                // Apply the scaled font to the form or controls
                this.labelFont = new Font(this.Font.FontFamily, scaledFontSize);
            }

            this.groupBox1.Font = labelFont;

            SetLabelProperties(labelFont);
            ApplyTheme();
            SubscribeToEvents();

            groupBox1.Text = $"Channel {groupID + 1}";
            groupBox1.Tag = groupID;

            focusChannelCount++;
        }

        #endregion

        #region Event Subscription

        /// <summary>
        /// Subscribes to necessary events for the component.
        /// </summary>
        private void SubscribeToEvents()
        {
            BahtinovProcessing.FocusDataEvent += FocusDataEvent;
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
            BahtinovProcessing.FocusDataEvent -= FocusDataEvent;
            groupBox1.MouseEnter -= FocusChannelGroupBox_MouseEnter;
            groupBox1.MouseLeave -= FocusChannelGroupBox_MouseLeave;

            foreach (var label in GetAllLabels())
            {
                label.Paint -= Label_Paint;
            }
        }

        #endregion

        #region Label Setup

        /// <summary>
        /// Initializes the properties of the labels.
        /// </summary>
        /// <param name="newFont">The new font to apply to the labels.</param>
        private void SetLabelProperties(Font newFont)
        {
            DisableLabels();
            SetupLabelLocation();
            SetLabelFont(newFont);
            SetLabelTextAlignment();
            SetLabelTextDirection();
            SetLabelAutoSize(false);
            SetLabelText();
            offsetBarControl1.Maximum = 2.0f;
            offsetBarControl1.Minimum = -2.0f;
        }

        /// <summary>
        /// Sets up the location of various labels in the form. 
        /// The labels are organized into three columns: first column, second column, and third column.
        /// </summary>
        private void SetupLabelLocation()
        {
            // First column: Positioning labels in the first column
            label1.Location = new Point(9, 26);
            label5.Location = new Point(9, 53);

            // Second column: Positioning labels in the second column
            FocusErrorLabel.Location = new Point(141, 24);
            WithinCriticalFocusLabel.Location = new Point(175, 54);

            // Third column: Positioning labels in the third column
            label2.Location = new Point(177, 28);
        }

        /// <summary>
        /// Sets the font for all labels.
        /// </summary>
        /// <param name="newFont">The new font to apply to the labels.</param>
        private void SetLabelFont(Font newFont)
        {
            foreach (var label in GetAllLabels())
            {
                label.Font = newFont;
            }
        }

        /// <summary>
        /// Sets the initial text of the labels.
        /// </summary>
        private void SetLabelText()
        {
            FocusErrorLabel.Text = "0.0";
            WithinCriticalFocusLabel.Text = "---";
        }

        /// <summary>
        /// Sets the text alignment of the labels.
        /// </summary>
        private void SetLabelTextAlignment()
        {
            foreach (var label in GetAllLabels())
            {
                // Check if the current label is WithinCriticalFocusLabel
                if (label == WithinCriticalFocusLabel)
                {
                    label.TextAlign = ContentAlignment.MiddleLeft;
                }
                else
                {
                    label.TextAlign = ContentAlignment.MiddleRight;
                }
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
            WithinCriticalFocusLabel.AutoSize = true;
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

        #endregion

        #region Theme Application

        /// <summary>
        /// Applies the theme to the component.
        /// </summary>
        private void ApplyTheme()
        {
            groupBox1.ForeColor = UITheme.GetGroupBoxTextColor(groupID);
        }

        #endregion

        #region Label Paint Event

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

                if (label.Name == "WithinCriticalFocusLabel")
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

        #endregion

        #region Focus Data Handling

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

            if (e.FocusData.Id == groupID || e.FocusData.Id == -1)
            {
                if (!e.FocusData.ClearDisplay)
                {
                    FocusErrorLabel.Text = e.FocusData.BahtinovOffset.ToString("F1");
                    offsetBarControl1.Value = (float)e.FocusData.BahtinovOffset;    
                    WithinCriticalFocusLabel.Text = e.FocusData.InsideFocus ? "YES" : "NO";
                    offsetBarControl1.MarkerColor = e.FocusData.InsideFocus ? Color.Green : Color.Red;
                    insideCriticalFocusColor = e.FocusData.InsideFocus ? UITheme.GetGroupBoxTextColor(groupID) : UITheme.GetGroupBoxCriticalColor;
                }
                else
                {
                    insideCriticalFocusColor = UITheme.GetGroupBoxTextColor(groupID);
                    SetLabelText();
                }
            }
        }

        #endregion

        #region GroupBox Mouse Events

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

        #endregion

        #region Disposal

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">If set to <c>true</c>, release both managed and unmanaged resources; otherwise, only release unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    components?.Dispose();
                    UnsubscribeToEvents();

                    if(focusChannelCount > 0)
                        focusChannelCount--;
                }

                disposed = true;
                base.Dispose(disposing);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets all labels in the control.
        /// </summary>
        /// <returns>An array of labels.</returns>
        private Label[] GetAllLabels()
        {
            return new Label[] { label1, label2, label5, FocusErrorLabel, WithinCriticalFocusLabel };
        }

        #endregion
    }
}
