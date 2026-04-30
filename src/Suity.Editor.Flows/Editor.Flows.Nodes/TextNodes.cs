using Suity.Collections;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Views;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Suity.Editor.Flows.Nodes;

#region InputTextBlock

/// <summary>
/// A flow node that holds a user-editable text block and outputs its content as a string.
/// </summary>
[DisplayText("Input Text")]
[SimpleFlowNodeStyle(HasHeader = false, Width = 100, Height = 20)]
public class InputTextBlock : TextFlowNode
{
    private readonly FlowNodeConnector _output;

    private SValue _text = new STextBlock();

    /// <summary>
    /// Initializes a new instance of the <see cref="InputTextBlock"/> class.
    /// </summary>
    public InputTextBlock()
    {
        _output = AddDataOutputConnector("Output", "*System|String", "Text");
    }

    /// <summary>
    /// Gets the plain text content of this node.
    /// </summary>
    public string Text => (_text?.GetValue() as TextBlock)?.Text ?? string.Empty;

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _text = sync.Sync(nameof(Text), _text, SyncFlag.NotNull) ?? new STextBlock();
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_text, new ViewProperty(nameof(Text), "Text"));
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        compute.SetValue(_output, Text ?? string.Empty);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        string s = Text;
        if (string.IsNullOrWhiteSpace(s))
        {
            return "Text";
        }

        if (s.Length < 30)
        {
            return s;
        }
        else
        {
            return s.Substring(0, 28) + "...";
        }
    }
}

#endregion

#region TextAssetRef

/// <summary>
/// A flow node that references a text asset from the asset library and outputs its content.
/// </summary>
[DisplayText("Text Reference")]
[ToolTipsText("Reference text asset from asset library.")]
[NativeAlias("Suity.Editor.Flows.Nodes.TextBlockRef", UseForSaving = true)]
public class TextAssetRef : TextFlowNode, INavigable
{
    private readonly ConnectorAssetProperty<ITextAsset> _selection = new("Text", "Text Asset");

    private readonly FlowNodeConnector _output;

    /// <summary>
    /// Initializes a new instance of the <see cref="TextAssetRef"/> class with no asset selected.
    /// </summary>
    public TextAssetRef()
    {
        _selection.AddConnector(this);
        _output = AddDataOutputConnector("Output", "string", "Output");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextAssetRef"/> class with the specified text asset.
    /// </summary>
    /// <param name="textAsset">The text asset to reference.</param>
    public TextAssetRef(TextAsset textAsset)
        : this()
    {
        _selection.TargetAsset = textAsset;
    }

    //public string Text => _selection.BaseTarget?.GetText() ?? string.Empty;

    /// <inheritdoc/>
    public override Image Icon => _selection.BaseTarget?.ToDisplayIcon() ?? base.Icon;

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _selection.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _selection.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var text = _selection.GetTarget(compute, this)?.GetText() ?? string.Empty;

        compute.SetValue(_output, text ?? string.Empty);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (_selection.BaseTarget != null)
        {
            return _selection.BaseTarget.ToDisplayText();
        }
        else
        {
            return "Text Reference";
        }
    }

    /// <inheritdoc/>
    object INavigable.GetNavigationTarget() => _selection.Id;
}

#endregion

#region PagedTextAssetRef

/// <summary>
/// A flow node that references a paged text asset from the asset library,
/// providing outputs for a single page, all pages from a starting index, and the total page count.
/// </summary>
[DisplayText("Paged Text Reference")]
[ToolTipsText("Reference paged text asset from asset library.")]
public class PagedTextAssetRef : TextFlowNode, INavigable
{
    private readonly ConnectorAssetProperty<IPagedTextAsset> _selection = new("Text", "Paged Text Asset");
    private readonly ConnectorValueProperty<int> _index = new("Index", "Index");

    private readonly FlowNodeConnector _onePage;
    private readonly FlowNodeConnector _array;
    private readonly FlowNodeConnector _count;

