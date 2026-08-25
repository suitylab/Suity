using System;

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
    }
}
