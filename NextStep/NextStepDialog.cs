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

        private const int ContentPadding = 30;
        private const int WidthPadding = 90;

        #endregion

        #region Fields
        private readonly Panel _scroll;
        private readonly FlowLayoutPanel _stack;
        private readonly RoundedButton _ok;
        #endregion

        /// <summary>
        /// Initializes and displays the "Next Step" guidance dialog.
        /// </summary>
        public NextStepDialog(NextStepGuidance guidance, Icon icon = null)
        {
            Text = guidance?.DialogTitle ?? "What should I do next?";
            Icon = icon;

            var color = UITheme.DarkBackground;
            int colorValue = color.R | (color.G << 8) | (color.B << 16);
            DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref colorValue, sizeof(int));

            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(760, 550);
            BackColor = UITheme.DarkBackground;
            ForeColor = Color.White;

            _scroll = new Panel
            {
                Location = new Point(20, 50),
                Size = new Size(ClientSize.Width - 40, ClientSize.Height - 90),
                AutoScroll = true,
                BackColor = UITheme.DarkBackground
            };

            _stack = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = UITheme.DarkBackground,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            _scroll.Controls.Add(_stack);

            _ok = new RoundedButton
            {
                Font = new Font("Segoe UI", UITheme.ButtonFontSize, FontStyle.Bold),
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

            Controls.Add(_scroll);
            Controls.Add(_ok);
            AcceptButton = _ok;

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
        /// Sizes the form to fit the current stack content, clamped to screen height.
        /// </summary>
        private void SizeFormToContent()
        {
            // Step 1: measure and apply required width
            int maxScreenWidth = Screen.FromControl(this).WorkingArea.Width - 80;
            int newClientWidth = Math.Max(400, Math.Min(MeasureRequiredWidth(), maxScreenWidth));

            // Step 2: update control widths and remeasure heights with new width
            int contentWidth = newClientWidth - 40 - SystemInformation.VerticalScrollBarWidth - 2;
            foreach (Control c in _stack.Controls)
            {
                c.Width = contentWidth;
                if (c is Label lbl) lbl.Height = MeasureLabelHeight(lbl);
                else if (c is RichTextBox rtb) rtb.Height = GetRichTextHeight(rtb);
            }

            _stack.PerformLayout();

            // Step 3: measure and apply required height
            int contentHeight = _stack.Height;
            int required = 50 + contentHeight + 40 + ContentPadding;
            int maxHeight = Screen.FromControl(this).WorkingArea.Height - 80;
            int newHeight = Math.Min(required, maxHeight);

            ClientSize = new Size(newClientWidth, newHeight);
            _scroll.Size = new Size(newClientWidth - 40, newHeight - 90);
            _ok.Location = new Point(newClientWidth - 130, newHeight - 50);
        }

        /// <summary>
        /// Reflows and resizes all child controls when the dialog client size changes.
        /// </summary>
        private void RelayoutForNewSize()
        {
            _scroll.Size = new Size(ClientSize.Width - 40, ClientSize.Height - 120);
            _ok.Location = new Point(ClientSize.Width - 130, ClientSize.Height - 50);

            int w = ContentWidth();
            foreach (Control c in _stack.Controls)
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
        /// Calculates the available content width within the scroll panel.
        /// </summary>
        private int ContentWidth()
        {
            int scrollBar = SystemInformation.VerticalScrollBarWidth;
            return Math.Max(100, _scroll.ClientSize.Width - scrollBar - 2);
        }

        /// <summary>
        /// Clears and rebuilds the dialog content from the supplied guidance model.
        /// </summary>
        private void RenderGuidance(NextStepGuidance g)
        {
            _stack.SuspendLayout();
            _stack.Controls.Clear();

            if (g == null)
            {
                _stack.Controls.Add(MakeLabel("No guidance available.", UITheme.NextStepFontSize, FontStyle.Italic));
                _stack.ResumeLayout(true);
                return;
            }

            if (!string.IsNullOrWhiteSpace(g.Header))
                _stack.Controls.Add(MakeHeaderBox(g.Header));

            if (!string.IsNullOrWhiteSpace(g.Summary))
                _stack.Controls.Add(MakeLabel(g.Summary, UITheme.NextStepFontSize, FontStyle.Regular));

            if (g.Sections != null)
            {
                foreach (var section in g.Sections)
                {
                    if (section == null) continue;

                    if (!string.IsNullOrWhiteSpace(section.Title))
                        _stack.Controls.Add(MakeTitleBox(section));

                    _stack.Controls.Add(MakeSectionBody(section));
                    _stack.Controls.Add(MakeSpacer(10));
                }
            }

            if (!string.IsNullOrWhiteSpace(g.FooterHint))
                _stack.Controls.Add(MakeLabel(g.FooterHint, UITheme.NextStepFontSize, FontStyle.Italic));

            _stack.ResumeLayout(true);
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
                Margin = new Padding(4, 8, 0, 6)
            };
            lbl.Height = MeasureLabelHeight(lbl);
            return lbl;
        }

        /// <summary>
        /// Measures the height required to fully render a label's text.
        /// </summary>
        private int MeasureLabelHeight(Label lbl)
        {
            if (lbl == null) return 24;
            var sz = TextRenderer.MeasureText(lbl.Text ?? "", lbl.Font,
                new Size(Math.Max(10, lbl.Width), int.MaxValue),
                TextFormatFlags.WordBreak);
            return Math.Max(24, sz.Height + 2);
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
                Margin = new Padding(0, 8, 0, 6),
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
                Margin = new Padding(0, 8, 0, 6),
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

            GotFocus += (s, e) => _ok.Focus();

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
            if (rtb == null) return 24;
            if (!rtb.IsHandleCreated) rtb.CreateControl();

            int len = rtb.TextLength;
            Point pt = rtb.GetPositionFromCharIndex(len);
            int lineHeight = (int)Math.Ceiling(rtb.Font.GetHeight());
            return Math.Max(24, pt.Y + lineHeight + 6);
        }

        private int MeasureRequiredWidth()
        {
            int maxWidth = 0;

            using (Graphics g = CreateGraphics())
            {
                foreach (Control c in _stack.Controls)
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
                        w = (int)g.MeasureString(tb.Text, tb.Font).Width + 40;
                    }

                    maxWidth = Math.Max(maxWidth, w);
                }
            }

            return Math.Max(600, maxWidth + WidthPadding);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // NextStepDialog
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(300, 300);
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "NextStepDialog";
            this.ResumeLayout(false);

        }
    }
}
