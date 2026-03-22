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
    /// Line art: <see cref="MctSecondaryMirrorLineArt"/> (MCT), <see cref="SctPrimaryMirrorLineArt"/> (SCT).
    /// </summary>
    public partial class MirrorDrawingComponent : Control
    {
        /// <summary>Authoring size for mirror geometry (96-DPI logical units). Drawing is scaled uniformly to <see cref="ClientRectangle"/>.</summary>
        private const int DesignWidth = 50;
        private const int DesignHeight = 100;

        private int cornerRadius = 1;
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

            Rectangle client = ClientRectangle;
            if (client.Width <= 0 || client.Height <= 0)
                return;

            // Uniform scale from fixed 50×100 design space — preserves proportions at any control size / DPI.
            float scale = Math.Min(client.Width / (float)DesignWidth, client.Height / (float)DesignHeight);
            float scaledW = DesignWidth * scale;
            float scaledH = DesignHeight * scale;
            float ox = client.X + (client.Width - scaledW) / 2f;
            float oy = client.Y + (client.Height - scaledH) / 2f;

            GraphicsState state = e.Graphics.Save();
            try
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                e.Graphics.TranslateTransform(ox, oy);
                e.Graphics.ScaleTransform(scale, scale);

                var designBounds = new Rectangle(0, 0, DesignWidth, DesignHeight);
                switch (MirrorType)
                {
                    case MirrorType.MctSecondary:
                        MctSecondaryMirrorLineArt.Draw(e.Graphics, designBounds, MirrorOutlineColor, OpticalAxisLength);
                        break;

                    case MirrorType.SctPrimary:
                    default:
                        SctPrimaryMirrorLineArt.Draw(e.Graphics, designBounds, MirrorOutlineColor, OpticalAxisLength, CornerRadius);
                        break;
                }
            }
            finally
            {
                e.Graphics.Restore(state);
            }
        }

        /// <inheritdoc />
        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);
            Invalidate();
        }

    }
}

