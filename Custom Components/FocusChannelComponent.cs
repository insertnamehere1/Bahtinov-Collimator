using System;
using System.Drawing;
using System.Runtime.InteropServices;
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
        private bool labelsVisible = false;

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
            ApplyLocalization();

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

            SetLabelProperties(labelFont);
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

        private void ApplyLocalization()
        {
            var textPack = UiText.Current;
            label6.Text = textPack.FocusLabelFarAway;
            label2.Text = textPack.FocusLabelClose;
            label5.Text = textPack.FocusLabelOk;
            label4.Text = textPack.FocusLabelToo;
            label3.Text = textPack.FocusLabelToo;
        }

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
            offsetBarControl1.Maximum = 2.0f;
            offsetBarControl1.Minimum = -2.0f;
        }

        /// <summary>
        /// Sets up the location of various labels in the form. 
        /// The labels are organized into three columns: first column, second column, and third column.
        /// </summary>
        private void SetupLabelLocation()
        {


            label3.Location = new Point(31, 85);
            label4.Location = new Point(190, 85);
            label5.Location = new Point(118, 85);
            label2.Location = new Point(28, 100);
            label6.Location = new Point(175, 100);

            label3.BringToFront();
            label4.BringToFront();
            label5.BringToFront();
            label2.BringToFront();
            label6.BringToFront();

            RefreshLabelVisibility();
        }

        public void SetLabelsVisible(bool visible)
        {
            labelsVisible = visible;
            RefreshLabelVisibility();
        }

        private void RefreshLabelVisibility()
        {
            bool visible = Properties.Settings.Default.CalibrationCompleted && labelsVisible;

            label2.Visible = visible;
            label3.Visible = visible;
            label4.Visible = visible;
            label5.Visible = visible;
            label6.Visible = visible;
        }   

        /// <summary>
        /// Sets the font for all labels.
        /// </summary>
        /// <param name="newFont">The new font to apply to the labels.</param>
        private void SetLabelFont(Font newFont)
        {
            foreach (var label in GetAllLabels())
            {
                var smallLabelFont = new Font(this.Font.FontFamily, 8.0f);
                
                if(label == label3 || label == label4 || label == label5 || label == label2 || label == label6)
                    label.Font = smallLabelFont;
                else
                    label.Font = newFont;
            }
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
                if (label.Name == "FocusErrorLabel" )
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
                    RefreshLabelVisibility();
                    offsetBarControl1.MarkerColor = UITheme.ErrorBarMarkerColorInRange;
                    offsetBarControl1.Value = (float)e.FocusData.BahtinovOffset; 
                    ErrorOffset = e.FocusData.BahtinovOffset;
                }
                else
                {
                    RefreshLabelVisibility();
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
            return new Label[] { label3, label4, label5, label2, label6 };
        }

        #endregion
    }
}
