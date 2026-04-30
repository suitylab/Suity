namespace Suity.Views;

/// <summary>
/// Defines an interface for objects that can display text.
/// </summary>
public interface ITextDisplay
{
    /// <summary>
    /// Gets the display text.
    /// </summary>
    string DisplayText { get; }
    /// <summary>
    /// Gets the display icon.
    /// </summary>
    object DisplayIcon { get; }
    /// <summary>
    /// Gets the display status.
    /// </summary>
    TextStatus DisplayStatus { get; }
}