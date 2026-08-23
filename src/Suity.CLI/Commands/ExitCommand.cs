namespace Suity.Editor.Commands;

[CliCommandKey("exit")]
public class ExitCommand : CliCommand
{
    public override string Description => "Close the project and exit the application";

    public override string Usage => "exit";

    public override void DoCommand(ICliArguments args)
    {
        if (Project.Current != null)
        {
            Console.WriteLine("Closing project...");
            SuityCLI.Instance.CloseProject();
            Console.WriteLine("Project closed.");
        }

        Console.WriteLine("Exiting...");
        Environment.Exit(0);
    }
}
