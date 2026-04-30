namespace Suity.Editor.VirtualTree.Nodes;

/// <summary>
/// The root node of a virtual tree. Serves as the top-level container
/// and provides default context menu behavior for child nodes.
/// </summary>
public class RootNode : VirtualNode
{
    /// <inheritdoc/>
    public override object DisplayedValue => null;

    /// <summary>
    /// Notifies the model that this root node is ready. Used internally by the virtual tree model.
    /// </summary>
    internal void InternalNotifyModel()
    {
        NotifyModel();
    }

    /// <inheritdoc/>
    public override string GetChildContextMenuKey(VirtualNode childNode)
    {
        return ContextMenu_ArrayOwner;
    }
}
