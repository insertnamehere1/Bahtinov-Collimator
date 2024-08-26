using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace Bahtinov_Collimator
{
    public partial class Settings : Form
    {
        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        // Constants
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 19;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        public Settings()
        {
            InitializeComponent();
            LoadSettings();
            SetColorScheme();
        }

        #endregion

        #region Methods

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            float increasedSize = this.Font.Size + 2.0f;
            Font newFont = new Font(this.Font.FontFamily, increasedSize, this.Font.Style);

            // Adjust fonts
            this.Font = newFont;

            this.Font = newFont;
            this.groupBox1.Font = newFont;
            this.groupBox2.Font = newFont;
            this.label1.Font = newFont;
            this.label2.Font = newFont;
            this.label3.Font = newFont;
            this.label4.Font = newFont;
            this.label5.Font = newFont;
            this.label6.Font = newFont;
            this.label7.Font = newFont;
            this.label8.Font = newFont;
            this.label9.Font = newFont;
            this.label10.Font = newFont;
            this.ApertureTextBox.Font = newFont;
            this.PixelSizeTextBox.Font = newFont;
            this.FocalLengthTextBox.Font = newFont;
            this.CancelSettingsButton.Font = newFont;
            this.okButton.Font = newFont;
        }


        private void SetColorScheme()
        {
            // main form
            this.ForeColor = UITheme.DarkForeground;
            this.BackColor = UITheme.DarkBackground;

            // OK button
            okButton.BackColor = UITheme.ButtonDarkBackground;
            okButton.ForeColor = UITheme.ButtonDarkForeground;
            okButton.FlatStyle = FlatStyle.Popup;

            // Cancel button
            CancelSettingsButton.BackColor = UITheme.ButtonDarkBackground;
            CancelSettingsButton.ForeColor = UITheme.ButtonDarkForeground;
            CancelSettingsButton.FlatStyle = FlatStyle.Popup;

            // Titlebar
            var color = UITheme.DarkBackground;
            int colorValue = color.R | (color.G << 8) | (color.B << 16);
            DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref colorValue, sizeof(int));

            // Group Boxes
            ChangeLabelColors(groupBox1, UITheme.MenuDarkForeground);
            ChangeLabelColors(groupBox2, UITheme.MenuDarkForeground);
            groupBox1.ForeColor = UITheme.MenuDarkForeground;
            groupBox2.ForeColor = UITheme.MenuDarkForeground;

            ChangeTextBoxColors(groupBox2);
        }

        private void ChangeTextBoxColors(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                if (control is TextBox textBox)
                {
                    textBox.ForeColor = UITheme.TextBoxForeground;
                    textBox.BackColor = UITheme.TextBoxBackground;
                    textBox.BorderStyle = BorderStyle.None;

                }
            }
        }

        private void ChangeLabelColors(Control parent, Color color)
        {
            foreach (Control control in parent.Controls)
            {
                if (control is Label label)
                {
                    label.ForeColor = color;
                }
            }
        }

        /// <summary>
        /// Loads settings from application settings and displays them in the form.
        /// </summary>
        private void LoadSettings()
        {
            ApertureTextBox.Text = Properties.Settings.Default.Aperture.ToString("F0");
            FocalLengthTextBox.Text = Properties.Settings.Default.FocalLength.ToString("F0");
            PixelSizeTextBox.Text = Properties.Settings.Default.PixelSize.ToString("F2");
            VoiceCheckBox.Checked = Properties.Settings.Default.VoiceEnabled;
        }

        /// <summary>
        /// Saves the settings when the OK button is clicked and closes the form.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OkButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Parse input values and save them to application settings
                Properties.Settings.Default.Aperture = int.Parse(ApertureTextBox.Text);
                Properties.Settings.Default.FocalLength = int.Parse(FocalLengthTextBox.Text);
                Properties.Settings.Default.PixelSize = float.Parse(PixelSizeTextBox.Text);
                Properties.Settings.Default.VoiceEnabled = VoiceCheckBox.Checked;

                // Save settings
                Properties.Settings.Default.Save();

                // Indicate that the form was closed with OK result
                this.DialogResult = DialogResult.OK;
            }
            catch
            {
                // Show warning message if input parsing fails
                MessageBox.Show("Invalid input, try again", "Warning",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Close the form
            this.Close();
        }

        /// <summary>
        /// Closes the form with a Cancel result when the Cancel button is clicked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        #endregion
    }
}
