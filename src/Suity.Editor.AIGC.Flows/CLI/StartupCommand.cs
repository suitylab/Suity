using Suity.Editor.AIGC;
using System;
using System.Text;

namespace Suity.Editor.CLI;

[CliCommandKey("startup")]
public class StartupCommand : CliCommand
{
    public override string Description => "Start a new chat";

    public override string Usage => "startup <user-input> [--base64] --workspace <name>";

    public override object DoCommand(ICliArguments args)
    {
        string userInput = args[0];
        if (string.IsNullOrWhiteSpace(userInput))
        {
            throw new CliException("User input cannot be empty");
        }

        if (args.HasFlag("base64"))
        {
            byte[] bytes = Convert.FromBase64String(userInput);
            userInput = Encoding.UTF8.GetString(bytes);
        }

        string workspaceName = args.GetOption("workspace");
        if (string.IsNullOrWhiteSpace(workspaceName))
        {
            throw new CliException("Workspace name cannot be empty");
        }

        AigcStartupWindow.Instance.AutoSelectDefaultStartup();
        IAigcStartup startup = AigcStartupWindow.Instance.SelectedStartup;
        if (startup is null)
        {
            throw new CliException("No startup selected");
        }

        if (!startup.IsStartup)
        {
            throw new CliException("Selected startup is not a startup assistant");
        }

        startup.HandleStartup(userInput, workspaceName).GetAwaiter().OnCompleted(() => 
        {
            //TODO: Send notification.
        });

        ChatReplInterface.RunChatLoopAsync().GetAwaiter().GetResult();

        return null;
    }
}
