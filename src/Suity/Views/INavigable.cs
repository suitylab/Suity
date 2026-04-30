namespace Suity.Views;

/// <summary>
/// Navigable object
/// </summary>
public interface INavigable
{
    /// <summary>
    /// Get the navigation target object
    /// </summary>
    /// <returns></returns>
    object GetNavigationTarget();
}
