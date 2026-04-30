using Suity.Synchonizing.Core;
using Suity.Views.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Views.Im.PropertyEditing;

/// <summary>
/// External abstraction for property grid UI construction and layout.
/// </summary>
internal abstract class PropertyGridExternal
{
    /// <summary>
    /// Creates a new property grid with the specified name.
    /// </summary>
    /// <param name="name">The name of the property grid.</param>
    /// <returns>A new <see cref="IPropertyGrid"/> instance.</returns>
    public abstract IPropertyGrid CreatePropertyGrid(string name);

    /// <summary>
    /// Renders a property frame using default identification.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="scrollable">Whether the property frame should be scrollable.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property frame.</returns>
    public abstract ImGuiNode PropertyFrame(ImGui gui, bool scrollable);

    /// <summary>
    /// Renders a property frame with a custom identifier.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="id">The unique identifier for the property frame.</param>
    /// <param name="scrollable">Whether the property frame should be scrollable.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property frame.</returns>
    public abstract ImGuiNode PropertyFrame(ImGui gui, string id, bool scrollable);

    /// <summary>
    /// Renders a property frame with initialization data and optional resizer state.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="id">The unique identifier for the property frame.</param>
    /// <param name="scrollable">Whether the property frame should be scrollable.</param>
    /// <param name="initGridData">The initial grid data configuration.</param>
    /// <param name="initResizerState">The optional initial resizer state.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property frame.</returns>
    public abstract ImGuiNode PropertyFrame(ImGui gui, string id, bool scrollable, PropertyGridData initGridData, GroupedResizerState? initResizerState = null);

    /// <summary>
    /// Renders a property box with a title and optional initial expand state.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="id">The unique identifier for the property box.</param>
    /// <param name="title">The display title of the property box.</param>
    /// <param name="initExpand">Whether the box should be initially expanded.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property box.</returns>
    public abstract ImGuiNode PropertyBox(ImGui gui, string id, string title, bool initExpand = true);

    /// <summary>
    /// Renders a property row bound to a target with an optional editor function.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The property target to bind to.</param>
    /// <param name="func">An optional editor function for the row.</param>
    /// <param name="rowAction">An optional row action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property row.</returns>
    public abstract ImGuiNode PropertyRow(ImGui gui, PropertyTarget target, PropertyEditorFunction? func, PropertyRowAction? rowAction = null);

    /// <summary>
    /// Renders a property row bound to a target with an optional target action.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The property target to bind to.</param>
    /// <param name="targetAction">An optional target action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property row.</returns>
    public abstract ImGuiNode PropertyRow(ImGui gui, PropertyTarget target, PropertyTargetAction? targetAction = null);

    /// <summary>
    /// Renders a property row with a custom identifier bound to a target.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="id">The unique identifier for the property row.</param>
    /// <param name="target">The property target to bind to.</param>
    /// <param name="targetAction">An optional target action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property row.</returns>
    public abstract ImGuiNode PropertyRow(ImGui gui, string id, PropertyTarget target, PropertyTargetAction? targetAction = null);

    /// <summary>
    /// Renders a property row with a title and optional status.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="id">The unique identifier for the property row.</param>
    /// <param name="title">The display title of the row.</param>
    /// <param name="status">An optional text status indicator.</param>
    /// <param name="rowAction">An optional row action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property row.</returns>
    public abstract ImGuiNode PropertyRow(ImGui gui, string id, string title, TextStatus? status = null, PropertyRowAction? rowAction = null);

    /// <summary>
    /// Renders a property row frame with optional action and data.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="id">The unique identifier for the row frame.</param>
    /// <param name="rowAction">An optional row action handler.</param>
    /// <param name="rowData">An optional row data configuration.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property row frame.</returns>
    public abstract ImGuiNode PropertyRowFrame(ImGui gui, string id, PropertyRowAction? rowAction = null, PropertyRowData? rowData = null);

    /// <summary>
    /// Renders a property label bound to a target.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The property target to bind to.</param>
    /// <param name="rowAction">An optional row action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property label.</returns>
    public abstract ImGuiNode PropertyLabel(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction = null);

