using Suity.Editor.AIGC;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using System;
using System.Text;

namespace Suity.Editor.CLI;

[CliCommandKey("startup")]
public class StartupCommand : CliCommand
{
    public override string Description => "Start a new chat";

    public override string Usage => "startup <user-input> [--base64] --workspace <name> [--auto-new]";

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

        var workSpace = WorkSpaceManager.Current.GetWorkSpace(workspaceName);
        if (workSpace is null)
        {
            if (args.HasFlag("auto-new"))
            {
                if (!NamingVerifier.VerifyIdentifier(workspaceName))
                {
                    throw new CliException($"Invalid workspace name: {workspaceName}");
                }

                try
                {
                    workSpace = WorkSpaceManager.Current.CreateWorkSpace(workspaceName);
                }
                catch (Exception ex)
                {
                    throw new CliException($"Failed to create workspace: {ex.Message}");
                }
            }
            else
            {
                throw new CliException("Workspace not found.");
            }
        }

        startup.HandleStartup(userInput, workSpace).GetAwaiter().OnCompleted(() => 
        {
            //TODO: Send notification.
        });

        ChatReplInterface.RunChatLoopAsync().GetAwaiter().GetResult();

        return null;
    }
}
