using Bahtinov_Collimator;
using Bahtinov_Collimator.Custom_Components;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Reflection.Emit;
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

        private System.Windows.Forms.Label _titleLabel;
        private System.Windows.Forms.Label _bodyLabel;
        private FixedCheckBox _dontShowAgainCheckBox;
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
            Text = UiText.Current.StartupDialogTitle;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            Font = SystemFonts.MessageBoxFont;

            // This dialog builds its UI dynamically, so we must scale all hard-coded pixel values ourselves.
            // (Designer autoscaling only applies to designer-created controls.)
            float s = DeviceDpi / 96f;
            int S(int v) => (int)Math.Round(v * s);

            Padding = new Padding(S(14));
            ClientSize = new Size(S(520), S(250));

            _titleLabel = new System.Windows.Forms.Label
            {
                AutoSize = true,
                Text = UiText.Current.StartupDialogHeading,
                Font = new Font(Font, FontStyle.Bold),
                Location = new Point(S(14), S(54))
            };

            _bodyLabel = new System.Windows.Forms.Label
            {
                AutoSize = false,
                Text = UiText.Current.StartupDialogBody,
                Location = new Point(S(14), _titleLabel.Bottom + S(20)),
                Size = new Size(ClientSize.Width - S(28), S(150))
            };


            _dontShowAgainCheckBox = new FixedCheckBox
            {
                AutoSize = true,
                Text = UiText.Current.StartupDialogDontShowAgain,
                Location = new Point(S(20), _bodyLabel.Bottom + S(10))
            };

            _runButton = new RoundedButton
            {
                Text = UiText.Current.StartupDialogYesButton,
                DialogResult = DialogResult.OK,
                Size = new Size(S(120), S(35)),
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
                Text = UiText.Current.StartupDialogNotNowButton,
                DialogResult = DialogResult.Cancel,
                Size = new Size(S(120), S(35)),
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
            int bottomY = ClientSize.Height - S(14) - _runButton.Height;
            _notNowButton.Location = new Point(ClientSize.Width - S(14) - _notNowButton.Width, bottomY);
            _runButton.Location = new Point(_notNowButton.Left - S(10) - _runButton.Width, bottomY);

            Controls.Add(_titleLabel);
            Controls.Add(_bodyLabel);
            Controls.Add(_dontShowAgainCheckBox);
            Controls.Add(_runButton);
            Controls.Add(_notNowButton);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.Font = new Font(this.Font.FontFamily, UITheme.DialogDefaultFontSize, this.Font.Style);

            _titleLabel.Font = new Font(this.Font.FontFamily, UITheme.GroupBoxFontSize, this.Font.Style);
            _bodyLabel.Font = new Font(this.Font.FontFamily, UITheme.GroupBoxFontSize, this.Font.Style);
            _dontShowAgainCheckBox.Font = new Font(this.Font.FontFamily, UITheme.CheckBoxFontSize, this.Font.Style);
            _runButton.Font = new Font(this.Font.FontFamily, UITheme.ButtonFontSize, this.Font.Style);
            _notNowButton.Font = new Font(this.Font.FontFamily, UITheme.ButtonFontSize, this.Font.Style);

            PerformLayout();
            BeginInvoke(new Action(ResizeFormToContent));
        }

        private void ResizeFormToContent()
        {
            if (!IsHandleCreated || string.IsNullOrEmpty(_bodyLabel.Text)) return;

            AnchorStyles savedAnchor = _bodyLabel.Anchor;
            _bodyLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            Size contentSize = TextRenderer.MeasureText(
                _bodyLabel.Text,
                _bodyLabel.Font,
                new Size(_bodyLabel.Width, int.MaxValue),
                TextFormatFlags.WordBreak);

            _bodyLabel.Height = contentSize.Height;
            float s = DeviceDpi / 96f;
            int S(int v) => (int)Math.Round(v * s);
            _dontShowAgainCheckBox.Location = new Point(S(20), _bodyLabel.Bottom + S(20));
            
            _notNowButton.Location = new Point(ClientSize.Width - S(20) - _notNowButton.Width, _dontShowAgainCheckBox.Bottom + S(20));
            _runButton.Location = new Point(_notNowButton.Left - S(10) - _runButton.Width, _dontShowAgainCheckBox.Bottom + S(20));
            Height = _runButton.Bottom + S(20);

            _bodyLabel.Anchor = savedAnchor;
        }

    }
}
