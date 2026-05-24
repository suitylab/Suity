using Suity.Editor.Documents;
using Suity.Helpers;
using Suity.Rex;
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

        string[] filePaths = view.SelectedNodes?.OfType<FileNode>().Select(o => o.NodePath).ToArray() ?? [];
        string[] clonePaths = new string[filePaths.Length];

        DoClone(filePaths, clonePaths);
    }

    private static void DoClone(string[] filePaths, string[] clonePaths)
    {
        HashSet<string> existedPaths = [];

        for (int i = 0; i < filePaths.Length; i++)
        {
            var path = filePaths[i];
            var newPath = GetClonePath(path, existedPaths);

            clonePaths[i] = newPath;
        }

        var docClones = DocumentManager.Instance.CloneDocuments(filePaths, clonePaths);

        for (int i = 0; i < filePaths.Length; i++)
        {
            var doc = docClones[i];
            if (doc is not null)
            {
                continue;
            }

            string path = filePaths[i];
            string newPath = clonePaths[i];

            try
            {
                File.Copy(path, newPath);
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }
    }

    private static string GetClonePath(string path, HashSet<string> existedPaths)
    {
        string ext = Path.GetExtension(path);
        string name = path.RemoveFromLast(ext.Length);

        KeyIncrementHelper.ParseKey(name, out string prefix, out int digiLen, out ulong digiValue);

        string newPath;
        while (true)
        {
            digiValue++;
            newPath = KeyIncrementHelper.MakeKey(prefix, digiLen, digiValue) + ext;
            if (!File.Exists(newPath) && !existedPaths.Contains(newPath))
            {
                existedPaths.Add(newPath);
                break;
            }
        }

        return newPath;
    }


}
