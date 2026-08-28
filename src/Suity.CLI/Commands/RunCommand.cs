namespace Suity.Editor.Commands;

[CliCommandKey("run")]
[NotAvailable]
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

    public override object DoCommand(ICliArguments args)
    {
        if (Project.Current == null)
        {
            throw new CliException("no project is open. Use 'open' command first.");
        }

        bool debug = args.HasFlag("debug");
        bool verbose = args.HasFlag("verbose");

        Console.WriteLine($"Running project '{Project.Current.ProjectName}'...");

        if (debug)
        {
            Console.WriteLine("Debug mode enabled.");
        }

        RunProject(verbose);

        return "Project execution completed.";
    }

    private void RunProject(bool verbose)
    {
        if (Project.Current == null) return;

        string publishDir = Project.Current.PublishDirectory;

        if (!Directory.Exists(publishDir))
        {
            throw new CliException("project not built. Run 'build' first.");
        }

        if (verbose)
            Console.WriteLine($"  Publish directory: {publishDir}");

        Console.WriteLine("  Starting project...");

        // TODO: Implement actual run logic
        // Find and execute the main executable in publishDir
        Console.WriteLine("  [Run not yet implemented - placeholder]");
    }
}
