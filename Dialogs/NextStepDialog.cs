using Bahtinov_Collimator.Custom_Components;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    /// <summary>
    /// Dialog that renders dynamic guidance content with DPI-aware sizing.
    /// </summary>
    public sealed partial class NextStepDialog : Form
    {
        #region DLL Imports
        /// <summary>
        /// Sets a Desktop Window Manager attribute on the dialog window.
        /// </summary>
        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        #endregion

        #region Constants
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 19;

        private const int ContentPadding = 26;
        private const int WidthPadding = 90;

        #endregion

        #region Layout Fields

        private int panelLeftInset;
        private int panelTopInset;
        private int panelRightInset;
        private int panelBottomInset;
        private int buttonBottomGap;

        #endregion

        #region DPI Helpers

        /// <summary>
        /// Scales 96-DPI logical pixels to the current device DPI.
        /// </summary>
        /// <returns>The scaled device-pixel value.</returns>
        private int S(int logicalPixelsAt96)
        {
            float dpiScale;
            using (Graphics g = CreateGraphics())
            {
                dpiScale = g.DpiX / 96f;
            }
            return (int)Math.Round(logicalPixelsAt96 * dpiScale, MidpointRounding.AwayFromZero);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Design-time constructor required by WinForms designer.
        /// </summary>
        public NextStepDialog() : this(null, null)
        {
        }

        /// <summary>
        /// Initializes and displays the "Next Step" guidance dialog.
        /// </summary>
        /// <param name="guidance">Guidance model used to populate dialog content.</param>
        /// <param name="icon">Optional dialog icon.</param>
        public NextStepDialog(NextStepGuidance guidance, Icon icon = null)
        {
            InitializeComponent();

            Text = guidance?.DialogTitle ?? "What should I do next?";
            Icon = icon;

            var color = UITheme.DarkBackground;
            int colorValue = color.R | (color.G << 8) | (color.B << 16);
            DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref colorValue, sizeof(int));

            BackColor = UITheme.DarkBackground;
            ForeColor = UITheme.White;
            AcceptButton = okButton;

            panelLeftInset = scrollPanel.Left;
            panelTopInset = scrollPanel.Top;
            panelRightInset = ClientSize.Width - scrollPanel.Right;
            panelBottomInset = ClientSize.Height - scrollPanel.Bottom;
            buttonBottomGap = ClientSize.Height - okButton.Bottom;

            RenderGuidance(guidance);

            ClientSizeChanged += (s, e) => RelayoutForNewSize();
            Load += (s, e) => SizeFormToContent();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Updates the dialog content and resizes the form to fit.
        /// </summary>
        /// <param name="guidance">Updated guidance model to render.</param>
        public void UpdateGuidance(NextStepGuidance guidance)
        {
            Text = guidance?.DialogTitle ?? "What should I do next?";
            RenderGuidance(guidance);
            BeginInvoke(new Action(SizeFormToContent));
        }

        #endregion

        #region Layout and Sizing

        /// <summary>
        /// Sizes the form to fit the current stackLayout content, clamped to screen height.
        /// </summary>
        private void SizeFormToContent()
        {
            int screenEdgeMargin = S(53);
            int minClientWidth = S(227);
            int contentPadding = S(ContentPadding);

            int maxScreenWidth = Screen.FromControl(this).WorkingArea.Width - screenEdgeMargin;
            int chromeWidth = panelLeftInset + panelRightInset + SystemInformation.VerticalScrollBarWidth + S(1);
            int newClientWidth = Math.Max(minClientWidth, Math.Min(MeasureRequiredWidth() + chromeWidth, maxScreenWidth));

            int panelWidth = Math.Max(S(67), newClientWidth - panelLeftInset - panelRightInset);
            int contentWidth = Math.Max(S(67), panelWidth - SystemInformation.VerticalScrollBarWidth - S(1));
            foreach (Control c in stackLayout.Controls)
            {
                c.Width = contentWidth;
                if (c is Label lbl) lbl.Height = MeasureLabelHeight(lbl);
                else if (c is RichTextBox rtb) rtb.Height = GetRichTextHeight(rtb);
            }

            stackLayout.PerformLayout();

            int contentHeight = stackLayout.Height;
            int requiredPanelHeight = contentHeight + contentPadding;
            int required = panelTopInset + requiredPanelHeight + panelBottomInset;
            int maxHeight = Screen.FromControl(this).WorkingArea.Height - screenEdgeMargin;
            int newHeight = Math.Min(required, maxHeight);

            ClientSize = new Size(newClientWidth, newHeight);
            RelayoutForNewSize();
        }

        /// <summary>
        /// Reflows and resizes all child controls when the dialog client size changes.
        /// </summary>
        private void RelayoutForNewSize()
        {
            int panelWidth = Math.Max(S(67), ClientSize.Width - panelLeftInset - panelRightInset);
            int panelHeight = Math.Max(S(67), ClientSize.Height - panelTopInset - panelBottomInset);
            scrollPanel.Bounds = new Rectangle(panelLeftInset, panelTopInset, panelWidth, panelHeight);

            int w = ContentWidth();
            foreach (Control c in stackLayout.Controls)
            {
                if (c is Label lbl)
                {
                    lbl.Width = w;
                    lbl.Height = MeasureLabelHeight(lbl);
                }
                else if (c is RichTextBox rtb)
                {
                    rtb.Width = w;
                    rtb.Height = GetRichTextHeight(rtb);
                }
                else if (c is Panel p)
                {
                    p.Width = w;
                }
            }
        }

        /// <summary>
        /// Responds to DPI changes by recalculating dialog sizing and layout.
        /// </summary>
        protected override void OnDpiChanged(DpiChangedEventArgs e)
        {
            base.OnDpiChanged(e);

            BeginInvoke(new Action(() =>
            {
                if (IsDisposed) return;
                SizeFormToContent();
                RelayoutForNewSize();
            }));
        }

        /// <summary>
        /// Calculates the available content width within the scrollPanel panel.
        /// </summary>
        /// <returns>Available content width in pixels.</returns>
        private int ContentWidth()
        {
            int scrollBar = SystemInformation.VerticalScrollBarWidth;
            return Math.Max(S(67), scrollPanel.ClientSize.Width - scrollBar - S(1));
        }

        #endregion

        #region Content Rendering

        /// <summary>
        /// Clears and rebuilds the dialog content from the supplied guidance model.
        /// </summary>
        private void RenderGuidance(NextStepGuidance g)
        {
            stackLayout.SuspendLayout();
            stackLayout.Controls.Clear();

            if (g == null)
            {
                stackLayout.Controls.Add(MakeLabel("No guidance available.", UITheme.NextStepFontSize, FontStyle.Italic));
                stackLayout.ResumeLayout(true);
                return;
            }

            if (!string.IsNullOrWhiteSpace(g.Header))
                stackLayout.Controls.Add(MakeHeaderBox(g.Header));

            if (!string.IsNullOrWhiteSpace(g.Summary))
                stackLayout.Controls.Add(MakeLabel(g.Summary, UITheme.NextStepFontSize, FontStyle.Regular));

            if (g.Sections != null)
            {
                foreach (var section in g.Sections)
                {
                    if (section == null) continue;

                    if (!string.IsNullOrWhiteSpace(section.Title))
                        stackLayout.Controls.Add(MakeTitleBox(section));

                    stackLayout.Controls.Add(MakeSectionBody(section));
                    stackLayout.Controls.Add(MakeSpacer(S(7)));
                }
            }

            if (!string.IsNullOrWhiteSpace(g.FooterHint))
                stackLayout.Controls.Add(MakeLabel(g.FooterHint, UITheme.NextStepFontSize, FontStyle.Italic));

            stackLayout.ResumeLayout(true);
        }

        /// <summary>
        /// Creates a fixed-height spacer to visually separate dialog sections.
        /// </summary>
        /// <returns>A panel that provides vertical spacing.</returns>
        private Control MakeSpacer(int height)
        {
            return new Panel
            {
                Height = height,
                Width = ContentWidth(),
                BackColor = UITheme.DarkBackground,
                Margin = Padding.Empty
            };
        }

        /// <summary>
        /// Creates a word-wrapped label sized to fully display its text.
        /// </summary>
        /// <returns>A configured label with measured height.</returns>
        private Label MakeLabel(string text, float size, FontStyle style)
        {
            var lbl = new Label
            {
                AutoSize = false,
                Text = text ?? "",
                Font = new Font("Segoe UI", size, style),
                ForeColor = UITheme.White,
                BackColor = UITheme.DarkBackground,
                Width = ContentWidth(),
                Margin = new Padding(
                    S(3),
                    S(6),
                    0,
                    S(4))
            };
            lbl.Height = MeasureLabelHeight(lbl);
            return lbl;
        }

        /// <summary>
        /// Measures the height required to fully render a label's text.
        /// </summary>
        /// <returns>Required pixel height.</returns>
        private int MeasureLabelHeight(Label lbl)
        {
            if (lbl == null) return S(16);
            var sz = TextRenderer.MeasureText(lbl.Text ?? "", lbl.Font,
                new Size(Math.Max(10, lbl.Width), int.MaxValue),
                TextFormatFlags.WordBreak);
            return Math.Max(S(16), sz.Height + S(1));
        }

        /// <summary>
        /// Creates a boxed header using a TitleBox control.
        /// </summary>
        /// <returns>A title-box control for the dialog header.</returns>
        private Control MakeHeaderBox(string header)
        {
            var tb = new TitleBox
            {
                Text = header,
                AutoSize = true,
                Width = ContentWidth(),
                Margin = new Padding(0, S(6), 0, S(4)),
                BorderColor = UITheme.ButtonDarkForeground,
                FillColor = UITheme.TitleBoxGlassFill,
                CornerRadius = 8,
                ForeColor = UITheme.White,
                BackColor = UITheme.DarkBackground,
                Font = new Font("Segoe UI", UITheme.NextStepTitleFontSize, FontStyle.Bold)
            };
            tb.Height = tb.GetPreferredSize(Size.Empty).Height;
            return tb;
        }

        /// <summary>
        /// Creates a boxed section title using a TitleBox control.
        /// </summary>
        /// <returns>A title-box control for a section heading.</returns>
        private Control MakeTitleBox(GuidanceSection section)
        {
            var tb = new TitleBox
            {
                Text = section.Title,
                AutoSize = true,
                Margin = new Padding(0, S(6), 0, S(4)),
                BorderColor = UITheme.ButtonDarkForeground,
                FillColor = UITheme.TitleBoxGlassFill,
                CornerRadius = 8,
                ForeColor = UITheme.White,
                BackColor = UITheme.DarkBackground
            };

            if (section.IsSafetyNote)
            {
                tb.Font = new Font("Segoe UI", UITheme.NextStepFontSize, FontStyle.Bold | FontStyle.Italic);
                tb.FillColor = UITheme.NextStepTitleHighlight;
            }
            else if (section.EmphasizeTitle)
            {
                tb.Font = new Font("Segoe UI", UITheme.NextStepTitleFontSize, FontStyle.Bold);
                tb.FillColor = UITheme.NextStepTitleFillNeutral;
            }
            else
            {
                tb.Font = new Font("Segoe UI", UITheme.NextStepFontSize, FontStyle.Bold);
            }

            return tb;
        }

        /// <summary>
        /// Creates the body content for a guidance section.
        /// </summary>
        /// <returns>A read-only rich text box for section body text.</returns>
        private RichTextBox MakeSectionBody(GuidanceSection section)
        {
            var rtb = new RichTextBox
            {
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.None,
                BackColor = UITheme.DarkBackground,
                ForeColor = UITheme.White,
                Font = new Font("Segoe UI", UITheme.NextStepFontSize),
                Width = ContentWidth(),
                Margin = Padding.Empty,
                DetectUrls = false,
                TabStop = false,
                HideSelection = true,
                Cursor = Cursors.Default
            };

            GotFocus += (s, e) => okButton.Focus();

            if (section.Lines != null)
            {
                foreach (var line in section.Lines)
                {
                    if (!string.IsNullOrWhiteSpace(line.Text))
                        rtb.AppendText((line.Bullet ? "• " : "") + line.Text + Environment.NewLine);
                }
            }

            if (section.InlineImage != null)
            {
                if (rtb.TextLength > 0)
                    rtb.AppendText(Environment.NewLine);
                rtb.AppendImageInline(section.InlineImage);
                rtb.AppendText(Environment.NewLine);
            }

            rtb.Height = GetRichTextHeight(rtb);
            return rtb;
        }

        /// <summary>
        /// Calculates the rendered height of a RichTextBox based on its current contents.
        /// </summary>
        /// <returns>Required pixel height for full content visibility.</returns>
        private int GetRichTextHeight(RichTextBox rtb)
        {
            if (rtb == null) return S(16);
            if (!rtb.IsHandleCreated) rtb.CreateControl();

            int len = rtb.TextLength;
            Point pt = rtb.GetPositionFromCharIndex(len);
            int lineHeight = (int)Math.Ceiling(rtb.Font.GetHeight());
            return Math.Max(S(16), pt.Y + lineHeight + S(4));
        }

        /// <summary>
        /// Measures the minimum content width needed to display all currently rendered controls.
        /// </summary>
        /// <returns>Recommended content width in pixels.</returns>
        private int MeasureRequiredWidth()
        {
            int maxWidth = 0;

            using (Graphics g = CreateGraphics())
            {
                foreach (Control c in stackLayout.Controls)
                {
                    int w = 0;

                    if (c is Label lbl)
                    {
                        w = (int)g.MeasureString(lbl.Text, lbl.Font).Width + lbl.Margin.Horizontal;
                    }
                    else if (c is RichTextBox rtb)
                    {
                        foreach (string line in rtb.Lines)
                        {
                            if (string.IsNullOrEmpty(line)) continue;
                            w = Math.Max(w, (int)g.MeasureString(line, rtb.Font).Width);
                        }
                    }
                    else if (c is TitleBox tb)
                    {
                        w = (int)g.MeasureString(tb.Text, tb.Font).Width + S(27);
                    }

                    maxWidth = Math.Max(maxWidth, w);
                }
            }

            int minWidth = S(400);
            int widthPadding = S(WidthPadding);
            return Math.Max(minWidth, maxWidth + widthPadding);
        }

        #endregion
    }
}
