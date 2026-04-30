using Suity.Editor.ProjectGui.Nodes;
using Suity.Editor.Services;
using Suity.Helpers;
using Suity.Views.Menu;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Suity.Editor.ProjectGui.Commands;

/// <summary>
/// Command that exports selected assets, directories, or workspaces as a package.
/// </summary>
public class ExportCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExportCommand"/> class.
    /// </summary>
    public ExportCommand()
        : base("Export", CoreIconCache.Export.ToIconSmall())
    {
        AcceptType<AssetRootNode>(false);
        AcceptType<AssetDirectoryNode>(false);
        AcceptType<AssetFileNode>(false);

        AcceptType<WorkSpaceManagerNode>(false);
        AcceptType<WorkSpaceRootNode>(false);
        AcceptType<WorkSpaceFileNode>(false);
        AcceptType<WorkSpaceDirectoryNode>(false);
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        HandleExport(view);
    }

    /// <summary>
    /// Handles the export operation for the specified project view.
    /// </summary>
    /// <param name="view">The project GUI view containing selected nodes to export.</param>
    public static void HandleExport(IProjectGui view)
    {
        //if (!ServiceInternals._license.GetCapability(EditorCapabilities.Export))
        //{
        //    Logs.LogError(ServiceInternals._license.GetFailedMessage(EditorCapabilities.Export));
        //    return;
        //}

        var selectedNodes = view.SelectedNodes.ToArray();
        if (selectedNodes.Length == 0)
        {
            return;
        }

        HashSet<string> files = [];
        List<string> workSpaces = [];

        foreach (var fileNode in selectedNodes.OfType<AssetFileNode>())
        {
            if (File.Exists(fileNode.NodePath))
            {
                files.Add(fileNode.NodePath);
            }
        }

        foreach (var dirNode in selectedNodes.OfType<AssetRootNode>())
        {
            var directory = new DirectoryInfo(dirNode.NodePath);
            if (directory.Exists)
            {
                foreach (var file in directory.EnumerateFiles("*.*", SearchOption.AllDirectories))
                {
                    files.Add(file.FullName);
                }
            }
        }

        foreach (var dirNode in selectedNodes.OfType<AssetDirectoryNode>())
        {
            var directory = new DirectoryInfo(dirNode.NodePath);
            if (directory.Exists)
            {
                foreach (var file in directory.EnumerateFiles("*.*", SearchOption.AllDirectories))
                {
                    files.Add(file.FullName);
                }
            }
        }

        foreach (var workspaceNode in selectedNodes.OfType<WorkSpaceManagerNode>())
        {
            foreach (var workspace in EditorServices.WorkSpaceManager.WorkSpaces)
            {
                workSpaces.Add(workspace.Name);
            }
        }

        foreach (var workspaceNode in selectedNodes.OfType<WorkSpaceRootNode>())
        {
            workSpaces.Add(workspaceNode.WorkSpace.Name);
        }

        foreach (var fileNode in selectedNodes.OfType<WorkSpaceFileNode>())
        {
            var workSpace = fileNode.FindWorkSpace();
            if (workSpace != null)
            {
                workSpaces.Add(workSpace.Name);
            }
        }

        foreach (var fileNode in selectedNodes.OfType<WorkSpaceDirectoryNode>())
        {
            var workSpace = fileNode.FindWorkSpace();
            if (workSpace != null)
            {
                workSpaces.Add(workSpace.Name);
            }
        }

        EditorUtility.ShowExportPackage(files, workSpaces);
    }
}