using System;

namespace Suity.Editor.AIGC.LLm.CLI;

[CliCommandKey("set-chat")]
public class SetChatCommand : CliCommand
{
    public override string Description => "Set current chat";

    public override string Usage => "set-chat <asset-key>";

    public override string? DoCommand(ICliArguments args)
    {
        string assetKey = args[0];
        if (string.IsNullOrEmpty(assetKey))
        {
            throw new CliException("asset-key is required");
        }

        var provider = AssetManager.Instance.GetAsset<ILLmChatProvider>(assetKey);
        if (provider is null)
        {
            throw new CliException("chat asset is not found");
        }

        AigcChatToolWindow.Instance.SelectedChatProvider = provider;
        return "chat is set to " + assetKey;
    }
}
