using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    /// <summary>
    /// Extension helper that shows a dialog so that its first paint uses the
    /// correct per-monitor DPI.
    ///
    /// Background:
    ///   Under .NET Framework WinForms with PerMonitorV2 awareness, a top-level
    ///   form whose HWND is created directly on a non-primary (different-DPI)
    ///   monitor renders its first paint with a stale device context DPI.
    ///   WinForms' AutoScaleMode.Dpi scales control sizes/locations correctly,
    ///   but fonts are not explicitly scaled - they rely on the DC DPI being
    ///   correct at paint time. When the DC is stale at 96 DPI, 10pt fonts
    ///   render at 13 px on a 168-DPI display ("fonts too small").
    ///
    ///   The fix, applied by this helper and the equivalent code in
    ///   <c>Form1</c>, is to create the HWND on the primary monitor first,
    ///   show the window user-invisible via <c>Opacity = 0</c>, then move it
    ///   to the intended target position while it is Windows-visible. That
    ///   real cross-monitor move refreshes the HWND's DC DPI context, fires
    ///   WM_DPICHANGED, and lets WinForms scale both controls and fonts
    ///   correctly for the first visible paint.
    /// </summary>
    internal static class DpiAwareDialog
    {
        [DllImport("user32.dll")]
        private static extern uint GetDpiForWindow(IntPtr hWnd);

        /// <summary>
        /// Shows <paramref name="dialog"/> modally with DPI-correct first
        /// paint. The dialog's effective start position is preserved from
        /// whatever is set in its designer / prior to this call
        /// (<see cref="FormStartPosition.CenterParent"/>,
        /// <see cref="FormStartPosition.CenterScreen"/>, or
        /// <see cref="FormStartPosition.Manual"/> at <c>dialog.Location</c>).
        /// </summary>
        public static DialogResult ShowDialogDpiAware(this Form dialog, IWin32Window owner = null)
        {
            if (dialog == null) throw new ArgumentNullException(nameof(dialog));

            FormStartPosition originalStart = dialog.StartPosition;
            Point originalManualLocation = dialog.Location;
            Size designerSize = dialog.Size;

            Screen primary = Screen.PrimaryScreen;
            Point primaryStart = new Point(
                primary.WorkingArea.Left + 50,
                primary.WorkingArea.Top + 50);

            dialog.StartPosition = FormStartPosition.Manual;
            dialog.Location = primaryStart;
            dialog.Opacity = 0d;

            // Force handle creation NOW on the primary monitor so WinForms'
            // starting DPI state is the designer baseline (96). We will move
            // the (Windows-visible, user-invisible) dialog to the owner's
            // monitor in the Shown handler below, which triggers a real
            // cross-monitor move, refreshes the DC DPI, fires WM_DPICHANGED,
            // and causes WinForms to scale controls and paint fonts at the
            // right size for the first visible frame.
            var _ = dialog.Handle;

            EventHandler shownHandler = null;
            shownHandler = (s, e) =>
            {
                dialog.Shown -= shownHandler;

                Point finalLocation = ComputeFinalLocation(
                    dialog, owner, originalStart, originalManualLocation, designerSize);

                if (finalLocation != dialog.Location)
                    dialog.Location = finalLocation;

                // Re-center after WinForms has scaled the dialog on the
                // owner's monitor - the final scaled size can only be known
                // after the move completes.
                if (originalStart == FormStartPosition.CenterParent ||
                    originalStart == FormStartPosition.CenterScreen)
                {
                    Point recentered = RecenterAfterScale(
                        dialog, owner, originalStart);
                    if (recentered != dialog.Location)
                        dialog.Location = recentered;
                }

                // Defer restoring Opacity until after the rest of OnShown
                // (including any override's post-base code, e.g.
                // DarkMessageBox.OnShown calling AutoSizeControls to finalize
                // its button-panel layout at the destination-monitor DPI) has
                // finished. BeginInvoke posts to the message queue, so the
                // action runs after the current Shown-event/OnShown stack
                // unwinds, avoiding a visible flash of stale layout.
                dialog.BeginInvoke((Action)(() =>
                {
                    if (!dialog.IsDisposed)
                        dialog.Opacity = 1d;
                }));
            };
            dialog.Shown += shownHandler;

            return owner != null ? dialog.ShowDialog(owner) : dialog.ShowDialog();
        }

        private static Point ComputeFinalLocation(
            Form dialog,
            IWin32Window owner,
            FormStartPosition originalStart,
            Point originalManualLocation,
            Size designerSize)
        {
            Form ownerForm = ResolveOwnerForm(owner);

            switch (originalStart)
            {
                case FormStartPosition.Manual:
                    return originalManualLocation;

                case FormStartPosition.CenterParent:
                    if (ownerForm != null && ownerForm.IsHandleCreated)
                    {
                        Size predicted = PredictScaledSize(designerSize, ownerForm.Handle);
                        Rectangle pb = ownerForm.Bounds;
                        return new Point(
                            pb.X + (pb.Width - predicted.Width) / 2,
                            pb.Y + (pb.Height - predicted.Height) / 2);
                    }
                    goto case FormStartPosition.CenterScreen;

                case FormStartPosition.CenterScreen:
                {
                    Screen s = ownerForm != null && ownerForm.IsHandleCreated
                        ? Screen.FromHandle(ownerForm.Handle)
                        : Screen.PrimaryScreen;
                    Size predicted = ownerForm != null && ownerForm.IsHandleCreated
                        ? PredictScaledSize(designerSize, ownerForm.Handle)
                        : designerSize;
                    Rectangle wa = s.WorkingArea;
                    return new Point(
                        wa.X + (wa.Width - predicted.Width) / 2,
                        wa.Y + (wa.Height - predicted.Height) / 2);
                }

                default:
                    Rectangle pw = Screen.PrimaryScreen.WorkingArea;
                    return new Point(pw.X + 50, pw.Y + 50);
            }
        }

        private static Point RecenterAfterScale(Form dialog, IWin32Window owner, FormStartPosition originalStart)
        {
            Form ownerForm = ResolveOwnerForm(owner);
            Size actualSize = dialog.Size;

            if (originalStart == FormStartPosition.CenterParent && ownerForm != null && ownerForm.IsHandleCreated)
            {
                Rectangle pb = ownerForm.Bounds;
                return new Point(
                    pb.X + (pb.Width - actualSize.Width) / 2,
                    pb.Y + (pb.Height - actualSize.Height) / 2);
            }

            Screen s = ownerForm != null && ownerForm.IsHandleCreated
                ? Screen.FromHandle(ownerForm.Handle)
                : Screen.PrimaryScreen;
            Rectangle wa = s.WorkingArea;
            return new Point(
                wa.X + (wa.Width - actualSize.Width) / 2,
                wa.Y + (wa.Height - actualSize.Height) / 2);
        }

        private static Form ResolveOwnerForm(IWin32Window owner)
        {
            Control c = owner as Control;
            if (c != null)
                return c.TopLevelControl as Form;
            return null;
        }

        /// <summary>
        /// Predicts the dialog's size after WinForms rescales it from the
        /// designer 96-DPI baseline to the DPI of the given owner's window.
        /// Used so we can pre-compute a near-correct center position before
        /// the dialog is actually moved and scaled.
        /// </summary>
        private static Size PredictScaledSize(Size designerSize, IntPtr ownerHandle)
        {
            uint dpi;
            try { dpi = GetDpiForWindow(ownerHandle); }
            catch (DllNotFoundException) { return designerSize; }
            catch (EntryPointNotFoundException) { return designerSize; }

            if (dpi == 0) return designerSize;
            float factor = dpi / 96f;
            return new Size(
                (int)Math.Round(designerSize.Width * factor),
                (int)Math.Round(designerSize.Height * factor));
        }
    }
}
