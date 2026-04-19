using System;
using Bahtinov_Collimator;
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
        #region Constants

        /// <summary>Authoring size for mirror geometry (96-DPI logical units). Drawing is scaled uniformly to <see cref="ClientRectangle"/>.</summary>
        private const int DesignWidth = 100;
        private const int DesignHeight = 100;

        #endregion

        #region Fields

        private int cornerRadius = 1;
        private int opticalAxisLength = 100;
        private Color mirrorOutlineColor = UITheme.MirrorOutline;
        private MirrorType mirrorType = MirrorType.SctPrimary;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MirrorDrawingComponent"/> class.
        /// </summary>
        public MirrorDrawingComponent()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);

            Size = new Size(100, 100);
            BackColor = UITheme.MirrorViewBackground;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets which mirror profile is rendered by the control.
        /// </summary>
        public MirrorType MirrorType
        {
            get => mirrorType;
            set
            {
                mirrorType = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the corner radius used by the SCT profile geometry.
        /// </summary>
        public int CornerRadius
        {
            get => cornerRadius;
            set
            {
                cornerRadius = Math.Max(0, value);
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the length, in logical design units, of the dotted optical axis.
        /// </summary>
        public int OpticalAxisLength
        {
            get => opticalAxisLength;
            set
            {
                opticalAxisLength = Math.Max(0, value);
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the outline color used for the active mirror profile.
        /// </summary>
        public Color MirrorOutlineColor
        {
            get => mirrorOutlineColor;
            set
            {
                mirrorOutlineColor = value;
                Invalidate();
            }
        }

        #endregion

        #region Painting

        /// <summary>
        /// Paints the selected mirror profile scaled to the current client bounds.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Rectangle client = ClientRectangle;
            if (client.Width <= 0 || client.Height <= 0)
                return;

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

        #endregion

        #region DPI

        /// <inheritdoc />
        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);
            Invalidate();
        }

        #endregion

    }
}

