using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    /// <summary>
    /// All user-facing strings used by SkyCalNextStep.
    /// Swap telescope models/languages by loading a different JSON pack at runtime.
    /// </summary>
    [DataContract]
    public sealed class NextStepTextPack
    {
        // Dialog / debug
        [DataMember] public string DialogTitle { get; set; }
        [DataMember] public string DebugLineFormat { get; set; } // e.g. "Values:  Red {0}   Green {1}   Blue {2}"

        // Common section title
        [DataMember] public string SectionWhatToDoTitle { get; set; }

        // Capture required
        [DataMember] public string CaptureHeader { get; set; }
        [DataMember] public string CaptureSummary { get; set; }
        [DataMember] public string CaptureBullet1 { get; set; }
        [DataMember] public string CaptureBullet2 { get; set; }

        // Focus only (TB not visible)
        [DataMember] public string FocusOnlyHeader { get; set; }
        [DataMember] public string FocusOnlyNeedsAdjustingSummary { get; set; }
        [DataMember] public string FocusOnlyGoodSummary { get; set; }
        [DataMember] public string FocusOnlyMoveInBullet1 { get; set; }
        [DataMember] public string FocusOnlyMoveInBullet2 { get; set; }
        [DataMember] public string FocusOnlyMoveOutBullet1 { get; set; }
        [DataMember] public string FocusOnlyMoveOutBullet2 { get; set; }
        [DataMember] public string FocusOnlyGoodBullet { get; set; }

        // Focus to balanced (TB visible but not balanced)
        [DataMember] public string FocusBalancedHeader { get; set; }
        [DataMember] public string FocusBalancedSummary { get; set; }
        [DataMember] public string FocusAllPositiveBullet { get; set; }
        [DataMember] public string FocusAllNegativeBullet { get; set; }
        [DataMember] public string FocusStraddleNotBalancedBullet1 { get; set; }
        [DataMember] public string FocusStraddleNotBalancedBullet2Format { get; set; } // direction placeholder {0} = IN/OUT
        [DataMember] public string FocusTargetLineFormat { get; set; } // {0}=mean string, {1}=maxAbs string
        [DataMember] public string FocusOnlyBeginScrewsBullet { get; set; }
        [DataMember] public string FocusFooterHint { get; set; }

        // Collimation step header/summary
        [DataMember] public string CollimationHeader { get; set; }
        [DataMember] public string CollimationSummary { get; set; }

        // Done (within tolerance)
        [DataMember] public string DoneHeader { get; set; }
        [DataMember] public string DoneSummary { get; set; }
        [DataMember] public string DoneBullet { get; set; }

        // Collimation instruction block
        [DataMember] public string CollimationIntroBullet { get; set; }
        [DataMember] public string CollimationPositiveInstructionFormat { get; set; } // {0}=channel name, {1}=value, {2}=screw label
        [DataMember] public string CollimationNegativeInstructionFormat { get; set; } // {0}=channel name, {1}=value, {2}=screw label
        [DataMember] public string CollimationAdjustTogetherBullet { get; set; }
        [DataMember] public string CollimationRepeatCycleBullet { get; set; }

        // Focus direction words
        [DataMember] public string DirectionIn { get; set; }
        [DataMember] public string DirectionOut { get; set; }

        // Screw fallback label
        [DataMember] public string ScrewFallbackFormat { get; set; } // {0}=group name
    }

    /// <summary>
    /// Holds the currently loaded text pack. Load at startup or after telescope selection.
    /// </summary>
    public static class NextStepText
    {
        private static NextStepTextPack _current;

        /// <summary>
        /// Current text pack. Must be loaded before SkyCalNextStep runs, otherwise an exception is thrown.
        /// </summary>
        public static NextStepTextPack Current
        {
            get
            {
                if (_current == null)
                    throw new InvalidOperationException("NextStepText.Current is null. Call NextStepText.LoadFromJson(...) at startup.");
                return _current;
            }
        }

        /// <summary>
        /// Loads a text pack from a JSON file. Throws if file missing or invalid.
        /// </summary>
        public static void LoadFromJson(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("filePath is null/empty.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Text pack JSON not found.", filePath);

            using (var fs = File.OpenRead(filePath))
            {
                var ser = new DataContractJsonSerializer(typeof(NextStepTextPack));

                var obj = ser.ReadObject(fs) as NextStepTextPack
                    ?? throw new SerializationException("Failed to deserialize NextStepTextPack from JSON.");

                _current = obj;
            }
        }
    }
}