    /// <summary>
    /// Renders a property label with full customization options.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="id">The unique identifier for the label.</param>
    /// <param name="title">The display title of the label.</param>
    /// <param name="icon">An optional icon image.</param>
    /// <param name="status">An optional text status indicator.</param>
    /// <param name="rowAction">An optional row action handler.</param>
    /// <param name="rowData">An optional row data configuration.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property label.</returns>
    public abstract ImGuiNode PropertyLabel(ImGui gui, string id, string title, Image? icon = null, TextStatus? status = null, PropertyRowAction? rowAction = null, PropertyRowData? rowData = null);

    /// <summary>
    /// Renders a property label frame with optional action and data.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="id">The unique identifier for the label frame.</param>
    /// <param name="rowAction">An optional row action handler.</param>
    /// <param name="rowData">An optional row data configuration.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property label frame.</returns>
    public abstract ImGuiNode PropertyLabelFrame(ImGui gui, string id, PropertyRowAction? rowAction = null, PropertyRowData? rowData = null);

    /// <summary>
    /// Renders property tooltips bound to a target.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The property target to bind to.</param>
    /// <param name="rowAction">An optional row action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property tooltips.</returns>
    public abstract ImGuiNode PropertyTooltips(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction = null);

    /// <summary>
    /// Renders property tooltips with full customization options.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="id">The unique identifier for the tooltips.</param>
    /// <param name="title">The display title.</param>
    /// <param name="icon">An optional icon image.</param>
    /// <param name="status">An optional text status indicator.</param>
    /// <param name="rowAction">An optional row action handler.</param>
    /// <param name="rowData">An optional row data configuration.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property tooltips.</returns>
    public abstract ImGuiNode PropertyTooltips(ImGui gui, string id, string title, Image? icon = null, TextStatus? status = null, PropertyRowAction? rowAction = null, PropertyRowData? rowData = null);

    /// <summary>
    /// Renders a property button bound to a target.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The property target to bind to.</param>
    /// <param name="rowAction">An optional row action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property button.</returns>
    public abstract ImGuiNode PropertyButton(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction = null);

    /// <summary>
    /// Renders a property button with a click action callback.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="id">The unique identifier for the button.</param>
    /// <param name="title">The display title of the button.</param>
    /// <param name="icon">An optional icon image.</param>
    /// <param name="rowAction">An optional row action handler.</param>
    /// <param name="onClick">An optional click action callback.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property button.</returns>
    public abstract ImGuiNode PropertyButton(ImGui gui, string id, string title, Image? icon = null, PropertyRowAction? rowAction = null, Action? onClick = null);

    /// <summary>
    /// Renders a property button with a node-based click action callback.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="id">The unique identifier for the button.</param>
    /// <param name="title">The display title of the button.</param>
    /// <param name="icon">An optional icon image.</param>
    /// <param name="rowAction">An optional row action handler.</param>
    /// <param name="onClick">An optional click action callback receiving an <see cref="ImGuiNode"/>.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property button.</returns>
    public abstract ImGuiNode PropertyButton(ImGui gui, string id, string title, Image? icon = null, PropertyRowAction? rowAction = null, Action<ImGuiNode>? onClick = null);

    /// <summary>
    /// Renders a property button that supports multiple selection targets.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The property target to bind to.</param>
    /// <param name="rowAction">An optional row action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property button.</returns>
    public abstract ImGuiNode PropertyMultipleButton(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction = null);

    /// <summary>
    /// Renders a collapsible property group bound to a target.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The property target to bind to.</param>
    /// <param name="preview">An optional preview text shown when collapsed.</param>
    /// <param name="targetAction">An optional target action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property group.</returns>
    public abstract ImGuiNode PropertyGroup(ImGui gui, PropertyTarget target, string? preview = null, PropertyTargetAction? targetAction = null);

    /// <summary>
    /// Renders a collapsible property group with full customization options.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="id">The unique identifier for the group.</param>
    /// <param name="title">The display title of the group.</param>
    /// <param name="preview">An optional preview text shown when collapsed.</param>
    /// <param name="rowAction">An optional row action handler.</param>
    /// <param name="initExpand">Whether the group should be initially expanded.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property group.</returns>
    public abstract ImGuiNode PropertyGroup(ImGui gui, string id, string title, string? preview = null, PropertyRowAction? rowAction = null, bool initExpand = true);

