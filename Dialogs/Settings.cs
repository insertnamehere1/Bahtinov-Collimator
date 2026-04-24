using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    public partial class Settings : Form
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

        #region Public State

        /// <summary>
        /// Gets a value indicating whether changing settings requires capture components to restart.
        /// </summary>
        public bool RestartCapture { get; private set; }
        
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        public Settings()
        {
            InitializeComponent();
            ApplyLocalization();
            LoadSettings();

            var color = UITheme.DarkBackground;
            int colorValue = color.R | (color.G << 8) | (color.B << 16);
            DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref colorValue, sizeof(int));
        }

        #endregion

        #region Initialization Helpers

        /// <summary>
        /// Changes the foreground color of Label controls within a parent control.
        /// </summary>
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
            SCTRadioButton.Checked = Properties.Settings.Default.SCTSelected;
            MakCassRadioButton.Checked = Properties.Settings.Default.MCTSelected;
            newtonianRadioButton.Checked = Properties.Settings.Default.NewtSelected;
            guidanceOffRadioButton.Checked = Properties.Settings.Default.NoneSelected;
            onTopCheckBox.Checked = Properties.Settings.Default.KeepOnTop;
            minimizeCheckBox.Checked = Properties.Settings.Default.Minimize;
        }

        /// <summary>
        /// Applies localized strings to all text-bearing controls in the settings dialog.
        /// </summary>
        private void ApplyLocalization()
        {
            var textPack = UiText.Current;
            Text = textPack.SettingsTitle;
            groupBox1.Text = textPack.SettingsGroupVoiceTitle;
            groupBox3.Text = textPack.SettingsGroupGuidanceTitle;
            groupBox4.Text = textPack.SettingsGroupHistoryTitle;
            ErrorSignGroupBox.Text = textPack.SettingsGroupCalibrationTitle;
            VoiceCheckBox.Text = textPack.SettingsVoiceGuidanceLabel;
            label5.Text = textPack.SettingsVoiceGuidanceDescription;
            SCTRadioButton.Text = textPack.SettingsGuidanceSct;
            MakCassRadioButton.Text = textPack.SettingsGuidanceMakCass;
            newtonianRadioButton.Text = textPack.SettingsGuidanceNewtonian;
            guidanceOffRadioButton.Text = textPack.SettingsGuidanceOff;
            errorSignCheckBox.Text = textPack.SettingsCalibrationSignSwitch;
            label2.Text = textPack.SettingsCalibrationDescription;
            label1.Text = textPack.SettingsHistoryMarkersLabel;
            label3.Text = textPack.SettingsHistoryMarkersDescription;
            okButton.Text = textPack.SettingsSaveButton;
            CancelSettingsButton.Text = textPack.SettingsCancelButton;
            onTopLabel.Text = textPack.SettingsKeepOnTopLabel;
            onTopCheckBox.Text = textPack.SettingsKeepOnTopDescription;
            keepOnTopGroupBox.Text = textPack.SettingsGroupWindowTitle;

            minimizeGroupBox.Text = textPack.SettingsGroupMinimizeTitle;
            minimizeCheckBox.Text = textPack.SettingsMinimizeDescription;
            minimizeLabel.Text = textPack.SettingsMinimizeLabel;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Saves the settings when the OK button is clicked and closes the form.
        /// </summary>
        private void OkButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (Properties.Settings.Default.VoiceEnabled != VoiceCheckBox.Checked ||
                    Properties.Settings.Default.SignChange != errorSignCheckBox.Checked ||
                    Properties.Settings.Default.historyCount != int.Parse(historyMakersTextBox.Text) ||
                    Properties.Settings.Default.SCTSelected != SCTRadioButton.Checked ||
                    Properties.Settings.Default.MCTSelected != MakCassRadioButton.Checked ||
                    Properties.Settings.Default.NewtSelected != newtonianRadioButton.Checked ||
                    Properties.Settings.Default.NoneSelected != guidanceOffRadioButton.Checked)
                {
                    RestartCapture = true;
                }
                else
                    RestartCapture = false;

                Properties.Settings.Default.VoiceEnabled = VoiceCheckBox.Checked;
                Properties.Settings.Default.SignChange = errorSignCheckBox.Checked;
                Properties.Settings.Default.historyCount = int.Parse(historyMakersTextBox.Text);
                Properties.Settings.Default.SCTSelected = SCTRadioButton.Checked;
                Properties.Settings.Default.MCTSelected = MakCassRadioButton.Checked;
                Properties.Settings.Default.NewtSelected = newtonianRadioButton.Checked;
                Properties.Settings.Default.NoneSelected = guidanceOffRadioButton.Checked;

                Properties.Settings.Default.KeepOnTop = onTopCheckBox.Checked;
                Properties.Settings.Default.Minimize = minimizeCheckBox.Checked;

                Properties.Settings.Default.Save();

                ImageCapture.ForceImageUpdate();

                this.DialogResult = DialogResult.OK;
            }
            catch
            {
                DarkMessageBox.Show(UiText.Current.SettingsInvalidInputMessage, UiText.Current.SettingsInvalidInputTitle, MessageBoxIcon.Warning, MessageBoxButtons.OK);
                return;
            }

            this.Close();
        }

        /// <summary>
        /// Closes the form with a Cancel result when the Cancel button is clicked.
        /// </summary>
        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        #endregion
    }
}
