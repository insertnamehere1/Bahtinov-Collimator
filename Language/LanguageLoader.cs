using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Bahtinov_Collimator
{
    public static class LanguageLoader
    {
        public const string DefaultLanguageCode = "en";

        public static string CurrentLanguageCode { get; private set; } = DefaultLanguageCode;

        public static void LoadFromSystemCulture(string baseDirectory, string telescopeModel = "SCT")
        {
            if (string.IsNullOrWhiteSpace(baseDirectory))
                throw new ArgumentException("baseDirectory is null/empty.", nameof(baseDirectory));

            var culture = CultureInfo.CurrentUICulture;
            foreach (var languageCode in BuildLanguageCandidates(culture))
            {
                if (TryLoadLanguagePack(baseDirectory, telescopeModel, languageCode))
                {
                    CurrentLanguageCode = languageCode;
                    return;
                }
            }

            throw new FileNotFoundException("No compatible language pack files were found.");
        }

        private static IEnumerable<string> BuildLanguageCandidates(CultureInfo culture)
        {
            if (culture != null && !string.IsNullOrWhiteSpace(culture.Name))
                yield return culture.Name;

            if (culture != null && !string.IsNullOrWhiteSpace(culture.TwoLetterISOLanguageName))
                yield return culture.TwoLetterISOLanguageName;

            yield return DefaultLanguageCode;
        }

        private static bool TryLoadLanguagePack(string baseDirectory, string telescopeModel, string languageCode)
        {
            string uiPath = Path.Combine(baseDirectory, "Language", languageCode, "UiText.json");
            string nextStepPath = Path.Combine(baseDirectory, "Language", languageCode, $"NextStepText_{telescopeModel}.json");

            if (!File.Exists(uiPath) || !File.Exists(nextStepPath))
                return false;

            UiText.LoadFromJson(uiPath);
            NextStepText.LoadFromJson(nextStepPath);
            return true;
        }
    }
}
