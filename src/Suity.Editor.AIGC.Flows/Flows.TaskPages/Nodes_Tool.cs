using ComputerBeacon.Json;
using Suity.Collections;
using Suity.Drawing;
using Suity.Editor.AIGC;
using Suity.Editor.AIGC.Helpers;
using Suity.Editor.AIGC.TaskPages;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Selecting;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Im;
using Suity.Views.Im.Flows;
using Suity.Views.Named;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Editor.Flows.TaskPages;

#region PageDefinitionReference

/// <summary>
/// Provides a reference to a page definition asset and outputs it as data.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.Page, HasHeader = false, Width = 100, Height = 20)]
[DisplayText("Page Resource Reference", "*CoreIcon|Page")]
[DisplayOrder(4950)]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.PageDefinitionRefNode")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.PageDefinitionReference")]
public class PageDefinitionReference : TaskPageNode
{
    readonly AssetProperty<SubFlowDefinitionAsset> _page = new("Page", "Page");
    readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageDefinitionReference"/> class.
    /// </summary>
    public PageDefinitionReference()
    {
        var type = AssetManager.Instance.GetAssetLink<SubFlowDefinitionAsset>().Definition;
        _out = this.AddDataOutputConnector("Out", type);

        base.FlowNodeGui = OnGui;
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);
        _page.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);
        _page.InspectorField(setup);
    }

    private ImGuiNode OnGui(ImGui gui, IDrawNodeContext context)
    {
        string text = EditorUtility.GetBriefStringL(_page.Target);
        if (string.IsNullOrEmpty(text))
        {
            // Ensure there is a space for layout placeholder.
            text = " ";
        }

        return gui.FlowSingleConnectorFrame(_out, context, text);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        compute.SetValue(_out, _page.Target);
    }

    /// <inheritdoc/>
    public override string DisplayText => _page.Target?.ToString() ?? base.Name;
}

#endregion

#region GetSelfPageDefinition

/// <summary>
/// Retrieves the definition of the current (self) page in the flow context.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.Page, HasHeader = false, Width = 100, Height = 20)]
[DisplayText("Get Self Page Definition", "*CoreIcon|Page")]
[DisplayOrder(4960)]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.GetSelfPageNode")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.GetSelfPageDefinition")]
public class GetSelfPageDefinition : TaskPageNode
{
    readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSelfPageDefinition"/> class.
    /// </summary>
    public GetSelfPageDefinition()
    {
        var type = TypeDefinition.FromAssetLink<IAigcToolAsset>();

        _out = this.AddDataOutputConnector("SelfPage", type, "Self Page Definition");
    }

    /// <inheritdoc/>
    public override ImageDef Icon => CoreIconCache.Page;

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var caller = compute.Context.GetArgument<IFlowCallerContext>();

        var asset = caller?.GetDefinitionPage();

        compute.SetValue(_out, asset);
    }
}

#endregion

#region GetCurrentToolList

/// <summary>
/// Retrieves the list of available tool pages in the current context.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.ToolBG, HasHeader = false, Width = 100, Height = 20)]
[DisplayText("Get Current Tool List", "*CoreIcon|Tool")]
[DisplayOrder(4940)]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.GetPageToolPagesNode")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.GetSelfToolPagesNode")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.GetCurrentToolListNode")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.GetCurrentToolList")]
public class GetCurrentToolList : TaskPageNode
{
    readonly ConnectorValueProperty<bool> _documentTools = new("DocumentTools", "Document Tools", false, "Whether to include tools defined in the main document.");
    readonly FlowNodeConnector _toolPages;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCurrentToolList"/> class.
    /// </summary>
    public GetCurrentToolList()
    {
        var type = AssetManager.Instance.GetAssetLink<IAigcToolAsset>().Definition.MakeArrayType();

        _documentTools.AddConnector(this);
        _toolPages = this.AddDataOutputConnector("SelfTools", type, "Self Tool List");
    }

    /// <inheritdoc/>
    public override ImageDef Icon => CoreIconCache.Page;

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _documentTools.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _documentTools.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        bool documentTools = _documentTools.GetValue(compute, this);

        var service = compute.Context.GetArgument<IAigcTaskPage>();
        var toolPages = service?.GetToolList(documentTools).ToArray() ?? [];

        compute.SetValue(_toolPages, toolPages);
    }
}

#endregion

#region SetPageTitle

