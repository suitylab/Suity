using Suity.Editor.AIGC.Helpers;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Linq;

namespace Suity.Editor.Flows.AIGC;

/// <summary>
/// Base class for XML-related AIGC flow nodes.
/// </summary>
[DisplayText("Xml Tag", "*CoreIcon|Tag")]
[ToolTipsText("Xml related nodes")]
public abstract class AigcXmlNode : FlowNode
{

}

#region ExtractXmlsTag

/// <summary>
/// Node that extracts XML tags from text and outputs them as data connectors.
/// </summary>
[DisplayText("Extract Xml Tag", "*CoreIcon|Tag")]
[NativeAlias("Suity.Editor.AIGC.Flows.ExtractXmlsTagNode")]
[NativeAlias("Suity.Editor.Flows.AIGC.ExtractXmlsTagNode")]
public class ExtractXmlsTag : AigcXmlNode
{
    readonly ListProperty<string> _tagNames = new("TagNames", "Tag Names");
    readonly ValueProperty<bool> _isArray = new("IsArray", "Output as Array", false);

    private FlowNodeConnector _in;
    private FlowNodeConnector _textIn;

    private FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtractXmlsTag"/> class.
    /// </summary>
    public ExtractXmlsTag()
    {
        _tagNames.ValueChanged += (s, e) => UpdateConnectorQueued();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _tagNames.Sync(sync);
        _isArray.Sync(sync);

        if (sync.IsSetterOf(_isArray.Property.Name))
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _tagNames.InspectorField(setup);
        _isArray.InspectorField(setup);
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        _in = this.AddActionInputConnector("In", "Input");
        _textIn = this.AddDataInputConnector("DataIn", "string", "Text Input");

        _out = this.AddActionOutputConnector("Out", "Output");

        var tagNames = _tagNames.List
            .Where(NamingVerifier.VerifyXmlTagName)
            .Distinct()
            .ToArray();

        TypeDefinition tagType;

        tagType = TypeDefinition.FromNative<LooseXmlTag>();
        if (_isArray.Value)
        {
            tagType = tagType.MakeArrayType();
        }

        foreach (var tagName in tagNames)
        {
            this.AddDataOutputConnector("Tag-" + tagName, tagType, tagName);
        }
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var tagNames = _tagNames.List
            .Where(s => NamingVerifier.VerifyXmlTagName(s))
            .Distinct()
            .ToArray();

        bool isArray = _isArray.Value;

        string text = compute.GetValue<string>(_textIn);
        if (string.IsNullOrWhiteSpace(text))
        {
            foreach (var tagName in tagNames)
            {
                var c = GetConnector("Tag-" + tagName);
                if (c is null) continue;

                if (isArray)
                {
                    compute.SetValue(c, Array.Empty<LooseXmlTag>());
                }
                else
                {
                    compute.SetValue(c, null);
                }
            }
        }
        else
        {
            foreach (var tagName in tagNames)
            {
                var c = GetConnector("Tag-" + tagName);
                if (c is null) continue;

                try
                {
                    var tags = LooseXml.ExtractNodes(text, tagName);

                    if (isArray)
                    {
                        compute.SetValue(c, tags);
                    }
                    else
                    {
                        compute.SetValue(c, tags.FirstOrDefault());
                    }
                }
                catch (Exception)
                {
                    compute.SetValue(c, Array.Empty<LooseXmlTag>());
                }
            }
        }

        compute.SetResult(this, _out);
    }
}
#endregion

#region ExtractXmlsTagAction

/// <summary>
/// Node that extracts XML tags from text and routes execution to the matching tag's action connector.
/// </summary>
[DisplayText("Extract Xml Tag Action", "*CoreIcon|Tag")]
[NativeAlias("Suity.Editor.AIGC.Flows.ExtractXmlsTagAction")]
[NativeAlias("Suity.Editor.Flows.AIGC.ExtractXmlsTagAction")]
public class ExtractXmlsTagAction : AigcXmlNode
{
    readonly ListProperty<string> _tagNames = new("TagNames", "Tag Names");
    readonly ValueProperty<bool> _isArray = new("IsArray", "Output as Array", false);

    private FlowNodeConnector _in;
    private FlowNodeConnector _textIn;

