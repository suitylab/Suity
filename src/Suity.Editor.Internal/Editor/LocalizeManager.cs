using I18N.DotNet;
using Suity.Helpers;
using System.IO;
using System;
using Suity.Editor.Services;

namespace Suity.Editor
{
    /// <summary>
    /// Singleton service that manages application localization and language settings, responsible for loading XML localization files and switching the active language.
    /// </summary>
    public class LocalizeManager : ILocalizationService
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="LocalizeManager"/>.
        /// </summary>
        public static LocalizeManager Instance { get; } = new();

        // Auto-loading localizer that handles the actual localization resource loading.
        private readonly AutoLoadLocalizer _localizer = new();

        // Queued action to defer language update operations.
        readonly QueueOnceAction _updateLanguageAction;
        // The currently active language code (e.g., "en", "zh-cn").
        string _languageCode;

        /// <summary>
        /// Gets the current active language code.
        /// </summary>
        public string LanguageCode => _languageCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizeManager"/> class.
        /// </summary>
        private LocalizeManager()
        {
            _updateLanguageAction = new QueueOnceAction(_UpdateLanguage);
        }

        /// <summary>
        /// Parses the current active language string into a <see cref="LocalizedLanguage"/> enum value.
        /// </summary>
        /// <returns>The <see cref="LocalizedLanguage"/> corresponding to the current language setting.</returns>
        public LocalizedLanguage ParseCurrentLanguage()
        {
            return LocalizeManager.ParseLanguage(_languageCode);
        }

        /// <summary>
        /// Gets the human-readable display name of the current language (e.g., "English", "简体中文").
        /// </summary>
        public string LanguageName
        {
            get
            {
                switch (_languageCode)
                {
                    case "en":
                        return "English";

                    case "zh-cn":
                        return "简体中文";

                    case "zh-tw":
                        return "繁體中文";

                    case "jp":
                        return "日本語";

                    case "ko":
                        return "한국어";

                    default:
                        return "English";
                }
            }
        }

        /// <inheritdoc/>
        public void UpdateLanguage(LocalizedLanguage language, bool atOnce = false)
        {
            var lang = language.ToString().Replace('_', '-');
            UpdateLanguage(lang, atOnce);
        }

        /// <inheritdoc/>
        public void UpdateLanguage(string language, bool atOnce = false)
        {
            if (_languageCode == language)
            {
                return;
            }

            _languageCode = language;

            if (atOnce)
            {
                _UpdateLanguage();
            }
            else
            {
                _updateLanguageAction.DoQueuedAction();
            }
        }

        /// <summary>
        /// Performs the actual language update by loading all XML localization files from the Localization directory and applying them to the global localizer.
        /// </summary>
        private void _UpdateLanguage()
        {
            GlobalLocalizer.Localizer = _localizer;

            var localizeDir = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "Localization"));
            if (!localizeDir.Exists)
            {
                return;
            }

            bool merge = false;
            EditorServices.SystemLog.AddLog($"Load localization files from: {localizeDir.FullName}...");
            EditorServices.SystemLog.PushIndent();
            foreach (var file in localizeDir.GetFiles("*.xml"))
            {
                string name = Path.GetFileNameWithoutExtension(file.Name);
                try
                {
                    EditorServices.SystemLog.AddLog($"Load localization file: {name}...");
                    EditorServices.SystemLog.PushIndent();
                    LoadXML(name, merge);
                }
                catch (Exception err)
                {
                    err.LogError("Load localization file failed: " + file.Name);
                    EditorServices.SystemLog.AddLog($"Load localization file failed: {file.Name} - {err.Message}");
                }
                finally
                {
                    EditorServices.SystemLog.PopIndent();
                }

                // First file is loaded without merge; subsequent files are merged.
                if (!merge)
                {
                    merge = true;
                }
            }
            EditorServices.SystemLog.PopIndent();
            EditorServices.SystemLog.AddLog($"Load localization files from: {localizeDir.FullName} done.");

            EditorRexes.Language.Value = _languageCode;
        }

        /// <summary>
        /// Loads a single XML localization file into the localizer.
        /// </summary>
        /// <param name="fileName">The name of the localization file (without extension) to load.</param>
        /// <param name="merge">If <c>true</c>, merges the loaded content with existing localizations; otherwise, loads for the target language only.</param>
        private void LoadXML(string fileName, bool merge = true)
        {
            string langFile = Path.Combine(AppContext.BaseDirectory, $"Localization/{fileName}.xml");

            if (merge)
            {
                _localizer.LoadXML(langFile, true);
            }
            else
            {
                _localizer.LoadXML(langFile, _languageCode);
            }
        }

        /// <summary>
        /// Parses a language code string into a <see cref="LocalizedLanguage"/> enum value.
        /// </summary>
        /// <param name="lang">The language code string (e.g., "en", "zh-cn", "zh-tw", "jp", "ko"). Supports both hyphen and underscore separators.</param>
        /// <returns>The corresponding <see cref="LocalizedLanguage"/> value, or <see cref="LocalizedLanguage.en"/> if the string cannot be parsed.</returns>
        public static LocalizedLanguage ParseLanguage(string lang)
        {
            lang ??= "en";
            lang = lang.Replace('-', '_');

            if (Enum.TryParse<LocalizedLanguage>(lang, out var langEnum))
            {
                return langEnum;
            }
            else
            {
                return LocalizedLanguage.en;
            }
        }
    }
}
