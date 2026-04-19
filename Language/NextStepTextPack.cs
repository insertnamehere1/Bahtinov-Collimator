using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Bahtinov_Collimator
{
    /// <summary>
    /// All user-facing strings used by SkyCalNextStep.
    /// Swap telescope models/languages by loading a different JSON pack at runtime.
    /// </summary>
    [DataContract]
    public sealed class NextStepTextPack
    {
        #region Next Step Text Keys

        [DataMember] public string DialogTitle { get; set; }
        [DataMember] public string DebugLineFormat { get; set; }

        [DataMember] public string SectionWhatToDoTitle { get; set; }

        [DataMember] public string CaptureHeader { get; set; }
        [DataMember] public string CaptureSummary { get; set; }
        [DataMember] public string CaptureBullet1 { get; set; }
        [DataMember] public string CaptureBullet2 { get; set; }

        [DataMember] public string FocusOnlyHeader { get; set; }
        [DataMember] public string FocusOnlyNeedsAdjustingSummary { get; set; }

        [DataMember] public string FocusBalancedHeader { get; set; }
        [DataMember] public string FocusBalancedSummary { get; set; }
        [DataMember] public string FocusAllPositiveBullet { get; set; }
        [DataMember] public string FocusAllNegativeBullet { get; set; }
        [DataMember] public string FocusStraddleNotBalancedBullet1 { get; set; }
        [DataMember] public string FocusStraddleNotBalancedBullet2Format { get; set; }
        [DataMember] public string FocusTargetLineFormat { get; set; }
        [DataMember] public string FocusOnlyBeginScrewsBullet { get; set; }
        [DataMember] public string FocusFooterHint { get; set; }

        [DataMember] public string CollimationHeader { get; set; }
        [DataMember] public string CollimationSummary { get; set; }

        [DataMember] public string DoneHeader { get; set; }
        [DataMember] public string DoneSummary { get; set; }
        [DataMember] public string DoneBullet { get; set; }

        [DataMember] public string CollimationPositiveInstructionFormat { get; set; }
        [DataMember] public string CollimationNegativeInstructionFormat { get; set; }
        [DataMember] public string CollimationAdjustTogetherBullet { get; set; }
        [DataMember] public string CollimationFooterHint { get; set; }

        [DataMember] public string DirectionIn { get; set; }
        [DataMember] public string DirectionOut { get; set; }

        [DataMember] public string CollimationSpace { get; set; }

        [DataMember] public string ScrewFallbackFormat { get; set; }

        [DataMember] public string ChannelRed { get; set; }
        [DataMember] public string ChannelGreen { get; set; }
        [DataMember] public string ChannelBlue { get; set; }

        [DataMember] public string CalibrationTitle { get; set; }
        [DataMember] public string CalibrationQuitButtonText { get; set; }
        [DataMember] public string CalibrationCloseButtonText { get; set; }
        [DataMember] public string CalibrationObjectiveHeader { get; set; }
        [DataMember] public string CalibrationStep1Title { get; set; }
        [DataMember] public string CalibrationStep1Bullet1 { get; set; }
        [DataMember] public string CalibrationStep1Bullet2 { get; set; }
        [DataMember] public string CalibrationStep1Warning { get; set; }
        [DataMember] public string CalibrationWaitingForImage { get; set; }
        [DataMember] public string CalibrationAimHeader { get; set; }
        [DataMember] public string CalibrationStep2Title { get; set; }
        [DataMember] public string CalibrationStep2Bullet1 { get; set; }
        [DataMember] public string CalibrationStep2Bullet2 { get; set; }
        [DataMember] public string CalibrationAutoCompleteLine { get; set; }
        [DataMember] public string CalibrationMovePrompt { get; set; }
        [DataMember] public string CalibrationCompleteHeader { get; set; }
        [DataMember] public string CalibrationCompleteSummary { get; set; }

        #endregion
    }

    /// <summary>
    /// Holds the currently loaded text pack. Load at startup or after telescope selection.
    /// </summary>
    public static class NextStepText
    {
        #region Fields

        private static NextStepTextPack current;

        #endregion

        #region Public API

        /// <summary>
        /// Current text pack. Must be loaded before SkyCalNextStep runs, otherwise an exception is thrown.
        /// </summary>
        public static NextStepTextPack Current
        {
            get
            {
                if (current == null)
                    throw new InvalidOperationException("NextStepText.Current is null. Call NextStepText.LoadFromJson(...) at startup.");
                return current;
            }
        }

        /// <summary>
        /// Loads a text pack from a JSON file. Throws if file missing or invalid.
        /// </summary>
        /// <param name="filePath">Full path to the next-step text JSON file.</param>
        public static void LoadFromJson(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("filePath is null/empty.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Text pack JSON not found.", filePath);

            string json = File.ReadAllText(filePath, Encoding.UTF8);
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                var ser = new DataContractJsonSerializer(typeof(NextStepTextPack));

                var obj = ser.ReadObject(ms) as NextStepTextPack
                    ?? throw new SerializationException("Failed to deserialize NextStepTextPack from JSON.");

                current = obj;
            }
        }

        #endregion
    }
}
