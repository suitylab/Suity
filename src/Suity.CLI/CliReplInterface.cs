using System.Text;

namespace Suity.Editor;

public class CliReplInterface
{
    private readonly CliCommandRouter _router = CliCommandRouter.Instance;

    public async Task StartInteractiveLoopAsync(CancellationToken token)
    {
        Console.WriteLine("CLI Environment Ready. Type 'help' for available commands, 'exit' to quit.");

        while (!token.IsCancellationRequested)
        {
            Console.Write("> ");

            string? input;
            try
            {
                input = await Console.In.ReadLineAsync(token);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            if (string.IsNullOrWhiteSpace(input))
                continue;

            try
            {
                var args = ParseInput(input);
                _router.Route(args);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
            }

            QueuedAction.FlushQueuedActions();
        }
    }

    private static string[] ParseInput(string input)
    {
        var tokens = new List<string>();
        var current = new StringBuilder();
        bool inQuote = false;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (inQuote)
            {
                if (c == '\\' && i + 1 < input.Length && input[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else if (c == '"')
                {
                    inQuote = false;
                }
                else
                {
                    current.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuote = true;
                }
                else if (char.IsWhiteSpace(c))
                {
                    if (current.Length > 0)
                    {
                        tokens.Add(current.ToString());
                        current.Clear();
                    }
                }
                else
                {
                    current.Append(c);
                }
            }
        }

        if (current.Length > 0)
            tokens.Add(current.ToString());

        return tokens.ToArray();
    }
}
