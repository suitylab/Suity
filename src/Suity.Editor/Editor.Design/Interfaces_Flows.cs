using Suity.Editor.Documents;
using Suity.Editor.Flows;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Editor.Design;

/// <summary>
/// Provides context for a flow diagram.
/// </summary>
public interface IFlowDiagramContext
{
    /// <summary>
    /// Gets the flow diagram.
    /// </summary>
    IFlowDiagram Diagram { get; }

    /// <summary>
    /// Gets a diagram item by name.
    /// </summary>
    IFlowDiagramItem GetDiagramItem(string name);

    /// <summary>
    /// Gets all diagram items.
    /// </summary>
    IEnumerable<IFlowDiagramItem> DiagramItems { get; }
}

/// <summary>
/// Represents a canvas document that contains flow diagrams and assets.
/// </summary>
public interface ICanvasDocument : IFlowDiagramContext, IMemberContainer
{
    /// <summary>
    /// Finds all nodes of the specified type.
    /// </summary>
    IEnumerable<T> FindNodes<T>() where T : class;

    /// <summary>
    /// Finds document nodes for the specified document.
    /// </summary>
    IEnumerable<CanvasAssetNode> FindDocumentNodes(Document doc);

    /// <summary>
    /// Finds document nodes of the specified type.
    /// </summary>
    IEnumerable<CanvasAssetNode> FindDocumentNodes<T>() where T : class;

    /// <summary>
    /// Gets all documents of the specified type.
    /// </summary>
    IEnumerable<T> GetDocuments<T>() where T : class;

    /// <summary>
    /// Creates an asset node with the specified asset.
    /// </summary>
    CanvasAssetNode CreateAssetNode(Asset asset, Size? size = null);

    /// <summary>
    /// Creates a document with the specified format and file path.
    /// </summary>
    CanvasAssetNode CreateDocument(DocumentFormat format, string rFilePath, Size? size = null);

    /// <summary>
    /// Creates a tool node of the specified type.
    /// </summary>
    T CreateToolNode<T>(Size? size = null) where T : CanvasToolNode, new();
}