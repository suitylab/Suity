namespace Suity.Editor.Commands;

[CliCommandKey("new")]
public class NewCommand : CliCommand
{
    public override string Description => "Create a new project in the specified folder";

    public override string Usage => "new <project-folder> [--template <template-file>]";

    public override void DoCommand(ICliArguments args)
    {
        if (Project.Current != null)
        {
            Console.Error.WriteLine("Error: a project is already open.");
            return;
        }

        string? folderPath = args[0];
        string? templateFile = args.Options["template"];

        if (string.IsNullOrWhiteSpace(folderPath))
        {
            Console.Error.WriteLine("Error: project folder path is required.");
            Console.Error.WriteLine($"Usage: {Usage}");
            return;
        }

        if (!string.IsNullOrWhiteSpace(templateFile))
        {
            if (!File.Exists(templateFile))
            {
                Console.Error.WriteLine($"Error: template file '{templateFile}' does not exist.");
                return;
            }
        }

        folderPath = Path.GetFullPath(folderPath);

        if (Directory.Exists(folderPath))
        {
            if (Directory.GetFiles(folderPath).Length > 0 || Directory.GetDirectories(folderPath).Length > 0)
            {
                Console.Error.WriteLine($"Error: folder '{folderPath}' is not empty.");
                return;
            }
        }
        else
        {
            Directory.CreateDirectory(folderPath);
        }

        string folderName = Path.GetFileName(folderPath);
        string fileName = Path.Combine(folderPath, $"{folderName}.suity");

        Console.WriteLine($"Creating project '{fileName}'...");
        if (!string.IsNullOrWhiteSpace(templateFile))
        {
            Console.WriteLine($"Using template '{templateFile}'...");
        }

        SuityCLI.Instance.OpenProject(fileName, null, templateFile).Wait();

        Console.WriteLine($"Project '{fileName}' created.");

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        var repl = new CliReplInterface();
        repl.StartInteractiveLoopAsync(cts.Token).GetAwaiter().GetResult();
    }
}
