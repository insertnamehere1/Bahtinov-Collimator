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
        #region P/Invoke Declarations

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        // Constants
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 19;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AboutBox"/> class.
        /// Sets up the form with assembly information and applies the color scheme.
        /// </summary>
        public AboutBox()
        {
            InitializeComponent();
            SetColorScheme();

            this.Text = string.Format(UiText.Current.AboutDialogTitleFormat, AssemblyTitle);
            this.labelProductName.Text = AssemblyProduct;
            this.labelProductName.ForeColor = UITheme.AboutTextColor;
            this.labelVersion.Text = string.Format(UiText.Current.AboutVersionFormat, GetVersion());
            this.labelVersion.ForeColor = UITheme.AboutTextColor;
            this.labelCopyright.Text = AssemblyCopyright;
            this.labelCopyright.ForeColor = UITheme.AboutTextColor;
            this.labelCompanyName.Text = AssemblyCompany;
            this.labelCompanyName.ForeColor = UITheme.AboutTextColor;
            this.textBoxDescription.Text = UiText.Current.AboutDescription;
            this.textBoxDescription.ForeColor = UITheme.AboutTextColor;
            okButton.Text = UiText.Current.CommonOk;
        }

        #endregion

        #region Assembly Attribute Accessors

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
                    if (!string.IsNullOrEmpty(titleAttribute.Title))
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
                    return string.Empty;
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
                    return string.Empty;
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
                    return string.Empty;
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
                    return string.Empty;
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the click event of the OK button.
        /// Closes the form when the OK button is clicked.
        /// </summary>
        private void OkButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

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
            this.labelProductName.Font = new Font(this.Font.FontFamily, this.Font.Size + 3.0f, this.Font.Style);
            this.labelVersion.Font = newFont;
            this.labelCopyright.Font = newFont;
            this.labelCompanyName.Font = newFont;
            this.textBoxDescription.Font = newFont;
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

        /// <summary>
        /// Applies the color scheme to the form and its controls.
        /// </summary>
        private void SetColorScheme()
        {
            // Set colors for main form
            this.ForeColor = UITheme.DarkForeground;
            this.BackColor = UITheme.DarkBackground;

            // Set colors for OK button
            okButton.BackColor = UITheme.ButtonDarkBackground;
            okButton.ForeColor = UITheme.ButtonDarkForeground;
            okButton.FlatStyle = FlatStyle.Popup;
            okButton.CornerRadius = 4;
            okButton.TextOffsetX = 0;
            okButton.BevelDark = Color.FromArgb(180, 90, 90, 90);
            okButton.BevelLight = Color.FromArgb(220, 160, 160, 160);

            // Set colors for titlebar
            var color = UITheme.DarkBackground;
            int colorValue = color.R | (color.G << 8) | (color.B << 16);
            DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref colorValue, sizeof(int));

            // Set colors for other controls
            textBoxDescription.ForeColor = UITheme.AboutTextColor;
            textBoxDescription.BackColor = UITheme.DarkBackground;
            logoPictureBox.BackColor = UITheme.AboutPictureBackground;
        }

        #endregion
    }
}
