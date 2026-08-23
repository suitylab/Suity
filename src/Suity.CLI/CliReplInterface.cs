namespace Suity.Editor;

public class CliReplInterface
{
    private readonly CliCommandRouter _router = CliCommandRouter.Instance;

    public async Task StartInteractiveLoopAsync(CancellationToken token)
    {
        Console.WriteLine("CLI Environment Ready. Type 'help' for available commands, '/exit' to quit.");

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

            if (input.Trim().Equals("/exit", StringComparison.OrdinalIgnoreCase))
                break;

            if (input.Trim().Equals("/help", StringComparison.OrdinalIgnoreCase))
            {
                _router.Route("help");
                continue;
            }

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
        }
    }

    private static string[] ParseInput(string input)
    {
        return input.Split([' '], StringSplitOptions.RemoveEmptyEntries);
    }
}