/// <summary>
/// Sets the title of the current page in the flow context.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.Page)]
[DisplayText("Set Page Title", "*CoreIcon|Page")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.SetPageTitleNode")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.SetPageTitle")]
public class SetPageTitle : TaskPageNode
{
    readonly FlowNodeConnector _in;
    readonly ConnectorStringProperty _title = new("Title", "Title", "");

    readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetPageTitle"/> class.
    /// </summary>
    public SetPageTitle()
    {
        _in = this.AddActionInputConnector("In", "Input");
        _out = this.AddActionOutputConnector("Out", "Output");

        _title.AddConnector(this);
    }

    /// <inheritdoc/>
    public override ImageDef Icon => CoreIconCache.Page;

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _title.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _title.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        base.Compute(compute);

        var title = _title.GetValue(compute, this);
        var caller = compute.Context.GetArgument<IFlowCallerContext>();
        caller.Title = title;

        compute.SetResult(this, _out);
    }
}

#endregion

#region ParseToolCalling

/// <summary>
/// Parses a tool calling object from incoming JSON text and a list of available tool pages.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.ToolBG)]
[DisplayText("Parse Tool Calling", "*CoreIcon|Tool")]
[DisplayOrder(3000)]
[ToolTipsText("Parse the calling object from the incoming JSON text and tool page list.")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.ParseToolCallingNode")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.ParseToolCalling")]
public class ParseToolCalling : TaskPageNode
{
    readonly FlowNodeConnector _in;
    readonly FlowNodeConnector _toolPages;
    readonly FlowNodeConnector _toolName;
    readonly FlowNodeConnector _toolJson;

    readonly FlowNodeConnector _outPageInstance;
    readonly FlowNodeConnector _outNoResult;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParseToolCalling"/> class, setting up input and output connectors for parsing tool calling data.
    /// </summary>
    public ParseToolCalling()
    {
        var pageType = AssetManager.Instance.GetAssetLink<IAigcToolAsset>().Definition.MakeArrayType();

        _in = this.AddActionInputConnector("In", "Input");
        _toolPages = this.AddDataInputConnector("ToolPages", pageType, "Tool Page List");
        _toolName = this.AddDataInputConnector("ToolName", "string", "Tool Name");
        _toolJson = this.AddDataInputConnector("ToolJson", "string", "Tool Json");

        var instanceType = TypeDefinition.FromNative<IAigcPageInstance>();

        _outPageInstance = this.AddConnector("PageInstance", instanceType, FlowDirections.Output, FlowConnectorTypes.Action, false, "Page Instance");
        _outNoResult = this.AddActionOutputConnector("NoResult", "No Result");
    }

    /// <inheritdoc/>
    public override ImageDef Icon => CoreIconCache.Tool;

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var toolPages = compute.GetValues<IAigcToolAsset>(_toolPages, true) ?? [];
        string toolName = compute.GetValue<string>(_toolName);
        string toolJson = compute.GetValue<string>(_toolJson);

        // 2. Call the extraction function
        var pageInstance = ParseToolCalling.CreatePageInstance(toolPages, toolName, toolJson);

        // 3. Set output based on result
        if (pageInstance != null)
        {
            compute.SetValue(_outPageInstance, pageInstance);
            compute.SetResult(this, _outPageInstance);
        }
        else
        {
            compute.SetResult(this, _outNoResult);
        }
    }

    /// <summary>
    /// Creates a page instance based on page definition, name and JSON data.
    /// </summary>
    /// <param name="toolPages">Page definition asset list.</param>
    /// <param name="toolName">Target page name.</param>
    /// <param name="toolJson">Configuration JSON string.</param>
    /// <returns>Successfully created AigcPageInstance, null if failed.</returns>
    public static IAigcPageInstance CreatePageInstance(IAigcToolAsset[] toolPages, string toolName, string toolJson)
    {
        // 1. Basic parameter validation
        if (toolPages == null || toolPages.Length == 0 || string.IsNullOrWhiteSpace(toolName) || string.IsNullOrWhiteSpace(toolJson))
        {
            return null;
        }

        // 2. Find the corresponding ToolPage
        var pageDefAsset = toolPages.FirstOrDefault(o => o?.Name == toolName);
        if (pageDefAsset is null)
        {
            return null;
        }

        // 3. Parse JSON
        JsonObject jobj = null;
        try
        {
            jobj = ComputerBeacon.Json.Parser.Parse(toolJson) as JsonObject;
        }
        catch (Exception)
        {
            // Return null on parse failure
            return null;
        }

        if (jobj is null)
        {
            return null;
        }

        // 4. Create instance and assign values
        try
        {
            var option = new PageElementOption
            {
                Mode = PageElementMode.Function,
            };

            var pageInstance = pageDefAsset.CreatePageInstance(option);
            var simpleType = pageInstance.ToSimpleType();

            var dic = EditorServices.JsonResource.FromJson(jobj, simpleType);
            if (dic is null)
            {
                return null;
            }

            foreach (var pair in dic)
            {
                var value = SItem.ResolveValue(pair.Value);
                pageInstance.SetParameter(pair.Key, value);
            }

            return pageInstance;
        }
        catch (Exception)
        {
            // Return null on runtime error (original logic threw, but returning null is more convenient for callers)
            return null;
        }
    }

}

