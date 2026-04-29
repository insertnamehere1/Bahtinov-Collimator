using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
        /// Represents a selectable UI language discovered on disk.
        /// </summary>
        public sealed class LanguageOption
        {
            public string Code { get; }
            public string DisplayName { get; }

            public LanguageOption(string code, string displayName)
            {
                Code = code;
                DisplayName = displayName;
            }
        }

        /// <summary>
        /// Loads language files from disk using override language, UI culture candidates, or default fallback.
        /// </summary>
        /// <param name="baseDirectory">Root application directory containing the `Language` folder.</param>
        /// <param name="telescopeModel">Telescope model suffix used for next-step text file selection.</param>
        public static void LoadFromSystemCulture(string baseDirectory, string telescopeModel = "SCT")
        {
            if (string.IsNullOrWhiteSpace(baseDirectory))
                throw new ArgumentException("baseDirectory is null/empty.", nameof(baseDirectory));

            string resolvedLanguageCode = ResolveAutoLanguageCode(baseDirectory, telescopeModel);
            if (!string.IsNullOrWhiteSpace(resolvedLanguageCode)
                && TryLoadLanguagePack(baseDirectory, telescopeModel, resolvedLanguageCode))
            {
                CurrentLanguageCode = resolvedLanguageCode;
                return;
            }

            MessageBox.Show("The required language files are missing. Please re-install", "Missing Language Files", MessageBoxButtons.OK, MessageBoxIcon.Error);
            throw new FileNotFoundException("No compatible language pack files were found.");
        }

        /// <summary>
        /// Loads language files using an explicit user preference ("auto" or language code).
        /// Falls back to English when an explicit preference is unavailable.
        /// </summary>
        public static bool LoadFromPreference(string baseDirectory, string telescopeModel, string languagePreference, out string resolvedLanguageCode)
        {
            if (string.IsNullOrWhiteSpace(baseDirectory))
                throw new ArgumentException("baseDirectory is null/empty.", nameof(baseDirectory));

            string preference = NormalizeLanguagePreference(languagePreference);

            if (preference == "auto")
            {
                LoadFromSystemCulture(baseDirectory, telescopeModel);
                resolvedLanguageCode = CurrentLanguageCode;
                return false;
            }

            if (TryLoadLanguagePack(baseDirectory, telescopeModel, preference))
            {
                CurrentLanguageCode = preference;
                resolvedLanguageCode = preference;
                return false;
            }

            if (TryLoadLanguagePack(baseDirectory, telescopeModel, DefaultLanguageCode))
            {
                CurrentLanguageCode = DefaultLanguageCode;
                resolvedLanguageCode = DefaultLanguageCode;
                return true;
            }

            MessageBox.Show("The required language files are missing. Please re-install", "Missing Language Files", MessageBoxButtons.OK, MessageBoxIcon.Error);
            throw new FileNotFoundException("No compatible language pack files were found.");
        }

        /// <summary>
        /// Returns available valid language packs with display names.
        /// </summary>
        public static IReadOnlyList<LanguageOption> GetAvailableLanguages(string baseDirectory)
        {
            if (string.IsNullOrWhiteSpace(baseDirectory))
                throw new ArgumentException("baseDirectory is null/empty.", nameof(baseDirectory));

            string languageRoot = Path.Combine(baseDirectory, "Language");
            if (!Directory.Exists(languageRoot))
                return Array.Empty<LanguageOption>();

            var options = new List<LanguageOption>();
            foreach (string languageDirectory in Directory.GetDirectories(languageRoot))
            {
                string code = Path.GetFileName(languageDirectory);
                if (string.IsNullOrWhiteSpace(code))
                    continue;

                if (!IsLanguagePackValid(baseDirectory, code))
                    continue;

                string displayName = TryReadLanguageDisplayName(baseDirectory, code);
                if (string.IsNullOrWhiteSpace(displayName))
                    displayName = code;

                options.Add(new LanguageOption(code, displayName));
            }

            return options
                .OrderBy(o => o.DisplayName, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Returns true when all required files exist for the language code.
        /// </summary>
        public static bool IsLanguagePackValid(string baseDirectory, string languageCode)
        {
            if (string.IsNullOrWhiteSpace(baseDirectory) || string.IsNullOrWhiteSpace(languageCode))
                return false;

            string languageRoot = Path.Combine(baseDirectory, "Language", languageCode);
            string uiPath = Path.Combine(languageRoot, "UiText.json");
            string sctPath = Path.Combine(languageRoot, "NextStepText_SCT.json");
            string mctPath = Path.Combine(languageRoot, "NextStepText_MCT.json");
            return File.Exists(uiPath) && File.Exists(sctPath) && File.Exists(mctPath);
        }

        /// <summary>
        /// Resolves the effective language for a user preference.
        /// </summary>
        public static string ResolveLanguageCodeForPreference(string baseDirectory, string languagePreference)
        {
            string preference = NormalizeLanguagePreference(languagePreference);
            if (preference == "auto")
                return ResolveAutoLanguageCode(baseDirectory, "SCT");

            return IsLanguagePackValid(baseDirectory, preference)
                ? preference
                : DefaultLanguageCode;
        }

        /// <summary>
        /// Reads a UI text pack for a language code without changing global state.
        /// </summary>
        public static UiTextPack ReadUiTextPackForLanguage(string baseDirectory, string languageCode)
        {
            string code = string.IsNullOrWhiteSpace(languageCode) ? DefaultLanguageCode : languageCode.Trim();
            string path = Path.Combine(baseDirectory, "Language", code, "UiText.json");
            return UiText.ReadPackFromJson(path);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Returns an explicit language override code from process environment configuration.
        /// </summary>
        /// <returns>The override language code, or null/empty when no override is set.</returns>
        private static string GetOverrideLanguageCode()
        {
            // Language Testing 
            //
            //    return "de";
            //    return "en";
            //    return "es";
            //    return "fr";
            //    return "it";
            //    return "ja";
            //    return "zh";
            
            return Environment.GetEnvironmentVariable(OverrideLanguageEnvironmentVariable)?.Trim();
        }

        private static string NormalizeLanguagePreference(string languagePreference)
        {
            if (string.IsNullOrWhiteSpace(languagePreference))
                return "auto";

            string trimmed = languagePreference.Trim();
            return string.Equals(trimmed, "auto", StringComparison.OrdinalIgnoreCase)
                ? "auto"
                : trimmed;
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

        private static string ResolveAutoLanguageCode(string baseDirectory, string telescopeModel)
        {
            string overrideLanguage = GetOverrideLanguageCode();
            if (!string.IsNullOrWhiteSpace(overrideLanguage)
                && HasRequiredLanguageFiles(baseDirectory, telescopeModel, overrideLanguage))
            {
                return overrideLanguage;
            }

            var culture = CultureInfo.CurrentUICulture;
            foreach (var languageCode in BuildLanguageCandidates(culture))
            {
                if (HasRequiredLanguageFiles(baseDirectory, telescopeModel, languageCode))
                    return languageCode;
            }

            return null;
        }

        private static string TryReadLanguageDisplayName(string baseDirectory, string languageCode)
        {
            try
            {
                UiTextPack pack = ReadUiTextPackForLanguage(baseDirectory, languageCode);
                return pack?.LanguageDisplayName;
            }
            catch
            {
                return null;
            }
        }

        private static bool HasRequiredLanguageFiles(string baseDirectory, string telescopeModel, string languageCode)
        {
            string uiPath = Path.Combine(baseDirectory, "Language", languageCode, "UiText.json");
            string nextStepPath = Path.Combine(baseDirectory, "Language", languageCode, $"NextStepText_{telescopeModel}.json");
            return File.Exists(uiPath) && File.Exists(nextStepPath);
        }

        #endregion
    }
}
