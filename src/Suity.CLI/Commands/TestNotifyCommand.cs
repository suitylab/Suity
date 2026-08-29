namespace Suity.Editor.Commands;

[CliCommandKey("test-notify")]
public class TestNotifyCommand : CliCommand
{
    public override string Description => "Test notify command";
    public override string Usage => "test-notify [message]";
    public override object DoCommand(ICliArguments args)
    {
        int count = 10;

        for (int i = 0; i < count; i++)
        {
            int num = i + 1;

            CliMagicLine.OutputMagicLine($"Notification {num}/{count}");
            Task.Delay(1000).Wait();
        }

        return "Finished";
    }
}
