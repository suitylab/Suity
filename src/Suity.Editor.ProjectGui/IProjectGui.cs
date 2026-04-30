using Suity.Editor.ProjectGui.Nodes;
using Suity.Editor.WorkSpaces;
using Suity.Views;
using Suity.Views.PathTree;
using System.Collections.Generic;

namespace Suity.Editor.ProjectGui;

/// <summary>
/// Provides access to the project view GUI and its operations.
/// </summary>
public interface IProjectGui : IDropTarget
{
    /// <summary>
    /// Gets the underlying path tree model for the project view.
    /// </summary>
    PathTreeModel Model { get; }

    /// <summary>
    /// Gets the currently opened project.
    /// </summary>
    Project CurrentProject { get; }

    /// <summary>
    /// Gets all currently selected nodes in the project tree view.
    /// </summary>
    IEnumerable<PathNode> SelectedNodes { get; }

    /// <summary>
    /// Gets the primary selected node in the project tree view.
    /// </summary>
    PathNode SelectedNode { get; }

    /// <summary>
    /// Gets the selected directory node, or null if the selection is not a directory.
    /// </summary>
    DirectoryNode SelectedDirectory { get; }

    /// <summary>
    /// Selects a single node in the project tree view.
    /// </summary>
    /// <param name="node">The node to select.</param>
    /// <param name="beginEdit">Whether to begin rename editing on the node after selection.</param>
    void SelectNode(PathNode node, bool beginEdit);

    /// <summary>
    /// Selects multiple nodes in the project tree view.
    /// </summary>
    /// <param name="nodes">The collection of nodes to select.</param>
    void SelectNodes(IEnumerable<PathNode> nodes);

    /// <summary>
    /// Inspects the currently selected nodes and displays their properties in the inspector panel.
    /// </summary>
    void InspectSelectedNodes();

    /// <summary>
    /// Begins a batch update operation on the project view, deferring UI refreshes.
    /// </summary>
    void BeginUpdate();

    /// <summary>
    /// Ends a batch update operation and triggers a UI refresh.
    /// </summary>
    void EndUpdate();

    /// <summary>
    /// Refreshes all asset-related project nodes by re-populating the tree.
    /// </summary>
    void RefreshProjectNodes();

    /// <summary>
    /// Refreshes all workspace-related project nodes by re-populating the tree.
    /// </summary>
    void RefreshWorkSpaceNodes();

    /// <summary>
    /// Finds a file node in the project tree by its full file path.
    /// </summary>
    /// <param name="fileName">The full path of the file to locate.</param>
    /// <returns>The matching <see cref="PathNode"/>, or null if not found.</returns>
    PathNode FindFileNode(string fileName);

    /// <summary>
    /// Finds the workspace root node associated with the specified workspace.
    /// </summary>
    /// <param name="workSpace">The workspace to locate.</param>
    /// <returns>The matching <see cref="WorkSpaceRootNode"/>, or null if not found.</returns>
    WorkSpaceRootNode FindWorkSpaceNode(WorkSpace workSpace);
}