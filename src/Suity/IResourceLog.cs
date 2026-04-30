namespace Suity;

/// <summary>
/// Defines an interface for logging resource access.
/// </summary>
public interface IResourceLog
{
    void AddResourceLog(string key, string path);
}

/// <summary>
/// Provides an empty implementation of IResourceLog that does nothing.
/// </summary>
public sealed class EmptyResourceLog : IResourceLog
{
    public static readonly EmptyResourceLog Empty = new EmptyResourceLog();

    private EmptyResourceLog()
    {
    }

    public void AddResourceLog(string key, string path)
    {
    }
}