    /// <summary>
    /// Renders a property group frame with optional action, expand state, and data.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="id">The unique identifier for the group frame.</param>
    /// <param name="rowAction">An optional row action handler.</param>
    /// <param name="initExpand">The optional initial expand state.</param>
    /// <param name="rowData">An optional row data configuration.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property group frame.</returns>
    public abstract ImGuiNode PropertyGroupFrame(ImGui gui, string id, PropertyRowAction? rowAction = null, bool? initExpand = true, PropertyRowData? rowData = null);

    /// <summary>
    /// Renders a property title bound to a target.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The property target to bind to.</param>
    /// <param name="dark">Whether to use dark theme styling.</param>
    public abstract void PropertyTitle(ImGui gui, PropertyTarget target, bool dark = false);

    /// <summary>
    /// Renders a property title bound to a value target.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The value target to bind to.</param>
    /// <param name="dark">Whether to use dark theme styling.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property title.</returns>
    public abstract ImGuiNode PropertyTitle(ImGui gui, IValueTarget target, bool dark = false);

    /// <summary>
    /// Attaches an expand action to a property group node.
    /// </summary>
    /// <param name="node">The property group node.</param>
    /// <param name="action">The action to invoke on expand.</param>
    /// <returns>The modified <see cref="ImGuiNode"/>.</returns>
    public abstract ImGuiNode OnPropertyGroupExpand(ImGuiNode node, Action action);

    /// <summary>
    /// Attaches an expand action with expand state to a property group node.
    /// </summary>
    /// <param name="node">The property group node.</param>
    /// <param name="action">The action to invoke on expand, receiving the expanded state.</param>
    /// <returns>The modified <see cref="ImGuiNode"/>.</returns>
    public abstract ImGuiNode OnPropertyGroupExpand(ImGuiNode node, Action<bool> action);
}

/// <summary>
/// External abstraction for property field rendering based on value types.
/// </summary>
internal abstract class PropertyFieldExternal
{
    /// <summary>
    /// Renders a property field for the given target.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The property target to render.</param>
    /// <param name="rowAction">An optional row action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property field, or null if not applicable.</returns>
    public abstract ImGuiNode? PropertyField(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction = null);

    /// <summary>
    /// Renders a fallback field for null property targets.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The property target to render.</param>
    /// <param name="rowAction">An optional row action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the null property field.</returns>
    public abstract ImGuiNode? NullPropertyField(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction);

    /// <summary>
    /// Renders a property field for enum values.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The property target to render.</param>
    /// <param name="rowAction">An optional row action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the enum property field.</returns>
    public abstract ImGuiNode EnumPropertyField(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction);

    /// <summary>
    /// Renders a property field for array values.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The property target to render.</param>
    /// <param name="rowAction">An optional row action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the array property field, or null if not applicable.</returns>
    public abstract ImGuiNode? ArrayPropertyField(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction);

    /// <summary>
    /// Renders a property field for text block values.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The property target to render.</param>
    /// <param name="rowAction">An optional row action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the text block property field, or null if not applicable.</returns>
    public abstract ImGuiNode? TextBlockPropertyField(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction);

    /// <summary>
    /// Renders a property editor with a value action handler.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The property target to render.</param>
    /// <param name="handler">The value action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property editor, or null if not applicable.</returns>
    public abstract ImGuiNode? PropertyEditor(ImGui gui, PropertyTarget target, Action<IValueAction> handler);

    /// <summary>
    /// Renders a fallback editor for null value targets.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The value target to render.</param>
    /// <param name="handler">The value action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the null property editor, or null if not applicable.</returns>
    public abstract ImGuiNode? NullPropertyEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler);

    /// <summary>
    /// Sets up an array target from a property target.
    /// </summary>
    /// <param name="target">The property target to convert.</param>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <returns>An <see cref="ArrayTarget"/> if successful, or null.</returns>
    public abstract ArrayTarget? SetupArrayTarget(PropertyTarget target, ImGui gui);

    /// <summary>
    /// Gets the synchronization path builder for a property target.
    /// </summary>
    /// <param name="target">The property target, or null.</param>
    /// <returns>A <see cref="SyncPathBuilder"/> instance.</returns>
    public abstract SyncPathBuilder GetSyncPathBuilder(PropertyTarget? target);

    /// <summary>
    /// Sets up drag-and-drop behavior for an array item node.
    /// </summary>
    /// <param name="node">The ImGui node representing the array item.</param>
    /// <param name="target">The property target of the array.</param>
    /// <param name="index">The index of the array item.</param>
    public abstract void SetupArrayItemDragDrop(ImGuiNode node, PropertyTarget target, int index);
}

