using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    public partial class DarkMessageBox : Form
    {
        #region DLL Imports

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
            SetTextSize();
            SetColor();
            ConfigureButtons(buttons);
            AutoSizeControls();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the color scheme of the message box and its controls.
        /// </summary>
        private void SetColor()
        {
            var color = UITheme.DarkBackground;
            int colorValue = color.R | (color.G << 8) | (color.B << 16);
            DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref colorValue, sizeof(int));

            panel1.BackColor = UITheme.MessageBoxPanelBackground;
            this.BackColor = UITheme.DarkBackground;
            messageLabel.ForeColor = UITheme.MessageBoxTextColor;

            okButton.BackColor = UITheme.ButtonDarkBackground;
            okButton.ForeColor = UITheme.ButtonDarkForeground;
            okButton.FlatStyle = FlatStyle.Popup;
            okButton.CornerRadius = 4;
            okButton.TextOffsetX = 0;
            okButton.BevelDark = Color.FromArgb(180, 90, 90, 90);
            okButton.BevelLight = Color.FromArgb(220, 160, 160, 160);

            cancelButton.BackColor = UITheme.ButtonDarkBackground;
            cancelButton.ForeColor = UITheme.ButtonDarkForeground;
            cancelButton.FlatStyle = FlatStyle.Popup;
            cancelButton.CornerRadius = 4;
            cancelButton.TextOffsetX = 0;
            cancelButton.BevelDark = Color.FromArgb(180, 90, 90, 90);
            cancelButton.BevelLight = Color.FromArgb(220, 160, 160, 160);

        }

        /// <summary>
        /// Increases the font size of the message box and its controls.
        /// </summary>
        private void SetTextSize()
        {
            float increasedSize = this.Font.Size + 2.0f;
            Font newFont = new Font(this.Font.FontFamily, increasedSize, this.Font.Style);

            // Adjust fonts
            this.Font = newFont;
            this.messageLabel.Font = newFont;
            this.cancelButton.Font = newFont;
            this.okButton.Font = newFont;
        }

        /// <summary>
        /// Configures the visibility and text of the buttons based on the specified button options.
        /// </summary>
        /// <param name="buttons">The buttons to display in the message box.</param>
        private void ConfigureButtons(MessageBoxButtons buttons)
        {
            okButton.Visible = false;
            cancelButton.Visible = false;

            if (buttons.HasFlag(MessageBoxButtons.OK))
            {
                okButton.Visible = true;
            }

            if (buttons.HasFlag(MessageBoxButtons.YesNo))
            {
                okButton.Visible = true;
                cancelButton.Visible = true;
                okButton.Text = UiText.Current.CommonYes;
                cancelButton.Text = UiText.Current.CommonNo;
            }
        }

        /// <summary>
        /// Adjusts the size of the form to fit its controls.
        /// </summary>
        private void AutoSizeControls()
        {
            this.messageLabel.AutoSize = true;
            int width = Math.Max(this.messageLabel.Width + 150, this.okButton.Width + 255);
            int height = this.messageLabel.Height + this.okButton.Height + 105; // Padding and spacing
            this.ClientSize = new Size(width, height);
        }

        /// <summary>
        /// Handles the click event for the OK button. Sets the user response to OK and closes the message box.
        /// </summary>
        /// <param name="sender">The source of the event, typically the OK button.</param>
        /// <param name="e">An <see cref="EventArgs"/> object that contains the event data.</param>
        private void OkButton_Click(object sender, EventArgs e)
        {
            UserResponse = DialogResult.OK;
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        /// <summary>
        /// Handles the click event for the Cancel button. Sets the user response to Cancel and closes the message box.
        /// </summary>
        /// <param name="sender">The source of the event, typically the Cancel button.</param>
        /// <param name="e">An <see cref="EventArgs"/> object that contains the event data.</param>
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
                // Fallback if there are no open forms
                return ShowCustomMessageBoxInternal(message, title, icon, buttons, owner);
            }
        }

        /// <summary>
        /// Creates and displays the custom message box within the specified owner's bounds.
        /// </summary>
        /// <param name="message">The message to display in the message box.</param>
        /// <param name="title">The title of the message box.</param>
        /// <param name="icon">The icon to display in the message box.</param>
        /// <param name="buttons">The buttons to display in the message box.</param>
        /// <param name="owner">The owner form of the message box.</param>
        /// <returns>The <see cref="DialogResult"/> indicating the user's response.</returns>
        private static DialogResult ShowCustomMessageBoxInternal(string message, string title, MessageBoxIcon icon, MessageBoxButtons buttons, Form owner)
        {
            using (DarkMessageBox customMessageBox = new DarkMessageBox(message, title, buttons, icon))
            {
                // Position the message box within the owner's bounds
                Rectangle ownerRect = owner.Bounds;
                int x = ownerRect.Left + (ownerRect.Width - customMessageBox.Width) / 2;
                int y = ownerRect.Top + (ownerRect.Height - customMessageBox.Height) / 2;

                customMessageBox.StartPosition = FormStartPosition.Manual;
                customMessageBox.Location = new Point(x, y);

                // Return the DialogResult directly
                return customMessageBox.ShowDialog(owner);
            }
        }

        /// <summary>
        /// Handles the Load event of the form.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // Centering logic now handled in ShowCustomMessageBoxInternal
        }

        /// <summary>
        /// Draws the specified icon in the iconBox control.
        /// </summary>
        /// <param name="icon">The icon to display.</param>
        private void DrawIconInPictureBox(MessageBoxIcon icon)
        {
            int iconSize = 32; // Adjust the size if needed
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
        /// <param name="messageBoxIcon">The MessageBoxIcon to retrieve.</param>
        /// <param name="size">The size of the icon.</param>
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
