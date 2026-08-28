namespace Suity.Editor.Commands;

[CliCommandKey("help")]
public class HelpCommand : CliCommand
{
    public override string Description => "Show all available commands";

    public override string Usage => "help [command]";

    public override string? DoCommand(ICliArguments args)
    {
        var router = CliCommandRouter.Instance;

        if (args.Count > 0)
        {
            string? specificCommand = args[0];
            if (specificCommand != null && router.Commands.TryGetValue(specificCommand, out var command))
            {
                var sb2 = new System.Text.StringBuilder();
                sb2.AppendLine(command.Description);
                if (!string.IsNullOrEmpty(command.Usage))
                {
                    sb2.AppendLine();
                    sb2.AppendLine($"Usage: {command.Usage}");
                }
                if (!string.IsNullOrEmpty(command.DetailedHelp))
                {
                    sb2.AppendLine();
                    sb2.AppendLine(command.DetailedHelp);
                }
                return sb2.ToString().TrimEnd();
            }

            throw new CliException($"Unknown command: '{specificCommand}'");
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Available commands:");

        var sorted = router.Commands.OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase);
        int maxKeyLength = sorted.Max(kvp => kvp.Key.Length);

        foreach (var (key, cmd) in sorted)
        {
            sb.AppendLine($"  {key.PadRight(maxKeyLength + 2)}{cmd.Description}");
        }

        sb.AppendLine();
        sb.AppendLine("Run '<command> --help' for command-specific help.");
        return sb.ToString().TrimEnd();
    }
}
