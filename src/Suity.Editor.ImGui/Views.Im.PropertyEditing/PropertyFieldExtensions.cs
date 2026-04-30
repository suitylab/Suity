using Suity.Synchonizing.Core;
using System;

namespace Suity.Views.Im.PropertyEditing;

/// <summary>
/// Represents an action performed on a property row, providing access to the node, column, and rendering pipeline.
/// </summary>
/// <param name="node">The ImGui node associated with the property row.</param>
/// <param name="column">The property grid column being processed.</param>
/// <param name="pipeline">The GUI pipeline used for rendering.</param>
public delegate void PropertyRowAction(ImGuiNode node, PropertyGridColumn column, GuiPipeline pipeline);

/// <summary>
/// Represents an action performed on a property target, providing access to the node, target, column, and rendering pipeline.
/// </summary>
/// <param name="node">The ImGui node associated with the property.</param>
/// <param name="target">The property target being processed.</param>
/// <param name="column">The property grid column being processed.</param>
/// <param name="pipeline">The GUI pipeline used for rendering.</param>
public delegate void PropertyTargetAction(ImGuiNode node, PropertyTarget target, PropertyGridColumn column, GuiPipeline pipeline);

/// <summary>
/// Provides extension methods for creating and managing property fields in ImGui-based property grids.
/// </summary>
public static class PropertyFieldExtensions
{
    /// <summary>
    /// Gets or sets the maximum number of elements allowed in an array property field.
    /// </summary>
    public static int ArrayMaxCount = 1000;

    /// <summary>
    /// Gets or sets the number of array elements displayed per page in paged array views.
    /// </summary>
    public static int ArrayPagingCount = 100;

    /// <summary>
    /// Gets or sets the external implementation provider for property field operations.
    /// </summary>
    internal static PropertyFieldExternal _external;

    /// <summary>
    /// Creates a property field for the specified target within the ImGui context.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The property target to display.</param>
    /// <param name="rowAction">An optional action to customize row rendering.</param>
    /// <returns>The created ImGui node, or null if the field could not be created.</returns>
    public static ImGuiNode? PropertyField(this ImGui gui, PropertyTarget target, PropertyRowAction? rowAction = null)
        => _external.PropertyField(gui, target, rowAction);

    /// <summary>
    /// Creates a null property field that renders nothing for the specified target.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The property target.</param>
    /// <param name="rowAction">An optional action to customize row rendering.</param>
    /// <returns>Always returns null.</returns>
    internal static ImGuiNode? NullPropertyField(this ImGui gui, PropertyTarget target, PropertyRowAction? rowAction)
        => null;

    /// <summary>
    /// Creates a property field with an enum editor for the specified target.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The property target containing an enum value.</param>
    /// <param name="rowAction">An optional action to customize row rendering.</param>
    /// <returns>The created ImGui node for the enum property field.</returns>
    internal static ImGuiNode EnumPropertyField(this ImGui gui, PropertyTarget target, PropertyRowAction? rowAction)
        => gui.PropertyRow(target, EditorTemplates.EnumEditor, rowAction);

    /// <summary>
    /// Creates a property field for array-type targets with support for element management.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The property target representing an array.</param>
    /// <param name="rowAction">An optional action to customize row rendering.</param>
    /// <returns>The created ImGui node for the array property field, or null if the target is not an array.</returns>
    internal static ImGuiNode? ArrayPropertyField(this ImGui gui, PropertyTarget target, PropertyRowAction? rowAction)
        => _external.ArrayPropertyField(gui, target, rowAction);

    /// <summary>
    /// Creates a property editor with a custom value action handler for the specified target.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The property target to edit.</param>
    /// <param name="handler">The action handler for value changes.</param>
    /// <returns>The created ImGui node for the property editor, or null if editing is not possible.</returns>
    public static ImGuiNode? PropertyEditor(this ImGui gui, PropertyTarget target, Action<IValueAction> handler)
        => _external.PropertyEditor(gui, target, handler);

    /// <summary>
    /// Creates a null property editor that renders nothing for the specified value target.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The action handler for value changes.</param>
    /// <returns>Always returns null.</returns>
    public static ImGuiNode? NullPropertyEditor(this ImGui gui, IValueTarget target, Action<IValueAction> handler)
        => null;

    /// <summary>
    /// Retrieves the property editor provider from the ImGui context, falling back to the default provider if none is registered.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <returns>The property editor provider, or null if none is available.</returns>
    public static IImGuiPropertyEditorProvider? GetPropertyEditorProvider(this ImGui gui)
        => gui.GetSystem<IImGuiPropertyEditorProvider>() ?? gui.GetSystem<ImGuiPropertyEditorProvider>();

