using Suity.Editor.WorkSpaces;
using System.Collections.Generic;

namespace Suity.Editor.ProjectGui;

/// <summary>
/// Provides access to the project view selection and navigation.
/// </summary>
public interface IProjectView
{
    /// <summary>Gets the selected nodes of the specified type.</summary>
    /// <typeparam name="T">The type of nodes to retrieve.</typeparam>
    /// <returns>An enumerable of selected nodes of type T.</returns>
    IEnumerable<T> GetSelectedNodes<T>() where T : IProjectViewNode;

    /// <summary>Gets the currently selected node.</summary>
    IProjectViewNode SelectedNode { get; }

    /// <summary>Finds a file node by its name.</summary>
    /// <param name="fileName">The name of the file to find.</param>
    /// <returns>The file node with the specified name, or null if not found.</returns>
    IProjectViewNode FindFileNode(string fileName);

    /// <summary>Finds an asset file node by its editor object.</summary>
    /// <param name="obj">The editor object to search for.</param>
    /// <returns>The asset file node associated with the object, or null if not found.</returns>
    IAssetFileNode FindFileNode(EditorObject obj);

    /// <summary>Finds a workspace node by its workspace.</summary>
    /// <param name="workSpace">The workspace to find.</param>
    /// <returns>The workspace node associated with the workspace, or null if not found.</returns>
    IWorkSpaceNode FindWorkSpaceNode(WorkSpace workSpace);
}