/// <summary>
/// External abstraction for creating and manipulating property targets.
/// </summary>
internal abstract class PropertyTargetExternal
{
    /// <summary>
    /// Creates a property target from a collection of objects.
    /// </summary>
    /// <param name="objs">The collection of objects to target.</param>
    /// <returns>A new <see cref="PropertyTarget"/> instance.</returns>
    public abstract PropertyTarget CreatePropertyTarget(IEnumerable<object> objs);

    /// <summary>
    /// Creates a property target from a collection of objects for a specific property.
    /// </summary>
    /// <param name="objs">The collection of objects to target.</param>
    /// <param name="propertyName">The name of the property to target.</param>
    /// <returns>A new <see cref="PropertyTarget"/> instance.</returns>
    public abstract PropertyTarget CreatePropertyTarget(IEnumerable<object> objs, string propertyName);

    /// <summary>
    /// Populates a property target along a synchronization path.
    /// </summary>
    /// <param name="target">The base property target.</param>
    /// <param name="path">The synchronization path to populate.</param>
    /// <param name="forceRepopulate">Whether to force repopulation even if already populated.</param>
    /// <returns>A populated <see cref="PropertyTarget"/>, or null if unsuccessful.</returns>
    public abstract PropertyTarget? PopulatePath(PropertyTarget target, SyncPath path, bool forceRepopulate);

    /// <summary>
    /// Populates properties on a target using an optional provider.
    /// </summary>
    /// <param name="target">The property target to populate.</param>
    /// <param name="provider">An optional property editor provider.</param>
    /// <returns>True if properties were successfully populated.</returns>
    public abstract bool PopulateProperties(PropertyTarget target, IImGuiPropertyEditorProvider? provider = null);

    /// <summary>
    /// Gets field information for an SItem in the target.
    /// </summary>
    /// <param name="target">The property target to inspect.</param>
    /// <returns>Field information object, or null.</returns>
    public abstract object? GetSItemFieldInfomation(PropertyTarget target);

    /// <summary>
    /// Repairs an SItem value action for the target.
    /// </summary>
    /// <param name="target">The property target to repair.</param>
    /// <returns>An <see cref="IValueAction"/> if successful, or null.</returns>
    public abstract IValueAction? RepairSItem(PropertyTarget target);

    /// <summary>
    /// Repairs an SContainer value action for the target.
    /// </summary>
    /// <param name="target">The property target to repair.</param>
    /// <returns>An <see cref="IValueAction"/> if successful, or null.</returns>
    public abstract IValueAction? RepairSContainer(PropertyTarget target);

    /// <summary>
    /// Gets the text representation of a target for a specific edit feature.
    /// </summary>
    /// <param name="target">The property target.</param>
    /// <param name="feature">The advanced edit feature to apply.</param>
    /// <returns>The text representation, or null.</returns>
    public abstract string? GetText(PropertyTarget target, ViewAdvancedEditFeatures feature);

    /// <summary>
    /// Sets the text value for a target using a specific edit feature.
    /// </summary>
    /// <param name="target">The property target.</param>
    /// <param name="feature">The advanced edit feature to apply.</param>
    /// <param name="text">The text value to set.</param>
    /// <returns>An <see cref="IValueAction"/> if successful, or null.</returns>
    internal abstract IValueAction? SetText(PropertyTarget target, ViewAdvancedEditFeatures feature, string text);

    /// <summary>
    /// Sets a dynamic action type for the target.
    /// </summary>
    /// <param name="target">The property target.</param>
    /// <param name="dynamicType">The dynamic type to set, or null.</param>
    /// <returns>An <see cref="IValueAction"/> if successful, or null.</returns>
    public abstract IValueAction? SetDynamicAction(PropertyTarget target, Type? dynamicType);

    /// <summary>
    /// Converts a property target to a preview path.
    /// </summary>
    /// <param name="target">The property target to convert.</param>
    /// <returns>A <see cref="PreviewPath"/> representing the target.</returns>
    public abstract PreviewPath ToPreviewPath(PropertyTarget target);
}

