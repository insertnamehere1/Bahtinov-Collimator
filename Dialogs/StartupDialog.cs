using Bahtinov_Collimator;
using Bahtinov_Collimator.Custom_Components;
using System;
using System.Drawing;
using System.Drawing.Design;
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

        private Label _titleLabel;
        private Label _bodyLabel;
        private CheckBox _dontShowAgainCheckBox;
        private RoundedButton _runButton;
        private RoundedButton _notNowButton;

        /// <summary>
        /// True if the user ticked "Do not show again".
        /// </summary>
        public bool DontShowAgain
        {
            get { return _dontShowAgainCheckBox != null && _dontShowAgainCheckBox.Checked; }
        }

        /// <summary>
        /// Creates the startup calibration prompt dialog.
        /// </summary>
        public StartupDialog()
        {
            InitializeComponent();
            UISetup();

            var color = UITheme.DarkBackground;
            int colorValue = color.R | (color.G << 8) | (color.B << 16);
            DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref colorValue, sizeof(int));

            this.BackColor = UITheme.DarkBackground;
            ForeColor = UITheme.MessageBoxTextColor;

            _runButton.BackColor = UITheme.ButtonDarkBackground;
            _runButton.ForeColor = UITheme.ButtonDarkForeground;
            _runButton.FlatStyle = FlatStyle.Popup;

            _notNowButton.BackColor = UITheme.ButtonDarkBackground;
            _notNowButton.ForeColor = UITheme.ButtonDarkForeground;
            _notNowButton.FlatStyle = FlatStyle.Popup;
        }

        /// <summary>
        /// Builds the dialog UI.
        /// </summary>
        private void UISetup()
        {
            Text = "SkyCal Calibration";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = SystemFonts.MessageBoxFont;

            Padding = new Padding(14);
            ClientSize = new Size(520, 250);

            _titleLabel = new Label
            {
                AutoSize = true,
                Text = "Recommended",
                Font = new Font(Font, FontStyle.Bold),
                Location = new Point(14, 14)
            };

            _bodyLabel = new Label
            {
                AutoSize = false,
                Text = "Calibration enables full SkyCal functionality. Some features are unavailable until calibration is completed." + 
                " (Calibration is also available from the Setup menu.)\r\n\r\n" +
                "Would you like to run it now?",
                Location = new Point(14, _titleLabel.Bottom + 10),
                Size = new Size(ClientSize.Width - 28, 150)
            };


            _dontShowAgainCheckBox = new CheckBox
            {
                AutoSize = true,
                Text = "Do not show again",
                Location = new Point(20, _bodyLabel.Bottom + 10)
            };

            _runButton = new RoundedButton
            {
                Text = "Yes",
                DialogResult = DialogResult.OK,
                Size = new Size(120, 35),
                CornerRadius = 4,
                BackColor = UITheme.ButtonDarkBackground,
                ForeColor = UITheme.ButtonDarkForeground,
                FlatStyle = FlatStyle.Popup,
                TextOffsetX = 0,
                BevelDark = Color.FromArgb(180, 90, 90, 90),
                BevelLight = Color.FromArgb(220, 160, 160, 160)
            };

            _notNowButton = new RoundedButton
            {
                Text = "Not now",
                DialogResult = DialogResult.Cancel,
                Size = new Size(120, 35),
                CornerRadius = 4,
                BackColor = UITheme.ButtonDarkBackground,
                ForeColor = UITheme.ButtonDarkForeground,
                FlatStyle = FlatStyle.Popup,
                TextOffsetX = 0,
                BevelDark = Color.FromArgb(180, 90, 90, 90),
                BevelLight = Color.FromArgb(220, 160, 160, 160)
            };

            AcceptButton = _runButton;
            CancelButton = _notNowButton;

            // Button placement (bottom-right)
            int bottomY = ClientSize.Height - 14 - _runButton.Height;
            _notNowButton.Location = new Point(ClientSize.Width - 14 - _notNowButton.Width, bottomY);
            _runButton.Location = new Point(_notNowButton.Left - 10 - _runButton.Width, bottomY);

            Controls.Add(_titleLabel);
            Controls.Add(_bodyLabel);
            Controls.Add(_dontShowAgainCheckBox);
            Controls.Add(_runButton);
            Controls.Add(_notNowButton);
        }
    }
}