    private FlowNodeConnector _noResult;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtractXmlsTagAction"/> class.
    /// </summary>
    public ExtractXmlsTagAction()
    {
        _tagNames.ValueChanged += (s, e) => UpdateConnectorQueued();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _tagNames.Sync(sync);
        _isArray.Sync(sync);

        if (sync.IsSetterOf(_isArray.Property.Name))
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _tagNames.InspectorField(setup);
        _isArray.InspectorField(setup);
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        _in = this.AddActionInputConnector("In", "Input");
        _textIn = this.AddDataInputConnector("DataIn", "string", "Text Input");

        var tagNames = _tagNames.List
            .Where(NamingVerifier.VerifyXmlTagName)
            .Distinct()
            .ToArray();

        var tagType = TypeDefinition.FromNative<LooseXmlTag>();
        if (_isArray.Value)
        {
            tagType = tagType.MakeArrayType();
        }

        foreach (var tagName in tagNames)
        {
            this.AddConnector("Tag-" + tagName, tagType, FlowDirections.Output, FlowConnectorTypes.Action, false, tagName);
        }

        _noResult = this.AddActionOutputConnector("NoResult", "No Result");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var tagNames = _tagNames.List
            .Where(NamingVerifier.VerifyXmlTagName)
            .Distinct()
            .ToArray();

        bool isArray = _isArray.Value;

        string text = compute.GetValue<string>(_textIn);
        if (string.IsNullOrWhiteSpace(text))
        {
            foreach (var tagName in tagNames)
            {
                var c = GetConnector("Tag-" + tagName);
                if (c is null) continue;

                if (isArray)
                {
                    compute.SetValue(c, Array.Empty<LooseXmlTag>());
                }
                else
                {
                    compute.SetValue(c, null);
                }
            }
        }
        else
        {
            foreach (var tagName in tagNames)
            {
                var c = GetConnector("Tag-" + tagName);
                if (c is null) continue;

                try
                {
                    var tags = LooseXml.ExtractNodes(text, tagName);
                    if (tags.Length > 0)
                    {
                        if (isArray)
                        {
                            compute.SetValue(c, tags);
                        }
                        else
                        {
                            compute.SetValue(c, tags.FirstOrDefault());
                        }
                        compute.SetResult(this, c);
                        return;
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

        compute.SetResult(this, _noResult);
    }
}
#endregion

#region GetXmlTagAttributes

/// <summary>
/// Node that retrieves specified attributes from an XML tag.
/// </summary>
[SimpleFlowNodeStyle(HasHeader = false)]
[DisplayText("Get Xml Attribute", "*CoreIcon|Tag")]
[NativeAlias("Suity.Editor.AIGC.Flows.GetXmlTagAttributesNode")]
[NativeAlias("Suity.Editor.Flows.AIGC.GetXmlTagAttributesNode")]
public class GetXmlTagAttributes : AigcXmlNode
{
    readonly ListProperty<string> _attributeNames = new("AttributeNames", "Attribute Names");

    private FlowNodeConnector _tagIn;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetXmlTagAttributes"/> class.
    /// </summary>
    public GetXmlTagAttributes()
    {
        _attributeNames.ValueChanged += (s, e) => UpdateConnectorQueued();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);
        _attributeNames.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);
        _attributeNames.InspectorField(setup);
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        var tagType = TypeDefinition.FromNative<LooseXmlTag>();

        _tagIn = this.AddDataInputConnector("TagIn", tagType, "Tag Input");

        var attrNames = _attributeNames.List
            .Where(s => NamingVerifier.VerifyXmlAttributeName(s))
            .Distinct()
            .ToArray();

        foreach (var attrName in attrNames)
        {
            this.AddDataOutputConnector("Attr-" + attrName, "string", attrName);
        }
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var tag = compute.GetValue<LooseXmlTag>(_tagIn);

        var attrNames = _attributeNames.List
            .Where(s => NamingVerifier.VerifyXmlAttributeName(s))
            .Distinct()
            .ToArray();

        if (tag is null)
        {
            foreach (var attrName in attrNames)
            {
                var c = GetConnector("Attr-" + attrName);
                if (c is null) continue;

                compute.SetValue(c, string.Empty);
            }
        }
        else
        {
            foreach (var tagName in attrNames)
            {
                var c = GetConnector("Attr-" + tagName);
                if (c is null) continue;

                string attr = tag.GetAttribute(tagName);
                compute.SetValue(c, attr);
            }
        }
    }
}
#endregion

#region SetXmlTagAttributes

/// <summary>
/// Node that sets specified attributes on an XML tag.
/// </summary>
[DisplayText("Set Xml Attribute", "*CoreIcon|Tag")]
[NativeAlias("Suity.Editor.AIGC.Flows.SetXmlTagAttributesNode")]
[NativeAlias("Suity.Editor.Flows.AIGC.SetXmlTagAttributesNode")]
public class SetXmlTagAttributes : AigcXmlNode
{
    readonly ListProperty<string> _attributeNames = new("AttributeNames", "Attribute Names");

    private FlowNodeConnector _in;
    private FlowNodeConnector _tagIn;

    private FlowNodeConnector _out;
    private FlowNodeConnector _tagOut;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetXmlTagAttributes"/> class.
    /// </summary>
    public SetXmlTagAttributes()
    {
        _attributeNames.ValueChanged += (s, e) => UpdateConnectorQueued();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);
        _attributeNames.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);
        _attributeNames.InspectorField(setup);
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        // Action Input/Output
        _in = this.AddActionInputConnector("In", "Input");
        _out = this.AddActionOutputConnector("Out", "Output");

        // Data Input: Tag Object
        var tagType = TypeDefinition.FromNative<LooseXmlTag>();
        _tagIn = this.AddDataInputConnector("TagIn", tagType, "Tag Input");

        var attrNames = _attributeNames.List
            .Where(s => NamingVerifier.VerifyXmlAttributeName(s))
            .Distinct()
            .ToArray();

        foreach (var attrName in attrNames)
        {
            this.AddDataInputConnector("Attr-" + attrName, "string", attrName);
        }

        // Data Output: Modified Tag
        _tagOut = this.AddDataOutputConnector("TagOut", tagType, "Tag Output");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        // Get input data
        var tag = compute.GetValue<LooseXmlTag>(_tagIn);
        if (tag is null)
        {
            compute.SetResult(this, _out);
            return;
        }

        var attrNames = _attributeNames.List
            .Where(s => NamingVerifier.VerifyXmlAttributeName(s))
            .Distinct()
            .ToArray();

        foreach (var tagName in attrNames)
        {
            var c = GetConnector("Attr-" + tagName);
            if (c is null) continue;

            string attr = compute.GetValue<string>(c);
            tag.SetAttribute(tagName, attr);
        }

// Trigger subsequent action nodes
        compute.SetResult(this, _out);
    }
}
#endregion