#endregion

#region ParseTagToolCalling

/// <summary>
/// Parses a tool calling object from an XML tag and a list of available tool pages.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.ToolBG)]
[DisplayText("Parse Xml Tag Tool Calling", "*CoreIcon|Tool")]
[DisplayOrder(2950)]
[ToolTipsText("Parse the calling object from the incoming JSON text and tool page list.")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.ParseTagToolCallingNode")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.ParseTagToolCalling")]
public class ParseTagToolCalling : TaskPageNode
{
    readonly FlowNodeConnector _in;
    readonly FlowNodeConnector _toolPages;
    readonly FlowNodeConnector _xmlTag;
    readonly ConnectorStringProperty _attributeName = new("AttributeName", "Attribute Name", "name", "The attribute name on the Xml tag that represents the tool name.");

    readonly FlowNodeConnector _outPageInstance;
    readonly FlowNodeConnector _outNoResult;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParseTagToolCalling"/> class, setting up input and output connectors for parsing XML tag tool calling data.
    /// </summary>
    public ParseTagToolCalling()
    {
        var pageType = AssetManager.Instance.GetAssetLink<IAigcToolAsset>().Definition.MakeArrayType();
        var tagType = TypeDefinition.FromNative<LooseXmlTag>();

        _in = this.AddActionInputConnector("In", "Input");
        _toolPages = this.AddDataInputConnector("ToolPages", pageType, "Tool Page List");
        _xmlTag = this.AddDataInputConnector("XmlTag", tagType, "Xml Tag");
        _attributeName.AddConnector(this);

        var instanceType = TypeDefinition.FromNative<IAigcPageInstance>();

        _outPageInstance = this.AddConnector("PageInstance", instanceType, FlowDirections.Output, FlowConnectorTypes.Action, false, "Page Instance");
        _outNoResult = this.AddActionOutputConnector("NoResult", "No Result");
    }

    /// <inheritdoc/>
    public override ImageDef Icon => CoreIconCache.Tool;

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _attributeName.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _attributeName.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        // 1. Get input data
        var toolPages = compute.GetValues<IAigcToolAsset>(_toolPages, true) ?? [];
        var tag = compute.GetValue<LooseXmlTag>(_xmlTag);
        string attributeName = _attributeName.GetValue(compute, this);

        string toolJson = tag?.InnerText;
        string toolName = tag?.GetAttribute(attributeName);

        // 2. Call the extraction function
        var pageInstance = ParseToolCalling.CreatePageInstance(toolPages, toolName, toolJson);

        // 3. Set output based on result
        if (pageInstance != null)
        {
            compute.SetValue(_outPageInstance, pageInstance);
            compute.SetResult(this, _outPageInstance);
        }
        else
        {
            compute.SetResult(this, _outNoResult);
        }
    }

}

#endregion

#region CreateToolCalling

/// <summary>
/// Creates an empty tool calling object based on a page definition.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.ToolBG)]
[DisplayText("Create Tool Calling", "*CoreIcon|Tool")]
[DisplayOrder(2960)]
[ToolTipsText("Create an empty tool calling object based on the page definition.")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.CreateEmptyToolCallingNode")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.CreateToolCallingNode")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.CreateToolCalling")]
public class CreateToolCalling : TaskPageNode
{
    readonly FlowNodeConnector _in;
    readonly FlowNodeConnector _toolPage;

