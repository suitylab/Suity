using static Suity.Helpers.GlobalLocalizer;
using Suity.UndoRedos;

namespace Suity.Editor.Flows.Gui.Actions;

/// <summary>
/// Undo/redo action that removes a link between two flow node connectors.
/// </summary>
internal class DeleteLinkAction : UndoRedoAction
{
    /// <inheritdoc/>
    public override string Name => L("Delete Links");

    /// <summary>
    /// The flow view that contains the link.
    /// </summary>
    private readonly IFlowView _view;

    private readonly NodeLink _link;

    /// <summary>
    /// Initializes a new instance of <see cref="DeleteLinkAction"/> from an existing <see cref="NodeLink"/>.
    /// </summary>
    /// <param name="view">The flow view that contains the link.</param>
    /// <param name="link">The link to remove.</param>
    public DeleteLinkAction(IFlowView view, NodeLink link)
    {
        _view = view;
        _link = link.Clone();
    }

    /// <summary>
    /// Initializes a new instance of <see cref="DeleteLinkAction"/> with explicit connector identifiers.
    /// </summary>
    /// <param name="view">The flow view that contains the link.</param>
    /// <param name="inputNode">Name of the input node.</param>
    /// <param name="inputConnector">Name of the input connector.</param>
    /// <param name="outputNode">Name of the output node.</param>
    /// <param name="outputConnector">Name of the output connector.</param>
    public DeleteLinkAction(IFlowView view, string inputNode, string inputConnector, string outputNode, string outputConnector)
    {
        _view = view;
        _link = new NodeLink(inputNode, inputConnector, outputNode, outputConnector);
    }

    /// <inheritdoc/>
    public override void Do()
    {
        _view.Diagram.RemoveLink(_link.FromNode, _link.FromConnector, _link.ToNode, _link.ToConnector);
        _view.Diagram.RefreshView();
        _view.Diagram.QueueComputeData();

        QueuedAction.Do(() => 
        {
            var inputNode = _view.GetViewNode(_link.FromNode);
            var outputNode = _view.GetViewNode(_link.ToNode);
            _view.RefreshNodes([inputNode, outputNode]);

            EditorUtility.Inspector.UpdateInspector();
        });
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        _view.Diagram.AddLink(_link.FromNode, _link.FromConnector, _link.ToNode, _link.ToConnector);
        _view.Diagram.RefreshView();
        _view.Diagram.QueueComputeData();

        QueuedAction.Do(() => 
        {
            var inputNode = _view.GetViewNode(_link.FromNode);
            var outputNode = _view.GetViewNode(_link.ToNode);
            _view.RefreshNodes([inputNode, outputNode]);

            EditorUtility.Inspector.UpdateInspector();
        });
    }
}