#region GetXmlTagContent

/// <summary>
/// Node that retrieves the inner text content from an XML tag.
/// </summary>
[SimpleFlowNodeStyle(HasHeader = false, Width = 100, Height = 20)]
[DisplayText("Get Xml Content", "*CoreIcon|Tag")]
[NativeAlias("Suity.Editor.AIGC.Flows.GetXmlTagContentNode")]
[NativeAlias("Suity.Editor.Flows.AIGC.GetXmlTagContentNode")]
public class GetXmlTagContent : AigcXmlNode
{
    readonly private FlowNodeConnector _tagIn;
    readonly private FlowNodeConnector _contentOut;
    readonly private ValueProperty<bool> _trim = new("Trim", "Trim Content", false, "Whether to trim the content text.");

    /// <summary>
    /// Initializes a new instance of the <see cref="GetXmlTagContent"/> class.
    /// </summary>
    public GetXmlTagContent()
    {
        var tagType = TypeDefinition.FromNative<LooseXmlTag>();

        _tagIn = this.AddDataInputConnector("TagIn", tagType, " ");
        _contentOut = this.AddDataOutputConnector("ContentOut", "string", " ");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _trim.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _trim.InspectorField(setup);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var tag = compute.GetValue<LooseXmlTag>(_tagIn);

        if (tag is null)
        {
            compute.SetValue(_contentOut, string.Empty);
            return;
        }

        string content = tag.InnerText ?? string.Empty;
        if (_trim.Value)
        {
            content = content.Trim();
        }

        compute.SetValue(_contentOut, content);
    }
}
#endregion

#region SetXmlTagContent

/// <summary>
/// Node that sets the inner text content of an XML tag.
/// </summary>
[DisplayText("Set Xml Content", "*CoreIcon|Tag")]
[NativeAlias("Suity.Editor.AIGC.Flows.SetXmlTagContentNode")]
[NativeAlias("Suity.Editor.Flows.AIGC.SetXmlTagContentNode")]
public class SetXmlTagContent : AigcXmlNode
{
    readonly private FlowNodeConnector _in;
    readonly private FlowNodeConnector _tagIn;
    readonly private FlowNodeConnector _contentIn;

