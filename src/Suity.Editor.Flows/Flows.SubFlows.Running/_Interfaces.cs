namespace Suity.Editor.Flows.SubFlows.Running;

/// <summary>
/// Interface for objects that can create <see cref="SubFlowElement"/> instances.
/// </summary>
public interface ISubFlowElementCreator
{
    /// <summary>
    /// Creates a new <see cref="SubFlowElement"/> instance.
    /// </summary>
    /// <returns>The created page element.</returns>
    SubFlowElement CreatePageElement();
}
