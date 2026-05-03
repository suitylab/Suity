using System;
using System.Collections.Generic;

namespace Suity.Editor.Views.Startup;


[Obsolete]
public class ProductConfig
{
    public string Name { get; set; }

    public string Version { get; set; }

    public string DownloadUrl { get; set; }
}


public enum ProjectTemplateUsages
{
    Preset,

    Template,

    Example,

    Learning,
}

public class ProjectStartupConfig
{
    public List<ProjectTemplateInfo> ProjectTemplates { get; set; } = [];
}

public class ProjectTemplateInfo
{
    public string Id { get; set; }

    public string Name { get; set; }

    public ProjectTemplateUsages TemplateUsage { get; set; }

    public string Description { get; set; }

    public string DownloadUrl { get; set; }

    public string CustomImageFileName { get; set; }

    public string InitialOpenDocument { get; set; }

    public string GetTitle()
    {
        string name = Name;
        if (string.IsNullOrWhiteSpace(name))
        {
            name = Id;
        }

        return name;
    }

}
