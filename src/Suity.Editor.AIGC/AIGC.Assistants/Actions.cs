using static Suity.Helpers.GlobalLocalizer;
using Suity.Editor.Design;
using Suity.Editor.Documents;
using Suity.Editor.Flows;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Helpers;
using Suity.Synchonizing.Core;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Editor.AIGC.Assistants;

#region NodeGraphApplyAction
/// <summary>
/// An undoable action that applies AI-generated node graph changes to a flow document.
/// </summary>
/// <typeparam name="TNode">The type of flow node to create or modify.</typeparam>
public class NodeGraphApplyAction<TNode> : AIGenerativeApplyAction
where TNode : FlowNode, ISObjectFlowNode, new()
{
    private class GenItem
    {
        public string Name;
        public string Description;
        public SObject Component;
        public Point? Position;
        public Color? Color;
        public GenerativeGuidingItem Guiding;
    }

    private readonly FlowDocument _doc;
    private readonly IDocumentView _docView;
    private readonly FlowGraphDesignResult _context;
    private readonly bool _recordTooltips;

    private readonly List<GenItem> _newItems = [];
    private readonly List<GenItem> _oldItems = [];

    private readonly List<NodeLink> _newLinks = [];
    private readonly List<NodeLink> _removedLinks = [];

    //TODO: Not all TNode types are externally visible, need to filter by type.
    private readonly List<IFlowDiagramItem> _appliedItems = [];

    /// <summary>
    /// Initializes a new instance of the node graph apply action.
    /// </summary>
    /// <param name="doc">The target flow document.</param>
    /// <param name="docView">The document view for refreshing after changes.</param>
    /// <param name="context">Optional graph design result containing node and link information.</param>
    /// <param name="recordTooltips">Whether to record tooltip and knowledge data on nodes.</param>
    public NodeGraphApplyAction(FlowDocument doc, IDocumentView docView, FlowGraphDesignResult context = null, bool recordTooltips = false)
    {
        _doc = doc ?? throw new ArgumentNullException(nameof(doc));
        _docView = docView ?? throw new ArgumentNullException(nameof(docView));
        _context = context;
        _recordTooltips = recordTooltips;
    }

    /// <summary>
    /// Gets the display name of this action based on the changes being applied.
    /// </summary>
    public override string Name
    {
        get
        {
            if (_newItems.Count > 0 && _newLinks.Count > 0)
            {
                return L("Generate node graph");
            }

            if (_newItems.Count > 0)
            {
                return L("Generate nodes");
            }

            if (_newLinks.Count > 0)
            {
                return L("Generate connections");
            }

            if (_removedLinks.Count > 0)
            {
                return L("Remove connections");
            }

            return L("Generate node graph (no data)");
        }
    }

    /// <summary>
    /// Adds a new node data entry to be applied, storing the old value for undo support.
    /// </summary>
    /// <param name="name">The name of the node.</param>
    /// <param name="description">The description of the node.</param>
    /// <param name="component">The SObject data for the node.</param>
    /// <param name="position">Optional position in the graph editor.</param>
    /// <param name="guiding">Optional generative guiding item with additional context.</param>
    public void AddData(string name, string description, SObject component, Point? position = null, GenerativeGuidingItem guiding = null)
    {
        var item = new GenItem
        {
            Name = name,
            Description = description,
            Position = position,
            Color = ColorHelper.ParseHtmlColor(guiding?.HtmlColor),
            Component = component,
            Guiding = guiding,
        };

        _newItems.Add(item);

        var row = _doc.GetDiagramItem(name)?.Node as TNode;
        if (row != null)
        {
            var oldItem = new GenItem
            {
                Name = name,
                //Description = row.Description,
                //Color = (row as DataRow)?.Color,
                Component = Cloner.Clone(row.Data),
            };

            _oldItems.Add(oldItem);
        }
        else
        {
            _oldItems.Add(null);
        }
    }

    /// <summary>
    /// Adds a new link (connection) to be created during the Do operation.
    /// </summary>
    /// <param name="nameFrom">The source node name.</param>
    /// <param name="field">The field on the target node to connect to.</param>
    /// <param name="nameTo">The target node name.</param>
    public void AddLink(string nameFrom, DStructField field, string nameTo)
    {
        _newLinks.Add(new NodeLink(nameFrom, field.Id.ToString(), nameTo, "In"));
    }

    /// <summary>
    /// Adds a link to be removed during the Do operation (will be restored on Undo).
    /// </summary>
    /// <param name="nameFrom">The source node name.</param>
    /// <param name="field">The field on the target node connected.</param>
    /// <param name="nameTo">The target node name.</param>
    public void AddRemovedLink(string nameFrom, DStructField field, string nameTo)
    {
        _removedLinks.Add(new NodeLink(nameFrom, field.Id.ToString(), nameTo, "In"));
    }

    /// <summary>
    /// Gets the array of diagram items that were modified by this action.
    /// </summary>
    /// <returns>Array of applied flow diagram items.</returns>
    public override object[] GetAppliedObjects() => _appliedItems.ToArray();

    /// <summary>
    /// Applies the pending node graph changes, creating or updating nodes and managing links.
    /// </summary>
    public override void Do()
    {
        _appliedItems.Clear();

        for (int i = 0; i < _newItems.Count; i++)
        {
            var newItem = _newItems[i];
            if (newItem is null)
            {
                continue;
            }

            if (_doc.GetFlowNode(newItem.Name) is not TNode node)
            {
                node = new TNode { Name = newItem.Name };
                var diagramItem = _doc.AddFlowNode(node);
                if (newItem.Position is { } pos)
                {
                    diagramItem.SetPosition(pos.X, pos.Y);
                }
                _appliedItems.Add(diagramItem);
            }

            var comp = newItem.Component;
            if (node.Data is { } data)
            {
                comp.MergeTo(data, true);
            }
            else
            {
                node.Data = Cloner.Clone(comp);
            }

            // Need to update once to ensure connection ports are created
            node.UpdateConnector();
            // Need to update once to ensure ports can be found when creating connection lines
            node.DiagramItem.NotifyNodeUpdated();

            if (node is DesignFlowNode current)
            {
                if (newItem.Guiding is { } guiding)
                {
                    string brief = guiding.Brief;
                    string fullText = guiding.Prompt;

                    if (_recordTooltips)
                    {
                        if (!string.IsNullOrWhiteSpace(brief))
                        {
                            current.SetAttribute<ToolTipsAttribute>(o => o.ToolTips = brief);
                        }

                        if (!string.IsNullOrWhiteSpace(fullText))
                        {
                            current.SetAttribute<KnowledgeAttribute>(o => o.Knowledge = fullText);
                        }
                    }
                }

                string desc = newItem.Guiding?.LocalizedName;
                if (string.IsNullOrWhiteSpace(desc))
                {
                    desc = newItem.Description;
                }

                if (!string.IsNullOrWhiteSpace(desc))
                {
                    current.Description = desc;
                }

                if (newItem.Color is { } color)
                {
                    current.DesignColor = color;
                }
            }
        }

        foreach (var link in _removedLinks)
        {
            _doc.Diagram.RemoveLink(link.FromNode, link.FromConnector, link.ToNode, link.ToConnector);
        }

        foreach (var link in _newLinks)
        {
            _doc.Diagram.AddLink(link.FromNode, link.FromConnector, link.ToNode, link.ToConnector);
        }

        _docView.RefreshView();
        _doc.Diagram.QueueComputeData();

        EditorUtility.Inspector.UpdateInspector();
    }

    /// <summary>
    /// Reverts the applied node graph changes, restoring previous node states and link configurations.
    /// </summary>
    public override void Undo()
    {
        _appliedItems.Clear();

        for (int i = 0; i < _newItems.Count; i++)
        {
            var newItem = _newItems[i];
            if (newItem is null)
            {
                continue;
            }

            var row = _doc.GetDiagramItem(newItem.Name)?.Node as TNode;
            if (row is null)
            {
                continue;
            }

            var oldItem = _oldItems[i];
            if (oldItem is null)
            {
                _doc.RemoveFlowNode(row);
            }
            else
            {
                var comp = oldItem.Component;
                if (row.Data is { } data)
                {
                    comp.MergeTo(data, true);
                }
                else
                {
                    row.Data = Cloner.Clone(comp);
                }

                //row.Description = oldItem.Description ?? string.Empty;

                //if (oldItem.Color is { } color && row is DataRow dataRow)
                //{
                //    dataRow.Color = color;
                //}
            }
        }

        foreach (var link in _newLinks)
        {
            _doc.Diagram.RemoveLink(link.FromNode, link.FromConnector, link.ToNode, link.ToConnector);
        }

        foreach (var link in _removedLinks)
        {
            _doc.Diagram.AddLink(link.FromNode, link.FromConnector, link.ToNode, link.ToConnector);
        }

        _docView.RefreshView();
        _doc.Diagram.QueueComputeData();

        EditorUtility.Inspector.UpdateInspector();
    }


}
#endregion

