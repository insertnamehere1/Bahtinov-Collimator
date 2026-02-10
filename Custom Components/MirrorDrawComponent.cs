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
   //                 DrawMctSecondaryMirror(e.Graphics, ClientRectangle);
                    break;

                case MirrorType.SctPrimary:
                default:
                    DrawSctPrimaryMirror(e.Graphics, ClientRectangle);
                    break;
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

                    const float tintStrength = 0.35f;

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

            using (Pen bafflePen = new Pen(Color.LightGray, 2f))
            {
                graphics.DrawLine(bafflePen, baffleStartX, baffleCutout.Top, baffleEndX, baffleCutout.Top);
                graphics.DrawLine(bafflePen, baffleStartX, baffleCutout.Bottom, baffleEndX, baffleCutout.Bottom);
            }

            int axisStartX = mirrorBounds.Right - 30;
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

