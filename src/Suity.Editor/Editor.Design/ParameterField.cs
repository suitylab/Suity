using Suity.Drawing;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Selecting;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Im;
using System.Drawing;

namespace Suity.Editor.Design;

/// <summary>
/// Base class for parameter fields in function design items.
/// </summary>
public abstract class ParameterField : SNamedField,
        IDescriptionDisplay,
        IPreviewDisplay,
        INavigable,
        IDrawEditorImGui
{
    private readonly ITypeDesign _varType;
    private object _defaultValue;
    private bool _optional = false;
    private string _description = string.Empty;
    private AssetSelection<ImageAsset> _iconSelection;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParameterField"/> class.
    /// </summary>
    public ParameterField()
    {
        _varType = DTypeManager.Instance.CreateTypeDesign(this);

        VariableType.BaseType.SelectedKey = "*System|String";
        VariableType.FieldTypeChanged += (s, e) =>
        {
            NotifyFieldUpdated();
        };

        _iconSelection = new AssetSelection<ImageAsset>();
    }

    /// <summary>
    /// Gets or sets whether this parameter is optional.
    /// </summary>
    public bool Optional
    {
        get => _optional;
        set => Set(ref _optional, value);
    }

    /// <summary>
    /// Gets or sets the description of this parameter.
    /// </summary>
    public string Description
    {
        get => _description;
        set => Set(ref _description, value);
    }

    /// <summary>
    /// Gets the type design for this parameter.
    /// </summary>
    public ITypeDesign VariableType => _varType;

    /// <summary>
    /// Gets or sets the default value for this parameter.
    /// </summary>
    public object DefaultValue
    {
        get => _defaultValue;
        set => Set(ref _defaultValue, value);
    }

    /// <summary>
    /// Gets the display text for this parameter.
    /// </summary>
    public string DisplayText => !string.IsNullOrEmpty(Description) ? Description : Name;

    /// <summary>
    /// Gets or sets the icon selection for this parameter.
    /// </summary>
    protected AssetSelection<ImageAsset> IconSelection
    {
        get => _iconSelection;
        set
        {
            value ??= new AssetSelection<ImageAsset>();

            if (_iconSelection.Id != value.Id)
            {
                _iconSelection = value;
                NotifyFieldUpdated();
            }
        }
    }

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        Description = sync.Sync("Description", Description, SyncFlag.NotNull);
        IconSelection = sync.Sync("Icon", IconSelection, SyncFlag.NotNull);

        sync.Sync("Type", VariableType, SyncFlag.GetOnly | SyncFlag.AffectsOthers);
        DefaultValue = sync.Sync("DefaultValue", DefaultValue);

        UpdateDefaultValue();

        Optional = sync.Sync("Nullable", Optional, SyncFlag.None, false); //TODO: Rename to Optional
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(Description, new ViewProperty("Description", "Description"));
        setup.InspectorField(IconSelection, new ViewProperty("Icon", "Icon"));
        setup.InspectorField(VariableType, new ViewProperty("Type", "Type") { Expand = true });

        UpdateDefaultValue();

        setup.InspectorFieldOfType(VariableType.FieldType, new ViewProperty("DefaultValue", "Default Value"));
        setup.InspectorField(Optional, new ViewProperty("Nullable", "Nullable"));
    }

    protected override ImageDef OnGetIcon()
    {
        ImageAsset icon = _iconSelection.Target;
        if (icon != null)
        {
            return icon.GetIconSmall();
        }
        else
        {
            return CoreIconCache.Field;
        }
    }

    protected override TextStatus OnGetTextStatus()
    {
        if (VariableType.BaseType.GetDType() != null)
        {
            return TextStatus.Normal;
        }
        else
        {
            return TextStatus.Error;
        }
    }

    private void UpdateDefaultValue()
    {
        DefaultValue = VariableType.SyncDefaultValue(DefaultValue, this.GetAssetFilter());
    }

    #region IDrawEditorImGui

    bool IDrawEditorImGui.OnEditorGui(ImGui gui, EditorImGuiPipeline pipeline, IDrawContext context)
    {
        if (pipeline == EditorImGuiPipeline.Preview)
        {
            TypeDefinition type = _varType.BaseType.GetTypeDefinition();
            ImageDef icon = _varType.Icon;
            Color color = type?.Target?.ViewColor ?? type?.Target?.TypeColor ?? EditorServices.ColorConfig.GetStatusColor(TextStatus.Preview);
            string text = _varType.DisplayText;
            //if (_defaultValue != null)
            //{
            //    text = $"{text} = {EditorUtility.GetDisplayString(_defaultValue)}";
            //}
            string toolTip = _varType.FieldType?.GetFullTypeName() ?? string.Empty;

            var node = gui.HorizontalFrame("typePreview")
            .InitClass("refBox")
            .InitFit()
            .OverrideColor(color)
            .InitOverridePadding(0, 0, 5, 5)
            .OnContent(() =>
            {
                if (icon != null)
                {
                    gui.Image(icon).InitClass("icon");
                }
                gui.Text(text).InitClass("numBoxText").SetFontColor(Color.Black).InitToolTips(toolTip);
            });

            EditorServices.ImGuiService.DrawItem(gui, this, pipeline, context);
            return true;
        }

        EditorServices.ImGuiService.DrawItem(gui, this, pipeline, context);
        return false;
    }

    #endregion

    #region IPreviewDisplay

    public virtual string PreviewText => VariableType.DisplayText;

    public virtual object PreviewIcon => VariableType.Icon;

    #endregion

    #region INavigable

    object INavigable.GetNavigationTarget() => VariableType.FieldType;

    #endregion

    #region IEntryItem Members

    public string SelectionKey => Name;

    #endregion
}