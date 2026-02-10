using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Bahtinov_Collimator.Custom_Components
{
    public enum MirrorType
    {
        SctPrimary,
        MctSecondary
    }

    /// <summary>
    /// Reusable user control that draws telescope mirror cross-sections.
    /// Supports selecting different mirror types via <see cref="MirrorType"/>.
    /// </summary>
    public class MirrorDrawingComponent : Control
    {
        private int cornerRadius = 3;
        private int opticalAxisLength = 50;
        private Color mirrorOutlineColor = Color.DimGray;
        private MirrorType mirrorType = MirrorType.SctPrimary;

        public MirrorDrawingComponent()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);

            Size = new Size(100, 100);
            BackColor = Color.FromArgb(32, 32, 32);
        }

        public MirrorType MirrorType
        {
            get => mirrorType;
            set
            {
                mirrorType = value;
                Invalidate();
            }
        }

        public int CornerRadius
        {
            get => cornerRadius;
            set
            {
                cornerRadius = Math.Max(0, value);
                Invalidate();
            }
        }

        public int OpticalAxisLength
        {
            get => opticalAxisLength;
            set
            {
                opticalAxisLength = Math.Max(0, value);
                Invalidate();
            }
        }

        public Color MirrorOutlineColor
        {
            get => mirrorOutlineColor;
            set
            {
                mirrorOutlineColor = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            switch (MirrorType)
            {
                case MirrorType.MctSecondary:
                    DrawMctSecondaryMirror(e.Graphics, ClientRectangle);
                    break;

                case MirrorType.SctPrimary:
                default:
                    DrawSctPrimaryMirror(e.Graphics, ClientRectangle);
                    break;
            }
        }

        private void DrawMctSecondaryMirror(Graphics graphics, Rectangle bounds)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return;

            Rectangle mirrorBounds = new Rectangle(
                bounds.X,
                bounds.Y,
                Math.Min(50, bounds.Width),
                Math.Min(100, bounds.Height));

            int topY = mirrorBounds.Top + 4;
            int bottomY = mirrorBounds.Bottom - 4;

            int leftX = mirrorBounds.Left + 2;
            int rightX = mirrorBounds.Right - 2;

            int axisY = mirrorBounds.Top + (mirrorBounds.Height / 2);

            int plateHeight = Math.Max(10, bottomY - topY);

            int basePlateWidth = Math.Max(10, Math.Min(14, mirrorBounds.Width / 2));
            int plateWidth = Math.Max(6, (int)Math.Round(basePlateWidth * 0.7f));

            int plateLeft = leftX;
            int plateRight = Math.Min(rightX, plateLeft + plateWidth);

            int bulge = Math.Max(2, Math.Min(6, plateWidth / 2));
            int frontX = plateLeft;
            int backX = plateRight;
            int frontBulgeX = frontX + bulge;
            int backBulgeX = backX + bulge;

            int secDiameter = Math.Max(8, Math.Min(plateHeight / 3, 22));
            int secRadius = secDiameter / 2;

            int secCenterY = axisY;

            int silverInsetX = Math.Max(2, plateWidth / 4);

            // 🔧 NEW: thickness of the secondary (horizontal)
            const int secondaryThickness = -4;   // was ~2 px before

            int silverX = backX - silverInsetX;

            Rectangle secondaryStripe = new Rectangle(
                silverX - secondaryThickness,
                secCenterY - secRadius,
                secondaryThickness,
                secDiameter);

            if (secondaryStripe.Top < topY) secondaryStripe.Y = topY;
            if (secondaryStripe.Bottom > bottomY) secondaryStripe.Y = bottomY - secondaryStripe.Height;

            GraphicsState state = graphics.Save();
            try
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.CompositingMode = CompositingMode.SourceOver;

                // Corrector plate
                using (GraphicsPath platePath = new GraphicsPath())
                {
                    platePath.StartFigure();

                    platePath.AddBezier(
                        frontX, topY,
                        frontBulgeX, topY + plateHeight * 0.25f,
                        frontBulgeX, topY + plateHeight * 0.75f,
                        frontX, bottomY);

                    platePath.AddLine(frontX, bottomY, backX, bottomY);

                    platePath.AddBezier(
                        backX, bottomY,
                        backBulgeX, topY + plateHeight * 0.75f,
                        backBulgeX, topY + plateHeight * 0.25f,
                        backX, topY);

                    platePath.AddLine(backX, topY, frontX, topY);
                    platePath.CloseFigure();

                    Color glassDarkBase = Color.FromArgb(85, 190, 190, 190);
                    Color glassLightBase = Color.FromArgb(55, 235, 235, 235);

                    const float tintStrength = 0.45f;

                    Color glassDarkTinted = TintColor(glassDarkBase, MirrorOutlineColor, tintStrength);
                    Color glassLightTinted = TintColor(glassLightBase, MirrorOutlineColor, tintStrength);

                    using (LinearGradientBrush glassBrush = new LinearGradientBrush(
                        new Point(frontX, topY),
                        new Point(backX + bulge, topY),
                        glassDarkTinted,
                        glassLightTinted))
                    {
                        graphics.FillPath(glassBrush, platePath);
                    }
                }

                // ---- Secondary mirror: thicker, straight left edge + curved right edge ----
                using (GraphicsPath secPath = new GraphicsPath())
                {
                    PointF p0 = new PointF(backX, bottomY);
                    PointF p1 = new PointF(backBulgeX, topY + plateHeight * 0.75f);
                    PointF p2 = new PointF(backBulgeX, topY + plateHeight * 0.25f);
                    PointF p3 = new PointF(backX, topY);

                    float yTop = secondaryStripe.Top;
                    float yBottom = secondaryStripe.Bottom;

                    float SolveTForY(float yTarget)
                    {
                        float lo = 0f, hi = 1f;
                        for (int i = 0; i < 28; i++)
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
                    {
                        float tmp = tBottom;
                        tBottom = tTop;
                        tTop = tmp;
                    }

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

                    float leftEdgeX = secondaryStripe.Left;

                    secPath.StartFigure();
                    secPath.AddLine(leftEdgeX, yTop, leftEdgeX, yBottom);
                    secPath.AddLine(leftEdgeX, yBottom, cBottom.X, yBottom);
                    secPath.AddBezier(cBottom, p1, p2, cTop);
                    secPath.AddLine(cTop.X, yTop, leftEdgeX, yTop);
                    secPath.CloseFigure();

                    Color secDarkBase = Color.FromArgb(160, 160, 160);
                    Color secLightBase = Color.FromArgb(220, 220, 220);

                    const float secTintStrength = 0.10f;

                    Color secDarkTinted = TintColor(secDarkBase, MirrorOutlineColor, secTintStrength);
                    Color secLightTinted = TintColor(secLightBase, MirrorOutlineColor, secTintStrength);

                    RectangleF secBounds = secPath.GetBounds();

                    using (LinearGradientBrush secBrush = new LinearGradientBrush(
                        new PointF(secBounds.Left, secBounds.Top),
                        new PointF(secBounds.Right + 10f, secBounds.Top),
                        secLightTinted,
                        secDarkTinted))
                    {
                        graphics.FillPath(secBrush, secPath);
                    }
                }

                int axisStartX = mirrorBounds.Left + 2;
                int axisEndX = Math.Min(bounds.Right, axisStartX + OpticalAxisLength);

                using (Pen axisPen = new Pen(MirrorOutlineColor, 1.5f))
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

        private void DrawSctPrimaryMirror(Graphics graphics, Rectangle bounds)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return;

            Rectangle mirrorBounds = new Rectangle(
                bounds.X,
                bounds.Y,
                Math.Min(50, bounds.Width),
                Math.Min(100, bounds.Height));

            int topY = mirrorBounds.Top + 4;
            int bottomY = mirrorBounds.Bottom - 4;
            int backX = mirrorBounds.Left + 2;
            int concaveDepthX = mirrorBounds.Right - 25;
            int frontEdgeX = backX + 10;

            int maxRadius = Math.Max(0, Math.Min((concaveDepthX - backX) / 2, (bottomY - topY) / 2));
            int radius = Math.Min(CornerRadius, maxRadius);

            int axisY = mirrorBounds.Top + (mirrorBounds.Height / 2);

            // ---- Baffle geometry ----
            const int baffleWallLength = 40;

            int baffleSquareSize = Math.Max(6, Math.Min(16, mirrorBounds.Height / 6));

            Rectangle baffleCutout = new Rectangle(
                backX,
                axisY - (baffleSquareSize / 2),
                baffleSquareSize,
                baffleSquareSize);

            if (baffleCutout.Top < topY) baffleCutout.Y = topY;
            if (baffleCutout.Bottom > bottomY) baffleCutout.Y = bottomY - baffleCutout.Height;

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
                        topY + radius + 18,
                        frontEdgeX,
                        bottomY - radius - 18,
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
                        frontEdgeX, topY + 18,
                        frontEdgeX, bottomY - 18,
                        concaveDepthX, bottomY);
                    outlinePath.AddLine(concaveDepthX, bottomY, backX, bottomY);
                    outlinePath.AddLine(backX, bottomY, backX, topY);
                }

                outlinePath.CloseFigure();

                using (GraphicsPath fillPath = (GraphicsPath)outlinePath.Clone())
                {
                    fillPath.FillMode = FillMode.Alternate;
                    fillPath.AddRectangle(baffleCutout);

                    Color darkBase = Color.FromArgb(120, 120, 120);
                    Color lightBase = Color.FromArgb(186, 186, 186);

                    const float tintStrength = 0.15f;

                    Color darkTinted = TintColor(darkBase, MirrorOutlineColor, tintStrength);
                    Color lightTinted = TintColor(lightBase, MirrorOutlineColor, tintStrength);

                    using (LinearGradientBrush mirrorBrush = new LinearGradientBrush(
                        new Point(backX, topY),
                        new Point(concaveDepthX, topY),
                        darkTinted,
                        lightTinted))
                    {
                        graphics.FillPath(mirrorBrush, fillPath);
                    }
                }
            }

            int baffleStartX = backX;
            int baffleEndX = Math.Min(bounds.Right, baffleStartX + baffleWallLength);

            using (Pen bafflePen = new Pen(Color.DarkGray, 2f))
            {
                graphics.DrawLine(bafflePen, baffleStartX, baffleCutout.Top, baffleEndX, baffleCutout.Top);
                graphics.DrawLine(bafflePen, baffleStartX, baffleCutout.Bottom, baffleEndX, baffleCutout.Bottom);
            }

            int axisStartX = mirrorBounds.Left + 2;
            int axisEndX = Math.Min(bounds.Right, axisStartX + OpticalAxisLength);

            using (Pen axisPen = new Pen(MirrorOutlineColor, 1.5f))
            {
                axisPen.DashStyle = DashStyle.Dot;
                graphics.DrawLine(axisPen, axisStartX, axisY, axisEndX, axisY);
            }
        }

        private static Color TintColor(Color baseColor, Color tint, float strength)
        {
            strength = Math.Max(0f, Math.Min(1f, strength));

            int r = (int)(baseColor.R + (tint.R - baseColor.R) * strength);
            int g = (int)(baseColor.G + (tint.G - baseColor.G) * strength);
            int b = (int)(baseColor.B + (tint.B - baseColor.B) * strength);

            return Color.FromArgb(baseColor.A, r, g, b);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ResumeLayout(false);

        }
    }
}

