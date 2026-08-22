namespace Suity.CLI.Commands;

[ConsoleCommandKey("help")]
public class HelpCommand : ConsoleCommand
{
    public override string Description => "Show all available commands";

    public override string Usage => "help [command]";

    public override void DoCommand(ConsoleArguments args)
    {
        var router = ConsoleCommandRouter.Instance;

        if (args.Count > 0)
        {
            string? specificCommand = args[0];
            if (specificCommand != null && router.Commands.TryGetValue(specificCommand, out var command))
            {
                Console.WriteLine($"{specificCommand} - {command.Description}");
                if (!string.IsNullOrEmpty(command.Usage))
                {
                    Console.WriteLine($"Usage: {command.Usage}");
                }
                return;
            }

            Console.Error.WriteLine($"Unknown command: '{specificCommand}'");
            Console.Error.WriteLine();
        }

        Console.WriteLine("Available commands:");
        Console.WriteLine();

        var sorted = router.Commands.OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase);
        int maxKeyLength = sorted.Max(kvp => kvp.Key.Length);

        foreach (var (key, cmd) in sorted)
        {
            Console.WriteLine($"  {key.PadRight(maxKeyLength + 2)}{cmd.Description}");
        }

        Console.WriteLine();
        Console.WriteLine("Run '<command> --help' for command-specific help.");
    }
}
