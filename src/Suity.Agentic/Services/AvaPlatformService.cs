using Suity.Editor.Helpers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.Services;

public class AvaPlatformService : IPlatformService
{
    public static AvaPlatformService Instance { get; } = new();

    public Task<string> ExecuteCommandAsync(string command, string? workingDirectory, Action<string>? onOutput, CancellationToken token)
    {
        return ShellCommandHelper.ExecuteCommandAsync(command, workingDirectory, onOutput, token);
    }
}
