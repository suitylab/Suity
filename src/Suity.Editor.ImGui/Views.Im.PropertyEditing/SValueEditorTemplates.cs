using System;

namespace Suity.Views.Im.PropertyEditing;

/// <summary>
/// Provides extension methods for creating SValue editor nodes in ImGui-based property editing views.
/// </summary>
public static class SValueEditorTemplates
{
    /// <summary>
    /// External implementation provider for SValue editor templates.
    /// </summary>
    internal static SValueEditorExternal _external;

    /// <summary>
    /// Creates an editor node for a key value.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The action handler for value changes.</param>
    /// <returns>An ImGuiNode representing the editor, or null if creation failed.</returns>
    public static ImGuiNode? SKeyEditor(this ImGui gui, IValueTarget target, Action<IValueAction> handler)
        => _external.SKeyEditor(gui, target, handler);

    /// <summary>
    /// Creates an editor node for an asset key value.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The action handler for value changes.</param>
    /// <returns>An ImGuiNode representing the editor, or null if creation failed.</returns>
    public static ImGuiNode? SAssetKeyEditor(this ImGui gui, IValueTarget target, Action<IValueAction> handler)
        => _external.SAssetKeyEditor(gui, target, handler);

    /// <summary>
    /// Creates an editor node for an enum value.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The action handler for value changes.</param>
    /// <returns>An ImGuiNode representing the editor, or null if creation failed.</returns>
    public static ImGuiNode? SEnumEditor(this ImGui gui, IValueTarget target, Action<IValueAction> handler)
        => _external.SEnumEditor(gui, target, handler);

    /// <summary>
    /// Creates an editor node for a boolean value.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The action handler for value changes.</param>
    /// <returns>An ImGuiNode representing the editor, or null if creation failed.</returns>
    public static ImGuiNode? SBooleanEditor(this ImGui gui, IValueTarget target, Action<IValueAction> handler)
        => _external.SBooleanEditor(gui, target, handler);

    /// <summary>
    /// Creates an editor node for a string value.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The action handler for value changes.</param>
    /// <returns>An ImGuiNode representing the editor, or null if creation failed.</returns>
    public static ImGuiNode? SStringEditor(this ImGui gui, IValueTarget target, Action<IValueAction> handler)
        => _external.SStringEditor(gui, target, handler);

    /// <summary>
    /// Creates an editor node for a numeric value.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The action handler for value changes.</param>
    /// <returns>An ImGuiNode representing the editor, or null if creation failed.</returns>
    public static ImGuiNode? SNumericEditor(this ImGui gui, IValueTarget target, Action<IValueAction> handler)
        => _external.SNumericEditor(gui, target, handler);

    /// <summary>
    /// Creates an editor node for a DateTime value.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The action handler for value changes.</param>
    /// <returns>An ImGuiNode representing the editor, or null if creation failed.</returns>
    public static ImGuiNode? SDateTimeEditor(this ImGui gui, IValueTarget target, Action<IValueAction> handler)
        => _external.SDateTimeEditor(gui, target, handler);

    /// <summary>
    /// Creates an editor node for a pending value.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The action handler for value changes.</param>
    /// <returns>An ImGuiNode representing the editor, or null if creation failed.</returns>
    public static ImGuiNode? SPendingValueEditor(this ImGui gui, IValueTarget target, Action<IValueAction> handler)
        => _external.SPendingValueEditor(gui, target, handler);
}