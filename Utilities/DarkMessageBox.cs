using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    public partial class DarkMessageBox : Form
    {
        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        // Constants
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 19;

        // Property to hold the user's response
        public static DialogResult UserResponse { get; private set; } = DialogResult.None;

        // Dictionary to map MessageBoxIcon to SystemIcons
        private static readonly Dictionary<MessageBoxIcon, Icon> IconMap = new Dictionary<MessageBoxIcon, Icon>
        {
            { MessageBoxIcon.Error, SystemIcons.Error },
            { MessageBoxIcon.Information, SystemIcons.Information },
            { MessageBoxIcon.Question, SystemIcons.Question },
            { MessageBoxIcon.Warning, SystemIcons.Warning }
        };

        // Constructor
        public DarkMessageBox(string message, string title, MessageBoxIcon icon, MessageBoxButtons buttons)
        {
            InitializeComponent();

            this.Text = title;
            messageLabel.Text = message;
            DrawIconInPictureBox(icon);

            SetColor();
            ConfigureButtons(buttons);
            AutoSizeControls();
        }

        // Set color and style
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

            cancelButton.BackColor = UITheme.ButtonDarkBackground;
            cancelButton.ForeColor = UITheme.ButtonDarkForeground;
            cancelButton.FlatStyle = FlatStyle.Popup;
        }

        // Configure the buttons based on the MessageBoxButtons parameter
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
                okButton.Text = "Yes";
                cancelButton.Text = "No";
            }
        }

        // Adjust the form size based on the content
        private void AutoSizeControls()
        {
            this.messageLabel.AutoSize = true;
            int width = Math.Max(this.messageLabel.Width + 150, this.okButton.Width + 255);
            int height = this.messageLabel.Height + this.okButton.Height + 105; // Padding and spacing
            this.ClientSize = new Size(width, height);
        }

        // Handle OK button click
        private void okButton_Click(object sender, EventArgs e)
        {
            UserResponse = DialogResult.OK;
            this.Close();
        }

        // Handle Cancel button click
        private void cancelButton_Click(object sender, EventArgs e)
        {
            UserResponse = DialogResult.Cancel;
            this.Close();
        }

        // Display the message box
        public static DialogResult Show(string message, string title, MessageBoxIcon icon, MessageBoxButtons buttons)
        {
            if (Application.OpenForms.Count > 0)
            {
                Form mainForm = Application.OpenForms[0];

                if (mainForm.InvokeRequired)
                {
                    return (DialogResult)mainForm.Invoke(new Func<DialogResult>(() => ShowCustomMessageBoxInternal(message, title, icon, buttons)));
                }
                else
                {
                    return ShowCustomMessageBoxInternal(message, title, icon, buttons);
                }
            }
            else
            {
                // Fallback if there are no open forms
                return ShowCustomMessageBoxInternal(message, title, icon, buttons);
            }
        }

        // Internal method to show the message box
        private static DialogResult ShowCustomMessageBoxInternal(string message, string title, MessageBoxIcon icon, MessageBoxButtons buttons)
        {
            using (DarkMessageBox customMessageBox = new DarkMessageBox(message, title, icon, buttons))
            {
                customMessageBox.ShowDialog();
                return UserResponse;
            }
        }

        // Center the form on the screen
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.StartPosition = FormStartPosition.Manual;
            Rectangle screen = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(
                (screen.Width - this.Width) / 2,
                (screen.Height - this.Height) / 2
            );
        }

        // Draw the icon in the PictureBox
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

        // Get the appropriate icon based on the MessageBoxIcon parameter
        private Icon GetIcon(MessageBoxIcon messageBoxIcon, int size)
        {
            if (IconMap.TryGetValue(messageBoxIcon, out Icon icon))
            {
                return new Icon(icon, size, size);
            }

            return null;
        }
    }
}
