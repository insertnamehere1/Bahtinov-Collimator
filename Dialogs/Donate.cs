using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    public partial class Donate : Form
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Donate"/> class.
        /// </summary>
        public Donate()
        {
            InitializeComponent();
            InitializeRichTextBox();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the rich text box with donation information.
        /// </summary>
        private void InitializeRichTextBox()
        {
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
            string description = "Bahtinov%20Collimator%20Donation";
            string country = "AU";  // AU, US, etc.
            string currency = "AUD";  // AUD, USD, etc.

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