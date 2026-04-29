using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    /// <summary>
    /// Dark-themed message box wrapper that supports standard icon and button combinations.
    /// </summary>
    public partial class DarkMessageBox : Form
    {
        #region DLL Imports

        /// <summary>
        /// Sets a Desktop Window Manager attribute on the dialog window.
        /// </summary>
        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        #endregion

        #region Constants

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 19;

        #endregion

        #region Fields

        public static DialogResult UserResponse { get; private set; } = DialogResult.None;
        private readonly UiTextPack _textPack;

        private static readonly Dictionary<MessageBoxIcon, Icon> IconMap = new Dictionary<MessageBoxIcon, Icon>
        {
            { MessageBoxIcon.Error, SystemIcons.Error },
            { MessageBoxIcon.Information, SystemIcons.Information },
            { MessageBoxIcon.Question, SystemIcons.Question },
            { MessageBoxIcon.Warning, SystemIcons.Warning }
        };

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DarkMessageBox"/> class.
        /// </summary>
        /// <param name="message">The message to display in the message box.</param>
        /// <param name="title">The title of the message box.</param>
        /// <param name="buttons">The buttons to display in the message box.</param>
        /// <param name="icon">The icon to display in the message box.</param>
        public DarkMessageBox(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon, UiTextPack textPack = null)
        {
            InitializeComponent();
            _textPack = textPack ?? UiText.Current;
            this.Text = title;
            messageLabel.Text = message;
            okButton.Text = _textPack.CommonOk;
            cancelButton.Text = _textPack.CommonCancel;
            DrawIconInPictureBox(icon);
            ConfigureButtons(buttons);
            // Initial baseline (96-DPI) sizing. This gives our DPI-aware
            // ShowDialog helper a reasonable starting Size to predict the
            // owner-centered target position. AutoSizeControls is re-run in
            // OnShown once the dialog has landed on its final monitor so the
            // panel1 docking is correct even after the cross-monitor DPI
            // scale.
            AutoSizeControls();

            var color = UITheme.DarkBackground;
            int colorValue = color.R | (color.G << 8) | (color.B << 16);
            DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref colorValue, sizeof(int));
        }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Configures the visibility and text of the buttons based on the specified button options.
        /// </summary>
        private void ConfigureButtons(MessageBoxButtons buttons)
        {
            okButton.Visible = false;
            cancelButton.Visible = false;

            switch (buttons)
            {
                case MessageBoxButtons.OK:
                    okButton.Visible = true;
                    okButton.Text = _textPack.CommonOk;
                    break;

                case MessageBoxButtons.OKCancel:
                    okButton.Visible = true;
                    cancelButton.Visible = true;
                    okButton.Text = _textPack.CommonOk;
                    cancelButton.Text = _textPack.CommonCancel;
                    break;

                case MessageBoxButtons.YesNo:
                    okButton.Visible = true;
                    cancelButton.Visible = true;
                    okButton.Text = _textPack.CommonYes;
                    cancelButton.Text = _textPack.CommonNo;
                    break;

                case MessageBoxButtons.YesNoCancel:
                    okButton.Visible = true;
                    cancelButton.Visible = true;
                    okButton.Text = _textPack.CommonYes;
                    cancelButton.Text = _textPack.CommonCancel;
                    break;

                default:
                    okButton.Visible = true;
                    okButton.Text = _textPack.CommonOk;
                    break;
            }
        }

        /// <summary>
        /// Adjusts the size of the form to fit its controls.
        /// </summary>
        private void AutoSizeControls()
        {
            float dpiScale = DeviceDpi / 96f;

            int S(int logicalPixels)
            {
                return (int)Math.Round(logicalPixels * dpiScale, MidpointRounding.AwayFromZero);
            }

            SuspendLayout();

            messageLabel.AutoSize = true;
            messageLabel.MaximumSize = new Size(S(650), 0);
            messageLabel.PerformLayout();

            PerformLayout();

            int horizontalPadding = S(24);
            int verticalPadding = S(20);
            int bottomPanelTopSpacing = S(18);

            int contentLeft = Math.Min(iconBox.Left, messageLabel.Left);
            int contentRight = Math.Max(iconBox.Right, messageLabel.Right);
            int contentTop = Math.Min(iconBox.Top, messageLabel.Top);
            int contentBottom = Math.Max(iconBox.Bottom, messageLabel.Bottom);

            int desiredClientWidth = (contentRight - contentLeft) + (horizontalPadding * 2);

            int desiredClientWidthForButtons = (okButton.Visible ? okButton.Width : 0)
                + (cancelButton.Visible ? cancelButton.Width : 0)
                + S(48); // side padding + button gap allowance

            int finalClientWidth = Math.Max(desiredClientWidth, desiredClientWidthForButtons);

            int contentHeight = (contentBottom - contentTop);
            int finalClientHeight = verticalPadding + contentHeight + bottomPanelTopSpacing + panel1.Height;

            ClientSize = new Size(finalClientWidth, finalClientHeight);

            panel1.Location = new Point(0, ClientSize.Height - panel1.Height);
            panel1.Width = ClientSize.Width;

            ResumeLayout(performLayout: true);
        }

        /// <summary>
        /// Handles the click event for the OK button. Sets the user response to OK and closes the message box.
        /// </summary>
        private void OkButton_Click(object sender, EventArgs e)
        {
            UserResponse = DialogResult.OK;
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        /// <summary>
        /// Handles the click event for the Cancel button. Sets the user response to Cancel and closes the message box.
        /// </summary>
        private void CancelButton_Click(object sender, EventArgs e)
        {
            UserResponse = DialogResult.Cancel;
            this.DialogResult = DialogResult.No;
            this.Close();
        }

        /// <summary>
        /// Displays the custom message box with the specified parameters.
        /// </summary>
        /// <param name="message">The message to display in the message box.</param>
        /// <param name="title">The title of the message box.</param>
        /// <param name="icon">The icon to display in the message box.</param>
        /// <param name="buttons">The buttons to display in the message box.</param>
        /// <param name="owner">The owner form of the message box. If null, the message box is centered on the screen.</param>
        /// <returns>The <see cref="DialogResult"/> indicating the user's response.</returns>
        public static DialogResult Show(string message, string title, MessageBoxIcon icon, MessageBoxButtons buttons, Form owner = null)
        {
            if (Application.OpenForms.Count > 0)
            {
                Form mainForm = owner ?? Application.OpenForms[0];

                if (mainForm.InvokeRequired)
                {
                    return (DialogResult)mainForm.Invoke(new Func<DialogResult>(() => ShowCustomMessageBoxInternal(message, title, icon, buttons, null, mainForm)));
                }
                else
                {
                    return ShowCustomMessageBoxInternal(message, title, icon, buttons, null, mainForm);
                }
            }
            else
            {
                return ShowCustomMessageBoxInternal(message, title, icon, buttons, null, owner);
            }
        }

        /// <summary>
        /// Displays the custom message box using an explicit text pack for button labels.
        /// </summary>
        public static DialogResult Show(string message, string title, MessageBoxIcon icon, MessageBoxButtons buttons, UiTextPack textPack, Form owner = null)
        {
            if (Application.OpenForms.Count > 0)
            {
                Form mainForm = owner ?? Application.OpenForms[0];

                if (mainForm.InvokeRequired)
                {
                    return (DialogResult)mainForm.Invoke(new Func<DialogResult>(() => ShowCustomMessageBoxInternal(message, title, icon, buttons, textPack, mainForm)));
                }
                else
                {
                    return ShowCustomMessageBoxInternal(message, title, icon, buttons, textPack, mainForm);
                }
            }
            else
            {
                return ShowCustomMessageBoxInternal(message, title, icon, buttons, textPack, owner);
            }
        }

        /// <summary>
        /// Creates and displays the custom message box within the specified owner's bounds.
        /// </summary>
        /// <returns>The <see cref="DialogResult"/> indicating the user's response.</returns>
        private static DialogResult ShowCustomMessageBoxInternal(string message, string title, MessageBoxIcon icon, MessageBoxButtons buttons, UiTextPack textPack, Form owner)
        {
            using (DarkMessageBox customMessageBox = new DarkMessageBox(message, title, buttons, icon, textPack))
            {
                // Let the DPI-aware helper do the centering. Computing an
                // owner-centered location up front is wrong in multi-DPI
                // setups because the dialog's Width/Height here are still
                // at the designer 96-DPI baseline and will grow once the
                // dialog crosses onto the owner's higher-DPI monitor.
                customMessageBox.StartPosition = FormStartPosition.CenterParent;
                return customMessageBox.ShowDialogDpiAware(owner);
            }
        }

        /// <summary>
        /// Handles the Load event of the form.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        /// <summary>
        /// Finalizes the layout and owner-centered position once the dialog
        /// has been moved onto the owner's monitor and WinForms has applied
        /// per-monitor DPI scaling. <see cref="DpiAwareDialog.ShowDialogDpiAware"/>
        /// has already run its <c>Shown</c> handler by the time we call
        /// <c>base.OnShown</c>, so at this point <c>DeviceDpi</c> reflects the
        /// final monitor and <see cref="AutoSizeControls"/> produces pixel
        /// sizes that actually match what the user will see.
        /// </summary>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            AutoSizeControls();

            // Re-center on the owner using the now-final (scaled) size. The
            // helper's pre-computed center was based on a predicted size; the
            // final size after AutoSizeControls can differ (it's content-sized
            // based on the message text), so without this the dialog ends up
            // visibly off-center on the owner.
            Form ownerForm = this.Owner;
            if (ownerForm != null && ownerForm.IsHandleCreated)
            {
                Rectangle ob = ownerForm.Bounds;
                int x = ob.Left + (ob.Width - this.Width) / 2;
                int y = ob.Top + (ob.Height - this.Height) / 2;
                this.Location = new Point(x, y);
            }
        }

        /// <summary>
        /// Draws the specified icon in the iconBox control.
        /// </summary>
        private void DrawIconInPictureBox(MessageBoxIcon icon)
        {
            int iconSize = 32;
            Bitmap bitmap = new Bitmap(iconSize, iconSize);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(UITheme.Transparent);

                Icon iconImage = GetIcon(icon, iconSize);
                g.DrawIcon(iconImage, new Rectangle(0, 0, iconSize, iconSize));
            }

            iconBox.Image = bitmap;
        }

        /// <summary>
        /// Retrieves the icon associated with the specified MessageBoxIcon and resizes it.
        /// </summary>
        /// <returns>The resized icon.</returns>
        private Icon GetIcon(MessageBoxIcon messageBoxIcon, int size)
        {
            if (IconMap.TryGetValue(messageBoxIcon, out Icon icon))
            {
                return new Icon(icon, size, size);
            }

            return null;
        }

        #endregion
    }
}