/// <summary>
/// External abstraction for creating value actions that modify property data.
/// </summary>
internal abstract class ActionSetterExternal
{
    /// <summary>
    /// Creates a value action that sets values on a target.
    /// </summary>
    /// <param name="target">The value target to modify.</param>
    /// <param name="objects">The new values to set.</param>
    /// <param name="undoValues">Optional previous values for undo support.</param>
    /// <returns>An <see cref="IValueAction"/> representing the set operation.</returns>
    public abstract IValueAction SetValuesAction(IValueTarget target, IEnumerable<object?> objects, IEnumerable<object?>? undoValues = null);

    /// <summary>
    /// Creates a value action that sets the count of an array target.
    /// </summary>
    /// <param name="target">The array target to modify.</param>
    /// <param name="counts">The new counts to set.</param>
    /// <returns>An <see cref="IValueAction"/> representing the array count change.</returns>
    public abstract IValueAction SetArrayCountAction(ArrayTarget target, IEnumerable<int> counts);

    /// <summary>
    /// Creates a value action that pushes items into an array at specific positions.
    /// </summary>
    /// <param name="target">The array target to modify.</param>
    /// <param name="objects">The objects to insert.</param>
    /// <returns>An <see cref="IValueAction"/> representing the insert operation.</returns>
    public abstract IValueAction PushArrayItemAtAction(ArrayTarget target, IEnumerable<object?> objects);

    /// <summary>
    /// Creates a value action that removes an item from an array at a specific index.
    /// </summary>
    /// <param name="target">The array target to modify.</param>
    /// <param name="index">The index of the item to remove.</param>
    /// <returns>An <see cref="IValueAction"/> representing the remove operation.</returns>
    public abstract IValueAction RemoveArrayItemAtAction(ArrayTarget target, int index);

    /// <summary>
    /// Creates a value action that removes multiple items from an array.
    /// </summary>
    /// <param name="target">The array target to modify.</param>
    /// <param name="indexes">The indexes of the items to remove.</param>
    /// <returns>An <see cref="IValueAction"/> representing the remove operation.</returns>
    public abstract IValueAction RemoveArrayItemAtAction(ArrayTarget target, IEnumerable<int> indexes);

    /// <summary>
    /// Creates a value action that moves an item from one index to another in an array.
    /// </summary>
    /// <param name="target">The array target to modify.</param>
    /// <param name="index">The current index of the item.</param>
    /// <param name="indexTo">The destination index.</param>
    /// <returns>An <see cref="IValueAction"/> representing the move operation.</returns>
    public abstract IValueAction RemoveInsertItemAction(ArrayTarget target, int index, int indexTo);

    /// <summary>
    /// Creates a value action that clones an item at a specific index in an array.
    /// </summary>
    /// <param name="target">The array target to modify.</param>
    /// <param name="index">The index of the item to clone.</param>
    /// <returns>An <see cref="IValueAction"/> representing the clone operation.</returns>
    public abstract IValueAction CloneArrayItemAtAction(ArrayTarget target, int index);

    /// <summary>
    /// Creates a value action that swaps an item at a specific index with another in an array.
    /// </summary>
    /// <param name="target">The array target to modify.</param>
    /// <param name="index">The index of the item to swap.</param>
    /// <returns>An <see cref="IValueAction"/> representing the swap operation.</returns>
    public abstract IValueAction SwapArrayItemAtAction(ArrayTarget target, int index);
}