#region NodeGraphDataEditAction

/// <summary>
/// An undoable action that edits the data of a single flow node.
/// </summary>
public class NodeGraphDataEditAction : AIGenerativeApplyAction
{
    readonly ISObjectFlowNode _node;
    readonly SObject _newValue;
    readonly SObject _oldValue;

    /// <summary>
    /// Initializes a new instance with the node and its new data value.
    /// </summary>
    /// <param name="node">The flow node to edit.</param>
    /// <param name="value">The new data value for the node.</param>
    public NodeGraphDataEditAction(ISObjectFlowNode node, SObject value)
    {
        _node = node ?? throw new ArgumentNullException(nameof(node));
        _newValue = value ?? throw new ArgumentNullException(nameof(value));
        _oldValue = _node.Data;
    }

    /// <summary>
    /// Gets the display name of this action.
    /// </summary>
    public override string Name => L("Edit node data");

    /// <summary>
    /// Gets the node that was modified by this action.
    /// </summary>
    /// <returns>Array containing the modified flow node.</returns>
    public override object[] GetAppliedObjects() => [_node];

    /// <summary>
    /// Applies the new data value to the node.
    /// </summary>
    public override void Do()
    {
        _node.Data = _newValue;
    }

    /// <summary>
    /// Restores the original data value to the node.
    /// </summary>
    public override void Undo()
    {
        _node.Data = _oldValue;
    }
}


#endregion

