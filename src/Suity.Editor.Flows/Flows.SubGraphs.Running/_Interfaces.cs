namespace Suity.Editor.Flows.SubGraphs.Running;

/// <summary>
/// Interface for objects that can create <see cref="SubGraphElement"/> instances.
/// </summary>
public interface ISubGraphElementCreator
{
    /// <summary>
    /// Creates a new <see cref="SubGraphElement"/> instance.
    /// </summary>
    /// <returns>The created page element.</returns>
    SubGraphElement CreatePageElement();
}
