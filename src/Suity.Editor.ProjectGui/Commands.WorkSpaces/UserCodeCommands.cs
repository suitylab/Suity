using Suity.Editor;
using Suity.Editor.CodeRender;
using Suity.Editor.ProjectGui.Nodes;
using Suity.Editor.Selecting;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.Views.Menu;
using System.Linq;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.ProjectGui.Commands.WorkSpaces;

/// <summary>
/// Command group for user code operations including commit and restore.
/// </summary>
internal class UserCodeGroupCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserCodeGroupCommand"/> class.
    /// </summary>
    public UserCodeGroupCommand()
        : base("User Code", CoreIconCache.Restore.ToIconSmall())
    {
        AcceptType<WorkSpaceReferenceNode>(false);
        AcceptType<WorkSpaceManagerNode>(false);
        AcceptType<WorkSpaceRootNode>(false);

        AddCommand(new StoreAllUserCodeToWorkSpaceCommand());
        AddCommand(new StoreAllUserCodeToFileCommand());
        AddCommand(new StoreUserCodeToWorkSpaceCommand());
        AddCommand(new StoreUserCodeToFileCommand());
        AddCommand(new StoreOneUserCodeToWorkSpaceCommand());
        AddCommand(new StoreOrCreateOneUserCodeFileCommand());

        AddSeparator();

        AddCommand(new RestoreAllUserCodeCommand());
        AddCommand(new RestoreWorkSpaceUserCodeCommand());
        AddCommand(new RestoreOneUserCodeCommand());
        AddCommand(new RestoreOneUserCodeFromOtherWorkSpaceCommand());
        AddCommand(new RestoreOneUserCodeFromOtherLibraryCommand());
    }
}

/// <summary>
/// Command to commit all user code from all workspaces to the workspace storage.
/// </summary>
internal class StoreAllUserCodeToWorkSpaceCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StoreAllUserCodeToWorkSpaceCommand"/> class.
    /// </summary>
    public StoreAllUserCodeToWorkSpaceCommand()
        : base("Commit All to Workspace", CoreIconCache.Upload.ToIconSmall())
    {
        AcceptType<WorkSpaceManagerNode>(false);
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        EditorUtility.StartBuildTask(() =>
        {
            var project = Project.Current;

            foreach (var workSpace in WorkSpaceManager.Current.WorkSpaces)
            {
                foreach (var id in workSpace.ReferenceIds)
                {
                    var setup = workSpace.GetReferenceItem(id);
                    if (setup != null)
                    {
                        CodeRenderUtility.UploadWorkSpaceUserCode(workSpace, setup);
                    }
                }
            }

            Logs.LogInfo(L("All completed"));
        });
    }
}

/// <summary>
/// Command to commit all user code from all workspaces to individual user files.
/// </summary>
internal class StoreAllUserCodeToFileCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StoreAllUserCodeToFileCommand"/> class.
    /// </summary>
    public StoreAllUserCodeToFileCommand()
        : base("Commit All to User File", CoreIconCache.Upload.ToIconSmall())
    {
        AcceptType<WorkSpaceManagerNode>(false);
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        EditorUtility.StartBuildTask(() =>
        {
            var project = Project.Current;

            foreach (var workSpace in WorkSpaceManager.Current.WorkSpaces)
            {
                foreach (var id in workSpace.ReferenceIds)
                {
                    var setup = workSpace.GetReferenceItem(id);
                    if (setup?.UserCode != null)
                    {
                        if (!CodeRenderUtility.CreateUserCodeFile(workSpace, setup))
                        {
                            Logs.LogError(L("Failed to create user file") + ": " + id);
                        }
                    }
                }
            }

            Logs.LogInfo(L("All completed"));
        });
    }
}

/// <summary>
/// Command to restore all user code across all workspaces.
/// </summary>
internal class RestoreAllUserCodeCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreAllUserCodeCommand"/> class.
    /// </summary>
    public RestoreAllUserCodeCommand()
        : base("Restore All", CoreIconCache.Download.ToIconSmall())
    {
        AcceptType<WorkSpaceManagerNode>(false);
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        EditorUtility.StartBuildTask(() =>
        {
            var project = Project.Current;

            foreach (var workSpace in WorkSpaceManager.Current.WorkSpaces)
            {
                foreach (var id in workSpace.ReferenceIds)
                {
                    var setup = workSpace.GetReferenceItem(id);
                    var userCode = setup?.UserCode;
                    CodeRenderUtility.RestoreWorkSpace(workSpace, setup, userCode);
                }
            }

            Logs.LogInfo(L("All completed"));
        });
    }
}

