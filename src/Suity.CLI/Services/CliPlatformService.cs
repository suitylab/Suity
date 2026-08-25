using Suity.Editor.Conversation;
using Suity.Editor.Helpers;
using Suity.Views;

namespace Suity.Editor.Services;

public class CliPlatformService : IPlatformService
{
    public static CliPlatformService Instance { get; } = new();

    public Task<string> ExecuteCommandAsync(string command, string? workingDirectory, Action<string>? onOutput, CancellationToken token)
        => ShellCommandHelper.ExecuteCommandAsync(command, workingDirectory, onOutput, token);

    public IConversationHost CreateConversation(string id, ConversationOptions option)
    {
        return new CliConversation(id)
        {
            DisableOldMessage = option.DisableOldMessage,
            Level = option.Level,
        };
    }
}
