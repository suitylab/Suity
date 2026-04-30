namespace Suity.Views.Im.PropertyEditing;

/// <summary>
/// Abstract base class for property row drawers in ImGui.
/// Provides default implementations for various property row drawing methods.
/// </summary>
public abstract class ImGuiPropertyDrawer
{
    /// <summary>
    /// Draws a property row for the specified node and target.
    /// </summary>
    /// <param name="node">The ImGui node to draw the property row for.</param>
    /// <param name="id">The unique identifier for the property row.</param>
    /// <param name="target">The property target to draw.</param>
    /// <param name="targetAction">Optional action to perform on the property target.</param>
    /// <returns>True if the property row was drawn successfully, false otherwise.</returns>
    public virtual bool DrawPropertyRow(ImGuiNode node, string id, PropertyTarget target, PropertyTargetAction? targetAction = null) => false;

    /// <summary>
    /// Draws a property row frame for the specified node.
    /// </summary>
    /// <param name="node">The ImGui node to draw the property row frame for.</param>
    /// <param name="id">The unique identifier for the property row frame.</param>
    /// <param name="rowAction">Optional action to perform on the property row.</param>
    /// <param name="rowData">Optional data associated with the property row.</param>
    /// <returns>True if the property row frame was drawn successfully, false otherwise.</returns>
    public virtual bool DrawPropertyRowFrame(ImGuiNode node, string id, PropertyRowAction? rowAction = null, PropertyRowData? rowData = null) => false;

    /// <summary>
    /// Draws a property label for the specified target.
    /// </summary>
    /// <param name="node">The ImGui node to draw the property label for.</param>
    /// <param name="target">The property target to draw the label for.</param>
    /// <param name="rowAction">Optional action to perform on the property row.</param>
    /// <returns>True if the property label was drawn successfully, false otherwise.</returns>
    public virtual bool DrawPropertyLabel(ImGuiNode node, PropertyTarget target, PropertyRowAction? rowAction = null) => false;

    /// <summary>
    /// Draws a property label frame for the specified node.
    /// </summary>
    /// <param name="node">The ImGui node to draw the property label frame for.</param>
    /// <param name="id">The unique identifier for the property label frame.</param>
    /// <param name="rowAction">Optional action to perform on the property row.</param>
    /// <param name="rowData">Optional data associated with the property row.</param>
    /// <returns>True if the property label frame was drawn successfully, false otherwise.</returns>
    public virtual bool DrawPropertyLabelFrame(ImGuiNode node, string id, PropertyRowAction? rowAction = null, PropertyRowData? rowData = null) => false;

    /// <summary>
    /// Draws a property group for the specified target.
    /// </summary>
    /// <param name="node">The ImGui node to draw the property group for.</param>
    /// <param name="target">The property target to draw the group for.</param>
    /// <param name="preview">Optional preview text for the property group.</param>
    /// <param name="targetAction">Optional action to perform on the property target.</param>
    /// <returns>True if the property group was drawn successfully, false otherwise.</returns>
    public virtual bool DrawPropertyGroup(ImGuiNode node, PropertyTarget target, string? preview = null, PropertyTargetAction? targetAction = null) => false;

    /// <summary>
    /// Draws a property group frame for the specified node.
    /// </summary>
    /// <param name="node">The ImGui node to draw the property group frame for.</param>
    /// <param name="id">The unique identifier for the property group frame.</param>
    /// <param name="rowAction">Optional action to perform on the property row.</param>
    /// <param name="initExpand">Optional flag indicating whether the group should be expanded initially.</param>
    /// <param name="rowData">Optional data associated with the property row.</param>
    /// <returns>True if the property group frame was drawn successfully, false otherwise.</returns>
    public virtual bool DrawPropertyGroupFrame(ImGuiNode node, string id, PropertyRowAction? rowAction = null, bool? initExpand = true, PropertyRowData? rowData = null) => false;
}
