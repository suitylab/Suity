using static Suity.Helpers.GlobalLocalizer;
using Suity.UndoRedos;
using System.Drawing;

namespace Suity.Editor.Flows.Gui.Actions;

/// <summary>
/// Undo/redo action that resizes a flow diagram node to a new bounding rectangle.
/// </summary>
internal class NodeResizeAction : UndoRedoAction
{
    /// <inheritdoc/>
    public override string Name => L("Resize Node");

    /// <summary>
    /// The diagram item to resize.
    /// </summary>
    private readonly IFlowDiagramItem _node;
    /// <summary>
    /// The original bounding rectangle before resizing.
    /// </summary>
    private readonly Rectangle _oldBound;
    /// <summary>
    /// The target bounding rectangle after resizing.
    /// </summary>
    private readonly Rectangle _newBound;

    /// <summary>
    /// Flag indicating whether an undo has been performed.
    /// </summary>
    private bool _flag;

    /// <summary>
    /// Initializes a new instance of <see cref="NodeResizeAction"/>.
    /// </summary>
    /// <param name="item">The diagram item to resize.</param>
    /// <param name="oldBound">The original bounding rectangle before resizing.</param>
    /// <param name="newBound">The target bounding rectangle after resizing.</param>
    public NodeResizeAction(IFlowDiagramItem item, Rectangle oldBound, Rectangle newBound)
    {
        _node = item;
        _oldBound = oldBound;
        _newBound = newBound;
    }

    /// <inheritdoc/>
    public override void Do()
    {
        _node.SetBound(_newBound);

        _node.Diagram?.RefreshView();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        _flag = true;

        _node.SetBound(_oldBound);

        _node.Diagram?.RefreshView();
    }
}