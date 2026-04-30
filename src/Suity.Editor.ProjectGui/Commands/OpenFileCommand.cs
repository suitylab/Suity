using Suity.Editor.Documents;
using Suity.Editor.ProjectGui.Nodes;
using Suity.Helpers;
using Suity.Views;
using Suity.Views.Menu;
using Suity.Views.PathTree;
using System;
using System.Diagnostics;
using System.IO;

namespace Suity.Editor.ProjectGui.Commands;

/// <summary>
/// Command that opens a selected file in the appropriate editor or external program.
/// </summary>
internal class OpenFileCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OpenFileCommand"/> class.
    /// </summary>
    public OpenFileCommand()
        : base("Open", CoreIconCache.Open.ToIconSmall())
    {
        AcceptType<AssetFileNode>(false);
        AcceptType<WorkSpaceFileNode>(false);
        AcceptType<FileNode>(false);
        AcceptType<RenderTargetNode>(false);
        AcceptType<AssetElementNode>(false);
        AcceptOneItemOnly = true;
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        HandleOpen(view, view.SelectedNode);
    }

    /// <summary>
    /// Handles opening the specified node in the appropriate viewer or editor.
    /// </summary>
    /// <param name="view">The project GUI view.</param>
    /// <param name="node">The path node to open.</param>
    /// <returns>True if the node was handled; otherwise, false.</returns>
    public static bool HandleOpen(IProjectGui view, PathNode node)
    {
        if (node is AssetFileNode || node is RenderTargetNode || node is WorkSpaceFileNode)
        {
            QueuedAction.Do(() =>
            {
                if (node is AssetFileNode { TargetAsset: IViewDoubleClickAction dblClick })
                {
                    try
                    {
                        dblClick.DoubleClick();
                    }
                    catch (Exception err)
                    {
                        err.LogError();
                    }

                    return;
                }

                var doc = DocumentManager.Instance.OpenDocument(node.NodePath);
                if (doc?.Content != null)
                {
                    doc.ShowView();

                    // Regardless of ShowView success, do not execute Process
                    return;
                }

                // If asset has no document, try to open with external program.

                if (File.Exists(node.NodePath))
                {
                    try
                    {
                        Process.Start(node.NodePath);
                    }
                    catch (Exception err)
                    {
                        Logs.LogError(err);
                    }
                }
            });

            return true;
        }
        if (node is AssetElementNode elementNode)
        {
            QueuedAction.Do(() =>
            {
                var document = DocumentManager.Instance.OpenDocument(elementNode.GetFilePath());
                if (document != null)
                {
                    var docView = document.ShowView();
                    if (docView != null)
                    {
                        // Build resource starting from the second frame
                        QueuedAction.Do(() =>
                        {
                            docView.GetService<IViewSelectable>()?.SetSelection(new ViewSelection(elementNode.Terminal));
                        });
                    }
                }
            });
            return true;
        }
        return false;
    }
}