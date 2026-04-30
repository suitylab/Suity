using System.Runtime.CompilerServices;

namespace Suity.Views.Im;

/// <summary>
/// Extension methods for creating text display controls in ImGui.
/// Provides methods for rendering single-line text, initialization-only text, and multi-line text areas.
/// </summary>
public static class GuiTextExtensions
{
    /// <summary>
    /// Creates a text display node with an auto-generated ID based on the caller's line and member name.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="text">The text content to display.</param>
    /// <param name="line">Automatically populated caller line number for unique ID generation.</param>
    /// <param name="member">Automatically populated caller member name for unique ID generation.</param>
    /// <returns>The created <see cref="ImGuiNode"/> for the text display.</returns>
    public static ImGuiNode Text(this ImGui gui, string text, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
        => gui.Text($"##text#{member}#{line}", text);

    /// <summary>
    /// Creates a text display node with the specified unique ID.
    /// Updates the displayed text dynamically when the content changes.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">Unique identifier for the node.</param>
    /// <param name="text">The text content to display.</param>
    /// <returns>The created <see cref="ImGuiNode"/> for the text display.</returns>
    public static ImGuiNode Text(this ImGui gui, string id, string text)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);

        if (node.IsInitializing)
        {
            node.TypeName = nameof(Text);
            node.SetRenderFunction();
            node.SetFitFunction();
            node.FitOrientation = GuiOrientation.Both;
        }

        text ??= string.Empty;
        if (node.Text != text)
        {
            node.Text = text;
            node.Fit();
        }

        node.Layout();

        return node;
    }

    /// <summary>
    /// Creates a text display node that sets the text content only during initialization.
    /// Subsequent calls will not update the displayed text.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">Unique identifier for the node.</param>
    /// <param name="initText">The initial text content to display.</param>
    /// <returns>The created <see cref="ImGuiNode"/> for the text display.</returns>
    public static ImGuiNode TextOnInit(this ImGui gui, string id, string initText)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);

        if (node.IsInitializing)
        {
            node.TypeName = nameof(Text);
            node.SetRenderFunction();
            node.SetFitFunction();

            node.FitOrientation = GuiOrientation.Both;
            node.Text = initText ?? string.Empty;
            node.Fit();
        }

        node.Layout();

        return node;
    }

    /// <summary>
    /// Creates a multi-line text area display with an auto-generated ID based on the caller's line and member name.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="text">The text content to display in the text area.</param>
    /// <param name="autoFit">Whether to automatically adjust the node size to fit the text content.</param>
    /// <param name="line">Automatically populated caller line number for unique ID generation.</param>
    /// <param name="member">Automatically populated caller member name for unique ID generation.</param>
    /// <returns>The created <see cref="ImGuiNode"/> for the text area display.</returns>
    public static ImGuiNode TextArea(this ImGui gui, string text, bool autoFit = true, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
        => gui.TextArea($"##text_area#{member}#{line}", text, autoFit);

    /// <summary>
    /// Creates a multi-line text area display with the specified unique ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">Unique identifier for the node.</param>
    /// <param name="text">The text content to display in the text area.</param>
    /// <param name="autoFit">Whether to automatically adjust the node size to fit the text content. Defaults to true.</param>
    /// <returns>The created <see cref="ImGuiNode"/> for the text area display.</returns>
    public static ImGuiNode TextArea(this ImGui gui, string id, string text, bool autoFit = true)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);

        if (node.IsInitializing)
        {
            node.TypeName = nameof(TextArea);
            if (autoFit)
            {
                node.SetFitFunction();
            }
            node.SetRenderFunction();
        }

        node.Text = text;
        node.Layout();

        return node;
    }
}
