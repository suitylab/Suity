using Suity.Editor.Documents.Linked;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Im;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Editor.Design;

/// <summary>
/// Base class for design documents in the editor.
/// </summary>
public abstract class DesignDocument : SNamedDocument,
    IHasAttributeDesign,
    IAttributeGetter,
    IDesignObject,
    IDrawEditorImGui,
    IViewColor,
    IHasToolTips
{
    private readonly SArrayAttributeDesign _attributes = new();
    readonly IDesignBuilder _dTypeBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="DesignDocument"/> class.
    /// </summary>
    protected DesignDocument()
        : base()
    {
        _dTypeBuilder = base.AssetBuilder as IDesignBuilder;
        _dTypeBuilder?.UpdateAttributes(_attributes);

        _attributes.AttributeAdded += OnAttributeModified;
        _attributes.AttributeRemoved += OnAttributeModified;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DesignDocument"/> class with the specified builder.
    /// </summary>
    protected DesignDocument(AssetBuilder builder)
        : base(builder)
    {
        _dTypeBuilder = base.AssetBuilder as IDesignBuilder;
        _dTypeBuilder?.UpdateAttributes(_attributes);
    }

    #region IAttributeDesign

    /// <summary>
    /// Gets the attribute design for this document.
    /// </summary>
    public IAttributeDesign Attributes => _attributes;

    private void OnAttributeModified(DesignAttribute attr)
    {
        _dTypeBuilder?.UpdateAttributes(_attributes);
        MarkDirty(_attributes);
    }

    #endregion

    #region IHasAttribute

    public IEnumerable<object> GetAttributes() => _attributes.GetAttributes();

    public IEnumerable<object> GetAttributes(string typeName) => _attributes.GetAttributes(typeName);

    public IEnumerable<T> GetAttributes<T>() where T : class => _attributes.GetAttributes<T>();

    #endregion

    #region IViewDesignObject

    SArray IDesignObject.DesignItems => _attributes.Array;
    string IDesignObject.DesignPropertyName => "Attributes";
    string IDesignObject.DesignPropertyDescription => "Property";

    #endregion

    #region IViewEditNotify

    protected override void OnViewEdited(object obj, string propertyName)
    {
        base.OnViewEdited(obj, propertyName);

        if (obj is SItem item && item.Root == _attributes.Array)
        {
            _dTypeBuilder?.UpdateAttributes(_attributes);
        }
        else if (obj is IDesignValue design && design.Value.Root == _attributes.Array)
        {
            _dTypeBuilder?.UpdateAttributes(_attributes);
        }
    }

    #endregion

    #region IDrawEditorImGui

    public virtual bool OnEditorGui(ImGui gui, EditorImGuiPipeline pipeline, IDrawContext context)
    {
        if (pipeline == EditorImGuiPipeline.Preview)
        {
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

    #region Virtual

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

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

    public override SNamedGroup CreateGroup() => new DesignGroup();

    #endregion
}

/// <summary>
/// Generic base class for design documents with a specific asset builder type.
/// </summary>
/// <typeparam name="TAssetBuilder">The type of asset builder.</typeparam>
public abstract class DesignDocument<TAssetBuilder> : DesignDocument
    where TAssetBuilder : AssetBuilder, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DesignDocument{TAssetBuilder}"/> class.
    /// </summary>
    public DesignDocument()
        : base(new TAssetBuilder())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DesignDocument{TAssetBuilder}"/> class with the specified builder.
    /// </summary>
    protected DesignDocument(TAssetBuilder builder)
        : base(builder)
    {
    }

    /// <summary>
    /// Gets the asset builder for this document.
    /// </summary>
    protected internal new TAssetBuilder AssetBuilder => (TAssetBuilder)base.AssetBuilder;
}