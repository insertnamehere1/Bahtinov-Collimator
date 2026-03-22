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
        // ═══════════════════════════════════════════════════════════════════════════════
        // SETTINGS — design space (same 50×100 logical units as <see cref="MirrorDrawingComponent"/>)
        // ═══════════════════════════════════════════════════════════════════════════════

        /// <summary>Maximum width/height taken from <c>bounds</c> when building the mirror rectangle.</summary>
        private const int DesignWidthMax = 50;

        private const int DesignHeightMax = 100;

        // --- Outer frame / mirror box ---
        /// <summary>Padding: shrink top/bottom of usable drawing area inside mirror bounds.</summary>
        private const int MarginTop = 1;

        private const int MarginBottom = 1;

        /// <summary>Back (left) edge of mirror outline: inset from mirror bounds left.</summary>
        private const int BackInsetLeft = 2;

        /// <summary>Concave corner / depth line: inset from mirror bounds <b>right</b> (logical X).</summary>
        private const int ConcaveInsetFromRight = 25;

        /// <summary>Front (reflective) edge X offset from <see cref="BackInsetLeft"/> when building geometry.</summary>
        private const int FrontEdgeOffsetFromBack = 10;

        // --- Rounded-corner outline (when corner radius > 0) ---
        /// <summary>Vertical offset (down) for Bezier control points from top/bottom when using rounded outline.</summary>
        private const int ConcaveBezierOffsetY = 18;

        // --- Flat-corner outline (radius == 0) ---
        /// <summary>Vertical pull for single Bezier from top/bottom edges (smaller = flatter face).</summary>
        private const int FlatFaceBezierOffsetY = 18;

        // --- Baffle tube (rear opening) ---
        /// <summary>Horizontal length of the two baffle rail lines from the back edge.</summary>
        private const int BaffleWallLength = 40;

        /// <summary>Min side of the square baffle opening (clamped up from height / divisor).</summary>
        private const int BaffleSquareMin = 6;

        private const int BaffleSquareMax = 16;

        /// <summary>Baffle opening size uses <c>clamp(height / BaffleSquareHeightDivisor, min, max)</c>.</summary>
        private const int BaffleSquareHeightDivisor = 6;

        /// <summary>Subtract from (<c>concaveDepthX - backX</c>) when building the through-cut rectangle width.</summary>
        private const int BaffleThroughCutInset = 9;

        // --- Glass body fill (tinted by outline color) ---
        private static readonly Color GlassDarkBase = Color.FromArgb(85, 190, 190, 190);

        private static readonly Color GlassLightBase = Color.FromArgb(55, 235, 235, 235);

        private const float GlassTintStrength = 0.45f;

        // --- Reflective coating stroke ---
        private const float CoatingPenThickness = 2f;

        private static readonly Color MetalDarkBase = Color.FromArgb(150, 150, 150);

        private static readonly Color MetalLightBase = Color.FromArgb(230, 230, 230);

        private const float MetalTintStrength = 0.10f;

        /// <summary>Metal gradient spans <c>concaveDepthX ± MetalGradientSpanX</c>.</summary>
        private const int MetalGradientSpanX = 10;

        /// <summary>Inflate baffle cutout when clipping so coating doesn’t leak into the hole.</summary>
        private const int CoatingClipBaffleInflate = 2;

        // --- Baffle rail lines (drawn after body) ---
        private static readonly Color BafflePenColor = Color.DarkGray;

        private const float BafflePenWidth = 2f;

        // --- Optical axis (dotted) ---
        /// <summary>Axis starts at <c>mirrorBounds.Left + AxisStartOffsetFromLeft</c>.</summary>
        private const int AxisStartOffsetFromLeft = 2;

        private const float AxisPenWidth = 1.5f;

        // ═══════════════════════════════════════════════════════════════════════════════
        // DRAW
        // ═══════════════════════════════════════════════════════════════════════════════

        /// <param name="cornerRadius">Corner radius from control (clamped by available space).</param>
        /// <param name="opticalAxisLength">Logical length of dotted axis from control.</param>
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

            int maxRadius = Math.Max(0, Math.Min((concaveDepthX - backX) / 2, (bottomY - topY) / 2));
            int radius = Math.Min(cornerRadius, maxRadius);

            int axisY = mirrorBounds.Top + (mirrorBounds.Height / 2);

            int baffleSquareSize = Math.Max(BaffleSquareMin, Math.Min(BaffleSquareMax, mirrorBounds.Height / BaffleSquareHeightDivisor));

            Rectangle baffleCutout = new Rectangle(
                backX,
                axisY - (baffleSquareSize / 2),
                baffleSquareSize,
                baffleSquareSize);

            if (baffleCutout.Top < topY) baffleCutout.Y = topY;
            if (baffleCutout.Bottom > bottomY) baffleCutout.Y = bottomY - baffleCutout.Height;

            Rectangle baffleCutoutThrough = new Rectangle(
                backX,
                baffleCutout.Top,
                Math.Max(1, concaveDepthX - backX - BaffleThroughCutInset),
                baffleCutout.Height);

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
                    outlinePath.AddLine(backX, bottomY - radius, backX, topY + radius);
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
                    outlinePath.AddLine(backX, bottomY, backX, topY);
                }

                outlinePath.CloseFigure();

                using (GraphicsPath fillPath = (GraphicsPath)outlinePath.Clone())
                {
                    fillPath.FillMode = FillMode.Alternate;
                    fillPath.AddRectangle(baffleCutoutThrough);

                    Color glassDarkTinted = MirrorLineArtTint.TintColor(GlassDarkBase, mirrorOutlineColor, GlassTintStrength);
                    Color glassLightTinted = MirrorLineArtTint.TintColor(GlassLightBase, mirrorOutlineColor, GlassTintStrength);

                    using (LinearGradientBrush glassBrush = new LinearGradientBrush(
                        new Point(backX, topY),
                        new Point(concaveDepthX, topY),
                        glassDarkTinted,
                        glassLightTinted))
                    {
                        graphics.FillPath(glassBrush, fillPath);
                    }
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
                                Rectangle clipHole = baffleCutoutThrough;
                                clipHole.Inflate(CoatingClipBaffleInflate, CoatingClipBaffleInflate);
                                r.Exclude(clipHole);
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

            int baffleStartX = backX;
            int baffleEndX = Math.Min(bounds.Right, baffleStartX + BaffleWallLength);

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
    }
}
