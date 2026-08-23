namespace Suity.Editor;

public abstract class CliCommand
{
    public abstract string Description { get; }

    public virtual string Usage => string.Empty;

    public virtual string DetailedHelp => string.Empty;

    public abstract void DoCommand(ICliArguments args);

    public virtual void ShowHelp()
    {
        Console.WriteLine(Description);
        Console.WriteLine();
        if (!string.IsNullOrEmpty(Usage))
        {
            Console.WriteLine($"Usage: {Usage}");
            Console.WriteLine();
        }
        if (!string.IsNullOrEmpty(DetailedHelp))
        {
            Console.WriteLine(DetailedHelp);
        }
    }
}
