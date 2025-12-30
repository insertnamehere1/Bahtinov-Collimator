using System;
using System.Drawing;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    public partial class Settings : Form
    {
        #region DLL Imports

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        #endregion

        #region Constants

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 19;

        #endregion

        #region Variables

        #endregion

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

        /// <summary>
        /// Handles the Load event of the form. Increases the font size of the form and its controls.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            float increasedSize = this.Font.Size + 2.0f;
            Font newFont = new Font(this.Font.FontFamily, increasedSize, this.Font.Style);

            // Adjust fonts
            this.Font = newFont;
            groupBox1.Font = newFont;
            label5.Font = newFont;
            label8.Font = newFont;
            label9.Font = newFont;
            label10.Font = newFont;
            CancelSettingsButton.Font = newFont;
            okButton.Font = newFont;
            SCTRadioButton.Font = newFont;
            MakCassRadioButton.Font = newFont;
            errorSignCheckBox.Font = newFont;
        }

        /// <summary>
        /// Sets the color scheme of the form and its controls.
        /// </summary>
        private void SetColorScheme()
        {
            // Main form
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

            // Title bar
            var color = UITheme.DarkBackground;
            int colorValue = color.R | (color.G << 8) | (color.B << 16);
            DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref colorValue, sizeof(int));

            // Group Boxes
            ChangeLabelColors(groupBox1, UITheme.MenuDarkForeground);
            ChangeLabelColors(groupBox3, UITheme.MenuDarkForeground);
            groupBox1.ForeColor = UITheme.MenuDarkForeground;
            groupBox3.ForeColor = UITheme.MenuDarkForeground;
            ErrorSignGroupBox.ForeColor = UITheme.MenuDarkForeground;
        }

        /// <summary>
        /// Changes the background and foreground colors of TextBox controls within a parent control.
        /// </summary>
        /// <param name="parent">The parent control containing the TextBox controls.</param>
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

        /// <summary>
        /// Changes the foreground color of Label controls within a parent control.
        /// </summary>
        /// <param name="parent">The parent control containing the Label controls.</param>
        /// <param name="color">The color to set for the labels' foreground.</param>
        private void ChangeLabelColors(Control parent, Color color)
        {
            foreach (Control control in parent.Controls)
            {
                if (control is System.Windows.Forms.Label label)
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
            VoiceCheckBox.Checked = Properties.Settings.Default.VoiceEnabled;
            errorSignCheckBox.Checked = Properties.Settings.Default.SignChange;
            historyMakersTextBox.Text = Properties.Settings.Default.historyCount.ToString();
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
                Properties.Settings.Default.VoiceEnabled = VoiceCheckBox.Checked;
                Properties.Settings.Default.SignChange = errorSignCheckBox.Checked;

                // Save settings
                Properties.Settings.Default.Save();

                // Indicate that the form was closed with OK result
                this.DialogResult = DialogResult.OK;
            }
            catch
            {
                // Show warning message if input parsing fails
                DarkMessageBox.Show("Invalid input, try again", "Warning", MessageBoxIcon.Warning, MessageBoxButtons.OK);
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
