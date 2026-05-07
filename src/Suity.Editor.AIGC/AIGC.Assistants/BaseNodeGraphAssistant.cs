using Suity.Collections;
using Suity.Editor.Flows;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.AIGC.Assistants;


#region AINodeGraphAssistant

/// <summary>
/// Base class for AI assistants that operate on node graph documents.
/// Provides functionality for building graph rules, type contexts, and node link information.
/// </summary>
public abstract class BaseNodeGraphAssistant : BaseGenerativeAssistant
{
    /// <summary>
    /// Gets the connection rules that define how nodes can be linked in the graph.
    /// </summary>
    public abstract string ConnectingRule { get; }

    /// <summary>
    /// Gets the naming convention guidelines for nodes in the graph.
    /// </summary>
    public abstract string NamingConvention { get; }

    /// <summary>
    /// Gets a value indicating whether strict naming validation should be enforced.
    /// </summary>
    public abstract bool StrictNaming { get; }


    /// <summary>
    /// Builds a list of compound function call types available for the AI request context.
    /// </summary>
    /// <param name="request">The AI request containing context information.</param>
    /// <returns>A list of compound function call types.</returns>
    public abstract List<DCompondFunctionCallType> BuildTypeList(AIRequest request);

    /// <summary>
    /// Builds a collection of struct fields that should be excluded from processing.
    /// </summary>
    /// <param name="dCompond">The compound type to analyze for excluded fields.</param>
    /// <returns>A collection of struct fields to exclude.</returns>
    public abstract ICollection<DStructField> BuildExcludedFields(DCompond dCompond);

    /// <summary>
    /// Builds the guiding context for graph generation based on available types.
    /// </summary>
    /// <param name="request">The AI request containing context information.</param>
    /// <returns>A string containing the graph guiding context.</returns>
    public virtual string BuildGraphGuiding(AIRequest request)
    {
        var list = BuildTypeList(request);

        return AIGenerativeService.Instance.BuildTypeContext(list, false);
    }

    /// <summary>
    /// Builds the rule prompt for graph editing operations.
    /// </summary>
    /// <param name="request">The AI request containing context information.</param>
    /// <param name="guiding">Optional guiding context. If null, it will be built automatically.</param>
    /// <returns>A string containing the graph editing rules.</returns>
    public virtual string BuildGraphRule(AIRequest request, string guiding = null)
    {
        string editListPromptFormat = @"
This is an item graph document with multiple nodes and connections.
If you want to add a new item, please select the item type from the following nodes:
{0}
Please provide connection relationship in additional information.
Notice:
 - It must start from a root node.
 - Design multiple nodes and output to the Items array, each item corresponds to one node.
 - The node graph must be a directed acyclic graph (DAG).
 - Fields have two types: Single connectable field and Multiple connectable field. A single connectable field can only connect to one item, while a multiple connectable field can connect to multiple types.
 - If the design document is json/XML-like format, divide the document into multiple items and connect it to a tree graph, then create an independent name for each tree node.
 - Output node graph with mermaid format, in 'AdditionalInformation' field.
 - {1}
";

        guiding ??= BuildGraphGuiding(request);

        string rule = string.Format(editListPromptFormat, guiding, ConnectingRule);

        return rule;
    }

    /// <summary>
    /// Builds a text description of the link information for a specific node in the graph.
    /// </summary>
    /// <param name="context">The flow graph design result containing nodes and links.</param>
    /// <param name="nodeName">The name of the node to build link information for.</param>
    /// <returns>A string containing the node's connection information, or null if the node is not found or has no connections.</returns>
    public static string BuildNodeLinkInfo(FlowGraphDesignResult context, string nodeName)
    {
        var design = context.Nodes.GetValueSafe(nodeName);
        if (design is null)
        {
            return null;
        }

        var nodesById = context.Nodes.Values.ToDictionarySafe(o => o.NodeId, o => o);

        var fromConns = context.CreatedLinks.Where(o => o.ToNodeId == design.NodeId).ToArray();
        var toConns = context.CreatedLinks.Where(o => o.FromNodeId == design.NodeId).ToArray();

        var fromNodes = fromConns.Select(o => nodesById.GetValueSafe(o.FromNodeId)).SkipNull();
        var toNodes = toConns.Select(o => nodesById.GetValueSafe(o.ToNodeId)).SkipNull();
        var nodes = fromNodes.Concat(toNodes).Distinct().ToArray();

        if (nodes.Length == 0)
        {
            return null;
        }

        string template = @"
# You are designing a node in a node graph.

# Connection information about this node: '{{NODE_ID}}':
## From connection:
{{FROM}}

## To connection:
{{TO}}

# Information about sibling nodes:
{{SIBLING}}
";

        var builder = new PromptBuilder(template);
        builder.Replace("{{NODE_ID}}", design.NodeId);
        builder.Replace("{{FROM}}", string.Join("\n", fromConns.Select(o => "- " + o.ToString())));
        builder.Replace("{{TO}}", string.Join("\n", toConns.Select(o => "- " + o.ToString())));
        builder.Replace("{{SIBLING}}", string.Join("\n", nodes.Select(o => o.ToString())));

        string text = builder.ToString();

        return text;
    }
}

