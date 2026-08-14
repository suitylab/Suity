using Suity.Collections;
using Suity.Editor;
using Suity.Editor.ProjectGui.Nodes;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.Views.Menu;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Editor.ProjectGui.Commands.WorkSpaces;

/// <summary>
/// Command group for workspace backup and restore operations.
/// </summary>
internal class WorkSpaceBackupGroupCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkSpaceBackupGroupCommand"/> class.
    /// </summary>
    public WorkSpaceBackupGroupCommand()
        : base("Backup", CoreIconCache.Save.ToIconSmall())
    {
        AcceptType<WorkSpaceRootNode>(false);

        AddCommand(new BackupWorkSpaceCommand());
        AddCommand(new RestoreWorkSpaceCommand());
    }
}

/// <summary>
/// Command to back up the selected workspaces.
/// </summary>
internal class BackupWorkSpaceCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BackupWorkSpaceCommand"/> class.
    /// </summary>
    public BackupWorkSpaceCommand()
        : base("Backup", CoreIconCache.Save.ToIconSmall())
    {
        AcceptType<WorkSpaceRootNode>(false);
    }

    /// <inheritdoc/>
    public override async void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        var workSpaces = view.SelectedNodes
            .OfType<WorkSpaceRootNode>()
            .Select(o => o.WorkSpace)
            .SkipNull()
            .ToArray();

        string id = IdGenerator.GenerateId(12);
        string backupFileName = $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}_{id}";

        string name = await DialogUtility.ShowSingleLineTextDialogAsyncL("Input backup name", backupFileName, s =>
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return true;
            }

            if (NamingVerifier.VerifyFileName(s))
            {
                return true;
            }

            //DialogUtility.ShowMessageBoxAsyncL("The name is invalid.");
            return false;
        });

        name = name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        foreach (var workSpace in workSpaces)
        {
            try
            {
                workSpace.BackupWorkspace(name);
            }
            catch (Exception err)
            {
                err.LogErrorL("Backup failed: " + workSpace.Name);
            }
        }

        await DialogUtility.ShowMessageBoxAsyncL("Backup completed.");
    }
}

/// <summary>
/// Command to restore the selected workspaces from a backup.
/// </summary>
internal class RestoreWorkSpaceCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreWorkSpaceCommand"/> class.
    /// </summary>
    public RestoreWorkSpaceCommand()
        : base("Restore...", CoreIconCache.Download.ToIconSmall())
    {
        AcceptType<WorkSpaceRootNode>(false);
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        var workSpaces = view.SelectedNodes
            .OfType<WorkSpaceRootNode>()
            .Select(o => o.WorkSpace)
            .ToArray();

        // TODO: implement restore execution flow.
    }
}