using Suity.Drawing;
using Suity.Editor.Types;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.Flows.WorkSpaces;

#region ListWorkSpaceFiles

/// <summary>
/// Lists all files in a workspace matching a filter pattern.
/// </summary>
[SimpleFlowNodeStyle(HasHeader = false)]
[DisplayText("List WorkSpace Files", "*CoreIcon|WorkSpace")]
[NativeAlias("Suity.Editor.AIGC.WorkSpaces.Flows.ListWorkSpaceFiles")]
[NativeAlias("Suity.Editor.AIGC.Flows.WorkSpaces.ListWorkSpaceFiles")]
public class ListWorkSpaceFiles : WorkSpaceNode
{
    private readonly ConnectorAssetProperty<WorkSpaceAsset> _workSpace = new("WorkSpace", "WorkSpace");
    readonly ConnectorStringProperty _filter = new("Filter", "Filter", "*");
    readonly FlowNodeConnector _workSpaceFiles;

    public ListWorkSpaceFiles()
    {
        _workSpace.AddConnector(this);
        _filter.AddConnector(this);
        _workSpaceFiles = this.AddDataOutputConnector("WorkSpaceFiles", "string[]", "WorkSpace Files");
    }

    /// <inheritdoc/>
    public override ImageDef Icon => CoreIconCache.File;

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _workSpace.Sync(sync);
        _filter.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _workSpace.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var workSpace = _workSpace.GetTarget(compute, this)?.WorkSpace;
        if (workSpace is null)
        {
            throw new NullReferenceException($"{nameof(WorkSpace)} not found.");
        }

        string filter = _filter.GetValue(compute, this);
        if (string.IsNullOrWhiteSpace(filter))
        {
            filter = "*";
        }

        var rootDir = new DirectoryInfo(workSpace.MasterDirectory);
        var fileInfos = rootDir.GetFiles(filter, SearchOption.AllDirectories);

        string[] filePaths = fileInfos
            .Select(o => PathUtility.MakeRalativePath(o.FullName, rootDir.FullName))
            .ToArray();

        compute.SetValue(_workSpaceFiles, filePaths);
    }
}
#endregion

#region ReadWorkSpaceFile

/// <summary>
/// Reads the content of a file from a workspace.
/// </summary>
[SimpleFlowNodeStyle(HasHeader = false)]
[DisplayText("Read WorkSpace File", "*CoreIcon|WorkSpace")]
[NativeAlias("Suity.Editor.AIGC.WorkSpaces.Flows.ReadWorkSpaceFile")]
[NativeAlias("Suity.Editor.AIGC.Flows.WorkSpaces.ReadWorkSpaceFile")]
public class ReadWorkSpaceFile : WorkSpaceNode
{
    private readonly ConnectorAssetProperty<WorkSpaceAsset> _workSpace = new("WorkSpace", "WorkSpace");
    private readonly ConnectorStringProperty _filePath = new("FilePath", "File Path", default, "Input file relative path");

    private readonly FlowNodeConnector _content;

    public ReadWorkSpaceFile()
    {
        _workSpace.AddConnector(this);
        _filePath.AddConnector(this);

        _content = AddDataOutputConnector("Content", "string", "File Content");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _workSpace.Sync(sync);
        _filePath.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _workSpace.InspectorField(setup, this);
        _filePath.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var workSpace = _workSpace.GetTarget(compute, this)?.WorkSpace;
        if (workSpace is null)
        {
            throw new NullReferenceException($"{nameof(WorkSpace)} not found.");
        }

        string filePath = _filePath.GetValue(compute, this) ?? string.Empty;
        filePath = filePath.Trim().Replace('\\', '/').TrimStart('.', '/');
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new NullReferenceException($"File path is empty.");
        }

        string fileFullPath = workSpace.MakeMasterFullPath(filePath);

        string content = FileUtils.Read(fileFullPath);

        compute.SetValue(_content, content);
    }
}

#endregion

#region WriteWorkSpaceFile

/// <summary>
/// Writes content to a file in a workspace.
/// </summary>
[DisplayText("Write WorkSpace File", "*CoreIcon|WorkSpace")]
[NativeAlias("Suity.Editor.AIGC.WorkSpaces.Flows.WriteWorkSpaceFile")]
[NativeAlias("Suity.Editor.AIGC.Flows.WorkSpaces.WriteWorkSpaceFile")]
public class WriteWorkSpaceFile : WorkSpaceNode
{
    private readonly FlowNodeConnector _in;
    private readonly FlowNodeConnector _out;

    private readonly ConnectorAssetProperty<WorkSpaceAsset> _workSpace = new("WorkSpace", "WorkSpace");
    private readonly ConnectorStringProperty _filePath = new("FilePath", "File Path", default, "Input file relative path");
    private readonly ConnectorTextBlockProperty _content = new("Content", "Content", default, "Input file content");

    public WriteWorkSpaceFile()
    {
        _in = AddActionInputConnector("In", "Input");
        _out = AddActionOutputConnector("Out", "Output");

        _workSpace.AddConnector(this);
        _filePath.AddConnector(this);
        _content.AddConnector(this);
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _workSpace.Sync(sync);
        _filePath.Sync(sync);
        _content.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _workSpace.InspectorField(setup, this);
        _filePath.InspectorField(setup, this);
        _content.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        var workSpace = _workSpace.GetTarget(compute, this)?.WorkSpace;
        if (workSpace is null)
        {
            throw new NullReferenceException($"{nameof(WorkSpace)} not found.");
        }

        string filePath = _filePath.GetValue(compute, this) ?? string.Empty;
        string content = _content.GetText(compute, this);

        workSpace.WriteWorkSpaceFile(filePath, content);

        return Task.FromResult<object>(_out);
    }
}

