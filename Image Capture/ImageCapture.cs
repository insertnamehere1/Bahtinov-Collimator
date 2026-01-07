using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Speech.Synthesis.TtsEngine;
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

        // Threshold for detecting significant movement
        private const int MovementThreshold = 1;
        #endregion

        #region Private Fields
        // Private fields for managing image capture
        private static Timer captureTimer;
        private static IntPtr targetWindowHandle;
        private static Rectangle selectedStarBox;
        private static string firstHash = "";
        private static bool notEqualLastTick = true;
        private static bool newImageSend = false;
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

        public static void ForceImageUpdate()
        {
            // forces updated image on next tick
            firstHash = "";
        }

        ///// <summary>
        ///// Handles the Timer.Tick event to capture an image and invoke the ImageReceivedEvent.
        ///// </summary>
        ///// <param name="sender">The source of the event.</param>
        ///// <param name="e">Event arguments for the tick event.</param>
        private static void CaptureTimer_Tick(object sender, EventArgs e)
        {
            Bitmap image = GetImage();
            if (image == null)
                return;

            // Hash check for duplicate frames
            string secondHash = Utilities.ComputeHash(image);
            bool isSameImage = firstHash.Equals(secondHash);

            if (isSameImage)
            {
                if (notEqualLastTick)
                {
                    newImageSend = true;
                    notEqualLastTick = false;
                }
                else
                {
                    return;
                }
            }
            else
            {
                firstHash = secondHash;
                notEqualLastTick = true;
            }

            Bitmap latestImage = null;
            Bitmap updatedImage = null;
            Graphics g = null;

            try
            {
                latestImage = CreateCircularImage(image);
                updatedImage = new Bitmap(UITheme.DisplayWindow.X, UITheme.DisplayWindow.Y);

                g = Graphics.FromImage(updatedImage);
                g.Clear(Color.Black);

                // Compute movement offset
                Point offset = Point.Empty;

                if (TrackingType == 1)        // bahtinov
                {
                    offset = FindBrightestAreaCentroid(latestImage);

                    if (Math.Abs(offset.X) > MovementThreshold || Math.Abs(offset.Y) > MovementThreshold)
                        captureTimer.Interval = 10;

                    int dx = (latestImage.Width / 2) - offset.X;
                    int dy = (latestImage.Height / 2) - offset.Y;

                    selectedStarBox.Offset(-dx, -dy);
                    g.TranslateTransform(dx, dy);
                }
                else if (TrackingType == 2)   // defocus
                {
                    try
                    {
                        offset = FindInnerCircleOffset(latestImage);
                    }
                    catch (InvalidOperationException)
                    {
                        ImageLostEventProvider.OnImageLost("Unable to detect Defocus Image", "Circle Offset", MessageBoxIcon.Warning, MessageBoxButtons.OK);
                        return;
                    }

                    if (Math.Abs(offset.X) > MovementThreshold || Math.Abs(offset.Y) > MovementThreshold)
                        captureTimer.Interval = 10;

                    selectedStarBox.Offset(offset.X, offset.Y);
                    g.TranslateTransform(-offset.X, -offset.Y);
                }

                // Draw centered image
                int x = (UITheme.DisplayWindow.X - latestImage.Width) / 2;
                int y = (UITheme.DisplayWindow.Y - latestImage.Height) / 2;

                g.DrawImage(latestImage, x, y);
                g.ResetTransform();

                // Only send when next frame is stable
                if (newImageSend)
                {
                    ImageReceivedEvent?.Invoke(null, new ImageReceivedEventArgs(updatedImage));
                    newImageSend = false;
                    captureTimer.Interval = 200;
                }
            }
            catch
            {
                if (captureTimer != null)
                {
                    DarkMessageBox.Show("Invalid image selection", "Capture Timer", MessageBoxIcon.Information, MessageBoxButtons.OK);
                }
                return;
            }
            finally
            {
                g?.Dispose();
                latestImage?.Dispose();
                image?.Dispose();
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
        /// Finds the centroid of the brightest compact region in a bitmap, robust to
        /// secondary fainter stars. It:
        ///  1) Converts the image to 8-bit luminance (no thresholding).
        ///  2) Builds an integral image (summed-area table).
        ///  3) Slides a fixed-size square window to locate the maximum summed luminance.
        ///  4) Refines the position with an intensity-weighted centroid in a small radius.
        /// Returns image coordinates suitable for centering the Bahtinov pattern.
        /// </summary>
        /// <param name="bitmap">The source bitmap (any common 24/32bpp format).</param>
        /// <returns>Centroid of the brightest region. Falls back to image center if it cannot compute.</returns>
        public static Point FindBrightestAreaCentroid(Bitmap bitmap)
        {
            if (bitmap == null) return new Point(0, 0);

            int width = bitmap.Width;
            int height = bitmap.Height;
            if (width < 8 || height < 8)
                return new Point(0, 0);

            PixelFormat pf = bitmap.PixelFormat;
            bool supported = pf == PixelFormat.Format24bppRgb || pf == PixelFormat.Format32bppArgb ||
                             pf == PixelFormat.Format32bppRgb || pf == PixelFormat.Format32bppPArgb;

            Bitmap src = supported ? bitmap : bitmap.Clone(
                new Rectangle(0, 0, width, height), PixelFormat.Format24bppRgb);

            BitmapData bd = null;
            byte[] lum = new byte[width * height];
            try
            {
                bd = src.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, src.PixelFormat);
                int stride = bd.Stride;
                int bpp = Image.GetPixelFormatSize(src.PixelFormat) / 8;
                unsafe
                {
                    byte* basePtr = (byte*)bd.Scan0;
                    int idx = 0;
                    for (int y = 0; y < height; y++)
                    {
                        byte* row = basePtr + y * stride;
                        for (int x = 0; x < width; x++)
                        {
                            byte b = row[x * bpp + 0];
                            byte g = row[x * bpp + 1];
                            byte r1 = row[x * bpp + 2];
                            int y8 = (int)(0.299 * r1 + 0.587 * g + 0.114 * b + 0.5);
                            if (y8 > 255) y8 = 255;
                            lum[idx++] = (byte)y8;
                        }
                    }
                }
            }
            finally
            {
                if (bd != null) src.UnlockBits(bd);
                if (!ReferenceEquals(src, bitmap)) src.Dispose();
            }

            // Quick test: if average brightness is low, no meaningful centroid
            double avg = lum.Average(b => (double)b);
            if (avg < 5.0)  // all dark
                return new Point(0, 0);

            // Build integral image
            int W1 = width + 1;
            int H1 = height + 1;
            int[] ii = new int[W1 * H1];

            for (int y = 1; y < H1; y++)
            {
                int rowSum = 0;
                int offLum = (y - 1) * width;
                int offII = y * W1;
                for (int x = 1; x < W1; x++)
                {
                    rowSum += lum[offLum + (x - 1)];
                    ii[offII + x] = ii[(y - 1) * W1 + x] + rowSum;
                }
            }

            int win = Math.Max(9, Math.Min(width, height) / 16);
            if ((win & 1) == 0) win++;
            int r = win / 2;

            int bestX = 0, bestY = 0;
            int bestSum = -1;

            for (int y = r; y < height - r; y++)
            {
                int y0 = y - r;
                int y1 = y + r + 1;
                int rowTop = y0 * W1;
                int rowBot = y1 * W1;

                for (int x = r; x < width - r; x++)
                {
                    int x0 = x - r;
                    int x1 = x + r + 1;

                    int sum = ii[rowBot + x1] - ii[rowBot + x0] - ii[rowTop + x1] + ii[rowTop + x0];
                    if (sum > bestSum)
                    {
                        bestSum = sum;
                        bestX = x;
                        bestY = y;
                    }
                }
            }

            if (bestSum < 10000)  // nothing bright enough, adjust threshold as needed
                return new Point(0, 0);

            // Refine with intensity-weighted centroid
            int refineRadius = Math.Max(3, win / 3);
            long wSum = 0, wX = 0, wY = 0;

            int xStart = Math.Max(0, bestX - refineRadius);
            int xEnd = Math.Min(width - 1, bestX + refineRadius);
            int yStart = Math.Max(0, bestY - refineRadius);
            int yEnd = Math.Min(height - 1, bestY + refineRadius);

            for (int y = yStart; y <= yEnd; y++)
            {
                int baseIdx = y * width;
                for (int x = xStart; x <= xEnd; x++)
                {
                    int w = lum[baseIdx + x];
                    if (w > 0) { wSum += w; wX += (long)w * x; wY += (long)w * y; }
                }
            }

            if (wSum == 0)
                return new Point(0, 0);

            double cx = (double)wX / wSum;
            double cy = (double)wY / wSum;

            return new Point((int)Math.Round(cx), (int)Math.Round(cy));
        }
        #endregion
    }
}
