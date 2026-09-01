using Suity.Editor.Conversation;
using Suity.Editor.Helpers;
using Suity.Views;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.Services;

public class AvaPlatformService : IPlatformService
{
    public static AvaPlatformService Instance { get; } = new();

    public ApplicationModes ApplicationMode => ApplicationModes.Desktop;

    public bool IsLocalDbEnabled => true;

    public Task<string> ExecuteCommandAsync(string command, string? workingDirectory, Action<string>? onOutput, CancellationToken token) 
        => ShellCommandHelper.ExecuteCommandAsync(command, workingDirectory, onOutput, token);

    public IConversationHost CreateConversation(string id, ConversationOptions option)
    {
        return new ConversationImGui(id) 
        {
            DisableOldMessage = option.DisableOldMessage,
            Level = option.Level,
        };
    }
}
