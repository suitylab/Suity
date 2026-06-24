namespace Suity.Editor.AIGC;

public interface IAigcStartup
{
    bool IsStartup { get; }

    void HandleStartup(string prompt);
}

/// <summary>
/// Filter that selects assets suitable for use as startup pages.
/// </summary>
public class StartupPageFilter : IAssetFilter
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="StartupPageFilter"/>.
    /// </summary>
    public static StartupPageFilter Instance { get; } = new();

    /// <inheritdoc/>
    public bool FilterAsset(Asset asset)
    {
        if (asset is not IAigcStartup startup)
        {
            return false;
        }

        return startup.IsStartup;
    }
}
