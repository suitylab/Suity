namespace Suity.Editor.Commands;

[CliCommandKey("build")]
[NotAvailable]
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

    public override object DoCommand(ICliArguments args)
    {
        if (Project.Current == null)
        {
            throw new CliException("no project is open. Use 'open' command first.");
        }

        bool release = args.HasFlag("release");
        bool clean = args.HasFlag("clean");
        bool verbose = args.HasFlag("verbose");

        string config = release ? "Release" : "Debug";

        Console.WriteLine($"Building project '{Project.Current.ProjectName}' in {config} mode...");

        if (clean)
        {
            Console.WriteLine("Cleaning build artifacts...");
            CleanProject(verbose);
        }

        BuildProject(config, verbose);

        return "Build completed successfully.";
    }

    private void CleanProject(bool verbose)
    {
        if (Project.Current == null) return;

        string publishDir = Project.Current.PublishDirectory;
        if (Directory.Exists(publishDir))
        {
            if (verbose)
                Console.WriteLine($"  Cleaning: {publishDir}");

            Directory.Delete(publishDir, true);
            Console.WriteLine("  Cleaned publish directory.");
        }
    }

    private void BuildProject(string configuration, bool verbose)
    {
        if (Project.Current == null) return;

        string solutionFile = Project.Current.SolutionFile;

        if (!File.Exists(solutionFile))
        {
            throw new CliException($"solution file not found: {solutionFile}");
        }

        if (verbose)
            Console.WriteLine($"  Solution: {solutionFile}");

        Console.WriteLine($"  Configuration: {configuration}");
        Console.WriteLine("  Building...");

        // TODO: Implement actual build logic using MSBuild or dotnet build
        // For now, show what would be built
        Console.WriteLine("  [Build not yet implemented - placeholder]");
    }
}
