using Suity.Editor.AIGC;
using System;

namespace Suity.Editor.CLI;

[CliCommandKey("set-startup")]
public class SetStartupCommand : CliCommand
{
    public override string Description => "Set startup chat";

    public override string Usage => "set-startup <asset-key>";

    public override object DoCommand(ICliArguments args)
    {
        string assetKey = args[0];
        if (string.IsNullOrEmpty(assetKey))
        {
            throw new CliException("asset-key is required");
        }

        var startupChat = AssetManager.Instance.GetAsset<IAigcStartup>(assetKey);
        if (startupChat is null)
        {
            throw new CliException("chat asset is not found");
        }

        AigcStartupWindow.Instance.SelectedStartup = startupChat;
        return "chat is set to " + assetKey;
    }
}