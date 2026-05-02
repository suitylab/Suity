using Suity.Editor.Documents;
using Suity.Editor.Expressions;
using Suity.Editor.Flows;
using Suity.Editor.Selecting;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Drawing;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.Design;

#region AliasTypeNameAttribute
/// <summary>
/// Specifies an alias for a type name.
/// </summary>
[NativeType(CodeBase = "*Design", Name = "AliasTypeName", Description = "Type Alias", Icon = "*CoreIcon|Rename")]
public class AliasTypeNameAttribute : DesignAttribute, IViewObject
{
    /// <summary>
    /// Gets or sets the alias name.
    /// </summary>
    public string AliasName { get; set; }
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        AliasName = sync.Sync("AliasName", AliasName);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        setup.InspectorField(AliasName, new ViewProperty("AliasName", "Alias"));
    }

    public override string ToString()
    {
        return L("Alias") + ": " + AliasName;
    }
}
#endregion
#region ToolTipAttribute
/// <summary>
/// Specifies tooltip text for an element.
/// </summary>
[NativeType(CodeBase = "*Design", Name = nameof(ToolTipsAttribute), Description = "Tooltip", Icon = "*CoreIcon|Tooltips")]
public class ToolTipsAttribute : DesignAttribute, IViewObject
{
    private readonly TextBlockProperty _toolTips = new(nameof(ToolTips), "Tooltip Text");

    /// <summary>
    /// Gets or sets the tooltip text.
    /// </summary>
    public string ToolTips { get => _toolTips.Text; set => _toolTips.Text = value ?? string.Empty; }

    #region IViewObject

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _toolTips.Sync(sync);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _toolTips.InspectorField(setup);
    }

    #endregion

    public override string ToString()
    {
        string str = _toolTips.Text ?? string.Empty;
        if (str.Length < 30)
        {
            return str;
        }
        else
        {
            return str[..30] + "...";
        }
    }
}
#endregion
#region UsageAttribute
/// <summary>
/// Specifies the usage of an element.
/// </summary>
[NativeType(CodeBase = "*Design", Name = "Usage", Description = "Usage", Icon = "*CoreIcon|Design")]
public class UsageAttribute : DesignAttribute, IViewObject
{
    /// <summary>
    /// Gets or sets the usage value.
    /// </summary>
    public string Usage { get; set; }
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        Usage = sync.Sync("Usage", Usage);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        setup.InspectorField(Usage, new ViewProperty("Usage", "Usage"));
    }

    public override string ToString()
    {
        return L("Usage") + ": " + Usage;
    }
}
#endregion
#region DataUsageAttribute
/// <summary>
/// Specifies how data is used or displayed in the editor.
/// </summary>
[NativeType(CodeBase = "*Design", Name = "DataUsage", Description = "Data Usage", Icon = "*CoreIcon|Structure")]
[NativeAlias("*AIGC|AIDataUsage")]
[ToolTipsText("Indicates the data usage of this type when added to a type.")]
public sealed class DataUsageAttribute : DesignAttribute, ITextDisplay
{
    private ValueProperty<DataUsageMode> _usage = new(nameof(Usage), "Usage ", DataUsageMode.None);

