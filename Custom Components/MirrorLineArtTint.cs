using System.Drawing;

namespace Bahtinov_Collimator.Custom_Components
{
    /// <summary>Shared tint blending for MCT / SCT mirror line art fills.</summary>
    internal static class MirrorLineArtTint
    {
        #region Color Utilities

        /// <summary>
        /// Blends <paramref name="baseColor"/> toward <paramref name="tint"/> by a normalized strength value.
        /// </summary>
        /// <returns>A color with preserved alpha and RGB channels blended toward the tint color.</returns>
        internal static Color TintColor(Color baseColor, Color tint, float strength)
        {
            strength = System.Math.Max(0f, System.Math.Min(1f, strength));

            int r = (int)(baseColor.R + (tint.R - baseColor.R) * strength);
            int g = (int)(baseColor.G + (tint.G - baseColor.G) * strength);
            int b = (int)(baseColor.B + (tint.B - baseColor.B) * strength);

            return Color.FromArgb(baseColor.A, r, g, b);
        }

        #endregion
    }
}
