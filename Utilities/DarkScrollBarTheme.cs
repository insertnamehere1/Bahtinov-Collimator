using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    /// <summary>
    /// Applies Windows dark scrollbar chrome to standard text controls (Windows 10 2004+).
    /// </summary>
    public static class DarkScrollBarTheme
    {
        private const string DarkExplorerTheme = "DarkMode_Explorer";

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hwnd, string pszSubAppName, string pszSubIdList);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool EnumChildWindows(IntPtr hWndParent, EnumChildProc lpEnumFunc, IntPtr lParam);

        private delegate bool EnumChildProc(IntPtr hWnd, IntPtr lParam);

        private static readonly EnumChildProc ScrollBarThemeEnumCallback = ThemeScrollBarChildrenCallback;

        private static bool ThemeScrollBarChildrenCallback(IntPtr hWnd, IntPtr lParam)
        {
            var sb = new StringBuilder(256);
            if (GetClassName(hWnd, sb, sb.Capacity) > 0 && sb.ToString() == "ScrollBar")
                SetWindowTheme(hWnd, DarkExplorerTheme, null);
            return true;
        }

        /// <summary>
        /// Themes the control and any child scrollbar HWNDs. Safe to call repeatedly; no-op when unsupported.
        /// </summary>
        public static void Apply(Control control)
        {
            if (control == null || !control.IsHandleCreated)
                return;

            try
            {
                SetWindowTheme(control.Handle, DarkExplorerTheme, null);
                EnumChildWindows(control.Handle, ScrollBarThemeEnumCallback, IntPtr.Zero);
            }
            catch
            {
                // Best-effort; OS may not support DarkMode_Explorer.
            }
        }
    }
}
