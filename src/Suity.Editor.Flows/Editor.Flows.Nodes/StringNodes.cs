using Suity.Synchonizing;
using Suity.Views;
using System.Text.RegularExpressions;

namespace Suity.Editor.Flows.Nodes;

#region TrimNumeric

/// <summary>
/// A flow node that trims leading and trailing numeric characters, dots, and underscores from a string.
/// </summary>
[DisplayText("Trim Numeric Part")]
public class TrimNumeric : ValueFlowNode
{
    private readonly FlowNodeConnector _in;
    private readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="TrimNumeric"/> class.
    /// </summary>
    public TrimNumeric()
    {
        _in = AddConnector("In", "*System|String", FlowDirections.Input, FlowConnectorTypes.Data);
        _out = AddConnector("Out", "*System|String", FlowDirections.Output, FlowConnectorTypes.Data);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        string input = compute.GetValueConvert<string>(_in) ?? string.Empty;

        // Define the regular expression pattern.
        string pattern = @"^[0-9._]+|_[0-9._]+$";

        // Use the Regex class to remove leading and trailing numbers and underscores.
        string output = Regex.Replace(input, pattern, "");

        compute.SetValue(_out, output);
    }
}

#endregion

#region StringReplace

/// <summary>
/// A flow node that replaces occurrences of a specified pattern in a string, with optional regex support.
/// </summary>
[DisplayText("Replace String")]
public class StringReplace : ValueFlowNode
{
    private readonly FlowNodeConnector _in;
    private readonly FlowNodeConnector _out;

    private string _pattern = string.Empty;
    private string _replace = string.Empty;
    private bool _regex = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="StringReplace"/> class.
    /// </summary>
    public StringReplace()
    {
        _in = AddConnector("In", "*System|String", FlowDirections.Input, FlowConnectorTypes.Data);
        _out = AddConnector("Out", "*System|String", FlowDirections.Output, FlowConnectorTypes.Data);
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _pattern = sync.Sync("Pattern", _pattern, SyncFlag.NotNull) ?? string.Empty;
        _replace = sync.Sync("Replace", _replace, SyncFlag.NotNull) ?? string.Empty;
        _regex = sync.Sync("Regex", _regex);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_pattern, new ViewProperty("Pattern", "Match"));
        setup.InspectorField(_replace, new ViewProperty("Replace", "Replace"));
        setup.InspectorField(_regex, new ViewProperty("Regex", "Use Regex"));
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        string input = compute.GetValueConvert<string>(_in) ?? string.Empty;

        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(_pattern))
        {
            compute.SetValue(_out, input);

            return;
        }

        string output; 
        if (_regex)
        {
            output = Regex.Replace(input, _pattern, _replace);
        }
        else
        {
            output = input.Replace(_pattern, _replace);
        }
        
        compute.SetValue(_out, output);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{_pattern} > {_replace}";
    }
}

#endregion

#region ReplaceEmptyString

/// <summary>
/// A flow node that replaces an empty or whitespace-only string with a specified replacement value.
/// </summary>
[SimpleFlowNodeStyle(HasHeader = false)]
[DisplayText("Replace Empty String")]
public class ReplaceEmptyString : ValueFlowNode
{
    private readonly FlowNodeConnector _in;
    private readonly FlowNodeConnector _replace;
    private readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReplaceEmptyString"/> class.
    /// </summary>
    public ReplaceEmptyString()
    {
        _in = AddDataInputConnector("In", "string", "Input");
        _replace = AddDataInputConnector("Replace", "string", "Replace");
        _out = AddDataOutputConnector("Out", "string", "Output");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        string str = compute.GetValueConvert<string>(_in);
        if (string.IsNullOrWhiteSpace(str))
        {
            str = compute.GetValueConvert<string>(_replace);
        }

        compute.SetValue(_out, str);
    }
}

#endregion

#region IsEmptyString

/// <summary>
/// A flow node that checks whether a string is null or empty, excluding whitespace-only strings.
/// </summary>
[SimpleFlowNodeStyle(HasHeader = false)]
[DisplayText("Is Empty String", "*CoreIcon|Text")]
[ToolTipsText("Check if string is empty, excluding whitespace.")]
public class IsEmptyString : ValueFlowNode
{
    readonly FlowNodeConnector _in;
    readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="IsEmptyString"/> class.
    /// </summary>
    public IsEmptyString()
    {
        _in = AddDataInputConnector("In", "string", "Input");
        _out = AddDataOutputConnector("Out", "bool", "Empty String");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        string input = compute.GetValueConvert<string>(_in);

        compute.SetValue(_out, string.IsNullOrEmpty(input));
    }
}

#endregion

#region IsBlankString

/// <summary>
/// A flow node that checks whether a string is null, empty, or consists only of whitespace characters.
/// </summary>
[SimpleFlowNodeStyle(HasHeader = false)]
[DisplayText("Is Blank String", "*CoreIcon|Text")]
[ToolTipsText("Check if string is blank, including whitespace.")]
public class IsBlankString : ValueFlowNode
{
    readonly FlowNodeConnector _in;
    readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="IsBlankString"/> class.
    /// </summary>
    public IsBlankString()
    {
        _in = AddDataInputConnector("In", "string", "Input");
        _out = AddDataOutputConnector("Out", "bool", "Blank String");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        string input = compute.GetValueConvert<string>(_in);

        compute.SetValue(_out, string.IsNullOrWhiteSpace(input));
    }
}

#endregion
