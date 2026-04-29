using System;
using System.IO;
using System.Reflection;
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

        #region Manifest presence check

        /// <summary>
        /// Verifies that the external application manifest (required for the
        /// PerMonitorV2 DPI settings) is present alongside the executable.
        /// Shows a localized error message and aborts startup if it is missing.
        /// Must be called after <see cref="LanguageLoader.LoadFromSystemCulture"/>
        /// so that <see cref="UiText.Current"/> is populated.
        /// </summary>
        /// <returns>True when the manifest exists; false when startup should abort.</returns>
        private static bool VerifyManifestPresent()
        {
            string exePath = Assembly.GetEntryAssembly()?.Location;
            if (string.IsNullOrEmpty(exePath))
                exePath = Application.ExecutablePath;

            string manifestPath = exePath + ".manifest";

            if (File.Exists(manifestPath))
                return true;

            MessageBox.Show(
                UiText.Current.ManifestFileMissingMessage,
                UiText.Current.ManifestFileMissingTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
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

            // Migrate user-scoped settings forward exactly once after an update.
            if (Properties.Settings.Default.UpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.Save();
            }

            // Language must load before the manifest check so the missing-manifest
            // dialog can be shown in the user's language via UiText.Current.
            string telescopeModel = Properties.Settings.Default.MCTSelected ? "MCT" : "SCT";
            bool usedEnglishFallback = LanguageLoader.LoadFromPreference(
                AppDomain.CurrentDomain.BaseDirectory,
                telescopeModel,
                Properties.Settings.Default.LanguagePreference,
                out string resolvedLanguageCode);

            if (usedEnglishFallback)
            {
                string invalidPreference = Properties.Settings.Default.LanguagePreference;
                Properties.Settings.Default.LanguagePreference = "auto";
                Properties.Settings.Default.Save();

                DarkMessageBox.Show(
                    string.Format(UiText.Current.LanguageUnavailableMessageFormat, invalidPreference, resolvedLanguageCode),
                    UiText.Current.LanguageUnavailableTitle,
                    MessageBoxIcon.Warning,
                    MessageBoxButtons.OK);
            }

            if (!VerifyManifestPresent())
                return;

            Application.Run(new Form1());
        }
    }
}
