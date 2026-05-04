using Bahtinov_Collimator.Custom_Components;
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

        // Zoom / pan state. zoomFactor multiplies the fit-to-window scale, so 1.0 == "fit"
        // (the original rendering). imageOffset is the layer-coordinate position of the
        // displayed image's top-left corner after fit + zoom + pan. Both are applied as
        // a single matrix in ApplyContentTransform so every layer (image, defocus circles,
        // Bahtinov lines, error text) zooms and pans uniformly.
        private float zoomFactor = 1.0f;
        private PointF imageOffset = PointF.Empty;
        private const float MinZoom = 1.0f;
        private const float MaxZoom = 10.0f;
        private const float WheelZoomStep = 1.2f;

        private bool isPanning;
        private Point panStartMouse;
        private PointF panStartImageOffset;

        private MouseWheelMessageFilter mouseWheelFilter;

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

            this.pictureBox1.MouseDown += PictureBoxMouseDown;
            this.pictureBox1.MouseMove += PictureBoxMouseMove;
            this.pictureBox1.MouseUp += PictureBoxMouseUp;
            this.pictureBox1.MouseDoubleClick += PictureBoxMouseDoubleClick;
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

            // Application-wide message filter so wheel events reach our picture box even when
            // keyboard focus is on another control. We deliberately do not make the picture
            // box selectable / focus it on hover so other controls (e.g. dialog text fields)
            // keep their focus when the user moves the mouse over the image to scroll-zoom.
            if (mouseWheelFilter == null)
            {
                mouseWheelFilter = new MouseWheelMessageFilter(pictureBox1, HandleImageMouseWheel);
                Application.AddMessageFilter(mouseWheelFilter);
            }
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

        #region Zoom and Pan

        /// <summary>
        /// Handles mouse-wheel zoom dispatched from <see cref="MouseWheelMessageFilter"/>. Zoom is anchored at the
        /// mouse cursor position so the source pixel under the cursor stays under the cursor across the zoom step.
        /// </summary>
        private void HandleImageMouseWheel(MouseEventArgs e)
        {
            if (cachedSourceImage == null)
                return;

            Size target = GetLayerTargetSize();
            if (target.Width <= 0 || target.Height <= 0)
                return;

            float oldZoom = zoomFactor;
            float multiplier = e.Delta > 0 ? WheelZoomStep : 1f / WheelZoomStep;
            float newZoom = Math.Max(MinZoom, Math.Min(MaxZoom, oldZoom * multiplier));
            if (Math.Abs(newZoom - oldZoom) < 1e-4f)
                return;

            // Pin the source pixel under the cursor to the cursor's screen position:
            //   new_offset = cursor - (cursor - old_offset) * (newZoom / oldZoom)
            float ratio = newZoom / oldZoom;
            imageOffset = new PointF(
                e.X - (e.X - imageOffset.X) * ratio,
                e.Y - (e.Y - imageOffset.Y) * ratio);

            zoomFactor = newZoom;

            if (zoomFactor <= MinZoom + 1e-4f)
            {
                // Snap back to the centered fit when fully zoomed out, locking the pan.
                zoomFactor = MinZoom;
                imageOffset = GetFitOffset(target);
            }
            else
            {
                ClampImageOffset();
            }

            RedrawFromCache();
        }

        /// <summary>
        /// Begins a pan drag if the user left-clicks while zoomed in.
        /// </summary>
        private void PictureBoxMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            if (cachedSourceImage == null)
                return;
            if (zoomFactor <= MinZoom + 1e-4f)
                return;

            isPanning = true;
            panStartMouse = e.Location;
            panStartImageOffset = imageOffset;
            pictureBox1.Cursor = Cursors.SizeAll;
        }

        /// <summary>
        /// Updates the image offset while a pan drag is active.
        /// </summary>
        private void PictureBoxMouseMove(object sender, MouseEventArgs e)
        {
            if (!isPanning)
                return;

            imageOffset = new PointF(
                panStartImageOffset.X + (e.X - panStartMouse.X),
                panStartImageOffset.Y + (e.Y - panStartMouse.Y));

            ClampImageOffset();
            RedrawFromCache();
        }

        /// <summary>
        /// Ends the pan drag and restores the default cursor.
        /// </summary>
        private void PictureBoxMouseUp(object sender, MouseEventArgs e)
        {
            if (!isPanning)
                return;

            isPanning = false;
            pictureBox1.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Double-click resets the zoom to fit so the user can recover quickly from a deep zoom.
        /// </summary>
        private void PictureBoxMouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            if (Math.Abs(zoomFactor - MinZoom) < 1e-4f)
                return;

            ResetZoom();
            RedrawFromCache();
        }

        /// <summary>
        /// Application-wide message filter that forwards <c>WM_MOUSEWHEEL</c> to a target control whenever the cursor
        /// is over it, regardless of which control currently holds keyboard focus. This lets us zoom on hover
        /// without making the picture box selectable (which would steal focus from other controls).
        /// </summary>
        private sealed class MouseWheelMessageFilter : IMessageFilter
        {
            private const int WM_MOUSEWHEEL = 0x020A;

            private readonly Control target;
            private readonly Action<MouseEventArgs> handler;

            public MouseWheelMessageFilter(Control target, Action<MouseEventArgs> handler)
            {
                this.target = target;
                this.handler = handler;
            }

            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg != WM_MOUSEWHEEL)
                    return false;
                if (target == null || target.IsDisposed || !target.IsHandleCreated || !target.Visible)
                    return false;

                // LParam: low word = screen X (signed), high word = screen Y (signed).
                int lParam = m.LParam.ToInt32();
                Point screenPoint = new Point((short)(lParam & 0xFFFF), (short)((lParam >> 16) & 0xFFFF));

                Point clientPoint = target.PointToClient(screenPoint);
                if (!target.ClientRectangle.Contains(clientPoint))
                    return false;

                // WParam: high word = wheel delta (signed); low word = key/button flags (unused here).
                int wParam = m.WParam.ToInt32();
                int delta = (short)((wParam >> 16) & 0xFFFF);

                handler(new MouseEventArgs(MouseButtons.None, 0, clientPoint.X, clientPoint.Y, delta));
                return true;
            }
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
        /// The layered bitmaps are rectangular, so we clip them to the same rounded path the
        /// <see cref="RoundedPictureBox"/> uses for its image fill (otherwise the opaque
        /// source-image pixels in <c>layers[0]</c> would paint over the rounded corners), then
        /// re-stroke the border on top so it stays visible above the overlays.
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

            Rectangle strokeBounds = pictureBox1.GetStrokeBounds();

            using (GraphicsPath roundedPath = pictureBox1.GetRoundedPath(strokeBounds))
            {
                Region previousClip = e.Graphics.Clip;
                e.Graphics.SetClip(roundedPath, CombineMode.Intersect);

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

                e.Graphics.Clip = previousClip;
                previousClip.Dispose();

                using (var borderPen = new Pen(pictureBox1.BorderColor, pictureBox1.BorderThickness))
                {
                    borderPen.Alignment = PenAlignment.Center;
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.DrawPath(borderPen, roundedPath);
                }
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
                ResetZoom();
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

            // Layer geometry changed (resize / DPI); reset zoom + pan so the image starts fitted again.
            ResetZoom();
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
        /// Applies a transform that maps source-image coordinates into the current layer bitmap while preserving
        /// aspect ratio. Includes the user's mouse-wheel zoom factor and pan offset, so every layer rendered
        /// through this method (image, defocus circles, Bahtinov lines, error text) zooms and pans together.
        /// </summary>
        private void ApplyContentTransform(Graphics g)
        {
            if (g == null)
                return;

            Size target = GetLayerTargetSize();
            if (target.Width <= 0 || target.Height <= 0 || sourceWidth <= 0 || sourceHeight <= 0)
                return;

            float effectiveScale = GetFitScale(target) * zoomFactor;

            var m = new Matrix();
            m.Translate(imageOffset.X, imageOffset.Y);
            m.Scale(effectiveScale, effectiveScale);
            g.Transform = m;
        }

        /// <summary>
        /// Returns the scale factor that fits the source image inside the picture box, preserving aspect ratio.
        /// </summary>
        private float GetFitScale(Size target)
        {
            if (target.Width <= 0 || target.Height <= 0 || sourceWidth <= 0 || sourceHeight <= 0)
                return 1f;
            return Math.Min((float)target.Width / sourceWidth, (float)target.Height / sourceHeight);
        }

        /// <summary>
        /// Returns the layer-coordinate offset of the source image's top-left corner when fitted (zoom = 1, no pan).
        /// </summary>
        private PointF GetFitOffset(Size target)
        {
            float fitScale = GetFitScale(target);
            float scaledW = sourceWidth * fitScale;
            float scaledH = sourceHeight * fitScale;
            return new PointF((target.Width - scaledW) / 2f, (target.Height - scaledH) / 2f);
        }

        /// <summary>
        /// Returns the current layer (= picture box client) size used as the target for content transforms.
        /// </summary>
        private Size GetLayerTargetSize()
        {
            return layers != null && layers.Count > 0 && layers[0] != null
                ? layers[0].Size
                : pictureBox1.ClientSize;
        }

        /// <summary>
        /// Resets zoom (1:1 fit) and recenters the image. Called on first load, on resize/DPI changes,
        /// when the display is cleared, and on user double-click.
        /// </summary>
        private void ResetZoom()
        {
            zoomFactor = MinZoom;
            imageOffset = GetFitOffset(GetLayerTargetSize());
        }

        /// <summary>
        /// Constrains <see cref="imageOffset"/> so the displayed (zoomed) image always covers the picture box
        /// when zoomed in, and is centered (letterbox) when at 1:1 fit.
        /// </summary>
        private void ClampImageOffset()
        {
            Size target = GetLayerTargetSize();
            float effectiveScale = GetFitScale(target) * zoomFactor;
            float dispW = sourceWidth * effectiveScale;
            float dispH = sourceHeight * effectiveScale;

            if (dispW <= target.Width)
                imageOffset.X = (target.Width - dispW) / 2f;
            else
                imageOffset.X = Math.Max(target.Width - dispW, Math.Min(0f, imageOffset.X));

            if (dispH <= target.Height)
                imageOffset.Y = (target.Height - dispH) / 2f;
            else
                imageOffset.Y = Math.Max(target.Height - dispH, Math.Min(0f, imageOffset.Y));
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
                ResetZoom();

                // Dispose only bitmaps we own. Properties.Resources returns the same cached
                // Bitmap instance every call, so disposing it would poison the ResourceManager
                // for the whole process. Clone once and own the copy we assign here.
                pictureBox1.Image?.Dispose();
                pictureBox1.Image = null;

                pictureBox1.Invalidate();
                pictureBox1.Image = new Bitmap(Properties.Resources.TBMask_DarkGray);
                pictureBox1.BackColor = UITheme.DarkerBackground;
            }
        }

        /// <summary>
        /// Releases cached graphics resources when the control handle is being destroyed.
        /// </summary>
        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (mouseWheelFilter != null)
            {
                Application.RemoveMessageFilter(mouseWheelFilter);
                mouseWheelFilter = null;
            }

            errorFont?.Dispose();
            errorFont = null;
            cachedSourceImage?.Dispose();
            cachedSourceImage = null;
            base.OnHandleDestroyed(e);
        }
        #endregion
    }
}