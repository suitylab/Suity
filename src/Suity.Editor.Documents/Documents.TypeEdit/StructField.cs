using Suity.Editor.Analyzing;
using Suity.Editor.CodeRender;
using Suity.Editor.Design;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Selecting;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using Suity.Views;
using Suity.Views.Im;
using Suity.Views.Named;
using System;
using System.Drawing;

namespace Suity.Editor.Documents.TypeEdit;

/// <summary>
/// Represents a field in a struct or abstract type, with type information, default value, and display options.
/// </summary>
[NativeAlias]
[DisplayText("Struct field", "*CoreIcon|Field")]
public class StructField : StructFieldItem,
    IDescriptionDisplay,
    IPreviewDisplay,
    INavigable,
    IVariable,
    IViewEditNotify
{
    private readonly FieldTypeDesign _fieldType;

    private object _defaultValue;
    private bool _optional;
    private bool _showInDetail;
    private string _unit;

    internal bool _cachedHasLabel;

    /// <inheritdoc/>
    public override EditorObject AssetField
    {
        get
        {
            var parent = ParentSItem;
            
            if (parent is StructType st)
            {
                return st.AssetBuilder?.Asset?.GetField(Name);
            }
            else if (parent is AbstractType abstractType)
            {
                return abstractType.AssetBuilder?.Asset?.GetField(Name);
            }

            return null;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StructField"/> class.
    /// </summary>
    public StructField()
    {
        _fieldType = new(this);
        _fieldType.BaseType.SelectedKey = "*System|String";
        _fieldType.FieldTypeChanged += (s, e) =>
        {
            NotifyFieldUpdated();
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StructField"/> class with the specified name.
    /// </summary>
    /// <param name="name">The name of the field.</param>
    public StructField(string name)
        : this()
    {
        Name = name;
    }

    /// <summary>
    /// Gets the field type design for this field.
    /// </summary>
    public FieldTypeDesign FieldType => _fieldType;

    /// <summary>
    /// Gets or sets the default value for this field.
    /// </summary>
    public object DefaultValue
    {
        get => _defaultValue;
        set => Set(ref _defaultValue, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether this field is optional.
    /// </summary>
    public bool Optional
    {
        get => _optional;
        set => Set(ref _optional, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether this field should be shown in the detail view.
    /// </summary>
    public bool ShowInDetail
    {
        get => _showInDetail;
        set => Set(ref _showInDetail, value);
    }

    /// <summary>
    /// Gets or sets the unit of measurement for this field.
    /// </summary>
    public string Unit
    {
        get => _unit;
        set => Set(ref _unit, value);
    }

    /// <inheritdoc/>
    public string DisplayText => !string.IsNullOrEmpty(Description) ? Description : Name;

    /// <inheritdoc/>
    protected override TextStatus OnGetTextStatus()
    {
        TextStatus textStatus = base.OnGetTextStatus();
        if (Attributes.GetIsHiddenOrDisabled())
        {
            return textStatus > TextStatus.Disabled ? textStatus : TextStatus.Disabled;
        }
        else
        {
            return textStatus;
        }
    }

    /// <inheritdoc/>
    protected override string OnGetSuggestedPrefix()
    {
        return "Field";
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        sync.Sync("Type", _fieldType, SyncFlag.GetOnly | SyncFlag.AffectsOthers);

        DefaultValue = sync.Sync("DefaultValue", DefaultValue);
        UpdateDefaultValue();

        Optional = sync.Sync("Nullable", Optional, SyncFlag.None, false);
        ShowInDetail = sync.Sync("ShowInDetail", ShowInDetail);
        Unit = sync.Sync("Unit", Unit);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        UpdateDefaultValue();

        if (setup.SupportInspector())
        {
            setup.InspectorField(FieldType, new ViewProperty("Type", "Type") { Expand = true });
            setup.InspectorFieldOfType(FieldType.FieldType, new ViewProperty("DefaultValue", "Default Value"));
            setup.InspectorField(Optional, new ViewProperty("Nullable", "Optional")); //TODO: Change to Optional
            if (FieldType.FieldType.CanShowInDetailView())
            {
                setup.InspectorField(ShowInDetail, new ViewProperty("ShowInDetail", "Expanded"));
            }
            setup.InspectorField(Unit, new ViewProperty("Unit", "Unit"));
        }
    }

    /// <inheritdoc/>
    public override void CollectProblem(AnalysisProblem problems, AnalysisIntents intent)
    {
        base.CollectProblem(problems, intent);

        if (TypeDefinition.IsNullOrBroken(FieldType.FieldType))
        {
            string typeString = FieldType.TypeString;
            if (!string.IsNullOrWhiteSpace(typeString))
            {
                problems.Add(new AnalysisProblem(TextStatus.Error, "Type undefined:" + typeString));
            }
            else
            {
                problems.Add(new AnalysisProblem(TextStatus.Error, "Type undefined"));
            }                
        }

        var baseType = (ParentSItem as StructType)?.BaseTypeTarget;
        if (baseType != null)
        {
            if (baseType.GetField(Name) != null)
            {
                problems.Add(new AnalysisProblem(TextStatus.Error, $"Field {Name} already defined in base class, should be removed."));
            }
        }
    }

    /// <summary>
    /// Updates the default value based on the current field type.
    /// </summary>
    private void UpdateDefaultValue()
    {
        if (!TypeDefinition.IsNullOrBroken(FieldType.FieldType))
        {
            DefaultValue = FieldType.SyncDefaultValue(DefaultValue, this.GetAssetFilter());
        }
    }

    /// <inheritdoc/>
    protected override void OnDrawPreviewImGui(ImGui gui)
    {
        TypeDefinition type = _fieldType.BaseType.GetTypeDefinition();
        Image icon = _fieldType.Icon;
        Color color = type?.Target?.ViewColor ?? type?.Target?.TypeColor ?? EditorServices.ColorConfig.GetStatusColor(TextStatus.Preview);
        string text = _fieldType.DisplayText;

        var fieldType = _fieldType.FieldType;
        string toolTips = fieldType?.GetFullTypeName() ?? string.Empty;

        if (TypeDefinition.IsNullOrEmpty(fieldType))
        {
            color = EditorServices.ColorConfig.GetStatusColor(TextStatus.Warning);
        }

        var node = gui.HorizontalFrame("typePreview")
        .InitClass("refBox")
        .InitFit()
        .OverrideColor(color)
        .InitOverridePadding(0, 0, 5, 5)
        .OnContent(() =>
        {
            if (icon != null)
            {
                if (_fieldType.IsArray)
                {
                    gui.Image(icon).InitClass("iconDark");
                    //.SetImageFilter(color.Add(-50, -50, -50));
                }
                else
                {
                    gui.Image(icon).InitClass("icon");
                }
            }
            gui.Text(text).InitClass("numBoxText").SetFontColor(Color.Black).InitToolTips(toolTips);
        });
    }

    #region IPreviewDisplay

    /// <inheritdoc/>
    string IPreviewDisplay.PreviewText
    {
        get
        {
            if (DefaultValue != null)
            {
                if (DefaultValue is string)
                {
                    if (!string.IsNullOrEmpty(DefaultValue as string))
                    {
                        return $"{FieldType}=\"{DefaultValue}\"";
                    }
                    else
                    {
                        return FieldType.ToString();
                    }
                }
                else
                {
                    return $"{FieldType}={DefaultValue}";
                }
            }
            else
            {
                return FieldType.ToString();
            }
        }
    }

    /// <inheritdoc/>
    object IPreviewDisplay.PreviewIcon => FieldType.FieldType.GetIcon();

    #endregion

    #region INavigable

    /// <inheritdoc/>
    object INavigable.GetNavigationTarget()
    {
        return FieldType.FieldType;
    }

    #endregion

    #region IVariable Members

    /// <inheritdoc/>
    IMemberContainer IMember.Container => ParentSItem as IMemberContainer;

    /// <inheritdoc/>
    string INamed.Name => this.Name;

    /// <inheritdoc/>
    Guid IHasId.Id => (ParentSItem?.TargetAsset as DCompond)?.GetField(Name)?.Id ?? Guid.Empty;

    /// <inheritdoc/>
    TypeDefinition IVariable.VariableType => this.FieldType.FieldType;

    /// <inheritdoc/>
    string IVariable.DisplayName => this.DisplayText;

    /// <inheritdoc/>
    bool IVariable.IsParameter => true;

    /// <inheritdoc/>
    object IVariable.DefaultValue => this.DefaultValue;

    #endregion

    #region INavigationItem Members

    /// <inheritdoc/>
    string ISelectionItem.SelectionKey => this.Name;

    #endregion

    #region IAssetContext Members

    /// <inheritdoc/>
    Asset IHasAsset.TargetAsset => ParentSItem?.TargetAsset;

    #endregion

    /// <inheritdoc/>
    public override void Find(ValidationContext context, string findStr, SearchOption findOption)
    {
        base.Find(context, findStr, findOption);

        if (_defaultValue != null)
        {
            string defaultStr = _defaultValue.ToString();
            if (Validator.Compare(defaultStr, findStr, findOption))
            {
                context.Report(defaultStr, this);
            }
        }

        if (Validator.Compare(_unit, findStr, findOption))
        {
            context.Report(_unit, this);
        }
    }
}
