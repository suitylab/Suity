using System.Runtime.CompilerServices;

namespace Suity.Views.Im;

/// <summary>
/// Extension methods for creating layout nodes in ImGui.
/// </summary>
public static class GuiLayoutExtensions
{
    /// <summary>
    /// Creates a fill layout with auto-generated ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="line">The caller line number, used for auto-generating a unique ID.</param>
    /// <param name="member">The caller member name, used for auto-generating a unique ID.</param>
    /// <returns>The created <see cref="ImGuiNode"/> for the fill layout.</returns>
    public static ImGuiNode FillLayout(this ImGui gui, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
        => FillLayout(gui, $"##f_layout#{member}#{line}");

    /// <summary>
    /// Creates a fill layout that fills the entire available space.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the layout node.</param>
    /// <returns>The created <see cref="ImGuiNode"/> for the fill layout.</returns>
    public static ImGuiNode FillLayout(this ImGui gui, string id)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = "Layout";
            node.SetLayoutFunction(ImGuiLayoutSystem.Fill);
        }
        node.Layout();
        return node;
    }

    /// <summary>
    /// Creates a horizontal layout with auto-generated ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="line">The caller line number, used for auto-generating a unique ID.</param>
    /// <param name="member">The caller member name, used for auto-generating a unique ID.</param>
    /// <returns>The created <see cref="ImGuiNode"/> for the horizontal layout.</returns>
    public static ImGuiNode HorizontalLayout(this ImGui gui, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null) 
        => HorizontalLayout(gui, $"##h_layout#{member}#{line}");

    /// <summary>
    /// Creates a horizontal layout with the specified ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the layout node.</param>
    /// <returns>The created <see cref="ImGuiNode"/> for the horizontal layout.</returns>
    public static ImGuiNode HorizontalLayout(this ImGui gui, string id)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = "Layout";
            node.SetLayoutFunction(ImGuiLayoutSystem.Horizontal);
            node.SetFitFunction(ImGuiFitSystem.Auto);
            node.FitOrientation = GuiOrientation.Vertical;
        }
        node.Layout();
        return node;
    }

    /// <summary>
    /// Creates a vertical layout with auto-generated ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="line">The caller line number, used for auto-generating a unique ID.</param>
    /// <param name="member">The caller member name, used for auto-generating a unique ID.</param>
    /// <returns>The created <see cref="ImGuiNode"/> for the vertical layout.</returns>
    public static ImGuiNode VerticalLayout(this ImGui gui, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
        => VerticalLayout(gui, $"##v_layout#{member}#{line}");

    /// <summary>
    /// Creates a vertical layout with the specified ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the layout node.</param>
    /// <returns>The created <see cref="ImGuiNode"/> for the vertical layout.</returns>
    public static ImGuiNode VerticalLayout(this ImGui gui, string id)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = "Layout";
            node.SetLayoutFunction(ImGuiLayoutSystem.Vertical);
            node.SetFitFunction(ImGuiFitSystem.Auto);
            node.FitOrientation = GuiOrientation.Vertical;
        }
        node.Layout();
        return node;
    }

    /// <summary>
    /// Creates a horizontal reverse layout with auto-generated ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="line">The caller line number, used for auto-generating a unique ID.</param>
    /// <param name="member">The caller member name, used for auto-generating a unique ID.</param>
    /// <returns>The created <see cref="ImGuiNode"/> for the horizontal reverse layout.</returns>
    public static ImGuiNode HorizontalReverseLayout(this ImGui gui, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
        => HorizontalReverseLayout(gui, $"##hr_layout#{member}#{line}");

    /// <summary>
    /// Creates a horizontal reverse layout with the specified ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the layout node.</param>
    /// <returns>The created <see cref="ImGuiNode"/> for the horizontal reverse layout.</returns>
    public static ImGuiNode HorizontalReverseLayout(this ImGui gui, string id)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = "Layout";
            node.SetLayoutFunction(ImGuiLayoutSystem.HorizontalReverse);
            node.SetFitFunction(ImGuiFitSystem.Auto);
            node.FitOrientation = GuiOrientation.Vertical;
        }
        node.Layout();
        return node;
    }

    /// <summary>
    /// Creates a vertical reverse layout with auto-generated ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="line">The caller line number, used for auto-generating a unique ID.</param>
    /// <param name="member">The caller member name, used for auto-generating a unique ID.</param>
    /// <returns>The created <see cref="ImGuiNode"/> for the vertical reverse layout.</returns>
    public static ImGuiNode VerticalReverseLayout(this ImGui gui, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
        => VerticalReverseLayout(gui, $"##vr_layout#{member}#{line}");

    /// <summary>
    /// Creates a vertical reverse layout with the specified ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the layout node.</param>
    /// <returns>The created <see cref="ImGuiNode"/> for the vertical reverse layout.</returns>
    public static ImGuiNode VerticalReverseLayout(this ImGui gui, string id)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = "Layout";
            node.SetLayoutFunction(ImGuiLayoutSystem.VerticalReverse);
            node.SetFitFunction(ImGuiFitSystem.Auto);
            node.FitOrientation = GuiOrientation.Vertical;
        }
        node.Layout();
        return node;
    }

    /// <summary>
    /// Creates an overlay layout with auto-generated ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="line">The caller line number, used for auto-generating a unique ID.</param>
    /// <param name="member">The caller member name, used for auto-generating a unique ID.</param>
    /// <returns>The created <see cref="ImGuiNode"/> for the overlay layout.</returns>
    public static ImGuiNode OverlayLayout(this ImGui gui, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
        => OverlayLayout(gui, $"##o_layout#{member}#{line}");

    /// <summary>
    /// Creates an overlay layout where children stack on top of each other.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the layout node.</param>
    /// <returns>The created <see cref="ImGuiNode"/> for the overlay layout.</returns>
    public static ImGuiNode OverlayLayout(this ImGui gui, string id)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = "Layout";
            node.SetLayoutFunction(ImGuiLayoutSystem.Overlay);
            node.SetFitFunction(ImGuiFitSystem.Overlay);
            node.SetOverlapped(true);
            node.FitOrientation = GuiOrientation.Both;
        }
        node.Layout();
        return node;
    }

    /// <summary>
    /// Sets horizontal layout on a node.
    /// </summary>
    /// <param name="node">The ImGui node to configure.</param>
    /// <param name="fit">If true, also applies auto-fit sizing to the node.</param>
    /// <returns>The configured <see cref="ImGuiNode"/> for chaining.</returns>
    public static ImGuiNode SetHorizontalLayout(this ImGuiNode node, bool fit = false)
    {
        node.SetLayoutFunction(ImGuiLayoutSystem.Horizontal);
        if (fit)
        {
            node.SetFitFunction(ImGuiFitSystem.Auto);
        }
        return node;
    }

    /// <summary>
    /// Sets vertical layout on a node.
    /// </summary>
    /// <param name="node">The ImGui node to configure.</param>
    /// <param name="fit">If true, also applies auto-fit sizing to the node.</param>
    /// <returns>The configured <see cref="ImGuiNode"/> for chaining.</returns>
    public static ImGuiNode SetVerticalLayout(this ImGuiNode node, bool fit = false)
    {
        node.SetLayoutFunction(ImGuiLayoutSystem.Vertical);
        if (fit)
        {
            node.SetFitFunction(ImGuiFitSystem.Auto);
        }
        return node;
    }

    /// <summary>
    /// Sets horizontal reverse layout on a node.
    /// </summary>
    /// <param name="node">The ImGui node to configure.</param>
    /// <param name="fit">If true, also applies auto-fit sizing to the node.</param>
    /// <returns>The configured <see cref="ImGuiNode"/> for chaining.</returns>
    public static ImGuiNode SetHorizontalReverseLayout(this ImGuiNode node, bool fit = false)
    {
        node.SetLayoutFunction(ImGuiLayoutSystem.HorizontalReverse);
        if (fit)
        {
            node.SetFitFunction(ImGuiFitSystem.Auto);
        }
        return node;
    }

    /// <summary>
    /// Sets vertical reverse layout on a node.
    /// </summary>
    /// <param name="node">The ImGui node to configure.</param>
    /// <param name="fit">If true, also applies auto-fit sizing to the node.</param>
    /// <returns>The configured <see cref="ImGuiNode"/> for chaining.</returns>
    public static ImGuiNode SetVerticalReverseLayout(this ImGuiNode node, bool fit = false)
    {
        node.SetLayoutFunction(ImGuiLayoutSystem.VerticalReverse);
        if (fit)
        {
            node.SetFitFunction(ImGuiFitSystem.Auto);
        }
        return node;
    }

    /// <summary>
    /// Sets horizontal layout only during initialization.
    /// </summary>
    /// <param name="node">The ImGui node to configure.</param>
    /// <param name="fit">If true, also applies auto-fit sizing to the node.</param>
    /// <returns>The configured <see cref="ImGuiNode"/> for chaining.</returns>
    public static ImGuiNode InitHorizontalLayout(this ImGuiNode node, bool fit = false)
    {
        if (node.IsInitializing)
        {
            node.SetLayoutFunction(ImGuiLayoutSystem.Horizontal);
            if (fit)
            {
                node.SetFitFunction(ImGuiFitSystem.Auto);
            }
        }
        return node;
    }

    /// <summary>
    /// Sets vertical layout only during initialization.
    /// </summary>
    /// <param name="node">The ImGui node to configure.</param>
    /// <param name="fit">If true, also applies auto-fit sizing to the node.</param>
    /// <returns>The configured <see cref="ImGuiNode"/> for chaining.</returns>
    public static ImGuiNode InitVerticalLayout(this ImGuiNode node, bool fit = false)
    {
        if (node.IsInitializing)
        {
            node.SetLayoutFunction(ImGuiLayoutSystem.Vertical);
            if (fit)
            {
                node.SetFitFunction(ImGuiFitSystem.Auto);
            }
        }
        return node;
    }

    /// <summary>
    /// Sets horizontal reverse layout only during initialization.
    /// </summary>
    /// <param name="node">The ImGui node to configure.</param>
    /// <param name="fit">If true, also applies auto-fit sizing to the node.</param>
    /// <returns>The configured <see cref="ImGuiNode"/> for chaining.</returns>
    public static ImGuiNode InitHorizontalReverseLayout(this ImGuiNode node, bool fit = false)
    {
        if (node.IsInitializing)
        {
            node.SetLayoutFunction(ImGuiLayoutSystem.HorizontalReverse);
            if (fit)
            {
                node.SetFitFunction(ImGuiFitSystem.Auto);
            }
        }
        return node;
    }

    /// <summary>
    /// Sets vertical reverse layout only during initialization.
    /// </summary>
    /// <param name="node">The ImGui node to configure.</param>
    /// <param name="fit">If true, also applies auto-fit sizing to the node.</param>
    /// <returns>The configured <see cref="ImGuiNode"/> for chaining.</returns>
    public static ImGuiNode InitVerticalReverseLayout(this ImGuiNode node, bool fit = false)
    {
        if (node.IsInitializing)
        {
            node.SetLayoutFunction(ImGuiLayoutSystem.VerticalReverse);
            if (fit)
            {
                node.SetFitFunction(ImGuiFitSystem.Auto);
            }
        }
        return node;
    }

    /// <summary>
    /// Sets overlay layout only during initialization.
    /// </summary>
    /// <param name="node">The ImGui node to configure.</param>
    /// <returns>The configured <see cref="ImGuiNode"/> for chaining.</returns>
    public static ImGuiNode InitOverlayLayout(this ImGuiNode node)
    {
        if (node.IsInitializing)
        {
            node.TypeName = "Layout";
            node.SetLayoutFunction(ImGuiLayoutSystem.VerticalReverse);
            node.SetFitFunction(ImGuiFitSystem.Auto);
            node.FitOrientation = GuiOrientation.Vertical;
        }
        node.Layout();
        return node;
    }

    /// <summary>
    /// Sets overlay layout on a node.
    /// </summary>
    /// <param name="node">The ImGui node to configure.</param>
    /// <returns>The configured <see cref="ImGuiNode"/> for chaining.</returns>
    public static ImGuiNode SetOverlayLayout(this ImGuiNode node)
    {
        node.TypeName = "Layout";
        node.SetLayoutFunction(ImGuiLayoutSystem.Overlay);
        node.SetFitFunction(ImGuiFitSystem.Overlay);
        node.FitOrientation = GuiOrientation.Both;
        node.Layout();
        return node;
    }

    /// <summary>
    /// Sets position-based layout only during initialization.
    /// </summary>
    /// <param name="node">The ImGui node to configure.</param>
    /// <returns>The configured <see cref="ImGuiNode"/> for chaining.</returns>
    public static ImGuiNode InitPositionLayout(this ImGuiNode node)
    {
        if (node.IsInitializing)
        {
            node.SetLayoutFunction(ImGuiLayoutSystem.Viewport);
        }
        return node;
    }
}
