using Suity.Editor.ProjectGui.Nodes;
using Suity.Helpers;
using Suity.Views.Menu;

namespace Suity.Editor.ProjectGui.Commands;

/// <summary>
/// Command that initiates renaming of a selected file or directory node.
/// </summary>
internal class RenameCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RenameCommand"/> class.
    /// </summary>
    public RenameCommand()
        : base("Rename", CoreIconCache.Rename.ToIconSmall())
    {
        AcceptType<AssetDirectoryNode>(false);
        AcceptType<AssetFileNode>(false);

        AcceptType<WorkSpaceDirectoryNode>(false);
        AcceptType<WorkSpaceFileNode>(false);

        AcceptType<PublishDirectoryNode>(false);
        AcceptType<PublishFileNode>(false);

        AcceptOneItemOnly = true;
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        var node = view.SelectedNode;
        if (node != null)
        {
            view.SelectNode(node, true);
        }
    }
}