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
    /// <summary>
    /// Dialog that opens the donation flow and falls back to offline instructions when needed.
    /// </summary>
    public partial class Donate : Form
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

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Donate"/> dialog.
        /// </summary>
        public Donate()
        {
            InitializeComponent();
            ApplyLocalization();

            var color = UITheme.DarkBackground;
            int colorValue = color.R | (color.G << 8) | (color.B << 16);
            DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref colorValue, sizeof(int));
        }

        #endregion

        #region Lifecycle and Layout

        /// <summary>
        /// Runs after <see cref="DpiAwareDialog.ShowDialogDpiAware"/> has
        /// moved the dialog to the owner's monitor and WinForms has finished
        /// per-monitor DPI scaling. Explicitly re-applies the left-column
        /// layout so the <c>pictureBox1</c> SkyCal logo is guaranteed to fill
        /// the full designed vertical extent and so <c>pictureBox2</c> (the
        /// PayPal icon overlaid at the top of <c>pictureBox1</c>) is visibly
        /// above it in the z-order. On high-DPI secondary monitors the
        /// designer's mixed anchors + BeginInit/EndInit resource-loading
        /// sequence sometimes leaves pictureBox1 mis-sized at zero or near-
        /// zero height, which manifested as the logo simply not being drawn.
        /// Setting Bounds here forces a known-good layout at the final DPI.
        /// </summary>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Keep the existing (DPI-scaled) widths, but pin pictureBox1 to
            // the full height of the client area so the logo is always drawn
            // at its full designed size regardless of monitor DPI.
            int pb1Width = pictureBox1.Width;
            pictureBox1.Bounds = new Rectangle(0, 1, pb1Width, Math.Max(1, ClientSize.Height - 1));

            // Ensure the PayPal icon stays in front of the SkyCal logo so it
            // isn't hidden when we resize pictureBox1 above.
            pictureBox2.BringToFront();
        }

        /// <summary>
        /// Loads the dialog, checks connectivity, and launches or falls back from the online donation flow.
        /// </summary>
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

        /// <summary>
        /// Resizes the form so the rich-text content fits without scrolling.
        /// The form is only allowed to GROW - never shrink below its
        /// designer/DPI-scaled size - so the left-side images
        /// (<c>pictureBox1</c>, which is anchored Top|Bottom|Left) always have
        /// the full designed vertical space and don't end up squashed at the
        /// top of the dialog. This matters especially on high-DPI secondary
        /// monitors where the text box is proportionally wider, the text
        /// wraps into fewer lines, and the measured content height is much
        /// smaller than the baseline richTextBox height.
        /// </summary>
        private void ResizeFormToContent()
        {
            if (!IsHandleCreated || richTextBox.TextLength == 0) return;

            AnchorStyles savedAnchor = richTextBox.Anchor;
            richTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            int savedHeight = richTextBox.Height;
            richTextBox.Height = 32000;

            Point lastPos = richTextBox.GetPositionFromCharIndex(richTextBox.TextLength - 1);

            // Measure using the richTextBox's own device context so the line
            // height is in the SAME DPI space as lastPos.Y. The parameterless
            // TextRenderer.MeasureText overload falls back to a 96-DPI screen
            // DC, which would mix coordinate spaces on a per-monitor-DPI
            // display and under-report contentHeight on e.g. 168-DPI monitors.
            int lineHeight;
            using (Graphics g = richTextBox.CreateGraphics())
            {
                lineHeight = TextRenderer.MeasureText(g, "W", richTextBox.Font).Height;
            }

            int contentHeight = lastPos.Y + lineHeight + 8;
            int diff = contentHeight - savedHeight;

            if (diff > 0)
            {
                richTextBox.Height = contentHeight;
                Height += diff;
            }
            else
            {
                richTextBox.Height = savedHeight;
            }

            richTextBox.Anchor = savedAnchor;
        }

        #endregion

        #region Donation Flow Helpers

        /// <summary>
        /// Composes the PayPal donation URL with localized metadata.
        /// </summary>
        /// <returns>A complete PayPal donation URL string.</returns>
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

        /// <summary>
        /// Handles the close button click by closing the dialog.
        /// </summary>
        private void Button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Tests whether internet connectivity is available for opening the donation endpoint.
        /// </summary>
        /// <returns>True when internet access appears available; otherwise false.</returns>
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

        /// <summary>
        /// Displays offline donation instructions when the online flow is unavailable.
        /// </summary>
        private void ShowOfflineDonationText()
        {
            string githubLink = "https://insertnamehere1.github.io/Bahtinov-Collimator/";
            richTextBox.Text = string.Format(UiText.Current.DonateRichTextOfflineFormat, githubLink);
            BeginInvoke(new Action(ResizeFormToContent));
        }

        /// <summary>
        /// Applies localized text for controls in the donation dialog.
        /// </summary>
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
