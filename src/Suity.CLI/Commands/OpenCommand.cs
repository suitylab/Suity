namespace Suity.Editor.Commands;

[CliCommandKey("open")]
public class OpenCommand : CliCommand
{
    public override string Description => "Open a project from the specified project file";

    public override string Usage => "open <project-file>";

    public override void DoCommand(ICliArguments args)
    {
        string? fileName = args[0];

        if (string.IsNullOrWhiteSpace(fileName))
        {
            Console.Error.WriteLine("Error: project path is required.");
            Console.Error.WriteLine($"Usage: {Usage}");
            return;
        }

        if (Directory.Exists(fileName))
        {
            string dirName = Path.GetDirectoryName(fileName)!;
            fileName = Path.Combine(dirName, $"{dirName}.suity");
        }

        if (!fileName.EndsWith(".suity", StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine("Error: project file must be a .suity file.");
            return;
        }

        if (!File.Exists(fileName))
        {
            Console.Error.WriteLine($"Error: file '{fileName}' does not exist.");
            return;
        }

        Console.WriteLine($"Opening project '{fileName}'...");

        SuityCLI.Instance.OpenProject(fileName).Wait();
    }
}