    /// <summary>
    /// Gets or sets the data usage mode.
    /// </summary>
    public DataUsageMode Usage
    {
        get => _usage.Value; set => _usage.Value = value; }

#region IViewObject

protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _usage.Sync(sync);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _usage.InspectorField(setup);
    }

        #endregion

    #region ITextDisplay

    string ITextDisplay.DisplayText => ToString();

    object ITextDisplay.DisplayIcon
    {
        get
        {
            switch (_usage.Value)
            {
                case DataUsageMode.DataGrid:
                    return CoreIconCache.DataGrid;

                case DataUsageMode.FlowGraph:
                    return CoreIconCache.DataFlow;

                case DataUsageMode.TreeGraph:
                    return CoreIconCache.Brain;

                case DataUsageMode.Config:
                    return CoreIconCache.Config;

                case DataUsageMode.EntityData:
                    return CoreIconCache.Entity;

                case DataUsageMode.Entity:
                    return CoreIconCache.Entity;

                case DataUsageMode.Action:
                    return CoreIconCache.Action;

                case DataUsageMode.Activity:
                    return CoreIconCache.Activity;

                case DataUsageMode.Nullable:
                    return CoreIconCache.Disable;
            }

            return null;
        }
    }

    TextStatus ITextDisplay.DisplayStatus => TextStatus.Normal;

    #endregion

    public override string ToString() => L("Data Usage") + ": " + _usage.Value.ToDisplayText();
}
#endregion
#region DrivenAttribute
/// <summary>
/// Specifies the driven generation method for data.
/// </summary>
[NativeType(CodeBase = "*Design", Name = "Driven", Description = "Driven", Icon = "*CoreIcon|Driven")]
[NativeAlias("Suity.Editor.AIGC.SkipAIGenerationAttribute")]
[NativeAlias("Suity.Editor.AIGC.Assistants.PassiveAIGenerationAttribute")]
[NativeAlias("Suity.Editor.AIGC.Assistants.AIDrivenAttribute")]
[NativeAlias("*AIGC|PassiveAIGeneration")]
[NativeAlias("*Design|PassiveAIGeneration")]
[ToolTipsText("Indicates the driven generation method of this type when added to a type. Indicates the driven generation method of the field type under this field generation when added to a field.")]
public class DrivenAttribute : DesignAttribute, ITextDisplay
{
    private ValueProperty<DataDrivenMode> _mode = new(nameof(Mode), "Mode", DataDrivenMode.Unique);

    /// <summary>
    /// Gets or sets the driven mode.
    /// </summary>
    public DataDrivenMode Mode { get => _mode.Value; set => _mode.Value = value; }

    #region IViewObject

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _mode.Sync(sync);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _mode.InspectorField(setup);
    }

    #endregion

    #region ITextDisplay

    string ITextDisplay.DisplayText => ToString();

    object ITextDisplay.DisplayIcon
    {
        get
        {
            switch (_mode.Value)
            {
                case DataDrivenMode.None:
                    return CoreIconCache.Driven;

                case DataDrivenMode.Active:
                    return CoreIconCache.Prompt;

                case DataDrivenMode.Unique:
                    return CoreIconCache.Controlled;

                case DataDrivenMode.Shared:
                    return CoreIconCache.Share;
            }

            return null;
        }
    }

    TextStatus ITextDisplay.DisplayStatus => TextStatus.Normal;

    #endregion

    public override string ToString() => L("Driven") + ": " + _mode.Value.ToDisplayText();
}
#endregion
#region KnowledgeAttribute
/// <summary>
/// Specifies knowledge content for AI generation.
/// </summary>
[NativeType(CodeBase = "*Design", Name = nameof(KnowledgeAttribute), Description = "Knowledge", Icon = "*CoreIcon|Knowledge")]
public sealed class KnowledgeAttribute : DesignAttribute, IViewObject
{
    private readonly TextBlockProperty _knowledge = new(nameof(Knowledge), "Knowledge");

    /// <summary>
    /// Gets or sets the knowledge text.
    /// </summary>
    public string Knowledge { get => _knowledge.Text; set => _knowledge.Text = value ?? string.Empty; }

    #region IViewObject

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _knowledge.Sync(sync);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _knowledge.InspectorField(setup);
    }

    #endregion

    public override string ToString()
    {
        string str = _knowledge.Text ?? string.Empty;
        if (str.Length < 30)
        {
            return str;
        }
        else
        {
            return str[..30] + "...";
        }
    }
}
#endregion
#region ArticleReferenceAttribute
/// <summary>
/// References an article document.
/// </summary>
[NativeType(CodeBase = "*Design", Name = nameof(ArticleReferenceAttribute), Description = "Article Reference", Icon = "*CoreIcon|Article")]
public sealed class ArticleReferenceAttribute : DesignAttribute, IViewObject
{
    private readonly AssetProperty<ArticleAsset> _article = new("Article", "Article Reference");

    /// <summary>
    /// Gets the article text at the specified depth.
    /// </summary>
    public string GetArticle(int depth = 1) => _article.Target?.GetFullText(depth);

    /// <summary>
    /// Gets or sets the article asset.
    /// </summary>
    public ArticleAsset Asset
    {
        get => _article.Target;
        set => _article.Target = value;
    }

