using System.Drawing;
using System.Runtime.CompilerServices;

namespace Suity.Views.Im;

/// <summary>
/// Extension methods for creating common ImGui frame and layout controls.
/// </summary>
public static class GuiCommonExtensions
{
    #region Frame

    /// <summary>
    /// Creates an empty frame with auto-generated ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="line">The caller line number for auto-generated ID.</param>
    /// <param name="member">The caller member name for auto-generated ID.</param>
    /// <returns>The created <see cref="ImGuiNode"/>.</returns>
    public static ImGuiNode EmptyFrame(this ImGui gui, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
         => EmptyFrame(gui, $"##empty_frame#{member}#{line}");

    /// <summary>
    /// Creates an empty frame with the specified ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the frame.</param>
    /// <returns>The created <see cref="ImGuiNode"/>.</returns>
    public static ImGuiNode EmptyFrame(this ImGui gui, string id)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = nameof(EmptyFrame);
        }
        node.Layout();
        return node;
    }

    /// <summary>
    /// Creates a frame with auto-generated ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="line">The caller line number for auto-generated ID.</param>
    /// <param name="member">The caller member name for auto-generated ID.</param>
    /// <returns>The created <see cref="ImGuiNode"/>.</returns>
    public static ImGuiNode Frame(this ImGui gui, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
         => Frame(gui, $"##frame#{member}#{line}");

    /// <summary>
    /// Creates a frame with vertical layout and the specified ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the frame.</param>
    /// <returns>The created <see cref="ImGuiNode"/>.</returns>
    public static ImGuiNode Frame(this ImGui gui, string id)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = nameof(Frame);
            node.SetLayoutFunction(ImGuiLayoutSystem.Vertical);
            node.SetRenderFunction(nameof(Frame));
            node.SetFitFunction(ImGuiFitSystem.Auto);
            node.FitOrientation = GuiOrientation.Vertical;
            node.SetPadding(node.Theme.FramePadding);
        }
        node.Layout();
        return node;
    }

    /// <summary>
    /// Creates a basic rectangle node with auto-generated ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="line">The caller line number for auto-generated ID.</param>
    /// <param name="member">The caller member name for auto-generated ID.</param>
    /// <returns>The created <see cref="ImGuiNode"/>.</returns>
    public static ImGuiNode Rect(this ImGui gui, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
         => Rect(gui, $"##rect#{member}#{line}");

    /// <summary>
    /// Creates a basic rectangle node with the specified ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the rectangle.</param>
    /// <returns>The created <see cref="ImGuiNode"/>.</returns>
    public static ImGuiNode Rect(this ImGui gui, string id)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = nameof(Frame);
            node.SetInputFunction();
            node.SetRenderFunction();
            node.SetFitFunction();
            node.FitOrientation = GuiOrientation.Both;
            node.OverrideBorder(0, null);
            node.OverrideCorner(0);
            node.SetPadding(0);
        }
        node.Layout();
        return node;
    }

    /// <summary>
    /// Creates a vertical frame with auto-generated ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="line">The caller line number for auto-generated ID.</param>
    /// <param name="member">The caller member name for auto-generated ID.</param>
    /// <returns>The created <see cref="ImGuiNode"/>.</returns>
    public static ImGuiNode VerticalFrame(this ImGui gui, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
         => Frame(gui, $"##v_frame#{member}#{line}");

    /// <summary>
    /// Creates a vertical frame with the specified ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the frame.</param>
    /// <returns>The created <see cref="ImGuiNode"/>.</returns>
    public static ImGuiNode VerticalFrame(this ImGui gui, string id) => gui.Frame(id);

    /// <summary>
    /// Creates a vertical reverse frame with auto-generated ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="line">The caller line number for auto-generated ID.</param>
    /// <param name="member">The caller member name for auto-generated ID.</param>
    /// <returns>The created <see cref="ImGuiNode"/>.</returns>
    public static ImGuiNode VerticalReverseFrame(this ImGui gui, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
         => VerticalReverseFrame(gui, $"##vr_frame_#{member}#{line}");

    /// <summary>
    /// Creates a vertical reverse frame with the specified ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the frame.</param>
    /// <returns>The created <see cref="ImGuiNode"/>.</returns>
    public static ImGuiNode VerticalReverseFrame(this ImGui gui, string id)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = nameof(Frame);
            node.SetLayoutFunction(ImGuiLayoutSystem.VerticalReverse);
            node.SetRenderFunction(nameof(Frame));
            node.SetFitFunction(ImGuiFitSystem.Auto);
            node.FitOrientation = GuiOrientation.Vertical;
            node.SetPadding(node.Theme.FramePadding);
        }
        node.Layout();
        return node;
    }

    /// <summary>
    /// Creates a horizontal frame with auto-generated ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="line">The caller line number for auto-generated ID.</param>
    /// <param name="member">The caller member name for auto-generated ID.</param>
    /// <returns>The created <see cref="ImGuiNode"/>.</returns>
    public static ImGuiNode HorizontalFrame(this ImGui gui, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
         => HorizontalFrame(gui, $"##h_frame_#{member}#{line}");

    /// <summary>
    /// Creates a horizontal frame with the specified ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the frame.</param>
    /// <returns>The created <see cref="ImGuiNode"/>.</returns>
    public static ImGuiNode HorizontalFrame(this ImGui gui, string id)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = nameof(Frame);
            node.SetLayoutFunction(ImGuiLayoutSystem.Horizontal);
            node.SetRenderFunction(nameof(Frame));
            node.SetFitFunction(ImGuiFitSystem.Auto);
            node.FitOrientation = GuiOrientation.Vertical;
            node.SetPadding(node.Theme.FramePadding);
        }
        node.Layout();
        return node;
    }

    /// <summary>
    /// Creates a horizontal reverse frame with auto-generated ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="line">The caller line number for auto-generated ID.</param>
    /// <param name="member">The caller member name for auto-generated ID.</param>
    /// <returns>The created <see cref="ImGuiNode"/>.</returns>
    public static ImGuiNode HorizontalReverseFrame(this ImGui gui, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
         => HorizontalReverseFrame(gui, $"##hr_frame_#{member}#{line}");

    /// <summary>
    /// Creates a horizontal reverse frame with the specified ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the frame. If null, an auto-generated ID is used.</param>
    /// <returns>The created <see cref="ImGuiNode"/>.</returns>
    public static ImGuiNode HorizontalReverseFrame(this ImGui gui, string? id)
    {
        id ??= $"##frame_{gui.CurrentNode.CurrentLayoutIndex}";
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = nameof(Frame);
            node.SetLayoutFunction(ImGuiLayoutSystem.HorizontalReverse);
            node.SetRenderFunction(nameof(Frame));
            node.SetFitFunction(ImGuiFitSystem.Auto);
            node.FitOrientation = GuiOrientation.Vertical;
            node.SetPadding(node.Theme.FramePadding);
        }
        node.Layout();
        return node;
    }

    /// <summary>
    /// Creates an overlay frame with auto-generated ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="line">The caller line number for auto-generated ID.</param>
    /// <param name="member">The caller member name for auto-generated ID.</param>
    /// <returns>The created <see cref="ImGuiNode"/>.</returns>
    public static ImGuiNode OverlayFrame(this ImGui gui, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
         => OverlayFrame(gui, $"##o_frame_#{member}#{line}");

    /// <summary>
    /// Creates an overlay frame with the specified ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the frame.</param>
    /// <returns>The created <see cref="ImGuiNode"/>.</returns>
    public static ImGuiNode OverlayFrame(this ImGui gui, string id)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = nameof(Frame);
            node.SetLayoutFunction(ImGuiLayoutSystem.Overlay);
            node.SetRenderFunction(nameof(Frame));
            node.SetFitFunction(ImGuiFitSystem.Overlay);
            node.FitOrientation = GuiOrientation.Both;
            node.SetPadding(node.Theme.FramePadding);
        }
        node.Layout();
        return node;
    }

    /// <summary>
    /// Gets the frame corner roundness, optionally scaled.
    /// </summary>
    /// <param name="node">The ImGui node.</param>
    /// <param name="scaled">Whether to apply global scaling to the value.</param>
    /// <returns>The corner roundness value.</returns>
    public static float GetFrameCornerRound(this ImGuiNode node, bool scaled = false)
    {
        float v = node.CornerRound ?? node.Theme.FrameCornerRound;
        if (scaled)
        {
            v = node.GlobalScaleValue(v);
        }
        return v;
    }

    /// <summary>
    /// Gets the checkbox corner roundness, optionally scaled.
    /// </summary>
    /// <param name="node">The ImGui node.</param>
    /// <param name="scaled">Whether to apply global scaling to the value.</param>
    /// <returns>The checkbox corner roundness value.</returns>
    public static float GetCheckBoxCornerRound(this ImGuiNode node, bool scaled = false)
    {
        float v = node.CornerRound ?? node.Theme.CheckBoxCornerRound;
        if (scaled)
        {
            v = node.GlobalScaleValue(v);
        }
        return v;
    }

    #endregion

    #region Line

    /// <summary>
    /// Creates a horizontal line with auto-generated ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="line">The caller line number for auto-generated ID.</param>
    /// <param name="member">The caller member name for auto-generated ID.</param>
    /// <returns>The created <see cref="ImGuiNode"/>.</returns>
    public static ImGuiNode HorizontalLine(this ImGui gui, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
         => HorizontalLine(gui, $"##h_line_#{member}#{line}");

    /// <summary>
    /// Creates a horizontal line with the specified ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the line.</param>
    /// <returns>The created <see cref="ImGuiNode"/>.</returns>
    public static ImGuiNode HorizontalLine(this ImGui gui, string id)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = "Line";
            node.SetRenderFunction(nameof(HorizontalLine));
        }
        node.Layout();
        return node;
    }

    /// <summary>
    /// Creates a vertical line with auto-generated ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="line">The caller line number for auto-generated ID.</param>
    /// <param name="member">The caller member name for auto-generated ID.</param>
    /// <returns>The created <see cref="ImGuiNode"/>.</returns>
    public static ImGuiNode VerticalLine(this ImGui gui, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
         => VerticalLine(gui, $"##v_line_#{member}#{line}");

    /// <summary>
    /// Creates a vertical line with the specified ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the line. If null, an auto-generated ID is used.</param>
    /// <returns>The created <see cref="ImGuiNode"/>.</returns>
    public static ImGuiNode VerticalLine(this ImGui gui, string id)
    {
        id ??= $"##v_line_{gui.CurrentNode.CurrentLayoutIndex}";
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = "Line";
            node.SetRenderFunction(nameof(VerticalLine));
        }
        node.Layout();
        return node;
    }

    #endregion

    #region Image

    /// <summary>
    /// Creates an image display node with auto-generated ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="image">The image to display.</param>
    /// <param name="fitOriginSize">Whether to fit the node to the original image size.</param>
    /// <param name="line">The caller line number for auto-generated ID.</param>
    /// <param name="member">The caller member name for auto-generated ID.</param>
    /// <returns>The created <see cref="ImGuiNode"/>.</returns>
    public static ImGuiNode Image(this ImGui gui, Image image, bool fitOriginSize = false, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
         => gui.Image($"##v_line_#{member}#{line}", image, fitOriginSize);

    /// <summary>
    /// Creates an image display node with the specified ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the image node.</param>
    /// <param name="image">The image to display.</param>
    /// <param name="fitOriginSize">Whether to fit the node to the original image size.</param>
    /// <returns>The created <see cref="ImGuiNode"/>.</returns>
    public static ImGuiNode Image(this ImGui gui, string id, Image image, bool fitOriginSize = false)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = nameof(Image);
            node.SetRenderFunction(nameof(Image));
            if (fitOriginSize)
            {
                node.SetFitFunction(nameof(Image));
            }
        }
        var value = node.GetOrCreateValue<GuiImageValue>();
        value.Image = image;
        node.Layout();
        return node;
    }

    /// <summary>
    /// Sets an image filter color on a node.
    /// </summary>
    /// <param name="node">The ImGui node.</param>
    /// <param name="color">The filter color to apply, or null to remove the filter.</param>
    /// <returns>The same <see cref="ImGuiNode"/> for chaining.</returns>
    public static ImGuiNode SetImageFilter(this ImGuiNode node, Color? color)
    {
        if (color.HasValue)
        {
            var value = node.GetOrCreateValue<GuiImageFilterStyle>();
            value.Color = color;
        }
        else
        {
            node.RemoveValue<GuiImageFilterStyle>();
        }
        return node;
    }

    /// <summary>
    /// Sets an image filter color only during initialization.
    /// </summary>
    /// <param name="node">The ImGui node.</param>
    /// <param name="color">The filter color to apply, or null to remove the filter.</param>
    /// <returns>The same <see cref="ImGuiNode"/> for chaining.</returns>
    public static ImGuiNode InitImageFilter(this ImGuiNode node, Color? color)
    {
        if (node.IsInitializing)
        {
            if (color.HasValue)
            {
                var value = node.GetOrCreateValue<GuiImageFilterStyle>();
                value.Color = color;
            }
            else
            {
                node.RemoveValue<GuiImageFilterStyle>();
            }
        }
        return node;
    }

    #endregion

    #region Viewport

    /// <summary>
    /// Creates a viewport node for zoomable and pannable views.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the viewport.</param>
    /// <param name="value">The viewport value containing zoom and pan state.</param>
    /// <returns>The created <see cref="ImGuiNode"/>.</returns>
    public static ImGuiNode Viewport(this ImGui gui, string id, GuiViewportValue value)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = nameof(Viewport);
            node.SetLayoutFunction(nameof(Viewport));
            node.SetInputFunction(nameof(Viewport));
            node.SetValue(value);
            node.IsMouseDragOutSideEvent = true;
            node.IsNoTransform = true;
        }
        return node;
    }

    #endregion
}
