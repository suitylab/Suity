using static Suity.Helpers.GlobalLocalizer;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Selecting;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Helpers;
using Suity.Selecting;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Im;
using Suity.Views.Named;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Suity.Editor.Services;

namespace Suity.Editor.Design;

/// <summary>
/// Base class for design items in the editor.
/// </summary>
public abstract class DesignItem : SNamedItem,
    IDescriptionDisplay, IPreviewDisplay,
    IDesignObject, IHasAttributeDesign, IAttributeGetter,
    IViewRedirect,
    ISelectionItem,
    IDrawEditorImGui,
    IViewColor,
    IHasToolTips,
    IInspectorSplittedView
{
    private string _description = string.Empty;
    private AssetSelection<ImageAsset> _iconSelection = new();
    private Color _color = Color.Empty;

    private AssetAccessMode _accessMode;
    private AssetInstanceMode _instanceMode;

    private readonly SArrayAttributeDesign _attributes = new();
    readonly IDesignBuilder _designBuilder;

    protected DesignItem()
        : base()
    { }

    protected DesignItem(AssetBuilder builder)
        : base(builder)
    {
        _designBuilder = builder as IDesignBuilder;
        _designBuilder?.UpdateAttributes(_attributes);

        _attributes.AttributeAdded += attr => _designBuilder?.UpdateAttributes(_attributes);
        _attributes.AttributeRemoved += attr => _designBuilder?.UpdateAttributes(_attributes);
    }

    /// <summary>
    /// Gets or sets the access mode for this item.
    /// </summary>
    public AssetAccessMode AccessMode
    {
        get => _accessMode;
        set
        {
            if (_accessMode != value)
            {
                _accessMode = value;
                AssetBuilder?.SetAccessMode(value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the instance mode for this item.
    /// </summary>
    public AssetInstanceMode InstanceMode
    {
        get => _instanceMode;
        set
        {
            if (_instanceMode != value)
            {
                _instanceMode = value;
                AssetBuilder?.SetInstanceMode(value);
            }
        }
    }
    
    /// <summary>
    /// Gets the type icon for this item.
    /// </summary>
    public virtual Image TypeIcon => null;

    /// <summary>
    /// Gets the type color for this item.
    /// </summary>
    public virtual Color? TypeColor => null;

    #region Displaying

    /// <summary>
    /// Gets or sets the description of this item.
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
            AssetBuilder?.SetDescription(value);
        }
    }

    /// <summary>
    /// Gets or sets the icon selection for this item.
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
                AssetBuilder?.SetIconId(_iconSelection.Id);
            }
        }
    }

    /// <summary>
    /// Gets or sets the icon ID for this item.
    /// </summary>
    public virtual Guid IconId
    {
        get => _iconSelection.Id;
        set
        {
            if (_iconSelection.Id != value)
            {
                _iconSelection.Id = value;
                AssetBuilder.SetIconId(value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the icon key for this item.
    /// </summary>
    public virtual string IconKey
    {
        get => _iconSelection.SelectedKey;
        set
        {
            if (_iconSelection.SelectedKey != value)
            {
                _iconSelection.SelectedKey = value;
                AssetBuilder.SetIconKey(value);
            }
        }
    }

    /// <summary>
    /// Gets the selected icon for this item.
    /// </summary>
    public Image SelectedIcon => _iconSelection.Target?.GetIconSmall();

    /// <summary>
    /// Gets or sets the color for this item.
    /// </summary>
    public Color Color
    {
        get
        {
            if (_color != Color.Empty)
            {
                return _color;
            }

            return OnGetColor() ?? Color.Empty;
        }
        set
        {
            if (_color == value)
            {
                return;
            }

            _color = value;
            AssetBuilder.SetColor(_color != Color.Empty ? _color : (Color?)null);
        }
    }

    /// <summary>
    /// Gets the category path for this item based on its parent nodes.
    /// </summary>
    public string Category
    {
        get
        {
            string c = null;
            var node = ParentNode;
            while (node != null)
            {
                string name = (node as NamedGroup)?.GroupName ?? node.Name;

                if (string.IsNullOrEmpty(c))
                {
                    c = name;
                }
                else
                {
                    c = $"{name}.{c}";
                }
                node = node.ParentNode;
            }

            return c;
        }
    }

    public override string DisplayText => !string.IsNullOrEmpty(_description) ? _description : Name;

    protected virtual object OnGetRedirectedObject(int viewId) => this;

    protected override TextStatus OnGetTextStatus()
    {
        var status = base.OnGetTextStatus();
        if (status != TextStatus.Normal)
        {
            return status;
        }

        if (_attributes.GetIsHiddenOrDisabled())
        {
            return TextStatus.Disabled;
        }

        return TextStatus.Normal;
    }

    #endregion

    #region IPreviewDisplay

    public virtual string PreviewText => string.Empty;
    public virtual object PreviewIcon => null;

    #endregion

    #region IViewDesignObject

    SArray IDesignObject.DesignItems => _attributes.Array;
    string IDesignObject.DesignPropertyName => "Attributes";
    string IDesignObject.DesignPropertyDescription => "Property";

    #endregion

    #region IAttributeDesign

    /// <summary>
    /// Gets the attribute design for this item.
    /// </summary>
    public IAttributeDesign Attributes => _attributes;

    #endregion

    #region IHasAttribute

    public IEnumerable<object> GetAttributes() => _attributes.GetAttributes();

    public IEnumerable<object> GetAttributes(string typeName) => _attributes.GetAttributes(typeName);

    public IEnumerable<T> GetAttributes<T>() where T : class => _attributes.GetAttributes<T>();

    #endregion

    #region IViewEditNotify

    protected override void OnViewEdited(object obj, string propertyName)
    {
        base.OnViewEdited(obj, propertyName);

        if (obj is SItem item && item.Root == _attributes.Array)
        {
            _designBuilder?.UpdateAttributes(_attributes);
        }
        else if (obj is IDesignValue design && design.Value.Root == _attributes.Array)
        {
            _designBuilder?.UpdateAttributes(_attributes);
        }
    }

    #endregion

    #region IViewRedirect

    object IViewRedirect.GetRedirectedObject(int viewId) => OnGetRedirectedObject(viewId);

    #endregion

    #region Virtual

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        Description = sync.Sync("Description", Description, SyncFlag.NotNull);
        IconSelection = sync.Sync("Icon", IconSelection, SyncFlag.NotNull);
        // Do not directly read Color property because Color property reads the overridden value from OnGetColor().
        Color = sync.Sync("Color", _color, SyncFlag.None, Color.Empty);
        sync.Sync("Attributes", _attributes.Array, SyncFlag.GetOnly);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        if (setup.SupportExtended())
        {
            setup.ExtendedField(_attributes.Array, new ViewProperty("Attributes", "Property"));
        }
    }

    protected override void OnSetupViewAppearance(IViewObjectSetup setup)
    {
        base.OnSetupViewAppearance(setup);

        setup.Label(new ViewProperty("#Appearance", "Appearance", CoreIconCache.View));

        setup.InspectorField(_description, new ViewProperty("Description", "Description"));
        setup.InspectorField(_iconSelection, new ViewProperty("Icon", "Icon"));
        setup.InspectorField(_color, new ViewProperty("Color", "Color", CoreIconCache.Color)
            .WithColor(_color != Color.Empty ? _color : (Color?)null));
    }

    protected override Image OnGetIcon() => SelectedIcon;

    protected override IEnumerable<Guid> OnFilterUsingList(IEnumerable<Guid> ids)
    {
        return ids.Where(o => o.ToAsset() != null);
    }

    #endregion

    #region IDrawEditorImGui

    public virtual bool OnEditorGui(ImGui gui, EditorImGuiPipeline pipeline, IDrawContext context)
    {
        if (pipeline == EditorImGuiPipeline.Preview)
        {
            string preview = L(this.PreviewText);
            if (!string.IsNullOrWhiteSpace(preview))
            {
                var node = gui.HorizontalFrame("frame")
                .InitClass("refBox")
                .OverrideColor(TypeColor ?? EditorServices.ColorConfig.GetStatusColor(TextStatus.Preview))
                .OnContent(() =>
                {
                    var icon = TypeIcon;
                    if (icon != null)
                    {
                        gui.Image(icon).InitClass("icon");
                    }

                    gui.Text(preview).InitClass("numBoxText").SetFontColor(Color.Black).SetToolTipsL(preview);

                    if (FieldList?.Count > 0)
                    {
                        gui.Frame("inner")
                        .InitClass("numBoxDark")
                        .OnContent(() =>
                        {
                            gui.Text("text", FieldList.Count.ToString()).InitClass("numBoxText");
                        });
                    }
                });
            }

            int num = 0;
            foreach (var item in _attributes.Array.Items.OfType<SItem>())
            {
                if (item.GetIcon() is { } icon)
                {
                    gui.Image($"#icon{num}", icon)
                    .InitClass("icon")
                    .SetToolTipsL(AssetManager.Instance.GetAsset(item.TypeId)?.ToDisplayText());

                    num++;
                }
            }

            EditorServices.ImGuiService.DrawItem(gui, this, pipeline, context);
            return true;
        }

        EditorServices.ImGuiService.DrawItem(gui, this, pipeline, context);
        return false;
    }

    #endregion

    #region IViewColor

    Color? IViewColor.ViewColor
    {
        get
        {
            var c = _attributes.GetAttribute<IViewColor>()?.ViewColor;
            if (c.HasValue && c.Value != Color.Empty)
            {
                return c;
            }

            return Color.ToNullable();
        }
    }

    #endregion

    #region IHasToolTips

    string IHasToolTips.ToolTips => this.Attributes.GetAttribute<ToolTipsAttribute>()?.ToolTips;


    #endregion

    public override string ToString() => $"{GetType().Name} {Name}";
}

/// <summary>
/// Generic base class for design items with a specific asset builder type.
/// </summary>
/// <typeparam name="TAssetBuilder">The type of asset builder.</typeparam>
public abstract class DesignItem<TAssetBuilder> : DesignItem
    where TAssetBuilder : AssetBuilder, new()
{
    private readonly TAssetBuilder _builder;

    /// <summary>
    /// Initializes a new instance of the <see cref="DesignItem{TAssetBuilder}"/> class.
    /// </summary>
    protected DesignItem()
        : base(new TAssetBuilder())
    {
        _builder = (TAssetBuilder)base.AssetBuilder;
    }

    /// <summary>
    /// Gets the asset builder for this item.
    /// </summary>
    protected new TAssetBuilder AssetBuilder => _builder;
}