/// <summary>
/// External abstraction for type-specific value editor templates.
/// </summary>
internal abstract class EditorTemplateExternal
{
    /// <summary>
    /// Renders a boolean value editor.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The value action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the boolean editor.</returns>
    public abstract ImGuiNode BooleanEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler);

    /// <summary>
    /// Renders a string value editor.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The value action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the string editor.</returns>
    public abstract ImGuiNode StringEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler);

    /// <summary>
    /// Renders a text block value editor.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The value action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the text block editor, or null if not applicable.</returns>
    public abstract ImGuiNode? TextBlockEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler);

    /// <summary>
    /// Renders a numeric value editor for a specific struct type.
    /// </summary>
    /// <typeparam name="T">The numeric struct type.</typeparam>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The value action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the numeric editor.</returns>
    public abstract ImGuiNode NumericEditor<T>(ImGui gui, IValueTarget target, Action<IValueAction> handler) where T : struct;

    /// <summary>
    /// Renders an enum value editor.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The value action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the enum editor, or null if not applicable.</returns>
    public abstract ImGuiNode? EnumEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler);

    /// <summary>
    /// Renders a GUID value editor.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The value action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the GUID editor, or null if not applicable.</returns>
    public abstract ImGuiNode? GuidEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler);

    /// <summary>
    /// Renders a DateTime value editor.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The value action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the DateTime editor.</returns>
    public abstract ImGuiNode DateTimeEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler);

    /// <summary>
    /// Renders a color value editor.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The value action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the color editor.</returns>
    public abstract ImGuiNode ColorEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler);

    /// <summary>
    /// Renders an editor for empty or missing values.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The value action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the empty value editor.</returns>
    public abstract ImGuiNode EmptyValueEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler);

    /// <summary>
    /// Renders a button-style value editor.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The value action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the button value editor.</returns>
    public abstract ImGuiNode ButtonValueEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler);

    /// <summary>
    /// Renders a selection editor for picking values.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The value action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the selection editor, or null if not applicable.</returns>
    public abstract ImGuiNode? SelectionEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler);

    /// <summary>
    /// Renders an asset selection editor for picking assets.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The value action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the asset selection editor, or null if not applicable.</returns>
    public abstract ImGuiNode? AssetSelectionEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler);

    /// <summary>
    /// Renders a type design selection editor.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The value action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the type design selection editor, or null if not applicable.</returns>
    public abstract ImGuiNode? TypeDesignSelectionEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler);

    /// <summary>
    /// Renders a selection editor with customizable placement text and drag-drop behavior.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The value action handler.</param>
    /// <param name="placementTextAction">An optional function for custom placement text.</param>
    /// <param name="dragDropFunc">An optional drag-drop function.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the selection editor template, or null if not applicable.</returns>
    public abstract ImGuiNode? SelectionEditorTemplate(
        ImGui gui,
        IValueTarget target,
        Action<IValueAction> handler,
        SelectionPlacementTextFunc? placementTextAction = null,
        SelectionDragDropFunc? dragDropFunc = null);

    /// <summary>
    /// Renders an enum selection editor.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The value action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the enum selection editor, or null if not applicable.</returns>
    public abstract ImGuiNode? EnumSelectionEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler);

    /// <summary>
    /// Processes input state for a selection editor node.
    /// </summary>
    /// <param name="pipeline">The GUI pipeline.</param>
    /// <param name="node">The selection editor node.</param>
    /// <param name="input">The graphic input.</param>
    /// <param name="baseAction">The base child input function.</param>
    /// <returns>A <see cref="GuiInputState"/> representing the processed input.</returns>
    public abstract GuiInputState SelectionInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction);
}

/// <summary>
/// External abstraction for S-value (serialized value) editors.
/// </summary>
internal abstract class SValueEditorExternal
{
    /// <summary>
    /// Renders an editor for SKey values.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The value action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the SKey editor, or null if not applicable.</returns>
    public abstract ImGuiNode? SKeyEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler);

    /// <summary>
    /// Renders an editor for SAssetKey values.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The value action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the SAssetKey editor, or null if not applicable.</returns>
    public abstract ImGuiNode? SAssetKeyEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler);

    /// <summary>
    /// Renders an editor for SEnum values.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The value action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the SEnum editor, or null if not applicable.</returns>
    public abstract ImGuiNode? SEnumEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler);

    /// <summary>
    /// Renders an editor for SBoolean values.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The value action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the SBoolean editor, or null if not applicable.</returns>
    public abstract ImGuiNode? SBooleanEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler);

    /// <summary>
    /// Renders an editor for SString values.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The value action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the SString editor, or null if not applicable.</returns>
    public abstract ImGuiNode? SStringEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler);

    /// <summary>
    /// Renders an editor for STextBlock values.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The value action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the STextBlock editor, or null if not applicable.</returns>
    public abstract ImGuiNode? STextBlockEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler);

    /// <summary>
    /// Renders an editor for SNumeric values.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The value action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the SNumeric editor, or null if not applicable.</returns>
    public abstract ImGuiNode? SNumericEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler);

    /// <summary>
    /// Renders an editor for SDateTime values.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The value action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the SDateTime editor, or null if not applicable.</returns>
    public abstract ImGuiNode? SDateTimeEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler);

    /// <summary>
    /// Renders an editor for SPendingValue values.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The value action handler.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the SPendingValue editor, or null if not applicable.</returns>
    public abstract ImGuiNode? SPendingValueEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler);
}