using Bahtinov_Collimator;
using Bahtinov_Collimator.Custom_Components;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SkyCal
{
    /// <summary>
    /// Startup dialog that suggests running the Settings Calibration.
    /// Includes a "Do not show again" checkbox.
    /// The caller decides what to do with the checkbox state and persists it.
    /// </summary>
    public partial class StartupDialog : Form
    {

        #region DLL Imports

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        #endregion

        #region Constants

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 19;

        #endregion

        /// <summary>
        /// Creates the startup calibration prompt dialog.
        /// </summary>
        public StartupDialog()
        {
            InitializeComponent();
            UISetup();

            // Setup dark mode title bar
            var color = UITheme.DarkBackground;
            int colorValue = color.R | (color.G << 8) | (color.B << 16);
            DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref colorValue, sizeof(int));
        }

        /// <summary>
        /// True if the user ticked "Do not show again".
        /// </summary>
        public bool DontShowAgain
        {
            get { return dontShowAgainCheckBox != null && dontShowAgainCheckBox.Checked; }
        }

        /// <summary>
        /// Builds the dialog UI.
        /// </summary>
        private void UISetup()
        {
            this.StartPosition = FormStartPosition.CenterParent;

            this.Text = UiText.Current.StartupDialogTitle;
            runButton.Text = UiText.Current.StartupDialogYesButton;
            titleLabel.Text = UiText.Current.StartupDialogHeading;
            bodyLabel.Text = UiText.Current.StartupDialogBody;
            dontShowAgainCheckBox.Text = UiText.Current.StartupDialogDontShowAgain;
            notNowButton.Text = UiText.Current.StartupDialogNotNowButton;
        }
    }
}
