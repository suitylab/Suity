namespace Suity.Editor.Commands;

[CliCommandKey("run")]
public class RunCommand : CliCommand
{
    public override string Description => "Run the current project";

    public override string Usage => "run [--debug]";

    public override string DetailedHelp =>
        "Run the current project.\n" +
        "\n" +
        "Options:\n" +
        "  --debug      Run with debugger attached\n" +
        "  --verbose    Show detailed output";

    public override void DoCommand(ICliArguments args)
    {
        if (Project.Current == null)
        {
            Console.Error.WriteLine("Error: no project is open. Use 'open' command first.");
            return;
        }

        bool debug = args.HasFlag("debug");
        bool verbose = args.HasFlag("verbose");

        Console.WriteLine($"Running project '{Project.Current.ProjectName}'...");

        if (debug)
        {
            Console.WriteLine("Debug mode enabled.");
        }

        RunProject(verbose);

        Console.WriteLine("Project execution completed.");
    }

    private void RunProject(bool verbose)
    {
        if (Project.Current == null) return;

        string publishDir = Project.Current.PublishDirectory;

        if (!Directory.Exists(publishDir))
        {
            Console.Error.WriteLine("  Error: project not built. Run 'build' first.");
            return;
        }

        if (verbose)
            Console.WriteLine($"  Publish directory: {publishDir}");

        Console.WriteLine("  Starting project...");

        // TODO: Implement actual run logic
        // Find and execute the main executable in publishDir
        Console.WriteLine("  [Run not yet implemented - placeholder]");
    }
}
