namespace Suity.Editor.Commands;

public class TestNotifyCommand : CliCommand
{
    public override string Description => "Test notify command";
    public override string Usage => "testnotify [message]";
    public override object DoCommand(ICliArguments args)
    {
        for (int i = 0; i < 10; i++)
        {
            int num = i + 1;

        }

        return "Finished";
    }
}
