using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    internal static class Program
    {
        #region DPI awareness P/Invoke

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetProcessDpiAwarenessContext(IntPtr value);

        // DPI_AWARENESS_CONTEXT sentinel handles (Windows 10 1703+).
        private static readonly IntPtr DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = new IntPtr(-4);

        /// <summary>
        /// Forces the process to PerMonitorV2 DPI awareness, overriding whatever
        /// the application manifest provided. Some Windows builds apply the
        /// older <c>&lt;dpiAware&gt;true/pm&lt;/dpiAware&gt;</c> manifest entry
        /// first and do not upgrade to V2 from <c>&lt;dpiAwareness&gt;</c>, which
        /// leaves the top-level form stuck at 96 DPI when launched on a
        /// non-primary monitor. Calling this API before any window is created
        /// fixes that.
        /// </summary>
        private static void ForcePerMonitorV2DpiAwareness()
        {
            try
            {
                SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
                // Return value is intentionally ignored: it returns false if the
                // awareness was already set (e.g. by the manifest), which is fine.
            }
            catch (DllNotFoundException)
            {
                // Pre-Windows 10 1703 - nothing we can do, fall back to manifest.
            }
            catch (EntryPointNotFoundException)
            {
                // Same as above.
            }
        }

        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Must be the very first thing: SetProcessDpiAwarenessContext only
            // works if no top-level window has been created yet.
            ForcePerMonitorV2DpiAwareness();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            LanguageLoader.LoadFromSystemCulture(AppDomain.CurrentDomain.BaseDirectory, Properties.Settings.Default.MCTSelected ? "MCT" : "SCT");
            Application.Run(new Form1());
        }
    }
}
