namespace Suity.Editor.Flows.SubGraphs.Running;

/// <summary>
/// Interface for objects that can create <see cref="AigcPageElement"/> instances.
/// </summary>
public interface IPageElementCreator
{
    /// <summary>
    /// Creates a new <see cref="AigcPageElement"/> instance.
    /// </summary>
    /// <returns>The created page element.</returns>
    AigcPageElement CreatePageElement();
}
