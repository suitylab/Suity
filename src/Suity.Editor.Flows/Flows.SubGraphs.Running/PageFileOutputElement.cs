using Suity.Editor.AIGC.Flows.Pages;
using Suity.Editor.AIGC.TaskPages;
using Suity.Editor.Flows;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.IO;

namespace Suity.Editor.Flows.SubGraphs.Running;

/// <summary>
/// Represents a file output element in an AIGC page that handles file path output and validation.
/// </summary>
public class PageFileOutputElement : AigcPageElement, IPageParameterOutput
{
    private readonly PageFileOutputItem _outputItem;
    private string _value = string.Empty;
    private FlowNodeConnector _connector;


    /// <summary>
    /// Initializes a new instance of the <see cref="PageFileOutputElement"/> class.
    /// </summary>
    /// <param name="outputItem">The page file output item.</param>
    public PageFileOutputElement(PageFileOutputItem outputItem)
        : base(outputItem)
    {
        _outputItem = outputItem ?? throw new ArgumentNullException(nameof(outputItem));
    }

    /// <inheritdoc/>
    public override FlowNodeConnector OuterConnector => _connector;

    #region IPageParameterOutput

    /// <summary>
    /// Gets the parameter type, which is always string for file output elements.
    /// </summary>
    public TypeDefinition ParameterType => NativeTypes.StringType;

    /// <summary>
    /// Gets the current file path value.
    /// </summary>
    public object Value => _value;

    /// <summary>
    /// Gets or sets a value indicating whether a value has been set.
    /// </summary>
    public bool IsValueSet { get; set; }


    /// <summary>
    /// Sets the file path value for this element.
    /// </summary>
    /// <param name="value">The file path value to set.</param>
    public void SetValue(object value)
    {
        _value = value as string ?? string.Empty;
        IsValueSet = true;
    }

    /// <summary>
    /// Ensures and returns the current file path value.
    /// </summary>
    /// <returns>The current file path value.</returns>
    public object EnsureValue() => _value;

    /// <summary>
    /// Gets a value indicating whether this element signals task completion.
    /// </summary>
    public bool TaskCompletion { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this element signals task commit.
    /// </summary>
    public bool TaskCommit { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this element contributes to chat history.
    /// </summary>
    public bool ChatHistory { get; private set; }

    /// <summary>
    /// Resolves the chat history text representation of the file path value.
    /// </summary>
    /// <returns>The file path as chat history text.</returns>
    public ChatHistoryText ResolveChatHistory()
    {
        return _value?.ToString();
    }
    #endregion

    /// <inheritdoc/>
    protected override void OnBuild()
    {
        base.OnBuild();

        TaskCompletion = _outputItem.Node?.TaskCompletion == true;
        TaskCommit = _outputItem.Node?.TaskCommit == true;
        ChatHistory = _outputItem.Node?.ChatHistory == true;
    }

    /// <inheritdoc/>
    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        base.Sync(sync, context);

        if (!Option.Mode.IsTaskOrPage())
        {
            return;
        }

        _value = sync.Sync(Name, _value);
    }

    /// <inheritdoc/>
    public override void SetupView(IViewObjectSetup setup)
    {
        if (!Option.Mode.IsTaskOrPage())
        {
            return;
        }

        bool fileExist = GetFileExist();
        if (!fileExist)
        {
            setup.Warning("File not exist.");
        }

        var property = new ViewProperty(Name, DisplayText, Icon)
            .WithHintText(_value)
            .WithStatus(GetStatus());

        setup.InspectorFieldOf<string>(property);
    }

    /// <inheritdoc/>
    public override void UpdateConnector(PageFunctionNode node)
    {
        var valueType = ParameterType;
        _connector = node.AddDataOutputConnector(Name, valueType, DisplayText);
    }

    /// <inheritdoc/>
    public override void UpdateFromOther(IAigcPageElement other)
    {
        if (other is PageFileOutputElement otherOutput)
        {
            UpdateFromOther(otherOutput);
        }
    }

    /// <summary>
    /// Updates the current element's value from another <see cref="PageFileOutputElement"/>.
    /// </summary>
    /// <param name="otherParameter">The other file output element to copy values from.</param>
    public void UpdateFromOther(PageFileOutputElement otherParameter)
    {
        _value = otherParameter._value;
    }

    /// <summary>
    /// Sets the outer value from the flow computation context.
    /// </summary>
    /// <param name="outerCompute">The flow computation context.</param>
    /// <param name="value">The value to set.</param>
    public void SetOuterValue(IFlowComputation outerCompute, object value)
    {
        if (_connector != null)
        {
            outerCompute.SetValue(_connector, value);
        }
    }

    /// <inheritdoc/>
    public override bool? GetIsDone()
    {
        if (TaskCompletion)
        {
            return GetFileExist();
        }
        else
        {
            return null;
        }
    }


    /// <summary>
    /// Checks whether the file at the current path exists in the task workspace.
    /// </summary>
    /// <returns>True if the file exists; otherwise, false.</returns>
    public bool GetFileExist()
    {
        if (string.IsNullOrWhiteSpace(_value))
        {
            return false;
        }

        var taskPage = Option.Owner as IAigcTaskPage;
        var workSpace = taskPage.TaskHost?.WorkSpace;
        if (workSpace is null)
        {
            return false;
        }

        string fullPath = PathUtility.MakeFullPath(_value, workSpace.MasterDirectory);

        return File.Exists(fullPath);
    }
}
