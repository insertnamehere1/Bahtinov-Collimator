using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    /// <summary>
    /// Provides theme-related properties and methods for the user interface.
    /// </summary>
    public static class UITheme
    {
        #region Forms Theme

        /// <summary>
        /// Gets the background color for dark-themed forms.
        /// </summary>
        public static Color DarkBackground { get; } = Color.FromArgb(13, 17, 23);

        /// <summary>
        /// Gets the color used for the dark background of the title area.
        /// </summary>
        public static Color TitleDarkBackground { get; } = Color.FromArgb(17, 19, 24);

        /// <summary>
        /// Gets the color used to indicate a mouse-over state for title bar buttons.
        /// </summary>
        public static Color TitleButtonsMouseOver { get; } = Color.FromArgb(37, 39, 44);

        /// <summary>
        /// Gets the foreground color for dark-themed forms.
        /// </summary>
        public static Color DarkForeground { get; } = Color.FromArgb(70, 80, 90);

        public static float DeviceDpi { get; set; } = 96f;

        public static float DpiScaleFactor => DeviceDpi / 96f;
        #endregion

        #region Menu Bar Theme
        /// <summary>
        /// Gets the background color for dark-themed menu strips.
        /// 
        public static Color MenuStripDarkBackground { get; } = DarkBackground;

        /// <summary>
        ///  Gets the border color color for dark-themed menu strips.
        /// </summary>
        public static Color MenuBorderColor = Color.FromArgb(100, 100, 100);

        /// <summary>
        /// Gets the background color for dark-themed drop-down menus.
        /// </summary>
        public static Color MenuDarkBackground { get; } = DarkBackground;

        /// <summary>
        /// Gets the foreground color for dark-themed drop-down menus.
        /// </summary>
        public static Color MenuDarkForeground { get; } = Color.LightGray;

        #endregion

        #region Primitive (for consumers; only UITheme may use System.Drawing.Color literals)

        public static Color Transparent { get; } = Color.Transparent;
        public static Color White { get; } = Color.LightGray;
        public static Color Black { get; } = Color.Black;

        #endregion

        #region Rounded button (bevel / overlays)

        public static Color RoundedButtonBevelDark { get; } = Color.FromArgb(180, 90, 90, 90);
        public static Color RoundedButtonBevelLight { get; } = Color.FromArgb(220, 160, 160, 160);
        public static Color RoundedButtonHoverOverlay { get; } = Color.FromArgb(40, Color.LightGray);
        public static Color RoundedButtonPressedOverlay { get; } = Color.FromArgb(60, 100, 100, 100);
        public static Color RoundedButtonPressedOverlayOnBlack { get; } = Color.FromArgb(60, 0, 0, 0);
        public static Color RoundedButtonDesignerBackGray { get; } = Color.Gray;

        #endregion

        #region Toggle switch rendering

        public static Color ToggleSwitchOffBase { get; } = Color.FromArgb(200, 198, 194);
        public static Color ToggleSwitchOffBaseHover { get; } = Color.FromArgb(210, 208, 204);
        public static Color ToggleSwitchBorder { get; } = Color.FromArgb(120, 120, 120);
        public static Color ToggleSwitchBorderHover { get; } = Color.FromArgb(100, 100, 100);
        public static Color ToggleSwitchOnRed { get; } = Color.FromArgb(180, 45, 45);
        public static Color ToggleSwitchOnRedHover { get; } = Color.FromArgb(195, 55, 55);
        public static Color ToggleSwitchDisabledOffBase { get; } = Color.FromArgb(215, 215, 215);
        public static Color ToggleSwitchDisabledOn { get; } = Color.FromArgb(190, 190, 190);
        public static Color ToggleSwitchDisabledBorder { get; } = Color.FromArgb(180, 180, 180);
        public static Color ToggleSwitchFocusRing { get; } = Color.FromArgb(120, 90, 140, 255);
        public static Color ToggleSwitchThumbOutline { get; } = Color.FromArgb(120, 120, 120);
        public static Color ToggleSwitchDesignerBack { get; } = Color.FromArgb(100, 100, 100);

        #endregion

        #region Mirror line art (SCT primary)

        public static Color MirrorSctGlassDarkBase { get; } = Color.FromArgb(70, 190, 190, 190);
        public static Color MirrorSctGlassLightBase { get; } = Color.FromArgb(70, 235, 235, 235);
        public static Color MirrorSctMetalDarkBase { get; } = Color.FromArgb(150, 150, 150);
        public static Color MirrorSctMetalLightBase { get; } = Color.FromArgb(230, 230, 230);
        public static Color MirrorSctBafflePen { get; } = Color.DarkGray;

        #endregion

        #region Mirror line art (MCT secondary)

        public static Color MirrorMctGlassDarkBase { get; } = Color.FromArgb(85, 190, 190, 190);
        public static Color MirrorMctGlassLightBase { get; } = Color.FromArgb(55, 235, 235, 235);
        public static Color MirrorMctSecondaryDarkBase { get; } = Color.FromArgb(160, 160, 160);
        public static Color MirrorMctSecondaryLightBase { get; } = Color.FromArgb(220, 220, 220);

        #endregion

        #region Panels and misc UI

        public static Color BorderDefaultGray { get; } = Color.Gray;
        public static Color MirrorViewBackground { get; } = Color.FromArgb(32, 32, 32);
        public static Color MirrorOutline { get; } = Color.DimGray;
        public static Color RoundedRichTextTitleBack { get; } = Color.FromArgb(245, 245, 245);
        public static Color TitleBoxGlassFill { get; } = Color.FromArgb(35, 255, 255, 255);
        public static Color TitleBoxBorder { get; } = Color.LightGray;
        public static Color NextStepTitleHighlight { get; } = Color.FromArgb(45, 255, 200, 0);
        public static Color NextStepTitleFillNeutral { get; } = Color.FromArgb(40, 255, 255, 255);
        public static Color CalibrationTitleBarBack { get; } = Color.FromArgb(90, 90, 90);
        public static Color OffsetBarZeroTickLight { get; } = Color.LightGray;
        public static Color FocusChannelCaptionForeground { get; } = Color.Black;

        #endregion

        #region Channel / display line (shared RGB)

        public static Color ChannelAccent0 { get; } = Color.FromArgb(247, 69, 96);
        public static Color ChannelAccent1 { get; } = Color.LightGreen;
        public static Color ChannelAccent2 { get; } = Color.FromArgb(66, 179, 245);

        public static Color GroupBoxBackgroundChannel0 { get; } = Color.FromArgb(100, 70, 80);
        public static Color GroupBoxBackgroundChannel1 { get; } = Color.FromArgb(60, 90, 80);
        public static Color GroupBoxBackgroundChannel2 { get; } = Color.FromArgb(60, 70, 100);

        #endregion

        #region Next Step Font Sizes
        public static float NextStepFontSize => 10f;
        public static float NextStepTitleFontSize => 12f;

        #endregion

        #region Menu Strip Theme

        /// <summary>
        /// Gets the foreground color for the menu strip.
        /// </summary>
        public static Color MenuStripForeground { get; } = Color.LightGray;

        /// <summary>
        /// Gets the background color for highlighted menu items.
        /// </summary>
        public static Color MenuHighlightBackground { get; } = Color.Gray;

        #endregion

        #region Button Theme

        /// <summary>
        /// Gets the background color for dark-themed buttons.
        /// </summary>
        public static Color ButtonDarkBackground { get; } = Color.DimGray;

        /// <summary>
        /// Gets the foreground color for dark-themed buttons.
        /// </summary>
        public static Color ButtonDarkForeground { get; } = Color.LightGray;

        #endregion

        #region Text Box Theme

        /// <summary>
        /// Gets the background color for text boxes.
        /// </summary>
        public static Color TextBoxBackground { get; } = Color.DarkGray;

        /// <summary>
        /// Gets the foreground color for text boxes.
        /// </summary>
        public static Color TextBoxForeground { get; } = Color.Black;

        #endregion

        #region Error Circle Theme

        /// <summary>
        /// Gets the radius for displaying error circles.
        /// </summary>
        public static int ErrorCircleRadius { get; } = 64;

        #endregion

        #region Group Box Theme

        /// <summary>
        /// Gets the color for critical group box text.
        /// </summary>
        public static Color GetGroupBoxCriticalColor { get; } = Color.LightGray;

        #endregion

        #region Display Window

        /// <summary>
        /// Gets the baseline square display size at 96 DPI used for source-space normalization.
        /// </summary>
        public static float DisplayWindowReferenceSizeAt96 { get; } = 600f;

        /// <summary>
        /// Gets or sets the size of the display window.
        /// </summary>
        public static Point DisplayWindow => new Point(
            (int)Math.Round(DisplayWindowReferenceSizeAt96 * DpiScaleFactor),
            (int)Math.Round(DisplayWindowReferenceSizeAt96 * DpiScaleFactor)
        );

        /// <summary>
        /// Gets the background color for the display window.
        /// </summary>
        public static Color DisplayBackgroundColor { get; } = Color.Black;

        #endregion

        #region MessageBox Theme

        /// <summary>
        /// Gets the background color for message boxes.
        /// </summary>
        public static Color MessageBoxPanelBackground { get; } = Color.FromArgb(30, 30, 35);

        /// <summary>
        /// Gets the text color for message boxes.
        /// </summary>
        public static Color MessageBoxTextColor { get; } = Color.LightGray;

        #endregion

        #region Selection Theme

        /// <summary>
        /// Gets the background color for selection areas.
        /// </summary>
        public static Color SelectionBackground { get; } = Color.Black;

        /// <summary>
        /// Gets the transparency level for the selection background.
        /// </summary>
        public static float SelectionBackgroundTransparency { get; } = 0.75f;

        /// <summary>
        /// Gets the fill color for the selection circle interior (red with alpha; alpha controls transparency).
        /// </summary>
        public static Color SelectionCircleFill { get; } = Color.FromArgb(90, 255, 0, 0);

        /// <summary>
        /// Gets the outline color for selection circles.
        /// </summary>
        public static Color SelectionCircleBoarder { get; } = Color.Red;

        /// <summary>
        /// Gets the stroke width in pixels for the selection circle outline.
        /// </summary>
        public static int SelectionBoarderWidth { get; } = 10;

        /// <summary>
        /// Number of concentric bands used to draw the stroke gradient (inner transparent to outer opaque).
        /// </summary>
        public static int SelectionCircleStrokeGradientSteps { get; } = 20;

        #endregion

        #region Donate Theme

        /// <summary>
        /// Gets the text color for the donate section.
        /// </summary>
        public static Color DonateTextColor { get; } = Color.LightGray;

        /// <summary>
        /// Gets the background color for the donate picture.
        /// </summary>
        public static Color DonatePictureBackground { get; } = Color.LightGray;

        #endregion

        #region About Theme

        /// <summary>
        /// Gets the text color for the about section.
        /// </summary>
        public static Color AboutTextColor { get; } = Color.LightGray;

        /// <summary>
        /// Gets the background color for the about picture.
        /// </summary>
        public static Color AboutPictureBackground { get; } = Color.LightGray;

        #endregion

        #region ErrorBars Theme

        /// <summary>
        /// Gets the Error Bar color.
        /// </summary>
        public static Color ErrorBarColor { get; } = Color.Gray;

        /// <summary>
        /// Gets the Error Bar zero tick mark color.
        /// </summary>
        public static Color ZeroTickColor { get; } = Color.DarkGray;

        /// <summary>
        /// Gets the Error Bar text color.
        /// </summary>
        public static Color ErrorBarTextColor { get; } = Color.LightGray;

        /// <summary>
        /// Gets the Error Marker color for the Error Bar when the marker is in the valid range.
        /// </summary>
   //     public static Color ErrorBarMarkerColorInRange { get; } = Color.MediumSeaGreen;
        public static Color ErrorBarMarkerColorInRange { get; } = Color.OrangeRed;

        /// <summary>
        /// Gets the Error Marker History dot color for the Error Bar.
        /// </summary>
        public static Color ErrorBarHistoryColor { get; } = Color.FromArgb(128, Color.LightGray);

        /// <summary>
        /// Gets the Error Marker History dot Pen color for the Error Bar.
        /// </summary>
        public static Color ErrorBarHistoryPenColor { get; } = Color.FromArgb(255, Color.Black);

        /// <summary>
        /// Gets the Error Marker Outline Pen color for the Error Bar.
        /// </summary>
        public static Color ErrorBarMarkerPen { get; } = Color.Black;

        /// <summary>
        /// Gets the Defocus Inner Circle color.     
        ///</summary>
        public static Color DefocusInnerCircleColor { get; } = Color.Red;

        /// <summary>
        ///     
        /// Gets the Defocus Outer Circle color.
        /// </summary>
        public static Color DefocusOuterCircleColor { get; } = Color.LightGreen;    

        /// <summary>
        ///Gets the ToggleSwitch GroupBox Foreground Color
        /// </summary>
        public static Color ImageCaptureGroupBoxColor {  get; } = Color.LightGray; 
        public static Color ImageCaptureGroupBoxDisabledColor { get; } = Color.Gray;

        #endregion

        #region Calibration

        /// <summary>
        /// Gets the width of the calibration side frame to the main form at 96DPI
        /// </summary>
        public static int CalibrateFrameWidth { get; } = 350;
        #endregion

        #region Dictionaries

        private static readonly Dictionary<int, Color> GroupBoxTextColors;
        private static readonly Dictionary<int, Color> GroupBoxBackgroundColors;
        private static readonly Dictionary<int, Color> DisplayLineColors;
        private static readonly Dictionary<int, Pen> DisplayLinePens;
        private static readonly Dictionary<int, Pen> ErrorCirclePens;
        private static readonly Dictionary<int, Font> ErrorTextFonts;
        private static readonly Dictionary<int, SolidBrush> ErrorTextBrush;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes theme dictionaries for group colors, display lines, and error rendering resources.
        /// </summary>
        static UITheme()
        {
            GroupBoxTextColors = new Dictionary<int, Color>();
            GroupBoxBackgroundColors = new Dictionary<int, Color>();
            DisplayLineColors = new Dictionary<int, Color>();
            DisplayLinePens = new Dictionary<int, Pen>();
            ErrorCirclePens = new Dictionary<int, Pen>();
            ErrorTextFonts = new Dictionary<int, Font>();
            ErrorTextBrush = new Dictionary<int, SolidBrush>();

            GroupBoxTextColors[0] = ChannelAccent0;
            GroupBoxTextColors[1] = ChannelAccent1;
            GroupBoxTextColors[2] = ChannelAccent2;

            GroupBoxBackgroundColors[0] = GroupBoxBackgroundChannel0;
            GroupBoxBackgroundColors[1] = GroupBoxBackgroundChannel1;
            GroupBoxBackgroundColors[2] = GroupBoxBackgroundChannel2;

            DisplayLineColors[0] = ChannelAccent0;
            DisplayLineColors[1] = ChannelAccent1;
            DisplayLineColors[2] = ChannelAccent2;

            DisplayLinePens[0] = new Pen(White, 2.0f) { DashStyle = DashStyle.Dash };
            DisplayLinePens[1] = new Pen(White, 2.0f) { DashStyle = DashStyle.Dash };
            DisplayLinePens[2] = new Pen(White, 2.0f) { DashStyle = DashStyle.Dash };

            ErrorCirclePens[0] = new Pen(DisplayLineColors[0], 5.0f);
            ErrorCirclePens[1] = new Pen(DisplayLineColors[1], 5.0f);
            ErrorCirclePens[2] = new Pen(DisplayLineColors[2], 5.0f);

            ErrorTextFonts[0] = new Font("Segoe UI", 16f);
            ErrorTextFonts[1] = new Font("Segoe UI", 16f);
            ErrorTextFonts[2] = new Font("Segoe UI", 16f);

            ErrorTextBrush[0] = new SolidBrush(White);
            ErrorTextBrush[1] = new SolidBrush(White);
            ErrorTextBrush[2] = new SolidBrush(White);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the text color for a group box based on the group ID.
        /// </summary>
        /// <param name="groupId">The group ID.</param>
        /// <returns>The color for the group box text.</returns>
        public static Color GetGroupBoxTextColor(int groupId)
        {
            if (GroupBoxTextColors.ContainsKey(groupId))
                return GroupBoxTextColors[groupId];
            return Black;
        }

        /// <summary>
        /// Gets the background color for a group box based on the group ID.
        /// </summary>
        /// <param name="groupId">The group ID.</param>
        /// <returns>The color for the group box background.</returns>
        public static Color GetGroupBoxBackgroundColor(int groupId)
        {
            if (GroupBoxBackgroundColors.ContainsKey(groupId))
                return GroupBoxBackgroundColors[groupId];
            return Black;
        }

        /// <summary>
        /// Gets the pen used for displaying lines based on the group ID and line ID.
        /// </summary>
        /// <param name="groupId">The group ID.</param>
        /// <param name="lineId">The line ID.</param>
        /// <returns>The pen for displaying lines.</returns>
        public static Pen GetDisplayLinePen(int groupId, int lineId)
        {
            if (DisplayLineColors.ContainsKey(groupId) && DisplayLinePens.ContainsKey(lineId))
            {
                Pen pen = DisplayLinePens[lineId];
                pen.Color = DisplayLineColors[groupId];
                return pen;
            }
            return new Pen(White);
        }

        /// <summary>
        /// Gets the pen used for drawing error circles based on the group ID.
        /// </summary>
        /// <param name="groupId">The group ID.</param>
        /// <returns>The pen for drawing error circles.</returns>
        public static Pen GetErrorCirclePen(int groupId)
        {
            if (ErrorCirclePens.ContainsKey(groupId))
            {
                Pen pen = ErrorCirclePens[groupId];
                pen.DashStyle = DashStyle.Solid;
                return pen;
            }
            return new Pen(White);
        }

        /// <summary>
        /// Gets the font used for error text based on the group ID.
        /// </summary>
        /// <param name="groupId">The group ID.</param>
        /// <returns>The font for error text.</returns>
        public static Font GetErrorTextFont(int groupId)
        {
            if (ErrorTextFonts.ContainsKey(groupId))
            {
                return ErrorTextFonts[groupId];
            }
            return new Font("Segoe UI", 16f);
        }

        /// <summary>
        /// Gets the brush used for error text based on the group ID.
        /// </summary>
        /// <param name="groupId">The group ID.</param>
        /// <returns>The brush for error text.</returns>
        public static SolidBrush GetErrorTextBrush(int groupId)
        {
            if (ErrorTextBrush.ContainsKey(groupId))
            {
                return ErrorTextBrush[groupId];
            }
            return new SolidBrush(White);
        }

        /// <summary>
        /// Returns <paramref name="c"/> with the alpha channel replaced.
        /// </summary>
        public static Color WithAlpha(Color c, int alpha) => Color.FromArgb(alpha, c.R, c.G, c.B);

        /// <summary>
        /// Constructs an ARGB color (single theme entry point for non-literal composition).
        /// </summary>
        public static Color FromArgb(int a, int r, int g, int b) => Color.FromArgb(a, r, g, b);

        /// <summary>
        /// Linearly interpolates two RGBA colors.
        /// </summary>
        public static Color LerpRgb(Color a, Color b, float k)
        {
            if (k < 0f) k = 0f;
            else if (k > 1f) k = 1f;
            return Color.FromArgb(
                (int)(a.A + (b.A - a.A) * k + 0.5f),
                (int)(a.R + (b.R - a.R) * k + 0.5f),
                (int)(a.G + (b.G - a.G) * k + 0.5f),
                (int)(a.B + (b.B - a.B) * k + 0.5f));
        }

        #endregion
    }
}
