using Suity.Editor.Services;
using Suity.Synchonizing;
using System;
using System.Collections.Generic;

namespace Suity.Editor;

/// <summary>
/// Project setting
/// </summary>
public class ProjectSetting : ISyncObject
{
    public string Version = "5";
    public string AssetDirectory = "Assets";
    public string UserDirectory = "Users";
    public string SystemDirectory = "System";
    public string PublishDirectory = "Publish";
    public string WorkSpaceDirectory = "WorkSpaces";
    public string AssembliesDirectory = "Assemblies";

    public Guid ProjectGuid;
    public Guid PlanetFolderGuid;
    public Guid SateliteFolderGuid;
    public Guid SpaceshipFolderGuid;
    public Guid AstronautFolderGuid;
    public Guid PluginFolderGuid;

    internal List<ProjectSettingItem> Configs { get; set; } = [];

    public ProjectSetting()
    {
        ProjectGuid = Guid.NewGuid();
        PlanetFolderGuid = Guid.NewGuid();
        SateliteFolderGuid = Guid.NewGuid();
        SpaceshipFolderGuid = Guid.NewGuid();
        AstronautFolderGuid = Guid.NewGuid();
        PluginFolderGuid = Guid.NewGuid();
    }

    #region ISyncObject

    void ISyncObject.Sync(IPropertySync sync, ISyncContext context)
    {
        //AssetDirectory = sync.Sync("AssetDirectory", AssetDirectory);
        //UserDirectory = sync.Sync("UserDirectory", UserDirectory);
        Version = sync.Sync("Version", Version);
        ProjectGuid = sync.Sync("Guid", ProjectGuid);
    }

    #endregion

    public static ProjectSetting CreateDefault()
    {
        return new ProjectSetting
        {
            Version = EditorServices.LicenseService.ProductVersion,
        };
    }
}

public class ProjectSettingItem
{
    public string Key { get; set; }
    public string Value { get; set; }
}