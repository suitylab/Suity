namespace Suity.Editor.Commands;

[CliCommandKey("project")]
public class ProjectCommand : CliCommand
{
    public override string Description => "Load a project from the specified folder";

    public override string Usage => "project <folder-path>";

    public override void DoCommand(ICliArguments args)
    {
        if (args.Count < 1 || string.IsNullOrWhiteSpace(args[0]))
        {
            Console.Error.WriteLine("Error: project folder path is required.");
            Console.Error.WriteLine($"Usage: {Usage}");
            return;
        }

        string folderPath = args[0]!;

        // TODO: implement project loading logic
    }
}
