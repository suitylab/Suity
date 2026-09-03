using Suity.Editor.Services;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC;

public static class ChatReplInterface
{
    public static async Task RunChatLoopAsync(Task runningTask)
    {
        while (true)
        {
            if (runningTask.IsCompleted)
                break;

            var inputBuilder = new StringBuilder();

            while (true)
            {
                if (runningTask.IsCompleted)
                    break;

                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true);

                    if (key.Key == ConsoleKey.Escape)
                    {
                        Console.WriteLine("[Cancelled]");
                        LLmService.Instance.StopChat();
                        await runningTask;
                        return;
                    }

                    if (key.Key == ConsoleKey.Enter)
                    {
                        Console.WriteLine();
                        break;
                    }

                    if (key.Key == ConsoleKey.Backspace)
                    {
                        if (inputBuilder.Length > 0)
                        {
                            inputBuilder.Remove(inputBuilder.Length - 1, 1);
                            Console.Write("\b \b");
                        }
                    }
                    else if (key.KeyChar != '\0')
                    {
                        inputBuilder.Append(key.KeyChar);
                        Console.Write(key.KeyChar);
                    }
                }
                else
                {
                    await Task.Delay(10);
                }
            }

            var input = inputBuilder.ToString().Trim();

            if (string.IsNullOrWhiteSpace(input))
                continue;

            if (input.StartsWith("/"))
            {
                input = input[1..];

                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                    input.Equals("quit", StringComparison.OrdinalIgnoreCase))
                {
                    LLmService.Instance.StopChat();
                    await runningTask;
                    break;
                }
                else
                {
                    try
                    {
                        await LLmService.Instance.ChatButtonClick(input);
                    }
                    catch (Exception ex)
                    {
                        if (EditorServices.PlatformService.IsConsoleColorSupported)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                        }

                        Console.Error.WriteLine($"Error: {ex.Message}");
                        Console.ResetColor();
                    }
                }
            }
            else
            {
                try
                {
                    await LLmService.Instance.ChatMessageInput(input);
                }
                catch (Exception ex)
                {
                    if (EditorServices.PlatformService.IsConsoleColorSupported)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }

                    Console.Error.WriteLine($"Error: {ex.Message}");
                    Console.ResetColor();
                }
            }

            QueuedAction.FlushQueuedActions();
        }

        QueuedAction.FlushQueuedActions();
    }
}
