using Bahtinov_Collimator.Custom_Components;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    public sealed class NextStepDialog : Form
    {
        #region DLL Imports

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        #endregion

        #region Constants

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 19;

        #endregion

        private readonly RichTextBox _rtb;
        private readonly RoundedButton _ok;

        public NextStepDialog(NextStepGuidance guidance, Icon icon = null)
        {
            Text = guidance.DialogTitle ?? "What should I do next?";
            Icon = icon;

            // Title bar
            var color = UITheme.DarkBackground;
            int colorValue = color.R | (color.G << 8) | (color.B << 16);
            DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref colorValue, sizeof(int));

            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            AutoScaleMode = AutoScaleMode.Dpi;

            ClientSize = new Size(760, 480);

            _rtb = new RichTextBox
            {
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                Font = new Font("Segoe UI", 10.5f),
                Location = new Point(20, 20),
                Size = new Size(ClientSize.Width - 40, ClientSize.Height - 90),
                BackColor = UITheme.DarkBackground,
                ForeColor = Color.White
            };

            RenderGuidance(guidance);

            _ok = new RoundedButton
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Size = new Size(110, 34),
                Location = new Point(ClientSize.Width - 130, ClientSize.Height - 50),
                CornerRadius = 6,
                TextOffsetX = 0,
                BackColor = UITheme.ButtonDarkBackground,
                ForeColor = UITheme.ButtonDarkForeground,
                FlatStyle = FlatStyle.Popup
            };

            Controls.AddRange(new Control[] { _rtb, _ok });

            BackColor = UITheme.DarkBackground;
            ForeColor = Color.White;

            AcceptButton = _ok;
        }

        private void AppendStyledLine(string text, Font font)
        {
            if (string.IsNullOrEmpty(text))
                return;

            _rtb.SelectionStart = _rtb.TextLength;
            _rtb.SelectionLength = 0;

            _rtb.SelectionFont = font;
            _rtb.AppendText(text + Environment.NewLine);

            // reset selection to base
            _rtb.SelectionFont = _rtb.Font;
        }

        private void RenderGuidance(NextStepGuidance g)
        {
            _rtb.Clear();

            // TOP-LEVEL HEADER
            AppendStyledLine(g.Header, new Font(_rtb.Font.FontFamily, 14f, FontStyle.Bold));
            _rtb.AppendText(Environment.NewLine);

            // SUMMARY
            AppendStyledLine(g.Summary, new Font(_rtb.Font.FontFamily, 11f));
            _rtb.AppendText(Environment.NewLine);

            // SECTIONS
            foreach (var section in g.Sections)
            {
                _rtb.AppendText(Environment.NewLine);

                if (!string.IsNullOrWhiteSpace(section.Title))
                    AppendSectionTitle(section);

                foreach (var bullet in section.Bullets)
                    _rtb.AppendText("• " + bullet + Environment.NewLine);

                if (section.InlineImage != null)
                    _rtb.AppendImageInline(section.InlineImage);
            }

            // FOOTER
            if (!string.IsNullOrWhiteSpace(g.FooterHint))
            {
                _rtb.AppendText(Environment.NewLine);
                AppendStyledLine(g.FooterHint, new Font(_rtb.Font, FontStyle.Italic));
            }

            _rtb.SelectionStart = 0;
            _rtb.SelectionLength = 0;
            _rtb.ScrollToCaret();
        }

        private void AppendSectionTitle(GuidanceSection section)
        {
            if (section == null || string.IsNullOrWhiteSpace(section.Title))
                return;

            Font baseFont = _rtb.Font;

            FontStyle style = FontStyle.Regular;
            float size = baseFont.Size;

            if (section.EmphasizeTitle)
            {
                style = FontStyle.Bold;
                size = baseFont.Size + 1.5f;
            }
            else if (section.IsSubSection)
            {
                style = FontStyle.Bold;
                size = baseFont.Size;
            }
            else if (section.IsSafetyNote)
            {
                style = FontStyle.Bold | FontStyle.Italic;
                size = baseFont.Size;
            }

            using (var f = new Font(baseFont.FontFamily, size, style))
            {
                AppendStyledLine(section.Title, f);
            }
        }
    }
}

