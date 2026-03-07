using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Bahtinov_Collimator
{
    internal static class Utilities
    {
         #region Structures

        /// <summary>
        /// Represents a rectangular area defined by the coordinates of its top-left and bottom-right corners.
        /// </summary>
        public struct RECT
        {
            /// <summary>
            /// Gets or sets the x-coordinate of the left side of the rectangle.
            /// </summary>
            public int Left { get; set; }

            /// <summary>
            /// Gets or sets the y-coordinate of the top side of the rectangle.
            /// </summary>
            public int Top { get; set; }

            /// <summary>
            /// Gets or sets the x-coordinate of the right side of the rectangle.
            /// </summary>
            public int Right { get; set; }

            /// <summary>
            /// Gets or sets the y-coordinate of the bottom side of the rectangle.
            /// </summary>
            public int Bottom { get; set; }
        }

        #endregion
    }
}
