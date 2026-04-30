using Suity.Editor.VirtualTree.Nodes;

namespace Suity.Editor.VirtualTree;

/// <summary>
/// Provides helper methods for retrieving adapters from virtual nodes.
/// </summary>
internal static class AdapterHelper
{
    /// <summary>
    /// Gets the adapter editor from a virtual node if it's a list-based node.
    /// </summary>
    /// <param name="node">The node to get the adapter from.</param>
    /// <returns>The adapter instance, or null if the node doesn't have one.</returns>
    public static object GetAdapterEditor(VirtualNode node)
    {
        if (node is null)
        {
            return null;
        }
        else if (node is ListVirtualNode listVirtualNode)
        {
            return listVirtualNode.Adapter;
        }
        else if (node is IListVirtualNode iListVirtualNode)
        {
            return iListVirtualNode.Adapter;
        }
        else
        {
            return null;
        }
    }
}