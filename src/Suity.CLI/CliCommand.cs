namespace Suity.Editor;

public abstract class CliCommand
{
    public abstract string Description { get; }

    public virtual string Usage => string.Empty;

    public abstract void DoCommand(ICliArguments args);
}
