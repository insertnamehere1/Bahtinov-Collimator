using System;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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
        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            float increasedSize = this.Font.Size + 2.0f;
            Font newFont = new Font(this.Font.FontFamily, increasedSize, this.Font.Style);

            this.Font = newFont;
            richTextBox.Font = newFont;
            label1.Font = newFont;
            cancelButton.Font = newFont;

            // Decide online vs offline
            bool hasInternet = await HasInternetAccessAsync().ConfigureAwait(true);

            if (hasInternet)
            {
                string url = GenerateDonationUrl();
                try
                {
                    Process.Start(url);
                }
                catch
                {
                    // If Process.Start fails for any reason, fall back to offline text
                    ShowOfflineDonationText();
                }
            }
            else
            {
                ShowOfflineDonationText();
            }
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
            cancelButton.CornerRadius = 4;
            cancelButton.TextOffsetX = 0;
            cancelButton.BevelDark = Color.FromArgb(180, 90, 90, 90);
            cancelButton.BevelLight = Color.FromArgb(220, 160, 160, 160);



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

        private static async Task<bool> HasInternetAccessAsync()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                return false;

            // Check the thing you actually need to reach
            const string host = "www.paypal.com";
            const int port = 443;

            try
            {
                // 1) DNS resolution
                var addresses = await Dns.GetHostAddressesAsync(host).ConfigureAwait(false);
                if (addresses == null || addresses.Length == 0)
                    return false;

                // 2) TCP connect with a short timeout
                using (var client = new TcpClient())
                {
                    var connectTask = client.ConnectAsync(addresses[0], port);
                    var timeoutTask = Task.Delay(2500);

                    var completed = await Task.WhenAny(connectTask, timeoutTask).ConfigureAwait(false);
                    if (completed != connectTask)
                        return false;

                    // If ConnectAsync faulted, this will throw here
                    await connectTask.ConfigureAwait(false);

                    return client.Connected;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return false;
            }
        }

        private void ShowOfflineDonationText()
        {
            // Don’t auto-launch browser; show clear instructions instead.
            // You can include your PayPal URL so they can copy it for later.
            string paypalLink = GenerateDonationUrl();
            string githubLink = "https://insertnamehere1.github.io/Bahtinov-Collimator/";

            richTextBox.Text =
                "Thank you for using SkyCal!\n\n" +
                "It looks like this computer does not currently have internet access, so the donation page\n" +
                "can’t be opened automatically.\n\n" +
                "To donate, please use any internet-connected device and visit:\n\n" +
                githubLink + "\n\n" +
                "Alternatively, you can email me and I can provide offline-friendly options:\n" +
                "chris@chrisandbev.com.au\n\n" +
                "Clear Skies and Sharp Stars\n" +
                "Chris Dowd,\n" +
                "Developer of SkyCal";
        }


        #endregion
    }
}
