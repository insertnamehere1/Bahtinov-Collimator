using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
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
        /// Captures a bitmap image of the specified window and renders it into the provided device context.
        /// </summary>
        /// <param name="hwnd">Handle to the window to capture.</param>
        /// <param name="hdcBlt">Handle to the device context to which the image is copied.</param>
        /// <param name="nFlags">Flags specifying how to capture the window.</param>
        /// <returns>True if the function succeeds; otherwise, false.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, int nFlags);

        /// <summary>
        /// Retrieves the handle to the window that contains the specified point.
        /// </summary>
        /// <param name="p">The point in screen coordinates.</param>
        /// <returns>Handle to the window that contains the point.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr WindowFromPoint(Point p);

        /// <summary>
        /// Retrieves an ancestor of the specified window.
        /// Used to resolve child/overlay windows to a stable top-level window handle.
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);

        /// <summary>
        /// Determines whether the specified window is minimized (iconic).
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsIconic(IntPtr hWnd);

        /// <summary>
        /// Determines whether the specified window handle identifies an existing window.
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindow(IntPtr hWnd);

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

        // GetAncestor flags
        private const uint GA_ROOT = 2;

        #endregion

        #region Private Fields

        private static Timer captureTimer;
        private static IntPtr targetWindowHandle;
        private static Rectangle selectedStarBox;
        private static string firstHash = "";
        private static bool notEqualLastTick = true;
        private static bool newImageSend = false;

        #endregion

        #region Public Fields

        public static int TrackingType { get; set; } = 0; //0-off, 1-bahtinov, 2-defocus

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
        /// Forces an updated image to be sent on the next timer tick.
        /// </summary>
        public static void ForceImageUpdate()
        {
            // Forces updated image on next tick
            firstHash = "";
        }

        /// <summary>
        /// Resolves an arbitrary window handle (often a child/overlay) to its top-level root window.
        /// This reduces cases where WindowFromPoint returns a transient child window that is not suitable for capture.
        /// </summary>
        /// <param name="hwnd">A window handle returned from WindowFromPoint.</param>
        /// <returns>The top-level root window handle, or IntPtr.Zero if invalid.</returns>
        private static IntPtr GetRootWindow(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
                return IntPtr.Zero;

            if (!IsWindow(hwnd))
                return IntPtr.Zero;

            IntPtr root = GetAncestor(hwnd, GA_ROOT);
            if (root != IntPtr.Zero && IsWindow(root))
                return root;

            return hwnd;
        }

        /// <summary>
        /// Handles the Timer.Tick event to capture an image and invoke the ImageReceivedEvent.
        /// </summary>
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
                    image.Dispose();
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
                if (latestImage == null)
                    return;

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
                else
                {
                    updatedImage.Dispose();
                }
            }
            catch
            {
                if (captureTimer != null)
                {
                    DarkMessageBox.Show("Invalid image selection", "Capture Timer", MessageBoxIcon.Information, MessageBoxButtons.OK);
                }
                updatedImage?.Dispose();
                return;
            }
            finally
            {
                g?.Dispose();
                latestImage?.Dispose();
                image.Dispose();
            }
        }

        /// <summary>
        /// Finds the offset of the inner circle in a donut-shaped image by analyzing brightness transitions along circular paths.
        /// </summary>
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

            Marshal.Copy(scan0, pixels, 0, pixels.Length);

            for (int angle = 0; angle < 360; angle += 4)
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
                        double brightness = GetBrightness(pixels[pixelIndex + 2], pixels[pixelIndex + 1], pixels[pixelIndex]); // RGB

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
        private static double GetBrightness(byte r, byte g, byte b)
        {
            return (r * 0.299 + g * 0.587 + b * 0.114) / 255.0;
        }

        /// <summary>
        /// Creates a circular bitmap from the given original image, using the smaller dimension of the selection area as the diameter.
        /// </summary>
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

                    // WindowFromPoint can return a child/overlay window, so resolve to a stable root window.
                    IntPtr hit = WindowFromPoint(new Point(
                        selectedStarBox.Left + selectedStarBox.Width / 2,
                        selectedStarBox.Top + selectedStarBox.Height / 2
                    ));

                    targetWindowHandle = GetRootWindow(hit);

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
        /// Adjusts the selection area to ensure it fits within the window and adheres to UI constraints.
        /// </summary>
        private static Bitmap GetImage()
        {
            // Validate target window handle early
            if (targetWindowHandle == IntPtr.Zero || !IsWindow(targetWindowHandle))
            {
                ImageLostEventProvider.OnImageLost("Image has been lost", "GetImage", MessageBoxIcon.Warning, MessageBoxButtons.OK);
                return null;
            }

            // If the window is minimized, PrintWindow is often unreliable and may return a stale/blank image.
            // Treat this as "image lost" so the UI can instruct the user.
            if (IsIconic(targetWindowHandle))
            {
                ImageLostEventProvider.OnImageLost("Target window is minimized", "GetImage", MessageBoxIcon.Warning, MessageBoxButtons.OK);
                return null;
            }

            // Get the window rectangle
            if (!GetWindowRect(targetWindowHandle, out Utilities.RECT rect))
            {
                ImageLostEventProvider.OnImageLost("Image has been lost", "GetImage", MessageBoxIcon.Warning, MessageBoxButtons.OK);
                return null;
            }

            int windowWidth = (int)(rect.Right - rect.Left);
            int windowHeight = (int)(rect.Bottom - rect.Top);

            // Adjust selectedStarBox to be within the bounds of the window
            selectedStarBox.X = Math.Max(selectedStarBox.X, 0);
            selectedStarBox.Y = Math.Max(selectedStarBox.Y, 0);
            selectedStarBox.Width = Math.Min(selectedStarBox.Width, windowWidth - selectedStarBox.X);
            selectedStarBox.Height = Math.Min(selectedStarBox.Height, windowHeight - selectedStarBox.Y);

            // Enforce UITheme display constraints
            if (selectedStarBox.Width > UITheme.DisplayWindow.X || selectedStarBox.Height > UITheme.DisplayWindow.Y)
            {
                int excessWidth = selectedStarBox.Width - UITheme.DisplayWindow.X;
                int excessHeight = selectedStarBox.Height - UITheme.DisplayWindow.Y;

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
        /// Uses PrintWindow and validates the return value to avoid returning an empty bitmap when capture fails.
        /// </summary>
        /// <param name="hwnd">Handle to the window to capture.</param>
        /// <returns>A Bitmap object of the captured window image or null if capturing failed.</returns>
        private static Bitmap CaptureImage(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero || !IsWindow(hwnd))
                return null;

            if (!GetWindowRect(hwnd, out Utilities.RECT rc))
                return null;

            int width = rc.Right - rc.Left;
            int height = rc.Bottom - rc.Top;

            if (width <= 0 || height <= 0)
                return null;

            Bitmap bmp = null;
            Graphics gfxBmp = null;

            try
            {
                bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                gfxBmp = Graphics.FromImage(bmp);

                IntPtr hdcBitmap = gfxBmp.GetHdc();
                bool ok;
                try
                {
                    ok = PrintWindow(hwnd, hdcBitmap, PW_RENDERFULLCONTENT);
                    if (!ok)
                    {
                        // Some windows respond better without PW_RENDERFULLCONTENT.
                        ok = PrintWindow(hwnd, hdcBitmap, 0);
                    }
                }
                finally
                {
                    gfxBmp.ReleaseHdc(hdcBitmap);
                }

                if (!ok)
                {
                    bmp.Dispose();
                    return null;
                }

                // Return a detached copy so the caller can dispose safely
                return (Bitmap)bmp.Clone();
            }
            catch
            {
                bmp?.Dispose();
                return null;
            }
            finally
            {
                gfxBmp?.Dispose();
                bmp?.Dispose();
            }
        }

        /// <summary>
        /// Finds the centroid of the brightest compact region in a bitmap, robust to
        /// secondary fainter stars.
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
                int srcRow = (y - 1) * width;
                int iiRow = y * W1;
                int iiPrevRow = (y - 1) * W1;

                for (int x = 1; x < W1; x++)
                {
                    rowSum += lum[srcRow + (x - 1)];
                    ii[iiRow + x] = ii[iiPrevRow + x] + rowSum;
                }
            }

            // Sliding window search
            int window = Math.Max(12, Math.Min(width, height) / 8);
            int bestX = width / 2;
            int bestY = height / 2;
            int bestSum = int.MinValue;

            int maxX = width - window;
            int maxY = height - window;

            for (int y = 0; y <= maxY; y++)
            {
                int y1 = y;
                int y2 = y + window;

                int iiY1 = y1 * W1;
                int iiY2 = y2 * W1;

                for (int x = 0; x <= maxX; x++)
                {
                    int x1 = x;
                    int x2 = x + window;

                    int sum = ii[iiY2 + x2] - ii[iiY2 + x1] - ii[iiY1 + x2] + ii[iiY1 + x1];
                    if (sum > bestSum)
                    {
                        bestSum = sum;
                        bestX = x + window / 2;
                        bestY = y + window / 2;
                    }
                }
            }

            // Refine with intensity-weighted centroid around best
            int radius = Math.Max(6, window / 2);
            int xMin = Math.Max(0, bestX - radius);
            int xMax2 = Math.Min(width - 1, bestX + radius);
            int yMin = Math.Max(0, bestY - radius);
            int yMax2 = Math.Min(height - 1, bestY + radius);

            double sumW = 0, sumX = 0, sumY = 0;
            for (int y = yMin; y <= yMax2; y++)
            {
                int row = y * width;
                for (int x = xMin; x <= xMax2; x++)
                {
                    byte v = lum[row + x];
                    if (v == 0) continue;
                    double w = v;
                    sumW += w;
                    sumX += w * x;
                    sumY += w * y;
                }
            }

            if (sumW <= 0.0001)
                return new Point(width / 2, height / 2);

            int cx = (int)Math.Round(sumX / sumW);
            int cy = (int)Math.Round(sumY / sumW);

            // Clamp
            if (cx < 0) cx = 0; else if (cx >= width) cx = width - 1;
            if (cy < 0) cy = 0; else if (cy >= height) cy = height - 1;

            return new Point(cx, cy);
        }

        #endregion
    }
}
