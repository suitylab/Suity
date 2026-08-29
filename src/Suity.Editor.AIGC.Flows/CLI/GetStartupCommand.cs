using Suity.Editor.AIGC;
using System;

namespace Suity.Editor.CLI;

[CliCommandKey("get-startup")]
public class GetStartupCommand : CliCommand
{
    public override string Description => "Get startup chat";

    public override string Usage => "get-startup";

    public override object DoCommand(ICliArguments args)
    {
        AigcStartupWindow.Instance.AutoSelectDefaultStartup();

        Guid assetId = AigcStartupWindow.Instance.SelectedStartupAssetId;
        var asset = AssetManager.Instance.GetAsset(assetId);
        if (asset != null)
        {
            return asset.AssetKey;
        }
        else
        {
            throw new CliException("No startup asset selected");
        }
    }
}
