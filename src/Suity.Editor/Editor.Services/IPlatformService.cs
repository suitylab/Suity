using System;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.Services;

public interface IPlatformService
{
    /// <summary>
    /// Executes a command asynchronously.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="workingDirectory">The working directory.</param>
    /// <param name="onOutput">The output callback.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The command output.</returns>
    Task<string> ExecuteCommandAsync(string command, string? workingDirectory, Action<string>? onOutput, CancellationToken token);

}
