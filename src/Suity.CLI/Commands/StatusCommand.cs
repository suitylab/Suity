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

    public override string? DoCommand(ICliArguments args)
    {
        if (Project.Current == null)
        {
            throw new CliException("No project is currently open.");
        }

        bool showPaths = args.HasFlag("paths");

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Project Status:");
        sb.AppendLine($"  Name:           {Project.Current.ProjectName}");
        sb.AppendLine($"  Status:         {Project.Current.Status}");
        sb.AppendLine($"  GUID:           {Project.Current.ProjectGuid}");
        sb.AppendLine($"  Base Path:      {Project.Current.ProjectBasePath}");

        if (showPaths)
        {
            sb.AppendLine();
            sb.AppendLine("Directory Paths:");
            sb.AppendLine($"  Assets:         {Project.Current.AssetDirectory}");
            sb.AppendLine($"  Workspaces:     {Project.Current.WorkSpaceDirectory}");
            sb.AppendLine($"  Assemblies:     {Project.Current.AssembliesDirectory}");
            sb.AppendLine($"  Publish:        {Project.Current.PublishDirectory}");
            sb.AppendLine($"  System:         {Project.Current.SystemDirectory}");
            sb.AppendLine($"  User:           {Project.Current.UserDirectory}");
        }

        return sb.ToString().TrimEnd();
    }
}
