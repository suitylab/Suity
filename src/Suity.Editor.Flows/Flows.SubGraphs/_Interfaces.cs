using Suity.Editor.Documents.Linked;
using Suity.Editor.Flows;
using Suity.Editor.Types;

namespace Suity.Editor.AIGC.Flows.Pages;

/// <summary>
/// Defines the types of page commit events that can occur in the AIGC workflow.
/// </summary>
public enum PageCommitTypes
{
    /// <summary>
    /// No commit type specified.
    /// </summary>
    [DisplayText("None")]
    None,

    /// <summary>
    /// Indicates the task has finished successfully.
    /// </summary>
    [DisplayText("Task Finished")]
    TaskFinished,

    /// <summary>
    /// Indicates the task has failed.
    /// </summary>
    [DisplayText("Task Failed")]
    TaskFailed,
}

/// <summary>
/// Represents an AIGC node that has an associated type definition.
/// </summary>
public interface IAigcTypeNode
{
    /// <summary>
    /// Gets the type definition associated with this node.
    /// </summary>
    TypeDefinition TypeDef { get; }

    /// <summary>
    /// Gets a value indicating whether the parameter is displayed as a link address instead of content.
    /// </summary>
    bool LinkedMode { get; }
}

/// <summary>
/// Represents an AIGC end node that has an associated type definition and an end type.
/// </summary>
public interface IAigcEndNode : IAigcTypeNode
{
    /// <summary>
    /// Gets the type of page commit that occurs when this end node is reached.
    /// </summary>
    PageCommitTypes EndType { get; }
}

/// <summary>
/// Represents a connection between pages in the AIGC workflow.
/// </summary>
[NativeType(CodeBase = "AIGC", Description = "Page Connection", Color = "#333333")]
public interface IPageConnection
{

}

/// <summary>
/// <summary>
/// Filters assets to only include those that belong to the same document as the flow node.
/// </summary>
class SameDocFilter : IAssetFilter
{
    readonly FlowNode _node;

    /// <summary>
    /// Initializes a new instance of the <see cref="SameDocFilter"/> class.
    /// </summary>
    /// <param name="node">The flow node used to determine the document context.</param>
    public SameDocFilter(FlowNode node)
    {
        _node = node ?? throw new System.ArgumentNullException(nameof(node));
    }

    /// <inheritdoc/>
    public bool FilterAsset(Asset asset)
    {
        if (asset is null)
        {
            return false;
        }

        if (asset.ParentAsset is not { } parentAsset)
        {
            return false;
        }

        return parentAsset == (_node.DiagramItem as SNamedItem)?.GetDocument()?.GetAsset();
    }
}
