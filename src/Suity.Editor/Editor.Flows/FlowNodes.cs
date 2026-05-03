using Suity.Drawing;
using Suity.Editor.Analyzing;
using Suity.Editor.Design;
using Suity.Editor.Selecting;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Graphics;
using Suity.Views.Im;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.Flows;

#region DesignFlowNode

/// <summary>
/// Base class for design flow nodes with editor properties such as name, description, icon, and color.
/// </summary>
public abstract class DesignFlowNode : FlowNode,
    IDesignObject,
    IHasAttributeDesign,
    IAttributeGetter
{
    private string _description;
    private AssetSelection<ImageAsset> _iconSelection = new();
    private Color _color = Color.Empty;

    private readonly SArrayAttributeDesign _attributes = new();

    /// <summary>
    /// Initializes a new instance of the DesignFlowNode.
    /// </summary>
    public DesignFlowNode()
    {
        base.CustomDraw = Draw;
        base.EditorGui = OnEditorGui;
    }

    /// <summary>
    /// Gets or sets the description text of the node.
    /// </summary>
    public string Description
    {
        get => _description;
        set
        {
            if (_description == value)
            {
                return;
            }

            _description = value;

            (DiagramItem as FlowDiagramItem)?.AssetBuilder?.SetDescription(_description);
        }
    }

    /// <summary>
    /// Gets the default icon for the node when no custom icon is set.
    /// </summary>
    public virtual ImageDef DefaultIcon => base.Icon;

    /// <summary>
    /// Gets the icon for the node, using custom icon if available.
    /// </summary>
    public override ImageDef Icon => _iconSelection.Icon ?? DefaultIcon;

    /// <summary>
    /// Gets the default color for the node title.
    /// </summary>
    public virtual Color? DefaultNodeColor => base.TitleColor;

    /// <summary>
    /// Gets or sets the title color, using custom color if set, otherwise using default.
    /// </summary>
    public override Color? TitleColor
    {
        get
        {
            if (_color != Color.Empty)
            {
                return _color;
            }

            return DefaultNodeColor;
        }
    }

    /// <summary>
    /// Gets the ID of the icon asset.
    /// </summary>
    public Guid IconId => _iconSelection.Id;

    /// <summary>
    /// Gets the default design color for the node.
    /// </summary>
    public virtual Color? DefaultDesignColor => null;

    /// <summary>
    /// Gets or sets the design color of the node.
    /// </summary>
    public Color DesignColor
    {
        get => _color != Color.Empty ? _color : DefaultDesignColor ?? Color.Empty;
        set
        {
            if (_color == value)
            {
                return;
            }

            _color = value;

            Color? c = _color != Color.Empty ? _color : null;
            (DiagramItem as FlowDiagramItem)?.AssetBuilder?.SetColor(c);
        }
    }

    /// <summary>
    /// Gets the display text for the node, using description if set, otherwise using name.
    /// </summary>
    public override string DisplayText => !string.IsNullOrEmpty(_description) ? _description : this.Name;

    /// <summary>
    /// Gets or sets the icon selection for the node.
    /// </summary>
    protected AssetSelection<ImageAsset> IconSelection
    {
        get => _iconSelection;
        set
        {
            value ??= new();

            if (_iconSelection.Id != value.Id)
            {
                _iconSelection = value;
                (DiagramItem as FlowDiagramItem)?.AssetBuilder?.SetIconId(_iconSelection.Id);
            }
        }
    }

    #region Virtual
    /// <summary>
    /// Synchronizes the properties of the node.
    /// </summary>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        Name = sync.Sync("Name", Name, SyncFlag.NotNull);

        Description = sync.Sync("Description", Description);
        IconSelection = sync.Sync("Icon", IconSelection, SyncFlag.NotNull);
        // Pass _color, not DesignColor, because reading DesignColor overrides the default color logic
        DesignColor = sync.Sync("Color", _color, SyncFlag.None, Color.Empty);
        sync.Sync("Attributes", _attributes.Array, SyncFlag.GetOnly);
    }

    /// <summary>
    /// Sets up the view for the node.
    /// </summary>
    public override void SetupView(IViewObjectSetup setup)
    {
        base.SetupView(setup);

        OnSetupViewAppearance(setup);
        OnSetupViewContent(setup);
    }

    /// <summary>
    /// Sets up the view properties for the inspector.
    /// </summary>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(Name, new ViewProperty("Name", "Name"));
    }

    /// <summary>
    /// Sets up the appearance view properties for the node.
    /// </summary>
    protected virtual void OnSetupViewAppearance(IViewObjectSetup setup)
    {
        setup.Label(new ViewProperty("#Appearance", "Appearance", CoreIconCache.View));

        setup.InspectorField(Description, new ViewProperty("Description", "Description"));
        setup.InspectorField(_iconSelection, new ViewProperty("Icon", "Icon"));
        setup.InspectorField(_color, new ViewProperty("Color", "Color", CoreIconCache.Color)
            .WithColor(_color != Color.Empty ? _color : (Color?)null));
    }

    /// <summary>
    /// Sets up the content view properties for the node.
    /// </summary>
    protected virtual void OnSetupViewContent(IViewObjectSetup setup)
    { }

    /// <summary>
    /// Called when the diagram item is updated.
    /// </summary>
    protected override void OnDiagramItemUpdated()
    {
        base.OnDiagramItemUpdated();

        var builder = (DiagramItem as FlowDiagramItem)?.AssetBuilder;
        if (builder != null)
        {
            builder.SetDescription(_description);

            Color? c = _color != Color.Empty ? _color : null;
            builder.SetColor(c);

            builder.SetIconId(_iconSelection.Id);
        }
    }
    #endregion

    #region IDesignObject

    /// <summary>
    /// Gets the design items array.
    /// </summary>
    SArray IDesignObject.DesignItems => _attributes.Array;

    /// <summary>
    /// Gets the property name for design attributes.
    /// </summary>
    string IDesignObject.DesignPropertyName => "Attributes";

    /// <summary>
    /// Gets the property description for design attributes.
    /// </summary>
    string IDesignObject.DesignPropertyDescription => "Property";

    #endregion

    #region IAttributeDesign

    /// <summary>
    /// Gets the attribute design for the node.
    /// </summary>
    public IAttributeDesign Attributes => _attributes;

    #endregion

    #region IHasAttribute

    /// <summary>
    /// Gets all attributes for the node.
    /// </summary>
    public IEnumerable<object> GetAttributes() => _attributes.GetAttributes();

    /// <summary>
    /// Gets attributes of the specified type name.
    /// </summary>
    public IEnumerable<object> GetAttributes(string typeName) => _attributes.GetAttributes(typeName);

    /// <summary>
    /// Gets attributes of the specified type.
    /// </summary>
    public IEnumerable<T> GetAttributes<T>() where T : class => _attributes.GetAttributes<T>();

    #endregion

    /// <summary>
    /// Draws the node with its background, header, and connectors.
    /// </summary>
    protected virtual void Draw(IGraphicOutput output, IDrawNodeContext context, float zoom, Point pos, Rectangle rect, bool drawText)
    {
        context.DrawShadow(output);

        context.DrawPanel(output, zoom, rect);

        if (drawText)
        {
            context.DrawHeader(output, zoom, rect);
        }

        context.DrawConnectors(output);

        if (!string.IsNullOrWhiteSpace(context.PreviewText) && drawText)
        {
            context.DrawPreviewText(output, zoom, rect, context.PreviewText);
        }

        if (DiagramItem is ISupportAnalysis s && s.Analysis is AnalysisResult analysis && analysis.ReferenceCount > 0 && zoom > 0.9)
        {
            DrawRef(output, zoom, rect, analysis);
        }
    }

    /// <summary>
    /// Draws the editor GUI for the node.
    /// </summary>
    protected virtual bool OnEditorGui(ImGui gui, EditorImGuiPipeline pipeline, IDrawContext context)
    {
        if (pipeline == EditorImGuiPipeline.Preview)
        {
            if (DiagramItem is ISupportAnalysis s && s.Analysis is AnalysisResult analysis && analysis.ReferenceCount > 0)
            {
                var c = EditorServices.ColorConfig;
                gui.NumberBox("ref", analysis.ReferenceCount.ToString(), c.GetStatusColor(TextStatus.Reference), CoreIconCache.Reference, iconDark: true, tooltips: L("Reference Count"));

                return true;
            }
        }

        return false;
    }

    private Color _refColorDark = Color.FromArgb(36, 36, 36);

    private BrushDef _refBrush;
    private FontDef _refFont;
    private BrushDef _refTextBrush;
    private BrushDef _refBrushDark;

    /// <summary>
    /// Draws the reference count indicator.
    /// </summary>
    private void DrawRef(IGraphicOutput output, float zoom, Rectangle rect, AnalysisResult analysis)
    {
        _refBrush ??= new SolidBrushDef(TextStatus.Reference.ToColor());
        _refFont ??= new FontDef(ImGuiTheme.DefaultFont, 12);
        _refBrushDark ??= new SolidBrushDef(_refColorDark);
        _refTextBrush ??= new SolidBrushDef(TextStatus.Normal.ToColor());

        string text = analysis.ReferenceCount.ToString();
        SizeF textSize = output.MeasureString(text, _refFont);
        int textW = (int)textSize.Width + 4;

        int w = 20 + textW;
        int h = 16;
        int m = (int)(4 * zoom);

        var rectRef = new Rectangle(rect.Right - w - m, rect.Y + m, w, h);
        var rectIcon = new Rectangle(rectRef.X + 1, rectRef.Y + 1, 16, 16);

        var rectDark = new Rectangle(rectRef.X + 18, rectRef.Y + 2, textW, 12);
        var rectText = new Rectangle(rectRef.X + 20, rectRef.Y + 13, textW, 16);

        // int textCenter = rectText.X + rectText.Width / 2;

        output.FillRoundRectangle(_refBrush, rectRef, 6);
        output.DrawImage(CoreIconCache.Reference, rectIcon, _refColorDark);
        output.FillRoundRectangle(_refBrushDark, rectDark, 10);
        output.DrawString(text, _refFont, _refTextBrush, rectText.X, rectText.Y);
    }
}

