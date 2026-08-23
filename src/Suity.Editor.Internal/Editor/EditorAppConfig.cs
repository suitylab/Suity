using System.Collections.Generic;

namespace Suity.Editor;

public class EditorAppConfig
{
    public string Language { get; set; } = "en";
    public List<string> LastProjects { get; set; } = [];

    public EditorAppConfig()
    {
    }

    public void AddProjectRecord(string projectFile)
    {
        LastProjects ??= [];

        LastProjects.RemoveAll(v => v == projectFile);
        LastProjects.Insert(0, projectFile);

        while (LastProjects.Count > 10)
        {
            LastProjects.RemoveAt(LastProjects.Count - 1);
        }
    }
}