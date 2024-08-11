using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    internal class ImageCapture
    {
        // P/Invoke declarations for interacting with the Windows API

        /// <summary>
        /// Retrieves the dimensions of the window specified by the given handle.
        /// </summary>
        /// <param name="hWnd">Handle to the window.</param>
        /// <param name="lpRect">Structure that receives the window coordinates.</param>
        /// <returns>True if the function succeeds; otherwise, false.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out Utilities.RECT lpRect);

        /// <summary>
        /// Captures a bitmap image of the specified window's client area and sends it to the device context.
        /// </summary>
        /// <param name="hwnd">Handle to the window to capture.</param>
        /// <param name="hdcBlt">Handle to the device context to which the image is copied.</param>
        /// <param name="nFlags">Flags specifying how to capture the window.</param>
        /// <returns>True if the function succeeds; otherwise, false.</returns>
        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, int nFlags);

        /// <summary>
        /// Retrieves the handle to the window that contains the specified point.
        /// </summary>
        /// <param name="p">The point in screen coordinates.</param>
        /// <returns>Handle to the window that contains the point.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr WindowFromPoint(Point p);

        // Event and delegate for image received

        /// <summary>
        /// Represents the method that will handle the ImageReceived event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An ImageReceivedEventArgs that contains the event data.</param>
        public delegate void ImageReceivedEventHandler(object sender, ImageReceivedEventArgs e);

        /// <summary>
        /// Occurs when a new image has been captured.
        /// </summary>
        public static event ImageReceivedEventHandler ImageReceivedEvent;

        // Constants for PrintWindow function flags
        private const int PW_RENDERFULLCONTENT = 0x2;

        // Private fields for managing image capture
        private static Timer captureTimer;
        private static Rectangle captureRectangle;
        private static IntPtr targetWindowHandle;
        private static Rectangle selectedStarBox;

        private static string previousImageHash = "";
        private static bool newHashFound = false;
        private static string firstHash = "";

        public static bool TrackingEnabled { get; set; } = false;

        /// <summary>
        /// Starts the image capture process by initializing and starting a timer.
        /// The timer triggers the CaptureTimer_Tick method at regular intervals.
        /// </summary>
        public static void StartImageCapture()
        {
            StopImageCapture(); // Ensure any existing timer is stopped

            captureTimer = new Timer
            {
                Interval = 200 // Capture every 200 ms
            };
            captureTimer.Tick += CaptureTimer_Tick;
            captureTimer.Start();
        }

        /// <summary>
        /// Stops the image capture process by stopping and disposing of the timer.
        /// </summary>
        public static void StopImageCapture()
        {
            captureTimer?.Stop();
            captureTimer?.Dispose();
            captureTimer = null;
        }

        /// <summary>
        /// Handles the Timer.Tick event to capture an image and invoke the ImageReceivedEvent.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments for the tick event.</param>
        private static void CaptureTimer_Tick(object sender, EventArgs e)
        {
            Bitmap image = GetImage();

            if (image == null)
            {
                return;
            }

            // Check if this is a new image
            string secondHash = Utilities.ComputeHash(image);

            if (firstHash.Equals(secondHash))
                return;
            else
                firstHash = secondHash;

            Bitmap latestImage = CreateCircularImage(image);
            Bitmap updatedImage = new Bitmap(UITheme.DisplayWindow.X, UITheme.DisplayWindow.Y);
            Graphics g = null;

            try
            {
                g = Graphics.FromImage(updatedImage);

                // Fill the entire image with black before drawing
                g.Clear(Color.Black);

                if (TrackingEnabled)
                {
                    // Get offset for the brightest area
                    Point offset = FindBrightestAreaCentroid(latestImage);

                    // Calculate translation
                    int delta_X = (latestImage.Width / 2) - offset.X;
                    int delta_Y = (latestImage.Height / 2) - offset.Y;

                    // Update selectedStarBox with new position
                    selectedStarBox.Offset(-delta_X, -delta_Y);

                    // Apply translation transformation to the Graphics object
                    g.TranslateTransform(delta_X, delta_Y);
                }

                // Calculate the center position
                int x = (UITheme.DisplayWindow.X - latestImage.Width) / 2;
                int y = (UITheme.DisplayWindow.Y - latestImage.Height) / 2;

                // Draw the image at the calculated position
                g.DrawImage(latestImage, x, y);

                Bitmap circularImage = updatedImage;
                
                // Check if this is a new image
                string newHash = Utilities.ComputeHash(circularImage);

                if (newHashFound)
                {
                    ImageReceivedEvent?.Invoke(null, new ImageReceivedEventArgs(circularImage));
                    newHashFound = false; // Reset the flag after triggering the event
                    previousImageHash = newHash;
                }
                else if (!newHash.Equals(previousImageHash))
                {
                    newHashFound = true; // Set the flag if a new hash is found
                    previousImageHash = newHash;
                }
            }
            finally
            {
                // Dispose of the Graphics object if it was created
                if (g != null)
                {
                    g.Dispose();
                }

                // Dispose of the original image
                latestImage.Dispose();
            }
        }

        private static Bitmap CreateCircularImage(Bitmap originalImage)
        {
            int diameter = Math.Min(selectedStarBox.Width, selectedStarBox.Height);

            if (diameter == 0)
            {
                DarkMessageBox.Show("The selection area is too small", "Information", MessageBoxIcon.Information, MessageBoxButtons.OK);
                return null;
            }

            Bitmap circularImage = new Bitmap(diameter, diameter);

            using (Graphics g = Graphics.FromImage(circularImage))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                // Create a circle and fill it with the original image
                using (TextureBrush brush = new TextureBrush(originalImage))
                {
                    g.FillEllipse(brush, 0, 0, diameter, diameter);
                }
            }

            return circularImage;
        }

        /// <summary>
        /// Opens a dialog for selecting a region of the screen to capture.
        /// Sets the target window handle and adjusts the selected area based on the window's coordinates.
        /// </summary>
        /// <returns>True if the selection was successful; otherwise, false.</returns>
        public static bool SelectStar()
        {
            using (var overlay = new ImageSelect())
            {
                if (overlay.ShowDialog() == DialogResult.OK)
                {
                    selectedStarBox = overlay.SelectedArea;
                    targetWindowHandle = WindowFromPoint(new Point(
                        selectedStarBox.Left + selectedStarBox.Width / 2,
                        selectedStarBox.Top + selectedStarBox.Height / 2
                    ));

                    if (targetWindowHandle == IntPtr.Zero)
                    {
                        MessageBox.Show("No application found under the selected area");
                        return false;
                    }

                    if (GetWindowRect(targetWindowHandle, out Utilities.RECT rect))
                    {
                        // Adjust the selected area to be relative to the window's coordinates
                        selectedStarBox.Offset(-rect.Left, -rect.Top);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Captures an image of the specified window area based on the selected rectangle.
        /// </summary>
        /// <returns>A Bitmap object of the captured image or null if capturing failed.</returns>
        private static Bitmap GetImage()
        {
            if (!GetWindowRect(targetWindowHandle, out Utilities.RECT rect))
            {
                ImageLostEventProvider.OnImageLost("Image has been lost");
                return null;
            }

            int windowWidth = rect.Right - rect.Left;
            int windowHeight = rect.Bottom - rect.Top;

            // Adjust selectedStarBox to be within the bounds of the window
            selectedStarBox.X = Math.Max(selectedStarBox.X, 0);
            selectedStarBox.Y = Math.Max(selectedStarBox.Y, 0);
            selectedStarBox.Width = Math.Min(selectedStarBox.Width, windowWidth - selectedStarBox.X);
            selectedStarBox.Height = Math.Min(selectedStarBox.Height, windowHeight - selectedStarBox.Y);

            // Ensure the selected rectangle is valid
            if (selectedStarBox.Width == 0 || selectedStarBox.Height == 0)
            {
                ImageLostEventProvider.OnImageLost("The selection area is too small.");
                return null;
            }

            try
            {
                using (var fullWindowBitmap = CaptureImage(targetWindowHandle))
                {
                    if (fullWindowBitmap == null)
                    {
                        ImageLostEventProvider.OnImageLost("Unable to capture image");
                        return null;
                    }

                    return fullWindowBitmap.Clone(selectedStarBox, fullWindowBitmap.PixelFormat);
                }
            }
            catch
            {
                ImageLostEventProvider.OnImageLost("Image capture failed");
                return null;
            }
        }

        /// <summary>
        /// Captures an image of the entire window specified by the window handle.
        /// </summary>
        /// <param name="hwnd">Handle to the window to capture.</param>
        /// <returns>A Bitmap object of the captured window image or null if capturing failed.</returns>
        private static Bitmap CaptureImage(IntPtr hwnd)
        {
            if (GetWindowRect(hwnd, out Utilities.RECT rc))
            {
                int width = rc.Right - rc.Left;
                int height = rc.Bottom - rc.Top;

                using (var bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb))
                using (var gfxBmp = Graphics.FromImage(bmp))
                {
                    IntPtr hdcBitmap = gfxBmp.GetHdc();
                    try
                    {
                        // Capture the window's content
                        PrintWindow(hwnd, hdcBitmap, PW_RENDERFULLCONTENT);
                    }
                    finally
                    {
                        gfxBmp.ReleaseHdc(hdcBitmap);
                    }

                    return (Bitmap)bmp.Clone();
                }
            }

            return null;
        }

        public static Point FindBrightestAreaCentroid(Bitmap bitmap)
        {
            byte threshold = CalculateThreshold(bitmap, 0.10f); // 10% brightest pixels
            using (Bitmap binaryImage = ApplyThreshold(bitmap, threshold))
            {
                Point centroid = FindBlobCentroid(binaryImage);
                return centroid;
            }
        }

        private static byte CalculateThreshold(Bitmap bitmap, float topPercent)
        {
            int pixelCount = bitmap.Width * bitmap.Height;
            byte[] greenChannelValues = new byte[pixelCount];

            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* ptr = (byte*)bitmapData.Scan0;
                int stride = bitmapData.Stride;

                int bitmapHeight = bitmap.Height;
                int bitmapWidth = bitmap.Width;

                for (int y = 0; y < bitmapHeight; y++)
                {
                    byte* row = ptr + y * stride;

                    for (int x = 0; x < bitmapWidth; x++)
                    {
                        byte greenValue = row[x * 3 + 1];
                        greenChannelValues[y * bitmapWidth + x] = greenValue;
                    }
                }
            }

            bitmap.UnlockBits(bitmapData);

            Array.Sort(greenChannelValues);
            int thresholdIndex = (int)(pixelCount * (1 - topPercent)) - 1;
            return greenChannelValues[thresholdIndex];
        }

        private static Bitmap ApplyThreshold(Bitmap bitmap, byte threshold)
        {
            Bitmap binaryBitmap = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format1bppIndexed);
            BitmapData binaryData = binaryBitmap.LockBits(new Rectangle(0, 0, binaryBitmap.Width, binaryBitmap.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);
            BitmapData colorData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* binaryPtr = (byte*)binaryData.Scan0;
                byte* colorPtr = (byte*)colorData.Scan0;

                int stride = binaryData.Stride;
                int colorStride = colorData.Stride;

                int bitmapHeight = binaryBitmap.Height;
                int bitmapWidth = binaryBitmap.Width;

                for (int y = 0; y < bitmapHeight; y++)
                {
                    byte* binaryRow = binaryPtr + y * stride;
                    byte* colorRow = colorPtr + y * colorStride;

                    for (int x = 0; x < bitmapWidth; x++)
                    {
                        byte greenValue = colorRow[x * 3 + 1];
                        byte bit = (greenValue >= threshold) ? (byte)1 : (byte)0;
                        int bitIndex = x % 8;
                        int byteIndex = x / 8;

                        if (bit == 1)
                        {
                            binaryRow[byteIndex] |= (byte)(1 << (7 - bitIndex));
                        }
                        else
                        {
                            binaryRow[byteIndex] &= (byte)~(1 << (7 - bitIndex));
                        }
                    }
                }
            }

            binaryBitmap.UnlockBits(binaryData);
            bitmap.UnlockBits(colorData);

            return binaryBitmap;
        }

        private static Point FindBlobCentroid(Bitmap binaryImage)
        {
            long sumX = 0, sumY = 0, count = 0;
            int width = binaryImage.Width;
            int height = binaryImage.Height;

            BitmapData bitmapData = binaryImage.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, PixelFormat.Format1bppIndexed);

            unsafe
            {
                byte* ptr = (byte*)bitmapData.Scan0;
                int stride = bitmapData.Stride;

                for (int y = 0; y < height; y++)
                {
                    byte* row = ptr + y * stride;

                    for (int x = 0; x < width; x++)
                    {
                        if ((row[x / 8] & (1 << (7 - (x % 8)))) != 0)
                        {
                            sumX += x;
                            sumY += y;
                            count++;
                        }
                    }
                }
            }

            binaryImage.UnlockBits(bitmapData);

            if (count == 0)
            {
                return new Point(width / 2, height / 2); // Default to center if no bright area is found
            }

            // Calculate centroid
            int centroidX = (int)(sumX / count);
            int centroidY = (int)(sumY / count);

            return new Point(centroidX, centroidY);
        }
    }
}
