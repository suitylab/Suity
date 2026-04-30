namespace Suity.Views;

/// <summary>
/// Routable navigation object
/// </summary>
/// <remarks>
/// Mainly used for routing enumeration-to-object fields to enumeration fields.
/// </remarks>
public interface INavigableRoute
{
    /// <summary>
    /// Get the navigation route target object. If the route object does not exist, navigate itself.
    /// </summary>
    /// <returns></returns>
    object GetNavigableRoute();
}

/// <summary>
/// Represents an object that can navigate to its members by name.
/// </summary>
public interface INavigateMember
{
    /// <summary>
    /// Gets the member with the specified name for navigation.
    /// </summary>
    /// <param name="name">The name of the member to navigate to.</param>
    /// <returns>The member object, or null if not found.</returns>
    object GetNavigateMember(string name);
}