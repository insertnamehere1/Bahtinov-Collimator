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
        #region External Methods
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
        #endregion

        #region Events and Delegates
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
        #endregion

        #region Constants
        // Constants for PrintWindow function flags
        private const int PW_RENDERFULLCONTENT = 0x2;
        #endregion

        #region Private Fields
        // Private fields for managing image capture
        private static Timer captureTimer;
        private static Rectangle captureRectangle;
        private static IntPtr targetWindowHandle;
        private static Rectangle selectedStarBox;
        private static string firstHash = "";
        #endregion

        #region Public Fields
        public static int TrackingType {  get; set; } = 0; //0-off, 1-bahtinov, 2-defocus
        #endregion

        #region Methods
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
            firstHash = "";
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

                if (TrackingType == 1) // bahtinov style image
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
                else if (TrackingType == 2) // defocus style image
                {
                    Point offset;

                    try
                    {
                        // Get offset for the inner circle
                        offset = FindInnerCircleOffset(latestImage);
                    }
                    catch (InvalidOperationException ex)
                    {
                        ImageLostEventProvider.OnImageLost("Unable to detect Defocus Image", "Circle Offset", MessageBoxIcon.Warning, MessageBoxButtons.OK);
                        return;
                    }

                    // Update selectedStarBox with new position
                    selectedStarBox.Offset(offset.X, offset.Y);

                    // Apply translation transformation to the Graphics object
                    g.TranslateTransform(-offset.X, -offset.Y);
                }

                // Calculate the center position
                int x = (UITheme.DisplayWindow.X - latestImage.Width) / 2;
                int y = (UITheme.DisplayWindow.Y - latestImage.Height) / 2;

                // Draw the image at the calculated position
                g.DrawImage(latestImage, x, y);
                g.ResetTransform();

                // distribut new image
                ImageReceivedEvent?.Invoke(null, new ImageReceivedEventArgs(updatedImage));
            }
            catch(Exception er)
            {
                if(captureTimer != null)
                    DarkMessageBox.Show("Invalid image selection", "Capture Timer", MessageBoxIcon.Information, MessageBoxButtons.OK);

                return;
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

        /// <summary>
        /// Finds the offset of the inner circle in a donut-shaped image by analyzing brightness transitions along circular paths.
        /// </summary>
        /// <param name="image">The bitmap image to analyze. It is expected to contain a donut-shaped feature with a bright outer ring and a dark inner circle.</param>
        /// <returns>A <see cref="Point"/> representing the estimated center of the inner circle within the image.</returns>
        private static Point FindInnerCircleOffset(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;

            double centerX = width / 2;
            double centerY = height / 2;

            int maxRadius = Math.Min(width, height) / 2;
            int transitionCount = 0;
            double transitionXSum = 0;
            double transitionYSum = 0;

            // Precompute trigonometric values
            double[] cosValues = new double[360];
            double[] sinValues = new double[360];

            for (int i = 0; i < 360; i++)
            {
                cosValues[i] = Math.Cos(i * Math.PI / 180);
                sinValues[i] = Math.Sin(i * Math.PI / 180);
            }

            // Lock the bitmap for faster pixel access
            BitmapData bitmapData = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, image.PixelFormat);
            int bytesPerPixel = Image.GetPixelFormatSize(image.PixelFormat) / 8;
            int stride = bitmapData.Stride;
            IntPtr scan0 = bitmapData.Scan0;
            byte[] pixels = new byte[stride * height];

            System.Runtime.InteropServices.Marshal.Copy(scan0, pixels, 0, pixels.Length);

            for (int angle = 0; angle < 360; angle+=4)
            {
                double lineSumBrightness = 0;
                int numPoints = 0;

                int startRadius = 0;
                int endRadius = maxRadius;
                int step = 1;

                for (int r = startRadius; r < endRadius; r += step)
                {
                    double x = centerX + (r * cosValues[angle]);
                    double y = centerY + (r * sinValues[angle]);

                    if (x >= 0 && x < width && y >= 0 && y < height)
                    {
                        int pixelIndex = (int)(y) * stride + (int)(x) * bytesPerPixel;
                        double brightness = GetBrightness(pixels[pixelIndex + 2], pixels[pixelIndex + 1], pixels[pixelIndex]); // assuming RGB format

                        if (brightness != 0)
                        {
                            lineSumBrightness += brightness;
                            numPoints++;
                        }
                    }
                }

                double averageLineBrightness = numPoints > 0 ? lineSumBrightness / numPoints : 0.0;

                for (int r = startRadius + 10; r < endRadius; r += step)
                {
                    double x = centerX + (r * cosValues[angle]);
                    double y = centerY + (r * sinValues[angle]);

                    if (x >= 0 && x < width && y >= 0 && y < height)
                    {
                        int pixelIndex = (int)(y) * stride + (int)(x) * bytesPerPixel;
                        double brightness = GetBrightness(pixels[pixelIndex + 2], pixels[pixelIndex + 1], pixels[pixelIndex]);

                        if (brightness > averageLineBrightness)
                        {
                            transitionCount++;
                            transitionXSum += x - centerX;
                            transitionYSum += y - centerY;
                            break;
                        }
                    }
                }
            }

            // Unlock the bitmap
            image.UnlockBits(bitmapData);

            if (transitionXSum == 0 || transitionYSum == 0)
                throw new InvalidOperationException("No valid transitions detected in the image.");

            double circleX = transitionXSum / (transitionCount / 2);
            double circleY = transitionYSum / (transitionCount / 2);

            return new Point((int)Math.Round(circleX), (int)Math.Round(circleY));
        }

        /// <summary>
        /// Calculates the brightness of a pixel based on its RGB color values.
        /// </summary>
        /// <param name="r">The red component of the pixel's color.</param>
        /// <param name="g">The green component of the pixel's color.</param>
        /// <param name="b">The blue component of the pixel's color.</param>
        /// <returns>A <see cref="double"/> representing the brightness of the pixel, normalized to the range [0.0, 1.0].</returns>
        private static double GetBrightness(byte r, byte g, byte b)
        {
            return (r * 0.299 + g * 0.587 + b * 0.114) / 255.0;
        }

        /// <summary>
        /// Creates a circular bitmap from the given original image, using the smaller dimension of the selection area as the diameter.
        /// </summary>
        /// <param name="originalImage">The original bitmap image to be cropped into a circular shape.</param>
        /// <returns>A <see cref="Bitmap"/> representing the original image cropped into a circular shape. Returns null if the selection area is too small.</returns>
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
                        DarkMessageBox.Show("No application found under the selected area", "Image Capture", MessageBoxIcon.Error, MessageBoxButtons.OK);
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
        /// Captures a portion of the target window's image based on the specified selection area and returns it as a <see cref="Bitmap"/>.
        /// Adjusts the selection area to ensure it fits within the window and adheres to UI constraints. If any issues occur during image capture, returns null and shows a warning message.
        /// </summary>
        /// <returns>A <see cref="Bitmap"/> representing the selected area of the target window. Returns null if there are issues with image capture or if the selection area is invalid.</returns>
        private static Bitmap GetImage()
        {
            // Get the window rectangle
            if (!GetWindowRect(targetWindowHandle, out Utilities.RECT rect))
            {
                ImageLostEventProvider.OnImageLost("Image has been lost", "GetImage", MessageBoxIcon.Warning, MessageBoxButtons.OK);
                return null;
            }

            // Convert window dimensions to logical pixels
            int windowWidth = (int)((rect.Right - rect.Left));
            int windowHeight = (int)((rect.Bottom - rect.Top));

            // Adjust selectedStarBox to be within the bounds of the window
            selectedStarBox.X = Math.Max(selectedStarBox.X, 0);
            selectedStarBox.Y = Math.Max(selectedStarBox.Y , 0);
            selectedStarBox.Width = Math.Min(selectedStarBox.Width, windowWidth - selectedStarBox.X);
            selectedStarBox.Height = Math.Min(selectedStarBox.Height, windowHeight - selectedStarBox.Y);

            // Check if selectedStarBox exceeds UITheme.DisplayWindow.X and Y, and adjust if necessary
            if (selectedStarBox.Width > UITheme.DisplayWindow.X || selectedStarBox.Height > UITheme.DisplayWindow.Y)
            {
                // Calculate the excess width and height
                int excessWidth = selectedStarBox.Width - UITheme.DisplayWindow.X;
                int excessHeight = selectedStarBox.Height - UITheme.DisplayWindow.Y;

                // Adjust equally on all sides
                selectedStarBox.X += excessWidth / 2;
                selectedStarBox.Y += excessHeight / 2;
                selectedStarBox.Width -= excessWidth;
                selectedStarBox.Height -= excessHeight;
            }

            // Ensure the selected rectangle is valid
            if (selectedStarBox.Width <= 0 || selectedStarBox.Height <= 0)
            {
                ImageLostEventProvider.OnImageLost("The selection area is too small.", "GetImage", MessageBoxIcon.Warning, MessageBoxButtons.OK);
                return null;
            }

            try
            {
                using (var fullWindowBitmap = CaptureImage(targetWindowHandle))
                {
                    if (fullWindowBitmap == null)
                    {
                        ImageLostEventProvider.OnImageLost("Unable to capture image", "GetImage", MessageBoxIcon.Warning, MessageBoxButtons.OK);
                        return null;
                    }

                    return fullWindowBitmap.Clone(selectedStarBox, fullWindowBitmap.PixelFormat);
                }
            }
            catch
            {
                ImageLostEventProvider.OnImageLost("Image capture failed", "GetImage", MessageBoxIcon.Warning, MessageBoxButtons.OK);
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

            DarkMessageBox.Show("Unable to capture window image", "Capture Image", MessageBoxIcon.Exclamation, MessageBoxButtons.OK);
            return null;
        }

        /// <summary>
        /// Finds the centroid of the brightest area in a bitmap image. The method first calculates a brightness threshold that includes the top 10% of the brightest pixels. It then creates a binary image based on this threshold and determines the centroid of the brightest area from the binary image.
        /// </summary>
        /// <param name="bitmap">The <see cref="Bitmap"/> image from which to find the brightest area.</param>
        /// <returns>A <see cref="Point"/> representing the centroid of the brightest area in the image.</returns>
        public static Point FindBrightestAreaCentroid(Bitmap bitmap)
        {
            byte threshold = CalculateThreshold(bitmap, 0.10f); // 10% brightest pixels
            using (Bitmap binaryImage = ApplyThreshold(bitmap, threshold))
            {
                Point centroid = FindBlobCentroid(binaryImage);
                return centroid;
            }
        }

        /// <summary>
        /// Calculates the brightness threshold for the top percentage of brightest pixels in a bitmap image. The method extracts green channel values from the image, sorts them, and determines the threshold value to include the specified percentage of the brightest pixels.
        /// </summary>
        /// <param name="bitmap">The <see cref="Bitmap"/> image from which to calculate the threshold.</param>
        /// <param name="topPercent">The percentage of the brightest pixels to include in the threshold calculation (e.g., 0.10 for the top 10%).</param>
        /// <returns>A <see cref="byte"/> representing the calculated brightness threshold.</returns>
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

        /// <summary>
        /// Converts a color bitmap to a binary bitmap based on a brightness threshold. Pixels with a green channel value greater than or equal to the specified threshold are set to white; all other pixels are set to black.
        /// </summary>
        /// <param name="bitmap">The <see cref="Bitmap"/> image to be thresholded.</param>
        /// <param name="threshold">The brightness threshold value. Pixels with a green channel value greater than or equal to this threshold are set to white in the resulting binary image.</param>
        /// <returns>A <see cref="Bitmap"/> in 1-bit per pixel format where pixels are either black or white based on the threshold.</returns>
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

        /// <summary>
        /// Calculates the centroid of the white areas in a binary bitmap. The centroid is determined by averaging the coordinates of all white pixels. If no white pixels are found, the method returns the center of the image.
        /// </summary>
        /// <param name="binaryImage">The <see cref="Bitmap"/> image in 1-bit per pixel format where white pixels are considered as part of the blob.</param>
        /// <returns>A <see cref="Point"/> representing the centroid of the white areas in the binary image. If no white pixels are found, returns the center of the image.</returns>
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
            double centroidX = sumX / count;
            double centroidY = sumY / count;

            return new Point((int)Math.Round(centroidX), (int)Math.Round(centroidY));
        }
        #endregion
    }
}
