using System;
using System.Drawing;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    public partial class FocusChannelComponent : UserControl
    {
        #region Fields

        public delegate void ChannelSelectEventHandler(object sender, ChannelSelectEventArgs e);
        public static event ChannelSelectEventHandler ChannelSelectDataEvent;

        private static int focusChannelCount = 0;
        private static readonly bool[] mouseOver = new bool[3] { false, false, false };

        public double ErrorOffset { private set; get; } = 0.0f;

        private bool disposed = false; // To detect redundant calls
        private readonly int groupID;
        private readonly Font labelFont;

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
            InitializeComponent();
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

            this.groupBox1.Font = labelFont;

            ApplyTheme();
            SubscribeToEvents();

            switch (groupID)
            {
                case 0:
                    groupBox1.Text = UiText.Current.FocusGroupRed;
                    break;
                case 1:
                    groupBox1.Text = UiText.Current.FocusGroupGreen;
                    break;
                case 2:
                    groupBox1.Text = UiText.Current.FocusGroupBlue;
                    break;
                default:
                    break;
            }

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
        }

        /// <summary>
        /// Unsubscribes from events to avoid memory leaks.
        /// </summary>
        private void UnsubscribeToEvents()
        {
            BahtinovProcessing.FocusDataEvent -= FocusDataEvent;
            groupBox1.MouseEnter -= FocusChannelGroupBox_MouseEnter;
            groupBox1.MouseLeave -= FocusChannelGroupBox_MouseLeave;
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
                if (label.Name == "FocusErrorLabel")
                {
                    flags = TextFormatFlags.VerticalCenter | TextFormatFlags.Right;
                }

                // Draw the text with the specified color and format
                TextRenderer.DrawText(e.Graphics, label.Text, label.Font, label.ClientRectangle, textColor, flags);
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
                    offsetBarControl1.MarkerColor = UITheme.ErrorBarMarkerColorInRange;
                    offsetBarControl1.Value = (float)e.FocusData.BahtinovOffset;
                    ErrorOffset = e.FocusData.BahtinovOffset;
                }
                else
                {
                    offsetBarControl1.ResetHistory();
                    offsetBarControl1.MarkerColor = Color.Green;
                    offsetBarControl1.Value = 0.0f;
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

                    if (focusChannelCount > 0)
                        focusChannelCount--;
                }

                disposed = true;
                base.Dispose(disposing);
            }
        }

        #endregion
    }
}
