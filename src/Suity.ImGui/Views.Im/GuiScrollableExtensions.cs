using System;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Suity.Views.Im;

/// <summary>
/// Extension methods for creating scrollable containers in ImGui.
/// </summary>
public static class GuiScrollableExtensions
{
    /// <summary>
    /// Creates a scrollable frame with an auto-generated ID based on the caller's line and member name.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="scrollOrientation">The scrolling orientation (horizontal, vertical, or both). Defaults to both.</param>
    /// <param name="line">The caller's line number, used for generating a unique ID.</param>
    /// <param name="member">The caller's member name, used for generating a unique ID.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the scrollable frame.</returns>
    public static ImGuiNode ScrollableFrame(this ImGui gui, GuiOrientation scrollOrientation = GuiOrientation.Both, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
    {
        string id = $"##scroll_frame_{member}#{line}";
        return gui.ScrollableFrame(id, scrollOrientation);
    }

    /// <summary>
    /// Creates a scrollable frame with the specified ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the scrollable frame.</param>
    /// <param name="orientation">The scrolling orientation (horizontal, vertical, or both). Defaults to both.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the scrollable frame.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is null.</exception>
    public static ImGuiNode ScrollableFrame(this ImGui gui, string id, GuiOrientation orientation = GuiOrientation.Both)
    {
        if (id is null)
        {
            throw new ArgumentNullException(nameof(id));
        }
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = nameof(GuiCommonExtensions.Frame);
        }
        node.SetupScrollable(orientation);
        node.Layout();
        return node;
    }

    /// <summary>
    /// Configures a node as scrollable with the specified orientation, setting up input, layout, render, and fit functions.
    /// </summary>
    /// <param name="node">The node to configure as scrollable.</param>
    /// <param name="scrollOrientation">The scrolling orientation to apply.</param>
    public static void SetupScrollable(this ImGuiNode node, GuiOrientation scrollOrientation)
    {
        if (node.IsInitializing)
        {
            node.SetInputFunction(nameof(ScrollableFrame));
            node.SetLayoutFunction(ImGuiLayoutSystem.Vertical);
            node.SetRenderFunction(nameof(ScrollableFrame));
            node.SetFitFunction(ImGuiFitSystem.Scrollable);
        }
        node.FitOrientation = scrollOrientation;
        var value = node.GetOrCreateValue(() => new GuiScrollableValue { ScrollOrientation = scrollOrientation });
        node.SetInitialLayoutPosition(new PointF(-value.ScrollX, -value.ScrollY));
    }

    /// <summary>
    /// Gets whether manual scroll input is enabled for the node.
    /// </summary>
    /// <param name="node">The node to query.</param>
    /// <returns><c>true</c> if manual scroll input is enabled; otherwise, <c>false</c>.</returns>
    public static bool GetScrollManualInput(this ImGuiNode node)
    {
        var value = node.GetOrCreateValue<GuiScrollableValue>();
        return value.ManualInput;
    }

    /// <summary>
    /// Sets whether manual scroll input is enabled for the node.
    /// </summary>
    /// <param name="node">The node to configure.</param>
    /// <param name="manualInput"><c>true</c> to enable manual scroll input; otherwise, <c>false</c>.</param>
    /// <returns>The same <see cref="ImGuiNode"/> for method chaining.</returns>
    public static ImGuiNode SetScrollManualInput(this ImGuiNode node, bool manualInput)
    {
        var value = node.GetOrCreateValue<GuiScrollableValue>();
        value.ManualInput = manualInput;
        return node;
    }

    /// <summary>
    /// Gets the horizontal scroll rate as a normalized value between 0 and 1.
    /// </summary>
    /// <param name="node">The node to query.</param>
    /// <returns>The horizontal scroll rate, where 0 represents the leftmost position and 1 represents the rightmost.</returns>
    public static float GetScrollRateX(this ImGuiNode node) 
        => ImGuiExternal._external.GetScrollRateX(node);

