using Suity.Editor.Conversation;
using Suity.Editor.Helpers;
using Suity.Editor.WorkSpaces;
using Suity.Views;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.Services;

public class AvaPlatformService : IPlatformService
{
    public static AvaPlatformService Instance { get; } = new();

    public ApplicationModes ApplicationMode => ApplicationModes.Desktop;

    public bool IsLocalDbSupported => true;

    public bool IsFileSystemWatcherSupported => true;

    public bool IsConsoleColorSupported => false;

    public bool IsConversationExtraButtonEnabled => true;

    public bool AutoSaveObjectId => false;

    public IPlatformFileSystem CreateFileSystem(Project project) => new ProjectFileSystem(project);

    public Task<string> ExecuteCommandAsync(string command, string? workingDirectory, Action<string>? onOutput, CancellationToken cancellation) 
        => ShellCommandHelper.ExecuteCommandAsync(command, workingDirectory, onOutput, cancellation);

    public void ExecuteWorkSpaceCommand(WorkSpace workSpace, string command, CancellationToken cancellation)
        => ShellCommandHelper.ExecuteExternalCommand(command, workSpace.MasterDirectory, cancellation);

    public IConversationHost CreateConversation(string id, ConversationOptions option)
    {
        return new ConversationImGui(id) 
        {
            DisableOldMessage = option.DisableOldMessage,
            Level = option.Level,
        };
    }

    public string BackupWorkspace(WorkSpace workspace, string? backupName = null, string? ignorePatterns = null)
    {
        if (workspace is null)
            throw new ArgumentNullException(nameof(workspace));

        return workspace.Backup(backupName, ignorePatterns);
    }
}
