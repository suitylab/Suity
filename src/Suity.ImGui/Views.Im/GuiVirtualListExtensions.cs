namespace Suity.Views.Im;

/// <summary>
/// Extension methods for creating virtual list controls in ImGui.
/// Virtual lists only render visible items for performance optimization.
/// </summary>
public static class GuiVirtualListExtensions
{
    /// <summary>
    /// Identifier for the virtual list header node.
    /// </summary>
    public const string HeaderId = "##virtual_list_header";

    /// <summary>
    /// Type name used for virtual list header nodes.
    /// </summary>
    public const string VirtualListHeaderTypeName = "VirtualListHeader";

    /// <summary>
    /// Type name used for virtual list item nodes.
    /// </summary>
    public const string VirtualListItemTypeName = "VirtualListItem";

    /// <summary>
    /// Creates a virtual list control that only renders visible items for performance.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">Unique identifier for the node. If null, an auto-generated ID is used.</param>
    /// <param name="scroll">The scroll orientation for the list.</param>
    /// <returns>The created ImGuiNode.</returns>
    public static ImGuiNode VirtualList(this ImGui gui, string? id = null, GuiOrientation scroll = GuiOrientation.Vertical)
    {
        id ??= $"##virtual_list_{gui.CurrentNode.CurrentLayoutIndex}";
        ImGuiNode node = gui.BeginCurrentNode(id);

        if (node.IsInitializing)
        {
            node.TypeName = nameof(VirtualList);
            node.SetupScrollable(scroll);
            node.SetLayoutFunction(ImGuiLayoutSystem.Vertical);
            node.SetFitFunction();
            node.SetRenderFunction();
        }

        return node;
    }

    /// <summary>
    /// Sets the data source for a virtual list control.
    /// </summary>
    /// <param name="node">The virtual list node.</param>
    /// <param name="data">The visual list data containing items to display.</param>
    /// <param name="factory">Optional factory for creating item nodes.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetVirtualListData(this ImGuiNode node, VisualListData data, NodeFactory? factory = null)
    {
        ImGuiExternal._external.SetVirtualListData(node, data, factory);

        return node;
    }

    /// <summary>
    /// Removes the data source from a virtual list control.
    /// </summary>
    /// <param name="node">The virtual list node.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode UnsetVirtualListData(this ImGuiNode node)
    {
        node.RemoveValue<VisualListData>();

        node.OnContent(() => { });

        return node;
    }
}