    /// <summary>
    /// Initializes a new instance of the <see cref="PagedTextAssetRef"/> class with no asset selected.
    /// </summary>
    public PagedTextAssetRef()
    {
        _selection.AddConnector(this);
        _index.AddConnector(this);

        _onePage = AddDataOutputConnector("OnePage", "string", "Single Page Output");
        _array = AddDataOutputConnector("Array", "string[]", "Multi Page Output");
        _count = AddDataOutputConnector("Count", "int", "Total Pages");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PagedTextAssetRef"/> class with the specified text asset.
    /// </summary>
    /// <param name="textAsset">The text asset to reference.</param>
    public PagedTextAssetRef(TextAsset textAsset)
        : this()
    {
        _selection.TargetAsset = textAsset;
    }

    //public string Text => _selection.BaseTarget?.GetText() ?? string.Empty;

    /// <inheritdoc/>
    public override Image Icon => _selection.BaseTarget?.ToDisplayIcon() ?? base.Icon;

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _selection.Sync(sync);
        _index.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _selection.InspectorField(setup, this);
        _index.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var asset = _selection.GetTarget(compute, this);
        int index = _index.GetValue(compute, this);

        var diagram = this.Diagram;

        if (diagram.GetIsLinked(_onePage))
        {
            string text = asset?.GetText(index) ?? string.Empty;
            compute.SetValue(_onePage, text ?? string.Empty);
        }

        if (diagram.GetIsLinked(_array))
        {
            List<string> list = [];
            int count = asset?.PageCount ?? 0;

            for (int i = index; i < count; i++)
            {
                string text = asset.GetText(index) ?? string.Empty;
                list.Add(text);
            }

            compute.SetValue(_array, list.ToArray());
        }

        if (diagram.GetIsLinked(_count))
        {
            int count = asset?.PageCount ?? 0;
            compute.SetValue(_count, count);
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (_selection.BaseTarget != null)
        {
            return _selection.BaseTarget.ToDisplayText();
        }
        else
        {
            return "Text Reference";
        }
    }

    /// <inheritdoc/>
    object INavigable.GetNavigationTarget() => _selection.Id;
}

#endregion

#region TextReplace

/// <summary>
/// A flow node that finds a specified substring in the input text and replaces all occurrences with another string.
/// </summary>
[DisplayText("Replace Text")]
[ToolTipsText("Find keywords in text and replace with another text.")]
public class TextReplace : TextFlowNode
{
    private readonly ConnectorTextBlockProperty _input = new("Input", "Input Text");
    private readonly ConnectorStringProperty _findString = new("FindString", "Find Text");
    private readonly ConnectorTextBlockProperty _replace = new("Replace", "Replace Text");
    private readonly FlowNodeConnector _output;


    /// <summary>
    /// Initializes a new instance of the <see cref="TextReplace"/> class.
    /// </summary>
    public TextReplace()
    {
        _input.AddConnector(this);
        _findString.AddConnector(this);
        _replace.AddConnector(this);
        
        _output = AddDataOutputConnector("Output", "*System|String", "Output");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _input.Sync(sync);
        _findString.Sync(sync);
        _replace.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _input.InspectorField(setup, this);
        _findString.InspectorField(setup, this);
        _replace.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        string str = _input.GetText(compute, this) ?? string.Empty;
        string find = _findString.GetValue(compute, this) ?? string.Empty;
        string replace = _replace.GetText(compute, this) ?? string.Empty;

        if (string.IsNullOrWhiteSpace(str) || string.IsNullOrWhiteSpace(find))
        {
            compute.SetValue(_output, str);

            return;
        }

        string result = str.Replace(find, replace);

        compute.SetValue(_output, result);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (!_findString.GetIsLinked(this))
        {
            return $"Replace {_findString.BaseValue}";
        }
        else
        {
            return DisplayText;
        }
    }
}

#endregion

#region MultiTextReplace

/// <summary>
/// A flow node that performs multiple find-and-replace operations on the input text,
/// with dynamically generated input connectors based on a list of keywords to find.
/// </summary>
[DisplayText("Replace Multiple Texts")]
[ToolTipsText("Find multiple keywords in text and replace with other texts respectively.")]
public class MultiTextReplace : TextFlowNode
{
    private ConnectorTextBlockProperty _input = new("Input", "Input Text");
    private FlowNodeConnector _output;

