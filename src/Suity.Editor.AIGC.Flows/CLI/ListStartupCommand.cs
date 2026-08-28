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
        var sb = new System.Text.StringBuilder();
        var startups = AssetManager.Instance.GetAssets<IAigcStartup>();
        foreach (var startup in startups)
        {
            if (startup is Asset asset && StartupPageFilter.Instance.FilterAsset(asset))
            {
                sb.AppendLine(asset.AssetKey);
            }
        }
        return sb.Length > 0 ? sb.ToString().TrimEnd() : null;
    }
}
