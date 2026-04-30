namespace Suity.Editor.Services;

/// <summary>
/// Provides services for managing application localization and language settings.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Get the current localization language code.
    /// </summary>
    string LanguageCode { get; }

    /// <summary>
    /// Get the current localization language name.
    /// </summary>
    string LanguageName { get; }

    /// <summary>
    /// Update the localization language.
    /// </summary>
    /// <param name="language">Language code, currently support: "en", "zh-cn", "zh-tw", "jp"</param>
    /// <param name="atOnce">Update immediately or in the next update cycle</param>
    void UpdateLanguage(string language, bool atOnce = false);
}