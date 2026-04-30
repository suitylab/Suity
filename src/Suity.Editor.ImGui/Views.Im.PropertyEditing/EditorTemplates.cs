using Suity.Selecting;
using Suity.Views.Graphics;
using System;
using System.Collections.Generic;

namespace Suity.Views.Im.PropertyEditing;

/// <summary>
/// Delegate for generating placement text for a selection in the editor.
/// </summary>
/// <param name="node">The ImGui node where the selection is being placed.</param>
/// <param name="selections">The collection of selections being placed.</param>
public delegate void SelectionPlacementTextFunc(ImGuiNode node, IEnumerable<ISelection> selections);

/// <summary>
/// Delegate for handling drag-and-drop operations on a selection in the editor.
/// </summary>
/// <param name="dropEvent">The drag event containing drop information.</param>
/// <param name="selection">The selection being dropped.</param>
/// <returns>A string representing the result of the drag-drop operation, or null if not handled.</returns>
public delegate string? SelectionDragDropFunc(IDragEvent dropEvent, ISelection selection);

/// <summary>
/// Provides extension methods for creating property editor UI elements using ImGui.
/// These templates delegate to external implementations for rendering various property types.
/// </summary>
public static class EditorTemplates
{
    /// <summary>
    /// External implementation provider for editor templates.
    /// </summary>
    internal static EditorTemplateExternal _external;

    /// <summary>
    /// Creates a boolean property editor UI element.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The value target to bind the editor to.</param>
    /// <param name="handler">The action handler for value changes.</param>
    /// <returns>The created ImGui node.</returns>
    public static ImGuiNode BooleanEditor(this ImGui gui, IValueTarget target, Action<IValueAction> handler)
        => _external.BooleanEditor(gui, target, handler);

    /// <summary>
    /// Creates a string property editor UI element.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The value target to bind the editor to.</param>
    /// <param name="handler">The action handler for value changes.</param>
    /// <returns>The created ImGui node.</returns>
    public static ImGuiNode StringEditor(this ImGui gui, IValueTarget target, Action<IValueAction> handler)
        => _external.StringEditor(gui, target, handler);

    /// <summary>
    /// Creates a read-only text block editor UI element.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The value target to bind the editor to.</param>
    /// <param name="handler">The action handler for value changes.</param>
    /// <returns>The created ImGui node, or null if not applicable.</returns>
    public static ImGuiNode? TextBlockEditor(this ImGui gui, IValueTarget target, Action<IValueAction> handler)
        => _external.TextBlockEditor(gui, target, handler);

    /// <summary>
    /// Creates a numeric property editor UI element for the specified numeric type.
    /// </summary>
    /// <typeparam name="T">The numeric type (must be a struct).</typeparam>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The value target to bind the editor to.</param>
    /// <param name="handler">The action handler for value changes.</param>
    /// <returns>The created ImGui node.</returns>
    public static ImGuiNode NumericEditor<T>(this ImGui gui, IValueTarget target, Action<IValueAction> handler) where T : struct
        => _external.NumericEditor<T>(gui, target, handler);

    /// <summary>
    /// Creates an enum property editor UI element with a dropdown selector.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The value target to bind the editor to.</param>
    /// <param name="handler">The action handler for value changes.</param>
    /// <returns>The created ImGui node, or null if not applicable.</returns>
    public static ImGuiNode? EnumEditor(this ImGui gui, IValueTarget target, Action<IValueAction> handler)
        => _external.EnumEditor(gui, target, handler);

    /// <summary>
    /// Creates a GUID property editor UI element.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The value target to bind the editor to.</param>
    /// <param name="handler">The action handler for value changes.</param>
    /// <returns>The created ImGui node, or null if not applicable.</returns>
    public static ImGuiNode? GuidEditor(this ImGui gui, IValueTarget target, Action<IValueAction> handler)
        => _external.GuidEditor(gui, target, handler);

