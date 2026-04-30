using Suity.Editor.ProjectGui.Commands;
using Suity.Editor.WorkSpaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.ProjectGui;

/// <summary>
/// Plugin that provides the project view functionality, including the project tree explorer and related services.
/// </summary>
public class ProjectViewPlugin : EditorPlugin, IProjectView
{
    /// <inheritdoc/>
    public override string Description => "Project View";

    /// <inheritdoc/>
    internal protected override void AwakeProject()
    {
        base.AwakeProject();

        if (GetProjectState() is ProjectViewConfig config)
        {
            var viewImGui = EditorUtility.GetToolWindow<ProjectGui>();
            viewImGui?.LoadConfig(config);
        }

        EditorRexes.ImportPackage.AddActionListener(ImportPackageCommand.HandleImport);
    }

    /// <inheritdoc/>
    internal protected override void StopProject()
    {
        base.StopProject();

        var config = new ProjectViewConfig();

        var viewImGui = EditorUtility.GetToolWindow<ProjectGui>();
        viewImGui?.SaveConfig(config);

        SetProjectState(config);
    }

    /// <inheritdoc/>
    public override object GetService(Type serviceType)
    {
        if (serviceType == typeof(IProjectView))
        {
            return this;
        }

        return null;
    }

    /// <inheritdoc/>
    IEnumerable<T> IProjectView.GetSelectedNodes<T>()
    {
        return Device.Current.GetService<IProjectGui>().SelectedNodes.OfType<T>();
    }

    /// <inheritdoc/>
    IProjectViewNode IProjectView.SelectedNode
    {
        get
        {
            return Device.Current.GetService<IProjectGui>().SelectedNode as IProjectViewNode;
        }
    }

    /// <inheritdoc/>
    public IProjectViewNode FindFileNode(string fileName)
    {
        return Device.Current.GetService<IProjectGui>().FindFileNode(fileName) as IProjectViewNode;
    }

    /// <inheritdoc/>
    public IAssetFileNode FindFileNode(EditorObject obj)
    {
        string fileName = obj.GetStorageLocation()?.PhysicFileName;
        if (string.IsNullOrEmpty(fileName))
        {
            return null;
        }

        return Device.Current.GetService<IProjectGui>().FindFileNode(fileName) as IAssetFileNode;
    }

    /// <inheritdoc/>
    public IWorkSpaceNode FindWorkSpaceNode(WorkSpace workSpace)
    {
        return Device.Current.GetService<IProjectGui>().FindWorkSpaceNode(workSpace);
    }
}