#endregion

#region DeleteWorkSpaceFile

/// <summary>
/// Deletes a file from a workspace.
/// </summary>
[DisplayText("Delete WorkSpace File", "*CoreIcon|WorkSpace")]
[NativeAlias("Suity.Editor.AIGC.WorkSpaces.Flows.DeleteWorkSpaceFile")]
[NativeAlias("Suity.Editor.AIGC.Flows.WorkSpaces.DeleteWorkSpaceFile")]
public class DeleteWorkSpaceFile : WorkSpaceNode
{
    private readonly FlowNodeConnector _in;
    private readonly FlowNodeConnector _out;

    private readonly ConnectorAssetProperty<WorkSpaceAsset> _workSpace = new("WorkSpace", "WorkSpace");
    private readonly ConnectorStringProperty _filePath = new("FilePath", "File Path", default, "Input file relative path");

    public DeleteWorkSpaceFile()
    {
        _in = AddActionInputConnector("In", "Input");

        _workSpace.AddConnector(this);
        _filePath.AddConnector(this);

        _out = AddActionOutputConnector("Out", "Output");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _workSpace.Sync(sync);
        _filePath.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _workSpace.InspectorField(setup, this);
        _filePath.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        var workSpace = _workSpace.GetTarget(compute, this)?.WorkSpace;
        if (workSpace is null)
        {
            throw new NullReferenceException($"{nameof(WorkSpace)} not found.");
        }

        string filePath = _filePath.GetValue(compute, this) ?? string.Empty;
        filePath = filePath.Trim().Replace('\\', '/').TrimStart('.', '/');
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new NullReferenceException($"File path is empty.");
        }

        string fileFullPath = workSpace.MakeMasterFullPath(filePath);

        if (File.Exists(fileFullPath))
        {
            File.Delete(fileFullPath);
        }

        return Task.FromResult<object>(_out);
    }
}

#endregion

#region IsWorkSpaceFileExist

/// <summary>
/// Checks whether a file exists in a workspace.
/// </summary>
[SimpleFlowNodeStyle(HasHeader = false)]
[DisplayText("Check WorkSpace File Exists", "*CoreIcon|WorkSpace")]
[NativeAlias("Suity.Editor.AIGC.WorkSpaces.Flows.IsWorkSpaceFileExist")]
[NativeAlias("Suity.Editor.AIGC.Flows.WorkSpaces.IsWorkSpaceFileExist")]
public class IsWorkSpaceFileExist : WorkSpaceNode
{
    private readonly ConnectorAssetProperty<WorkSpaceAsset> _workSpace = new("WorkSpace", "WorkSpace");
    private readonly ConnectorStringProperty _filePath = new("FilePath", "File Path", default, "Input file relative path");

    private readonly FlowNodeConnector _exists;

    public IsWorkSpaceFileExist()
    {
        _workSpace.AddConnector(this);
        _filePath.AddConnector(this);

        _exists = AddDataOutputConnector("Exists", "bool", "File Exists");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _workSpace.Sync(sync);
        _filePath.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _workSpace.InspectorField(setup, this);
        _filePath.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var workSpace = _workSpace.GetTarget(compute, this)?.WorkSpace;
        if (workSpace is null)
        {
            throw new NullReferenceException($"{nameof(WorkSpace)} not found.");
        }

        string filePath = _filePath.GetValue(compute, this) ?? string.Empty;
        filePath = filePath.Trim().Replace('\\', '/').TrimStart('.', '/');
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new NullReferenceException($"File path is empty.");
        }

        string fileFullPath = workSpace.MakeMasterFullPath(filePath);

        bool exists = File.Exists(fileFullPath);

        compute.SetValue(_exists, exists);
    }
}

#endregion

// GetAllFilePaths;

#region GetWorkSpaceDirectory

/// <summary>
/// Gets the full directory path of a workspace.
/// </summary>
[SimpleFlowNodeStyle(HasHeader = false)]
[DisplayText("Get WorkSpace Directory", "*CoreIcon|WorkSpace")]
[NativeAlias("Suity.Editor.AIGC.Flows.WorkSpaces.GetWorkSpaceDirectory")]
public class GetWorkSpaceDirectory : WorkSpaceNode
{
    private readonly ConnectorAssetProperty<WorkSpaceAsset> _workSpace = new("WorkSpace", "WorkSpace");
    private readonly FlowNodeConnector _directory;

    public GetWorkSpaceDirectory()
    {
        _workSpace.AddConnector(this);
        _directory = this.AddDataOutputConnector("Directory", "string", "Directory Path");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _workSpace.Sync(sync);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _workSpace.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var workSpace = _workSpace.GetTarget(compute, this)?.WorkSpace;
        if (workSpace is null)
        {
            throw new NullReferenceException($"{nameof(WorkSpace)} not found.");
        }

        compute.SetValue(_directory, workSpace.MasterDirectory);
    }
}

#endregion