/// <summary>
/// Generic base class for AI assistants that operate on typed node graph documents.
/// Handles pipeline execution, node creation, modification, and editable list building.
/// </summary>
/// <typeparam name="TDocument">The type of flow document this assistant operates on.</typeparam>
/// <typeparam name="TNode">The type of flow node, must implement ISObjectFlowNode and have a parameterless constructor.</typeparam>
public abstract class BaseNodeGraphAssistant<TDocument, TNode> : BaseNodeGraphAssistant
    where TDocument : FlowDocument where TNode : FlowNode, ISObjectFlowNode, new()
{
    #region Pipeline

    /// <summary>
    /// Handles different stages of the generative pipeline.
    /// </summary>
    /// <param name="request">The AI generative request containing context and state.</param>
    /// <param name="pipeline">The current pipeline stage to handle.</param>
    /// <param name="intent">The generative intent describing the user's goal.</param>
    /// <returns>A task representing the asynchronous pipeline handling operation.</returns>
    public override Task HandlePipeline(AIGenerativeRequest request, GenerativePipeline pipeline, GenerativeIntent intent)
    {
        switch (pipeline)
        {
            case GenerativePipeline.BuildEditableList:
                request.EditableList ??= BuildEditableList(request);
                break;

            case GenerativePipeline.BuildEditListKnowledge:
                request.GuidingKnowledge ??= BuildGraphGuiding(request);
                request.EditListKnowledge ??= BuildGraphRule(request, request.GuidingKnowledge);
                break;

            case GenerativePipeline.BuildGuidingKnowledge:
                request.GuidingKnowledge ??= BuildGraphGuiding(request);
                break;

            case GenerativePipeline.PrepareGenerative:
                return PrepareGenerative(request, intent);

            case GenerativePipeline.FinishGenerative:
                return FinishGenerative(request, intent);

            default:
                return base.HandlePipeline(request, pipeline, intent);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Prepares the generative context by creating or loading the node graph design.
    /// </summary>
    /// <param name="request">The AI generative request containing context and state.</param>
    /// <param name="intent">The generative intent describing the user's goal.</param>
    /// <returns>A task representing the asynchronous preparation operation.</returns>
    public virtual async Task PrepareGenerative(AIGenerativeRequest request, GenerativeIntent intent)
    {
        if (request.GenerativeContext is FlowGraphDesignResult)
        {
            return;
        }

        if (TargetDocument is not TDocument doc)
        {
            throw new AigcException(L("Please select a node graph document."));
        }

        var design = await AIGenerativeService.Instance.HandleCreateNodeGraph(request, intent);
        if (design is null)
        {
            throw new AigcException(L("Failed to create node graph."));
        }

        // Try to remove excess connections before executing subsequent operations.
        NodeGraphApplyAction<TNode> action = null;
        if (design.RemovedLinks.Count > 0)
        {
            action = new NodeGraphApplyAction<TNode>(doc, this.RootView, design, request.RecordTooltips);

            foreach (var link in design.RemovedLinks)
            {
                action.AddRemovedLink(link.FromNodeId, link.Field, link.ToNodeId);
            }
        }

        request.GenerativeContext = design;
        request.Preparation = new AIGenerativeCallResult(action);
    }

    /// <summary>
    /// Finalizes the generative operation by applying new connections to the graph.
    /// </summary>
    /// <param name="request">The AI generative request containing context and state.</param>
    /// <param name="intent">The generative intent describing the user's goal.</param>
    /// <returns>A task representing the asynchronous finalization operation.</returns>
    public virtual Task FinishGenerative(AIGenerativeRequest request, GenerativeIntent intent)
    {
        if (TargetDocument is not TDocument doc)
        {
            throw new AigcException(L("Please select a node graph document."));
        }

        var context = request.GenerativeContext as FlowGraphDesignResult
            ?? throw new AigcException(L("Please generate node graph first."));

        // At the final stage, try to add new connections.
        NodeGraphApplyAction<TNode> action = null;
        if (context.CreatedLinks.Count > 0)
        {
            action = new NodeGraphApplyAction<TNode>(doc, this.RootView, context, request.RecordTooltips);

            foreach (var link in context.CreatedLinks)
            {
                action.AddLink(link.FromNodeId, link.Field, link.ToNodeId);
            }
        }

        request.Conclusion = new AIGenerativeCallResult(action, request.Results);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets editable item information for a diagram item in the node graph.
    /// </summary>
    /// <param name="request">The AI generative request containing context.</param>
    /// <param name="item">The item to retrieve information for.</param>
    /// <returns>A GenerativeEditableItem containing the item's metadata.</returns>
    public override GenerativeEditableItem GetEditableItemInfo(AIGenerativeRequest request, object item)
    {
        if (item is FlowDiagramItem diagramItem)
        {
            string brief = diagramItem?.ToToolTipsText() ?? item?.ToDisplayText() ?? string.Empty;
            string typeName = (diagramItem.Node as TNode)?.Data?.ObjectType?.GetFullTypeName() ?? string.Empty;

            return new GenerativeEditableItem
            {
                Name = diagramItem.Name,
                Type = typeName,
                Position = new Point(diagramItem.X, diagramItem.Y),
                Brief = brief,
            };
        }

        return base.GetEditableItemInfo(request, item);
    }

    /// <summary>
    /// Builds an editable list representation of the current document's nodes and links.
    /// </summary>
    /// <param name="request">The AI generative request containing context.</param>
    /// <returns>A GenerativeEditableList containing all nodes and link information.</returns>
    public virtual GenerativeEditableList BuildEditableList(AIGenerativeRequest request)
    {
        if (TargetDocument is not TDocument doc)
        {
            throw new AigcException(L("Please select a node graph document."));
        }

        var nodes = doc.DiagramItems.Select(o => GetEditableItemInfo(request, o)).SkipNull().ToList();
        var links = doc.Diagram.Links.Select(o => o.ToString(true));

        return new GenerativeEditableList
        {
            Items = nodes,
            AdditionalInformation = $"Links:\n{string.Join("\n", links)}",
        };
    }




    #endregion

    #region Generate

    /// <summary>
    /// Handles the generation of nodes based on the guiding item's edit type.
    /// </summary>
    /// <param name="request">The AI generative request containing context and state.</param>
    /// <param name="guiding">The guiding item describing what edit operation to perform.</param>
    /// <returns>A task representing the asynchronous generation operation with the result.</returns>
    public override Task<AIGenerativeCallResult> HandleGenerate(AIGenerativeRequest request, GenerativeGuidingItem guiding)
    {
        switch (guiding.EditType)
        {
            case GenerativeEditType.Create:
                return GenerativeCreate(request, guiding);

            case GenerativeEditType.Modify:
                return GenerativeModify(request, guiding);

            default:
                return base.HandleGenerate(request, guiding);
        }
    }

    /// <summary>
    /// Generates a new node in the graph based on the guiding item.
    /// </summary>
    /// <param name="request">The AI generative request containing context and state.</param>
    /// <param name="guiding">The guiding item describing the node to create.</param>
    /// <returns>A task representing the asynchronous creation operation with the result.</returns>
    public virtual async Task<AIGenerativeCallResult> GenerativeCreate(AIGenerativeRequest request, GenerativeGuidingItem guiding)
    {
        if (TargetDocument is not TDocument doc)
        {
            throw new AigcException(L("Please select a node graph document."));
        }

        var context = request.GenerativeContext as FlowGraphDesignResult
            ?? throw new AigcException(L("Please generate node graph first."));

        var design = context.Nodes.GetValueSafe(guiding.Name)
            ?? throw new AigcException(L("Failed to get node: ") + guiding.Name);

        string msg = guiding.ToFullText();
        string linkInfoText = BuildNodeLinkInfo(context, guiding.Name);
        if (!string.IsNullOrWhiteSpace(linkInfoText))
        {
            msg += linkInfoText;
        }

        var guidingReq = request.CreateWithMessage(msg);

        // Here we can not use design.Description because guiding already contains more detailed information.
        var comp = design.NodeData = await guidingReq.GenerateSObject(this.Context, design.NodeType);
        if (comp is null)
        {
            throw new AigcException(L("Failed to generate node: ") + guiding.Name);
        }

        //TODO: Add front and back node connection information

        var action = new NodeGraphApplyAction<TNode>(doc, this.RootView, context, request.RecordTooltips);
        action.AddData(design.NodeId, design.Description, design.NodeData, design.Position, guiding);

        return new AIGenerativeCallResult(action);
    }

    /// <summary>
    /// Modifies an existing node in the graph based on the guiding item.
    /// </summary>
    /// <param name="request">The AI generative request containing context and state.</param>
    /// <param name="guiding">The guiding item describing the modifications to apply.</param>
    /// <returns>A task representing the asynchronous modification operation with the result.</returns>
    public virtual async Task<AIGenerativeCallResult> GenerativeModify(AIGenerativeRequest request, GenerativeGuidingItem guiding)
    {
        if (TargetDocument is not TDocument doc)
        {
            throw new AigcException(L("Please select a node graph document."));
        }

        if (doc.GetElement(guiding.Name) is not FlowDiagramItem item)
        {
            throw new AigcException(L("The object to update is not valid data."));
        }
        if (item.Node is not TNode node)
        {
            throw new AigcException(L("The object to update is not a valid node."));
        }

        var origin = node.Data;
        SObject compNew;

        if (origin is null)
        {
            request.Conversation.AddErrorMessage(L("Current node is missing data."));
            return null;
        }

        if (origin.ObjectType.Target is not DCompond dcompond)
        {
            request.Conversation.AddErrorMessage(L("Current node is missing data type."));
            return null;
        }

        string msg = guiding.ToFullText();
        var guidingReq = request.CreateWithMessage(msg);
        var exFields = BuildExcludedFields(dcompond);
        compNew = await guidingReq.EditSObject(this.Context, origin, exFields);
        if (compNew is null)
        {
            request.Conversation.AddErrorMessage(L("Failed to generate component: ") + dcompond.DisplayText);
            return null;
        }

        var action = new NodeGraphApplyAction<TNode>(doc, this.RootView, recordTooltips: request.RecordTooltips);
        action.AddData(node.Name, null, compNew);

        return new AIGenerativeCallResult(action);
    }

    #endregion

}
#endregion

#region AINodeGraphDesignRequest

/// <summary>
/// Request object for AI-driven node graph design operations.
/// Contains all necessary context for generating or modifying node graphs.
/// </summary>
public class AINodeGraphDesignRequest : AIRequest
{
    /// <summary>
    /// Gets the canvas context associated with the node graph document.
    /// </summary>
    public CanvasContext Canvas { get; init; }

    /// <summary>
    /// Gets the flow document being operated on.
    /// </summary>
    public FlowDocument Document { get; init; }

    /// <summary>
    /// Current document content, should contain nodes and connections
    /// </summary>
    public string DocumentContent { get; init; }

    /// <summary>
    /// Object types supported by the document
    /// </summary>
    public string TypeKnowledge { get; init; }

    /// <summary>
    /// Connection rules of the document
    /// </summary>
    public string ConnectingRule { get; init; }

    /// <summary>
    /// Detailed design guidance
    /// </summary>
    public string DetailedDesignGuiding { get; init; }


    /// <summary>
    /// Gets the naming convention rules for nodes in the graph.
    /// </summary>
    public string NamingConvention { get; init; }

    /// <summary>
    /// Gets a value indicating whether strict naming validation should be enforced.
    /// </summary>
    public bool StrictNaming { get; init; }


    /// <summary>
    /// Gets the list of available node types that can be used in the graph.
    /// </summary>
    public List<DCompondFunctionCallType> NodeTypes { get; init; }

    /// <summary>
    /// Gets a value indicating whether new nodes should be generated.
    /// </summary>
    public bool GenerateNodes { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AINodeGraphDesignRequest"/> class.
    /// </summary>
    public AINodeGraphDesignRequest()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AINodeGraphDesignRequest"/> class by copying from an existing request.
    /// </summary>
    /// <param name="origin">The original AI request to copy base properties from.</param>
    /// <param name="canvas">The canvas context associated with the node graph document.</param>
    /// <param name="increaseDepth">Whether to increment the request depth counter.</param>
    public AINodeGraphDesignRequest(AIRequest origin, CanvasContext canvas, bool increaseDepth = false)
        : base(origin, increaseDepth)
    {
        Canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
    }
}

#endregion
