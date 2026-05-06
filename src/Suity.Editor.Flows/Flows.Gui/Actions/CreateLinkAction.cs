using static Suity.Helpers.GlobalLocalizer;
using Suity.UndoRedos;

namespace Suity.Editor.Flows.Gui.Actions;

/// <summary>
/// Undo/redo action that creates a link between two flow node connectors.
/// </summary>
internal class CreateLinkAction : UndoRedoAction
{
    /// <inheritdoc/>
    public override string Name => L("Create Link");

    /// <summary>
    /// The flow view that will contain the link.
    /// </summary>
    readonly IFlowView _view;
    /// <summary>
    /// Name of the input (target) node.
    /// </summary>
    readonly string _inputNode;
    /// <summary>
    /// Name of the input connector.
    /// </summary>
    readonly string _inputConnector;
    /// <summary>
    /// Name of the output (source) node.
    /// </summary>
    readonly string _outputNode;
    /// <summary>
    /// Name of the output connector.
    /// </summary>
    readonly string _outputConnector;

    /// <summary>
    /// Initializes a new instance of <see cref="CreateLinkAction"/>.
    /// </summary>
    /// <param name="view">The flow view that will contain the link.</param>
    /// <param name="inputNode">Name of the input node.</param>
    /// <param name="inputConnector">Name of the input connector.</param>
    /// <param name="outputNode">Name of the output node.</param>
    /// <param name="outputConnector">Name of the output connector.</param>
    public CreateLinkAction(IFlowView view, string inputNode, string inputConnector, string outputNode, string outputConnector)
    {
        _view = view;
        _outputNode = outputNode;
        _outputConnector = outputConnector;
        _inputNode = inputNode;
        _inputConnector = inputConnector;
    }

    /// <inheritdoc/>
    public override void Do()
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

    /// <inheritdoc/>
    public override void Undo()
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
}
