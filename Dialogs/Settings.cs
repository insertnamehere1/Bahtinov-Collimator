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

        // settings value for focal length
        private float focalLength = 0.0f;

        // settings value for aperture
        private float aperture = 0.0f;

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

            // Set the CriticalFocusLabel properties for left expanding text
            CriticalFocusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            CriticalFocusLabel.RightToLeft = RightToLeft.Yes;
            CriticalFocusLabel.AutoSize = false; 

            // Subscribe to the TextChanged events
            FocalLengthTextBox.TextChanged += FocalLengthTextBox_TextChanged;
            ApertureTextBox.TextChanged += ApertureTextBox_TextChanged;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles the TextChanged event for the FocalLengthTextBox.
        /// This method is called whenever the text in the FocalLengthTextBox changes.
        /// It triggers the update of the critical focus calculation to reflect the new focal length input.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void FocalLengthTextBox_TextChanged(object sender, EventArgs e)
        {
            UpdateCriticalFocus();
        }

        /// <summary>
        /// Handles the TextChanged event for the ApertureTextBox.
        /// This method is called whenever the text in the ApertureTextBox changes.
        /// It triggers the update of the critical focus calculation to reflect the new aperture input.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void ApertureTextBox_TextChanged(object sender, EventArgs e)
        {
            UpdateCriticalFocus();
        }

        /// <summary>
        /// Updates the critical focus based on the current values of the focal length and aperture inputs.
        /// This method attempts to parse the input values from the respective text boxes.
        /// If valid numeric values are provided and the aperture is greater than zero,
        /// it updates the corresponding class variables and recalculates the critical focus.
        /// If the input is invalid, it sets the CriticalFocusLabel to indicate an error.
        /// </summary>
        private void UpdateCriticalFocus()
        {
            // Try to parse the input values
            if (float.TryParse(FocalLengthTextBox.Text, out float newFocalLength) &&
                float.TryParse(ApertureTextBox.Text, out float newAperture) &&
                newAperture > 0) // Ensure the aperture is greater than zero
            {
                // Update the class variables
                focalLength = newFocalLength;
                aperture = newAperture;

                // Calculate and update the critical focus label
                CalculateCriticalFocus();
            }
            else
            {
                // Optionally, you can clear or reset the critical focus label if input is invalid
                CriticalFocusLabel.Text = "Invalid Input";
            }
        }

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
            groupBox2.Font = newFont;
            label1.Font = newFont;
            label2.Font = newFont;
            label3.Font = newFont;
            label4.Font = newFont;
            label5.Font = newFont;
            label6.Font = newFont;
            label7.Font = newFont;
            label8.Font = newFont;
            label9.Font = newFont;
            label10.Font = newFont;
            label11.Font = newFont;
            label13.Font = newFont;
            CriticalFocusLabel.Font = newFont;
            ApertureTextBox.Font = newFont;
            PixelSizeTextBox.Font = newFont;
            FocalLengthTextBox.Font = newFont;
            CancelSettingsButton.Font = newFont;
            okButton.Font = newFont;
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
            ChangeLabelColors(groupBox2, UITheme.MenuDarkForeground);
            groupBox1.ForeColor = UITheme.MenuDarkForeground;
            groupBox2.ForeColor = UITheme.MenuDarkForeground;

            ChangeTextBoxColors(groupBox2);
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
            this.aperture = Properties.Settings.Default.Aperture;
            this.focalLength = Properties.Settings.Default.FocalLength;
            ApertureTextBox.Text = aperture.ToString("F0");
            FocalLengthTextBox.Text = focalLength.ToString("F0");
            PixelSizeTextBox.Text = Properties.Settings.Default.PixelSize.ToString("F2");
            VoiceCheckBox.Checked = Properties.Settings.Default.VoiceEnabled;

            CalculateCriticalFocus();
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

        private void CalculateCriticalFocus()
        {
            float focalApertureRatio = focalLength / aperture;

            float criticalFocusValue = 8.99999974990351E-01f * focalApertureRatio * focalApertureRatio;

            CriticalFocusLabel.Text = criticalFocusValue.ToString("F0");
        }

        #endregion
    }
}
