using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Bahtinov_Collimator
{
    public enum NextStepKind
    {
        None = 0,
        Focus = 1,
        Collimation = 2,
        Done = 3,
        NeedsCapture = 4
    }

    public enum NextStepIcon
    {
        None = 0,
        Info = 1,
        Warning = 2,
        Focus = 3,
        Collimation = 4,
        Success = 5
    }

    public sealed class NextStepGuidance
    {
        public string DialogTitle { get; set; }
        public string Header { get; set; }
        public string Summary { get; set; }

        public NextStepKind Kind { get; set; }
        public NextStepIcon Icon { get; set; }

        public List<GuidanceSection> Sections { get; } = new List<GuidanceSection>();

        public string FooterHint { get; set; }
        public string DebugLine { get; set; }

        public string ToPlainText()
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(Header))
                sb.AppendLine(Header).AppendLine();

            if (!string.IsNullOrWhiteSpace(Summary))
                sb.AppendLine(Summary).AppendLine();

            foreach (var s in Sections)
            {
                if (!string.IsNullOrWhiteSpace(s.Title))
                    sb.AppendLine(s.Title);

                foreach (var b in s.Bullets)
                    sb.AppendLine("• " + b);

                sb.AppendLine();
            }

            if (!string.IsNullOrWhiteSpace(FooterHint))
                sb.AppendLine(FooterHint);

            if (!string.IsNullOrWhiteSpace(DebugLine))
                sb.AppendLine().AppendLine(DebugLine);

            return sb.ToString();
        }
    }

    public sealed class GuidanceSection
    {
        // Text
        public string Title { get; set; }
        public List<string> Bullets { get; } = new List<string>();

        // Semantic intent (NO UI types)
        public bool EmphasizeTitle { get; set; }     // major step
        public bool IsSubSection { get; set; }       // secondary step
        public bool IsSafetyNote { get; set; }       // warnings / care notes

        // Optional inline image (logic provides, UI renders)
        public Image InlineImage { get; set; }
    }
}