    /// <summary>
    /// Creates a DateTime property editor UI element.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The value target to bind the editor to.</param>
    /// <param name="handler">The action handler for value changes.</param>
    /// <returns>The created ImGui node.</returns>
    public static ImGuiNode DateTimeEditor(this ImGui gui, IValueTarget target, Action<IValueAction> handler)
        => _external.DateTimeEditor(gui, target, handler);

    /// <summary>
    /// Creates a color property editor UI element with a color picker.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The value target to bind the editor to.</param>
    /// <param name="handler">The action handler for value changes.</param>
    /// <returns>The created ImGui node.</returns>
    public static ImGuiNode ColorEditor(this ImGui gui, IValueTarget target, Action<IValueAction> handler)
        => _external.ColorEditor(gui, target, handler);

    /// <summary>
    /// Creates an editor UI element for empty or null values.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The value target to bind the editor to.</param>
    /// <param name="handler">The action handler for value changes.</param>
    /// <returns>The created ImGui node.</returns>
    public static ImGuiNode EmptyValueEditor(this ImGui gui, IValueTarget target, Action<IValueAction> handler)
        => _external.EmptyValueEditor(gui, target, handler);
    
    /// <summary>
    /// Creates a button-style property editor UI element.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The value target to bind the editor to.</param>
    /// <param name="handler">The action handler for value changes.</param>
    /// <returns>The created ImGui node.</returns>
    public static ImGuiNode ButtonValueEditor(this ImGui gui, IValueTarget target, Action<IValueAction> handler)
        => _external.ButtonValueEditor(gui, target, handler);

    /// <summary>
    /// Creates a selection property editor UI element for picking references.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The value target to bind the editor to.</param>
    /// <param name="handler">The action handler for value changes.</param>
    /// <returns>The created ImGui node, or null if not applicable.</returns>
    public static ImGuiNode? SelectionEditor(this ImGui gui, IValueTarget target, Action<IValueAction> handler)
        => _external.SelectionEditor(gui, target, handler);

    /// <summary>
    /// Creates an asset selection property editor UI element for picking assets.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The value target to bind the editor to.</param>
    /// <param name="handler">The action handler for value changes.</param>
    /// <returns>The created ImGui node, or null if not applicable.</returns>
    public static ImGuiNode? AssetSelectionEditor(this ImGui gui, IValueTarget target, Action<IValueAction> handler)
        => _external.AssetSelectionEditor(gui, target, handler);

    /// <summary>
    /// Creates a type design selection property editor UI element.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The value target to bind the editor to.</param>
    /// <param name="handler">The action handler for value changes.</param>
    /// <returns>The created ImGui node, or null if not applicable.</returns>
    public static ImGuiNode? TypeDesignSelectionEditor(this ImGui gui, IValueTarget target, Action<IValueAction> handler)
        => _external.TypeDesignSelectionEditor(gui, target, handler);

    /// <summary>
    /// Creates a customizable selection property editor UI element with optional placement text and drag-drop support.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The value target to bind the editor to.</param>
    /// <param name="handler">The action handler for value changes.</param>
    /// <param name="placementTextAction">Optional delegate for generating placement text.</param>
    /// <param name="dragDropFunc">Optional delegate for handling drag-drop operations.</param>
    /// <returns>The created ImGui node, or null if not applicable.</returns>
    public static ImGuiNode? SelectionEditorTemplate(
        this ImGui gui,
        IValueTarget target,
        Action<IValueAction> handler,
        SelectionPlacementTextFunc? placementTextAction = null,
        SelectionDragDropFunc? dragDropFunc = null)
        => _external.SelectionEditorTemplate(gui, target, handler, placementTextAction, dragDropFunc);

    /// <summary>
    /// Creates an enum selection property editor UI element.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The value target to bind the editor to.</param>
    /// <param name="handler">The action handler for value changes.</param>
    /// <returns>The created ImGui node, or null if not applicable.</returns>
    public static ImGuiNode? EnumSelectionEditor(this ImGui gui, IValueTarget target, Action<IValueAction> handler)
        => _external.EnumSelectionEditor(gui, target, handler);

    private static GuiInputState SelectionInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
        => _external.SelectionInput(pipeline, node, input, baseAction);
}