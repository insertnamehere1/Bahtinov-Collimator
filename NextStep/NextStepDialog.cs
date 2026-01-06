using Bahtinov_Collimator.Custom_Components;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static System.Collections.Specialized.BitVector32;

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

        #region Fields

        private readonly Panel _scroll;
        private readonly FlowLayoutPanel _stack;
        private readonly RoundedButton _ok;

        #endregion

        /// <summary>
        /// Initializes and displays the "Next Step" guidance dialog.
        ///
        /// The dialog presents a structured, scrollable layout consisting of:
        /// - An optional header
        /// - A summary paragraph
        /// - One or more guidance sections, each with a boxed title and body content
        /// - An optional footer hint
        ///
        /// Section titles are rendered using a dedicated <see cref="TitleBox"/> control
        /// to provide clear visual separation and improved readability.
        /// </summary>
        /// <param name="guidance">The guidance model describing what the user should do next.</param>
        /// <param name="icon">Optional window icon.</param>
        public NextStepDialog(NextStepGuidance guidance, Icon icon = null)
        {
            Text = guidance?.DialogTitle ?? "What should I do next?";
            Icon = icon;

            // Title bar (keep your existing behaviour)
            var color = UITheme.DarkBackground;
            int colorValue = color.R | (color.G << 8) | (color.B << 16);
            DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref colorValue, sizeof(int));

            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            AutoScaleMode = AutoScaleMode.Dpi;

            ClientSize = new Size(760, 480);

            BackColor = UITheme.DarkBackground;
            ForeColor = Color.White;

            // Scroll container
            _scroll = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(ClientSize.Width - 40, ClientSize.Height - 90),
                AutoScroll = true,
                BackColor = UITheme.DarkBackground
            };

            // Vertical stack inside scroll
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

            // Re-flow widths for DPI / resizing
            ClientSizeChanged += (s, e) => RelayoutForNewSize();
        }

        /// <summary>
        /// Reflows and resizes all child controls when the dialog client size changes.
        ///
        /// This ensures that labels, title boxes, and section bodies remain aligned,
        /// correctly sized, and readable during DPI changes or dynamic resizing.
        /// </summary>
        private void RelayoutForNewSize()
        {
            _scroll.Size = new Size(ClientSize.Width - 40, ClientSize.Height - 90);
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
        /// Calculates the available width for content controls within the scroll panel.
        ///
        /// This accounts for the presence of a vertical scrollbar to prevent
        /// horizontal clipping or unwanted wrapping.
        /// </summary>
        /// <returns>The usable content width in pixels.</returns>
        private int ContentWidth()
        {
            int scrollBar = SystemInformation.VerticalScrollBarWidth;
            return Math.Max(100, _scroll.ClientSize.Width - scrollBar - 2);
        }

        /// <summary>
        /// Clears the current dialog content and rebuilds it from the supplied guidance model.
        ///
        /// This method creates:
        /// - Header and summary labels
        /// - Boxed section titles
        /// - Bullet lists and optional inline images
        /// - Footer hint text
        ///
        /// The layout is rebuilt from scratch to ensure consistent spacing and styling.
        /// </summary>
        /// <param name="g">The guidance model to render.</param>
        private void RenderGuidance(NextStepGuidance g)
        {
            _stack.SuspendLayout();
            _stack.Controls.Clear();

            if (g == null)
            {
                _stack.Controls.Add(MakeLabel("No guidance available.", 11f, FontStyle.Italic));
                _stack.ResumeLayout(true);
                return;
            }

            // Header
            if (!string.IsNullOrWhiteSpace(g.Header))
                _stack.Controls.Add(MakeHeaderBox(g.Header));

            // Summary
            if (!string.IsNullOrWhiteSpace(g.Summary))
                _stack.Controls.Add(MakeLabel(g.Summary, 11f, FontStyle.Regular));

            // Sections
            if (g.Sections != null)
            {
                foreach (var section in g.Sections)
                {
                    if (section == null)
                        continue;

                    if (!string.IsNullOrWhiteSpace(section.Title))
                        _stack.Controls.Add(MakeTitleBox(section));

                    _stack.Controls.Add(MakeSectionBody(section));

                    _stack.Controls.Add(MakeSpacer(10));
                }
            }

            // Footer
            if (!string.IsNullOrWhiteSpace(g.FooterHint))
                _stack.Controls.Add(MakeLabel(g.FooterHint, 10.5f, FontStyle.Italic));

            _stack.ResumeLayout(true);
        }

        /// <summary>
        /// Creates a fixed-height spacer used to visually separate dialog sections.
        /// </summary>
        /// <param name="height">The height of the spacer in pixels.</param>
        /// <returns>A panel acting as vertical spacing.</returns>
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
        ///
        /// This is used for headers, summary text, and footer hints where
        /// RichTextBox features are unnecessary.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="size">Font size in points.</param>
        /// <param name="style">Font style.</param>
        /// <returns>A fully sized label control.</returns>
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
        /// Measures the height required to fully render a label's text using word wrapping.
        /// </summary>
        /// <param name="lbl">The label to measure.</param>
        /// <returns>The required height in pixels.</returns>
        private int MeasureLabelHeight(Label lbl)
        {
            if (lbl == null)
                return 24;

            var sz = TextRenderer.MeasureText(lbl.Text ?? "", lbl.Font,
                new Size(Math.Max(10, lbl.Width), int.MaxValue),
                TextFormatFlags.WordBreak);

            return Math.Max(24, sz.Height + 2);
        }

        /// <summary>
        /// Creates a boxed section title using a <see cref="TitleBox"/> control.
        ///
        /// The control auto-sizes to the width of its text plus padding,
        /// rather than stretching across the dialog.
        /// </summary>
        /// <param name="section">The header section providing styling flags.</param>
        /// <returns>A configured title box control.</returns>
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
                Font = new Font("Segoe UI", 14f, FontStyle.Bold)
            };
            tb.Height = tb.GetPreferredSize(Size.Empty).Height;
            return tb;
        }


        /// <summary>
        /// Creates a boxed section title using a <see cref="TitleBox"/> control.
        ///
        /// The control auto-sizes to the width of its text plus padding,
        /// rather than stretching across the dialog.
        /// </summary>
        /// <param name="section">The guidance section providing title and styling flags.</param>
        /// <returns>A configured title box control.</returns>
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

            // Style tweaks by section type
            if (section.IsSafetyNote)
            {
                tb.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold | FontStyle.Italic);
                tb.FillColor = Color.FromArgb(45, 255, 200, 0);
            }
            else if (section.EmphasizeTitle)
            {
                tb.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
                tb.FillColor = Color.FromArgb(40, 255, 255, 255);
            }
            else if (section.IsSubSection)
            {
                tb.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            }
            else
            {
                tb.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            }

            return tb;
        }


        /// <summary>
        /// Creates the body content for a guidance section.
        ///
        /// This includes:
        /// - Bullet-point instructions
        /// - An optional inline image
        ///
        /// The control automatically sizes itself to fit the rendered content.
        /// </summary>
        /// <param name="section">The guidance section to render.</param>
        /// <returns>A read-only RichTextBox containing the section body.</returns>
        private RichTextBox MakeSectionBody(GuidanceSection section)
        {
            var rtb = new RichTextBox
            {
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.None,
                BackColor = UITheme.DarkBackground,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10.5f),
                Width = ContentWidth(),
                Margin = new Padding(0, 0, 0, 0),
                DetectUrls = false,
                TabStop = false,
                HideSelection = true,
                Cursor = Cursors.Default
            };

            GotFocus += (s, e) => _ok.Focus();

            if (section.Bullets != null)
            {
                foreach (var bullet in section.Bullets)
                {
                    if (!string.IsNullOrWhiteSpace(bullet))
                        rtb.AppendText("• " + bullet + Environment.NewLine);
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
        ///
        /// This method positions the caret at the end of the text and uses the resulting
        /// layout position to determine the required height.
        /// </summary>
        /// <param name="rtb">The RichTextBox to measure.</param>
        /// <returns>The required height in pixels.</returns>
        private int GetRichTextHeight(RichTextBox rtb)
        {
            if (rtb == null)
                return 24;

            if (!rtb.IsHandleCreated)
                rtb.CreateControl();

            int len = rtb.TextLength;
            Point pt = rtb.GetPositionFromCharIndex(len);

            int lineHeight = (int)Math.Ceiling(rtb.Font.GetHeight());
            int h = pt.Y + lineHeight + 6;

            return Math.Max(24, h);
        }
    }
}