    /// <summary>
    /// Executes a value action on the specified node, using the property grid data from the node hierarchy if available.
    /// </summary>
    /// <param name="node">The ImGui node on which to perform the action.</param>
    /// <param name="action">The value action to execute.</param>
    /// <param name="handler">Optional property grid data to handle the action. If null, it will be resolved from the node hierarchy.</param>
    public static void DoValueAction(this ImGuiNode node, IValueAction action, PropertyGridData? handler = null)
    {
        handler ??= node.FindValueInHierarchy<PropertyRowData>()?.GridData;
            // ?? node.FindValueInHierarchy<PropertyGridData>();

        if (handler is { })
        {
            handler.DoAction(action);
        }
        else
        {
            action.DoAction();
        }
    }

    /// <summary>
    /// Sets up an array target for property editing, preparing it for array-specific operations.
    /// </summary>
    /// <param name="target">The property target to set up as an array.</param>
    /// <param name="gui">The ImGui instance.</param>
    /// <returns>An ArrayTarget if the setup succeeds, or null if the target is not array-compatible.</returns>
    public static ArrayTarget? SetupArrayTarget(this PropertyTarget target, ImGui gui)
    {
        return _external.SetupArrayTarget(target, gui);
    }

    /// <summary>
    /// Registers a key-down action handler for the property field node, processing any pending key-down requests.
    /// </summary>
    /// <param name="node">The ImGui node representing the property field.</param>
    /// <param name="action">The action to execute when a key-down event occurs, receiving the node and key string.</param>
    /// <returns>The same ImGui node for method chaining.</returns>
    public static ImGuiNode OnPropertyFieldKeyDown(this ImGuiNode node, Action<ImGuiNode, string> action)
    {
        if (node.GetValue<PropertyRowData>() is { } value && value.KeyDownRequest is { } key)
        {
            value.KeyDownRequest = null;
            action(node, key);
        }

        return node;
    }

    /// <summary>
    /// Retrieves a synchronization path builder for the specified property target.
    /// </summary>
    /// <param name="target">The property target to build a sync path for.</param>
    /// <returns>A SyncPathBuilder instance for constructing synchronization paths.</returns>
    public static SyncPathBuilder GetSyncPathBuilder(this PropertyTarget? target)
    {
        return _external.GetSyncPathBuilder(target);
    }

    /// <summary>
    /// Determines whether any property field is currently selected within the ImGui context.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <returns>True if a property field is selected; otherwise, false.</returns>
    public static bool GetIsPropertyFieldSelected(this ImGui gui)
    {
        return gui.CurrentNode?.GetIsPropertyFieldSelected() ?? false;
    }

    /// <summary>
    /// Determines whether the specified ImGui node represents a selected property field.
    /// </summary>
    /// <param name="node">The ImGui node to check.</param>
    /// <returns>True if the node's property row data indicates it is selected; otherwise, false.</returns>
    public static bool GetIsPropertyFieldSelected(this ImGuiNode node)
    {
        var value = node.GetValueInHierarchy<PropertyRowData>();
        return value?.IsSelected == true;
    }

    /// <summary>
    /// Updates the visual selection state of a property field by swapping CSS classes based on the selection status.
    /// </summary>
    /// <param name="node">The ImGui node representing the property field.</param>
    /// <param name="value">The property row data containing the selection state.</param>
    public static void UpdatePropertyFieldSelection(this ImGuiNode node, PropertyRowData value)
    {
        // Debug.WriteLine($"{value.Id}({value.GetHashCode()}) IsSelected={value.IsSelected}");
        if (!value.SelectEnabled)
        {
            return;
        }

        if (value.IsSelected)
        {
            node.SwapClass(PropertyGridThemes.ClassPropertyLine, PropertyGridThemes.ClassPropertyLineSel);
        }
        else
        {
            node.SwapClass(PropertyGridThemes.ClassPropertyLineSel, PropertyGridThemes.ClassPropertyLine);
        }
    }

    /// <summary>
    /// Sets up drag-and-drop functionality for an array item within the property grid.
    /// </summary>
    /// <param name="node">The ImGui node representing the array item.</param>
    /// <param name="target">The property target for the array.</param>
    /// <param name="index">The zero-based index of the array item.</param>
    public static void SetupArrayItemDragDrop(this ImGuiNode node, PropertyTarget target, int index) 
        => _external.SetupArrayItemDragDrop(node, target, index);
}