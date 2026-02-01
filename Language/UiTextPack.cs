using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Bahtinov_Collimator
{
    /// <summary>
    /// All user-facing strings for the main SkyCal UI.
    /// Swap languages by loading a different JSON pack at runtime.
    /// </summary>
    [DataContract]
    public sealed class UiTextPack
    {
        [DataMember] public string CommonOk { get; set; }
        [DataMember] public string CommonCancel { get; set; }
        [DataMember] public string CommonYes { get; set; }
        [DataMember] public string CommonNo { get; set; }

        [DataMember] public string AppTitle { get; set; }
        [DataMember] public string AppVersionFormat { get; set; }
        [DataMember] public string MenuFile { get; set; }
        [DataMember] public string MenuQuit { get; set; }
        [DataMember] public string MenuSetup { get; set; }
        [DataMember] public string MenuSettings { get; set; }
        [DataMember] public string MenuCalibration { get; set; }
        [DataMember] public string MenuHelp { get; set; }
        [DataMember] public string MenuUserManual { get; set; }
        [DataMember] public string MenuCheckUpdates { get; set; }
        [DataMember] public string MenuAbout { get; set; }
        [DataMember] public string MenuSupportSkyCal { get; set; }
        [DataMember] public string MenuWhatDoIDoNext { get; set; }
        [DataMember] public string StartButtonSelectStar { get; set; }
        [DataMember] public string StartButtonStop { get; set; }
        [DataMember] public string LabelBahtinov { get; set; }
        [DataMember] public string LabelDefocus { get; set; }
        [DataMember] public string ScrewRedLabel { get; set; }
        [DataMember] public string ScrewGreenLabel { get; set; }
        [DataMember] public string ScrewBlueLabel { get; set; }

        [DataMember] public string SettingsTitle { get; set; }
        [DataMember] public string SettingsGroupVoiceTitle { get; set; }
        [DataMember] public string SettingsVoiceGuidanceLabel { get; set; }
        [DataMember] public string SettingsVoiceGuidanceDescription { get; set; }
        [DataMember] public string SettingsGroupGuidanceTitle { get; set; }
        [DataMember] public string SettingsGuidanceNewtonian { get; set; }
        [DataMember] public string SettingsGuidanceMakCass { get; set; }
        [DataMember] public string SettingsGuidanceSct { get; set; }
        [DataMember] public string SettingsGroupCalibrationTitle { get; set; }
        [DataMember] public string SettingsCalibrationSignSwitch { get; set; }
        [DataMember] public string SettingsCalibrationDescription { get; set; }
        [DataMember] public string SettingsGroupHistoryTitle { get; set; }
        [DataMember] public string SettingsHistoryMarkersLabel { get; set; }
        [DataMember] public string SettingsHistoryMarkersDescription { get; set; }
        [DataMember] public string SettingsSaveButton { get; set; }
        [DataMember] public string SettingsCancelButton { get; set; }
        [DataMember] public string SettingsInvalidInputMessage { get; set; }
        [DataMember] public string SettingsInvalidInputTitle { get; set; }

        [DataMember] public string FocusGroupRed { get; set; }
        [DataMember] public string FocusGroupGreen { get; set; }
        [DataMember] public string FocusGroupBlue { get; set; }
        [DataMember] public string FocusLabelFarAway { get; set; }
        [DataMember] public string FocusLabelClose { get; set; }
        [DataMember] public string FocusLabelOk { get; set; }
        [DataMember] public string FocusLabelToo { get; set; }

        [DataMember] public string AboutDialogTitleFormat { get; set; }
        [DataMember] public string AboutVersionFormat { get; set; }
        [DataMember] public string AboutDescription { get; set; }

        [DataMember] public string StartupDialogTitle { get; set; }
        [DataMember] public string StartupDialogHeading { get; set; }
        [DataMember] public string StartupDialogBody { get; set; }
        [DataMember] public string StartupDialogDontShowAgain { get; set; }
        [DataMember] public string StartupDialogYesButton { get; set; }
        [DataMember] public string StartupDialogNotNowButton { get; set; }

        [DataMember] public string DonateTitle { get; set; }
        [DataMember] public string DonateCloseButton { get; set; }
        [DataMember] public string DonateHeaderText { get; set; }
        [DataMember] public string DonateRichTextOnline { get; set; }
        [DataMember] public string DonateRichTextOfflineFormat { get; set; }
        [DataMember] public string DonatePaypalDescription { get; set; }
        [DataMember] public string DonatePaypalOptionalMessage { get; set; }

        [DataMember] public string HelpOpenErrorMessageFormat { get; set; }
        [DataMember] public string HelpOpenErrorTitle { get; set; }
        [DataMember] public string LanguageFileMissingMessageFormat { get; set; }
        [DataMember] public string LanguageFileMissingTitle { get; set; }

        [DataMember] public string ImageCaptureDefocusNotDetectedMessage { get; set; }
        [DataMember] public string ImageCaptureDefocusNotDetectedTitle { get; set; }
        [DataMember] public string ImageCaptureInvalidSelectionMessage { get; set; }
        [DataMember] public string ImageCaptureInvalidSelectionTitle { get; set; }
        [DataMember] public string ImageCaptureSelectionTooSmallMessage { get; set; }
        [DataMember] public string ImageCaptureSelectionTooSmallTitle { get; set; }
        [DataMember] public string ImageCaptureNoAppFoundMessage { get; set; }
        [DataMember] public string ImageCaptureNoAppFoundTitle { get; set; }
        [DataMember] public string ImageCaptureImageLostMessage { get; set; }
        [DataMember] public string ImageCaptureGetImageTitle { get; set; }
        [DataMember] public string ImageCaptureTargetMinimizedMessage { get; set; }
        [DataMember] public string ImageCaptureSelectionTooSmallForGetImageMessage { get; set; }
        [DataMember] public string ImageCaptureUnableCaptureMessage { get; set; }
        [DataMember] public string ImageCaptureFailedMessage { get; set; }

        [DataMember] public string BahtinovLineDetectionFailedMessage { get; set; }
        [DataMember] public string BahtinovLineDetectionFailedTitle { get; set; }
        [DataMember] public string BahtinovIntersectionNotDetectedMessage { get; set; }
        [DataMember] public string BahtinovIntersectionNotDetectedTitle { get; set; }
        [DataMember] public string BahtinovLinesNotDetectedMessage { get; set; }
        [DataMember] public string BahtinovLinesNotDetectedTitle { get; set; }
        [DataMember] public string BahtinovImageLostMessage { get; set; }
        [DataMember] public string BahtinovImageLostTitle { get; set; }
        [DataMember] public string BahtinovIntersectionComputeFailedMessage { get; set; }
        [DataMember] public string BahtinovIntersectionComputeFailedTitle { get; set; }

        [DataMember] public string DefocusNoImageMessage { get; set; }
        [DataMember] public string DefocusNoImageTitle { get; set; }

        [DataMember] public string UpdateAvailableMessage { get; set; }
        [DataMember] public string UpdateAvailableTitle { get; set; }
        [DataMember] public string UpdateDownloadedMessage { get; set; }
        [DataMember] public string UpdateDownloadedTitle { get; set; }
        [DataMember] public string UpdateFailedMessage { get; set; }
        [DataMember] public string UpdateFailedTitle { get; set; }
        [DataMember] public string UpdateCanceledMessage { get; set; }
        [DataMember] public string UpdateCanceledTitle { get; set; }
        [DataMember] public string UpdateNoneMessage { get; set; }
        [DataMember] public string UpdateNoneTitle { get; set; }
        [DataMember] public string UpdateNetworkErrorMessage { get; set; }
        [DataMember] public string UpdateNetworkErrorTitle { get; set; }
    }

    /// <summary>
    /// Holds the currently loaded UI text pack.
    /// </summary>
    public static class UiText
    {
        private static UiTextPack _current;

        public static UiTextPack Current
        {
            get
            {
                if (_current == null)
                    throw new InvalidOperationException("UiText.Current is null. Call UiText.LoadFromJson(...) at startup.");
                return _current;
            }
        }

        public static void LoadFromJson(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("filePath is null/empty.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("UI text JSON not found.", filePath);

            using (var fs = File.OpenRead(filePath))
            {
                var ser = new DataContractJsonSerializer(typeof(UiTextPack));

                var obj = ser.ReadObject(fs) as UiTextPack
                    ?? throw new SerializationException("Failed to deserialize UiTextPack from JSON.");

                _current = obj;
            }
        }
    }
}
