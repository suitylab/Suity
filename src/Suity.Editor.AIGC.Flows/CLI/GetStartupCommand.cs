using Suity.Editor.AIGC;
using System;

namespace Suity.Editor.CLI;

[CliCommandKey("get-startup")]
public class GetStartupCommand : CliCommand
{
    public override string Description => "Get startup chat";

    public override string Usage => "get-startup";

    public override void DoCommand(ICliArguments args)
    {
        AigcStartupWindow.Instance.AutoSelectDefaultStartup();

        Guid assetId = AigcStartupWindow.Instance.SelectedChatAssetId;
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