    private readonly ListProperty<string> _findStrings = new("FindStrings", "Find", "Keywords to find and replace. After filling in, ports will be automatically mapped.");
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MultiTextReplace"/> class.
    /// </summary>
    public MultiTextReplace()
    {
        UpdateConnector();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _input.Sync(sync);
        _findStrings.Sync(sync);

        if (sync.IsSetter())
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _input.InspectorField(setup, this);
        _findStrings.InspectorField(setup);
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        _input.AddConnector(this);
        _output = AddDataOutputConnector("Output", "*System|String", "Output");

        var finds = _findStrings.List.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct();
        foreach (var find in finds)
        {
            string text = find;
            if (text.Length > 20)
            {
                text = text.Substring(0, 17) + "...";
            }

            AddDataInputConnector("find-" + find, "string", text);
        }
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        string str = _input.GetText(compute, this);

        if (string.IsNullOrWhiteSpace(str))
        {
            compute.SetValue(_output, str);
            return;
        }

        var builder = new StringBuilder(str);

        var finds = _findStrings.List.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct();
        foreach (var find in finds)
        {
            var conn = GetConnector("find-" + find);
            if (conn is null)
            {
                continue;
            }

            string replace = compute.GetValue<string>(conn);

            builder.Replace(find, replace);
        }

        compute.SetValue(_output, builder.ToString());
    }
}

#endregion

#region EscapeString

/// <summary>
/// A flow node that converts special characters in the input string to their escaped representations using backslashes.
/// </summary>
[DisplayText("Escape String")]
[ToolTipsText("Convert special characters in text to escape sequences with \\.")]
public class EscapeString : TextFlowNode
{
    private readonly FlowNodeConnector _input;
    private readonly FlowNodeConnector _output;

    /// <summary>
    /// Initializes a new instance of the <see cref="EscapeString"/> class.
    /// </summary>
    public EscapeString()
    {
        _input = AddDataInputConnector("Input", "*System|String", "Input");
        _output = AddDataOutputConnector("Output", "*System|String", "Output");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        string s = compute.GetValue(_input) as string;

        s = s.Escape();

        compute.SetValue(_output, s);
    }
}

#endregion

#region UnescapeString

/// <summary>
/// A flow node that converts escape sequences (prefixed with backslash) in the input string back to their original special characters.
/// </summary>
[DisplayText("Unescape String")]
[ToolTipsText("Convert escape sequences with \\ in text back to original special characters.")]
public class UnescapeString : TextFlowNode
{
    private readonly FlowNodeConnector _input;
    private readonly FlowNodeConnector _output;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnescapeString"/> class.
    /// </summary>
    public UnescapeString()
    {
        _input = AddDataInputConnector("Input", "*System|String", "Input");
        _output = AddDataOutputConnector("Output", "*System|String", "Output");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        string s = compute.GetValue(_input) as string;

        s = s.Unescape();

        compute.SetValue(_output, s);
    }
}

#endregion

#region ReplaceBlankString

/// <summary>
/// A flow node that replaces blank (null or whitespace-only) input text with a specified fallback string.
/// </summary>
[SimpleFlowNodeStyle(HasHeader = false)]
[DisplayText("Replace Blank Text")]
[ToolTipsText("If input text is blank, replace with fallback text.")]
public class ReplaceBlankString : TextFlowNode
{
    private readonly ConnectorTextBlockProperty _input = new("Input", "Input Text");
    private readonly ConnectorTextBlockProperty _replace = new("Replace", "Replace Text");
    private readonly FlowNodeConnector _output;


    /// <summary>
    /// Initializes a new instance of the <see cref="ReplaceBlankString"/> class.
    /// </summary>
    public ReplaceBlankString()
    {
        _input.AddConnector(this);
        _replace.AddConnector(this);

        _output = AddDataOutputConnector("Output", "*System|String", "Output");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _input.Sync(sync);
        _replace.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _input.InspectorField(setup, this);
        _replace.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        string str = _input.GetText(compute, this) ?? string.Empty;
        string replace = _replace.GetText(compute, this) ?? string.Empty;

        string result = str;
        if (string.IsNullOrWhiteSpace(str))
        {
            result = replace;
        }

        compute.SetValue(_output, result);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return DisplayText;
    }
}

#endregion

#region JoinString

/// <summary>
/// A flow node that joins multiple input strings into a single string, optionally separated by a configurable separator.
/// Input values are collected and ordered based on the node's Y-axis arrangement in the flow diagram.
/// </summary>
[SimpleFlowNodeStyle(HasHeader = false, Width = 140, Height = 20)]
[DisplayText("Join Strings")]
[ToolTipsText("Join multiple strings in order based on node Y-axis arrangement.")]
public class JoinString : TextFlowNode
{
    private FlowNodeConnector _in;
    private FlowNodeConnector _out;

    private StringProperty _separator = new("Separator", "Separator", ",", toolTips:"Insert separator between strings when combining. Use \\ to escape special characters.");

