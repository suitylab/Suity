using Suity.Editor.Conversation;
using Suity.Editor.Helpers;
using Suity.Views;

namespace Suity.Editor.Services;

public class CliPlatformService : IPlatformService
{
    public static CliPlatformService Instance { get; } = new();

    public Task<string> ExecuteCommandAsync(string command, string? workingDirectory, Action<string>? onOutput, CancellationToken token)
        => ShellCommandHelper.ExecuteCommandAsync(command, workingDirectory, onOutput, token);

    public IConversationHost CreateConversation(string id, bool disableOldMessage = true)
        => new ConversationImGui(id) { DisableOldMessage = disableOldMessage };
}
