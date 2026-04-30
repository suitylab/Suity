using System;

namespace Suity.Editor;

/// <summary>
/// Represents a single plugin configuration item within a project, storing the plugin name, associated asset GUID, and configuration value.
/// </summary>
public class ProjectConfigItem
{
    /// <summary>
    /// Gets or sets the name of the plugin this configuration item belongs to.
    /// </summary>
    public string Plugin { get; set; }

    /// <summary>
    /// Gets or sets the unique asset identifier (GUID) associated with this configuration item.
    /// </summary>
    public Guid Asset { get; set; }

    /// <summary>
    /// Gets or sets the configuration value for this plugin item.
    /// </summary>
    public object Value { get; set; }
}