using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Bahtinov_Collimator
{
    /// <summary>
    /// Identifies the guidance category used by the next-step workflow.
    /// </summary>
    public enum NextStepKind
    {
        None = 0,
        Focus = 1,
        Collimation = 2,
        Done = 3,
        NeedsCapture = 4
    }

    /// <summary>
    /// Identifies the icon semantic used when rendering next-step guidance.
    /// </summary>
    public enum NextStepIcon
    {
        None = 0,
        Info = 1,
        Warning = 2,
        Focus = 3,
        Collimation = 4,
        Success = 5
    }

    /// <summary>
    /// Represents structured content for the next-step guidance dialog.
    /// </summary>
    public sealed class NextStepGuidance
    {
        #region Content Properties

        public string DialogTitle { get; set; }
        public string Header { get; set; }
        public string Summary { get; set; }

        public NextStepKind Kind { get; set; }
        public NextStepIcon Icon { get; set; }

        public List<GuidanceSection> Sections { get; } = new List<GuidanceSection>();

        public string FooterHint { get; set; }
        public string DebugLine { get; set; }

        #endregion
    }

    /// <summary>
    /// Represents a titled section in the guidance content with optional bullet lines and formatting hints.
    /// </summary>
    public sealed class GuidanceSection
    {
        #region Content Properties

        public string Title { get; set; }
        public List<StringBoolPair> Lines { get; } = new List<StringBoolPair>();

        public bool EmphasizeTitle { get; set; }
        public bool IsSubSection { get; set; }
        public bool IsSafetyNote { get; set; }

        public Image InlineImage { get; set; }

        #endregion
    }

    /// <summary>
    /// Represents a text line and whether it should be rendered as a bullet item.
    /// </summary>
    public class StringBoolPair
    {
        #region Content Properties

        public string Text { get; set; }
        public bool Bullet { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new text and bullet pair.
        /// </summary>
        /// <param name="text">Line text content.</param>
        /// <param name="bullet">True when the line should be displayed as a bullet item.</param>
        public StringBoolPair(string text, bool bullet)
        {
            Text = text;
            Bullet = bullet;
        }

        #endregion
    }
}