#endregion

#region RenderFlowNode

/// <summary>
/// Base class for code render flowchart nodes.
/// </summary>
public abstract class RenderFlowNode : FlowNode
{
    /// <summary>
    /// Renderable data type identifier.
    /// </summary>
    public const string RenderableDataType = "<Renderable>";

    /// <summary>
    /// Material data type identifier.
    /// </summary>
    public const string MaterialDataType = "<Material>";

    /// <summary>
    /// User code data type identifier.
    /// </summary>
    public const string UserCodeDataType = "<UserCode>";

    /// <summary>
    /// Render targets data type identifier.
    /// </summary>
    public const string RenderTargetsDataType = "<RenderTargets>";

    /// <summary>
    /// Gets the icon for the node.
    /// </summary>
    public override ImageDef Icon => CoreIconCache.Render;
    //public override bool PreviewValue => false;

    /// <summary>
    /// Initializes a new instance of the RenderFlowNode.
    /// </summary>
    protected RenderFlowNode()
    {

    }
}

#endregion

#region ValueFlowNode

/// <summary>
/// Base class for value flowchart nodes.
/// </summary>
[DisplayText("Value", "*CoreIcon|Value")]
[ToolTipsText("Value operation related nodes")]
public abstract class ValueFlowNode : FlowNode
{
    /// <summary>
    /// Gets the icon for the node.
    /// </summary>
    public override ImageDef Icon => EditorUtility.ToDisplayIcon(this.GetType()) ?? CoreIconCache.Value;
}

