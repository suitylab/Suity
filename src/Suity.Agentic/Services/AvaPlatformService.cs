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

    public Task<string> ExecuteCommandAsync(string command, string? workingDirectory, Action<string>? onOutput, CancellationToken token) 
        => ShellCommandHelper.ExecuteCommandAsync(command, workingDirectory, onOutput, token);

    public IConversationHost CreateConversation(string id, ConversationOptions option)
        => new ConversationImGui(id) { DisableOldMessage = option.DisableOldMessage };
}
