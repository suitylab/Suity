namespace Suity.Editor.VirtualTree.Nodes;

/// <summary>
/// An internal placeholder node with no displayed value.
/// Used as a fallback or sentinel node within the virtual tree.
/// </summary>
internal class EmptyNode : VirtualNode
{
    /// <inheritdoc/>
    public override object DisplayedValue => null;
}
