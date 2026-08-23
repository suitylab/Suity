using System;

namespace Suity.Editor.AIGC.LLm.CLI;

[CliCommandKey("get-chat")]
public class GetChatCommand : CliCommand
{
    public override string Description => "Get current chat";

    public override string Usage => "get-chat";

    public override void DoCommand(ICliArguments args)
    {
        Guid assetId = AigcChatToolWindow.Instance.SelectedChatAssetId;
        var asset = AssetManager.Instance.GetAsset(assetId);
        if (asset != null)
        {
            Console.WriteLine(asset.AssetKey);
        }
        else 
        {
            Console.WriteLine("No chat selected");
        }
    }
}