#endregion

#region JsonFlowNode

/// <summary>
/// JSON content types for JsonFlowNode.
/// </summary>
[NativeType(CodeBase = "Suity")]
public enum JsonContentTypes
{
    /// <summary>
    /// JSON content object.
    /// </summary>
    [DisplayText("Json Content")]
    Content,

    /// <summary>
    /// String value.
    /// </summary>
    [DisplayText("String")]
    String,

    /// <summary>
    /// Numeric value.
    /// </summary>
    [DisplayText("Number")]
    Numeric,

    /// <summary>
    /// Boolean value.
    /// </summary>
    [DisplayText("Boolean")]
    Boolean,
}

/// <summary>
/// Base class for JSON operation flowchart nodes.
/// </summary>
[DisplayText("Json", "*CoreIcon|Json")]
[ToolTipsText("Json operation related nodes")]
public abstract class JsonFlowNode : FlowNode
{
    /// <summary>
    /// JSON data type identifier.
    /// </summary>
    public const string JsonData = "<JsonData>";

    /// <summary>
    /// Gets the icon for the node.
    /// </summary>
    public override ImageDef Icon => EditorUtility.ToDisplayIcon(this.GetType()) ?? CoreIconCache.Json;

    /// <summary>
    /// Converts a JsonContentTypes to a data type string.
    /// </summary>
    public static string GetJsonDataType(JsonContentTypes contentType) => contentType switch
    {
        JsonContentTypes.String => "string",
        JsonContentTypes.Numeric => "float",
        JsonContentTypes.Boolean => "bool",
        _ => JsonData,
    };
}

