namespace Suity.Editor.Commands;

[CliCommandKey("new")]
public class NewCommand : CliCommand
{
    public override string Description => "Create a new project in the specified folder";

    public override string Usage => "new <project-folder> [--template <template-file>]";

    public override object DoCommand(ICliArguments args)
    {
        if (Project.Current != null)
        {
            throw new CliException("a project is already open.");
        }

        string? folderPath = args[0];
        string? templateFile = args.GetOption("template");

        if (string.IsNullOrWhiteSpace(folderPath))
        {
            throw new CliException("project folder path is required.");
        }

        if (!string.IsNullOrWhiteSpace(templateFile))
        {
            if (!File.Exists(templateFile))
            {
                throw new CliException($"template file '{templateFile}' does not exist.");
            }
        }

        folderPath = Path.GetFullPath(folderPath);

        if (Directory.Exists(folderPath))
        {
            if (Directory.GetFiles(folderPath).Length > 0 || Directory.GetDirectories(folderPath).Length > 0)
            {
                throw new CliException($"folder '{folderPath}' is not empty.");
            }
        }
        else
        {
            Directory.CreateDirectory(folderPath);
        }

        string folderName = Path.GetFileName(folderPath);
        string fileName = Path.Combine(folderPath, $"{folderName}.sunity");

        Console.WriteLine($"Creating project '{fileName}'...");
        if (!string.IsNullOrWhiteSpace(templateFile))
        {
            Console.WriteLine($"Using template '{templateFile}'...");
        }

        SuityCLI.Instance.OpenProject(fileName, null, templateFile).Wait();

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        Console.WriteLine($"Project '{fileName}' created.");

        if (args.GetOption("sid") is { } sid && !string.IsNullOrWhiteSpace(sid))
        {
            CliMagicLine.OutputMagicLine($"Project '{fileName}' created.", sid);
        }

        var repl = new CliReplInterface();
        repl.StartInteractiveLoopAsync(cts.Token).GetAwaiter().GetResult();

        return null;
    }
}
