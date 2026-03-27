using Bahtinov_Collimator.Image_Processing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using static Bahtinov_Collimator.BahtinovLineDataEventArgs;
using Bahtinov_Collimator.Helper;


namespace Bahtinov_Collimator
{
    /// <summary>
    /// Displays the incoming capture frame and overlays defocus circles or Bahtinov line analysis.
    /// Uses layered bitmaps so overlays can be redrawn efficiently after resize or DPI changes.
    /// </summary>
    public partial class ImageDisplayComponent : UserControl
    {
        #region Fields
        // List to hold multiple layers of Bitmap images
        readonly private List<Bitmap> layers;

        // Index to track the currently selected group
        private int selectedGroup = 0;

        // scalable font for the displayed error values
        private Font errorFont;

        // Track current layer bitmap size so we can recreate on DPI/resize.
        private Size layerSize = Size.Empty;

        // The coordinate-space of the incoming image/overlay data (typically 600x600).
        private int sourceWidth = 600;
        private int sourceHeight = 600;

        private enum OverlayMode
        {
            None,
            Defocus,
            Bahtinov
        }

        private OverlayMode cachedOverlayMode = OverlayMode.None;
        private Bitmap cachedSourceImage;
        private PointD cachedInnerCentre;
        private PointD cachedOuterCentre;
        private double cachedInnerRadius;
        private double cachedOuterRadius;
        private BahtinovLineDataEventArgs.BahtinovLineData cachedBahtinovData;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageDisplayComponent"/> class.
        /// </summary>
        public ImageDisplayComponent()
        {
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();

            InitializeComponent();
            SetupPictureBox();

            this.layers = new List<Bitmap>();
            AddNewLayers(4);

            SetupSubscriptions();
        }
        #endregion

        #region Setup Methods
        /// <summary>
        /// Sets up event subscriptions for various components and events.
        /// </summary>
        private void SetupSubscriptions()
        {
            // Subscribe to the ImageReceivedEvent
            FocusChannelComponent.ChannelSelectDataEvent += ChannelSelected;
            BahtinovProcessing.BahtinovLineDrawEvent += OnBahtinovLineReceive;
            ImageLostEventProvider.ImageLostEvent += HandleImageLost;
            DefocusStarProcessing.DefocusCircleEvent += OnDefocusCirceReceived;

            this.pictureBox1.Paint += new PaintEventHandler(this.PictureBoxPaint);
            this.pictureBox1.Resize += new EventHandler(this.PictureBoxResized);
        }

        /// <summary>
        /// Configures the PictureBox settings, such as background color.
        /// </summary>
        private void SetupPictureBox()
        {
            pictureBox1.BackColor = UITheme.DisplayBackgroundColor;
            pictureBox1.CornerRadius = 10;
        }

        /// <summary>
        /// Handles the form's Load event and adjusts the font size based on the current display's DPI.
        /// This ensures that the font maintains a consistent physical size, regardless of the system's
        /// scaling settings. It retrieves the current DPI, calculates the appropriate font size by
        /// comparing it to the standard DPI (96), and then applies the scaled font to the form.
        /// </summary>
        /// <param name="e">Event data for the Load event.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            UpdateErrorFontForCurrentDpi();
            EnsureLayerSizeMatchesPictureBox(forceRecreate: true);
        }

