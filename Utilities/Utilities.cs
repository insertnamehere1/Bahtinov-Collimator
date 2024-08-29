using System;
using System.Drawing;
using System.IO;
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
        public static string ComputeHash(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            // Convert the Bitmap to a byte array more efficiently
            byte[] imageBytes = ImageToByteArray(bitmap);

            // Compute the SHA-256 hash
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(imageBytes);

                // Convert the byte array to a hexadecimal string
                StringBuilder hexString = new StringBuilder(hashBytes.Length * 2);
                foreach (byte b in hashBytes)
                {
                    hexString.AppendFormat("{0:x2}", b);
                }

                return hexString.ToString();
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

            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
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