    readonly FlowNodeConnector _out;
    readonly FlowNodeConnector _pageInstance;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateToolCalling"/> class, setting up input and output connectors for creating tool calling objects.
    /// </summary>
    public CreateToolCalling()
    {
        var pageType = AssetManager.Instance.GetAssetLink<IAigcToolAsset>().Definition;

        _in = this.AddActionInputConnector("In", "Input");
        _toolPage = this.AddDataInputConnector("ToolPage", pageType, "Tool Page");

        var instanceType = TypeDefinition.FromNative<IAigcPageInstance>();

        _out = this.AddActionOutputConnector("Out", "Output");
        _pageInstance = this.AddDataOutputConnector("PageInstance", instanceType, "Page Instance");
    }

    /// <inheritdoc/>
    public override ImageDef Icon => CoreIconCache.Tool;

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        do
        {
            var toolPage = compute.GetValue<IAigcToolAsset>(_toolPage);
            if (toolPage is null)
            {
                break;
            }

            try
            {
                var option = new PageElementOption
                {
                    Mode = PageElementMode.Function,
                };

                var pageInstance = toolPage.CreatePageInstance(option);

                compute.SetValue(_pageInstance, pageInstance);
                compute.SetResult(this, _out);
                return;
            }
            catch (Exception)
            {
                break;
            }

        } while (false);

        compute.SetValue(_pageInstance, null);
        compute.SetResult(this, _out);
    }
}

#endregion

#region CreateToolCallingWithParameter

/// <summary>
/// Represents a single parameter definition for a tool call, including name, description, and type.
/// </summary>
[NativeAlias("Suity.Editor.AIGC.Flows.ToolCallParameter")]
public class ToolCallParameter : INamed, IViewObject, ITextDisplay
{
    private StringProperty _name = new("Name", "Parameter Name");
    private StringProperty _description = new("Description", "Description");
    private ITypeDesignSelection _parameterType;
    private ValueProperty<bool> _isArray = new("IsArray", "Array", false);

    /// <summary>
    /// Initializes a new instance of the <see cref="ToolCallParameter"/> class.
    /// </summary>
    public ToolCallParameter()
    {
        _parameterType = DTypeManager.Instance.CreateTypeDesignSelection();
    }

    /// <inheritdoc/>
    public string Name
    {
        get => _name.Text;
        set => _name.Text = value ?? string.Empty;
    }

    /// <summary>
    /// Gets or sets the description of this parameter.
    /// </summary>
    public string Description
    {
        get => _description.Text;
        set => _description.Text = value ?? string.Empty;
    }

    /// <summary>
    /// Gets or sets the type definition for this parameter.
    /// </summary>
    public TypeDefinition ParameterType
    {
        get
        {
            var type = _parameterType?.GetTypeDefinition();
            if (_isArray)
            {
                type = type?.MakeArrayType();
            }

            return type ?? TypeDefinition.Empty;
        }
        set
        {
            if (_parameterType is null)
            {
                return;
            }

            if (TypeDefinition.IsNullOrEmpty(value))
            {
                _parameterType.SelectedKey = null;
                return;
            }

            if (value.IsArray)
            {
                _parameterType.SelectedKey = value.ElementType.TypeCode;
                _isArray.Value = true;
            }
            else
            {
                _parameterType.SelectedKey = value.TypeCode;
                _isArray.Value = false;
            }
        }
    }

    /// <summary>
    /// Synchronizes the parameter properties with the given sync provider.
    /// </summary>
    /// <param name="sync">The property sync provider.</param>
    /// <param name="context">The sync context.</param>
    public void Sync(IPropertySync sync, ISyncContext context)
    {
        _name.Sync(sync);
        _description.Sync(sync);
        _parameterType = sync.Sync("ParameterType", _parameterType, SyncFlag.NotNull | SyncFlag.AffectsOthers);
        _isArray.Sync(sync);
    }

    /// <summary>
    /// Sets up the view fields for this parameter in the inspector.
    /// </summary>
    /// <param name="setup">The view object setup context.</param>
    public void SetupView(IViewObjectSetup setup)
    {
        _name.InspectorField(setup);
        _description.InspectorField(setup);
        setup.InspectorField(_parameterType, new ViewProperty("ParameterType", "Type"));
        _isArray.InspectorField(setup);
    }

    #region ITextDisplay

    /// <inheritdoc/>
    public string DisplayText
    {
        get
        {
            string desc = _description.Text;
            if (!string.IsNullOrWhiteSpace(desc))
            {
                return desc;
            }

            return _name.Text ?? string.Empty;
        }
    }

    /// <inheritdoc/>
    public object DisplayIcon => null;

