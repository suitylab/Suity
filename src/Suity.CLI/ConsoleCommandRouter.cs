using System.Reflection;

namespace Suity.CLI;

public class ConsoleCommandRouter
{
    private static readonly Lazy<ConsoleCommandRouter> _instance = new(() => new ConsoleCommandRouter());
    public static ConsoleCommandRouter Instance => _instance.Value;

    private readonly Dictionary<string, ConsoleCommand> _commands = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, ConsoleCommand> Commands => _commands;

    private ConsoleCommandRouter()
    {
        RegisterCommandsFromAssembly(Assembly.GetEntryAssembly()!);
        RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
    }

    private void RegisterCommandsFromAssembly(Assembly assembly)
    {
        var commandTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(ConsoleCommand)));

        foreach (var type in commandTypes)
        {
            var attr = type.GetCustomAttribute<ConsoleCommandKeyAttribute>();
            if (attr == null) continue;

            if (Activator.CreateInstance(type) is ConsoleCommand command)
            {
                _commands[attr.Key] = command;
            }
        }
    }

    public int Route(string[] args)
    {
        if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
        {
            ShowHelp();
            return 0;
        }

        string commandKey = args[0];
        var commandArgs = new ConsoleArguments(commandKey, args.Length > 1 ? args[1..] : Array.Empty<string>());

        if (_commands.TryGetValue(commandKey, out var command))
        {
            try
            {
                command.DoCommand(commandArgs);
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Command '{commandKey}' failed: {ex.Message}");
                return 1;
            }
        }

        Console.Error.WriteLine($"Unknown command: '{commandKey}'");
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
