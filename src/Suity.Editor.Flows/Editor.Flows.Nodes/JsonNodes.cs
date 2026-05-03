using MarkedNet;
using Suity.Drawing;
using Suity.Editor.Flows.Gui;
using Suity.Editor.Selecting;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Json;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Drawing;
using System.Linq;

namespace Suity.Editor.Flows.Nodes;

/// <summary>
/// Represents a custom graph data type for JSON content in flow-based visual scripting.
/// Configures the visual appearance of JSON data connections with a cyan color scheme.
/// </summary>
internal class JsonDataType : CustomGraphDataType
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonDataType"/> class,
    /// setting the type name, allowing multiple incoming connections, and
    /// configuring the link and connector visual styling with a cyan color.
    /// </summary>
    public JsonDataType()
    {
        _typeName = JsonFlowNode.JsonData;
        this._allowMultipleToConnection = true;
        this._linkPen = new PenDef(Color.FromArgb(0, 255, 255), 3);
        this._linkArrowBrush = new SolidBrushDef(Color.FromArgb(0, 255, 255));
        this._connectorOutlinePen = new PenDef(Color.FromArgb(0, 255, 255), 3);
        this._connectorFillBrush = new SolidBrushDef(Color.FromArgb(0, 255, 255));
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return "Json Content";
    }
}

#region ExtractJson

/// <summary>
/// A flow node that extracts a JSON object from plain text input.
/// Supports parsing full JSON text as well as the first code block found in Markdown-formatted text.
/// </summary>
[DisplayText("Extract Json Content", "*CoreIcon|Json")]
[ToolTipsText("Try to extract Json object from text. Supports full Json text and the first code block in Markdown text.")]
public class ExtractJson : JsonFlowNode
{
    private readonly FlowNodeConnector _textInput;
    private readonly FlowNodeConnector _dataOutput;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtractJson"/> class,
    /// adding a text input connector and a JSON data output connector.
    /// </summary>
    public ExtractJson()
    {
        _textInput = AddDataInputConnector("Text", "string", "Text");
        _dataOutput = AddDataOutputConnector("Data", JsonData, "Content");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        string s = (compute.GetValue(_textInput) as string)?.Trim();
        if (string.IsNullOrWhiteSpace(s))
        {
            compute.SetValue(_dataOutput, null);
            return;
        }


        if (s.StartsWith("{") && TryParseJson(s, out var reader))
        {
            compute.SetValue(_dataOutput, reader);
            return;
        }

        var lexer = new Lexer(new Options());
        var markdown = "# Title Content";

        // Manually call Lexer to get Token list
        var tokens = Lexer.Lex(markdown, new Options());

        if (tokens is null || tokens.Tokens.Count == 0)
        {
            compute.SetValue(_dataOutput, null);
            return;
        }

        foreach (var token in tokens.Tokens.Where(o => o.Type == "code"))
        {
            string c = token.Text ?? string.Empty;

            if (c.StartsWith("{") && TryParseJson(c, out reader))
            {
                compute.SetValue(_dataOutput, reader);
                return;
            }
        }

        compute.SetValue(_dataOutput, null);
    }

    /// <summary>
    /// Attempts to parse the given string as JSON content.
    /// </summary>
    /// <param name="s">The string to parse as JSON.</param>
    /// <param name="reader">When this method returns, contains the <see cref="JsonDataReader"/> if parsing succeeded; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the string was successfully parsed as JSON; otherwise, <c>false</c>.</returns>
    private bool TryParseJson(string s, out JsonDataReader reader)
    {
        try
        {
            reader = new JsonDataReader(s);

            return true;
        }
        catch (Exception)
        {
            reader = null;

            return false;
        }
    }
}

#endregion

#region JsonConvert

/// <summary>
/// A flow node that converts JSON content to a specific data type,
/// such as string, numeric, boolean, or retains it as raw JSON content.
/// </summary>
[DisplayText("Convert Json Content", "*CoreIcon|Json")]
[ToolTipsText("Try to convert Json object to specific type values or objects.")]
public class JsonConvert : JsonFlowNode
{
    private FlowNodeConnector _content;
    private FlowNodeConnector _out;

