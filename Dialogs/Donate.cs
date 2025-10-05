using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    public partial class Donate : Form
    {
        #region DLL Imports

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        #endregion

        #region Constants

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 19;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Donate"/> class.
        /// </summary>
        public Donate()
        {
            InitializeComponent();
            InitializeRichTextBox();
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
            richTextBox.Font = newFont;
            label1.Font = newFont;
            donateButton.Font = newFont;
            cancelButton.Font = newFont;
        }

        /// <summary>
        /// Sets the color scheme of the form and its controls.
        /// </summary>
        private void SetColorScheme()
        {
            // Main form
            this.ForeColor = UITheme.DarkForeground;
            this.BackColor = UITheme.DarkBackground;

            // Donate button
            donateButton.BackColor = UITheme.ButtonDarkBackground;
            donateButton.ForeColor = UITheme.ButtonDarkForeground;
            donateButton.FlatStyle = FlatStyle.Popup;

            // Cancel button
            cancelButton.BackColor = UITheme.ButtonDarkBackground;
            cancelButton.ForeColor = UITheme.ButtonDarkForeground;
            cancelButton.FlatStyle = FlatStyle.Popup;

            // Title bar
            var color = UITheme.DarkBackground;
            int colorValue = color.R | (color.G << 8) | (color.B << 16);
            DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref colorValue, sizeof(int));

            label1.ForeColor = UITheme.DonateTextColor;

            pictureBox1.BackColor = UITheme.DonatePictureBackground;
        }

        /// <summary>
        /// Initializes the rich text box with donation information.
        /// </summary>
        private void InitializeRichTextBox()
        {
            richTextBox.BackColor = UITheme.DarkBackground;
            richTextBox.ForeColor = UITheme.DonateTextColor;
            richTextBox.Text = "If you've found this app useful and it has helped improve your images, please consider donating. \n\nYour contribution is appreciated.";
        }

        /// <summary>
        /// Opens the PayPal donation page in the default web browser.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Button1_Click(object sender, EventArgs e)
        {
            string url = GenerateDonationUrl();
            Process.Start(url);
            Close();
        }

        /// <summary>
        /// Generates the URL for the PayPal donation page.
        /// </summary>
        /// <returns>The URL as a string.</returns>
        private string GenerateDonationUrl()
        {
            string business = "chris@chrisandbev.com.au";
            string description = "SkyCal%20Collimator%20Donation";
            string country = "AU";  // AU, US, etc.
            string currency = "USD";  // AUD, USD, etc.

            return "https://www.paypal.com/cgi-bin/webscr" +
                   "?cmd=_donations" +
                   "&business=" + business +
                   "&lc=" + country +
                   "&item_name=" + description +
                   "&currency_code=" + currency +
                   "&bn=PP%2dDonationsBF";
        }

        /// <summary>
        /// Closes the form when the Cancel button is clicked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        #endregion
    }
}