    /// <inheritdoc/>
    public TextStatus DisplayStatus => TextStatus.Normal;

    #endregion

    /// <inheritdoc/>
    public override string ToString() => DisplayText;
}

/// <summary>
/// Creates a tool calling object with configurable parameters based on a page definition.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.ToolBG)]
[DisplayText("Create Tool Calling With Parameter", "*CoreIcon|Tool")]
[DisplayOrder(2960)]
[ToolTipsText("Create a tool calling object with parameters based on the page definition.")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.CreateToolCallingWithParameter")]
public class CreateToolCallingWithParameter : TaskPageNode
{
    FlowNodeConnector _in;

    FlowNodeConnector _out;
    FlowNodeConnector _pageInstance;

    readonly AssetProperty<IAigcToolAsset> _toolDef = new("ToolDefinition", "Tool Definition");
    readonly ListProperty<ToolCallParameter> _parameters = new("Parameters", "Parameters");

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateToolCallingWithParameter"/> class.
    /// </summary>
    public CreateToolCallingWithParameter()
    {
        UpdateConnector();

        _parameters.ValueChanged += (s, e) => UpdateConnectorQueued();
    }

    /// <inheritdoc/>
    public override ImageDef Icon => CoreIconCache.Tool;

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _toolDef.Sync(sync);
        _parameters.Sync(sync);

        if (sync.Sync("UpdateParameter", ButtonValue.Empty) == ButtonValue.Clicked)
        {
            ParseFromToolDef();
        }
    }

    private void ParseFromToolDef()
    {
        if (_toolDef.Target is { } toolPageAsset)
        {
            try
            {
                var option = new PageElementOption
                {
                    Mode = PageElementMode.Function,
                };

                var pageInstance = toolPageAsset.CreatePageInstance(option);

                _parameters.List.Clear();

                foreach (var element in pageInstance.GetInputParameters())
                {
                    if (string.IsNullOrWhiteSpace(element.Name))
                    {
                        continue;
                    }

                    var type = element.ParameterType;
                    if (TypeDefinition.IsNullOrEmpty(type))
                    {
                        continue;
                    }

                    var parameter = new ToolCallParameter
                    {
                        Name = element.Name,
                        Description = element.ToDisplayTextL(),
                        ParameterType = type,
                    };

                    _parameters.List.Add(parameter);
                }

                UpdateConnectorQueued();
                this.GetFlowDocument()?.MarkDirty(this);
            }
            catch (Exception)
            {
                return;
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _toolDef.InspectorField(setup);
        _parameters.InspectorField(setup);

        setup.Button("UpdateParameter", "Update Parameters");
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        base.OnUpdateConnector();

        var pageType = AssetManager.Instance.GetAssetLink<SubFlowDefinitionAsset>().Definition;

        _in = this.AddActionInputConnector("In", "Input");

        var instanceType = TypeDefinition.FromNative<IAigcPageInstance>();

        _out = this.AddActionOutputConnector("Out", "Output");
        _pageInstance = this.AddDataOutputConnector("PageInstance", instanceType, "Page Instance");

        var parameters = GetParameters();
        foreach (var parameter in parameters)
        {
            AddDataInputConnector("p_" + parameter.Name, parameter.ParameterType, parameter.DisplayText);
        }
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var diagram = this.Diagram;

        do
        {
            var toolPageAsset = _toolDef.Target;
            if (toolPageAsset is null)
            {
                break;
            }

            try
            {
                var option = new PageElementOption
                {
                    Mode = PageElementMode.Function,
                };

                var pageInstance = toolPageAsset.CreatePageInstance(option);

                var parameters = GetParameters();
                foreach (var parameter in parameters)
                {
                    if (this.GetConnector("p_" + parameter.Name) is { } connector && diagram.GetIsLinked(connector))
                    {
                        object value = compute.GetValue<object>(connector);
                        pageInstance.SetParameter(parameter.Name, value);
                    }
                }

                compute.SetValue(_pageInstance, pageInstance);
                compute.SetResult(this, _out);

                return;
            }
            catch (Exception)
            {
                break;
            }

        } while (false);

        compute.SetValue(_pageInstance, null);
        compute.SetResult(this, _out);
    }

    /// <summary>
    /// Gets the validated list of tool call parameters, filtering out duplicates and invalid entries.
    /// </summary>
    /// <returns>An array of valid tool call parameters.</returns>
    public ToolCallParameter[] GetParameters()
    {
        List<ToolCallParameter> list = [];
        HashSet<string> visited = [];

        foreach (var item in _parameters.List.SkipNull())
        {
            string name = item.Name?.Trim() ?? string.Empty;
            if (!visited.Add(name))
            {
                continue;
            }

            var type = item.ParameterType;
            if (TypeDefinition.IsNullOrEmpty(type))
            {
                continue;
            }

            list.Add(item);
        }

        return list.ToArray();
    }

}

#endregion

#region GetPageInstanceParameter

/// <summary>
/// Retrieves a specific parameter value from a page instance by name.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.PageParameter, HasHeader = false)]
[DisplayText("Get Page Instance Parameter", "*CoreIcon|Page")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.GetPageInstanceParameter")]
public class GetPageInstanceParameter : TaskPageNode
{
    FlowNodeConnector _pageInstance;
    ConnectorStringProperty _parameterName = new("ParameterName", "Parameter Name");
    FlowNodeConnector _out;

    private ITypeDesignSelection _valueType;
    private bool _isArray;

    /// <summary>
    /// Gets the type definition for the output value.
    /// </summary>
    public TypeDefinition TypeDef
    {
        get
        {
            var type = _valueType?.GetTypeDefinition();
            if (_isArray)
            {
                type = type?.MakeArrayType();
            }

            return type ?? TypeDefinition.Empty;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPageInstanceParameter"/> class.
    /// </summary>
    public GetPageInstanceParameter()
    {
        _valueType = DTypeManager.Instance.CreateTypeDesignSelection();
        _valueType.SelectedKey = NativeTypes.StringType.TypeCode;
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _valueType = sync.Sync("ValueType", _valueType, SyncFlag.NotNull | SyncFlag.AffectsOthers);
        _isArray = sync.Sync("IsArray", _isArray);

        _parameterName.Sync(sync);

        if (sync.IsSetterOf("ValueType") || sync.IsSetterOf("IsArray"))
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_valueType, new ViewProperty("ValueType", "Type"));
        setup.InspectorField(_isArray, new ViewProperty("IsArray", "Array"));

        _parameterName.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        var pageType = TypeDefinition.FromNative<IAigcPageInstance>();
        _pageInstance = AddDataInputConnector("PageInstance", pageType, "Page Instance");
        _parameterName.AddConnector(this);
        _out = AddDataOutputConnector("Out", TypeDef, "Output");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var pageInstance = compute.GetValue<IAigcPageInstance>(_pageInstance);
        var context = pageInstance as IFlowCallerContext;
        string name = _parameterName.GetValue(compute, this)?.Trim() ?? string.Empty;

        if (context is null || string.IsNullOrEmpty(name))
        {
            compute.SetValue(_out, null);
            return;
        }

        if (context.TryGetParameter(compute, name, out var result))
        {
            compute.SetValue(_out, result);
        }
        else
        {
            compute.SetValue(_out, null);
        }
    }
}

#endregion

#region SetPageInstanceParameter

/// <summary>
/// Sets a specific parameter value on a page instance by name.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.PageParameter)]
[DisplayText("Set Page Instance Parameter", "*CoreIcon|Page")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.SetPageInstanceParameter")]
public class SetPageInstanceParameter : TaskPageNode
{
    FlowNodeConnector _in;
    FlowNodeConnector _pageInstance;
    ConnectorStringProperty _parameterName = new("ParameterName", "Parameter Name");

    FlowNodeConnector _value;
    FlowNodeConnector _out;

    private ITypeDesignSelection _valueType;
    private bool _isArray;

    /// <summary>
    /// Gets the type definition for the input value.
    /// </summary>
    public TypeDefinition TypeDef
    {
        get
        {
            var type = _valueType?.GetTypeDefinition();
            if (_isArray)
            {
                type = type?.MakeArrayType();
            }

            return type ?? TypeDefinition.Empty;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SetPageInstanceParameter"/> class.
    /// </summary>
    public SetPageInstanceParameter()
    {
        _valueType = DTypeManager.Instance.CreateTypeDesignSelection();
        _valueType.SelectedKey = NativeTypes.StringType.TypeCode;
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _valueType = sync.Sync("ValueType", _valueType, SyncFlag.NotNull | SyncFlag.AffectsOthers);
        _isArray = sync.Sync("IsArray", _isArray);

        _parameterName.Sync(sync);

        if (sync.IsSetterOf("ValueType") || sync.IsSetterOf("IsArray"))
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_valueType, new ViewProperty("ValueType", "Type"));
        setup.InspectorField(_isArray, new ViewProperty("IsArray", "Array"));

        _parameterName.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        _in = AddActionInputConnector("In", "Input");

        var pageType = TypeDefinition.FromNative<IAigcPageInstance>();
        _pageInstance = AddDataInputConnector("PageInstance", pageType, "Page Instance");
        _parameterName.AddConnector(this);

        _value = AddDataInputConnector("Value", TypeDef, "Value");
        _out = AddActionOutputConnector("Out", "Output");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var pageInstance = compute.GetValue<IAigcPageInstance>(_pageInstance)
            ?? throw new ArgumentNullException("PageInstance");

        var context = pageInstance as IFlowCallerContext;
        string name = _parameterName.GetValue(compute, this)?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException("ParameterName");
        }

        var value = compute.GetValue(_value);

        context?.SetParameter(compute, name, value);

        compute.SetResult(this, _out);
    }

    /// <inheritdoc/>
    public override string DisplayText
    {
        get
        {
            if (_parameterName.GetIsLinked(this))
            {
                return base.DisplayText;
            }
            else
            {
                return "Set: " + _parameterName.Text;
            }
        }
    }
}
#endregion

#region SetPageParameter

/// <summary>
/// Sets a page parameter value by connecting to a parameter reference node.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.PageParameter)]
[DisplayText("Set Page Parameter", "*CoreIcon|Parameter")]
[DisplayOrder(2799)]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.SetPageParameterNode")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.SetPageParameter")]
public class SetPageParameter : TaskPageNode
{
    private FlowNodeConnector _in;
    private FlowNodeConnector _inValue;

    private FlowNodeConnector _out;
    private FlowNodeConnector _refOutput;

    private ITypeDesignSelection _valueType;
    private bool _isArray;

    /// <summary>
    /// Gets the type definition for the value connector.
    /// </summary>
    public TypeDefinition TypeDef
    {
        get
        {
            var type = _valueType?.GetTypeDefinition();
            if (_isArray)
            {
                type = type?.MakeArrayType();
            }

            return type ?? TypeDefinition.Empty;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SetPageParameter"/> class.
    /// </summary>
    public SetPageParameter()
    {
        _valueType = DTypeManager.Instance.CreateTypeDesignSelection();
        _valueType.SelectedKey = NativeTypes.StringType.TypeCode;
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _valueType = sync.Sync("ValueType", _valueType, SyncFlag.NotNull | SyncFlag.AffectsOthers);
        _isArray = sync.Sync("IsArray", _isArray);

        if (sync.IsSetterOf("ValueType") || sync.IsSetterOf("IsArray"))
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_valueType, new ViewProperty("ValueType", "Type"));
        setup.InspectorField(_isArray, new ViewProperty("IsArray", "Array"));
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        _in = AddActionInputConnector("In", "Input");
        _inValue = AddDataInputConnector("InValue", TypeDef, "Value");

        _out = AddActionOutputConnector("Out", "Output");

        _refOutput = AddConnector("RefOut", TypeDef, FlowDirections.Output, FlowConnectorTypes.Control, false, "Parameter Reference");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var refConnector = this.Diagram?.GetLinkedConnector(_refOutput);
        if (refConnector is null)
        {
            compute.SetValue(_out, null);
            return;
        }

        var refName = refConnector?.ParentNode?.Name;
        if (string.IsNullOrWhiteSpace(refName))
        {
            compute.SetValue(_out, null);
            return;
        }

        if (compute.Context.GetArgument<IFlowCallerContext>() is not { } caller)
        {
            compute.SetValue(_out, null);
            return;
        }

        var value = compute.GetValue(_inValue);

        EditorServices.TypeConvertService.TryConvert(_refOutput, refConnector, value, out var converted);

        caller.SetParameter(compute, refName, converted);

        compute.SetResult(this, _out);
    }
}

#endregion

#region PageParameterReference

/// <summary>
/// Provides a reference to a page parameter node, outputting its current value.
/// </summary>
[SimpleFlowNodeStyle(HasHeader = false)]
[DisplayText("AIGC Page Parameter Reference", "*CoreIcon|Parameter")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.PageParameterRefNode")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.PageParameterRef")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.PageParameterReference")]
public class PageParameterReference : TaskPageNode
{
    private FlowNodeConnector _out;
    private FlowNodeConnector _refOutput;

    private ITypeDesignSelection _valueType;
    private bool _isArray;

    /// <summary>
    /// Gets the type definition for the output connector.
    /// </summary>
    public TypeDefinition TypeDef
    {
        get
        {
            var type = _valueType?.GetTypeDefinition();
            if (_isArray)
            {
                type = type?.MakeArrayType();
            }

            return type ?? TypeDefinition.Empty;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PageParameterReference"/> class.
    /// </summary>
    public PageParameterReference()
    {
        _valueType = DTypeManager.Instance.CreateTypeDesignSelection();
        _valueType.SelectedKey = NativeTypes.StringType.TypeCode;

        base.FlowNodeGui = OnGui;

        UpdateConnector();
    }


    /// <inheritdoc/>
    public override Color? BackgroundColor => TitleColor;

    /// <summary>
    /// Gets the name of the referenced parameter node.
    /// </summary>
    public string ParameterName
    {
        get
        {
            var refConnector = this.Diagram?.GetLinkedConnector(_refOutput);
            return refConnector?.ParentNode?.Name ?? string.Empty;
        }
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _valueType = sync.Sync("ValueType", _valueType, SyncFlag.NotNull | SyncFlag.AffectsOthers);
        _isArray = sync.Sync("IsArray", _isArray);

        if (sync.IsSetterOf("ValueType") || sync.IsSetterOf("IsArray"))
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_valueType, new ViewProperty("ValueType", "Type"));
        setup.InspectorField(_isArray, new ViewProperty("IsArray", "Array"));
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        _out = AddConnector("Out", TypeDef, FlowDirections.Output, FlowConnectorTypes.Data);
        _refOutput = AddConnector("RefOut", TypeDef, FlowDirections.Output, FlowConnectorTypes.Control, false, "Parameter Reference");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var refConnector = this.Diagram?.GetLinkedConnector(_refOutput);
        if (refConnector is null)
        {
            compute.SetValue(_out, null);
            return;
        }

        var refName = refConnector.ParentNode?.Name;
        if (string.IsNullOrWhiteSpace(refName))
        {
            compute.SetValue(_out, null);
            return;
        }

        if (compute.Context.GetArgument<IFlowCallerContext>() is not { } caller)
        {
            compute.SetValue(_out, null);
            return;
        }

        if (!caller.TryGetParameter(compute, refName, out object value))
        {
            compute.SetValue(_out, null);
            return;
        }

        EditorServices.TypeConvertService.TryConvert(refConnector, _out, value, out var converted);

        compute.SetValue(_out, converted);

    }

    private ImGuiNode OnGui(ImGui gui, IDrawNodeContext context)
    {
        string text = ParameterName;
        if (string.IsNullOrEmpty(text))
        {
            // Ensure there is a space for layout placeholder.
            text = " ";
        }

        return gui.FlowSingleConnectorFrame(_out, context, text, editorGui: DrawExEditorGui);
    }

    private bool DrawExEditorGui(ImGui gui, EditorImGuiPipeline pipeline, IDrawContext context)
    {
        if (pipeline == EditorImGuiPipeline.Output)
        {
            gui.HorizontalLayout("#control-input")
            .OnInitialize(n =>
            {
                n.InitClass("debug_draw");
                n.InitFit();
                n.InitHorizontalAlignment(GuiAlignment.Center);
                n.InitPadding(1);
            })
            .OnContent(() =>
            {
                gui.FlowConnectorPoint(_refOutput, context, _refOutput.Name);
            });
        }

        return true;
    }
}

#endregion

#region Converters

/// <summary>
/// Converts an <see cref="IAigcTaskPage"/> to its associated <see cref="IAigcPageInstance"/>.
/// </summary>
public class TaskPageToPageInstanceConverter : TypeConverter<IAigcTaskPage, IAigcPageInstance>
{
    /// <inheritdoc/>
    public override IAigcPageInstance Convert(IAigcTaskPage objFrom)
    {
        return objFrom.GetPageInstance();
    }
}

/// <summary>
/// Converts an <see cref="IAigcPageInstance"/> to its owning <see cref="IAigcTaskPage"/>.
/// </summary>
public class PageInstanceToTaskPageConverter : TypeConverter<IAigcPageInstance, IAigcTaskPage>
{
    /// <inheritdoc/>
    public override IAigcTaskPage Convert(IAigcPageInstance objFrom)
    {
        return objFrom.Owner as IAigcTaskPage;
    }
}

#endregion
