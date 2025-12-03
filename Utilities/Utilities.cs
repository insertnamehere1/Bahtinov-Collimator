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
        /// Computes the SHA-256 hash of the given bitmap image.
        /// </summary>
        /// <param name="bitmap">The bitmap image to compute the hash for.</param>
        /// <returns>A hexadecimal string representing the SHA-256 hash of the bitmap.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="bitmap"/> parameter is null.</exception>
        public static string ComputeHash(Bitmap bmp)
        {
            if (bmp == null)
                throw new ArgumentNullException(nameof(bmp));

            const int regionSize = 100;

            int startX = (bmp.Width - regionSize) / 2;
            int startY = (bmp.Height - regionSize) / 2;

            var rect = new Rectangle(startX, startY, regionSize, regionSize);

            // Lock only the 100×100 region
            BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            try
            {
                int bytes = Math.Abs(data.Stride) * regionSize;
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

        #region Image Conversion

        /// <summary>
        /// Converts a bitmap image to a byte array.
        /// </summary>
        /// <param name="image">The bitmap image to convert.</param>
        /// <returns>A byte array representation of the bitmap image.</returns>
        private static byte[] ImageToByteArray(Bitmap image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            // Lock the bitmap's bits for fast, unmanaged access
            var rect = new Rectangle(0, 0, image.Width, image.Height);
            var bmpData = image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            try
            {
                int bytes = Math.Abs(bmpData.Stride) * bmpData.Height;
                byte[] buffer = new byte[bytes];

                // Copy the RGB values into the managed array
                Marshal.Copy(bmpData.Scan0, buffer, 0, bytes);

                return buffer;
            }
            finally
            {
                image.UnlockBits(bmpData);
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
