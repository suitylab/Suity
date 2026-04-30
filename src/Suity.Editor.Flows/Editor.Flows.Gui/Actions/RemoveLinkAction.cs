using static Suity.Helpers.GlobalLocalizer;
using Suity.UndoRedos;

namespace Suity.Editor.Flows.Gui.Actions;

/// <summary>
/// Undo/redo action that removes a link between two flow node connectors.
/// </summary>
internal class RemoveLinkAction : UndoRedoAction
{
    /// <inheritdoc/>
    public override string Name => L("Remove Link");

    /// <summary>
    /// The flow view that contains the link.
    /// </summary>
    private readonly IFlowView _view;
    /// <summary>
    /// Name of the input (target) node.
    /// </summary>
    private readonly string _inputNode;
    /// <summary>
    /// Name of the input connector.
    /// </summary>
    private readonly string _inputConnector;
    /// <summary>
    /// Name of the output (source) node.
    /// </summary>
    private readonly string _outputNode;
    /// <summary>
    /// Name of the output connector.
    /// </summary>
    private readonly string _outputConnector;

    /// <summary>
    /// Initializes a new instance of <see cref="RemoveLinkAction"/> from an existing <see cref="NodeLink"/>.
    /// </summary>
    /// <param name="view">The flow view that contains the link.</param>
    /// <param name="link">The link to remove.</param>
    public RemoveLinkAction(IFlowView view, NodeLink link)
        : this(view, link.FromNode, link.FromConnector, link.ToNode, link.ToConnector)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="RemoveLinkAction"/> with explicit connector identifiers.
    /// </summary>
    /// <param name="view">The flow view that contains the link.</param>
    /// <param name="inputNode">Name of the input node.</param>
    /// <param name="inputConnector">Name of the input connector.</param>
    /// <param name="outputNode">Name of the output node.</param>
    /// <param name="outputConnector">Name of the output connector.</param>
    public RemoveLinkAction(IFlowView view, string inputNode, string inputConnector, string outputNode, string outputConnector)
    {
        _view = view;
        _inputNode = inputNode;
        _inputConnector = inputConnector;
        _outputNode = outputNode;
        _outputConnector = outputConnector;
    }

    /// <inheritdoc/>
    public override void Do()
    {
        _view.Diagram.RemoveLink(_inputNode, _inputConnector, _outputNode, _outputConnector);
        _view.Diagram.RefreshView();
        _view.Diagram.QueueComputeData();

        QueuedAction.Do(() => 
        {
            var inputNode = _view.GetViewNode(_inputNode);
            var outputNode = _view.GetViewNode(_outputNode);
            _view.RefreshNodes([inputNode, outputNode]);

            EditorUtility.Inspector.UpdateInspector();
        });
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        _view.Diagram.AddLink(_inputNode, _inputConnector, _outputNode, _outputConnector);
        _view.Diagram.RefreshView();
        _view.Diagram.QueueComputeData();

        QueuedAction.Do(() => 
        {
            var inputNode = _view.GetViewNode(_inputNode);
            var outputNode = _view.GetViewNode(_outputNode);
            _view.RefreshNodes([inputNode, outputNode]);

            EditorUtility.Inspector.UpdateInspector();
        });
    }
}