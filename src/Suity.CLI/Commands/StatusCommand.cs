namespace Suity.Editor.Commands;

[CliCommandKey("status")]
public class StatusCommand : CliCommand
{
    public override string Description => "Show current project status";

    public override string Usage => "status [--paths]";

    public override string DetailedHelp =>
        "Show current project status.\n" +
        "\n" +
        "Options:\n" +
        "  --paths      Show project directory paths";

    public override void DoCommand(ICliArguments args)
    {
        if (Project.Current == null)
        {
            Console.WriteLine("No project is currently open.");
            return;
        }

        bool showPaths = args.HasFlag("paths");

        Console.WriteLine("Project Status:");
        Console.WriteLine($"  Name:           {Project.Current.ProjectName}");
        Console.WriteLine($"  Status:         {Project.Current.Status}");
        Console.WriteLine($"  GUID:           {Project.Current.ProjectGuid}");
        Console.WriteLine($"  Base Path:      {Project.Current.ProjectBasePath}");

        if (showPaths)
        {
            Console.WriteLine();
            Console.WriteLine("Directory Paths:");
            Console.WriteLine($"  Assets:         {Project.Current.AssetDirectory}");
            Console.WriteLine($"  Workspaces:     {Project.Current.WorkSpaceDirectory}");
            Console.WriteLine($"  Assemblies:     {Project.Current.AssembliesDirectory}");
            Console.WriteLine($"  Publish:        {Project.Current.PublishDirectory}");
            Console.WriteLine($"  System:         {Project.Current.SystemDirectory}");
            Console.WriteLine($"  User:           {Project.Current.UserDirectory}");
        }
    }
}
