using System;
using System.Text;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.LLm.CLI;

[CliCommandKey("start-chat")]
public class StartChatCommand : CliCommand
{
    public override string Description => "Start chat";

    public override string Usage => "start-chat (<asset-key>)";

    public override string? DoCommand(ICliArguments args)
    {
        string assetKey = args[0];
        if (!string.IsNullOrEmpty(assetKey))
        {
            var provider = AssetManager.Instance.GetAsset<ILLmChatProvider>(assetKey);
            if (provider is null)
            {
                throw new CliException("chat asset is not found");
            }

            AigcChatToolWindow.Instance.SelectedChatProvider = provider;
        }

        if (AigcChatToolWindow.Instance.SelectedChatProvider is null)
        {
            throw new CliException("chat is not set");
        }

        AigcChatToolWindow.Instance.HandleStartChat();

        RunChatLoopAsync().GetAwaiter().GetResult();
        return null;
    }

    private async Task RunChatLoopAsync()
    {
        while (true)
        {
            var inputBuilder = new StringBuilder();

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true);

                    if (key.Key == ConsoleKey.Escape)
                    {
                        Console.WriteLine("[Cancelled]");
                        AigcChatToolWindow.Instance.HandleStopChat();
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
                    AigcChatToolWindow.Instance.HandleStopChat();
                    break;
                }
                else
                {
                    try
                    {
                        await AigcChatToolWindow.Instance.HandleButtonClick(input);
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Error.WriteLine($"Error: {ex.Message}");
                        Console.ResetColor();
                    }
                }
            }
            else
            {
                try
                {
                    await AigcChatToolWindow.Instance.HandleInput(input);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine($"Error: {ex.Message}");
                    Console.ResetColor();
                }
            }

            QueuedAction.FlushQueuedActions();
        }

        QueuedAction.FlushQueuedActions();
    }
}