        /// <summary>
        /// WinForms per-monitor DPI: called when the parent DPI changes.
        /// We need to recreate our backing bitmaps and invalidate so the image scales correctly.
        /// </summary>
        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);
            UpdateErrorFontForCurrentDpi();
            EnsureLayerSizeMatchesPictureBox(forceRecreate: true);
            RedrawFromCache();
        }

        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles the event when an image is lost. Clears the display.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void HandleImageLost(object sender, ImageLostEventArgs e)
        {
            ClearDisplay();
        }

        /// <summary>
        /// Handles the event when a defocus circle is received. Updates the PictureBox with the new image and circle data.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments containing the image and circle data.</param>
        private void OnDefocusCirceReceived(object sender, DefocusCircleEventArgs e)
        {
            Bitmap image = new Bitmap(e.Image);

            if (pictureBox1.InvokeRequired)
                pictureBox1.Invoke(new Action(() => UpdatePictureBox(image, e.InnerCircleCentre, e.InnerCircleRadius, e.OuterCircleCentre, e.OuterCircleRadius)));
            else
                UpdatePictureBox(image, e.InnerCircleCentre, e.InnerCircleRadius, e.OuterCircleCentre, e.OuterCircleRadius);
        }

        /// <summary>
        /// Handles the event when Bahtinov line data is received. Updates the PictureBox with the new image and line data.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments containing the image and line data.</param>
        private void OnBahtinovLineReceive(object sender, BahtinovLineDataEventArgs e)
        {
            Bitmap image = new Bitmap(e.Image);

            if (pictureBox1.InvokeRequired)
                pictureBox1.Invoke(new Action(() => UpdatePictureBox(image, e.Linedata)));
            else
                UpdatePictureBox(image, e.Linedata);
        }

        /// <summary>
        /// Handles the event when the channel selection changes. Updates the selected group index and refreshes the PictureBox.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments containing the channel selection data.</param>
        private void ChannelSelected(object sender, ChannelSelectEventArgs e)
        {
            selectedGroup = 0; // Default value

            for (int i = 0; i < e.ChannelSelected.Length; i++)
            {
                if (e.ChannelSelected[i])
                {
                    selectedGroup = i + 1;
                    break;
                }
            }

            pictureBox1.Invalidate();
        }
        #endregion

        #region Update Methods
        /// <summary>
        /// Updates the cached display frame and defocus overlay geometry, then redraws all layers.
        /// </summary>
        /// <param name="image">The Bitmap image to display.</param>
        /// <param name="innerCentre">The center point of the inner defocus circle.</param>
        /// <param name="innerRadius">The radius of the inner defocus circle.</param>
        /// <param name="outerCentre">The center point of the outer defocus circle.</param>
        /// <param name="outerRadius">The radius of the outer defocus circle.</param>
        private void UpdatePictureBox(Bitmap image, PointD innerCentre, double innerRadius, PointD outerCentre, double outerRadius)
        {
            CacheSourceImage(image);
            cachedOverlayMode = OverlayMode.Defocus;
            cachedInnerCentre = innerCentre;
            cachedInnerRadius = innerRadius;
            cachedOuterCentre = outerCentre;
            cachedOuterRadius = outerRadius;
            cachedBahtinovData = null;

            RedrawFromCache();
            image.Dispose();
        }

        /// <summary>
        /// Updates the PictureBox with the provided image and drawing data, including bahtinov line data and error values.
        /// </summary>
        /// <param name="image">The Bitmap image to display.</param>
        /// <param name="data">The Bahtinov line information and the bahtinov error values.</param>
        private void UpdatePictureBox(Bitmap image, BahtinovLineDataEventArgs.BahtinovLineData data)
        {
            CacheSourceImage(image);
            cachedOverlayMode = OverlayMode.Bahtinov;
            cachedBahtinovData = CloneBahtinovData(data);

            RedrawFromCache();
            image.Dispose();
        }
        #endregion

        #region Drawing Methods

        /// <summary>
        /// Draws inner (green) and outer (red) defocus circles, plus 20x20px center crosses.
        /// Crosses are semi-transparent so overlapping areas blend (red+green -> yellow).
        /// </summary>
        private void DrawDefocusCircles(PointD innerCentre, double innerRadius, PointD outerCentre, double outerRadius)
        {
            using (var g = Graphics.FromImage(layers[1]))
            {
                // Make alpha blending predictable and nice-looking.
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.CompositingMode = CompositingMode.SourceOver;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                ApplyContentTransform(g);

                float layerCentreX = sourceWidth / 2f;
                float layerCentreY = sourceHeight / 2f;
                float overlayScale = GetOverlayScale();

                // Convert circle centres into layer pixel coordinates once.
                var innerPx = new PointF(layerCentreX + (float)innerCentre.X, layerCentreY + (float)innerCentre.Y);
                var outerPx = new PointF(layerCentreX + (float)outerCentre.X, layerCentreY + (float)outerCentre.Y);

                float penWidth = 3f * overlayScale;
                float crossSize = 20f * overlayScale;  // overall width/height of cross
                const int crossAlpha = 200;   // 0..255. Lower = more blending.

                using (var innerPen = new Pen(Color.FromArgb(crossAlpha, UITheme.DefocusInnerCircleColor), penWidth))
                using (var outerPen = new Pen(Color.FromArgb(crossAlpha, UITheme.DefocusOuterCircleColor), penWidth))
                using (var outerDashPen = new Pen(UITheme.DefocusOuterCircleColor, penWidth)
                {
                    DashStyle = System.Drawing.Drawing2D.DashStyle.Dash
                })
                using (var innerDashPen = new Pen(UITheme.DefocusInnerCircleColor, penWidth)
                {
                    DashStyle = System.Drawing.Drawing2D.DashStyle.Dash
                })
                {
                    // Circles (same pens for consistent look)
                    DrawCircle(g, innerDashPen, innerPx, (float)innerRadius);
                    DrawCircle(g, outerDashPen, outerPx, (float)outerRadius);

                    // Crosses (semi-transparent so overlaps blend instead of replace)
                    DrawCross(g, outerPen, outerPx, crossSize);
                    DrawCross(g, innerPen, innerPx, crossSize);
                }
            }
        }

        private static void DrawCircle(Graphics g, Pen pen, PointF centre, float radius)
        {
            float d = radius * 2f;
            g.DrawEllipse(pen, centre.X - radius, centre.Y - radius, d, d);
        }

        private static void DrawCross(Graphics g, Pen pen, PointF centre, float size)
        {
            float half = size / 1.5f;

            // Horizontal
            g.DrawLine(pen, centre.X - half, centre.Y, centre.X + half, centre.Y);
            // Vertical
            g.DrawLine(pen, centre.X, centre.Y - half, centre.X, centre.Y + half);
        }

        /// <summary>
        /// Draws the provided image on the first layer of the PictureBox.
        /// </summary>
        /// <param name="image">The Bitmap image to draw.</param>
        private void DrawImageOnFirstLayer(Bitmap image)
        {
            if (image == null)
                return;

            sourceWidth = image.Width;
            sourceHeight = image.Height;

            using (var g = Graphics.FromImage(layers[0]))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                // Draw the source bitmap scaled to fit inside the component, preserving aspect ratio.
                ApplyContentTransform(g);
                g.DrawImage(image, new Rectangle(0, 0, sourceWidth, sourceHeight));
            }
        }

        /// <summary>
        /// Draws a group of lines on the PictureBox based on the provided line group data.
        /// </summary>
        /// <param name="group">The group line data for the bahtinov lines.</param>
        /// <param name="clippingRegion">The clipping circle.</param> 
        private void DrawGroupLines(BahtinovLineDataEventArgs.LineGroup group, GraphicsPath clippingRegion)
        {
            using (var g = Graphics.FromImage(layers[group.GroupId + 1]))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                // Draw in source coordinates, scaled/centered to fit the current control size.
                ApplyContentTransform(g);
                // Clip path is defined in source coordinates; with the transform set above,
                // GDI+ will apply the same transform to the clip region.
                g.SetClip(clippingRegion);

                const int alpha = 150;   // 0–255  lower = more transparent, higher = more opaque

                foreach (var line in group.Lines)
                {
                    var pen = UITheme.GetDisplayLinePen(group.GroupId, line.LineId);

                    // Modify opacity
                    var c = pen.Color;
                    pen.Color = Color.FromArgb(alpha, c.R, c.G, c.B);

                    g.DrawLine(pen, line.Start, line.End);
                }

                g.ResetClip();

                DrawErrorCircleAndLine(g, group);
                DrawErrorValueText(g, group);
            }
        }

        /// <summary>
        /// Draws an error circle and an error line on the provided graphics object.
        /// </summary>
        /// <param name="g">The <see cref="Graphics"/> object used for drawing.</param>
        /// <param name="group">An instance of <see cref="BahtinovLineDataEventArgs.LineGroup"/> containing the data for the error circle and line.</param>
        private void DrawErrorCircleAndLine(Graphics g, BahtinovLineDataEventArgs.LineGroup group)
        {
            var errorPen = UITheme.GetErrorCirclePen(group.GroupId);
            float overlayScale = GetOverlayScale();

            // Preserve center position but scale marker geometry with source image dimensions.
            float baseCircleRadius = Math.Max(1f, group.ErrorCircle.Width / 2f);
            float scaledCircleRadius = baseCircleRadius * overlayScale;
            float centerX = group.ErrorCircle.Origin.X + baseCircleRadius;
            float centerY = group.ErrorCircle.Origin.Y + baseCircleRadius;

            g.DrawEllipse(errorPen, centerX - scaledCircleRadius, centerY - scaledCircleRadius, scaledCircleRadius * 2f, scaledCircleRadius * 2f);

            float midX = (group.ErrorLine.Start.X + group.ErrorLine.End.X) * 0.5f;
            float midY = (group.ErrorLine.Start.Y + group.ErrorLine.End.Y) * 0.5f;
            float halfDx = (group.ErrorLine.End.X - group.ErrorLine.Start.X) * 0.5f * overlayScale;
            float halfDy = (group.ErrorLine.End.Y - group.ErrorLine.Start.Y) * 0.5f * overlayScale;

            g.DrawLine(errorPen, midX - halfDx, midY - halfDy, midX + halfDx, midY + halfDy);
        }

        /// <summary>
        /// Draws the error value text on the provided graphics object at a calculated location based on the error line data from the specified line group.
        /// </summary>
        /// <param name="g">The <see cref="Graphics"/> object used for drawing.</param>
        /// <param name="group">An instance of <see cref="BahtinovLineDataEventArgs.LineGroup"/> containing the error line data and error circle information.</param>
        private void DrawErrorValueText(Graphics g, BahtinovLineDataEventArgs.LineGroup group)
        {
            var solidBrush = UITheme.GetErrorTextBrush(group.GroupId);
            float overlayScale = GetOverlayScale();

            var radius = (sourceWidth / 2) - 50;

            var textStart = group.Lines[1].Start;
            var textEnd = group.Lines[1].End;

            var (adjustedStart, adjustedEnd) = AdjustPointsForRadius(textStart, textEnd, sourceWidth, sourceHeight, radius);

            using (var font = new Font(this.Font.FontFamily, Math.Max(8f, 25f * overlayScale), FontStyle.Regular, GraphicsUnit.Pixel))
            {
                if (group.GroupId == 1)
                    DrawLineWithCenteredText(g, adjustedStart, group.ErrorCircle.ErrorValue, font, solidBrush);
                else
                    DrawLineWithCenteredText(g, adjustedEnd, group.ErrorCircle.ErrorValue, font, solidBrush);
            }
        }

        /// <summary>
        /// Draws a line of text centered at the specified start point on the provided graphics object.
        /// </summary>
        /// <param name="g">The <see cref="Graphics"/> object used for drawing the text.</param>
        /// <param name="start">The <see cref="Point"/> specifying the start position where the text will be centered.</param>
        /// <param name="text">The text string to be drawn.</param>
        /// <param name="font">The <see cref="Font"/> used to draw the text.</param>
        /// <param name="brush">The <see cref="Brush"/> used to color the text.</param>
        private void DrawLineWithCenteredText(Graphics g, Point start, string text, Font font, Brush brush)
        {
            // Measure the size of the text
            SizeF textSize = g.MeasureString(text, font);

            // Calculate the position to center the text at the start point
            PointF textPosition = new PointF(start.X - textSize.Width / 2, start.Y - textSize.Height / 2);

            // Draw the text
            g.DrawString(text, font, brush, textPosition);
        }
        #endregion

        #region Helper Methods

        /// <summary>
        /// Adds a specified number of new layers to the <see cref="layers"/> collection. Each layer is represented as a <see cref="Bitmap"/> with the dimensions of the PictureBox control.
        /// </summary>
        /// <param name="count">The number of layers to add.</param>
        private void AddNewLayers(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var layer = new Bitmap(Math.Max(1, pictureBox1.Width), Math.Max(1, pictureBox1.Height));
                layers.Add(layer);
            }

            layerSize = pictureBox1.ClientSize;
        }

        /// <summary>
        /// Handles the Paint event of the PictureBox control to render the image layers on the control.
        /// </summary>
        /// <param name="sender">The source of the event, typically the PictureBox control.</param>
        /// <param name="e">An <see cref="PaintEventArgs"/> that contains information about the paint event, including the <see cref="Graphics"/> object used for drawing.</param>
        private void PictureBoxPaint(object sender, PaintEventArgs e)
        {
            EnsureLayerSizeMatchesPictureBox(forceRecreate: false);

            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Rectangle destRect = pictureBox1.ClientRectangle;
            if (destRect.Width <= 0 || destRect.Height <= 0)
                return;

            // Draw scaled so DPI/resize changes scale the content.
            e.Graphics.DrawImage(layers[0], destRect);

            if (selectedGroup == 0)
            {
                for (int i = 1; i < layers.Count; i++)
                {
                    e.Graphics.DrawImage(layers[i], destRect);
                }
            }
            else if (selectedGroup > 0 && selectedGroup < layers.Count)
            {
                e.Graphics.DrawImage(layers[selectedGroup], destRect);
            }
        }

        private void PictureBoxResized(object sender, EventArgs e)
        {
            EnsureLayerSizeMatchesPictureBox(forceRecreate: true);
            RedrawFromCache();
        }

        /// <summary>
        /// Clears all image layers by setting their contents to transparent.
        /// </summary>
        private void ClearAllLayers()
        {
            foreach (var layer in layers)
            {
                using (var g = Graphics.FromImage(layer))
                {
                    g.Clear(Color.Transparent);
                }
            }
        }

        private void UpdateErrorFontForCurrentDpi()
        {
            // Keep a base font around for compatibility; actual error-value drawing scales
            // from source image dimensions in DrawErrorValueText.
            const float sourcePixelFontSize = 25.0f;

            errorFont?.Dispose();
            errorFont = new Font(this.Font.FontFamily, sourcePixelFontSize, FontStyle.Regular, GraphicsUnit.Pixel);
        }

        private float GetOverlayScale()
        {
            const float referenceSize = 600f;
            float minSource = Math.Max(1f, Math.Min(sourceWidth, sourceHeight));
            return minSource / referenceSize;
        }

        private void EnsureLayerSizeMatchesPictureBox(bool forceRecreate)
        {
            Size target = pictureBox1.ClientSize;
            if (target.Width <= 0 || target.Height <= 0)
                return;

            if (!forceRecreate && layerSize == target)
                return;

            if (layers == null || layers.Count == 0)
            {
                layerSize = target;
                return;
            }

            // Recreate each layer bitmap to match the new size.
            // Rendering is done from cached source data to avoid quality loss from resampling.
            for (int i = 0; i < layers.Count; i++)
            {
                Bitmap old = layers[i];
                if (old == null)
                {
                    layers[i] = new Bitmap(target.Width, target.Height);
                    continue;
                }

                if (!forceRecreate && old.Width == target.Width && old.Height == target.Height)
                    continue;

                Bitmap resized = new Bitmap(target.Width, target.Height);

                layers[i] = resized;
                old.Dispose();
            }

            layerSize = target;
        }

        /// <summary>
        /// Updates the cached base image used to rebuild all display layers.
        /// </summary>
        /// <param name="image">The source image to clone into cache, or null to clear the cached source.</param>
        private void CacheSourceImage(Bitmap image)
        {
            cachedSourceImage?.Dispose();
            cachedSourceImage = image != null ? new Bitmap(image) : null;
        }

        /// <summary>
        /// Rebuilds all rendering layers from the currently cached source image and overlay mode.
        /// </summary>
        private void RedrawFromCache()
        {
            ClearAllLayers();

            if (cachedSourceImage == null)
            {
                pictureBox1.Invalidate();
                return;
            }

            DrawImageOnFirstLayer(cachedSourceImage);

            switch (cachedOverlayMode)
            {
                case OverlayMode.Defocus:
                    DrawDefocusCircles(cachedInnerCentre, cachedInnerRadius, cachedOuterCentre, cachedOuterRadius);
                    break;

                case OverlayMode.Bahtinov:
                    if (cachedBahtinovData != null)
                    {
                        var centerX = sourceWidth / 2;
                        var centerY = sourceHeight / 2;
                        var radius = centerX - 20;

                        using (var clippingRegion = CreateCircularClippingRegion(centerX, centerY, radius))
                        {
                            foreach (var group in cachedBahtinovData.LineGroups)
                            {
                                DrawGroupLines(group, clippingRegion);
                            }
                        }
                    }
                    break;
            }

            pictureBox1.Invalidate();
        }

        /// <summary>
        /// Creates a deep clone of Bahtinov overlay data so redraw operations are independent of source object lifetime.
        /// </summary>
        /// <param name="data">The source overlay data to clone.</param>
        /// <returns>A deep cloned data object, or null when input is null.</returns>
        private static BahtinovLineDataEventArgs.BahtinovLineData CloneBahtinovData(BahtinovLineDataEventArgs.BahtinovLineData data)
        {
            if (data == null)
                return null;

            var clone = new BahtinovLineDataEventArgs.BahtinovLineData
            {
                NumberOfGroups = data.NumberOfGroups
            };

            foreach (var group in data.LineGroups)
            {
                var groupClone = new BahtinovLineDataEventArgs.LineGroup(group.GroupId)
                {
                    ErrorLine = group.ErrorLine != null
                        ? new BahtinovLineDataEventArgs.ErrorLine(group.ErrorLine.Start, group.ErrorLine.End)
                        : null,
                    ErrorCircle = group.ErrorCircle != null
                        ? new BahtinovLineDataEventArgs.ErrorCircle(group.ErrorCircle.Origin, group.ErrorCircle.Height, group.ErrorCircle.Width, group.ErrorCircle.ErrorValue)
                        : null
                };

                foreach (var line in group.Lines)
                {
                    groupClone.AddLine(new BahtinovLineDataEventArgs.Line(line.Start, line.End, line.LineId));
                }

                clone.AddLineGroup(groupClone);
            }

            return clone;
        }

        /// <summary>
        /// Creates a circular clipping region centered at a specified point with a given radius.
        /// </summary>
        /// <param name="centerX">The x-coordinate of the center of the circle.</param>
        /// <param name="centerY">The y-coordinate of the center of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <returns>A <see cref="GraphicsPath"/> object representing the circular clipping region.</returns>
        private GraphicsPath CreateCircularClippingRegion(int centerX, int centerY, int radius)
        {
            var path = new GraphicsPath();
            path.AddEllipse(centerX - radius, centerY - radius, radius * 2, radius * 2);
            return path;
        }

        /// <summary>
        /// Applies a transform that maps source-image coordinates into the current layer bitmap while preserving aspect ratio.
        /// </summary>
        /// <param name="g">Graphics context receiving the transform.</param>
        private void ApplyContentTransform(Graphics g)
        {
            if (g == null)
                return;

            // Map the source coordinate space (e.g. 600x600) into the current layer bitmap size,
            // preserving aspect ratio and centering (letterboxing/pillarboxing as needed).
            var target = layers != null && layers.Count > 0 ? layers[0].Size : pictureBox1.ClientSize;
            if (target.Width <= 0 || target.Height <= 0 || sourceWidth <= 0 || sourceHeight <= 0)
                return;

            float sx = (float)target.Width / sourceWidth;
            float sy = (float)target.Height / sourceHeight;
            float scale = Math.Min(sx, sy);

            float scaledW = sourceWidth * scale;
            float scaledH = sourceHeight * scale;
            float offsetX = (target.Width - scaledW) / 2f;
            float offsetY = (target.Height - scaledH) / 2f;

            var m = new Matrix();
            m.Translate(offsetX, offsetY);
            m.Scale(scale, scale);
            g.Transform = m;
        }

        /// <summary>
        /// Adjusts two points to be positioned on a circle centered at the middle of a PictureBox with a specified radius.
        /// </summary>
        /// <param name="start">The initial start point to be adjusted.</param>
        /// <param name="end">The initial end point to be adjusted.</param>
        /// <param name="center_X">The source width used to compute the center x-coordinate.</param>
        /// <param name="center_Y">The source height used to compute the center y-coordinate.</param>
        /// <param name="radius">The radius of the circle on which the points should be adjusted.</param>
        /// <returns>
        /// A tuple containing the adjusted <paramref name="start"/> and <paramref name="end"/> points that are repositioned on the circle.
        /// </returns>
        public static (Point start, Point end) AdjustPointsForRadius(Point start, Point end, int center_X, int center_Y, float radius)
        {
            // Calculate the center of the PictureBox
            int centerX = center_X / 2;
            int centerY = center_Y / 2;

            // Calculate direction vector
            float dx = end.X - start.X;
            float dy = end.Y - start.Y;
            float length = (float)Math.Sqrt(dx * dx + dy * dy);

            // Normalize direction vector
            float unitDx = dx / length;
            float unitDy = dy / length;

            // Calculate the start point on the circle with the given radius
            start.X = (int)(centerX + radius * unitDx);
            start.Y = (int)(centerY + radius * unitDy);

            // Calculate the end point on the circle with the given radius
            end.X = (int)(centerX - radius * unitDx);
            end.Y = (int)(centerY - radius * unitDy);

            return (start, end);
        }

        /// <summary>
        /// Clears the display in the PictureBox by removing all drawn layers and disposing of the current image.
        /// </summary>
        public void ClearDisplay()
        {
            if (pictureBox1.InvokeRequired)
            {
                // If not on the UI thread, use Invoke to marshal the call to the UI thread
                pictureBox1.Invoke(new Action(ClearDisplay));
            }
            else
            {
                ClearAllLayers();
                cachedSourceImage?.Dispose();
                cachedSourceImage = null;
                cachedBahtinovData = null;
                cachedOverlayMode = OverlayMode.None;

                // Dispose of the current image if it exists
                pictureBox1.Image?.Dispose();
                pictureBox1.Image = null;

                // Force the PictureBox to redraw itself as blank
                pictureBox1.Invalidate();
                pictureBox1.Image = Properties.Resources.TBMask_DarkGray;
                pictureBox1.BackColor = UITheme.DarkBackground;
            }
        }

        /// <summary>
        /// Releases cached graphics resources when the control handle is being destroyed.
        /// </summary>
        /// <param name="e">Event data for handle destruction.</param>
        protected override void OnHandleDestroyed(EventArgs e)
        {
            errorFont?.Dispose();
            errorFont = null;
            cachedSourceImage?.Dispose();
            cachedSourceImage = null;
            base.OnHandleDestroyed(e);
        }
        #endregion
    }
}