    readonly private FlowNodeConnector _out;
    readonly private FlowNodeConnector _tagOut;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetXmlTagContent"/> class.
    /// </summary>
    public SetXmlTagContent()
    {
        // Action Input/Output
        _in = this.AddActionInputConnector("In", "Input");
        _out = this.AddActionOutputConnector("Out", "Output");

        // Data Input: Tag Object
        var tagType = TypeDefinition.FromNative<LooseXmlTag>();
        _tagIn = this.AddDataInputConnector("TagIn", tagType, "Tag Input");

        // Data Input: Content Value
        _contentIn = this.AddDataInputConnector("ContentIn", "string", "Tag Content");

        // Data Output: Modified Tag
        _tagOut = this.AddDataOutputConnector("TagOut", tagType, "Tag Output");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        // Get input data
        var tag = compute.GetValue<LooseXmlTag>(_tagIn);
        string content = compute.GetValue<string>(_contentIn);

        // Execute set logic
        if (tag != null)
        {
            // Set tag's text content
            tag.InnerText = content ?? string.Empty;
        }

        // Set output data (pass original object reference to support chaining)
        compute.SetValue(_tagOut, tag);

        // Trigger subsequent action nodes
        compute.SetResult(this, _out);
    }
}
#endregion

#region GetXmlTagContents

/// <summary>
/// Node that retrieves the inner text content from multiple XML tags.
/// </summary>
[SimpleFlowNodeStyle(HasHeader = false, Width = 100, Height = 20)]
[DisplayText("Get Xml Contents", "*CoreIcon|Tag")]
public class GetXmlTagContents : AigcXmlNode
{
    readonly private FlowNodeConnector _tagsIn;
    readonly private FlowNodeConnector _contentsOut;
    readonly private ValueProperty<bool> _trim = new("Trim", "Trim Content", false, "Whether to trim the content text.");

    /// <summary>
    /// Initializes a new instance of the <see cref="GetXmlTagContents"/> class.
    /// </summary>
    public GetXmlTagContents()
    {
        var tagType = TypeDefinition.FromNative<LooseXmlTag>().MakeArrayType();

        _tagsIn = this.AddDataInputConnector("TagsIn", tagType, " ");
        _contentsOut = this.AddDataOutputConnector("ContentsOut", "string[]", " ");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _trim.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _trim.InspectorField(setup);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var tags = compute.GetValue<LooseXmlTag[]>(_tagsIn);

        if (tags is null || tags.Length == 0)
        {
            compute.SetValue(_contentsOut, Array.Empty<string>());
            return;
        }

        string GetContent(LooseXmlTag t)
        {
            string content = t.InnerText ?? string.Empty;
            if (_trim.Value)
            {
                content = content.Trim();
            }
            return content;
        }

        string[] contents = tags.Select(GetContent).ToArray();

        compute.SetValue(_contentsOut, contents);
    }
}
#endregion

#region GetXmlTagContent

/// <summary>
/// Action Node that retrieves the inner text content from an XML tag.
/// </summary>
[SimpleFlowNodeStyle(HasHeader = false, Width = 100, Height = 20)]
[DisplayText("Get Xml Content Action", "*CoreIcon|Tag")]
public class GetXmlTagContentAction : AigcXmlNode
{
    readonly private FlowNodeConnector _tagIn;
    readonly private FlowNodeConnector _contentOut;
    readonly private ValueProperty<bool> _trim = new("Trim", "Trim Content", false, "Whether to trim the content text.");

    /// <summary>
    /// Initializes a new instance of the <see cref="GetXmlTagContentAction"/> class.
    /// </summary>
    public GetXmlTagContentAction()
    {
        var tagType = TypeDefinition.FromNative<LooseXmlTag>();

        _tagIn = this.AddConnector("TagIn", tagType, FlowDirections.Input, FlowConnectorTypes.Action);
            //this.AddDataInputConnector("TagIn", tagType, " ");
        _contentOut = this.AddConnector("ContentOut", "string", FlowDirections.Output, FlowConnectorTypes.Action);
            //this.AddDataOutputConnector("ContentOut", "string", " ");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _trim.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _trim.InspectorField(setup);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var tag = compute.GetValue<LooseXmlTag>(_tagIn);

        if (tag is null)
        {
            compute.SetValue(_contentOut, string.Empty);
            compute.SetResult(this, _contentOut);
            return;
        }

        string content = tag.InnerText ?? string.Empty;
        if (_trim.Value)
        {
            content = content.Trim();
        }

        compute.SetValue(_contentOut, content);
        compute.SetResult(this, _contentOut);
    }
}
#endregion