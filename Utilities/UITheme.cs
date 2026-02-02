using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

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
        public static Color DarkBackground { get; } = Color.FromArgb(47, 54, 64);

        /// <summary>
        /// Gets the foreground color for dark-themed forms.
        /// </summary>
        public static Color DarkForeground { get; } = Color.FromArgb(70, 80, 90);

        #endregion

        #region Menu Strip Theme

        /// <summary>
        /// Gets the foreground color for the menu strip.
        /// </summary>
        public static Color MenuStripForeground { get; } = Color.White;

        /// <summary>
        /// Gets the background color for highlighted menu items.
        /// </summary>
        public static Color MenuHighlightBackground { get; } = Color.Gray;

        #endregion

        #region Drop Down Menu Theme

        /// <summary>
        /// Gets the background color for dark-themed drop-down menus.
        /// </summary>
        public static Color MenuDarkBackground { get; } = DarkBackground;

        /// <summary>
        /// Gets the foreground color for dark-themed drop-down menus.
        /// </summary>
        public static Color MenuDarkForeground { get; } = Color.LightGray;

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
        public static Color GetGroupBoxCriticalColor { get; } = Color.White;

        #endregion

        #region Display Window

        /// <summary>
        /// Gets or sets the size of the display window.
        /// </summary>
        public static Point DisplayWindow { get; private set; } = new Point(600, 600);

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
        public static Color MessageBoxTextColor { get; } = Color.White;

        #endregion

        #region Selection Theme

        /// <summary>
        /// Gets the background color for selection areas.
        /// </summary>
        public static Color SelectionBackground { get; } = Color.Black;

        /// <summary>
        /// Gets the transparency level for the selection background.
        /// </summary>
        public static float SelectionBackgroundTransparency { get; } = 0.7f;

        /// <summary>
        /// Gets the fill color for selection circles.
        /// </summary>
        public static Color SelectionCircleInfill { get; } = Color.FromArgb(247, 69, 96);

        /// <summary>
        /// Gets the border color for selection circles.
        /// </summary>
        public static Color SelectionCircleBoarder { get; } = Color.Red;

        /// <summary>
        /// Gets the border width for selection circles.
        /// </summary>
        public static int SelectionBoarderWidth { get; } = 3;

        #endregion

        #region Donate Theme

        /// <summary>
        /// Gets the text color for the donate section.
        /// </summary>
        public static Color DonateTextColor { get; } = Color.White;

        /// <summary>
        /// Gets the background color for the donate picture.
        /// </summary>
        public static Color DonatePictureBackground { get; } = Color.LightGray;

        #endregion

        #region About Theme

        /// <summary>
        /// Gets the text color for the about section.
        /// </summary>
        public static Color AboutTextColor { get; } = Color.White;

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
        public static Color ErrorBarTextColor { get; } = Color.White;

        /// <summary>
        /// Gets the Error Marker color for the Error Bar when the marker is in the valid range.
        /// </summary>
        public static Color ErrorBarMarkerColorInRange { get; } = Color.Green;

        /// <summary>
        /// Gets the Error Marker color for the Error Bar when the marker is outside the valid range.
        /// </summary>
        public static Color ErrorBarMarkerColorOutOfRange { get; } = Color.OrangeRed;

        /// <summary>
        /// Gets the Error Marker History dot color for the Error Bar.
        /// </summary>
        public static Color ErrorBarHistoryColor { get; } = Color.FromArgb(128, Color.White);

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
        public static Color AnalysisGroupBoxColor {  get; } = Color.LightGray; 

        #endregion

        #region Calibration

        /// <summary>
        /// Gets the width of the calibration side frame to the main form.
        /// </summary>
        public static int CalibrateFrameWidth { get; } = 400;
        
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

        static UITheme()
        {
            GroupBoxTextColors = new Dictionary<int, Color>();
            GroupBoxBackgroundColors = new Dictionary<int, Color>();
            DisplayLineColors = new Dictionary<int, Color>();
            DisplayLinePens = new Dictionary<int, Pen>();
            ErrorCirclePens = new Dictionary<int, Pen>();
            ErrorTextFonts = new Dictionary<int, Font>();
            ErrorTextBrush = new Dictionary<int, SolidBrush>();

            // Initialize GroupBox font colors
            GroupBoxTextColors[0] = Color.FromArgb(247, 69, 96);
            GroupBoxTextColors[1] = Color.LightGreen;
            GroupBoxTextColors[2] = Color.FromArgb(66, 179, 245);

            // Initialize GroupBox mouse over colors
            GroupBoxBackgroundColors[0] = Color.FromArgb(80, 70, 80);
            GroupBoxBackgroundColors[1] = Color.FromArgb(60, 90, 80);
            GroupBoxBackgroundColors[2] = Color.FromArgb(60, 70, 100);

            // Initialize Display Line Colors
            DisplayLineColors[0] = Color.FromArgb(247, 69, 96);
            DisplayLineColors[1] = Color.LightGreen;
            DisplayLineColors[2] = Color.FromArgb(66, 179, 245);

            // Initialize Display Line Pens
            DisplayLinePens[0] = new Pen(Color.White, 2.0f) { DashStyle = DashStyle.Dash };
            DisplayLinePens[1] = new Pen(Color.White, 2.0f) { DashStyle = DashStyle.Dash };
            DisplayLinePens[2] = new Pen(Color.White, 2.0f) { DashStyle = DashStyle.Dash };

            // Initialize Error Circle Pens
            ErrorCirclePens[0] = new Pen(DisplayLineColors[0], 5.0f);
            ErrorCirclePens[1] = new Pen(DisplayLineColors[1], 5.0f);
            ErrorCirclePens[2] = new Pen(DisplayLineColors[2], 5.0f);

            // Initialize Error Text Fonts
            ErrorTextFonts[0] = new Font("Arial", 16f);
            ErrorTextFonts[1] = new Font("Arial", 16f);
            ErrorTextFonts[2] = new Font("Arial", 16f);

            // Initialize Error Text Brush
            ErrorTextBrush[0] = new SolidBrush(Color.White);
            ErrorTextBrush[1] = new SolidBrush(Color.White);
            ErrorTextBrush[2] = new SolidBrush(Color.White);
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
            return Color.Black; // Default color if not found
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
            return Color.Black; // Default color if not found
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
            return new Pen(Color.White); // Default color if not found
        }

        /// <summary>
        /// Gets the pen used for drawing error circles based on the group ID and focus status.
        /// </summary>
        /// <param name="groupId">The group ID.</param>
        /// <param name="insideFocus">Whether the focus is inside or outside.</param>
        /// <returns>The pen for drawing error circles.</returns>
        public static Pen GetErrorCirclePen(int groupId)
        {
            if (ErrorCirclePens.ContainsKey(groupId))
            {
                Pen pen = ErrorCirclePens[groupId];
                pen.DashStyle = DashStyle.Solid;
                return pen;
            }
            return new Pen(Color.White); // Default color if not found
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
            return new Font("Arial", 16f); // Default font if not found
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
            return new SolidBrush(Color.White); // Default color if not found
        }

        #endregion
    }
}
