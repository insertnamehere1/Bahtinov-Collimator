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
        readonly private List<Bitmap> layers;
        private int selectedGroup = 0;
        private Font errorFont;
        private Size layerSize = Size.Empty;
        private int sourceWidth = 600;
        private int sourceHeight = 600;
        private OverlayMode cachedOverlayMode = OverlayMode.None;
        private Bitmap cachedSourceImage;
        private PointD cachedInnerCentre;
        private PointD cachedOuterCentre;
        private double cachedInnerRadius;
        private double cachedOuterRadius;
        private BahtinovLineDataEventArgs.BahtinovLineData cachedBahtinovData;

        private enum OverlayMode
        {
            None,
            Defocus,
            Bahtinov
        }

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
        private void HandleImageLost(object sender, ImageLostEventArgs e)
        {
            ClearDisplay();
        }

        /// <summary>
        /// Handles the event when a defocus circle is received. Updates the PictureBox with the new image and circle data.
        /// The incoming bitmap is borrowed for the duration of this (synchronous) call; <see cref="CacheSourceImage"/>
        /// takes its own copy before we return, so we do not need to clone here or dispose afterwards.
        /// </summary>
        private void OnDefocusCirceReceived(object sender, DefocusCircleEventArgs e)
        {
            if (pictureBox1.InvokeRequired)
                pictureBox1.Invoke(new Action(() => UpdatePictureBox(e.Image, e.InnerCircleCentre, e.InnerCircleRadius, e.OuterCircleCentre, e.OuterCircleRadius)));
            else
                UpdatePictureBox(e.Image, e.InnerCircleCentre, e.InnerCircleRadius, e.OuterCircleCentre, e.OuterCircleRadius);
        }

        /// <summary>
        /// Handles the event when Bahtinov line data is received. Updates the PictureBox with the new image and line data.
        /// The incoming bitmap is borrowed for the duration of this (synchronous) call; <see cref="CacheSourceImage"/>
        /// takes its own copy before we return, so we do not need to clone here or dispose afterwards.
        /// </summary>
        private void OnBahtinovLineReceive(object sender, BahtinovLineDataEventArgs e)
        {
            if (pictureBox1.InvokeRequired)
                pictureBox1.Invoke(new Action(() => UpdatePictureBox(e.Image, e.Linedata)));
            else
                UpdatePictureBox(e.Image, e.Linedata);
        }

        /// <summary>
        /// Handles the event when the channel selection changes. Updates the selected group index and refreshes the PictureBox.
        /// </summary>
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
        /// The bitmap is borrowed; <see cref="CacheSourceImage"/> takes its own copy, so we do not dispose it here.
        /// </summary>
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
        }

        /// <summary>
        /// Updates the PictureBox with the provided image and drawing data, including bahtinov line data and error values.
        /// The bitmap is borrowed; <see cref="CacheSourceImage"/> takes its own copy, so we do not dispose it here.
        /// </summary>
        private void UpdatePictureBox(Bitmap image, BahtinovLineDataEventArgs.BahtinovLineData data)
        {
            CacheSourceImage(image);
            cachedOverlayMode = OverlayMode.Bahtinov;
            cachedBahtinovData = CloneBahtinovData(data);

            RedrawFromCache();
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
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.CompositingMode = CompositingMode.SourceOver;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                ApplyContentTransform(g);

                float layerCentreX = sourceWidth / 2f;
                float layerCentreY = sourceHeight / 2f;
                float overlayScale = GetOverlayScale();

                var innerPx = new PointF(layerCentreX + (float)innerCentre.X, layerCentreY + (float)innerCentre.Y);
                var outerPx = new PointF(layerCentreX + (float)outerCentre.X, layerCentreY + (float)outerCentre.Y);

                float penWidth = 3f * overlayScale;
                float crossSize = 20f * overlayScale;  // overall width/height of cross
                const int crossAlpha = 200;   // 0..255. Lower = more blending.

                using (var innerPen = new Pen(UITheme.WithAlpha(UITheme.DefocusInnerCircleColor, crossAlpha), penWidth))
                using (var outerPen = new Pen(UITheme.WithAlpha(UITheme.DefocusOuterCircleColor, crossAlpha), penWidth))
                using (var outerDashPen = new Pen(UITheme.DefocusOuterCircleColor, penWidth)
                {
                    DashStyle = System.Drawing.Drawing2D.DashStyle.Dash
                })
                using (var innerDashPen = new Pen(UITheme.DefocusInnerCircleColor, penWidth)
                {
                    DashStyle = System.Drawing.Drawing2D.DashStyle.Dash
                })
                {
                    DrawCircle(g, innerDashPen, innerPx, (float)innerRadius);
                    DrawCircle(g, outerDashPen, outerPx, (float)outerRadius);
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

            g.DrawLine(pen, centre.X - half, centre.Y, centre.X + half, centre.Y);
            g.DrawLine(pen, centre.X, centre.Y - half, centre.X, centre.Y + half);
        }

        /// <summary>
        /// Draws the provided image on the first layer of the PictureBox.
        /// </summary>
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

                ApplyContentTransform(g);
                g.DrawImage(image, new Rectangle(0, 0, sourceWidth, sourceHeight));
            }
        }

        /// <summary>
        /// Draws a group of lines on the PictureBox based on the provided line group data.
        /// </summary>
        private void DrawGroupLines(BahtinovLineDataEventArgs.LineGroup group, GraphicsPath clippingRegion)
        {
            using (var g = Graphics.FromImage(layers[group.GroupId + 1]))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                ApplyContentTransform(g);
                g.SetClip(clippingRegion);

                const int alpha = 150;   // 0–255  lower = more transparent, higher = more opaque

                foreach (var line in group.Lines)
                {
                    var pen = UITheme.GetDisplayLinePen(group.GroupId, line.LineId);

                    var c = pen.Color;
                    pen.Color = UITheme.WithAlpha(c, alpha);

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
        private void DrawErrorCircleAndLine(Graphics g, BahtinovLineDataEventArgs.LineGroup group)
        {
            var errorPen = UITheme.GetErrorCirclePen(group.GroupId);
            float overlayScale = GetOverlayScale();

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
        private void DrawLineWithCenteredText(Graphics g, Point start, string text, Font font, Brush brush)
        {
            SizeF textSize = g.MeasureString(text, font);
            PointF textPosition = new PointF(start.X - textSize.Width / 2, start.Y - textSize.Height / 2);
            g.DrawString(text, font, brush, textPosition);
        }
        #endregion

        #region Helper Methods

        /// <summary>
        /// Adds a specified number of new layers to the <see cref="layers"/> collection. Each layer is represented as a <see cref="Bitmap"/> with the dimensions of the PictureBox control.
        /// </summary>
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
        private void PictureBoxPaint(object sender, PaintEventArgs e)
        {
            EnsureLayerSizeMatchesPictureBox(forceRecreate: false);

            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Rectangle destRect = pictureBox1.ClientRectangle;
            if (destRect.Width <= 0 || destRect.Height <= 0)
                return;

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

        /// <summary>
        /// Rebuilds backing layers when the picture box size changes, then redraws cached content.
        /// </summary>
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
                    g.Clear(UITheme.Transparent);
                }
            }
        }

        /// <summary>
        /// Updates the reusable error-text font instance used for overlay value rendering.
        /// </summary>
        private void UpdateErrorFontForCurrentDpi()
        {
            const float sourcePixelFontSize = 25.0f;

            errorFont?.Dispose();
            errorFont = new Font(this.Font.FontFamily, sourcePixelFontSize, FontStyle.Regular, GraphicsUnit.Pixel);
        }

        /// <summary>
        /// Computes the scale factor used to normalize overlay geometry against the reference source size.
        /// </summary>
        /// <returns>Overlay scale ratio derived from the current source image dimensions.</returns>
        private float GetOverlayScale()
        {
            float referenceSize = Math.Max(1f, UITheme.DisplayWindowReferenceSizeAt96);
            float minSource = Math.Max(1f, Math.Min(sourceWidth, sourceHeight));
            return minSource / referenceSize;
        }

        /// <summary>
        /// Ensures each bitmap layer matches the current picture box client size.
        /// </summary>
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
        private void ApplyContentTransform(Graphics g)
        {
            if (g == null)
                return;

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
            int centerX = center_X / 2;
            int centerY = center_Y / 2;

            float dx = end.X - start.X;
            float dy = end.Y - start.Y;
            float length = (float)Math.Sqrt(dx * dx + dy * dy);

            float unitDx = dx / length;
            float unitDy = dy / length;

            start.X = (int)(centerX + radius * unitDx);
            start.Y = (int)(centerY + radius * unitDy);

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
                pictureBox1.Invoke(new Action(ClearDisplay));
            }
            else
            {
                ClearAllLayers();
                cachedSourceImage?.Dispose();
                cachedSourceImage = null;
                cachedBahtinovData = null;
                cachedOverlayMode = OverlayMode.None;

                // Dispose only bitmaps we own. Properties.Resources returns the same cached
                // Bitmap instance every call, so disposing it would poison the ResourceManager
                // for the whole process. Clone once and own the copy we assign here.
                pictureBox1.Image?.Dispose();
                pictureBox1.Image = null;

                pictureBox1.Invalidate();
                pictureBox1.Image = new Bitmap(Properties.Resources.TBMask_DarkGray);
                pictureBox1.BackColor = UITheme.DarkBackground;
            }
        }

        /// <summary>
        /// Releases cached graphics resources when the control handle is being destroyed.
        /// </summary>
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