namespace Suity.Editor.Commands;

[CliCommandKey("exit")]
public class ExitCommand : CliCommand
{
    public override string Description => "Close the project and exit the application";

    public override string Usage => "exit";

    public override object DoCommand(ICliArguments args)
    {
        if (Project.Current != null)
        {
            SuityCliApp.Instance.CloseProject();
            Console.WriteLine("Exiting...");
            Environment.Exit(0);
            return "Project closed.";
        }

        Console.WriteLine("Exiting...");
        Environment.Exit(0);
        return "Exiting...";
    }
}
