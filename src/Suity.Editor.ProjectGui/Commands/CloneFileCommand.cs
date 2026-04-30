using Suity.Editor.Documents;
using Suity.Helpers;
using Suity.Views.Menu;
using Suity.Views.PathTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Suity.Editor.ProjectGui.Commands;

/// <summary>
/// Command that clones selected file nodes by creating copies with incremented names.
/// </summary>
internal class CloneFileCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CloneFileCommand"/> class.
    /// </summary>
    public CloneFileCommand()
        : base("Clone", CoreIconCache.Copy)
    {
        // AcceptOneItemOnly = true;
    }

    /// <inheritdoc/>
    protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        base.OnPopUp(selectionCount, types, commonNodeType);
        if (!Visible)
        {
            return;
        }

        if (Sender is not IProjectGui view)
        {
            Visible = false;

            return;
        }

        Visible = view.SelectedNode is FileNode;
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        var fileNodes = view.SelectedNodes?.OfType<FileNode>().ToArray() ?? [];

        foreach (var fileNode in fileNodes)
        {
            DoClone(fileNode);
        }
    }

    private static void DoClone(FileNode fileNode)
    {
        string path = fileNode.NodePath;
        string ext = Path.GetExtension(path);
        string name = path.RemoveFromLast(ext.Length);

        KeyIncrementHelper.ParseKey(name, out string prefix, out int digiLen, out ulong digiValue);

        string newPath;
        while (true)
        {
            digiValue++;
            newPath = KeyIncrementHelper.MakeKey(prefix, digiLen, digiValue) + ext;
            if (!File.Exists(newPath))
            {
                break;
            }
        }

        try
        {
            var docClone = DocumentManager.Instance.CloneDocument(path, newPath);
            if (docClone is null)
            {
                File.Copy(path, newPath);
            }
        }
        catch (Exception err)
        {
            err.LogError();
        }
    }
}
