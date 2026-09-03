using Suity.Editor.Services;

namespace Suity.Editor.Commands;

[CliCommandKey("status")]
public class StatusCommand : CliCommand
{
    public override string Description => "Show current project status";

    public override string Usage => "status";

    public override object DoCommand(ICliArguments args)
    {
        EditorServices.LogStatus();

        return null;
    }
}
