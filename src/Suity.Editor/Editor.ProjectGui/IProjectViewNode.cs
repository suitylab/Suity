using Suity.Editor.WorkSpaces;

namespace Suity.Editor.ProjectGui;

/// <summary>
/// Represents a node in the project view hierarchy.
/// </summary>
public interface IProjectViewNode
{
    /// <summary>Gets the path of the node.</summary>
    string NodePath { get; }

    /// <summary>Gets the terminal name of the node.</summary>
    string Terminal { get; }
}

/// <summary>
/// Represents a directory node in the project view hierarchy.
/// </summary>
public interface IDirectoryNode : IProjectViewNode
{ }

/// <summary>
/// Represents a file node in the project view hierarchy.
/// </summary>
public interface IFileNode : IProjectViewNode
{ }

/// <summary>
/// Represents the root node for project assets.
/// </summary>
public interface IProjectAssetRootNode : IDirectoryNode
{ }

/// <summary>
/// Represents a workspace manager node.
/// </summary>
public interface IWorkSpaceManagerNode : IProjectViewNode
{ }

/// <summary>
/// Represents a workspace node in the project view hierarchy.
/// </summary>
public interface IWorkSpaceNode : IProjectViewNode
{
    /// <summary>Gets the associated workspace.</summary>
    WorkSpace WorkSpace { get; }
}

/// <summary>
/// Represents the root node for a workspace.
/// </summary>
public interface IWorkSpaceRootNode : IWorkSpaceNode, IDirectoryNode
{
}

/// <summary>
/// Represents a filesystem node in a workspace.
/// </summary>
public interface IWorkSpaceFsNode : IProjectViewNode
{
    /// <summary>Gets whether the node is currently being rendered.</summary>
    bool IsRendering { get; }

    /// <summary>Gets whether the node is new.</summary>
    bool IsNew { get; }
}

/// <summary>
/// Represents a directory node within a workspace.
/// </summary>
public interface IWorkSpaceDirectoryNode : IWorkSpaceNode, IDirectoryNode
{ }

/// <summary>
/// Represents a file node within a workspace.
/// </summary>
public interface IWorkSpaceFileNode : IWorkSpaceNode, IFileNode
{ }

/// <summary>
/// Represents a reference group node within a workspace.
/// </summary>
public interface IWorkSpaceReferenceGroupNode : IWorkSpaceNode
{ }

/// <summary>
/// Represents a reference node within a workspace.
/// </summary>
public interface IWorkSpaceReferenceNode : IWorkSpaceNode
{ }

/// <summary>
/// Represents a render target node in the project view hierarchy.
/// </summary>
public interface IRenderTargetNode : IProjectViewNode
{ }

/// <summary>
/// Represents an asset directory node.
/// </summary>
public interface IAssetDirectoryNode : IDirectoryNode
{ }

/// <summary>
/// Represents an asset file node.
/// </summary>
public interface IAssetFileNode : IFileNode, IHasId, IHasAsset
{
    /// <summary>Finds an element node by its key.</summary>
    /// <param name="elementKey">The key of the element to find.</param>
    /// <returns>The element node with the specified key, or null if not found.</returns>
    IAssetElementNode FindElement(string elementKey);
}

/// <summary>
/// Represents an asset element node.
/// </summary>
public interface IAssetElementNode : IProjectViewNode, IHasId
{
    /// <summary>Gets the key of this element.</summary>
    string ElementKey { get; }
}

/// <summary>
/// Represents an inner file node within a bunch.
/// </summary>
public interface IBunchInnerFileNode : IProjectViewNode
{ }

/// <summary>
/// Represents an asset filesystem node.
/// </summary>
public interface IAssetFsNode
{ }