/// <summary>
/// Command to commit user code from selected workspaces to the workspace storage.
/// </summary>
internal class StoreUserCodeToWorkSpaceCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StoreUserCodeToWorkSpaceCommand"/> class.
    /// </summary>
    public StoreUserCodeToWorkSpaceCommand()
     : base("Commit to Workspace", CoreIconCache.Upload.ToIconSmall())
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

        EditorUtility.StartBuildTask(() =>
        {
            foreach (var workSpaceNode in view.SelectedNodes.OfType<WorkSpaceRootNode>().ToArray())
            {
                var workSpace = workSpaceNode.WorkSpace;
                foreach (var id in workSpace.ReferenceIds)
                {
                    var setup = workSpace.GetReferenceItem(id);
                    if (setup != null)
                    {
                        CodeRenderUtility.UploadWorkSpaceUserCode(workSpace, setup);
                    }
                }
            }

            Logs.LogInfo(L("All completed"));
        });
    }
}

/// <summary>
/// Command to commit user code from selected workspaces to individual user files.
/// </summary>
internal class StoreUserCodeToFileCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StoreUserCodeToFileCommand"/> class.
    /// </summary>
    public StoreUserCodeToFileCommand()
     : base("Commit to User File", CoreIconCache.Upload.ToIconSmall())
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

        EditorUtility.StartBuildTask(() =>
        {
            foreach (var workSpaceNode in view.SelectedNodes.OfType<WorkSpaceRootNode>().ToArray())
            {
                var workSpace = workSpaceNode.WorkSpace;
                foreach (var id in workSpace.ReferenceIds)
                {
                    var setup = workSpace.GetReferenceItem(id);
                    if (setup?.UserCode != null)
                    {
                        if (!CodeRenderUtility.CreateUserCodeFile(workSpace, setup))
                        {
                            Logs.LogError(L("Failed to create user file") + ": " + id);
                        }
                    }
                }
            }

            Logs.LogInfo(L("All completed"));
        });
    }
}

/// <summary>
/// Command to restore user code for selected workspaces.
/// </summary>
internal class RestoreWorkSpaceUserCodeCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreWorkSpaceUserCodeCommand"/> class.
    /// </summary>
    public RestoreWorkSpaceUserCodeCommand()
        : base("Restore", CoreIconCache.Download.ToIconSmall())
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

        EditorUtility.StartBuildTask(() =>
        {
            foreach (var workSpaceNode in view.SelectedNodes.OfType<WorkSpaceRootNode>().ToArray())
            {
                var workSpace = workSpaceNode.WorkSpace;

                foreach (var id in workSpace.ReferenceIds)
                {
                    var setup = workSpace.GetReferenceItem(id);
                    var userCode = setup?.UserCode;
                    CodeRenderUtility.RestoreWorkSpace(workSpace, setup, userCode);
                }
            }

            Logs.LogInfo(L("All completed"));
        });
    }
}

/// <summary>
/// Command to commit user code for a single reference to the workspace storage.
/// </summary>
internal class StoreOneUserCodeToWorkSpaceCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StoreOneUserCodeToWorkSpaceCommand"/> class.
    /// </summary>
    public StoreOneUserCodeToWorkSpaceCommand()
     : base("Commit to Workspace", CoreIconCache.Upload.ToIconSmall())
    {
        AcceptType<WorkSpaceReferenceNode>(false);
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        EditorUtility.StartBuildTask(() =>
        {
            foreach (var refNode in view.SelectedNodes.OfType<WorkSpaceReferenceNode>().ToArray())
            {
                var setup = refNode.GetReferenceItem();
                if (setup != null && refNode.WorkSpace is WorkSpace workSpace)
                {
                    CodeRenderUtility.UploadWorkSpaceUserCode(workSpace, setup);
                }
            }

            Logs.LogInfo(L("All completed"));
        });
    }
}

