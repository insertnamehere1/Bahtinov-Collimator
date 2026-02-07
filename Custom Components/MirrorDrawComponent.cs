using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Bahtinov_Collimator.Custom_Components
{
    public enum MirrorType
    {
        SctPrimary
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
            int concaveDepthX = mirrorBounds.Right - 20;
            int frontEdgeX = backX + 10;
            int maxRadius = Math.Max(0, Math.Min((concaveDepthX - backX) / 2, (bottomY - topY) / 2));
            int radius = Math.Min(CornerRadius, maxRadius);

            using (GraphicsPath mirrorPath = new GraphicsPath())
            {
                mirrorPath.StartFigure();

                if (radius > 0)
                {
                    int d = radius * 2;

                    mirrorPath.AddLine(backX + radius, topY, concaveDepthX - radius, topY);
                    mirrorPath.AddArc(concaveDepthX - d, topY, d, d, 270f, 90f);
                    mirrorPath.AddBezier(
                        concaveDepthX,
                        topY + radius,
                        frontEdgeX,
                        topY + radius + 18,
                        frontEdgeX,
                        bottomY - radius - 18,
                        concaveDepthX,
                        bottomY - radius);
                    mirrorPath.AddArc(concaveDepthX - d, bottomY - d, d, d, 0f, 90f);
                    mirrorPath.AddLine(concaveDepthX - radius, bottomY, backX + radius, bottomY);
                    mirrorPath.AddArc(backX, bottomY - d, d, d, 90f, 90f);
                    mirrorPath.AddLine(backX, bottomY - radius, backX, topY + radius);
                    mirrorPath.AddArc(backX, topY, d, d, 180f, 90f);
                }
                else
                {
                    mirrorPath.AddLine(backX, topY, concaveDepthX, topY);
                    mirrorPath.AddBezier(concaveDepthX, topY, frontEdgeX, topY + 18, frontEdgeX, bottomY - 18, concaveDepthX, bottomY);
                    mirrorPath.AddLine(concaveDepthX, bottomY, backX, bottomY);
                    mirrorPath.AddLine(backX, bottomY, backX, topY);
                }

                mirrorPath.CloseFigure();

                using (LinearGradientBrush mirrorBrush = new LinearGradientBrush(
                    new Point(backX, topY),
                    new Point(concaveDepthX, topY),
                    Color.FromArgb(120, 120, 120),
                    Color.FromArgb(186, 186, 186)))
                {
                    graphics.FillPath(mirrorBrush, mirrorPath);
                }

                using (Pen mirrorOutlinePen = new Pen(MirrorOutlineColor, 2f))
                {
                    graphics.DrawPath(mirrorOutlinePen, mirrorPath);
                }

                using (Pen backHighlightPen = new Pen(Color.FromArgb(220, 220, 220), 1.5f))
                {
                    graphics.DrawLine(backHighlightPen, backX + 1, topY + 1, backX + 1, bottomY - 1);
                }
            }

            int axisY = mirrorBounds.Top + (mirrorBounds.Height / 2);
            int axisStartX = mirrorBounds.Right - 30;
            int axisEndX = Math.Min(bounds.Right, axisStartX + OpticalAxisLength);

            using (Pen axisPen = new Pen(MirrorOutlineColor, 1.5f))
            {
                axisPen.DashStyle = DashStyle.Dot;
                graphics.DrawLine(axisPen, axisStartX, axisY, axisEndX, axisY);
            }
            this.BackColor = UITheme.DarkBackground;
        }
    }
}
