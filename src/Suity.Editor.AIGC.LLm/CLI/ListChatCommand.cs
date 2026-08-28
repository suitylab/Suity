using System;

namespace Suity.Editor.AIGC.LLm.CLI;

[CliCommandKey("list-chat")]
public class ListChatCommand : CliCommand
{
    public override string Description => "List all chat";

    public override string Usage => "list-chat";

    public override string? DoCommand(ICliArguments args)
    {
        var sb = new System.Text.StringBuilder();
        var providers = AssetManager.Instance.GetAssets<ILLmChatProvider>();
        foreach (var provider in providers)
        {
            if (provider is Asset asset) 
            {
                sb.AppendLine(asset.AssetKey);
            }
        }
        return sb.Length > 0 ? sb.ToString().TrimEnd() : null;
    }
}
