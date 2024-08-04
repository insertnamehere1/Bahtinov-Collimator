using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    public partial class DarkMessageBox : Form
    {
        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        // Constants
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 19;

        public DarkMessageBox(string message, string title)
        {
            InitializeComponent();

            this.Text = title;
            messageLabel.Text = message;
            this.BackColor = Color.FromArgb(45, 45, 48); // Dark background color
            messageLabel.ForeColor = Color.White; // Text color

            SetTitleColor();
        }

        private void SetTitleColor()
        {
            // Titlebar
            var color = UITheme.DarkBackground;
            int colorValue = color.R | (color.G << 8) | (color.B << 16);
            DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref colorValue, sizeof(int));
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public static void Show(string message, string title)
        {
            if (Application.OpenForms.Count > 0)
            {
                Form mainForm = Application.OpenForms[0];
                if (mainForm.InvokeRequired)
                {
                    mainForm.Invoke(new Action(() => ShowCustomMessageBoxInternal(message, title)));
                }
                else
                {
                    ShowCustomMessageBoxInternal(message, title);
                }
            }
            else
            {
                // Fallback if there are no open forms
                ShowCustomMessageBoxInternal(message, title);
            }
        }

        private static void ShowCustomMessageBoxInternal(string message, string title)
        {
            using (DarkMessageBox customMessageBox = new DarkMessageBox(message, title))
            {
                customMessageBox.ShowDialog();
            }
        }
    }
}