    #region IViewObject

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _article.Sync(sync);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _article.InspectorField(setup);
    }

    #endregion

    public override string ToString()
    {
        string str = _article.Target?.NameInTreeView;
        return str;
    }
}
#endregion
#region RemarkAttribute
/// <summary>
/// Specifies a remark or note.
/// </summary>
[NativeType(CodeBase = "*Design", Name = "RemarkAttribute", Description = "Remark", Icon = "*CoreIcon|Text")]
public class RemarkAttribute : DesignAttribute, IViewObject
{
    /// <summary>
    /// Gets or sets the remark text.
    /// </summary>
    public TextBlock _remark = new();

    /// <summary>
    /// Gets the remark as a string.
    /// </summary>
    public string Remark => _remark?.ToString() ?? string.Empty;

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _remark = sync.Sync(nameof(Remark), _remark, SyncFlag.NotNull) ?? new TextBlock();
    }

    #region IViewObject

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        setup.InspectorField(_remark, new ViewProperty(nameof(Remark), "Remark"));
    }

    #endregion

    public override string ToString()
    {
        return L("Remark") + ": " + Remark;
    }
}
#endregion
#region CustomClassAttribute
/// <summary>
/// Specifies custom class generation options.
/// </summary>
[NativeType(CodeBase = "*Design", Name = "CustomClass", Description = "Custom Class", Icon = "*CoreIcon|Class")]
public class CustomClassAttribute : DesignAttribute, IViewObject
{
    /// <summary>
    /// Gets or sets whether custom class generation is enabled.
    /// </summary>
    public bool ClassEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the class is static.
    /// </summary>
    public bool IsStatic { get; set; }

    /// <summary>
    /// Gets or sets the access state.
    /// </summary>
    public AccessState Access { get; set; } = AccessState.Public;

    /// <summary>
    /// Gets or sets the virtual state.
    /// </summary>
    public VirtualState Virtual { get; set; } = VirtualState.Normal;
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        ClassEnabled = sync.Sync(nameof(ClassEnabled), ClassEnabled, SyncFlag.AffectsOthers, true);
        IsStatic = sync.Sync(nameof(IsStatic), IsStatic);
        Access = sync.Sync(nameof(Access), Access, SyncFlag.None, AccessState.Public);
        Virtual = sync.Sync(nameof(Virtual), Virtual, SyncFlag.None, VirtualState.Normal);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        setup.InspectorField(ClassEnabled, new ViewProperty(nameof(ClassEnabled), "Enabled"));

        if (ClassEnabled)
        {
            setup.InspectorField(IsStatic, new ViewProperty(nameof(IsStatic), "Static"));
            setup.InspectorField(Access, new ViewProperty(nameof(Access), "Access"));
            setup.InspectorField(Virtual, new ViewProperty(nameof(Virtual), "Virtual"));
        }
    }

    public override string ToString()
    {
        return L("Custom Class");
    }
}
#endregion
#region CustomFormatterAttribute
/// <summary>
/// Specifies custom formatter options.
/// </summary>
[NativeType(CodeBase = "*Design", Name = "CustomFormatter", Description = "Custom Formatter", Icon = "*CoreIcon|Format")]
public class CustomFormatterAttribute : DesignAttribute, IViewObject
{
    /// <summary>
    /// Gets or sets whether formatter is enabled.
    /// </summary>
    public bool FormatterEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets whether read is supported.
    /// </summary>
    public bool Read { get; set; } = true;

    /// <summary>
    /// Gets or sets whether write is supported.
    /// </summary>
    public bool Write { get; set; } = true;

    /// <summary>
    /// Gets or sets whether clone is supported.
    /// </summary>
    public bool Clone { get; set; }

    /// <summary>
    /// Gets or sets whether object equals is supported.
    /// </summary>
    public bool ObjectEquals { get; set; }

    /// <summary>
    /// Gets or sets whether exchange is supported.
    /// </summary>
    public bool Exchange { get; set; }
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        FormatterEnabled = sync.Sync(nameof(FormatterEnabled), FormatterEnabled, SyncFlag.AffectsOthers, true);
        Read = sync.Sync(nameof(Read), Read, SyncFlag.None, true);
        Write = sync.Sync(nameof(Write), Write, SyncFlag.None, true);
        Clone = sync.Sync(nameof(Clone), Clone);
        ObjectEquals = sync.Sync(nameof(ObjectEquals), ObjectEquals);
        Exchange = sync.Sync(nameof(Exchange), Exchange);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        setup.InspectorField(FormatterEnabled, new ViewProperty(nameof(FormatterEnabled), "Enabled"));

