namespace Suity.Editor;

/// <summary>
/// Defines the set of supported localization languages for the editor.
/// </summary>
public enum LocalizedLanguage
{
    /// <summary>
    /// English language.
    /// </summary>
    [DisplayText("English")]
    en,

    /// <summary>
    /// Simplified Chinese language.
    /// </summary>
    [DisplayText("简体中文")]
    zh_cn,

    /// <summary>
    /// Traditional Chinese language.
    /// </summary>
    [DisplayText("繁體中文")]
    zh_tw,

    /// <summary>
    /// Japanese language.
    /// </summary>
    [DisplayText("日本語")]
    jp,

    /// <summary>
    /// Korean language.
    /// </summary>
    [DisplayText("한국어")]
    ko,
}