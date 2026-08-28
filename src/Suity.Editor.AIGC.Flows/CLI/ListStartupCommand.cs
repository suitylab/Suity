using Suity.Editor.AIGC;
using System;

namespace Suity.Editor.CLI;

[CliCommandKey("list-startup")]
public class ListStartupCommand : CliCommand
{
    public override string Description => "List all startup chat";

    public override string Usage => "list-startup";

    public override object DoCommand(ICliArguments args)
    {
        var startups = AssetManager.Instance.GetAssets<IAigcStartup>();
        foreach (var startup in startups)
        {
            if (startup is Asset asset && StartupPageFilter.Instance.FilterAsset(asset))
            {
                Console.WriteLine(asset.AssetKey);
            }
        }
        return null;
    }
}
