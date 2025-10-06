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
            cancelButton.Font = newFont;

            string url = GenerateDonationUrl();
            Process.Start(url);
        }

        /// <summary>
        /// Sets the color scheme of the form and its controls.
        /// </summary>
        private void SetColorScheme()
        {
            // Main form
            this.ForeColor = UITheme.DarkForeground;
            this.BackColor = UITheme.DarkBackground;

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

            richTextBox.Text = "Thank you for using SkyCal!\n\n" +
            "If SkyCal has helped you achieve sharper focus, easier collimation, or simply made\n" +
            "your nights under the stars a little more rewarding, please consider showing your \n" +
            "support. Every coffee keeps the project alive, keeps me awake while writing manuals,\n" +
            "helps me keep improving the software, and reminds me that the effort was worthwhile.\n\n" +
            "Clear Skies and Sharp Stars\n" +
            "Chris Dowd,\n" +
            "Developer of SkyCal";
        }

        /// <summary>
        /// Generates the URL for the PayPal donation page.
        /// </summary>
        /// <returns>The URL as a string.</returns>
        private string GenerateDonationUrl()
        {
            string business = Uri.EscapeDataString("chris@chrisandbev.com.au");
            string description = Uri.EscapeDataString("Buy Chris a Coffee – SkyCal Project");
            string country = "AU";
            string currency = "USD";
            string logoUrl = "https://raw.githubusercontent.com/insertnamehere1/Bahtinov-Collimator/refs/heads/master/SkyCal.logo.png";
            string thankYouUrl = "https://yourdomain.com/thankyou";

            return "https://www.paypal.com/cgi-bin/webscr" +
                   "?cmd=_donations" +
                   "&business=" + business +
                   "&lc=" + country +
                   "&item_name=" + description +
                   "&currency_code=" + currency +
                   "&no_shipping=1" +
                   "&no_note=0" +
                   "&cn=Leave%20a%20message%20(optional)" +
                   "&image_url=" + Uri.EscapeDataString(logoUrl) +
                   "&return=" + Uri.EscapeDataString(thankYouUrl) +
                   "&bn=PP-DonationsBF";
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
