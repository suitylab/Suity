using Suity.Collections;
using Suity.Editor.Design;
using Suity.Editor.Documents;
using Suity.Editor.Flows;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.AIGC.Assistants;

/// <summary>
/// Canvas context
/// </summary>
public sealed class CanvasContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CanvasContext"/> class.
    /// </summary>
    public CanvasContext()
    {
    }

    /// <summary>
    /// Canvas
    /// </summary>
    public ICanvasDocument Canvas { get; init; }

    /// <summary>
    /// Gets or sets the canvas flow nodes associated with this context.
    /// </summary>
    public CanvasFlowNode[] CanvasNodes { get; init; }

    /// <summary>
    /// Provides sub-selection object of the document when only one document is selected
    /// </summary>
    public object[] Selection { get; set; }

    /// <summary>
    /// Custom data handler
    /// </summary>
    public ILinkedDataHandler LinkedDataHandler { get; set; }

    #region Getter

    /// <summary>
    /// Document view. Note that this view is not necessarily the view of <see cref="TargetDocument"/>, possibly the view of <see cref="ICanvasDocument"/>.)
    /// </summary>
    public IDocumentView GetCanvasView() => (Canvas as Document)?.ShowView();

    /// <summary>
    /// Gets the first canvas flow node in the <see cref="CanvasNodes"/> collection, or null if the collection is empty.
    /// </summary>
    public CanvasFlowNode CanvasNode => CanvasNodes.FirstOrDefault();

    /// <summary>
    /// Gets all canvas asset nodes from the <see cref="CanvasNodes"/> collection.
    /// </summary>
    public IEnumerable<CanvasAssetNode> TargetAssetNodes => CanvasNodes?.OfType<CanvasAssetNode>();

    /// <summary>
    /// Gets the first canvas asset node from the <see cref="CanvasNodes"/> collection, or null if none exist.
    /// </summary>
    public CanvasAssetNode TargetAssetNode => CanvasNodes?.OfType<CanvasAssetNode>().FirstOrDefault();

    /// <summary>
    /// Gets all target documents associated with the canvas asset nodes.
    /// </summary>
    public IEnumerable<Document> TargetDocuments
        => CanvasNodes?.OfType<CanvasAssetNode>().Select(x => x.GetTargetDocument()).SkipNull() ?? [];

    /// <summary>
    /// Target document
    /// </summary>
    public Document TargetDocument => TargetAssetNode?.GetTargetDocument();

    /// <summary>
    /// Gets all canvas nodes of the specified type from the canvas diagram.
    /// </summary>
    /// <typeparam name="T">The type of canvas nodes to retrieve.</typeparam>
    /// <returns>An enumerable collection of canvas nodes of type <typeparamref name="T"/>.</returns>
    public IEnumerable<T> GetCanvasNodes<T>() => Canvas.Diagram.Nodes.OfType<T>();

    #endregion

    /// <summary>
    /// Creates a shallow clone of this canvas context.
    /// </summary>
    /// <param name="withSelection">If true, includes the selection in the cloned context; otherwise, selection is set to null.</param>
    /// <returns>A new <see cref="CanvasContext"/> instance with copied property values.</returns>
    public CanvasContext Clone(bool withSelection = false)
    {
        var sel = new CanvasContext
        {
            Canvas = this.Canvas,
            CanvasNodes = [.. this.CanvasNodes],
            Selection = withSelection ? this.Selection?.ToArray() : null,
            LinkedDataHandler = this.LinkedDataHandler,
        };

        return sel;
    }

    #region Static Create

    /// <summary>
    /// Creates a new <see cref="CanvasContext"/> with the specified canvas document and empty nodes and selection.
    /// </summary>
    /// <param name="canvas">The canvas document to associate with the context.</param>
    /// <returns>A new <see cref="CanvasContext"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="canvas"/> is null.</exception>
    public static CanvasContext Create(ICanvasDocument canvas)
    {
        if (canvas is null)
        {
            throw new ArgumentNullException(nameof(canvas));
        }

        var sel = new CanvasContext
        {
            Canvas = canvas,
            CanvasNodes = [],
            Selection = [],
        };

        return sel;
    }

    /// <summary>
    /// Creates a new <see cref="CanvasContext"/> with the specified canvas document and canvas flow nodes.
    /// </summary>
    /// <param name="canvas">The canvas document to associate with the context.</param>
    /// <param name="nodes">The canvas flow nodes to include in the context.</param>
    /// <returns>A new <see cref="CanvasContext"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="canvas"/> is null.</exception>
    public static CanvasContext Create(ICanvasDocument canvas, params CanvasFlowNode[] nodes)
    {
        if (canvas is null)
        {
            throw new ArgumentNullException(nameof(canvas));
        }

        var sel = new CanvasContext
        {
            Canvas = canvas,
            CanvasNodes = [.. nodes],
            Selection = [],
        };

        return sel;
    }

    /// <summary>
    /// Creates a new <see cref="CanvasContext"/> with the specified canvas document and documents.
    /// </summary>
    /// <param name="canvas">The canvas document to associate with the context.</param>
    /// <param name="documents">The documents to find nodes for and include in the context.</param>
    /// <returns>A new <see cref="CanvasContext"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="canvas"/> is null.</exception>
    public static CanvasContext Create(ICanvasDocument canvas, params Document[] documents)
    {
        if (canvas is null)
        {
            throw new ArgumentNullException(nameof(canvas));
        }

        var nodes = documents.Select(canvas.FindDocumentNodes).OfType<CanvasFlowNode>().Distinct();

        var sel = new CanvasContext
        {
            Canvas = canvas,
            CanvasNodes = [.. nodes],
            Selection = [],
        };

        return sel;
    }

    /// <summary>
    /// Creates a new <see cref="CanvasContext"/> with the specified canvas document, a single node, and selection objects.
    /// </summary>
    /// <param name="canvas">The canvas document to associate with the context.</param>
    /// <param name="node">The canvas flow node to include in the context.</param>
    /// <param name="selection">The selection objects to include in the context.</param>
    /// <returns>A new <see cref="CanvasContext"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="canvas"/> is null.</exception>
    public static CanvasContext Create(ICanvasDocument canvas, CanvasFlowNode node, params object[] selection)
    {
        if (canvas is null)
        {
            throw new ArgumentNullException(nameof(canvas));
        }

        var sel = new CanvasContext
        {
            Canvas = canvas,
            CanvasNodes = [node],
            Selection = selection ?? [],
        };

        return sel;
    }

    /// <summary>
    /// Creates a new <see cref="CanvasContext"/> with the specified canvas document, a single document, and selection objects.
    /// </summary>
    /// <param name="canvas">The canvas document to associate with the context.</param>
    /// <param name="document">The document to find nodes for and include in the context.</param>
    /// <param name="selection">The selection objects to include in the context.</param>
    /// <returns>A new <see cref="CanvasContext"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="canvas"/> is null.</exception>
    public static CanvasContext Create(ICanvasDocument canvas, Document document, params object[] selection)
    {
        if (canvas is null)
        {
            throw new ArgumentNullException(nameof(canvas));
        }

        var nodes = canvas.FindDocumentNodes(document);

        var sel = new CanvasContext
        {
            Canvas = canvas,
            CanvasNodes = [.. nodes],
            Selection = selection ?? [],
        };

        return sel;
    }

    #endregion
}
