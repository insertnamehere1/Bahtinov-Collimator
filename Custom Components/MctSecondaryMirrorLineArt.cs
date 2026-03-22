using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Bahtinov_Collimator.Custom_Components
{
    /// <summary>
    /// MCT secondary / corrector cross-section drawing (glass plate, secondary mirror, optical axis).
    /// Edit the <b>SETTINGS</b> region below to tune layout and appearance.
    /// </summary>
    internal static class MctSecondaryMirrorLineArt
    {
        // ═══════════════════════════════════════════════════════════════════════════════
        // SETTINGS — design space (same 50×100 logical units as <see cref="MirrorDrawingComponent"/>)
        // ═══════════════════════════════════════════════════════════════════════════════

        /// <summary>Maximum width/height taken from <c>bounds</c> when building the mirror rectangle.</summary>
        private const int DesignWidthMax = 50;

        private const int DesignHeightMax = 100;

        /// <summary>Padding inside mirror bounds: shrink top/bottom edges (logical units).</summary>
        private const int MarginTop = 4;

        private const int MarginBottom = 4;

        /// <summary>Padding from left/right edges of mirror bounds for the corrector plate.</summary>
        private const int MarginLeft = 2;

        private const int MarginRight = 2;

        // --- Corrector plate width ---
        /// <summary>Minimum base value before plate width scaling.</summary>
        private const int PlateBaseWidthMin = 10;

        /// <summary>Upper cap for base plate width (derived from mirror width / 2).</summary>
        private const int PlateBaseWidthCap = 14;

        /// <summary>Plate width = <c>max(6, round(base * PlateWidthFactor))</c>.</summary>
        private const float PlateWidthFactor = 0.7f;

        /// <summary>Minimum plate width after scaling.</summary>
        private const int PlateWidthMin = 6;

        // --- Plate side bulge (Bezier depth) ---
        private const int BulgeMin = 2;

        private const int BulgeMax = 8;

        /// <summary>Bulge ≈ clamp(plateWidth × 2, BulgeMin, BulgeMax).</summary>
        private const int BulgePerPlateWidth = 2;

        // --- Bezier control points: fractional height along plate for front/back curves ---
        private const float PlateBezierYUpper = 0.25f;

        private const float PlateBezierYLower = 0.75f;

        // --- Secondary mirror blob ---
        /// <summary>Min diameter; also scales with plate height / SecondaryDiameterHeightDivisor.</summary>
        private const int SecondaryDiameterMin = 8;

        private const int SecondaryDiameterMax = 22;

        private const int SecondaryDiameterHeightDivisor = 3;

        /// <summary>Horizontal inset of silver stripe from back edge of plate (min 2, or plateWidth/4).</summary>
        private const int SilverInsetMin = 2;

        private const int SilverInsetDivisor = 4;

        /// <summary>Horizontal “thickness” of the secondary strip (negative extends left of nominal).</summary>
        private const int SecondaryThickness = -4;

        /// <summary>Pixels (logical) added to stripe left for the straight edge of the secondary fill.</summary>
        private const float SecondaryLeftEdgeInset = 3f;

        /// <summary>Iterations for binary search on cubic Bezier to match Y at stripe top/bottom.</summary>
        private const int BezierSolveIterations = 28;

        // --- Glass plate fill (tinted by mirror outline color) ---
        private static readonly Color GlassDarkBase = Color.FromArgb(85, 190, 190, 190);

        private static readonly Color GlassLightBase = Color.FromArgb(55, 235, 235, 235);

        /// <summary>How strongly <see cref="MirrorLineArtTint.TintColor"/> blends outline into glass (0–1).</summary>
        private const float GlassTintStrength = 0.45f;

        // --- Secondary mirror fill ---
        private static readonly Color SecondaryDarkBase = Color.FromArgb(160, 160, 160);

        private static readonly Color SecondaryLightBase = Color.FromArgb(220, 220, 220);

        private const float SecondaryTintStrength = 0.10f;

        /// <summary>Extra X added to gradient end point (right of secondary bounds).</summary>
        private const float SecondaryGradientEndOffsetX = 10f;

        // --- Optical axis (dotted) ---
        /// <summary>Axis starts at <c>mirrorBounds.Left + AxisStartOffsetFromLeft</c>.</summary>
        private const int AxisStartOffsetFromLeft = 20;

        private const float AxisPenWidth = 1.5f;

        // ═══════════════════════════════════════════════════════════════════════════════
        // DRAW
        // ═══════════════════════════════════════════════════════════════════════════════

        /// <param name="opticalAxisLength">Logical length of the dotted optical axis (from control).</param>
        public static void Draw(Graphics graphics, Rectangle bounds, Color mirrorOutlineColor, int opticalAxisLength)
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

            int leftX = mirrorBounds.Left + MarginLeft;
            int rightX = mirrorBounds.Right - MarginRight;

            int axisY = mirrorBounds.Top + (mirrorBounds.Height / 2);

            int plateHeight = Math.Max(10, bottomY - topY);

            int basePlateWidth = Math.Max(PlateBaseWidthMin, Math.Min(PlateBaseWidthCap, mirrorBounds.Width / 2));
            int plateWidth = Math.Max(PlateWidthMin, (int)Math.Round(basePlateWidth * PlateWidthFactor));

            int plateLeft = leftX;
            int plateRight = Math.Min(rightX, plateLeft + plateWidth);

            int bulge = Math.Max(BulgeMin, Math.Min(BulgeMax, plateWidth * BulgePerPlateWidth));
            int frontX = plateLeft;
            int backX = plateRight;
            int frontBulgeX = frontX + bulge;
            int backBulgeX = backX + bulge;

            int secDiameter = Math.Max(SecondaryDiameterMin, Math.Min(plateHeight / SecondaryDiameterHeightDivisor, SecondaryDiameterMax));
            int secRadius = secDiameter / 2;

            int secCenterY = axisY;

            int silverInsetX = Math.Max(SilverInsetMin, plateWidth / SilverInsetDivisor);

            int silverX = backX - silverInsetX;

            Rectangle secondaryStripe = new Rectangle(
                silverX - SecondaryThickness,
                secCenterY - secRadius,
                SecondaryThickness,
                secDiameter);

            if (secondaryStripe.Top < topY) secondaryStripe.Y = topY;
            if (secondaryStripe.Bottom > bottomY) secondaryStripe.Y = bottomY - secondaryStripe.Height;

            GraphicsState state = graphics.Save();
            try
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.CompositingMode = CompositingMode.SourceOver;

                using (GraphicsPath platePath = new GraphicsPath())
                {
                    platePath.StartFigure();

                    platePath.AddBezier(
                        frontX, topY,
                        frontBulgeX, topY + plateHeight * PlateBezierYUpper,
                        frontBulgeX, topY + plateHeight * PlateBezierYLower,
                        frontX, bottomY);

                    platePath.AddLine(frontX, bottomY, backX, bottomY);

                    platePath.AddBezier(
                        backX, bottomY,
                        backBulgeX, topY + plateHeight * PlateBezierYLower,
                        backBulgeX, topY + plateHeight * PlateBezierYUpper,
                        backX, topY);

                    platePath.AddLine(backX, topY, frontX, topY);
                    platePath.CloseFigure();

                    Color glassDarkTinted = MirrorLineArtTint.TintColor(GlassDarkBase, mirrorOutlineColor, GlassTintStrength);
                    Color glassLightTinted = MirrorLineArtTint.TintColor(GlassLightBase, mirrorOutlineColor, GlassTintStrength);

                    using (LinearGradientBrush glassBrush = new LinearGradientBrush(
                        new Point(frontX, topY),
                        new Point(backX + bulge, topY),
                        glassDarkTinted,
                        glassLightTinted))
                    {
                        graphics.FillPath(glassBrush, platePath);
                    }
                }

                using (GraphicsPath secPath = new GraphicsPath())
                {
                    PointF p0 = new PointF(backX, bottomY);
                    PointF p1 = new PointF(backBulgeX, topY + plateHeight * PlateBezierYLower);
                    PointF p2 = new PointF(backBulgeX, topY + plateHeight * PlateBezierYUpper);
                    PointF p3 = new PointF(backX, topY);

                    float yTop = secondaryStripe.Top;
                    float yBottom = secondaryStripe.Bottom;

                    float SolveTForY(float yTarget)
                    {
                        float lo = 0f, hi = 1f;
                        for (int i = 0; i < BezierSolveIterations; i++)
                        {
                            float t = (lo + hi) * 0.5f;
                            float y =
                                (float)Math.Pow(1 - t, 3) * p0.Y +
                                3 * (float)Math.Pow(1 - t, 2) * t * p1.Y +
                                3 * (1 - t) * t * t * p2.Y +
                                t * t * t * p3.Y;

                            if (y > yTarget) lo = t;
                            else hi = t;
                        }
                        return (lo + hi) * 0.5f;
                    }

                    float tTop = SolveTForY(yTop);
                    float tBottom = SolveTForY(yBottom);

                    if (tBottom > tTop)
                        (tBottom, tTop) = (tTop, tBottom);

                    PointF CurvePoint(float t)
                    {
                        float u = 1 - t;
                        return new PointF(
                            u * u * u * p0.X + 3 * u * u * t * p1.X + 3 * u * t * t * p2.X + t * t * t * p3.X,
                            u * u * u * p0.Y + 3 * u * u * t * p1.Y + 3 * u * t * t * p2.Y + t * t * t * p3.Y
                        );
                    }

                    PointF cTop = CurvePoint(tTop);
                    PointF cBottom = CurvePoint(tBottom);

                    float leftEdgeX = secondaryStripe.Left + SecondaryLeftEdgeInset;

                    secPath.StartFigure();
                    secPath.AddLine(leftEdgeX, yTop, leftEdgeX, yBottom);
                    secPath.AddLine(leftEdgeX, yBottom, cBottom.X, yBottom);
                    secPath.AddBezier(cBottom, p1, p2, cTop);
                    secPath.AddLine(cTop.X, yTop, leftEdgeX, yTop);
                    secPath.CloseFigure();

                    Color secDarkTinted = MirrorLineArtTint.TintColor(SecondaryDarkBase, mirrorOutlineColor, SecondaryTintStrength);
                    Color secLightTinted = MirrorLineArtTint.TintColor(SecondaryLightBase, mirrorOutlineColor, SecondaryTintStrength);

                    RectangleF secBounds = secPath.GetBounds();

                    using (LinearGradientBrush secBrush = new LinearGradientBrush(
                        new PointF(secBounds.Left, secBounds.Top),
                        new PointF(secBounds.Right + SecondaryGradientEndOffsetX, secBounds.Top),
                        secLightTinted,
                        secDarkTinted))
                    {
                        graphics.FillPath(secBrush, secPath);
                    }
                }

                int axisStartX = mirrorBounds.Left + AxisStartOffsetFromLeft;
                int axisEndX = Math.Min(bounds.Right, axisStartX + opticalAxisLength);

                using (Pen axisPen = new Pen(mirrorOutlineColor, AxisPenWidth))
                {
                    axisPen.DashStyle = DashStyle.Dot;
                    graphics.DrawLine(axisPen, axisStartX, axisY, axisEndX, axisY);
                }
            }
            finally
            {
                graphics.Restore(state);
            }
        }
    }
}
