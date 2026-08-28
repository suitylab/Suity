using Suity.Json;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Suity.Editor;

public class CliCommandRouter
{
    public const string MagicPrefix = "[SUITY_CMD]->";

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

            string? sid = commandArgs.GetOption("sid");

            var stopwatch = Stopwatch.StartNew();
            try
            {
                object result = command.DoCommand(commandArgs);
                stopwatch.Stop();

                string? output = result?.ToString();
                if (output != null)
                    Console.WriteLine(output);

                if (!string.IsNullOrWhiteSpace(sid))
                {
                    Console.WriteLine(GetMagicCode(result, sid));
                }

                //Console.ForegroundColor = ConsoleColor.DarkGray;
                //Console.WriteLine($"[Done in {stopwatch.ElapsedMilliseconds}ms]");
                //Console.ResetColor();
                return 0;
            }
            catch (CliException ex)
            {
                stopwatch.Stop();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(ex.Message);
                Console.ResetColor();

                if (!string.IsNullOrWhiteSpace(sid))
                {
                    Console.WriteLine(GetMagicCode(ex, sid));
                }

                //Console.ForegroundColor = ConsoleColor.DarkGray;
                //Console.Error.WriteLine($"[Failed in {stopwatch.ElapsedMilliseconds}ms]");
                //Console.ResetColor();
                return 1;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"Command '{commandKey}' failed: {ex.Message}");
                Console.ResetColor();

                if (!string.IsNullOrWhiteSpace(sid))
                {
                    Console.WriteLine(GetMagicCode(ex, sid));
                }

                //Console.ForegroundColor = ConsoleColor.DarkGray;
                //Console.Error.WriteLine($"[Failed in {stopwatch.ElapsedMilliseconds}ms]");
                //Console.ResetColor();
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


    public static string GetMagicCode(object? obj, string sid)
    {
        var writer = new JsonDataWriter();

        if (obj is string str)
        {
            writer.Node("@type").WriteString("Text");
            writer.Node("text").WriteString(str);
        }
        else if (obj is IDataWritable writable)
        {
            writer.Node("@type").WriteString(writable.GetType().FullName);
            writable.WriteData(writer);
        }
        else if (obj is Exception ex)
        {
            writer.Node("@type").WriteString("Exception");
            writer.Node("exception").WriteString(ex.GetType().FullName);
            writer.Node("message").WriteString(ex.Message);
            writer.Node("stackTrace").WriteString(ex.StackTrace);
        }
        else
        {
            writer.Node("@type").WriteString("Text");
            writer.Node("text").WriteString(obj?.ToString() ?? string.Empty);
        }

        writer.Node("sid").WriteString(sid);
        string json = writer.ToString(false);
        byte[] b = Encoding.UTF8.GetBytes(json);
        string base64 = Convert.ToBase64String(b);

        return MagicPrefix + base64;
    }

}
