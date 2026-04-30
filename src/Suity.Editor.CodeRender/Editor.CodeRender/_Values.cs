namespace Suity.Editor.CodeRender;

/// <summary>
/// Options for replacement behavior during code rendering.
/// </summary>
public enum ReplaceOption
{
    /// <summary>
    /// No special options.
    /// </summary>
    None,
    /// <summary>
    /// Normal replacement behavior.
    /// </summary>
    Normal,
    /// <summary>
    /// Skip empty replacements.
    /// </summary>
    SkipEmpty,
}