        if (FormatterEnabled)
        {
            setup.InspectorField(Read, new ViewProperty(nameof(Read), "Read"));
            setup.InspectorField(Write, new ViewProperty(nameof(Write), "Write"));
            setup.InspectorField(Clone, new ViewProperty(nameof(Clone), "Clone"));
            setup.InspectorField(ObjectEquals, new ViewProperty(nameof(ObjectEquals), "Equals Chec"));
            setup.InspectorField(Exchange, new ViewProperty(nameof(Exchange), "Exchange "));
        }
    }

    public override string ToString()
    {
        return L("Custom Formatte");
    }
}
#endregion
#region DisabledAttribute
/// <summary>
/// Indicates that the associated element is disabled.
/// </summary>
[NativeType(CodeBase = "*Design", Name = "Disabled", Description = "Disabled", Icon = "*CoreIcon|Disable")]
public class DisabledAttribute : DesignAttribute, IViewObject
{
    public override string ToString() => "Disabled";
}
#endregion
#region HiddenAttribute
/// <summary>
/// Indicates that the associated element is hidden.
/// </summary>
[NativeType(CodeBase = "*Design", Name = "Hidden", Description = "Hidden", Icon = "*CoreIcon|Hide")]
public class HiddenAttribute : DesignAttribute, IViewObject
{
    public override string ToString()
    {
        return L("Hidden");
    }
}
#endregion
#region HiddenInMonitorAttribute
/// <summary>
/// Indicates that the associated element is hidden in monitor views.
/// </summary>
[NativeType(CodeBase = "*Design", Name = "HiddenInMonitor", Description = "Hidden in Monitor", Icon = "*CoreIcon|Hide")]
public class HiddenInMonitorAttribute : DesignAttribute, IViewObject
{
    public override string ToString()
    {
        return L("Hidden in Monitor");
    }
}
#endregion
#region PacketFormatAttribute
/// <summary>
/// Specifies the packet serialization format.
/// </summary>
[NativeType(CodeBase = "*Design", Name = "PacketFormat", Description = "Packet Serialization Format", Icon = "*CoreIcon|Format")]
public class PacketFormatAttribute : DesignAttribute, IViewObject
{
    /// <summary>
    /// Gets or sets the packet format.
    /// </summary>
    public PacketFormats PacketFormat { get; set; }
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        setup.InspectorField(PacketFormat, new ViewProperty("Format", "Format"));
    }

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        PacketFormat = sync.Sync("Format", PacketFormat, SyncFlag.None, PacketFormats.Default);
    }

    public override string ToString()
    {
        return L("Packet Serialization Format");
    }
}
#endregion
#region ValueTypeStructAttribute
/// <summary>
/// Indicates that the type is a value type struct.
/// </summary>
[NativeType(CodeBase = "*Design", Name = "ValueTypeStruct", Description = "Value Type Struct", Icon = "*CoreIcon|Value")]
public class ValueTypeStructAttribute : DesignAttribute, IViewObject
{
    public override string ToString()
    {
        return L("Value Type Struct");
    }
}
#endregion
#region DataIdFieldAttribute
/// <summary>
/// Indicates that this field contains a data ID.
/// </summary>
[NativeType(CodeBase = "*Design", Name = "DataIdField", Description = "Data ID Field", Icon = "*CoreIcon|Field")]
[Obsolete]
public class DataIdFieldAttribute : DesignAttribute, IViewObject
{
    public override string ToString()
    {
        return L("Data ID Field");
    }
}
#endregion
#region EditorGuidFieldAttribute
/// <summary>
/// Indicates that this field contains an editor GUID.
/// </summary>
[NativeType(CodeBase = "*Design", Name = "EditorGuidField", Description = "Editor GUID Field", Icon = "*CoreIcon|Field")]
[Obsolete]
public class EditorGuidFieldAttribute : DesignAttribute, IViewObject
{
    public override string ToString()
    {
        return L("Editor GUID Field");
    }
}
#endregion
#region PosisionFieldAttributes
/// <summary>
/// Defines the type of auto-fill field.
/// </summary>
public enum AutoFieldType
{
    [DisplayText("Data ID")]
    DataId,
    [DisplayText("Table ID")]
    TableId,

