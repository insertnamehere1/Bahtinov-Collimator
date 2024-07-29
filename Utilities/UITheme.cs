using System;
using System.Collections.Generic;
using System.Drawing;

namespace Bahtinov_Collimator
{
    public static class UITheme
    {
        public static Color DisplayBackgroundColor { get; } = Color.Black;
        public static Color DarkBackground { get; } = Color.FromArgb(47, 54, 64);
        public static Color DarkForeground { get; } = Color.FromArgb(70, 80, 90);
        public static Color MenuDarkBackground { get; } = DarkBackground;
        public static Color MenuDarkForeground { get; } = Color.LightGray;
        public static Color ButtonDarkBackground { get; } = Color.DimGray;
        public static Color ButtonDarkForeground { get; } = Color.LightGray;
        public static Color TextBoxBackground { get; } = Color.DarkGray;
        public static Color TextBoxForeground { get; } = Color.Black;
        public static Color MenuStripForeground { get; } = Color.White;
        public static Color MenuHighlightBackground { get; } = Color.Gray;  


        private static readonly Dictionary<int, Color> GroupBoxTextColors;
        private static readonly Dictionary<int, Color> GroupBoxBackgroundColors;

        static UITheme()
        {
            GroupBoxTextColors = new Dictionary<int, Color>();
            GroupBoxBackgroundColors = new Dictionary<int, Color>();

            // Initialize GroupBox font colors
            GroupBoxTextColors[0] = Color.LightSalmon;
            GroupBoxTextColors[1] = Color.LightGreen;
            GroupBoxTextColors[2] = Color.LightSteelBlue;

            // Initialize GroupBox mouse over colors
            GroupBoxBackgroundColors[0] = Color.FromArgb(80, 70, 80);
            GroupBoxBackgroundColors[1] = Color.FromArgb(60, 90, 80);
            GroupBoxBackgroundColors[2] = Color.FromArgb(60, 70, 100);
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
    }
}
