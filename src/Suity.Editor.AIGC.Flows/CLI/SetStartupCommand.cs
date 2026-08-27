using Suity.Editor.AIGC;
using System;

namespace Suity.Editor.CLI;

[CliCommandKey("set-startup")]
public class SetStartupCommand : CliCommand
{
    public override string Description => "Set startup chat";

    public override string Usage => "set-startup <asset-key>";

    public override void DoCommand(ICliArguments args)
    {
        string assetKey = args[0];
        if (string.IsNullOrEmpty(assetKey))
        {
            Console.WriteLine("asset-key is required");
            return;
        }

        var startupChat = AssetManager.Instance.GetAsset<IAigcStartup>(assetKey);
        if (startupChat is null)
        {
            Console.WriteLine("chat asset is not found");
            return;
        }

        AigcStartupWindow.Instance.SelectChat = startupChat;
        Console.WriteLine("chat is set to " + assetKey);
    }
}