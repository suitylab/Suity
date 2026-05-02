using static Suity.Helpers.GlobalLocalizer;
using Suity.UndoRedos;
using System.Collections.Generic;

namespace Suity.Editor.Flows.Gui.Actions;

/// <summary>
/// Undo/redo action that removes links between flow node connectors.
/// </summary>
internal class DeleteLinkAction : UndoRedoAction
{
    /// <inheritdoc/>
    public override string Name => L("Delete Links");

    /// <summary>
    /// The flow view that contains the links.
    /// </summary>
    private readonly IFlowView _view;

    private readonly NodeLink[] _links;

    /// <summary>
    /// Initializes a new instance of <see cref="DeleteLinkAction"/> from an existing <see cref="NodeLink"/>.
    /// </summary>
    /// <param name="view">The flow view that contains the link.</param>
    /// <param name="link">The link to remove.</param>
    public DeleteLinkAction(IFlowView view, NodeLink link)
    {
        _view = view;
        _links = [link.Clone()];
    }

    /// <summary>
    /// Initializes a new instance of <see cref="DeleteLinkAction"/> from multiple <see cref="NodeLink"/>s.
    /// </summary>
    /// <param name="view">The flow view that contains the links.</param>
    /// <param name="links">The links to remove.</param>
    public DeleteLinkAction(IFlowView view, IEnumerable<NodeLink> links)
    {
        _view = view;
        _links = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(links, l => l.Clone()));
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
        _links = [new NodeLink(inputNode, inputConnector, outputNode, outputConnector)];
    }

    /// <inheritdoc/>
    public override void Do()
    {
        if (_view.Diagram is not { } diagram)
        {
            return;
        }

        foreach (var link in _links)
        {
            diagram.RemoveLink(link.FromNode, link.FromConnector, link.ToNode, link.ToConnector);
        }
        diagram.RefreshView();
        diagram.QueueComputeData();

        QueuedAction.Do(() => 
        {
            var nodesToRefresh = new HashSet<IFlowViewNode>();
            foreach (var link in _links)
            {
                nodesToRefresh.Add(_view.GetViewNode(link.FromNode));
                nodesToRefresh.Add(_view.GetViewNode(link.ToNode));
            }
            _view.RefreshNodes(nodesToRefresh);

            EditorUtility.Inspector.UpdateInspector();
        });
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        if (_view.Diagram is not { } diagram)
        {
            return;
        }

        foreach (var link in _links)
        {
            diagram.AddLink(link.FromNode, link.FromConnector, link.ToNode, link.ToConnector);
        }
        diagram.RefreshView();
        diagram.QueueComputeData();

        QueuedAction.Do(() => 
        {
            var nodesToRefresh = new HashSet<IFlowViewNode>();
            foreach (var link in _links)
            {
                nodesToRefresh.Add(_view.GetViewNode(link.FromNode));
                nodesToRefresh.Add(_view.GetViewNode(link.ToNode));
            }

            _view.SetLinkSelection(_links);
            _view.RefreshNodes(nodesToRefresh);

            EditorUtility.Inspector.UpdateInspector();
        });
    }
}