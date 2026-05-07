using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Views;
using System;

namespace Suity.Editor.Flows.TaskPages;

#region GetTaskWorkSpace
/// <summary>
/// A flow node that retrieves the workspace associated with the current AIGC task page.
/// Outputs the workspace asset through a data output connector.
/// </summary>
[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = false)]
[DisplayText("Get Task WorkSpace", "*CoreIcon|WorkSpace")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.GetTaskWorkSpace")]
public class GetTaskWorkSpace : TaskPageNode
{
    readonly FlowNodeConnector _taskWorkSpace;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTaskWorkSpace"/> class,
    /// setting up the data output connector for the task workspace.
    /// </summary>
    public GetTaskWorkSpace()
    {
        var type = TypeDefinition.FromAssetLink<WorkSpaceAsset>();

        _taskWorkSpace = this.AddDataOutputConnector("TaskWorkSpace", type, "Task WorkSpace");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var taskPage = compute.Context.GetArgument<IAigcTaskPage>();

        var workSpace = taskPage?.TaskHost?.WorkSpace;

        compute.SetValue(_taskWorkSpace, workSpace?.GetAsset());
    }
}
#endregion

#region CreateTaskWorkSpace
/// <summary>
/// A flow node that creates a new workspace for the current AIGC task page,
/// or optionally retrieves an existing one if it already exists.
/// </summary>
[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = true)]
[DisplayText("Create Task WorkSpace", "*CoreIcon|WorkSpace")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.CreateTaskWorkSpace")]
public class CreateTaskWorkSpace : TaskPageNode
{
    readonly FlowNodeConnector _in;

    readonly ConnectorStringProperty _workSpaceName = new("WorkSpaceName", "WorkSpace Name");
    readonly ConnectorValueProperty<bool> _getIfExist = new("GetIfExist", "Get If Exist", true);
    readonly FlowNodeConnector _taskWorkSpace;

    readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTaskWorkSpace"/> class,
    /// setting up input, output connectors and configuration properties.
    /// </summary>
    public CreateTaskWorkSpace()
    {
        _in = this.AddActionInputConnector("In", "Input");

        _workSpaceName.AddConnector(this);
        _getIfExist.AddConnector(this);

        var type = TypeDefinition.FromAssetLink<WorkSpaceAsset>();
        _taskWorkSpace = this.AddDataOutputConnector("TaskWorkSpace", type, "Task WorkSpace");

        _out = this.AddActionOutputConnector("Out", "Output");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _workSpaceName.Sync(sync);
        _getIfExist.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.SetupView(setup);

        _workSpaceName.InspectorField(setup, this);
        _getIfExist.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var taskPage = compute.Context.GetArgument<IAigcTaskPage>();
        if (taskPage?.TaskHost is not { } host)
        {
            throw new NullReferenceException("TaskHost is null.");
        }

        var workSpace = host.WorkSpace;
        if (_getIfExist.GetValue(compute, this) == true && workSpace != null)
        {
            compute.SetValue(_taskWorkSpace, workSpace.GetAsset());
            compute.SetResult(this, _out);
            return;
        }

        string workSpaceName = _workSpaceName.GetValue(compute, this);
        if (string.IsNullOrWhiteSpace(workSpaceName))
        {
            throw new NullReferenceException("WorkSpaceName is null.");
        }
        if (!NamingVerifier.VerifyIdentifier(workSpaceName))
        {
            throw new ArgumentException("Invalid WorkSpaceName.", nameof(workSpaceName));
        }

        workSpace = host.CreateWorkSpace(workSpaceName);
        if (workSpace is null)
        {
            throw new NullReferenceException("Create WorkSpace failed.");
        }

        compute.SetValue(_taskWorkSpace, workSpace.GetAsset());
        compute.SetResult(this, _out);
    }
}

#endregion