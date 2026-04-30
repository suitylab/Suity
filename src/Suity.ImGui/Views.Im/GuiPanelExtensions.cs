using System;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Suity.Views.Im;

/// <summary>
/// Extension methods for creating panel and header controls in ImGui.
/// </summary>
public static class GuiPanelExtensions
{
    /// <summary>
    /// Creates a panel with auto-generated ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="title">The display title of the panel.</param>
    /// <param name="line">The caller line number for auto-generated ID.</param>
    /// <param name="member">The caller member name for auto-generated ID.</param>
    /// <returns>The created <see cref="ImGuiNode"/>.</returns>
    public static ImGuiNode Panel(this ImGui gui, string title, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
        => Panel(gui, $"##panel#{member}#{line}", title);

    /// <summary>
    /// Creates a panel with the specified ID and title.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the panel.</param>
    /// <param name="title">The display title of the panel.</param>
    /// <returns>The created <see cref="ImGuiNode"/>.</returns>
    public static ImGuiNode Panel(this ImGui gui, string id, string title)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = nameof(Panel);
            node.SetLayoutFunction(ImGuiLayoutSystem.Vertical);
            node.SetRenderFunction(nameof(Panel));
            node.SetFitFunction(ImGuiFitSystem.Auto);
            node.FitOrientation = GuiOrientation.Vertical;
            node.Padding = node.Theme.FramePadding;
            node.HeaderHeight = node.Theme.HeaderHeight;
            node.Text = title;
        }
        node.Layout();
        return node;
    }

    /// <summary>
    /// Creates an expandable panel with auto-generated ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="title">The display title of the panel.</param>
    /// <param name="initExpanded">Whether the panel should be initially expanded.</param>
    /// <param name="line">The caller line number for auto-generated ID.</param>
    /// <param name="member">The caller member name for auto-generated ID.</param>
    /// <returns>The created <see cref="ImGuiNode"/>.</returns>
    public static ImGuiNode ExpandablePanel(this ImGui gui, string title, bool initExpanded = false, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
        => ExpandablePanel(gui, $"##ex_panel#{member}#{line}", title, initExpanded);

    /// <summary>
    /// Creates an expandable panel that can be collapsed/expanded.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the panel.</param>
    /// <param name="title">The display title of the panel.</param>
    /// <param name="initExpanded">Whether the panel should be initially expanded.</param>
    /// <returns>The created <see cref="ImGuiNode"/>.</returns>
    public static ImGuiNode ExpandablePanel(this ImGui gui, string id, string title, bool initExpanded = false)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = nameof(Panel);
            node.SetInputFunction(nameof(ExpandablePanel));
            node.SetLayoutFunction(ImGuiLayoutSystem.Vertical);
            node.SetRenderFunction(nameof(ExpandablePanel));
            node.SetFitFunction(ImGuiFitSystem.Expandable);
            node.FitOrientation = GuiOrientation.Vertical;
            node.Padding = node.Theme.FramePadding;
            node.HeaderHeight = node.Theme.HeaderHeight;
            node.Text = title;
            node.GetOrCreateValue(
                () => new GuiExpandableValue
                {
                    Expanded = initExpanded,
                }
            );
        }
        node.Layout();
        return node;
    }

    /// <summary>
    /// Sets the header color for a panel node.
    /// </summary>
    /// <param name="node">The panel node to modify.</param>
    /// <param name="color">The header color to set, or null to use the default.</param>
    /// <returns>The same <see cref="ImGuiNode"/> for chaining.</returns>
    public static ImGuiNode SetHeaderColor(this ImGuiNode node, Color? color)
    {
        node.HeaderColor = color;
        return node;
    }

    /// <summary>
    /// Sets the header width for a panel node.
    /// </summary>
    /// <param name="node">The panel node to modify.</param>
    /// <param name="width">The header width in pixels, or null to use the default.</param>
    /// <returns>The same <see cref="ImGuiNode"/> for chaining.</returns>
    public static ImGuiNode SetHeaderWidth(this ImGuiNode node, int? width)
    {
        node.HeaderWidth = width;
        return node;
    }

    /// <summary>
    /// Sets the header height for a panel node.
    /// </summary>
    /// <param name="node">The panel node to modify.</param>
    /// <param name="height">The header height in pixels, or null to use the default.</param>
    /// <returns>The same <see cref="ImGuiNode"/> for chaining.</returns>
    public static ImGuiNode SetHeaderHeight(this ImGuiNode node, int? height)
    {
        node.HeaderHeight = height;
        return node;
    }

    /// <summary>
    /// Gets whether an expandable panel is currently expanded.
    /// </summary>
    /// <param name="node">The panel node to check.</param>
    /// <returns>True if the panel is expanded; otherwise, false.</returns>
    public static bool GetIsExpanded(this ImGuiNode node)
    {
        return node.Expanded == true;
    }

    /// <summary>
    /// Executes content only when the panel is expanded.
    /// </summary>
    /// <param name="node">The panel node.</param>
    /// <param name="action">The action to execute when expanded.</param>
    /// <returns>The same <see cref="ImGuiNode"/> for chaining.</returns>
    public static ImGuiNode OnExpandContent(this ImGuiNode node, Action action)
    {
        if (node.GetIsExpanded())
        {
            node.OnContent(action);
        }
        return node;
    }

    /// <summary>
    /// Executes an action when the panel is expanded.
    /// </summary>
    /// <param name="node">The panel node.</param>
    /// <param name="action">The action to execute when expanded.</param>
    /// <returns>The same <see cref="ImGuiNode"/> for chaining.</returns>
    public static ImGuiNode OnExpand(this ImGuiNode node, Action action)
    {
        if (node.GetIsExpanded())
        {
            action();
        }
        return node;
    }

    /// <summary>
    /// Executes an action when the panel's expand state changes.
    /// </summary>
    /// <param name="node">The panel node.</param>
    /// <param name="action">The action to execute, receiving the node and current expanded state.</param>
    /// <returns>The same <see cref="ImGuiNode"/> for chaining.</returns>
    public static ImGuiNode OnToggleExpand(this ImGuiNode node, Action<ImGuiNode, bool> action)
    {
        if (node.GetIsExpanded())
        {
            action(node, true);
        }
        else
        {
            action(node, false);
        }
        return node;
    }
}
