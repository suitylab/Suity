namespace Suity.Views.Menu;

/// <summary>
/// Interface for objects that provide sender context for menu operations.
/// </summary>
public interface IMenuSenderContext
{
    /// <summary>
    /// Gets the target object that triggered the menu.
    /// </summary>
    object SenderTarget { get; }
}