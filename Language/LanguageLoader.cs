using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    /// <summary>
    /// Loads language resources for UI and next-step guidance based on environment override or UI culture.
    /// </summary>
    public static class LanguageLoader
    {
        #region Constants and State

        public const string DefaultLanguageCode = "en";
        public const string OverrideLanguageEnvironmentVariable = "SKYCAL_LANG";

        public static string CurrentLanguageCode { get; private set; } = DefaultLanguageCode;

        #endregion

        #region Public API

        /// <summary>
        /// Loads language files from disk using override language, UI culture candidates, or default fallback.
        /// </summary>
        /// <param name="baseDirectory">Root application directory containing the `Language` folder.</param>
        /// <param name="telescopeModel">Telescope model suffix used for next-step text file selection.</param>
        public static void LoadFromSystemCulture(string baseDirectory, string telescopeModel = "SCT")
        {
            if (string.IsNullOrWhiteSpace(baseDirectory))
                throw new ArgumentException("baseDirectory is null/empty.", nameof(baseDirectory));

            string overrideLanguage = GetOverrideLanguageCode();
            if (!string.IsNullOrWhiteSpace(overrideLanguage)
                && TryLoadLanguagePack(baseDirectory, telescopeModel, overrideLanguage))
            {
                CurrentLanguageCode = overrideLanguage;
                return;
            }

            var culture = CultureInfo.CurrentUICulture;
            foreach (var languageCode in BuildLanguageCandidates(culture))
            {
                if (TryLoadLanguagePack(baseDirectory, telescopeModel, languageCode))
                {
                    CurrentLanguageCode = languageCode;
                    return;
                }
            }

            MessageBox.Show("The required language files are missing. Please re-install", "Missing Language Files", MessageBoxButtons.OK, MessageBoxIcon.Error);
            throw new FileNotFoundException("No compatible language pack files were found.");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Returns an explicit language override code from process environment configuration.
        /// </summary>
        /// <returns>The override language code, or null/empty when no override is set.</returns>
        private static string GetOverrideLanguageCode()
        {
            return Environment.GetEnvironmentVariable(OverrideLanguageEnvironmentVariable)?.Trim();
        }

        /// <summary>
        /// Builds ordered language code candidates from the current culture and default fallback.
        /// </summary>
        /// <returns>An ordered sequence of candidate language codes.</returns>
        private static IEnumerable<string> BuildLanguageCandidates(CultureInfo culture)
        {
            if (culture != null && !string.IsNullOrWhiteSpace(culture.Name))
                yield return culture.Name;

            if (culture != null && !string.IsNullOrWhiteSpace(culture.TwoLetterISOLanguageName))
                yield return culture.TwoLetterISOLanguageName;

            yield return DefaultLanguageCode;
        }

        /// <summary>
        /// Attempts to load both required language packs for a specific language code.
        /// </summary>
        /// <returns>True when both files exist and are loaded successfully; otherwise false.</returns>
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

        #endregion
    }
}
