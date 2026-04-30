namespace Suity.Views.Im;

/// <summary>
/// Represents the data source for a virtualized list in ImGui, providing item retrieval,
/// layout information, and content propagation for efficient rendering of large lists.
/// </summary>
public abstract class VisualListData
{
    private float? _width;
    private float? _headerHeight;

    /// <summary>
    /// Gets or sets the total width of the list content.
    /// When set to <c>null</c>, the width is automatically determined by the list container.
    /// Negative values are treated as <c>null</c>.
    /// </summary>
    public float? Width
    {
        get => _width;
        set
        {
            if (value < 0)
            {
                value = null;
            }
            _width = value;
        }
    }

    /// <summary>
    /// Gets or sets the height of the list header in pixels.
    /// Set to <c>null</c> to indicate the list has no header.
    /// Negative values are treated as <c>null</c>.
    /// </summary>
    public float? HeaderHeight
    {
        get => _headerHeight;
        set
        {
            if (value < 0)
            {
                value = null;
            }
            _headerHeight = value;
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="ContentTemplate"/> used to render the list header.
    /// Set to <c>null</c> if no header should be displayed.
    /// </summary>
    public ContentTemplate? HeaderTemplate { get; set; }

    /// <summary>
    /// Gets the total height of all list content, including items, spacing, and header (if present).
    /// Used by the virtualized list to calculate scroll bounds and visible item ranges.
    /// </summary>
    public abstract float TotalHeight { get; }

    /// <summary>
    /// Gets the total number of items available in the list.
    /// </summary>
    public abstract int Count { get; }

    /// <summary>
    /// Gets or sets the vertical spacing in pixels between consecutive list items.
    /// Defaults to zero if not explicitly set.
    /// </summary>
    public float Spacing { get; set; }

    /// <summary>
    /// Retrieves the item located at the specified zero-based index in the list.
    /// </summary>
    /// <param name="index">The zero-based index of the item to retrieve.</param>
    /// <returns>The item at the specified index, or <c>null</c> if the index is out of range.</returns>
    public abstract object? GetItemAt(int index);

    /// <summary>
    /// Propagates the visible portion of the list contents by creating child <see cref="ImGuiNode"/> instances
    /// for each item within the current viewport. This enables virtualized rendering where only visible
    /// items are instantiated, improving performance for large lists.
    /// </summary>
    /// <param name="node">The parent <see cref="ImGuiNode"/> to which child nodes will be added.</param>
    /// <param name="nodeFactory">The <see cref="NodeFactory"/> used to create and configure child nodes.</param>
    /// <param name="posX">The starting X coordinate for positioning items within the list.</param>
    /// <param name="posY">The starting Y coordinate for positioning items within the list.</param>
    public abstract void PropagateContents(ImGuiNode node, NodeFactory nodeFactory, float posX, float posY);
}

/// <summary>
/// Represents a strongly-typed virtualized list data source that provides item retrieval,
/// layout information, and typed row templates for efficient rendering of large lists in ImGui.
/// </summary>
/// <typeparam name="T">The type of items contained in the list.</typeparam>
public abstract class VisualListData<T> : VisualListData
{
    /// <summary>
    /// Gets or sets the <see cref="ContentTemplate{T}"/> used to render individual list rows.
    /// Set to <c>null</c> to use default rendering behavior.
    /// </summary>
    public ContentTemplate<T>? RowTemplate { get; set; }
}
