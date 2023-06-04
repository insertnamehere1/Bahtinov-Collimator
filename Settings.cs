using System;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();

            ApertureTextBox.Text = Properties.Settings.Default.Aperture.ToString("F0");
            FocalLengthTextBox.Text = Properties.Settings.Default.FocalLength.ToString("F0");
            PixelSizeTextBox.Text = Properties.Settings.Default.PixelSize.ToString("F2");
            RedCheckBox.Checked = Properties.Settings.Default.RedChannel;
            GreenCheckBox.Checked = Properties.Settings.Default.GreenChannel;
            BlueCheckBox.Checked = Properties.Settings.Default.BlueChannel;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            try
            {
                Properties.Settings.Default.Aperture = int.Parse(ApertureTextBox.Text);
                Properties.Settings.Default.FocalLength = int.Parse(FocalLengthTextBox.Text);
                Properties.Settings.Default.PixelSize = float.Parse(PixelSizeTextBox.Text);
                Properties.Settings.Default.RedChannel = RedCheckBox.Checked;
                Properties.Settings.Default.GreenChannel = GreenCheckBox.Checked;
                Properties.Settings.Default.BlueChannel = BlueCheckBox.Checked;
            }
            catch 
            {
                MessageBox.Show("Invalid input, try again", "Warning",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Properties.Settings.Default.Save();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
