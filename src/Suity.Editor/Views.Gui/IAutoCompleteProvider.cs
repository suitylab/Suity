namespace Suity.Views.Gui;

/// <summary>
/// Defines the contract for providing auto-complete suggestions.
/// </summary>
public interface IAutoCompleteProvider
{
    /// <summary>
    /// Returns the object used for auto-complete operations.
    /// </summary>
    /// <returns>An object that provides auto-complete functionality.</returns>
    object GetAutoCompleteObject();
}