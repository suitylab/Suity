using Suity.Collections;

namespace Suity.Editor.Commands;

[CliCommandKey("project")]
public class ProjectCommand : CliCommand
{
    public override string Description => "Load a project from the specified folder";

    public override string Usage => "project -path <folder-path>";

    public override void DoCommand(ICliArguments args)
    {
        string? path = args.Options.GetValueOrDefault("path");

        if (string.IsNullOrWhiteSpace(path))
        {
            Console.Error.WriteLine("Error: project folder path is required.");
            Console.Error.WriteLine($"Usage: {Usage}");
            return;
        }

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            Console.WriteLine($"Created project folder : {path}");
        }

        Device.InitializeDevice(CliEditorDevice.Instance);


    }
}
