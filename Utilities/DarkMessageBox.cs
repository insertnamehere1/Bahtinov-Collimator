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

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 19;
        public static DialogResult UserResponse { get; private set; } = DialogResult.None;

        private static readonly Dictionary<MessageBoxIcon, Icon> IconMap = new Dictionary<MessageBoxIcon, Icon>
        {
            { MessageBoxIcon.Error, SystemIcons.Error },
            { MessageBoxIcon.Information, SystemIcons.Information },
            { MessageBoxIcon.Question, SystemIcons.Question },
            { MessageBoxIcon.Warning, SystemIcons.Warning }
        };

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

        private void AutoSizeControls()
        {
            this.messageLabel.AutoSize = true;
            int width = Math.Max(this.messageLabel.Width + 150, this.okButton.Width + 255);
            int height = this.messageLabel.Height + this.okButton.Height + 105; // Padding and spacing
            this.ClientSize = new Size(width, height);
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            UserResponse = DialogResult.OK;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            UserResponse = DialogResult.Cancel;
            this.Close();
        }

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

        private static DialogResult ShowCustomMessageBoxInternal(string message, string title, MessageBoxIcon icon, MessageBoxButtons buttons, Form owner)
        {
            using (DarkMessageBox customMessageBox = new DarkMessageBox(message, title, icon, buttons))
            {
                // Position the message box within the owner's bounds
                Rectangle ownerRect = owner.Bounds;
                int x = ownerRect.Left + (ownerRect.Width - customMessageBox.Width) / 2;
                int y = ownerRect.Top + (ownerRect.Height - customMessageBox.Height) / 2;

                customMessageBox.StartPosition = FormStartPosition.Manual;
                customMessageBox.Location = new Point(x, y);

                return customMessageBox.ShowDialog(owner);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // Centering logic now handled in ShowCustomMessageBoxInternal
        }

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
