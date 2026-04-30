using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;

namespace Suity.Editor.Design;

/// <summary>
/// Base class for type design items.
/// </summary>
public abstract class TypeDesignItem : DesignItem,
    IViewEditNotify, IDesignObject, IViewColor, IHasToolTips
{
    private bool _isImported;
    private string _importName;

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeDesignItem"/> class with the specified builder.
    /// </summary>
    protected TypeDesignItem(AssetBuilder builder)
        : base(builder)
    {
    }

    /// <summary>
    /// Gets or sets whether this type is imported from an external source.
    /// </summary>
    public bool IsImported
    {
        get => _isImported;
        set
        {
            if (_isImported == value)
            {
                return;
            }

            _isImported = value;
            if (_isImported)
            {
                AssetBuilder.SetImportedId(_importName);
            }
            else
            {
                AssetBuilder.SetImportedId(null);
            }

            ShowRenderTargets = !value;
            ShowUsings = true;
        }
    }

    /// <summary>
    /// Gets or sets the import name for this type.
    /// </summary>
    public string ImportName
    {
        get => _importName;
        set
        {
            if (_importName == value)
            {
                return;
            }

            _importName = value;
            if (_isImported)
            {
                AssetBuilder.SetImportedId(_importName);
            }
            else
            {
                AssetBuilder.SetImportedId(null);
            }
        }
    }

    /// <summary>
    /// Gets the target type as DType.
    /// </summary>
    public DType TargetDType => TargetAsset as DType;

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        IsImported = sync.Sync("IsImported", IsImported, SyncFlag.AffectsOthers);
        ImportName = sync.Sync("ImportName", ImportName);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        if (setup.SupportInspector())
        {
            setup.InspectorField(_isImported, new ViewProperty("IsImported", "Import").WithToolTips("Imported types are implemented externally and do not generate source code."));
            if (_isImported)
            {
                setup.InspectorField(_importName, new ViewProperty("ImportName", "Import Name"));
            }
        }
    }

    protected override TextStatus OnGetTextStatus()
    {
        var status = base.OnGetTextStatus();
        if (status != TextStatus.Normal)
        {
            return status;
        }

        return _isImported ? TextStatus.Import : TextStatus.Normal;
    }
}

/// <summary>
/// Generic base class for type design items with a specific asset builder.
/// </summary>
/// <typeparam name="TAssetBuilder">The type of asset builder.</typeparam>
public abstract class TypeDesignItem<TAssetBuilder> : TypeDesignItem
    where TAssetBuilder : AssetBuilder, IDesignBuilder, new()
{
    private readonly TAssetBuilder _builder;

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeDesignItem{TAssetBuilder}"/> class.
    /// </summary>
    public TypeDesignItem()
        : base(new TAssetBuilder())
    {
        _builder = (TAssetBuilder)base.AssetBuilder;
    }

    /// <summary>
    /// Gets the asset builder.
    /// </summary>
    protected new TAssetBuilder AssetBuilder => _builder;
}