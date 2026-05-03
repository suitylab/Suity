using Suity.Drawing;
using Suity.Editor.Services;
using System.Drawing;

namespace Suity.Views.Im;

/// <summary>
/// Extension methods for ImGuiNode and ImGui to provide color styling and UI helper functionality.
/// </summary>
public static class EditorImGuiExtensions
{
    /// <summary>
    /// Sets the color of an ImGuiNode based on the specified TextStatus.
    /// </summary>
    /// <param name="node">The ImGuiNode to set the color on.</param>
    /// <param name="status">The TextStatus used to determine the color.</param>
    /// <returns>The modified ImGuiNode with the applied color.</returns>
    public static ImGuiNode SetColor(this ImGuiNode node, TextStatus status)
    {
        node.Color = EditorServices.ColorConfig.GetStatusColor(status);

        return node;
    }

    /// <summary>
    /// Initializes the color of an ImGuiNode based on the specified TextStatus, but only during node initialization.
    /// </summary>
    /// <param name="node">The ImGuiNode to initialize the color on.</param>
    /// <param name="status">The TextStatus used to determine the color.</param>
    /// <returns>The modified ImGuiNode with the applied color, or the original node if not initializing.</returns>
    public static ImGuiNode InitColor(this ImGuiNode node, TextStatus status)
    {
        if (node.IsInitializing)
        {
            node.Color = EditorServices.ColorConfig.GetStatusColor(status);
        }

        return node;
    }

    /// <summary>
    /// Overrides the color style of an ImGuiNode by creating or updating its GuiColorStyle with a color based on the specified TextStatus.
    /// </summary>
    /// <param name="node">The ImGuiNode to override the color on.</param>
    /// <param name="status">The TextStatus used to determine the color.</param>
    /// <returns>The modified ImGuiNode with the overridden color style.</returns>
    public static ImGuiNode OverrideColor(this ImGuiNode node, TextStatus status)
    {
        var color = EditorServices.ColorConfig.GetStatusColor(status);

        node.GetOrCreateValue<GuiColorStyle>().Color = color;

        return node;
    }

    /// <summary>
    /// Initializes the override color of an ImGuiNode during node initialization by creating or updating its GuiColorStyle.
    /// </summary>
    /// <param name="node">The ImGuiNode to initialize the override color on.</param>
    /// <param name="status">The TextStatus used to determine the color.</param>
    /// <returns>The modified ImGuiNode with the initialized override color style, or the original node if not initializing.</returns>
    public static ImGuiNode InitOverrideColor(this ImGuiNode node, TextStatus status)
    {
        var color = EditorServices.ColorConfig.GetStatusColor(status);

        if (node.IsInitializing)
        {
            node.GetOrCreateValue<GuiColorStyle>().Color = color;
        }

        return node;
    }

    /// <summary>
    /// Creates a horizontal frame node containing an optional icon and a text display, styled as a number box.
    /// </summary>
    /// <param name="gui">The ImGui instance to create the node with.</param>
    /// <param name="id">The unique identifier for the node.</param>
    /// <param name="text">The text to display in the number box.</param>
    /// <param name="color">Optional color to apply to the node.</param>
    /// <param name="icon">Optional image to display as an icon.</param>
    /// <param name="iconDark">Whether to use dark styling for the icon.</param>
    /// <param name="tooltips">Optional tooltip text to display on hover.</param>
    /// <returns>The created ImGuiNode representing the number box.</returns>
    public static ImGuiNode NumberBox(this ImGui gui, string id, string text, Color? color = null, ImageDef icon = null, bool iconDark = false, string tooltips = null)
    {
        var node = gui.HorizontalFrame(id)
        .InitClass("refBox", "debug_draw")
        .OverrideColor(color)
        .OnContent(() =>
        {
            if (icon != null)
            {
                gui.Image("icon", icon).InitClass(iconDark ? "iconDark" : "icon", "debug_draw");
            }

            gui.Frame("inner")
            .InitClass("numBoxDark", "debug_draw")
            .OnContent(() =>
            {
                gui.Text("text", text).InitClass("numBoxText", "debug_draw");
            });
        });

        if (tooltips is { } && !string.IsNullOrWhiteSpace(tooltips))
        {
            node.SetToolTips(tooltips);
        }

        return node;
    }
}
