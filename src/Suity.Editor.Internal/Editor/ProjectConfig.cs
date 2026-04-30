using System.Collections.Generic;

namespace Suity.Editor;

/// <summary>
/// Container class that holds the complete project configuration, including a collection of plugin-specific configuration items.
/// </summary>
public class ProjectConfig
{
    /// <summary>
    /// Gets or sets the list of plugin configuration items for this project.
    /// </summary>
    public List<ProjectConfigItem> PluginConfigs { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectConfig"/> class.
    /// </summary>
    public ProjectConfig()
    {
    }
}