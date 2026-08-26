using System;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.LLm.CLI;

[CliCommandKey("start-chat")]
public class StartChatCommand : CliCommand
{
    public override string Description => "Start chat";

    public override string Usage => "start-chat (<asset-key>)";

    public override void DoCommand(ICliArguments args)
    {
        string assetKey = args[0];
        if (!string.IsNullOrEmpty(assetKey))
        {
            var provider = AssetManager.Instance.GetAsset<ILLmChatProvider>(assetKey);
            if (provider is null)
            {
                Console.WriteLine("chat asset is not found");
                return;
            }

            AigcChatToolWindow.Instance.SelectedChatProvider = provider;
            Console.WriteLine("chat is set to " + assetKey);
        }

        if (AigcChatToolWindow.Instance.SelectedChatProvider is null)
        {
            Console.WriteLine("chat is not set");
            return;
        }

        AigcChatToolWindow.Instance.HandleStart();

        Console.WriteLine("Enter /exit or /quit to exit chat");

        RunChatLoopAsync().GetAwaiter().GetResult();
    }

    private async Task RunChatLoopAsync()
    {
        while (true)
        {
            Console.Write("chat> ");

            string? input;
            try
            {
                input = await Console.In.ReadLineAsync();
            }
            catch (OperationCanceledException)
            {
                break;
            }

            if (string.IsNullOrWhiteSpace(input))
                continue;

            if (input.StartsWith("/"))
            {
                input = input[1..];

                if (input.Equals("/exit", StringComparison.OrdinalIgnoreCase) ||
                    input.Equals("/quit", StringComparison.OrdinalIgnoreCase))
                    break;
            }

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

            QueuedAction.FlushQueuedActions();
        }
    }
}
