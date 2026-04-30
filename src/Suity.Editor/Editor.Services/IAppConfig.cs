namespace Suity.Editor.Services;

/// <summary>
/// Service interface for application configuration.
/// </summary>
public interface IAppConfig
{
    /// <summary>
    /// Gets a setting value by name.
    /// </summary>
    /// <param name="name">The setting name.</param>
    /// <returns>The setting value, or null if not found.</returns>
    string GetSetting(string name);
}

/// <summary>
/// Default implementation of the application config.
/// </summary>
internal class DefaultAppConfig : IAppConfig
{
    /// <summary>
    /// Gets the singleton instance of DefaultAppConfig.
    /// </summary>
    public static readonly DefaultAppConfig Instance = new DefaultAppConfig();

    /// <inheritdoc/>
    public string GetSetting(string name)
    {
        return null;
    }
}