    /// <summary>
    /// Initializes a new instance of the <see cref="JoinString"/> class.
    /// </summary>
    public JoinString()
    {
        _in = AddConnector("In", "object", FlowDirections.Input, FlowConnectorTypes.Data, true, "Element");
        _out = AddDataOutputConnector("Out", "string", "Joined String");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _separator.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _separator.InspectorField(setup);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var values = compute.GetValues<object>(_in, true).SkipNull().ToArray();

        string sep = _separator.Value.Unescape();
        string s = string.Join(sep, values);

        compute.SetValue(_out, s);
    }
}

#endregion

#region SplitString

/// <summary>
/// A flow node that splits an input string into an array of substrings based on one or more configurable separator characters.
/// </summary>
[SimpleFlowNodeStyle(HasHeader = false, Width = 140, Height = 20)]
[DisplayText("Split String")]
[ToolTipsText("Split string by separators, ordered by node Y-axis arrangement.")]
public class SplitString : TextFlowNode
{
    private FlowNodeConnector _in;
    private FlowNodeConnector _out;

    private ListProperty<string> _separators = new("Separators", "Separator", toolTips: "Separator between strings. Use \\ to escape special characters.");
    private ValueProperty<bool> _keepEmpty = new("KeepEmpty", "Keep Empty Strings", toolTips: "If not kept, remove empty strings from the array.");

    /// <summary>
    /// Initializes a new instance of the <see cref="SplitString"/> class.
    /// </summary>
    public SplitString()
    {
        _in = AddDataInputConnector("In", "string", "Input String");
        _out = AddDataOutputConnector("Out", "string[]", "Split Strings");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _separators.Sync(sync);
        _keepEmpty.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _separators.InspectorField(setup);
        _keepEmpty.InspectorField(setup);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        string input = compute.GetValueConvert<string>(_in);

        char[] seps = _separators.List
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Unescape()[0])
            .ToArray();

        string[] split = input.Split(seps);
        for (int i = 0; i < split.Length; i++)
        {
            split[i] = split[i].Trim();
        }

        bool keepEmpty = _keepEmpty.Value;
        if (!keepEmpty && split.Any(string.IsNullOrWhiteSpace))
        {
            split = split.Where(o => !string.IsNullOrWhiteSpace(o)).ToArray();
        }

        compute.SetValue(_out, split);
    }
}

#endregion

#region IsBlankText

/// <summary>
/// A flow node that checks whether the input string is null, empty, or consists only of white-space characters.
/// </summary>
[DisplayText("Check if Blank Text")]
public class IsBlankText : ValueFlowNode
{
    private readonly FlowNodeConnector _in;
    private readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="IsBlankText"/> class.
    /// </summary>
    public IsBlankText()
    {
        _in = AddDataInputConnector("In", "string", "Text");
        _out = AddDataOutputConnector("Out", "bool", "Is Empty");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        string str = compute.GetValueConvert<string>(_in);

        compute.SetValue(_out, string.IsNullOrWhiteSpace(str));
    }
}

#endregion

#region ObjectToString

/// <summary>
/// A flow node that converts an input object to its string representation.
/// Returns an empty string if the input is null or conversion fails.
/// </summary>
[DisplayText("Convert to String")]
[ToolTipsText("Attempt to convert an object to string. Returns empty string if conversion fails.")]
[SimpleFlowNodeStyle(HasHeader = false, Width = 100, Height = 20)]
public class ObjectToString : TextFlowNode
{
    private readonly FlowNodeConnector _in;
    private readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectToString"/> class.
    /// </summary>
    public ObjectToString()
    {
        _in = AddDataInputConnector("In", "object", "Input");
        _out = AddDataOutputConnector("Out", "string", "String Output");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        object obj = compute.GetValue(_in);

        string s = obj?.ToString() ?? string.Empty;

        compute.SetValue(_out, s);
    }
}

#endregion

#region StringLength

/// <summary>
/// A flow node that calculates and outputs the number of characters in the input string.
/// </summary>
[DisplayText("String Length")]
public class StringLength : TextFlowNode
{
    private readonly FlowNodeConnector _in;
    private readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="StringLength"/> class.
    /// </summary>
    public StringLength()
    {
        _in = AddDataInputConnector("In", "string", "Input");
        _out = AddDataOutputConnector("Out", "int", "Output");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        string s = compute.GetValue<string>(_in) ?? string.Empty;

        compute.SetValue(_out, s.Length);
    }
}

#endregion
