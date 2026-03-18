using System;
using System.Diagnostics;
using System.Drawing;
using System.Net;
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

        public Donate()
        {
            InitializeComponent();
            ApplyLocalization();

            // Set Title Bar color
            var color = UITheme.DarkBackground;
            int colorValue = color.R | (color.G << 8) | (color.B << 16);
            DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref colorValue, sizeof(int));
        }

        #endregion

        #region Methods

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

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
                    ShowOfflineDonationText();
                    return;
                }
            }
            else
            {
                ShowOfflineDonationText();
                return;
            }

            BeginInvoke(new Action(ResizeFormToContent));
        }

        private void ResizeFormToContent()
        {
            if (!IsHandleCreated || richTextBox.TextLength == 0) return;

            // Remove bottom anchor so form resize does not trigger anchor resizing of richTextBox
            AnchorStyles savedAnchor = richTextBox.Anchor;
            richTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // Temporarily expand richTextBox so all content is rendered and measurable
            int savedHeight = richTextBox.Height;
            richTextBox.Height = 32000;

            // Find where the last character sits
            Point lastPos = richTextBox.GetPositionFromCharIndex(richTextBox.TextLength - 1);
            int lineHeight = TextRenderer.MeasureText("W", richTextBox.Font).Height;
            int contentHeight = lastPos.Y + lineHeight + 8;

            // Apply the new height and adjust the form by the same delta
            int diff = contentHeight - savedHeight;
            richTextBox.Height = contentHeight;
            Height += diff;

            richTextBox.Anchor = savedAnchor;
        }

        private string GenerateDonationUrl()
        {
            var textPack = UiText.Current;
            string description = Uri.EscapeDataString(textPack.DonatePaypalDescription);
            string business = Uri.EscapeDataString("chris@chrisandbev.com.au");
            string logoUrl = "https://raw.githubusercontent.com/insertnamehere1/Bahtinov-Collimator/refs/heads/master/SkyCal.logo.png";
            string thankYouUrl = "https://yourdomain.com/thankyou";

            return "https://www.paypal.com/cgi-bin/webscr" +
                   "?cmd=_donations" +
                   "&business=" + business +
                   "&lc=AU" +
                   "&item_name=" + description +
                   "&currency_code=USD" +
                   "&no_shipping=1" +
                   "&no_note=0" +
                   "&cn=" + Uri.EscapeDataString(textPack.DonatePaypalOptionalMessage) +
                   "&image_url=" + Uri.EscapeDataString(logoUrl) +
                   "&return=" + Uri.EscapeDataString(thankYouUrl) +
                   "&bn=PP-DonationsBF";
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private static async Task<bool> HasInternetAccessAsync()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                return false;

            const string host = "www.paypal.com";
            const int port = 443;

            try
            {
                var addresses = await Dns.GetHostAddressesAsync(host).ConfigureAwait(false);
                if (addresses == null || addresses.Length == 0)
                    return false;

                using (var client = new TcpClient())
                {
                    var connectTask = client.ConnectAsync(addresses[0], port);
                    var timeoutTask = Task.Delay(2500);

                    var completed = await Task.WhenAny(connectTask, timeoutTask).ConfigureAwait(false);
                    if (completed != connectTask)
                        return false;

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
            string githubLink = "https://insertnamehere1.github.io/Bahtinov-Collimator/";
            richTextBox.Text = string.Format(UiText.Current.DonateRichTextOfflineFormat, githubLink);
            BeginInvoke(new Action(ResizeFormToContent));
        }

        private void ApplyLocalization()
        {
            var textPack = UiText.Current;
            Text = textPack.DonateTitle;
            cancelButton.Text = textPack.DonateCloseButton;
            richTextBox.Text = UiText.Current.DonateRichTextOnline;
        }

        #endregion
    }
}
