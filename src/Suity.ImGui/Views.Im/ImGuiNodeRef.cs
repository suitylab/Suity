using System;

namespace Suity.Views.Im;

/// <summary>
/// A strong reference to an ImGuiNode that automatically clears when the node's GUI is disposed.
/// </summary>
public sealed class ImGuiNodeRef
{
    private ImGuiNode? _node;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiNodeRef"/> class.
    /// </summary>
    public ImGuiNodeRef()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiNodeRef"/> class with the specified node.
    /// </summary>
    /// <param name="node">The node to reference.</param>
    public ImGuiNodeRef(ImGuiNode? node)
    {
        _node = node;
    }

    /// <summary>
    /// Gets or sets the referenced ImGuiNode. Returns null if the node's GUI has been disposed.
    /// </summary>
    public ImGuiNode? Node
    {
        get
        {
            var node = _node;

            if (node is null)
            {
                return null;
            }

            if (node.Gui is null)
            {
                _node = null;
                return null;
            }

            return node;
        }
        set
        {
            if (ReferenceEquals(value, _node))
            {
                return;
            }

            if (value?.Gui is not null)
            {
                _node = value;
            }
            else
            {
                _node = null;
            }
        }
    }

    /// <summary>
    /// Gets the ImGui instance associated with the referenced node, or null if the node is invalid.
    /// </summary>
    public ImGui? Gui => Node?.Gui;

    /// <summary>
    /// Queues a refresh request for the referenced node.
    /// </summary>
    /// <param name="redrawAll">If true, requests a full output redraw.</param>
    public void QueueRefresh(bool redrawAll = false)
    {
        if (Node is { } node)
        {
            node.QueueRefresh();
            if (redrawAll)
            {
                node.Gui?.Context?.RequestOutput();
            }
        }
    }
}

/// <summary>
/// A weak reference to an ImGuiNode that allows the node to be garbage collected.
/// </summary>
public sealed class ImGuiNodeWeakRef
{
    readonly WeakReference<ImGuiNode?> _ref;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiNodeWeakRef"/> class.
    /// </summary>
    public ImGuiNodeWeakRef()
    {
        _ref = new(null);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiNodeWeakRef"/> class with the specified node.
    /// </summary>
    /// <param name="node">The node to weakly reference.</param>
    public ImGuiNodeWeakRef(ImGuiNode? node)
    {
        _ref = new(node);
    }

    /// <summary>
    /// Gets or sets the referenced ImGuiNode. Returns null if the node has been garbage collected or its GUI disposed.
    /// </summary>
    public ImGuiNode? Node
    {
        get
        {
            if (!_ref.TryGetTarget(out var node) || node is null)
            {
                return null;
            }

            if (node.Gui is null)
            {
                _ref.SetTarget(null);
                return null;
            }

            return node;
        }
        set
        {
            if (value?.Gui is null)
            {
                value = null;
            }

            if (_ref.TryGetTarget(out var node) && ReferenceEquals(value, node))
            {
                return;
            }

            _ref.SetTarget(value);
        }
    }

    /// <summary>
    /// Gets the ImGui instance associated with the referenced node, or null if the node is invalid.
    /// </summary>
    public ImGui? Gui => Node?.Gui;

    /// <summary>
    /// Queues a refresh request for the referenced node.
    /// </summary>
    public void QueueRefresh()
    {
        Node?.QueueRefresh();
    }
}
