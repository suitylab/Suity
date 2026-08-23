using System;

namespace Suity.Editor.AIGC.LLm.CLI;

[CliCommandKey("list-chat")]
public class ListChatCommand : CliCommand
{
    public override string Description => "List all chat";

    public override string Usage => "list-chat";

    public override void DoCommand(ICliArguments args)
    {
        var providers = AssetManager.Instance.GetAssets<ILLmChatProvider>();
        foreach (var provider in providers)
        {
            if (provider is Asset asset) 
            {
                Console.WriteLine(asset.AssetKey);
            }
        }
    }
}
