namespace Suity.Editor.Commands;

[CliCommandKey("new")]
public class NewCommand : CliCommand
{
    public override string Description => "Create a new project in the specified folder";

    public override string Usage => "new <project-folder> [--template <template-file>]";

    public override string? DoCommand(ICliArguments args)
    {
        if (Project.Current != null)
        {
            throw new CliException("a project is already open.");
        }

        string? folderPath = args[0];
        string? templateFile = args.Options["template"];

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
        string fileName = Path.Combine(folderPath, $"{folderName}.suity");

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Creating project '{fileName}'...");
        if (!string.IsNullOrWhiteSpace(templateFile))
        {
            sb.AppendLine($"Using template '{templateFile}'...");
        }

        SuityCLI.Instance.OpenProject(fileName, null, templateFile).Wait();

        sb.AppendLine($"Project '{fileName}' created.");

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        var repl = new CliReplInterface();
        repl.StartInteractiveLoopAsync(cts.Token).GetAwaiter().GetResult();

        return sb.ToString().TrimEnd();
    }
}
