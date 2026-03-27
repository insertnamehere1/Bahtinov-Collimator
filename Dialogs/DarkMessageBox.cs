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
        public DarkMessageBox(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            InitializeComponent();
            this.Text = title;
            messageLabel.Text = message;
            okButton.Text = UiText.Current.CommonOk;
            cancelButton.Text = UiText.Current.CommonCancel;
            DrawIconInPictureBox(icon); 
            ConfigureButtons(buttons);
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
                    okButton.Text = UiText.Current.CommonOk;
                    break;

                case MessageBoxButtons.OKCancel:
                    okButton.Visible = true;
                    cancelButton.Visible = true;
                    okButton.Text = UiText.Current.CommonOk;
                    cancelButton.Text = UiText.Current.CommonCancel;
                    break;

                case MessageBoxButtons.YesNo:
                    okButton.Visible = true;
                    cancelButton.Visible = true;
                    okButton.Text = UiText.Current.CommonYes;
                    cancelButton.Text = UiText.Current.CommonNo;
                    break;

                case MessageBoxButtons.YesNoCancel:
                    okButton.Visible = true;
                    cancelButton.Visible = true;
                    okButton.Text = UiText.Current.CommonYes;
                    cancelButton.Text = UiText.Current.CommonCancel;
                    break;

                default:
                    okButton.Visible = true;
                    okButton.Text = UiText.Current.CommonOk;
                    break;
            }
        }

        /// <summary>
        /// Adjusts the size of the form to fit its controls.
        /// </summary>
        private void AutoSizeControls()
        {
            float dpiScale = DeviceDpi / 96f;

            int ScaleLogicalPixels(int logicalPixels)
            {
                return (int)Math.Round(logicalPixels * dpiScale, MidpointRounding.AwayFromZero);
            }

            SuspendLayout();

            messageLabel.AutoSize = true;
            messageLabel.MaximumSize = new Size(ScaleLogicalPixels(650), 0);
            messageLabel.PerformLayout();

            PerformLayout();

            int horizontalPadding = ScaleLogicalPixels(24);
            int verticalPadding = ScaleLogicalPixels(20);
            int bottomPanelTopSpacing = ScaleLogicalPixels(18);

            int contentLeft = Math.Min(iconBox.Left, messageLabel.Left);
            int contentRight = Math.Max(iconBox.Right, messageLabel.Right);
            int contentTop = Math.Min(iconBox.Top, messageLabel.Top);
            int contentBottom = Math.Max(iconBox.Bottom, messageLabel.Bottom);

            int desiredClientWidth = (contentRight - contentLeft) + (horizontalPadding * 2);

            int desiredClientWidthForButtons = (okButton.Visible ? okButton.Width : 0)
                + (cancelButton.Visible ? cancelButton.Width : 0)
                + ScaleLogicalPixels(48);

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
                    return (DialogResult)mainForm.Invoke(new Func<DialogResult>(() => ShowCustomMessageBoxInternal(message, title, icon, buttons, mainForm)));
                }
                else
                {
                    return ShowCustomMessageBoxInternal(message, title, icon, buttons, mainForm);
                }
            }
            else
            {
                return ShowCustomMessageBoxInternal(message, title, icon, buttons, owner);
            }
        }

        /// <summary>
        /// Creates and displays the custom message box within the specified owner's bounds.
        /// </summary>
        /// <returns>The <see cref="DialogResult"/> indicating the user's response.</returns>
        private static DialogResult ShowCustomMessageBoxInternal(string message, string title, MessageBoxIcon icon, MessageBoxButtons buttons, Form owner)
        {
            using (DarkMessageBox customMessageBox = new DarkMessageBox(message, title, buttons, icon))
            {
                Rectangle ownerRect = owner.Bounds;
                int x = ownerRect.Left + (ownerRect.Width - customMessageBox.Width) / 2;
                int y = ownerRect.Top + (ownerRect.Height - customMessageBox.Height) / 2;

                customMessageBox.StartPosition = FormStartPosition.Manual;
                customMessageBox.Location = new Point(x, y);

                return customMessageBox.ShowDialog(owner);
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
        /// Draws the specified icon in the iconBox control.
        /// </summary>
        private void DrawIconInPictureBox(MessageBoxIcon icon)
        {
            int iconSize = 32;
            Bitmap bitmap = new Bitmap(iconSize, iconSize);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);

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
