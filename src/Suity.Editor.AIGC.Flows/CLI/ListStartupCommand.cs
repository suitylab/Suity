using Suity.Editor.AIGC;
using System.Linq;

namespace Suity.Editor.CLI;

[CliCommandKey("list-startup")]
public class ListStartupCommand : CliCommand
{
    public override string Description => "List all startup chat";

    public override string Usage => "list-startup";

    public override object DoCommand(ICliArguments args)
    {
        var startups = AssetManager.Instance.GetAssets<IAigcStartup>();

        string[] assetKeys = startups
            .OfType<Asset>()
            .Where(StartupPageFilter.Instance.FilterAsset)
            .Select(o => o.AssetKey)
            .ToArray();

        return assetKeys;
    }
}
