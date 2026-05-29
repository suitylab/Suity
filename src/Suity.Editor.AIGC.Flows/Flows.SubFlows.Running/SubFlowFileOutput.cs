using Suity.Editor.AIGC;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using System;
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

        var page = Option.Owner as IAigcWorkflowPage;

        if (SaveToScratchPad && page?.GetScratchPadContainer() is { } scratchPad)
        {
            if (IsArray && _paths is { } paths)
            {
                foreach (var item in paths)
                {
                    string path = SItem.ResolveValue(item)?.ToString();
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        continue;
                    }

                    page?.WriteFileToScratchPad(scratchPad, path);
                }
            }
            else if (!IsArray && !string.IsNullOrWhiteSpace(_path))
            {
                page?.WriteFileToScratchPad(scratchPad, _path);
            }
        }
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

    public bool SaveToScratchPad { get; private set; }

    /// <inheritdoc/>
    public HistoryTag ResolveChatHistory(ResolveChatIntents intent)
    {
        var addrMode = AddressMode;
        if (intent == ResolveChatIntents.Preview)
        {
            addrMode = true;
        }

        if (IsArray)
        {
            if (_paths is null)
            {
                return null;
            }

            var builder = new StringBuilder();
            foreach (var item in _paths)
            {
                string path = SItem.ResolveValue(item)?.ToString();
                if (string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                builder.AppendLine(path);
            }

            return builder.ToString();
        }
        else
        {
            return _path;
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
        SaveToScratchPad = _outputItem.Node?.SaveToScratchPad == true;
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

        var property = new ViewProperty(Name, DisplayText, Icon)
            .WithStatus(GetStatus());

        if (IsArray)
        {
            setup.InspectorFieldOf<SArray>(property);
        }
        else
        {
            var page = Option.Owner as IAigcWorkflowPage;
            bool fileExist = page?.GetFileExist(_path) == true;
            if (!fileExist)
            {
                setup.Warning("File not exist.");
            }
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


}
