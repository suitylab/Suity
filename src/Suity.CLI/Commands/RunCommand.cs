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

    public override object DoCommand(ICliArguments args)
    {
        if (Project.Current == null)
        {
            throw new CliException("no project is open. Use 'open' command first.");
        }

        bool debug = args.HasFlag("debug");
        bool verbose = args.HasFlag("verbose");

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Running project '{Project.Current.ProjectName}'...");

        if (debug)
        {
            sb.AppendLine("Debug mode enabled.");
        }

        RunProject(verbose, sb);

        sb.AppendLine("Project execution completed.");
        return sb.ToString().TrimEnd();
    }

    private void RunProject(bool verbose, System.Text.StringBuilder sb)
    {
        if (Project.Current == null) return;

        string publishDir = Project.Current.PublishDirectory;

        if (!Directory.Exists(publishDir))
        {
            throw new CliException("project not built. Run 'build' first.");
        }

        if (verbose)
            sb.AppendLine($"  Publish directory: {publishDir}");

        sb.AppendLine("  Starting project...");

        // TODO: Implement actual run logic
        // Find and execute the main executable in publishDir
        sb.AppendLine("  [Run not yet implemented - placeholder]");
    }
}
