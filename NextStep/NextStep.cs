using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Bahtinov_Collimator
{
    /// <summary>
    /// Generates structured “next step” guidance for SkyCal.
    /// All user-visible wording is sourced from <see cref="NextStepText.Current"/> (JSON text pack).
    /// </summary>
    public static class NextStep
    {
        /// <summary>
        /// Creates the guidance content for the "What should I do next?" dialog based on:
        /// capture state, Tri-Bahtinov visibility, channel values, tolerance, and screw labels.
        /// </summary>
        public static NextStepGuidance GetNextStepGuidance(
            bool capturing,
            bool triBahtinovVisible,
            double red,
            double green,
            double blue,
            double tolerance,
            IDictionary<string, string> groupToScrewLabel)
        {
            var T = NextStepText.Current;
            var inv = CultureInfo.InvariantCulture;

            var g = new NextStepGuidance
            {
                DialogTitle = T.DialogTitle,
                DebugLine = string.Format(inv, T.DebugLineFormat, Format(red, inv), Format(green, inv), Format(blue, inv))
            };

            var channels = new[]
            {
                new Channel(T.ChannelRed, red),
                new Channel(T.ChannelGreen, green),
                new Channel(T.ChannelBlue, blue)
            };

            if (!capturing)
            {
                g.Kind = NextStepKind.NeedsCapture;
                g.Icon = NextStepIcon.Warning;
                g.Header = T.CaptureHeader;
                g.Summary = T.CaptureSummary;

                var s = new GuidanceSection { Title = T.SectionWhatToDoTitle, EmphasizeTitle = true };
                s.Lines.Add(new StringBoolPair(T.CaptureBullet1, true));
                s.Lines.Add(new StringBoolPair(T.CaptureBullet2, true));
                g.Sections.Add(s);

                return g;
            }

            // If TB mask is not visible, this is focus-only behaviour
            if (!triBahtinovVisible)
                return FocusOnlyGuidance(g);

            // STEP 1: Focus until values are BALANCED around 0
            if (!IsBalancedAroundZero(channels))
                return FocusToBalancedGuidance(g, channels);

            // STEP 2: Balanced -> collimation
            return BalancedCollimationGuidance(g, channels, tolerance, groupToScrewLabel, inv);
        }

        /// <summary>
        /// Determines whether channel values satisfy any of the balance tests (signed-sum, pair-vs-single, extremes).
        /// </summary>
        private static bool IsBalancedAroundZero(Channel[] channels)
        {
            double signTolerance = 0.3;     // tolerance must be quite close to 0
            double pvsTolerance = 0.5;
            double largestTolerance = 0.5;

            bool balanceSigned = ComputeSignedBalanceError(channels, signTolerance);
            bool balancePvs = IsPairVsSingleBalanced(channels, pvsTolerance);
            bool balanceLargest = IsLargestPosNegBalanced(channels, largestTolerance);
            
            return balanceSigned || balancePvs || balanceLargest;
        }

        /// <summary>
        /// Balance test based on symmetry of summed positive and negative magnitudes.
        /// </summary>
        private static bool ComputeSignedBalanceError(Channel[] channels, double tolerance)
        {
            double positiveSum = channels
                .Where(c => c.Value > 0)
                .Sum(c => c.Value);

            double negativeSum = channels
                .Where(c => c.Value < 0)
                .Sum(c => -c.Value); // absolute

            double total = positiveSum + negativeSum;

            if (total <= double.Epsilon)
                return true; // perfectly centered

            return Math.Abs(positiveSum - negativeSum) <= tolerance;
        }

        /// <summary>
        /// Balance test for a 2-vs-1 sign split: compares average magnitude of the pair to the single opposite magnitude.
        /// </summary>
        private static bool IsPairVsSingleBalanced(Channel[] channels, double tolerance)
        {
            var positives = channels.Where(c => c.Value > 0).ToList();
            var negatives = channels.Where(c => c.Value < 0).ToList();

            // Must be 2 on one side, 1 on the other
            if (positives.Count == 2 && negatives.Count == 1)
            {
                double avgPair = positives.Average(c => c.Value);
                double single = -negatives[0].Value; // abs
                return Math.Abs(avgPair - single) <= tolerance;
            }

            if (negatives.Count == 2 && positives.Count == 1)
            {
                double avgPair = negatives.Average(c => -c.Value); // abs
                double single = positives[0].Value;
                return Math.Abs(avgPair - single) <= tolerance;
            }

            // All same sign or zero-heavy case → not balanced
            return false;
        }

        /// <summary>
        /// Balance test comparing extremes: largest positive vs absolute of most-negative.
        /// </summary>
        private static bool IsLargestPosNegBalanced(Channel[] channels, double tolerance)
        {
            double maxPositive = channels
                .Where(c => c.Value > 0)
                .Select(c => c.Value)
                .DefaultIfEmpty(double.NaN)
                .Max();

            double minNegative = channels
                .Where(c => c.Value < 0)
                .Select(c => c.Value)
                .DefaultIfEmpty(double.NaN)
                .Min();

            if (double.IsNaN(maxPositive) || double.IsNaN(minNegative))
                return false;

            return Math.Abs(maxPositive - Math.Abs(minNegative)) <= tolerance;
        }

        /// <summary>
        /// Guidance shown when TB is visible but focus is not yet balanced around zero.
        /// </summary>
        private static NextStepGuidance FocusToBalancedGuidance(NextStepGuidance g, Channel[] channels)
        {
            var T = NextStepText.Current;

            g.Kind = NextStepKind.Focus;
            g.Icon = NextStepIcon.Focus;
            g.Header = T.FocusBalancedHeader;
            g.Summary = T.FocusBalancedSummary;

            double mean = channels.Average(c => c.Value);
            double maxAbs = channels.Max(c => Math.Abs(c.Value));

            var s = new GuidanceSection { Title = T.SectionWhatToDoTitle, EmphasizeTitle = true };

            if (channels.All(c => c.Value > 0))
            {
                s.Lines.Add(new StringBoolPair(T.FocusAllPositiveBullet, true));
            }
            else if (channels.All(c => c.Value < 0))
            {
                s.Lines.Add(new StringBoolPair(T.FocusAllNegativeBullet, true));
            }
            else
            {
                s.Lines.Add(new StringBoolPair(T.FocusStraddleNotBalancedBullet1, true));

                string dir = mean > 0.0 ? T.DirectionIn : T.DirectionOut;
                s.Lines.Add(new StringBoolPair(string.Format(CultureInfo.InvariantCulture, T.FocusStraddleNotBalancedBullet2Format, dir), true));
            }

            string meanStr = mean.ToString("+0.0;-0.0;0.0", CultureInfo.InvariantCulture);
            string maxAbsStr = maxAbs.ToString("0.0", CultureInfo.InvariantCulture);

            s.Lines.Add(new StringBoolPair(string.Format(CultureInfo.InvariantCulture, T.FocusTargetLineFormat, meanStr, maxAbsStr), true));
            s.Lines.Add(new StringBoolPair(T.FocusOnlyBeginScrewsBullet, true));

            g.Sections.Add(s);
            g.FooterHint = T.FocusFooterHint;

            return g;
        }

        /// <summary>
        /// Focus-only guidance used when TB is not visible; uses Red channel as the decision input.
        /// </summary>
        private static NextStepGuidance FocusOnlyGuidance(NextStepGuidance g)
        {
            var T = NextStepText.Current;

            g.Kind = NextStepKind.Focus;
            g.Icon = NextStepIcon.Focus;
            g.Header = T.FocusOnlyHeader;
            g.Summary = T.FocusOnlyNeedsAdjustingSummary;
            return g;
        }

        /// <summary>
        /// Produces balanced collimation guidance and builds screw instructions without changing decision logic.
        /// </summary>
        private static NextStepGuidance BalancedCollimationGuidance(
            NextStepGuidance g,
            Channel[] channels,
            double tolerance,
            IDictionary<string, string> groupToScrewLabel,
            CultureInfo inv)
        {
            var T = NextStepText.Current;

            g.Kind = NextStepKind.Collimation;
            g.Icon = NextStepIcon.Collimation;
            g.Header = T.CollimationHeader;
            g.Summary = T.CollimationSummary;

            var positives = channels.Where(c => c.Value > 0).OrderByDescending(c => c.Value).ToList();
            var negatives = channels.Where(c => c.Value < 0).OrderBy(c => c.Value).ToList();

            // If we're basically done
            if (channels.All(c => Math.Abs(c.Value) <= tolerance))
            {
                g.Kind = NextStepKind.Done;
                g.Icon = NextStepIcon.Success;
                g.Header = T.DoneHeader;
                g.Summary = T.DoneSummary;

                var sDone = new GuidanceSection { Title = T.SectionWhatToDoTitle, EmphasizeTitle = true };
                sDone.Lines.Add(new StringBoolPair(T.DoneBullet, true));
                g.Sections.Add(sDone);

                return g;
            }

            // One positive, two negative
            if (positives.Count == 1 && negatives.Count >= 2)
            {
                return BuildBalancedInstructionGuidance(
                    g,
                    tighten: new[] { negatives[0], negatives[1] },
                    loosen: new[] { positives[0] },
                    groupToScrewLabel,
                    inv);
            }

            // One negative, two positive
            if (negatives.Count == 1 && positives.Count >= 2)
            {
                return BuildBalancedInstructionGuidance(
                    g,
                    tighten: new[] { negatives[0] },
                    loosen: new[] { positives[0], positives[1] },
                    groupToScrewLabel,
                    inv);
            }

            // Fallback: strongest imbalance + counterbalance
            var worst = channels.OrderByDescending(c => Math.Abs(c.Value)).First();
            var counter = channels
                .Where(c => c.Name != worst.Name)
                .OrderByDescending(c => Math.Abs(c.Value))
                .First();

            if (worst.Value > 0)
            {
                return BuildBalancedInstructionGuidance(
                    g,
                    tighten: new[] { counter },
                    loosen: new[] { worst },
                    groupToScrewLabel,
                    inv);
            }

            return BuildBalancedInstructionGuidance(
                g,
                tighten: new[] { worst },
                loosen: new[] { counter },
                groupToScrewLabel,
                inv);
        }

        /// <summary>
        /// Formats the collimation instruction text for the chosen tighten/loosen channel sets.
        /// </summary>
        private static NextStepGuidance BuildBalancedInstructionGuidance(
            NextStepGuidance g,
            Channel[] tighten,
            Channel[] loosen,
            IDictionary<string, string> map,
            CultureInfo inv)
        {
            var T = NextStepText.Current;
            var s = new GuidanceSection { Title = T.SectionWhatToDoTitle, EmphasizeTitle = true };

            foreach (var c in loosen)
            {
                string screw = Screw(c.Name, map);
                s.Lines.Add(new StringBoolPair(string.Format(inv, T.CollimationPositiveInstructionFormat, c.Name, Format(c.Value, inv), screw), true));
            }

            foreach (var c in tighten)
            {
                string screw = Screw(c.Name, map);
                s.Lines.Add(new StringBoolPair(string.Format(inv, T.CollimationNegativeInstructionFormat, c.Name, Format(c.Value, inv), screw), true));
            }
            s.Lines.Add(new StringBoolPair(T.CollimationAdjustTogetherBullet, false));
            g.FooterHint = T.CollimationFooterHint;

            g.Sections.Add(s);
            return g;
        }

        /// <summary>
        /// Returns a screw label for the given channel/group name using the provided mapping, or a fallback format.
        /// </summary>
        private static string Screw(string group, IDictionary<string, string> map)
        {
            var T = NextStepText.Current;

            if (map != null && map.TryGetValue(group, out var label) && !string.IsNullOrWhiteSpace(label))
                return label;

            return string.Format(CultureInfo.InvariantCulture, T.ScrewFallbackFormat, group);
        }

        /// <summary>
        /// Formats a numeric channel value as signed fixed-point with one decimal place.
        /// </summary>
        private static string Format(double v, CultureInfo inv)
        {
            return (v >= 0 ? "+" : "") + v.ToString("0.0", inv);
        }

        private readonly struct Channel
        {
            public Channel(string name, double value)
            {
                Name = name;
                Value = value;
            }

            public string Name { get; }
            public double Value { get; }
        }
    }
}
