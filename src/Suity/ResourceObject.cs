namespace Suity;

/// <summary>
/// Base class for objects that represent resources.
/// Provides resource access tracking functionality.
/// </summary>
public abstract class ResourceObject : Suity.Object
{
    public string Key { get; protected set; }

    public void MarkAccess()
    {
        string key = Key;
        if (!string.IsNullOrEmpty(key))
        {
            Logs.AddResourceLog(key, null);
        }
    }

    public void MarkAccess(string message)
    {
        string key = Key;
        if (!string.IsNullOrEmpty(key))
        {
            Logs.AddResourceLog(key, message);
        }
    }
}