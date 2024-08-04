using System;
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

        // Group Box Color Theme Dictionary
        private static readonly Dictionary<int, Color> GroupBoxTextColors;
        private static readonly Dictionary<int, Color> GroupBoxBackgroundColors;

        // Display Bahtinov Lines Theme
        private static readonly Dictionary<int, Color> DisplayLineColors;
        private static readonly Dictionary<int, Pen> DisplayLinePens;

        static UITheme()
        {
            GroupBoxTextColors = new Dictionary<int, Color>();
            GroupBoxBackgroundColors = new Dictionary<int, Color>();
            DisplayLineColors = new Dictionary<int, Color>();
            DisplayLinePens = new Dictionary<int, Pen>();

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
    }
}






//// select pen colour for the lines
//switch (group)
//{
//    case 0:
//        dashPen.Color = Color.Red;
//        largeDashPen.Color = Color.Red;
//        break;
//    case 1:
//        dashPen.Color = Color.Green;
//        largeDashPen.Color = Color.Green;
//        break;
//    case 2:
//        dashPen.Color = Color.Blue;
//        largeDashPen.Color = Color.Blue;
//        break;
//}




//Pen errorPen;
//if (withinCriticalFocus)
//    errorPen = largeSolidPen;
//else
//    errorPen = dashPen;

//// select pen colour
//switch (group)
//{
//    case 0:
//        errorPen.Color = Color.Red;
//        break;
//    case 1:
//        errorPen.Color = Color.Green;
//        break;
//    case 2:
//        errorPen.Color = Color.Blue;
//        break;
//    default:
//        errorPen.Color = Color.White;
//        break;
//}

//// draw the circular error marker
//errorPen.Width = 5;
