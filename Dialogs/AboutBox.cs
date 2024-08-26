using System;
using System.Deployment.Application;
using System.Drawing;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    partial class AboutBox : Form
    {
        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        // Constants
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 19;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AboutBox"/> class.
        /// Sets up the form with assembly information.
        /// </summary>
        public AboutBox()
        {
            InitializeComponent();
            SetColorScheme();

            this.Text = String.Format("About {0}", AssemblyTitle);
            this.labelProductName.Text = AssemblyProduct;
            this.labelProductName.ForeColor = UITheme.AboutTextColor;
            this.labelVersion.Text = String.Format("Version {0}", GetVersion());
            this.labelVersion.ForeColor = UITheme.AboutTextColor;
            this.labelCopyright.Text = AssemblyCopyright;
            this.labelCopyright.ForeColor = UITheme.AboutTextColor;
            this.labelCompanyName.Text = AssemblyCompany;
            this.labelCompanyName.ForeColor = UITheme.AboutTextColor;
            this.textBoxDescription.Text = AssemblyDescription;
            this.textBoxDescription.ForeColor = UITheme.AboutTextColor;
        }

        #endregion

        #region Assembly Attribute Accessors


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            float increasedSize = this.Font.Size + 2.0f;
            Font newFont = new Font(this.Font.FontFamily, increasedSize, this.Font.Style);

            // Adjust fonts
            this.Font = newFont;

            this.Font = newFont;
            this.labelProductName.Font = newFont;
            this.labelVersion.Font = newFont;
            this.labelCopyright.Font = newFont;
            this.labelCompanyName.Font = newFont;
            this.textBoxDescription.Font = newFont;
            this.okButton.Font = newFont;
        }

        /// <summary>
        /// Gets the title of the assembly.
        /// </summary>
        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        /// <summary>
        /// Gets the version of the assembly.
        /// </summary>
        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        /// <summary>
        /// Gets the description of the assembly.
        /// </summary>
        public string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        /// <summary>
        /// Gets the product name of the assembly.
        /// </summary>
        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        /// <summary>
        /// Gets the copyright information of the assembly.
        /// </summary>
        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        /// <summary>
        /// Gets the company name of the assembly.
        /// </summary>
        public string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the click event of the OK button.
        /// Closes the form.
        /// </summary>
        private void okButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Retrieves the current version of the application, considering network deployment.
        /// </summary>
        /// <returns>The current version as a string.</returns>
        private string GetVersion()
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                return ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            }
            return AssemblyVersion;
        }

        private void SetColorScheme()
        {
            // main form
            this.ForeColor = UITheme.DarkForeground;
            this.BackColor = UITheme.DarkBackground;

            // OK button
            okButton.BackColor = UITheme.ButtonDarkBackground;
            okButton.ForeColor = UITheme.ButtonDarkForeground;
            okButton.FlatStyle = FlatStyle.Popup;


            // Titlebar
            var color = UITheme.DarkBackground;
            int colorValue = color.R | (color.G << 8) | (color.B << 16);
            DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref colorValue, sizeof(int));

            textBoxDescription.ForeColor = UITheme.AboutTextColor;
            textBoxDescription.BackColor = UITheme.DarkBackground;

            logoPictureBox.BackColor = UITheme.AboutPictureBackground;
        }

        #endregion
    }
}