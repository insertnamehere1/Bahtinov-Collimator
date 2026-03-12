using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    public partial class Settings : DpiAwareForm
    {
        #region DLL Imports

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        #endregion

        #region Constants

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 19;

        #endregion

        #region Variables
        public bool RestartCapture { get; private set; }
        
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        public Settings()
        {
            this.ShowMinimizeMaximize = false;
            InitializeComponent();
            ApplyLocalization();
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

            // Adjust fonts
            this.Font = new Font(this.Font.FontFamily, UITheme.DialogDefaultFontSize, this.Font.Style);
            groupBox1.Font = new Font(this.Font.FontFamily, UITheme.GroupBoxFontSize, this.Font.Style);
            label5.Font = new Font(this.Font.FontFamily, UITheme.LabelFontSize, this.Font.Style);
            CancelSettingsButton.Font = new Font(this.Font.FontFamily, UITheme.ButtonFontSize, this.Font.Style);
            okButton.Font = new Font(this.Font.FontFamily, UITheme.ButtonFontSize, this.Font.Style);
            SCTRadioButton.Font = new Font(this.Font.FontFamily, UITheme.CheckBoxFontSize, this.Font.Style);
            MakCassRadioButton.Font = new Font(this.Font.FontFamily, UITheme.CheckBoxFontSize, this.Font.Style);
            errorSignCheckBox.Font = new Font(this.Font.FontFamily, UITheme.CheckBoxFontSize, this.Font.Style);
            onTopCheckBox.Font = new Font(this.Font.FontFamily, UITheme.CheckBoxFontSize, this.Font.Style);
            minimizeCheckBox.Font = new Font(this.Font.FontFamily, UITheme.CheckBoxFontSize, this.Font.Style);
            minimizeLabel.Font = new Font(this.Font.FontFamily, UITheme.LabelFontSize, this.Font.Style);
            VoiceCheckBox.Font = new Font(this.Font.FontFamily, UITheme.CheckBoxFontSize, this.Font.Style);

            SCTRadioButton.BoxSize = 20;
            MakCassRadioButton.BoxSize = 20;
            newtonianRadioButton.BoxSize = 20;
            errorSignCheckBox.BoxSize = 20;
            onTopCheckBox.BoxSize = 20;
            VoiceCheckBox.BoxSize = 20;
            minimizeCheckBox.BoxSize = 20;


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
            okButton.CornerRadius = 4;
            okButton.TextOffsetX = 0;
            okButton.BevelDark = Color.FromArgb(180, 90, 90, 90);
            okButton.BevelLight = Color.FromArgb(220, 160, 160, 160);

            // Cancel button
            CancelSettingsButton.BackColor = UITheme.ButtonDarkBackground;
            CancelSettingsButton.ForeColor = UITheme.ButtonDarkForeground;
            CancelSettingsButton.FlatStyle = FlatStyle.Popup;
            CancelSettingsButton.CornerRadius = 4;
            CancelSettingsButton.TextOffsetX = 0;
            CancelSettingsButton.BevelDark = Color.FromArgb(180, 90, 90, 90);
            CancelSettingsButton.BevelLight = Color.FromArgb(220, 160, 160, 160);

            // Title bar
            var color = UITheme.DarkBackground;
            int colorValue = color.R | (color.G << 8) | (color.B << 16);
            DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref colorValue, sizeof(int));

            // Group Boxes
            ChangeLabelColors(groupBox1, UITheme.MenuDarkForeground);
            ChangeLabelColors(groupBox3, UITheme.MenuDarkForeground);
            ChangeLabelColors(groupBox4, UITheme.MenuDarkForeground);
            groupBox1.ForeColor = UITheme.MenuDarkForeground;
            groupBox3.ForeColor = UITheme.MenuDarkForeground;
            groupBox4.ForeColor = UITheme.MenuDarkForeground;
            ErrorSignGroupBox.ForeColor = UITheme.MenuDarkForeground;
            keepOnTopGroupBox.ForeColor = UITheme.MenuDarkForeground;
            minimizeGroupBox.ForeColor = UITheme.MenuDarkForeground;

            // text boxes
            ChangeTextBoxColors(groupBox4);
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
            SCTRadioButton.Checked = Properties.Settings.Default.SCTSelected;
            MakCassRadioButton.Checked = Properties.Settings.Default.MCTSelected;
            onTopCheckBox.Checked = Properties.Settings.Default.KeepOnTop;
            minimizeCheckBox.Checked = Properties.Settings.Default.Minimize;
        }

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

        /// <summary>
        /// Saves the settings when the OK button is clicked and closes the form.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OkButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (Properties.Settings.Default.VoiceEnabled != VoiceCheckBox.Checked ||
                    Properties.Settings.Default.SignChange != errorSignCheckBox.Checked ||
                    Properties.Settings.Default.historyCount != int.Parse(historyMakersTextBox.Text) ||
                    Properties.Settings.Default.SCTSelected != SCTRadioButton.Checked ||
                    Properties.Settings.Default.MCTSelected != MakCassRadioButton.Checked)
                {
                    RestartCapture = true;
                }
                else
                    RestartCapture = false;

                // Parse input values and save them to application settings
                Properties.Settings.Default.VoiceEnabled = VoiceCheckBox.Checked;
                Properties.Settings.Default.SignChange = errorSignCheckBox.Checked;
                Properties.Settings.Default.historyCount = int.Parse(historyMakersTextBox.Text);
                Properties.Settings.Default.SCTSelected = SCTRadioButton.Checked;
                Properties.Settings.Default.MCTSelected = MakCassRadioButton.Checked;
                Properties.Settings.Default.KeepOnTop = onTopCheckBox.Checked;
                Properties.Settings.Default.Minimize = minimizeCheckBox.Checked;

                // Save settings
                Properties.Settings.Default.Save();

                // display new image
                ImageCapture.ForceImageUpdate();

                // Indicate that the form was closed with OK result
                this.DialogResult = DialogResult.OK;
            }
            catch
            {
                // Show warning message if input parsing fails
                DarkMessageBox.Show(UiText.Current.SettingsInvalidInputMessage, UiText.Current.SettingsInvalidInputTitle, MessageBoxIcon.Warning, MessageBoxButtons.OK);
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
