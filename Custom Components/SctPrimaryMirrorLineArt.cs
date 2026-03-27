using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Bahtinov_Collimator.Custom_Components
{
    /// <summary>
    /// SCT primary mirror cross-section (glass body, reflective coating, baffle, optical axis).
    /// Edit the <b>SETTINGS</b> region below to tune layout and appearance.
    /// </summary>
    internal static class SctPrimaryMirrorLineArt
    {
        #region Settings

        /// <summary>Maximum width/height taken from <c>bounds</c> when building the mirror rectangle.</summary>
        private const int DesignWidthMax = 50;

        private const int DesignHeightMax = 100;

        /// <summary>Padding: shrink top/bottom of usable drawing area inside mirror bounds.</summary>
        private const int MarginTop = 1;

        private const int MarginBottom = 1;

        /// <summary>Back (left) edge of mirror outline: inset from mirror bounds left.</summary>
        private const int BackInsetLeft = 2;

        /// <summary>Concave corner / depth line: inset from mirror bounds <b>right</b> (logical X).</summary>
        private const int ConcaveInsetFromRight = 25;

        /// <summary>Front (reflective) edge X offset from <see cref="BackInsetLeft"/> when building geometry.</summary>
        private const int FrontEdgeOffsetFromBack = 10;

        /// <summary>Horizontal bulge for the left mirrored curve (0 = vertical line, sign flips bow direction).</summary>
        /// <remarks>
        /// Positive values bow one way; negative values bow the opposite way.
        /// Use magnitude to control how far the left curve deviates from vertical.
        /// </remarks>
        private const float LeftCurveControlXFactor = -1f;

        /// <summary>Vertical bulge multiplier for the left curve when <c>cornerRadius &gt; 0</c>.</summary>
        private const float LeftConcaveCurveOffsetYFactor = 5f;

        /// <summary>Vertical bulge multiplier for the left curve when <c>cornerRadius == 0</c>.</summary>
        private const float LeftFlatCurveOffsetYFactor = 5f;

        /// <summary>Vertical offset (down) for Bezier control points from top/bottom when using rounded outline.</summary>
        private const int ConcaveBezierOffsetY = 18;

        /// <summary>Vertical pull for single Bezier from top/bottom edges (smaller = flatter face).</summary>
        private const int FlatFaceBezierOffsetY = 18;

        /// <summary>Horizontal length of the two baffle rail lines from the back edge.</summary>
        private const int BaffleWallLength = 60;

        /// <summary>Min side of the square baffle opening (clamped up from height / divisor).</summary>
        private const int BaffleSquareMin = 0;

        private const int BaffleSquareMax = 16;

        /// <summary>Baffle opening size uses <c>clamp(height / BaffleSquareHeightDivisor, min, max)</c>.</summary>
        private const int BaffleSquareHeightDivisor = 6;

        /// <summary>Subtract from (<c>concaveDepthX - backX</c>) when building the through-cut rectangle width.</summary>
        private const int BaffleThroughCutInset = 20;

        /// <summary>
        /// Baffle back (left edge) inset relative to mirror bounds left.
        /// Keeps the mirror outline independent from how far the baffle is shifted.
        /// </summary>
        private const int BaffleBackInsetLeft = -7;

        /// <summary>Extra inset from design-left (x=0 in the 50×100 design space) for the through-cut hole.</summary>
        /// <remarks>
        /// Set to <c>0</c> to make the baffle cut-through reach all the way to <c>x=0</c>.
        /// </remarks>
        private const int BaffleThroughCutLeftInset = 0;

        private static readonly Color GlassDarkBase = Color.FromArgb(70, 190, 190, 190);

        private static readonly Color GlassLightBase = Color.FromArgb(70, 235, 235, 235);

        private const float GlassTintStrength = 1f;

        private const float CoatingPenThickness = 2f;

        private static readonly Color MetalDarkBase = Color.FromArgb(150, 150, 150);

        private static readonly Color MetalLightBase = Color.FromArgb(230, 230, 230);

        private const float MetalTintStrength = 0.10f;

        /// <summary>Metal gradient spans <c>concaveDepthX ± MetalGradientSpanX</c>.</summary>
        private const int MetalGradientSpanX = 10;

        /// <summary>Inflate baffle cutout when clipping so coating doesn’t leak into the hole.</summary>
        private const int CoatingClipBaffleInflate = 2;

        private static readonly Color BafflePenColor = Color.DarkGray;

        private const float BafflePenWidth = 2f;

        /// <summary>Inflate tube interior exclusion to avoid anti-aliased fill/metal remnants.</summary>
        private const int BaffleInteriorEraseInflate = 1;

        /// <summary>Axis starts at <c>mirrorBounds.Left + AxisStartOffsetFromLeft</c>.</summary>
        private const int AxisStartOffsetFromLeft = 2;

        private const float AxisPenWidth = 1.5f;


        #endregion

        #region Drawing

        /// <summary>
        /// Draws the SCT primary mirror line art within the provided design-space bounds.
        /// </summary>
        /// <param name="graphics">Graphics context used for drawing.</param>
        /// <param name="bounds">Design-space bounds that constrain the mirror geometry.</param>
        /// <param name="mirrorOutlineColor">Base outline color used for strokes and tint blending.</param>
        /// <param name="opticalAxisLength">Logical length of dotted axis from control.</param>
        /// <param name="cornerRadius">Corner radius from control (clamped by available space).</param>
        public static void Draw(Graphics graphics, Rectangle bounds, Color mirrorOutlineColor, int opticalAxisLength, int cornerRadius)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return;

            Rectangle mirrorBounds = new Rectangle(
                bounds.X,
                bounds.Y,
                Math.Min(DesignWidthMax, bounds.Width),
                Math.Min(DesignHeightMax, bounds.Height));

            int topY = mirrorBounds.Top + MarginTop;
            int bottomY = mirrorBounds.Bottom - MarginBottom;
            int backX = mirrorBounds.Left + BackInsetLeft;
            int concaveDepthX = mirrorBounds.Right - ConcaveInsetFromRight;
            int frontEdgeX = backX + FrontEdgeOffsetFromBack;
            int baffleBackX = mirrorBounds.Left + BaffleBackInsetLeft;

            float leftCurveCtrlXBase = backX + concaveDepthX - frontEdgeX;
            float leftCurveCtrlX = backX + (leftCurveCtrlXBase - backX) * LeftCurveControlXFactor;

            int maxRadius = Math.Max(0, Math.Min((concaveDepthX - backX) / 2, (bottomY - topY) / 2));
            int radius = Math.Min(cornerRadius, maxRadius);

            int axisY = mirrorBounds.Top + (mirrorBounds.Height / 2);

            int baffleSquareSize = Math.Max(BaffleSquareMin, Math.Min(BaffleSquareMax, mirrorBounds.Height / BaffleSquareHeightDivisor));

            Rectangle baffleCutout = new Rectangle(
                baffleBackX,
                axisY - (baffleSquareSize / 2),
                baffleSquareSize,
                baffleSquareSize);

            if (baffleCutout.Top < topY) baffleCutout.Y = topY;
            if (baffleCutout.Bottom > bottomY) baffleCutout.Y = bottomY - baffleCutout.Height;

            int baffleCutoutThroughX = mirrorBounds.Left + BaffleThroughCutLeftInset;
            Rectangle baffleCutoutThrough = new Rectangle(
                baffleCutoutThroughX,
                baffleCutout.Top,
                Math.Max(1, concaveDepthX - baffleCutoutThroughX - BaffleThroughCutInset),
                baffleCutout.Height);

            int baffleStartX = baffleBackX;
            int baffleEndX = Math.Min(bounds.Right, baffleStartX + BaffleWallLength);
            Rectangle baffleTubeInterior = Rectangle.FromLTRB(
                Math.Min(baffleStartX, baffleEndX),
                baffleCutout.Top,
                Math.Max(baffleStartX, baffleEndX),
                baffleCutout.Bottom);
            if (baffleTubeInterior.Width < 1)
                baffleTubeInterior.Width = 1;
            if (baffleTubeInterior.Height < 1)
                baffleTubeInterior.Height = 1;

            using (GraphicsPath outlinePath = new GraphicsPath())
            {
                outlinePath.StartFigure();

                if (radius > 0)
                {
                    int d = radius * 2;

                    outlinePath.AddLine(backX + radius, topY, concaveDepthX - radius, topY);
                    outlinePath.AddArc(concaveDepthX - d, topY, d, d, 270f, 90f);
                    outlinePath.AddBezier(
                        concaveDepthX,
                        topY + radius,
                        frontEdgeX,
                        topY + radius + ConcaveBezierOffsetY,
                        frontEdgeX,
                        bottomY - radius - ConcaveBezierOffsetY,
                        concaveDepthX,
                        bottomY - radius);
                    outlinePath.AddArc(concaveDepthX - d, bottomY - d, d, d, 0f, 90f);
                    outlinePath.AddLine(concaveDepthX - radius, bottomY, backX + radius, bottomY);
                    outlinePath.AddArc(backX, bottomY - d, d, d, 90f, 90f);
                    float leftOffsetY = ConcaveBezierOffsetY * LeftConcaveCurveOffsetYFactor;
                    float maxRoundedOffset = ((bottomY - radius) - (topY + radius)) * 0.45f;
                    if (leftOffsetY > maxRoundedOffset)
                        leftOffsetY = Math.Max(0f, maxRoundedOffset);
                    float leftCtrlTopY = topY + radius + leftOffsetY;
                    float leftCtrlBottomY = bottomY - radius - leftOffsetY;
                    if (leftCtrlBottomY < leftCtrlTopY)
                    {
                        float mid = (leftCtrlTopY + leftCtrlBottomY) * 0.5f;
                        leftCtrlTopY = mid;
                        leftCtrlBottomY = mid;
                    }

                    outlinePath.AddBezier(
                        backX, bottomY - radius,
                        leftCurveCtrlX, leftCtrlTopY,
                        leftCurveCtrlX, leftCtrlBottomY,
                        backX, topY + radius);
                    outlinePath.AddArc(backX, topY, d, d, 180f, 90f);
                }
                else
                {
                    outlinePath.AddLine(backX, topY, concaveDepthX, topY);
                    outlinePath.AddBezier(
                        concaveDepthX, topY,
                        frontEdgeX, topY + FlatFaceBezierOffsetY,
                        frontEdgeX, bottomY - FlatFaceBezierOffsetY,
                        concaveDepthX, bottomY);
                    outlinePath.AddLine(concaveDepthX, bottomY, backX, bottomY);
                    float leftOffsetY = FlatFaceBezierOffsetY * LeftFlatCurveOffsetYFactor;
                    float maxFlatOffset = (bottomY - topY) * 0.45f;
                    if (leftOffsetY > maxFlatOffset)
                        leftOffsetY = Math.Max(0f, maxFlatOffset);
                    float leftCtrlTopY = topY + leftOffsetY;
                    float leftCtrlBottomY = bottomY - leftOffsetY;
                    if (leftCtrlBottomY < leftCtrlTopY)
                    {
                        float mid = (leftCtrlTopY + leftCtrlBottomY) * 0.5f;
                        leftCtrlTopY = mid;
                        leftCtrlBottomY = mid;
                    }

                    outlinePath.AddBezier(
                        backX, bottomY,
                        leftCurveCtrlX, leftCtrlTopY,
                        leftCurveCtrlX, leftCtrlBottomY,
                        backX, topY);
                }

                outlinePath.CloseFigure();

                Color glassDarkTinted = MirrorLineArtTint.TintColor(GlassDarkBase, mirrorOutlineColor, GlassTintStrength);
                Color glassLightTinted = MirrorLineArtTint.TintColor(GlassLightBase, mirrorOutlineColor, GlassTintStrength);

                using (LinearGradientBrush glassBrush = new LinearGradientBrush(
                    new Point(backX, topY),
                    new Point(concaveDepthX, topY),
                    glassDarkTinted,
                    glassLightTinted))
                using (Region glassRegion = new Region(outlinePath))
                {
                    glassRegion.Exclude(baffleCutoutThrough);
                    glassRegion.Exclude(baffleCutout);
                    Rectangle glassEraseTube = baffleTubeInterior;
                    glassEraseTube.Inflate(BaffleInteriorEraseInflate, BaffleInteriorEraseInflate);
                    glassRegion.Exclude(glassEraseTube);
                    graphics.FillRegion(glassBrush, glassRegion);
                }

                using (GraphicsPath concaveFacePath = new GraphicsPath())
                {
                    if (radius > 0)
                    {
                        int d = radius * 2;

                        concaveFacePath.AddBezier(
                            concaveDepthX,
                            topY + radius,
                            frontEdgeX,
                            topY + radius + ConcaveBezierOffsetY,
                            frontEdgeX,
                            bottomY - radius - ConcaveBezierOffsetY,
                            concaveDepthX,
                            bottomY - radius);
                    }
                    else
                    {
                        concaveFacePath.AddBezier(
                            concaveDepthX, topY,
                            frontEdgeX, topY + FlatFaceBezierOffsetY,
                            frontEdgeX, bottomY - FlatFaceBezierOffsetY,
                            concaveDepthX, bottomY);
                    }

                    Color metalDarkTinted = MirrorLineArtTint.TintColor(MetalDarkBase, mirrorOutlineColor, MetalTintStrength);
                    Color metalLightTinted = MirrorLineArtTint.TintColor(MetalLightBase, mirrorOutlineColor, MetalTintStrength);

                    using (LinearGradientBrush metalBrush = new LinearGradientBrush(
                        new Point(concaveDepthX - MetalGradientSpanX, topY),
                        new Point(concaveDepthX + MetalGradientSpanX, topY),
                        metalDarkTinted,
                        metalLightTinted))
                    using (Pen coatingPen = new Pen(metalBrush, CoatingPenThickness))
                    {
                        coatingPen.LineJoin = LineJoin.Round;
                        coatingPen.StartCap = LineCap.Round;
                        coatingPen.EndCap = LineCap.Round;
                        coatingPen.Alignment = PenAlignment.Inset;

                        GraphicsState clipState = graphics.Save();
                        try
                        {
                            using (Region r = new Region(bounds))
                            {
                                Rectangle clipHoleThrough = baffleCutoutThrough;
                                clipHoleThrough.Inflate(CoatingClipBaffleInflate, CoatingClipBaffleInflate);
                                r.Exclude(clipHoleThrough);

                                Rectangle clipHoleSquare = baffleCutout;
                                clipHoleSquare.Inflate(CoatingClipBaffleInflate, CoatingClipBaffleInflate);
                                r.Exclude(clipHoleSquare);

                                Rectangle clipTube = baffleTubeInterior;
                                clipTube.Inflate(BaffleInteriorEraseInflate, BaffleInteriorEraseInflate);
                                r.Exclude(clipTube);
                                graphics.SetClip(r, CombineMode.Replace);
                            }

                            graphics.DrawPath(coatingPen, concaveFacePath);
                        }
                        finally
                        {
                            graphics.Restore(clipState);
                        }
                    }
                }
            }

            using (Pen bafflePen = new Pen(BafflePenColor, BafflePenWidth))
            {
                graphics.DrawLine(bafflePen, baffleStartX, baffleCutout.Top, baffleEndX, baffleCutout.Top);
                graphics.DrawLine(bafflePen, baffleStartX, baffleCutout.Bottom, baffleEndX, baffleCutout.Bottom);
            }

            int axisStartX = mirrorBounds.Left + AxisStartOffsetFromLeft;
            int axisEndX = Math.Min(bounds.Right, axisStartX + opticalAxisLength);

            using (Pen axisPen = new Pen(mirrorOutlineColor, AxisPenWidth))
            {
                axisPen.DashStyle = DashStyle.Dot;
                graphics.DrawLine(axisPen, axisStartX, axisY, axisEndX, axisY);
            }
        }

        #endregion
    }
}
