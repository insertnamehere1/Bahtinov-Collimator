using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    public partial class Donate : Form
    {
        public Donate()
        {
            InitializeComponent();

            // Set the label text
            richTextBox.Text = "If you've found this app useful, or it has helped improve your images, please consider donating. \n\nYour contribution is appreciated.";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string url = "";

            string business = "chris@chrisandbev.com.au";
            string description = "Bahtinov%20Collimator%20Donation";
            string country = "AU";                      // AU, US, etc.
            string currency = "AUD";                    // AUD, USD, etc.

            url += "https://www.paypal.com/cgi-bin/webscr" +
                "?cmd=" + "_donations" +
                "&business=" + business +
                "&lc=" + country +
                "&item_name=" + description +
                "&currency_code=" + currency +
                "&bn=" + "PP%2dDonationsBF";

            System.Diagnostics.Process.Start(url);

            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
