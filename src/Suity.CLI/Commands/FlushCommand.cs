namespace Suity.Editor.Commands;

[CliCommandKey("flush")]
public class FlushCommand : CliCommand
{
    public override string Description => "Flush the queued actions";

    public override string Usage => "flush";

    public override object DoCommand(ICliArguments args)
    {
        QueuedAction.FlushQueuedActions();

        return null;
    }
}
