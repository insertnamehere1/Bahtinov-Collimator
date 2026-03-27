using Bahtinov_Collimator.Custom_Components;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    public partial class FocusChannelComponent : UserControl
    {
        #region Constants

        private const int HISTORYBAR_VERTICAL_POSITION = -10;

        #endregion

        #region Fields

        public delegate void ChannelSelectEventHandler(object sender, ChannelSelectEventArgs e);
        public static event ChannelSelectEventHandler ChannelSelectDataEvent;

        private static int focusChannelCount = 0;
        private static readonly bool[] mouseOver = new bool[3] { false, false, false };

        public double ErrorOffset { private set; get; } = 0.0f;

        public Custom_Components.MirrorType MirrorType
        {
            get => mirrorDrawingComponent1.MirrorType;
            set => mirrorDrawingComponent1.MirrorType = value;
        }

        private bool disposed = false; // To detect redundant calls
        private int groupID;

        /// <summary>Horizontal inset from group box left/right (96 DPI logical px).</summary>
        private const int HistoryBarHorizontalMarginAt96 = 6;

        /// <summary>
        /// Default focus channel width (96 DPI logical px), same as <see cref="Form1"/> default channel width.
        /// Used to compute the history bar width when the mirror is visible: same width as a full-stretch bar at that size.
        /// </summary>
        private const int DefaultFocusChannelWidthAt96 = 185;

        /// <summary>Distance from the group box’s left client edge to the mirror (96 DPI logical px).</summary>
        private const int MirrorHorizontalInsetAt96 = 8;

        /// <summary>Minimum gap between the mirror’s right edge and the history bar’s left (96 DPI logical px).</summary>
        private const int MirrorGapToHistoryBarAt96 = 8;

        /// <summary>Mirror height as a fraction of <see cref="Control.ClientSize.Height"/> of the group box (scales when the channel resizes).</summary>
        private const double MirrorHeightFractionOfGroupClient = 0.62;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="FocusChannelComponent"/> class.
        /// </summary>
        public FocusChannelComponent()
        {
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();

            InitializeComponent();
            ApplyTheme();
            SubscribeToEvents();
            UpdateHistoryBarLayout();
            UpdateMirrorPanelLayout();
            focusChannelCount++;
        }

        /// <inheritdoc />
        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);
            UpdateHistoryBarLayout();
            UpdateMirrorPanelLayout();
        }

        /// <summary>
        /// Reconfigures this channel for the specified group and mask mode.
        /// </summary>
        /// <param name="groupID">Channel index (0 red, 1 green, 2 blue).</param>
        /// <param name="isTribahtinov">True when Tri-Bahtinov mode is active and mirror visualization should be shown.</param>
        public void ConfigureFocusChannel(int groupID, bool isTribahtinov)
        {
            UnsubscribeToEvents();

            if (focusChannelCount > 0)
                focusChannelCount--;
        
            this.groupID = groupID;

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

            if (isTribahtinov)
            {
                if (Properties.Settings.Default.SCTSelected == true)
                    mirrorDrawingComponent1.MirrorType = Custom_Components.MirrorType.SctPrimary;
                if (Properties.Settings.Default.MCTSelected == true)
                    mirrorDrawingComponent1.MirrorType = Custom_Components.MirrorType.MctSecondary;
                mirrorDrawingComponent1.Visible = true;
            }
            else
                mirrorDrawingComponent1.Visible = false;
            
            ApplyTheme();
            SubscribeToEvents();
            UpdateHistoryBarLayout();
            UpdateMirrorPanelLayout();
            focusChannelCount++;
        }

        /// <summary>
        /// Scales a 96-DPI logical pixel value to current monitor device pixels.
        /// </summary>
        /// <returns>The DPI-scaled device pixel value rounded away from zero.</returns>
        private int S(int logicalPixelsAt96)
        {
            float dpiScale = DeviceDpi / 96f;
            return (int)Math.Round(logicalPixelsAt96 * dpiScale, MidpointRounding.AwayFromZero);
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
            groupBox1.Resize += GroupBox1_Resize;

            mirrorDrawingComponent1.MouseEnter += ChildMouseEnter;
            mirrorDrawingComponent1.MouseLeave += ChildMouseLeave;
        }

        /// <summary>
        /// Unsubscribes from events to avoid memory leaks.
        /// </summary>
        private void UnsubscribeToEvents()
        {
            BahtinovProcessing.FocusDataEvent -= FocusDataEvent;
            groupBox1.MouseEnter -= FocusChannelGroupBox_MouseEnter;
            groupBox1.MouseLeave -= FocusChannelGroupBox_MouseLeave;
            groupBox1.Resize -= GroupBox1_Resize;
        }

        #endregion

        #region Theme Application

        /// <summary>
        /// Applies the theme to the component.
        /// </summary>
        private void ApplyTheme()
        {
            groupBox1.ForeColor = UITheme.GetGroupBoxTextColor(groupID);
            mirrorDrawingComponent1.BackColor = groupBox1.BackColor;
            mirrorDrawingComponent1.MirrorOutlineColor = UITheme.GetGroupBoxTextColor(groupID);
        }

        #endregion

        #region Label Paint Event

        /// <summary>
        /// Custom paint event handler for labels.
        /// </summary>
        private void Label_Paint(object sender, PaintEventArgs e)
        {
            if (sender is Label label && label.Parent is GroupBox parentGroupBox && parentGroupBox.Tag is int groupId)
            {
                Color textColor = UITheme.GetGroupBoxTextColor(groupId);
                TextFormatFlags flags = TextFormatFlags.Default;

                if (label.Name == "FocusErrorLabel")
                {
                    flags = TextFormatFlags.VerticalCenter | TextFormatFlags.Right;
                }

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
                    offsetBarControl1.ResetValueAndHistory();
                    offsetBarControl1.MarkerColor = Color.Green;
                    ErrorOffset = 0.0;
                }
            }
        }

        #endregion

        #region Telescope Cross-section Drawing

        /// <summary>
        /// Repositions the mirror drawing surface as the group box resizes.
        /// </summary>
        private void GroupBox1_Resize(object sender, EventArgs e)
        {
            UpdateHistoryBarLayout();
            UpdateMirrorPanelLayout();
        }

        /// <summary>
        /// Updates history-bar size and placement based on current channel mode and DPI.
        /// </summary>
        private void UpdateHistoryBarLayout()
        {
            if (offsetBarControl1 == null || groupBox1 == null)
                return;

            int margin = S(HistoryBarHorizontalMarginAt96);
            int clientW = groupBox1.ClientSize.Width;
            int innerW = Math.Max(0, clientW - 2 * margin);

            if (mirrorDrawingComponent1.Visible)
            {
                int desiredBarW = S(DefaultFocusChannelWidthAt96) - 2 * S(HistoryBarHorizontalMarginAt96);
                desiredBarW = Math.Max(S(80), desiredBarW);
                int barW = Math.Min(desiredBarW, innerW);

                offsetBarControl1.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                offsetBarControl1.Width = barW;
                offsetBarControl1.Left = clientW - margin - barW;
            }
            else
            {
                offsetBarControl1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                offsetBarControl1.Left = margin;
                offsetBarControl1.Width = Math.Min(Math.Max(S(80), innerW), Math.Max(innerW, 1));
            }

            Rectangle content = groupBox1.DisplayRectangle;
            int targetTop = content.Top + ((content.Height - offsetBarControl1.Height) / 2) + S(HISTORYBAR_VERTICAL_POSITION);
            targetTop = Math.Max(content.Top, targetTop);

            if (offsetBarControl1.Top != targetTop)
                offsetBarControl1.Top = targetTop;

            offsetBarControl1.BringToFront();
        }

        /// <summary>
        /// Anchors the mirror to the left; width uses space left of the history bar, height tracks group box height.
        /// <see cref="Custom_Components.MirrorDrawingComponent"/> scales a fixed 50×100 design to this bounds via <c>Graphics.ScaleTransform</c>.
        /// </summary>
        private void UpdateMirrorPanelLayout()
        {
            int leftInset = S(MirrorHorizontalInsetAt96);
            int gapToBar = S(MirrorGapToHistoryBarAt96);
            int maxMirrorW = Math.Max(0, offsetBarControl1.Left - gapToBar - leftInset);
            int panelWidth = Math.Max(1, maxMirrorW);

            int ch = groupBox1.ClientSize.Height;
            int maxH = Math.Max(0, ch - S(42));
            int panelHeight = Math.Max(S(40), Math.Min(maxH, (int)Math.Round(ch * MirrorHeightFractionOfGroupClient)));

            int panelX = leftInset;
            int panelY = Math.Max(S(26), (ch - panelHeight) / 2);

            mirrorDrawingComponent1.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            mirrorDrawingComponent1.Bounds = new Rectangle(panelX, panelY, panelWidth, panelHeight);
            mirrorDrawingComponent1.BackColor = groupBox1.BackColor;
            mirrorDrawingComponent1.MirrorOutlineColor = UITheme.GetGroupBoxTextColor(groupID);
            mirrorDrawingComponent1.Invalidate();
        }

        #endregion

        #region GroupBox Mouse Events

        /// <summary>
        /// Handles the MouseEnter event for the group box.
        /// </summary>
        protected void FocusChannelGroupBox_MouseEnter(object sender, EventArgs e)
        {
            groupBox1.BackColor = UITheme.GetGroupBoxBackgroundColor(groupID);
            mirrorDrawingComponent1.BackColor = UITheme.GetGroupBoxBackgroundColor(groupID);
            mirrorDrawingComponent1.MirrorOutlineColor = UITheme.GetGroupBoxTextColor(groupID);
            mirrorDrawingComponent1.Invalidate();
            mouseOver[groupID] = true;
            ChannelSelectDataEvent?.Invoke(null, new ChannelSelectEventArgs(mouseOver, focusChannelCount));
        }

        /// <summary>
        /// Handles the MouseLeave event for the group box.
        /// </summary>
        private void FocusChannelGroupBox_MouseLeave(object sender, EventArgs e)
        {
            groupBox1.BackColor = UITheme.DarkBackground;
            mirrorDrawingComponent1.BackColor = UITheme.DarkBackground;
            mirrorDrawingComponent1.MirrorOutlineColor = UITheme.GetGroupBoxTextColor(groupID);
            mirrorDrawingComponent1.Invalidate();
            mouseOver[groupID] = false;
            ChannelSelectDataEvent?.Invoke(null, new ChannelSelectEventArgs(mouseOver, focusChannelCount));
        }

        /// <summary>
        /// Redirects child hover events to the parent group-box enter handler.
        /// </summary>
        private void ChildMouseEnter(object sender, EventArgs e)
        {
            FocusChannelGroupBox_MouseEnter(sender, e);
        }

        /// <summary>
        /// Redirects child leave events when the cursor exits the component bounds.
        /// </summary>
        private void ChildMouseLeave(object sender, EventArgs e)
        {
            if (!ClientRectangle.Contains(PointToClient(Cursor.Position)))
                FocusChannelGroupBox_MouseLeave(sender, e);
        }

        #endregion

        #region Disposal

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
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
