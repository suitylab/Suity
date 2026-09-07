using Suity.Editor.Conversation;
using Suity.Editor.Helpers;
using Suity.Editor.WorkSpaces;
using Suity.Views;

namespace Suity.Editor.Services;

public class CliPlatformService : IPlatformService
{
    public static CliPlatformService Instance { get; } = new();

    public ApplicationModes ApplicationMode => ApplicationModes.CLI;

    public bool IsLocalDbSupported => false;

    public bool IsFileSystemWatcherSupported => true;

    public bool IsConsoleColorSupported => true;

    public bool IsConversationExtraButtonEnabled => false;

    public bool AutoSaveObjectId => false;

    public Task<string> ExecuteCommandAsync(string command, string? workingDirectory, Action<string>? onOutput, CancellationToken cancellation)
        => ShellCommandHelper.ExecuteCommandAsync(command, workingDirectory, onOutput, cancellation);

    public void ExecuteWorkSpaceCommand(WorkSpace workSpace, string command, CancellationToken cancellation) 
        => ShellCommandHelper.ExecuteExternalCommand(command, workSpace.MasterDirectory, cancellation);

    public IConversationHost CreateConversation(string id, ConversationOptions option)
    {
        return new CliConversation(id)
        {
            DisableOldMessage = option.DisableOldMessage,
            Level = option.Level,
        };
    }

}
