using System;
using System.Text;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.LLm.CLI;

[CliCommandKey("start-chat")]
public class StartChatCommand : CliCommand
{
    public override string Description => "Start chat";

    public override string Usage => "start-chat (<asset-key>)";

    public override object DoCommand(ICliArguments args)
    {
        string assetKey = args[0];
        if (!string.IsNullOrEmpty(assetKey))
        {
            var provider = AssetManager.Instance.GetAsset<ILLmChatProvider>(assetKey);
            if (provider is null)
            {
                throw new CliException("chat asset is not found");
            }

            AigcChatToolWindow.Instance.SelectedChatProvider = provider;
        }

        if (AigcChatToolWindow.Instance.SelectedChatProvider is null)
        {
            throw new CliException("chat is not set");
        }

        AigcChatToolWindow.Instance.HandleStartChat().GetAwaiter().OnCompleted(() => 
        {
            //TODO: Send notification.
        });

        ChatReplInterface.RunChatLoopAsync().GetAwaiter().GetResult();

        return null;
    }
}
