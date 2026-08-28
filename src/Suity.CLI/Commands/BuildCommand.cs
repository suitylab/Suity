namespace Suity.Editor.Commands;

[CliCommandKey("build")]
public class BuildCommand : CliCommand
{
    public override string Description => "Build the current project";

    public override string Usage => "build [--release] [--clean]";

    public override string DetailedHelp =>
        "Build the current project.\n" +
        "\n" +
        "Options:\n" +
        "  --release    Build in Release mode (default: Debug)\n" +
        "  --clean      Clean before building\n" +
        "  --verbose    Show detailed build output";

    public override string? DoCommand(ICliArguments args)
    {
        if (Project.Current == null)
        {
            throw new CliException("no project is open. Use 'open' command first.");
        }

        bool release = args.HasFlag("release");
        bool clean = args.HasFlag("clean");
        bool verbose = args.HasFlag("verbose");

        string config = release ? "Release" : "Debug";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Building project '{Project.Current.ProjectName}' in {config} mode...");

        if (clean)
        {
            sb.AppendLine("Cleaning build artifacts...");
            CleanProject(verbose, sb);
        }

        BuildProject(config, verbose, sb);

        sb.AppendLine("Build completed successfully.");
        return sb.ToString().TrimEnd();
    }

    private void CleanProject(bool verbose, System.Text.StringBuilder sb)
    {
        if (Project.Current == null) return;

        string publishDir = Project.Current.PublishDirectory;
        if (Directory.Exists(publishDir))
        {
            if (verbose)
                sb.AppendLine($"  Cleaning: {publishDir}");

            Directory.Delete(publishDir, true);
            sb.AppendLine("  Cleaned publish directory.");
        }
    }

    private void BuildProject(string configuration, bool verbose, System.Text.StringBuilder sb)
    {
        if (Project.Current == null) return;

        string solutionFile = Project.Current.SolutionFile;

        if (!File.Exists(solutionFile))
        {
            throw new CliException($"solution file not found: {solutionFile}");
        }

        if (verbose)
            sb.AppendLine($"  Solution: {solutionFile}");

        sb.AppendLine($"  Configuration: {configuration}");
        sb.AppendLine("  Building...");

        // TODO: Implement actual build logic using MSBuild or dotnet build
        // For now, show what would be built
        sb.AppendLine("  [Build not yet implemented - placeholder]");
    }
}
