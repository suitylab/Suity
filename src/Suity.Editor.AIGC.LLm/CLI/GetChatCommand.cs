using System;

namespace Suity.Editor.AIGC.LLm.CLI;

[CliCommandKey("get-chat")]
public class GetChatCommand : CliCommand
{
    public override string Description => "Get current chat";

    public override string Usage => "get-chat";

    public override object DoCommand(ICliArguments args)
    {
        Guid assetId = AigcChatToolWindow.Instance.SelectedChatAssetId;
        var asset = AssetManager.Instance.GetAsset(assetId);
        if (asset != null)
        {
            return asset.AssetKey;
        }
        else 
        {
            throw new CliException("No chat selected");
        }
    }
}