    [DisplayText("Local ID")]
    LocalId,

    [DisplayText("Asset GUID")]
    Guid,

    [DisplayText("Name")]
    Name,

    [DisplayText("Description")]
    Description,

    [DisplayText("Index")]
    Index,

    [DisplayText("Chart X Coordinate")]
    X,

    [DisplayText("Chart Y Coordinate")]
    Y,

    [DisplayText("Chart X Grid")]
    GridX,

    [DisplayText("Chart Y Grid")]
    GridY,
}

/// <summary>
/// Specifies that a field should be automatically filled with a system value.
/// </summary>
[NativeType(CodeBase = "*Design", Name = "PosisionXField", Description = "Auto-fill Field", Icon = "*CoreIcon|Field")]
[ToolTipsText("After adding attribute to a field, the field value will be automatically filled with the system's internal value based on its meaning when exporting data.")]
public class AutoFieldAttribute : DesignAttribute, IViewObject
{
    /// <summary>
    /// Gets or sets the auto field type.
    /// </summary>
    public AutoFieldType FieldType { get; set; }
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        FieldType = sync.Sync(nameof(FieldType), FieldType, SyncFlag.None);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        setup.InspectorField(FieldType, new ViewProperty(nameof(FieldType), "Field Type"));
    }

    public override string ToString()
    {
        return L($"Auto {FieldType} Field");
    }
}
#endregion
#region ConnectorAttribute
/// <summary>
/// Indicates that this field is a connector.
/// </summary>
[NativeType(CodeBase = "*Design", Name = "Connector", Description = "Connector", Icon = "*CoreIcon|Connect")]
[ToolTipsText("Indicates that this field is a connector when added to a field.")]
public class ConnectorAttribute : DesignAttribute, IViewObject
{
    public override string ToString() => L("Connector");
}
#endregion
#region AssociateAttribute
/// <summary>
/// Indicates that this field is an association port.
/// </summary>
[NativeType(CodeBase = "*Design", Name = "Associate", Description = "Association Port", Icon = "*CoreIcon|Connect")]
[ToolTipsText("Indicates that this field can display connection lines as associative connections in the flowchart when added to a field.")]
public class AssociateAttribute : DesignAttribute, IViewObject
{
    /// <summary>
    /// Gets or sets the flow direction.
    /// </summary>
    public FlowDirections Direction { get; set; }
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        Direction = sync.Sync(nameof(Direction), Direction, SyncFlag.None);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        setup.InspectorField(Direction, new ViewProperty(nameof(Direction), "Direction"));
    }

    public override string ToString()
    {
        return L("Association Port") + ": " + Direction.ToDisplayText();
    }
}
#endregion
#region ConsistencyAttribute
/// <summary>
/// Indicates that the value should be consistent within the same category.
/// </summary>
[NativeType(CodeBase = "*Design", Name = "Consistency", Description = "Consistency", Icon = "*CoreIcon|Consistency")]
[ToolTipsText("Adding this attribute to a complex field keeps the value of this field consistent within the same category.")]
public class ConsistencyAttribute : DesignAttribute
{
    public override string ToString() => L("Consistency");
}
#endregion
#region ClassifyAttribute
/// <summary>
/// Indicates that the enum field provides classifications for data tables.
/// </summary>
[NativeType(CodeBase = "*Design", Name = "Classify", Description = "Classification", Icon = "*CoreIcon|Classify")]
[ToolTipsText("Adding this attribute to an enum field allows creating classifications for data tables of this type based on each value of the enum.")]
public class ClassifyAttribute : DesignAttribute
{
    public override string ToString() => L("Classification");
}
#endregion
#region HorizontalLayoutAttribute
/// <summary>
/// Specifies horizontal layout options for a type.
/// </summary>
[NativeType(CodeBase = "*Design", Name = "HorizontalLayout", Description = "Horizontal Layout", Icon = "*CoreIcon|Layout")]
[ToolTipsText("Indicates that this type supports horizontal layout when added to a type.")]
public class HorizontalLayoutAttribute : DesignAttribute, IViewObject
{
    private static readonly ViewProperty PreviewFieldOnly_VP = new ViewProperty(nameof(PreviewFieldOnly), "Preview Field Only");
    private static readonly ViewProperty FirstColumnPercentage_VP = new ViewProperty(nameof(FirstColumnPercentage), "First Column Width Percentage")
            .WithAttribute(new SObject(new NumericRangeAttribute { Min = 0, Max = 100, Increment = 1 }))
            .WithUnit("%");

