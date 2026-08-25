using Suity.Views;
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
    Task<string> ExecuteCommandAsync(string command, string workingDirectory, Action<string> onOutput, CancellationToken token);

    /// <summary>
    /// Creates a conversation instance base on the current running environment.
    /// </summary>
    /// <param name="id">The conversation identifier.</param>
    /// <param name="disableOldMessage">Whether to disable old messages.</param>
    /// <returns>A conversation host instance.</returns>
    IConversationHost CreateConversation(string id, bool disableOldMessage = true);
}