    readonly ValueProperty<JsonContentTypes> _contentType = new("ContentType", "Convert Type");

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonConvert"/> class
    /// and updates the input/output connectors based on the default content type.
    /// </summary>
    public JsonConvert()
    {
        UpdateConnector();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _contentType.Sync(sync);

        if (sync.IsSetter())
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    public override void SetupView(IViewObjectSetup setup)
    {
        base.SetupView(setup);

        _contentType.InspectorField(setup);
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        base.OnUpdateConnector();

        _content = AddDataInputConnector("Content", JsonData, "Content");
        _out = AddDataOutputConnector("Out", GetJsonDataType(_contentType.Value), "Output");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var reader = compute.GetValue(_content) as IDataReader;

        switch (_contentType.Value)
        {
            case JsonContentTypes.String:
                compute.SetValue(_out, reader?.ReadString());
                break;

            case JsonContentTypes.Numeric:
                compute.SetValue(_out, reader?.ReadSingle() ?? 0);
                break;

            case JsonContentTypes.Boolean:
                compute.SetValue(_out, reader?.ReadBoolean() ?? false);
                break;

            case JsonContentTypes.Content:
            default:
                compute.SetValue(_out, reader);
                break;
        }
    }
}

#endregion

#region JsonToString

/// <summary>
/// A flow node that converts a JSON object into its plain text string representation.
/// </summary>
[DisplayText("Json to Text", "*CoreIcon|Json")]
[ToolTipsText("Convert a Json object to plain text.")]
public class JsonToString : JsonFlowNode
{
    private FlowNodeConnector _content;
    private FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonToString"/> class,
    /// adding a JSON data input connector and a string output connector.
    /// </summary>
    public JsonToString()
    {
        _content = AddDataInputConnector("Content", JsonData, "Content");
        _out = AddDataOutputConnector("Out", "string", "Output");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var reader = compute.GetValue(_content) as IDataReader;

        compute.SetValue(_out, reader?.ToString() ?? string.Empty);
    }
}

#endregion

#region GetJsonField

/// <summary>
/// A flow node that retrieves a specific field from a JSON object
/// and returns the value cast to the configured field type (string, numeric, boolean, or raw JSON content).
/// </summary>
[DisplayText("Get Json Field", "*CoreIcon|Json")]
[ToolTipsText("Get field content from a Json object, and retrieve the specified type value by specifying the field type.")]
[NativeAlias("Suity.Editor.Flows.Nodes.JsonField")]
public class GetJsonField : JsonFlowNode
{
    private FlowNodeConnector _content;
    private FlowNodeConnector _out;

    readonly ValueProperty<JsonContentTypes> _contentType = new("ContentType", "Field Type");
    readonly ConnectorStringProperty _fieldName = new("FieldName", "Field Name");

    /// <summary>
    /// Initializes a new instance of the <see cref="GetJsonField"/> class
    /// and updates the connectors based on the default field configuration.
    /// </summary>
    public GetJsonField()
    {
        UpdateConnector();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _fieldName.Sync(sync);
        _contentType.Sync(sync);

        if (sync.IsSetter())
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    public override void SetupView(IViewObjectSetup setup)
    {
        base.SetupView(setup);

        _fieldName.InspectorField(setup, this);
        _contentType.InspectorField(setup);
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        base.OnUpdateConnector();

        _content = AddDataInputConnector("Content", JsonData, "Content");
        _out = AddDataOutputConnector("Out", GetJsonDataType(_contentType.Value), "Output");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        string fieldName = _fieldName.GetValue(compute, this);

        if (string.IsNullOrWhiteSpace(fieldName))
        {
            compute.SetValue(_out, null);
            return;
        }

        var reader = compute.GetValue(_content) as IDataReader;
        var childReader = reader?.Node(fieldName);

        switch (_contentType.Value)
        {
            case JsonContentTypes.String:
                compute.SetValue(_out, childReader?.ReadString());
                break;

            case JsonContentTypes.Numeric:
                compute.SetValue(_out, childReader?.ReadSingle() ?? 0);
                break;

            case JsonContentTypes.Boolean:
                compute.SetValue(_out, childReader?.ReadBoolean() ?? false);
                break;

            case JsonContentTypes.Content:
            default:
                compute.SetValue(_out, childReader);
                break;
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (!_fieldName.GetIsLinked(this))
        {
            return $"Get {_fieldName.BaseValue} Field Content";
        }
        else
        {
            return DisplayText;
        }
    }
}

#endregion

#region JsonToSObject

/// <summary>
/// A flow node that parses a JSON object into an editor SObject (structured object)
/// based on a specified DStruct type definition.
/// </summary>
[DisplayText("Json to SObject", "*CoreIcon|Json")]
[ToolTipsText("Parse Json object to editor object.")]
public class JsonToSObject : JsonFlowNode
{
    private FlowNodeConnector _jsonInput;
    private FlowNodeConnector _objOutput;

    private readonly AssetProperty<DStruct> _structType = new("Struct", "Structure");

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonToSObject"/> class,
    /// subscribing to struct type selection changes and updating connectors accordingly.
    /// </summary>
    public JsonToSObject()
    {
        _structType.SelectionChanged += (s, e) => UpdateConnector();
        _structType.ListenEnabled = true;

        UpdateConnector();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _structType.Sync(sync);
        if (sync.IsSetter())
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _structType.InspectorField(setup);
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        string structType = _structType.Target?.Definition?.ToTypeName() ?? FlowNode.UNKNOWN_TYPE;

        _jsonInput = AddDataInputConnector("Input", JsonData, "Input");
        _objOutput = AddDataOutputConnector("Output", structType, "Output");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var reader = compute.GetValue(_jsonInput) as IDataReader;
        if (reader is null)
        {
            compute.SetValue(_objOutput, null);
            return;
        }

        try
        {
            var obj = reader.ReadObject();
            if (obj is null)
            {
                compute.SetValue(_objOutput, null);
                return;
            }

            var s = _structType.Target;
            if (s is null)
            {
                compute.SetValue(_objOutput, null);
                return;
            }

            var result = EditorServices.JsonResource.FromJson(obj, new() { TypeHint = s.Definition });

            compute.SetValue(_objOutput, result);
        }
        catch (Exception)
        {
            compute.SetValue(_objOutput, null);
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Json to {_structType.Target?.ToDisplayText()}";
    }
}

#endregion
