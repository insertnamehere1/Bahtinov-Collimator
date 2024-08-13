using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Bahtinov_Collimator
{
    public static class UITheme
    {
        // Forms theme
        public static Color DarkBackground { get; } = Color.FromArgb(47, 54, 64);
        public static Color DarkForeground { get; } = Color.FromArgb(70, 80, 90);

        // Menu Strip
        public static Color MenuStripForeground { get; } = Color.White;
        public static Color MenuHighlightBackground { get; } = Color.Gray;

        // Drop Down Menu theme
        public static Color MenuDarkBackground { get; } = DarkBackground;
        public static Color MenuDarkForeground { get; } = Color.LightGray;

        //Button Theme
        public static Color ButtonDarkBackground { get; } = Color.DimGray;
        public static Color ButtonDarkForeground { get; } = Color.LightGray;

        // Text Box theme
        public static Color TextBoxBackground { get; } = Color.DarkGray;
        public static Color TextBoxForeground { get; } = Color.Black;

        // Display Error Circle Theme
        public static int ErrorCircleRadius { get; } = 64;

        // Groupbox Theme
        public static Color GetGroupBoxCriticalColor { get; } = Color.White;

        // Display Window
        public static Point DisplayWindow { get; } = new Point(600, 600);
        public static Color DisplayBackgroundColor { get; } = Color.Black;

        // MessageBox Theme
        public static Color MessageBoxPanelBackground { get; } = Color.FromArgb(30, 30, 35);
        public static Color MessageBoxTextColor { get; } = Color.White;

        //Selection Theme
        public static Color SelectionBackground { get; } = Color.Black;
        public static float SelectionBackgroundTransparency { get; } = 0.5f;
        public static Color SelectionCircleInfill { get; } = Color.FromArgb(237, 78, 78);
        public static Color SelectionCircleBoarder { get; } = Color.DarkBlue;
        public static int SelectionBoarderWidth { get; } = 3; 
        
        // Donate Theme
        public static Color DonateTextColor { get; } = Color.White;
        public static Color DonatePictureBackground { get; } = Color.Gray;

        // About Theme
        public static Color AboutTextColor { get; } = Color.White;
        public static Color AboutPictureBackground { get; } = Color.Gray;

        // Group Box Color Theme Dictionary
        private static readonly Dictionary<int, Color> GroupBoxTextColors;
        private static readonly Dictionary<int, Color> GroupBoxBackgroundColors;

        // Display Bahtinov Lines Theme
        private static readonly Dictionary<int, Color> DisplayLineColors;
        private static readonly Dictionary<int, Pen> DisplayLinePens;
        private static readonly Dictionary<int, Pen> ErrorCirclePens;
        private static readonly Dictionary<int, Font> ErrorTextFonts;
        private static readonly Dictionary<int, SolidBrush> ErrorTextBrush;

        // AdjustAssist Theme
        public static Color AdjustAssistKnobHi { get; } = Color.FromArgb(100, 100, 100);
        public static Color AdjustAssistKnobLo { get; } = Color.FromArgb(200, 200, 200);
        public static Color AdjustAssistTextColor { get; } = Color.Black;

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

            // Initialize Error Text Pen
            ErrorTextBrush[0] = new SolidBrush(Color.White);
            ErrorTextBrush[1] = new SolidBrush(Color.White);
            ErrorTextBrush[2] = new SolidBrush(Color.White);
        }

        public static Color GetGroupBoxTextColor(int groupId)
        {
            if (GroupBoxTextColors.ContainsKey(groupId))
                return GroupBoxTextColors[groupId];
            return Color.Black; // Default color if not found
        }

        public static Color GetGroupBoxBackgroundColor(int groupId)
        {
            if (GroupBoxBackgroundColors.ContainsKey(groupId))
                return GroupBoxBackgroundColors[groupId];
            return Color.Black; // Default color if not found
        }

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

        public static Pen GetErrorCirclePen(int groupId, bool insideFocus)
        {
            if (ErrorCirclePens.ContainsKey(groupId))
            {
                Pen pen = ErrorCirclePens[groupId];

                if (insideFocus)
                    pen.DashStyle = DashStyle.Solid;
                else
                    pen.DashStyle = DashStyle.Dash;

                return pen;
            }
            return new Pen(Color.White); // Default color if not found
        }

        public static Font GetErrorTextFont(int groupId)
        {
            if (ErrorTextFonts.ContainsKey(groupId))
            {
                return ErrorTextFonts[groupId];
            }
            return new Font("Arial", 16f); // Default color if not found
        }

        public static SolidBrush GetErrorTextBrush(int groupId)
        {
            if (ErrorTextBrush.ContainsKey(groupId))
            {
                return ErrorTextBrush[groupId];
            }
            return new SolidBrush(Color.White); // Default color if not found
        }
    }
}