/// <summary>
/// Command to commit user code or create a user file for a single reference.
/// </summary>
internal class StoreOrCreateOneUserCodeFileCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StoreOrCreateOneUserCodeFileCommand"/> class.
    /// </summary>
    public StoreOrCreateOneUserCodeFileCommand()
        : base("Commit or Create User File", CoreIconCache.Upload.ToIconSmall())
    {
        AcceptType<WorkSpaceReferenceNode>(false);
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        EditorUtility.StartBuildTask(() =>
        {
            foreach (var refNode in view.SelectedNodes.OfType<WorkSpaceReferenceNode>().ToArray())
            {
                var setup = refNode.GetReferenceItem();
                if (!CodeRenderUtility.CreateUserCodeFile(refNode.WorkSpace, setup))
                {
                    Logs.LogError(L("Failed to generate user file") + ": " + refNode.Terminal);
                }
            }

            QueuedAction.Do(() => EditorUtility.Inspector.UpdateInspector());

            Logs.LogInfo(L("All completed"));
        });
    }
}

/// <summary>
/// Command to restore user code for a single reference.
/// </summary>
internal class RestoreOneUserCodeCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreOneUserCodeCommand"/> class.
    /// </summary>
    public RestoreOneUserCodeCommand()
        : base("Restore", CoreIconCache.Download.ToIconSmall())
    {
        AcceptType<WorkSpaceReferenceNode>(false);
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        EditorUtility.StartBuildTask(() =>
        {
            foreach (var node in view.SelectedNodes.ToArray())
            {
                if (node is WorkSpaceReferenceNode refNode)
                {
                    var userCode = refNode.GetReferenceItem()?.UserCode;
                    CodeRenderUtility.RestoreWorkSpace(refNode.WorkSpace, refNode.GetReferenceItem(), userCode);
                }
            }

            Logs.LogInfo(L("All completed"));
        });
    }
}

/// <summary>
/// Command to restore user code for a single reference from another workspace.
/// </summary>
internal class RestoreOneUserCodeFromOtherWorkSpaceCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreOneUserCodeFromOtherWorkSpaceCommand"/> class.
    /// </summary>
    public RestoreOneUserCodeFromOtherWorkSpaceCommand()
        : base("Restore From Workspace...", CoreIconCache.Download.ToIconSmall())
    {
        AcceptType<WorkSpaceReferenceNode>(false);
    }

    /// <inheritdoc/>
    public override async void DoCommand()
    {
        var selection = new AssetSelection<WorkSpaceAsset>();
        bool success = await selection.ShowSelectionGUIAsync(L("Select user code workspace to restore from"));
        if (!success)
        {
            return;
        }

        var workSpace = selection.Target?.WorkSpace;
        if (workSpace is null)
        {
            return;
        }

        if (Sender is not IProjectGui view)
        {
            return;
        }

        await EditorUtility.StartBuildTask(() =>
        {
            foreach (var node in view.SelectedNodes.ToArray())
            {
                if (node is WorkSpaceReferenceNode refNode)
                {
                    CodeRenderUtility.RestoreWorkSpace(refNode.WorkSpace, refNode.GetReferenceItem(), workSpace.UserCode);
                }
            }

            Logs.LogInfo(L("All completed"));
        });
    }
}

/// <summary>
/// Command to restore user code for a single reference from another code library.
/// </summary>
internal class RestoreOneUserCodeFromOtherLibraryCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreOneUserCodeFromOtherLibraryCommand"/> class.
    /// </summary>
    public RestoreOneUserCodeFromOtherLibraryCommand()
        : base("Restore From File...", CoreIconCache.Download.ToIconSmall())
    {
        AcceptType<WorkSpaceReferenceNode>(false);
    }

    /// <inheritdoc/>
    public override async void DoCommand()
    {
        var selection = new AssetSelection<ICodeLibrary>();
        bool success = await selection.ShowSelectionGUIAsync(L("Select user code library to restore from"));
        if (!success)
        {
            return;
        }

        var userCode = selection.Target;
        if (userCode is null)
        {
            return;
        }

        if (Sender is not IProjectGui view)
        {
            return;
        }

        await EditorUtility.StartBuildTask(() =>
        {
            foreach (var node in view.SelectedNodes.ToArray())
            {
                if (node is WorkSpaceReferenceNode refNode)
                {
                    CodeRenderUtility.RestoreWorkSpace(refNode.WorkSpace, refNode.GetReferenceItem(), userCode);
                }
            }

            Logs.LogInfo(L("All completed"));
        });
    }
}
