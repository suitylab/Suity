namespace Suity.Editor;

public class CliArguments : ICliArguments
{
    private readonly List<string> _positionalArgs = new();
    private readonly Dictionary<string, string> _options = new(StringComparer.OrdinalIgnoreCase);

    public string CommandKey { get; }
    public string[] RawArgs { get; }
    public IReadOnlyDictionary<string, string> Options => _options;
    public int Count => _positionalArgs.Count;

    public CliArguments(string commandKey, string[] args)
    {
        CommandKey = commandKey;
        RawArgs = args;
        Parse(args);
    }

    private void Parse(string[] args)
    {
        int i = 0;
        while (i < args.Length)
        {
            string arg = args[i];

            if (arg.StartsWith("--"))
            {
                // --key=value
                int eqIndex = arg.IndexOf('=');
                if (eqIndex > 0)
                {
                    string key = arg[2..eqIndex];
                    string value = arg[(eqIndex + 1)..];
                    _options[key] = value;
                }
                // --key value
                else if (i + 1 < args.Length && !args[i + 1].StartsWith('-'))
                {
                    _options[arg[2..]] = args[i + 1];
                    i++;
                }
                // --flag (boolean)
                else
                {
                    _options[arg[2..]] = "true";
                }
            }
            else if (arg.StartsWith('-') && arg.Length > 1)
            {
                // -k=value
                int eqIndex = arg.IndexOf('=');
                if (eqIndex > 0)
                {
                    string key = arg[1..eqIndex];
                    string value = arg[(eqIndex + 1)..];
                    _options[key] = value;
                }
                // -k value
                else if (i + 1 < args.Length && !args[i + 1].StartsWith('-'))
                {
                    _options[arg[1..]] = args[i + 1];
                    i++;
                }
                // -v (boolean)
                else
                {
                    _options[arg[1..]] = "true";
                }
            }
            else
            {
                _positionalArgs.Add(arg);
            }

            i++;
        }
    }

    public string? GetOption(string key, string? defaultValue = null)
    {
        return _options.TryGetValue(key, out var value) ? value : defaultValue;
    }

    public bool HasFlag(string flag)
    {
        return _options.TryGetValue(flag, out var value)
            && (value.Equals("true", StringComparison.OrdinalIgnoreCase) || value == "1");
    }

    public string? this[int index]
    {
        get
        {
            if (index < 0 || index >= _positionalArgs.Count)
                return null;
            return _positionalArgs[index];
        }
    }
}
