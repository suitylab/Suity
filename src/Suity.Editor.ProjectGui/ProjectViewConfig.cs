using System.Collections.Generic;

namespace Suity.Editor.ProjectGui;

/// <summary>
/// Stores the persistent configuration state for the project view, such as expanded tree paths.
/// </summary>
public class ProjectViewConfig
{
    /// <summary>
    /// Gets or sets the list of expanded node paths in the project tree view.
    /// </summary>
    public List<string> Expands { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectViewConfig"/> class.
    /// </summary>
    public ProjectViewConfig()
    {
    }
}