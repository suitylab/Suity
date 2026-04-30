using Suity.Views.NodeGraph;
using System.Drawing;

namespace Suity.Views.Im.Flows;

/// <summary>
/// Viewport implementation for ImGui-based graph controls that handles coordinate transformations.
/// </summary>
public class ImGuiGraphViewport : GraphViewport
{
    private readonly ImGuiNodeRef _viewportNode = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiGraphViewport"/> class.
    /// </summary>
    /// <param name="control">The graph control that owns this viewport.</param>
    public ImGuiGraphViewport(GraphControl control) : base(control)
    {
    }

    /// <summary>
    /// Gets the viewport node reference.
    /// </summary>
    public ImGuiNodeRef ViewportNode => _viewportNode;

    /// <summary>
    /// Gets or sets the viewport ImGui node.
    /// </summary>
    public ImGuiNode? Node { get => _viewportNode.Node; set  => _viewportNode.Node = value; }

    /// <summary>
    /// Gets the ImGui instance associated with the viewport node.
    /// </summary>
    public ImGui? Gui => _viewportNode.Gui;

    /// <inheritdoc/>
    public override PointF ViewToControl(PointF point)
    {
        if (_viewportNode.Node is not { } node)
        {
            return base.ViewToControl(point);
        }

        var p = point;
        var rect = node.Rect;

        p = base.ViewToControl(p);
        p.X += rect.X;
        p.Y += rect.Y;

        p = node.GlobalScalePoint(p);

        return p;
    }

    /// <inheritdoc/>
    public override PointF ControlToView(PointF point)
    {
        if (_viewportNode.Node is not { } node)
        {
            return base.ControlToView(point);
        }

        var p = point;
        var rect = node.Rect;

        p = node.GlobalRevertScalePoint(p);

        p.X -= rect.X;
        p.Y -= rect.Y;
        p = base.ControlToView(p);

        return p;
    }

    /// <inheritdoc/>
    public override float ScaledViewZoom
    {
        get
        {
            float zoom = base.ViewZoom;
            if (_viewportNode.Node is { } node)
            {
                return node.GlobalScaleValue(zoom);
            }
            else
            {
                return zoom;
            }
        }
    }
}
