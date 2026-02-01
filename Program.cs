using System;
using System.IO;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            LanguageLoader.LoadFromSystemCulture(AppDomain.CurrentDomain.BaseDirectory);
            Application.Run(new Form1());
        }
    }
}
