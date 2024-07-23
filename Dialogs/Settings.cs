using System;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    public partial class Settings : Form
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        public Settings()
        {
            InitializeComponent();
            LoadSettings();
        }

        #endregion

        #region Methods

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