#endregion

#region SValueFlowNode

/// <summary>
/// Base class for editor data SValue flowchart nodes.
/// </summary>
[DisplayText("Editor Data", "*CoreIcon|SValue")]
[ToolTipsText("Editor data operation related nodes")]
public abstract class SValueFlowNode : FlowNode
{
    /// <summary>
    /// Gets the icon for the node.
    /// </summary>
    public override ImageDef Icon => EditorUtility.ToDisplayIcon(this.GetType()) ?? CoreIconCache.SValue;
}

#endregion

#region ActionFlowNode

/// <summary>
/// Base class for action flowchart nodes that perform asynchronous computation.
/// </summary>
[DisplayText("Action", "*CoreIcon|Action")]
[ToolTipsText("Action flow related nodes")]
public abstract class ActionFlowNode : FlowNode, IFlowNodeComputeAsync
{
    /// <summary>
    /// Gets the icon for the node.
    /// </summary>
    public override ImageDef Icon => EditorUtility.ToDisplayIcon(this.GetType()) ?? CoreIconCache.Action;

    /// <summary>
    /// Computes the action asynchronously.
    /// </summary>
    public abstract Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel);
}

#endregion

#region DialogFlowNodw

/// <summary>
/// Base class for conversation/dialog flowchart nodes.
/// </summary>
[DisplayText("Conversation", "*CoreIcon|Conversation")]
[ToolTipsText("Conversation related nodes")]
public abstract class DialogFlowNode : FlowNode, IFlowNodeComputeAsync
{
    /// <summary>
    /// Gets the icon for the node.
    /// </summary>
    public override ImageDef Icon => EditorUtility.ToDisplayIcon(this.GetType()) ?? CoreIconCache.Conversation;

    /// <summary>
    /// Computes the dialog asynchronously.
    /// </summary>
    public abstract Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel);
}

#endregion

#region VariableFlowNode

/// <summary>
/// Base class for variable operation flowchart nodes.
/// </summary>
[DisplayText("Variable", "*CoreIcon|Variable")]
[ToolTipsText("Variable operation related nodes")]
public abstract class VariableFlowNode : FlowNode
{
    /// <summary>
    /// Gets the icon for the node.
    /// </summary>
    public override ImageDef Icon => EditorUtility.ToDisplayIcon(this.GetType()) ?? CoreIconCache.Variable;

    /// <summary>
    /// Gets the function context for the specified scope.
    /// </summary>
    protected FunctionContext GetContext(IFlowComputation compute, FlowContextScopes scope)
    {
        return compute.GetContext(scope, this.Diagram);
    }

    /// <summary>
    /// Gets a variable value from the specified scope.
    /// </summary>
    public object GetVariable(IFlowComputation compute, FlowContextScopes scope, string varName)
    {
        return compute.GetVariable(scope, this.Diagram, varName);
    }

    /// <summary>
    /// Sets a variable value in the specified scope.
    /// </summary>
    public void SetVariable(IFlowComputation compute, FlowContextScopes scope, string varName, object value)
    {
        compute.SetVariable(scope, this.Diagram, varName, value);
    }
}
#endregion

#region TextFlowNode

/// <summary>
/// Base class for text operation flowchart nodes.
/// </summary>
[DisplayText("Text", "*CoreIcon|Text")]
[ToolTipsText("Text operation related nodes")]
public abstract class TextFlowNode : FlowNode
{
    /// <summary>
    /// Gets the icon for the node.
    /// </summary>
    public override ImageDef Icon => EditorUtility.ToDisplayIcon(this.GetType()) ?? CoreIconCache.Text;
}

#endregion

#region LinqFlowNode

/// <summary>
/// Base class for LINQ operation flowchart nodes.
/// </summary>
[DisplayText("Linq", "*CoreIcon|Array")]
[ToolTipsText("Linq operation related nodes")]
public abstract class LinqFlowNode : FlowNode
{
    /// <summary>
    /// Gets the icon for the node.
    /// </summary>
    public override ImageDef Icon => EditorUtility.ToDisplayIcon(this.GetType()) ?? CoreIconCache.Array;
}

#endregion