    /// <summary>
    /// Gets or sets the first column width percentage.
    /// </summary>
    public float FirstColumnPercentage { get; set; }

    /// <summary>
    /// Gets or sets whether only preview field should be shown.
    /// </summary>
    public bool PreviewFieldOnly { get; set; }

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        PreviewFieldOnly = sync.Sync(nameof(PreviewFieldOnly), PreviewFieldOnly, SyncFlag.None, false);
        FirstColumnPercentage = sync.Sync(nameof(FirstColumnPercentage), FirstColumnPercentage, SyncFlag.None, 0);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        setup.InspectorField(PreviewFieldOnly, PreviewFieldOnly_VP);
        setup.InspectorField(FirstColumnPercentage, FirstColumnPercentage_VP);
    }

    public override string ToString()
    {
        if (FirstColumnPercentage > 0)
        {
            return L($"Horizontal Layout First Col {FirstColumnPercentage}%");
        }
        else
        {
            return L("Horizontal Layout");
        }
    }
}
#endregion
#region PreviewFieldAttribute
/// <summary>
/// Indicates that this field is a preview field.
/// </summary>
[NativeType(CodeBase = "*Design", Name = "PreviewField", Description = "Preview Field", Icon = "*CoreIcon|Preview")]
public class PreviewFieldAttribute : DesignAttribute, IViewObject
{
}
#endregion
#region NumericRangeAttribute
/// <summary>
/// Specifies a numeric range for a field value.
/// </summary>
[NativeType(CodeBase = "*Design", Name = "NumericRange", Description = "Numeric Range", Icon = "*CoreIcon|Range")]
public class NumericRangeAttribute : DesignAttribute, IViewObject
{
    static readonly Color DefaultColor1 = ColorTranslators.FromHtml("#0094FF");
    static readonly Color DefaultColor2 = ColorTranslators.FromHtml("#FF00DC");

    /// <summary>
    /// Gets or sets the minimum value.
    /// </summary>
    public decimal Min { get; set; } = 0m;

    /// <summary>
    /// Gets or sets the maximum value.
    /// </summary>
    public decimal Max { get; set; } = 1m;

    /// <summary>
    /// Gets or sets the increment value.
    /// </summary>
    public decimal Increment { get; set; } = 0.1m;

