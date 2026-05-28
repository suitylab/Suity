using Suity.Editor.AIGC;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Suity.Editor.Flows.SubFlows.Running;

/// <summary>
/// Represents a file output element in an sub-graph that handles file path output and validation.
/// </summary>
public class SubFlowFileOutput : SubFlowElement, IPageParameterOutput
{
    private readonly PageFileOutputItem _outputItem;

    private string _path = string.Empty;
    private SArray _paths = null;

    private FlowNodeConnector _connector;


    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowFileOutput"/> class.
    /// </summary>
    /// <param name="outputItem">The page file output item.</param>
    public SubFlowFileOutput(PageFileOutputItem outputItem)
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
    public object Value => _path;

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
        _path = value as string;
        _paths = value as SArray;
        IsValueSet = true;
    }

    /// <summary>
    /// Ensures and returns the current file path value.
    /// </summary>
    /// <returns>The current file path value.</returns>
    public object EnsureValue() => _path;

    /// <inheritdoc/>
    public bool Required { get; private set; }

    /// <inheritdoc/>
    public bool TaskCommit { get; private set; }

    /// <inheritdoc/>
    public bool ChatHistory { get; private set; }

    public bool AddressMode { get; private set; }

    public bool IsArray { get; private set; }

    /// <inheritdoc/>
    public HistoryTag ResolveChatHistory()
    {
        if (IsArray)
        {
            if (_paths is null)
            {
                return null;
            }

            var builder = new StringBuilder();

            if (AddressMode)
            {
                foreach (var item in _paths)
                {
                    string path = SItem.ResolveValue(item)?.ToString();
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        continue;
                    }

                    builder.AppendLine(path);
                }
            }
            else
            {
                foreach (var item in _paths)
                {
                    string path = SItem.ResolveValue(item)?.ToString();
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        continue;
                    }

                    string text = ReadAllText(path);
                    builder.AppendLine($"<File path='{path}'>");
                    builder.AppendLine(text);
                    builder.AppendLine("</File>");
                }
            }

            return builder.ToString();
        }
        else
        {
            if (AddressMode)
            {
                return _path;
            }
            else
            {
                string text = ReadAllText(_path);
                return new HistoryTag(text, [new("path", _path)]);
            }
        }
    }
    #endregion


    /// <inheritdoc/>
    protected override void OnBuild()
    {
        base.OnBuild();

        Required = _outputItem.Node?.Required == true;
        TaskCommit = _outputItem.Node?.TaskCommit == true;
        ChatHistory = _outputItem.Node?.ChatHistory == true;
        AddressMode = _outputItem.Node?.AddressMode == true;
        IsArray = _outputItem.Node?.IsArray == true;
    }

    /// <inheritdoc/>
    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        base.Sync(sync, context);

        if (!Option.Mode.IsTaskOrPage())
        {
            return;
        }

        if (IsArray)
        {
            _paths = sync.Sync(Name, _paths);
            _path = null;
        }
        else
        {
            _path = sync.Sync(Name, _path);
            _paths = null;
        }
    }

    /// <inheritdoc/>
    public override void SetupView(IViewObjectSetup setup)
    {
        if (!Option.Mode.IsTaskOrPage())
        {
            return;
        }

        bool fileExist = GetFileExist(_path);
        if (!fileExist)
        {
            setup.Warning("File not exist.");
        }

        var property = new ViewProperty(Name, DisplayText, Icon)
            .WithStatus(GetStatus());

        if (IsArray)
        {
            setup.InspectorFieldOf<SArray>(property);
        }
        else
        {
            setup.InspectorFieldOf<string>(property);
        }
    }

    /// <inheritdoc/>
    public override void UpdateConnector(PageFunctionNode node)
    {
        var valueType = ParameterType;
        _connector = node.AddDataOutputConnector(Name, valueType, DisplayText);
    }

    /// <inheritdoc/>
    public override void UpdateFromOther(ISubFlowElement other)
    {
        if (other is SubFlowFileOutput otherOutput)
        {
            UpdateFromOther(otherOutput);
        }
    }

    /// <summary>
    /// Updates the current element's value from another <see cref="SubFlowFileOutput"/>.
    /// </summary>
    /// <param name="otherParameter">The other file output element to copy values from.</param>
    public void UpdateFromOther(SubFlowFileOutput otherParameter)
    {
        _path = otherParameter._path;
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
        if (IsArray)
        {
            return _paths?.Count > 0;
        }
        else
        {
            //if (Required)
            //{
            //    return GetFileExist(_path);
            //}
            //else
            //{
            //    return null;
            //}

            return !string.IsNullOrWhiteSpace(_path);
        }
    }


    /// <summary>
    /// Checks whether the file at the current path exists in the task workspace.
    /// </summary>
    /// <returns>True if the file exists; otherwise, false.</returns>
    public bool GetFileExist(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var workSpace = GetWorkSpace();
        if (workSpace is null)
        {
            return false;
        }

        string fullPath = PathUtility.MakeFullPath(_path, workSpace.MasterDirectory);

        return File.Exists(fullPath);
    }

    public string ReadAllText(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var workSpace = GetWorkSpace();
        if (workSpace is null)
        {
            return null;
        }

        string fullPath = PathUtility.MakeFullPath(path, workSpace.MasterDirectory);

        try
        {
            return File.ReadAllText(fullPath);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public WorkSpace GetWorkSpace()
    {
        var taskPage = Option.Owner as IAigcWorkflowPage;
        var workSpace = taskPage.TaskHost?.WorkSpace;

        return workSpace;
    }
}
