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
        #region Bitmap Operations

        /// <summary>
        /// Resizes a bitmap to the specified width and height using high-quality bicubic interpolation.
        /// </summary>
        /// <param name="sourceBitmap">The original bitmap to be resized. Must not be null.</param>
        /// <param name="width">The desired width of the resized bitmap.</param>
        /// <param name="height">The desired height of the resized bitmap.</param>
        /// <returns>A new bitmap that is resized to the specified dimensions.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="sourceBitmap"/> parameter is null.</exception>
        public static Bitmap ResizeBitmap(Bitmap sourceBitmap, int width, int height)
        {
            if (sourceBitmap == null)
                throw new ArgumentNullException(nameof(sourceBitmap));

            var resizedBitmap = new Bitmap(width, height);

            using (var graphics = Graphics.FromImage(resizedBitmap))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(sourceBitmap, 0, 0, width, height);
            }

            return resizedBitmap;
        }

        /// <summary>
        /// Computes the SHA-256 hash for the centre 100x100 pixels of the given bitmap image.
        /// </summary>
        /// <param name="bitmap">The bitmap image to compute the hash for.</param>
        /// <returns>A hexadecimal string representing the SHA-256 hash of the bitmap.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="bitmap"/> parameter is null.</exception>
        public static string ComputeHash(Bitmap bmp)
        {
            if (bmp == null)
                throw new ArgumentNullException(nameof(bmp));

            const int regionSize = 100;

            int width = bmp.Width;
            int height = bmp.Height;

            if (width <= 0 || height <= 0)
                return string.Empty;

            // Determine actual region size (use smaller when needed)
            int sampleWidth = Math.Min(regionSize, width);
            int sampleHeight = Math.Min(regionSize, height);

            int startX = (width - sampleWidth) / 2;
            int startY = (height - sampleHeight) / 2;

            Rectangle rect = new Rectangle(startX, startY, sampleWidth, sampleHeight);

            BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            try
            {
                int bytes = Math.Abs(data.Stride) * sampleHeight;
                byte[] buffer = new byte[bytes];

                Marshal.Copy(data.Scan0, buffer, 0, bytes);

                using (SHA256 sha = SHA256.Create())
                {
                    byte[] hashBytes = sha.ComputeHash(buffer);

                    StringBuilder sb = new StringBuilder(hashBytes.Length * 2);
                    foreach (byte b in hashBytes)
                        sb.AppendFormat("{0:x2}", b);

                    return sb.ToString();
                }
            }
            finally
            {
                bmp.UnlockBits(data);
            }
        }

        #endregion

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
