using System.Reflection;

namespace Suity.Editor;

public class CliCommandRouter
{
    private static readonly Lazy<CliCommandRouter> _instance = new(() => new CliCommandRouter());
    public static CliCommandRouter Instance => _instance.Value;

    private readonly Dictionary<string, CliCommand> _commands = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, CliCommand> Commands => _commands;

    private CliCommandRouter()
    {
        RegisterCommandsFromAssembly(Assembly.GetEntryAssembly()!);
        RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public void RegisterCommandsFromAssembly(Assembly assembly)
    {
        var commandTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(CliCommand)));

        foreach (var type in commandTypes)
        {
            var attr = type.GetCustomAttribute<CliCommandKeyAttribute>();
            if (attr == null) continue;

            if (Activator.CreateInstance(type) is CliCommand command)
            {
                _commands[attr.Key] = command;
            }
        }
    }

    public int Route(params string[] args)
    {
        if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
        {
            ShowHelp();
            return 0;
        }

        string commandKey = args[0];
        var commandArgs = new CliArguments(commandKey, args.Length > 1 ? args[1..] : Array.Empty<string>());

        if (_commands.TryGetValue(commandKey, out var command))
        {
            if (commandArgs.HasFlag("help"))
            {
                command.ShowHelp();
                return 0;
            }

            try
            {
                command.DoCommand(commandArgs);
                return 0;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"Command '{commandKey}' failed: {ex.Message}");
                Console.ResetColor();
                return 1;
            }
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Error.WriteLine($"Unknown command: '{commandKey}'");
        Console.ResetColor();
        Console.Error.WriteLine();
        ShowHelp();
        return 1;
    }

    private void ShowHelp()
    {
        Console.WriteLine("Available commands:");
        Console.WriteLine();

        var sorted = _commands.OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase);
        int maxKeyLength = sorted.Max(kvp => kvp.Key.Length);

        foreach (var (key, command) in sorted)
        {
            System.Console.WriteLine($"  {key.PadRight(maxKeyLength + 2)}{command.Description}");
        }

        System.Console.WriteLine();
        System.Console.WriteLine("Run '<command> --help' for command-specific help.");
    }
}