    /// <summary>
    /// Gets the vertical scroll rate as a normalized value between 0 and 1.
    /// </summary>
    /// <param name="node">The node to query.</param>
    /// <returns>The vertical scroll rate, where 0 represents the topmost position and 1 represents the bottommost.</returns>
    public static float GetScrollRateY(this ImGuiNode node)
        => ImGuiExternal._external.GetScrollRateY(node);

    /// <summary>
    /// Sets the horizontal scroll rate for the node.
    /// </summary>
    /// <param name="node">The node to configure.</param>
    /// <param name="rate">The horizontal scroll rate (0-1), where 0 is leftmost and 1 is rightmost.</param>
    /// <returns>The same <see cref="ImGuiNode"/> for method chaining.</returns>
    public static ImGuiNode SetScrollRateX(this ImGuiNode node, float rate) 
        => ImGuiExternal._external.SetScrollRateX(node, rate);

    /// <summary>
    /// Sets the vertical scroll rate for the node.
    /// </summary>
    /// <param name="node">The node to configure.</param>
    /// <param name="rate">The vertical scroll rate (0-1), where 0 is topmost and 1 is bottommost.</param>
    /// <returns>The same <see cref="ImGuiNode"/> for method chaining.</returns>
    public static ImGuiNode SetScrollRateY(this ImGuiNode node, float rate)
        => ImGuiExternal._external.SetScrollRateY(node, rate);

    /// <summary>
    /// Gets the rectangle defining the position and size of the vertical scroll bar.
    /// </summary>
    /// <param name="node">The scrollable node.</param>
    /// <param name="value">The scrollable value containing scroll state information.</param>
    /// <returns>A <see cref="RectangleF"/> representing the vertical scroll bar's bounds.</returns>
    public static RectangleF GetVerticalScrollBarRect(this ImGuiNode node, GuiScrollableValue value)
        => ImGuiExternal._external.GetVerticalScrollBarRect(node, value);

    /// <summary>
    /// Gets the rectangle defining the position and size of the horizontal scroll bar.
    /// </summary>
    /// <param name="node">The scrollable node.</param>
    /// <param name="value">The scrollable value containing scroll state information.</param>
    /// <returns>A <see cref="RectangleF"/> representing the horizontal scroll bar's bounds.</returns>
    public static RectangleF GetHorizontalScrollBarRect(this ImGuiNode node, GuiScrollableValue value) 
        => ImGuiExternal._external.GetHorizontalScrollBarRect(node, value);

    /// <summary>
    /// Adjusts the scroll bar position to fit the node's current content size and scroll state.
    /// </summary>
    /// <param name="node">The scrollable node to adjust.</param>
    public static void FitScrollBarPosition(this ImGuiNode node) 
        => ImGuiExternal._external.FitScrollBarPosition(node);

    /// <summary>
    /// Adjusts the scroll bar position using the specified scrollable value.
    /// </summary>
    /// <param name="node">The scrollable node to adjust.</param>
    /// <param name="value">The scrollable value containing scroll state information.</param>
    public static void FitScrollBarPosition(this ImGuiNode node, GuiScrollableValue value) 
        => ImGuiExternal._external.FitScrollBarPosition(node, value);

    /// <summary>
    /// Scrolls the node to make the specified rectangle visible within the viewport.
    /// </summary>
    /// <param name="node">The scrollable node.</param>
    /// <param name="rect">The rectangle to make visible.</param>
    /// <param name="relative">If <c>true</c>, the rectangle coordinates are treated as relative to the content; otherwise, absolute.</param>
    /// <returns><c>true</c> if the scroll position was changed; otherwise, <c>false</c>.</returns>
    public static bool ScrollToPositionY(this ImGuiNode node, RectangleF rect, bool relative) 
        => ImGuiExternal._external.ScrollToPositionY(node, rect, relative);

    /// <summary>
    /// Configures the node to automatically scroll to the bottom when content changes.
    /// </summary>
    /// <param name="node">The scrollable node to configure.</param>
    /// <returns>The same <see cref="ImGuiNode"/> for method chaining.</returns>
    public static ImGuiNode AutoScrollToBottom(this ImGuiNode node)
        => ImGuiExternal._external.AutoScrollToBottom(node);
}