    /// <summary>
    /// Gets or sets whether to clamp the minimum value.
    /// </summary>
    public bool ClampMin { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to clamp the maximum value.
    /// </summary>
    public bool ClampMax { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show a color.
    /// </summary>
    public bool HasColor { get; set; } = false;

    /// <summary>
    /// Gets or sets the color.
    /// </summary>
    public Color Color { get; set; } = DefaultColor1;

    /// <summary>
    /// Gets or sets whether to show min/max colors.
    /// </summary>
    public bool HasMinMaxColor { get; set; } = false;

    /// <summary>
    /// Gets or sets the minimum color.
    /// </summary>
    public Color MinColor { get; set; } = DefaultColor1;

    /// <summary>
    /// Gets or sets the maximum color.
    /// </summary>
    public Color MaxColor { get; set; } = DefaultColor2;

    //Color? IViewColor.ViewColor => HasColor ? new Color?(Color) : null;

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        Min = sync.Sync(nameof(Min), Min, SyncFlag.None, 0);
        Max = sync.Sync(nameof(Max), Max, SyncFlag.None, 1);
        Increment = sync.Sync(nameof(Increment), Increment, SyncFlag.None, 0.1m);
        ClampMin = sync.Sync(nameof(ClampMin), ClampMin, SyncFlag.None, true);
        ClampMax = sync.Sync(nameof(ClampMax), ClampMax, SyncFlag.None, true);

        HasColor = sync.Sync(nameof(HasColor), HasColor, SyncFlag.None, false);
        Color = sync.Sync(nameof(Color), Color, SyncFlag.None, DefaultColor1);

        HasMinMaxColor = sync.Sync(nameof(HasMinMaxColor), HasMinMaxColor, SyncFlag.None, false);
        MinColor = sync.Sync(nameof(MinColor), MinColor, SyncFlag.None, DefaultColor1);
        MaxColor = sync.Sync(nameof(MaxColor), MaxColor, SyncFlag.None, DefaultColor2);

        if (Max < Min)
        {
            Max = Min;
        }
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        setup.InspectorField(Min, new ViewProperty(nameof(Min), "Min Value"));
        setup.InspectorField(Max, new ViewProperty(nameof(Max), "Max Value"));
        setup.InspectorField(Increment, new ViewProperty(nameof(Increment), "Increment"));
        setup.InspectorField(ClampMin, new ViewProperty(nameof(ClampMin), "Clamp Min"));
        setup.InspectorField(ClampMax, new ViewProperty(nameof(ClampMax), "Clamp Max"));

        setup.Label("Color");

        setup.InspectorField(HasColor, new ViewProperty(nameof(HasColor), "Show Color"));
        if (HasColor)
        {
            setup.InspectorField(Color, new ViewProperty(nameof(Color), "Color").WithColor(Color));
        }

        setup.InspectorField(HasMinMaxColor, new ViewProperty(nameof(HasMinMaxColor), "Show Range Color "));
        if (HasMinMaxColor)
        {
            setup.InspectorField(MinColor, new ViewProperty(nameof(MinColor), "Min Value Color").WithColor(MinColor));
            setup.InspectorField(MaxColor, new ViewProperty(nameof(MaxColor), "Max Value Color").WithColor(MaxColor));
        }
    }

    public override string ToString()
    {
        return L("Numeric Range") + $": {Min}-{Max}";
    }
}
#endregion
#region SourceCodeAutomationAttribute
/// <summary>
/// Specifies source code automation options.
/// </summary>
[NativeType(CodeBase = "*Design", Name = "SourceCodeAutomation", Description = "Source Code Automation Level", Icon = "*CoreIcon|Class")]
public class SourceCodeAutomationAttribute : DesignAttribute, IViewObject
{
    /// <summary>
    /// Gets or sets whether to get data from data storage.
    /// </summary>
    public bool DataStorage { get; set; } = true;
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        DataStorage = sync.Sync(nameof(DataStorage), DataStorage);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        setup.InspectorField(DataStorage, new ViewProperty(nameof(DataStorage), "Get Data from DataStorage"));
    }

    public override string ToString()
    {
        return L("Source Code Automation Level");
    }
}
#endregion
#region StatusColorAttribute
/// <summary>
/// Specifies a status color for display.
/// </summary>
[NativeType(CodeBase = "*Design", Name = "StatusColor", Description = "Status Color", Icon = "*CoreIcon|Color")]
public class StatusColorAttribute : DesignAttribute, IViewObject, IViewColor
{
    /// <summary>
    /// Gets or sets the text status.
    /// </summary>
    public TextStatus Status { get; set; }

    /// <summary>
    /// Gets the view color based on the status.
    /// </summary>
    public Color? ViewColor => EditorServices.ColorConfig.GetStatusColor(Status);

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        Status = sync.Sync(nameof(Status), Status);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        setup.InspectorField(Status, new ViewProperty(nameof(Status), "Status").WithColor(ViewColor));
    }

    public override string ToString()
    {
        return Status.ToString();
    }
}
#endregion
#region ColorAttribute
/// <summary>
/// Specifies a color value.
/// </summary>
[NativeType(CodeBase = "*Design", Name = "Color", Description = "Color", Icon = "*CoreIcon|Color")]
public class ColorAttribute : DesignAttribute, IViewObject, IViewColor
{
    private Color _color = Color.White;

    /// <summary>
    /// Gets or sets the view color.
    /// </summary>
    public Color? ViewColor
    {
        get => _color;
        set => _color = value ?? Color.White;
    }

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _color = sync.Sync("Color", _color, SyncFlag.NotNull);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        setup.InspectorField(_color, new ViewProperty("Color", "Color").WithColor(ViewColor));
    }

    public override string ToString()
    {
        return ColorTranslators.ToHtml(_color);
    }
}
#endregion
#region SaveDataAttribute
/// <summary>
/// Indicates that data should be saved.
/// </summary>
[NativeType(CodeBase = "*Design", Name = "SaveData", Description = "Save Data", Icon = "*CoreIcon|Save")]
public class SaveDataAttribute : DesignAttribute, IViewObject
{
    public override string ToString()
    {
        return L("Save Data");
    }
}
#endregion
