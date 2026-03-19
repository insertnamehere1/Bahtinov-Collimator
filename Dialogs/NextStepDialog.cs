using Bahtinov_Collimator.Custom_Components;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    public sealed partial class NextStepDialog : Form
    {
        #region DLL Imports
        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        #endregion

        #region Constants
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 19;

        // These values were originally tuned at 144 DPI.
        // We normalize them to a 96-DPI baseline, then scale to the current monitor DPI.
        private const int ContentPadding = 26;
        private const int WidthPadding = 90;

        #endregion

        private int ScaleFrom96(int logicalPixelsAt96)
        {
            float dpiScale = DeviceDpi / 96f;
            return (int)Math.Round(logicalPixelsAt96 * dpiScale, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Design-time constructor required by WinForms designer.
        /// </summary>
        public NextStepDialog() : this(null, null)
        {
        }

        /// <summary>
        /// Initializes and displays the "Next Step" guidance dialog.
        /// </summary>
        public NextStepDialog(NextStepGuidance guidance, Icon icon = null)
        {
            InitializeComponent();

            Text = guidance?.DialogTitle ?? "What should I do next?";
            Icon = icon;

            var color = UITheme.DarkBackground;
            int colorValue = color.R | (color.G << 8) | (color.B << 16);
            DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref colorValue, sizeof(int));

            BackColor = UITheme.DarkBackground;
            ForeColor = Color.White;
            AcceptButton = okButton;

            RenderGuidance(guidance);

            ClientSizeChanged += (s, e) => RelayoutForNewSize();
            Load += (s, e) => SizeFormToContent();
        }

        /// <summary>
        /// Updates the dialog content and resizes the form to fit.
        /// </summary>
        public void UpdateGuidance(NextStepGuidance guidance)
        {
            Text = guidance?.DialogTitle ?? "What should I do next?";
            RenderGuidance(guidance);
            BeginInvoke(new Action(SizeFormToContent));
        }

        /// <summary>
        /// Sizes the form to fit the current stackLayout content, clamped to screen height.
        /// </summary>
        private void SizeFormToContent()
        {
            int screenEdgeMargin = ScaleFrom96(53);
            int minClientWidth = ScaleFrom96(227);
            int horizontalInset = ScaleFrom96(27);
            int contentInsetBottom = ScaleFrom96(67);
            int buttonRightInset = ScaleFrom96(87);
            int buttonBottomInset = ScaleFrom96(33);
            int topChromePadding = ScaleFrom96(33);
            int bottomChromePadding = ScaleFrom96(27);
            int contentPadding = ScaleFrom96(ContentPadding);

            // Step 1: measure and apply required width
            int maxScreenWidth = Screen.FromControl(this).WorkingArea.Width - screenEdgeMargin;
            int newClientWidth = Math.Max(minClientWidth, Math.Min(MeasureRequiredWidth(), maxScreenWidth));

            // Step 2: update control widths and remeasure heights with new width
            int contentWidth = newClientWidth - horizontalInset - SystemInformation.VerticalScrollBarWidth - ScaleFrom96(1);
            foreach (Control c in stackLayout.Controls)
            {
                c.Width = contentWidth;
                if (c is Label lbl) lbl.Height = MeasureLabelHeight(lbl);
                else if (c is RichTextBox rtb) rtb.Height = GetRichTextHeight(rtb);
            }

            stackLayout.PerformLayout();

            // Step 3: measure and apply required height
            int contentHeight = stackLayout.Height;
            int required = topChromePadding + contentHeight + bottomChromePadding + contentPadding;
            int maxHeight = Screen.FromControl(this).WorkingArea.Height - screenEdgeMargin;
            int newHeight = Math.Min(required, maxHeight);

            ClientSize = new Size(newClientWidth, newHeight);
            scrollPanel.Size = new Size(newClientWidth - horizontalInset, newHeight - contentInsetBottom);
            okButton.Location = new Point(newClientWidth - buttonRightInset, newHeight - buttonBottomInset);
        }

        /// <summary>
        /// Reflows and resizes all child controls when the dialog client size changes.
        /// </summary>
        private void RelayoutForNewSize()
        {
            int horizontalInset = ScaleFrom96(27);
            int contentInsetBottom = ScaleFrom96(127);
            int buttonRightInset = ScaleFrom96(87);
            int buttonBottomInset = ScaleFrom96(33);

            scrollPanel.Size = new Size(ClientSize.Width - horizontalInset, ClientSize.Height - contentInsetBottom);
            okButton.Location = new Point(ClientSize.Width - buttonRightInset, ClientSize.Height - buttonBottomInset);

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
        /// Calculates the available content width within the scrollPanel panel.
        /// </summary>
        private int ContentWidth()
        {
            int scrollBar = SystemInformation.VerticalScrollBarWidth;
            return Math.Max(ScaleFrom96(67), scrollPanel.ClientSize.Width - scrollBar - ScaleFrom96(1));
        }

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
                    stackLayout.Controls.Add(MakeSpacer(ScaleFrom96(7)));
                }
            }

            if (!string.IsNullOrWhiteSpace(g.FooterHint))
                stackLayout.Controls.Add(MakeLabel(g.FooterHint, UITheme.NextStepFontSize, FontStyle.Italic));

            stackLayout.ResumeLayout(true);
        }

        /// <summary>
        /// Creates a fixed-height spacer to visually separate dialog sections.
        /// </summary>
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
        private Label MakeLabel(string text, float size, FontStyle style)
        {
            var lbl = new Label
            {
                AutoSize = false,
                Text = text ?? "",
                Font = new Font("Segoe UI", size, style),
                ForeColor = Color.White,
                BackColor = UITheme.DarkBackground,
                Width = ContentWidth(),
                Margin = new Padding(
                    ScaleFrom96(3),
                    ScaleFrom96(6),
                    0,
                    ScaleFrom96(4))
            };
            lbl.Height = MeasureLabelHeight(lbl);
            return lbl;
        }

        /// <summary>
        /// Measures the height required to fully render a label's text.
        /// </summary>
        private int MeasureLabelHeight(Label lbl)
        {
            if (lbl == null) return ScaleFrom96(16);
            var sz = TextRenderer.MeasureText(lbl.Text ?? "", lbl.Font,
                new Size(Math.Max(10, lbl.Width), int.MaxValue),
                TextFormatFlags.WordBreak);
            return Math.Max(ScaleFrom96(16), sz.Height + ScaleFrom96(1));
        }

        /// <summary>
        /// Creates a boxed header using a TitleBox control.
        /// </summary>
        private Control MakeHeaderBox(string header)
        {
            var tb = new TitleBox
            {
                Text = header,
                AutoSize = true,
                Width = ContentWidth(),
                Margin = new Padding(0, ScaleFrom96(6), 0, ScaleFrom96(4)),
                BorderColor = UITheme.ButtonDarkForeground,
                FillColor = Color.FromArgb(35, 255, 255, 255),
                CornerRadius = 8,
                ForeColor = Color.White,
                BackColor = UITheme.DarkBackground,
                Font = new Font("Segoe UI", UITheme.NextStepTitleFontSize, FontStyle.Bold)
            };
            tb.Height = tb.GetPreferredSize(Size.Empty).Height;
            return tb;
        }

        /// <summary>
        /// Creates a boxed section title using a TitleBox control.
        /// </summary>
        private Control MakeTitleBox(GuidanceSection section)
        {
            var tb = new TitleBox
            {
                Text = section.Title,
                AutoSize = true,
                Margin = new Padding(0, ScaleFrom96(6), 0, ScaleFrom96(4)),
                BorderColor = UITheme.ButtonDarkForeground,
                FillColor = Color.FromArgb(35, 255, 255, 255),
                CornerRadius = 8,
                ForeColor = Color.White,
                BackColor = UITheme.DarkBackground
            };

            if (section.IsSafetyNote)
            {
                tb.Font = new Font("Segoe UI", UITheme.NextStepFontSize, FontStyle.Bold | FontStyle.Italic);
                tb.FillColor = Color.FromArgb(45, 255, 200, 0);
            }
            else if (section.EmphasizeTitle)
            {
                tb.Font = new Font("Segoe UI", UITheme.NextStepTitleFontSize, FontStyle.Bold);
                tb.FillColor = Color.FromArgb(40, 255, 255, 255);
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
        private RichTextBox MakeSectionBody(GuidanceSection section)
        {
            var rtb = new RichTextBox
            {
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.None,
                BackColor = UITheme.DarkBackground,
                ForeColor = Color.White,
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
        private int GetRichTextHeight(RichTextBox rtb)
        {
            if (rtb == null) return ScaleFrom96(16);
            if (!rtb.IsHandleCreated) rtb.CreateControl();

            int len = rtb.TextLength;
            Point pt = rtb.GetPositionFromCharIndex(len);
            int lineHeight = (int)Math.Ceiling(rtb.Font.GetHeight());
            return Math.Max(ScaleFrom96(16), pt.Y + lineHeight + ScaleFrom96(4));
        }

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
                        w = (int)g.MeasureString(tb.Text, tb.Font).Width + ScaleFrom96(27);
                    }

                    maxWidth = Math.Max(maxWidth, w);
                }
            }

            int minWidth = ScaleFrom96(400);
            int widthPadding = ScaleFrom96(WidthPadding);
            return Math.Max(minWidth, maxWidth + widthPadding);
        }
    }
}
