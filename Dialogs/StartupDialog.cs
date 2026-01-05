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
    public sealed class StartupDialog : Form
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
        private void InitializeComponent()
        {
            Text = "SkyCal";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = SystemFonts.MessageBoxFont;

            Padding = new Padding(14);
            ClientSize = new Size(520, 220);

            _titleLabel = new Label();
            _titleLabel.AutoSize = true;
            _titleLabel.Text = "Recommended: Run Settings Calibration";
            _titleLabel.Font = new Font(Font, FontStyle.Bold);
            _titleLabel.Location = new Point(14, 14);

            _bodyLabel = new Label();
            _bodyLabel.AutoSize = false;
            _bodyLabel.Text =
                "SkyCal works best when the Settings Calibration has been run.\r\n" +
                "This helps tune capture and measurement thresholds for your setup.\r\n\r\n" +
                "Would you like to run it now?";
            _bodyLabel.Location = new Point(14, _titleLabel.Bottom + 10);
            _bodyLabel.Size = new Size(ClientSize.Width - 28, 110);

            _dontShowAgainCheckBox = new CheckBox();
            _dontShowAgainCheckBox.AutoSize = true;
            _dontShowAgainCheckBox.Text = "Do not show again";
            _dontShowAgainCheckBox.Location = new Point(14, _bodyLabel.Bottom + 6);

            _runButton = new RoundedButton();
            _runButton.Text = "Run Calibration";
            _runButton.DialogResult = DialogResult.OK;
            _runButton.Size = new Size(120, 35);
            _runButton.CornerRadius = 4;
            _runButton.BackColor = UITheme.ButtonDarkBackground;
            _runButton.ForeColor = UITheme.ButtonDarkForeground;
            _runButton.FlatStyle = FlatStyle.Popup;
            _runButton.TextOffsetX = 0;
            _runButton.BevelDark = Color.FromArgb(180, 90, 90, 90);
            _runButton.BevelLight = Color.FromArgb(220, 160, 160, 160);

            _notNowButton = new RoundedButton();
            _notNowButton.Text = "Not now";
            _notNowButton.DialogResult = DialogResult.Cancel;
            _notNowButton.Size = new Size(120, 35);
            _notNowButton.CornerRadius = 4;
            _notNowButton.BackColor = UITheme.ButtonDarkBackground;
            _notNowButton.ForeColor = UITheme.ButtonDarkForeground;
            _notNowButton.FlatStyle = FlatStyle.Popup;
            _notNowButton.TextOffsetX = 0;
            _notNowButton.BevelDark = Color.FromArgb(180, 90, 90, 90);
            _notNowButton.BevelLight = Color.FromArgb(220, 160, 160, 160);

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
