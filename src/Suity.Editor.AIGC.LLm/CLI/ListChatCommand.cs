using System.Linq;

namespace Suity.Editor.AIGC.LLm.CLI;

[CliCommandKey("list-chat")]
public class ListChatCommand : CliCommand
{
    public override string Description => "List all chat";

    public override string Usage => "list-chat";

    public override object DoCommand(ICliArguments args)
    {
        var providers = AssetManager.Instance.GetAssets<ILLmChatProvider>();

        string[] assetKeys = providers
            .OfType<Asset>()
            .Select(o => o.AssetKey)
            .ToArray();

        return new CliStringArray { Strings = assetKeys };
    }
}
