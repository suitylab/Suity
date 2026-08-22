using Suity.Editor.Helpers;

namespace Suity.Editor.Services;

public class CliPlatformService : IPlatformService
{
    public static CliPlatformService Instance { get; } = new();

    public Task<string> ExecuteCommandAsync(string command, string? workingDirectory, Action<string>? onOutput, CancellationToken token)
    {
        return ShellCommandHelper.ExecuteCommandAsync(command, workingDirectory, onOutput, token);
    }
}
