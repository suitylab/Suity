namespace Suity.CLI;

public abstract class ConsoleCommand
{
    public abstract string Description { get; }

    public virtual string Usage => string.Empty;

    public abstract void DoCommand(ConsoleArguments args);
}
