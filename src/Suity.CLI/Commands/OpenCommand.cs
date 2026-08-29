namespace Suity.Editor.Commands;

[CliCommandKey("open")]
public class OpenCommand : CliCommand
{
    public override string Description => "Open a project from the specified project file";

    public override string Usage => "open <project-file>";

    public override object DoCommand(ICliArguments args)
    {
        if (Project.Current != null)
        {
            throw new CliException("a project is already open.");
        }

        string? fileName = args[0];

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new CliException("project path is required.");
        }

        if (Directory.Exists(fileName))
        {
            string dirName = Path.GetFileName(fileName)!;
            fileName = Path.Combine(fileName, $"{dirName}.suity");
        }

        if (!fileName.EndsWith(".suity", StringComparison.OrdinalIgnoreCase))
        {
            throw new CliException("project file must be a .suity file.");
        }

        if (!File.Exists(fileName))
        {
            throw new CliException($"file '{fileName}' does not exist.");
        }

        SuityCLI.Instance.OpenProject(fileName).Wait();

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        Console.WriteLine($"Project '{fileName}' opened.");

        if (args.GetSessionId() is { } sid && !string.IsNullOrWhiteSpace(sid))
        {
            CliMagicLine.OutputMagicLine($"Project '{fileName}' opened.", sid);
        }

        var repl = new CliReplInterface();
        repl.StartInteractiveLoopAsync(cts.Token).GetAwaiter().GetResult();

        return null;
    }
}
