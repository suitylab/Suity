using Suity.Collections;
using Suity.Editor.AIGC;
using Suity.Editor.Design;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Flows;
using Suity.Editor.Services;
using Suity.Helpers;
using Suity.Selecting;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.Documents.Canvas;

[DocumentFormat(FormatName = "Canvas", Extension = "scanvas", DisplayText = "Canvas", Icon = "*CoreIcon|Canvas", Order = 990)]
[EditorFeature(EditorFeatures.Canvas)]
/// <summary>
/// Represents a canvas document that manages flow-based visual editing with asset nodes.
/// </summary>
public class CanvasDocument : FlowDocument<CanvasAssetBuilder>,
    ICanvasDocument,
    IDropInCheck, 
    ICanvasCompute
{
    /// <summary>
    /// Gets the icon representing this canvas document.
    /// </summary>
    public override Image Icon => CoreIconCache.Canvas;

    private readonly QueueOnceAction _updateViewAction;
    private FlowComputation _computation;

    /// <summary>
    /// Gets a value indicating whether preview computation is enabled. Always returns false for canvas documents.
    /// </summary>
    public override bool PreviewComputeEnabled => false;

    /// <summary>
    /// Gets the selection list used for creating new factory nodes on the canvas.
    /// </summary>
    public override ISelectionList GetFactoryNodeList() => CanvasSelectionList.Instance;

    /// <summary>
    /// Initializes a new instance of the <see cref="CanvasDocument"/> class.
    /// </summary>
    public CanvasDocument()
    {
        _updateViewAction = new QueueOnceAction(() => View?.RefreshView());
    }

    #region ICanvasDocument

    /// <summary>
    /// Finds all nodes of the specified type in the canvas document.
    /// </summary>
    /// <typeparam name="T">The type of nodes to find.</typeparam>
    /// <returns>An enumerable collection of nodes matching the specified type.</returns>
    public IEnumerable<T> FindNodes<T>() where T : class
    {
        return ItemCollection.AllItems.OfType<FlowDiagramItem>()
            .Select(o => o.Node)
            .OfType<T>();
    }

    /// <summary>
    /// Finds all canvas asset nodes that reference the specified document.
    /// </summary>
    /// <param name="doc">The document to search for.</param>
    /// <returns>An enumerable collection of canvas asset nodes referencing the document.</returns>
    public IEnumerable<CanvasAssetNode> FindDocumentNodes(Document doc)
    {
        if (doc is null)
        {
            throw new ArgumentNullException(nameof(doc));
        }

        return ItemCollection.AllItems.OfType<CanvasAssetDiagramItem>()
            .Select(o => o.Node)
            .SkipNull()
            .Where(o => ReferenceEquals(o.TargetAsset?.GetStorageObject(true), doc));
    }

    /// <summary>
    /// Finds all canvas asset nodes whose target asset is of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the target asset.</typeparam>
    /// <returns>An enumerable collection of canvas asset nodes with matching target asset type.</returns>
    public IEnumerable<CanvasAssetNode> FindDocumentNodes<T>() where T : class
    {
        return ItemCollection.AllItems.OfType<CanvasAssetDiagramItem>()
            .Select(o => o.Node)
            .SkipNull()
            .Where(o => o.TargetAsset?.GetStorageObject(true) is T);
    }

    /// <summary>
    /// Gets all documents of the specified type referenced by canvas asset nodes.
    /// </summary>
    /// <typeparam name="T">The type of documents to retrieve.</typeparam>
    /// <returns>An enumerable collection of documents matching the specified type.</returns>
    public IEnumerable<T> GetDocuments<T>() where T : class
    {
        return ItemCollection.AllItems.OfType<CanvasAssetDiagramItem>()
            .Select(o => o.Node?.TargetAsset?.GetStorageObject(true))
            .OfType<T>();
    }

    /// <summary>
    /// Creates a new asset node on the canvas for the specified asset.
    /// </summary>
    /// <param name="asset">The asset to create a node for.</param>
    /// <param name="size">Optional size for the node. Defaults to 200x200 if not specified.</param>
    /// <returns>The created canvas asset node, or null if creation failed.</returns>
    public CanvasAssetNode CreateAssetNode(Asset asset, Size? size = null)
    {
        if (asset is null)
        {
            throw new ArgumentNullException(nameof(asset));
        }

        Size nodeSize = size ?? new Size(200, 200);

        var pos = GetBlankPosition(this, nodeSize);

        var node = CanvasAssetNodeResolver.Instance.CreateNode(asset);
        if (node is null)
        {
            return null;
        }

        var diagram = this.Diagram;

        diagram.AddNode(node);

        node.DiagramItem.UpdatePreferredSize(nodeSize.Width, nodeSize.Height);
        node.DiagramItem.SetPosition(pos.X, pos.Y);
        node.DiagramItem.SetExpanded(true);

        diagram.RefreshView();
        diagram.QueueComputeData();

        (this.View as IFlowView)?.SetNodeSelection([node]);

        this.MarkDirty(this);

        return node;
    }

    /// <summary>
    /// Creates a new document and adds it as a canvas asset node.
    /// </summary>
    /// <param name="format">The document format to create.</param>
    /// <param name="rFilePath">The file path for the new document.</param>
    /// <param name="size">Optional size for the node. Defaults to 200x200 if not specified.</param>
    /// <returns>The created canvas asset node containing the new document.</returns>
    public CanvasAssetNode CreateDocument(DocumentFormat format, string rFilePath, Size? size = null)
    {
        if (format is null)
        {
            throw new ArgumentNullException(nameof(format));
        }

        string errMsg = L($"Failed to create {format.DisplayText} document.");
        var docEntry = format.AutoNewDocument(rFilePath)
            ?? throw new NullReferenceException(errMsg);

        var assetDoc = docEntry.Content as AssetDocument
            ?? throw new NullReferenceException(errMsg);

        var asset = assetDoc.TargetAsset
            ?? throw new NullReferenceException(errMsg);

        var node = this.CreateAssetNode(asset, size)
            ?? throw new AigcException(L("Failed to create canvas node."));

        this.MarkDirty(this);

        return node;
    }

    /// <summary>
    /// Creates a new tool node of the specified type on the canvas.
    /// </summary>
    /// <typeparam name="T">The type of canvas tool node to create.</typeparam>
    /// <param name="size">Optional size for the node. Defaults to 200x200 if not specified.</param>
    /// <returns>The created canvas tool node.</returns>
    public T CreateToolNode<T>(Size? size) where T : CanvasToolNode, new()
    {
        var node = new T();

        Size nodeSize = size ?? new Size(200, 200);

        var pos = GetBlankPosition(this, nodeSize);

        var diagram = this.Diagram;

        diagram.AddNode(node);

        node.DiagramItem.UpdatePreferredSize(nodeSize.Width, nodeSize.Height);
        node.DiagramItem.SetPosition(pos.X, pos.Y);
        node.DiagramItem.SetExpanded(true);

        diagram.RefreshView();
        diagram.QueueComputeData();

        (this.View as IFlowView)?.SetNodeSelection([node]);

        this.MarkDirty(this);

        return node;
    }

    private static Point GetBlankPosition(ICanvasDocument canvas, Size size)
    {
        var rects = canvas.DiagramItems.Select(o => new Rectangle(o.X, o.Y, o.Width, o.Height)).ToList();
        var pos = RectanglePlacer.PlaceRectangle(size, rects, 2000, 10);

        return pos;
    }

    #endregion

    #region IDropInCheck

    /// <summary>
    /// Checks if the specified value can be dropped into the canvas.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is an asset and can be dropped; otherwise, false.</returns>
    public bool DropInCheck(object value)
    {
        return value is Asset;
    }

    /// <summary>
    /// Converts a dropped value into a canvas asset node.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A canvas asset node if the value is an asset; otherwise, null.</returns>
    public object DropInConvert(object value)
    {
        if (value is Asset s)
        {
            return CanvasAssetNodeResolver.Instance.CreateNode(s);
        }

        return null;
    }

    #endregion

    #region ICanvasCompute

    /// <summary>
    /// Gets the computed value for the specified flow node connector.
    /// </summary>
    /// <param name="connector">The connector to get the value for.</param>
    /// <returns>The computed value, or null if the connector is invalid.</returns>
    public object GetConnectorValue(FlowNodeConnector connector)
    {
        if (connector is null)
        {
            return null;
        }

        var compute = _computation ??= new();

        return compute.GetValue(connector);
    }

    /// <summary>
    /// Invalidates the computation for the specified node and recalculates its outputs.
    /// </summary>
    /// <param name="node">The node to invalidate.</param>
    public void InvalidateNodeComputation(FlowNode node)
    {
        var compute = _computation ??= new();

        List<FlowNode> refreshNodes = null;

        // Invalidate node and all its output nodes
        compute.InvalidateNode(node);
        compute.InvalidateOutputs(node, n => (refreshNodes ??= []).Add(n));

        // Recalculate
        node.Compute(compute);

        if (refreshNodes != null)
        {
            foreach (var refreshNode in refreshNodes)
            {
                if (!compute.GetNodeRunningState(refreshNode).GetIsEnded())
                {
                    refreshNode.Compute(compute);
                }
            }
        }
    }

    #endregion

    internal protected override void OnLoaded(DocumentLoadingIntent intent)
    {
        base.OnLoaded(intent);

// Most canvas nodes reference external resources, and their connection points are from external resources.
/// Connection ports are created with delay, so need to queue once before executing computation
        QueuedAction.Do(() =>
        {
            if (Entry is null)
            {
                return;
            }

            var compute = _computation ??= new();
            foreach (var node in FlowNodes)
            {
                if (!compute.GetNodeRunningState(node).GetIsEnded())
                {
                    node.Compute(compute);
                }
            }
        });
    }

    internal protected override void OnLinkAdded(NodeLink link)
    {
        if (Entry?.State == DocumentState.Loading)
        {
            return;
        }

        // Only need to refresh output nodes
        if (GetFlowNode(link.ToNode) is { } toNode)
        {
            InvalidateNodeComputation(toNode);
        }
    }

    internal protected override void OnLinkRemoved(NodeLink link)
    {
        // No need to detect FlowNodeRemoved since OnLinkRemoved is called first

        // Only need to refresh output nodes
        if (GetFlowNode(link.ToNode) is { } toNode)
        {
            InvalidateNodeComputation(toNode);
        }
    }

    /// <summary>
    /// Handles inspection of objects in the inspector panel, converting canvas nodes to their target objects when appropriate.
    /// </summary>
    /// <param name="objs">The objects to inspect.</param>
    /// <param name="context">The inspector context.</param>
    /// <returns>True if the inspection was handled; otherwise, false.</returns>
    public override bool HandleInspect(IEnumerable<object> objs, IInspectorContext context)
    {
        if (!objs.Any())
        {
            return false;
        }

        // Try to convert CanvasAssetNode to corresponding target node

        if (objs.OneOrDefault() is CanvasAssetNode node && ReferenceEquals(node.ParentDocument, this))
        {
            // Single node
            bool isDoc = node.GetTargetObject() is Document;
            // Handle single
            // var treeMode = isDoc ? InspectorTreeModes.None : InspectorTreeModes.DetailTree;
            // var treeMode = isDoc ? InspectorTreeModes.MainTree : InspectorTreeModes.DetailTree;

            EditorUtility.Inspector.InspectObject(node, node, node.InspectorReadonly);

            return true;
        }
        else if (objs.All(o => o is CanvasAssetNode) &&
            objs.OfType<CanvasAssetNode>().Select(o => o.GetTargetDocument()).AllReferenceEqual())
        {
            // Multiple nodes from same document
            var first = objs.OfType<CanvasAssetNode>().FirstOrDefault();
            if (first is null)
            {
                return false;
            }

            bool isDoc = objs.OfType<CanvasAssetNode>().Any(o => o.GetTargetObject() is Document);
            //var treeMode = isDoc ? InspectorTreeModes.None : InspectorTreeModes.DetailTree;
            //var treeMode = isDoc ? InspectorTreeModes.MainTree : InspectorTreeModes.DetailTree;

            bool isReadonly = objs.OfType<CanvasAssetNode>().Any(o => o.InspectorReadonly);

            EditorUtility.Inspector.InspectObjects(objs, first, isReadonly);

            return true;
        }
        else if (objs.OfType<CanvasAssetNode>().Any())
        {
            // Contains canvas nodes, must be read-only
            EditorUtility.Inspector.InspectObjects(objs, readOnly: true);

            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the data style for the specified data type.
    /// </summary>
    /// <param name="dataType">The data type to get the style for.</param>
    /// <returns>The flow data style for the specified type.</returns>
    public override IFlowDataStyle GetDataStyle(string dataType)
    {
        return TypeFlowDataStyle.GetDataStyle(dataType);
    }
}

/// <summary>
/// Represents a canvas asset that groups other assets.
/// </summary>
public class CanvasAsset : GroupAsset
{
    /// <summary>
    /// Gets the default icon for canvas assets.
    /// </summary>
    public override Image DefaultIcon => CoreIconCache.Canvas;
}

/// <summary>
/// Builder class for creating canvas assets.
/// </summary>
public class CanvasAssetBuilder : GroupAssetBuilder<CanvasAsset>
{
}