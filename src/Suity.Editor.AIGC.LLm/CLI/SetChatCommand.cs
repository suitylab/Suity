using System;

namespace Suity.Editor.AIGC.LLm.CLI;

[CliCommandKey("set-chat")]
public class SetChatCommand : CliCommand
{
    public override string Description => "Set current chat";

    public override string Usage => "set-chat <asset-key>";

    public override void DoCommand(ICliArguments args)
    {
        string assetKey = args[0];
        if (string.IsNullOrEmpty(assetKey))
        {
            Console.WriteLine("asset-key is required");
            return;
        }

        var provider = AssetManager.Instance.GetAsset<ILLmChatProvider>(assetKey);
        if (provider is null)
        {
            Console.WriteLine("chat asset is not found");
            return;
        }

        AigcChatToolWindow.Instance.SelectedChatProvider = provider;

        Console.WriteLine("chat is set to " + assetKey);
    }
}
