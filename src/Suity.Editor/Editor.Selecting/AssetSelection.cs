using Suity.Editor.Analyzing;
using Suity.Editor.Types;
using Suity.Selecting;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using Suity.Views;
using System;
using System.Drawing;

namespace Suity.Editor.Selecting;

#region AssetSelection
// Should be changed to abstract, ContentTypeName should also be abstract, dynamically read each time, should not be cached

/// <summary>
/// Base class for asset selections that provides functionality to select and reference assets.
/// </summary>
public abstract class AssetSelection : ISelection,
    IReference, 
    IHasId, 
    IHasAsset
{
    private readonly EditorAssetRef _assetRef = new();
    private IAssetFilter _filter = AssetFilters.Default;

    /// <summary>
    /// Initializes a new instance of the AssetSelection class.
    /// </summary>
    public AssetSelection()
    {
        _assetRef.TargetUpdated += _assetRef_TargetUpdated;
    }

    /// <summary>
    /// Initializes a new instance of the AssetSelection class with the specified asset filter.
    /// </summary>
    /// <param name="filter">The asset filter to apply.</param>
    public AssetSelection(IAssetFilter filter)
        : this()
    {
        _filter = filter ?? throw new ArgumentNullException(nameof(filter));
    }

    /// <summary>
    /// Event raised when the target asset is updated.
    /// </summary>
    public event EditorObjectEventHandler<EntryEventArgs> TargetUpdated;

    /// <summary>
    /// Gets the content type identifier for this selection.
    /// </summary>
    public abstract Guid ContentTypeId { get; }
    
    /// <summary>
    /// Gets the content type name for this selection.
    /// </summary>
    public abstract string ContentTypeName { get; }

    /// <summary>
    /// Gets whether the selection has a present (valid) target.
    /// </summary>
    public bool Present => _assetRef.Id != Guid.Empty && _assetRef.Target != null;

    /// <summary>
    /// Tag filter. Must be reset after each Sync execution, otherwise it will be lost
    /// </summary>
    public virtual IAssetFilter Filter
    {
        get => _filter;
        set => _filter = value ?? AssetFilters.Default;
    }

    /// <summary>
    /// Gets or sets the unique identifier of the selected asset.
    /// </summary>
    public Guid Id
    {
        get => _assetRef.Id;
        set => _assetRef.Id = value;
    }

    /// <summary>
    /// Gets or sets the asset key of the selected asset.
    /// </summary>
    public string AssetKey
    {
        get => _assetRef.AssetKey;
        set => _assetRef.AssetKey = value;
    }

    /// <summary>
    /// Gets or sets whether listening to target updates is enabled.
    /// </summary>
    public bool ListenEnabled
    {
        get => _assetRef.ListenEnabled;
        set => _assetRef.ListenEnabled = value;
    }

    /// <summary>
    /// Gets or sets the target asset.
    /// </summary>
    public Asset TargetAsset
    {
        get => _assetRef.Target;
        set
        {
            if (_filter is null || _filter.FilterAsset(value))
            {
                _assetRef.Target = value;
            }
        }
    }

    public override string ToString() => _assetRef.ToString();

    /// <summary>
    /// Raises the target updated event.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    /// <returns>True if the event was handled; otherwise, false.</returns>
    protected bool RaiseObjectUpdated(EntryEventArgs args)
    {
        bool handled = false;
        TargetUpdated?.Invoke(this, args, ref handled);

        return handled;
    }

    private void _assetRef_TargetUpdated(object sender, EntryEventArgs e, ref bool handled)
    {
        TargetUpdated?.Invoke(this, e, ref handled);
    }

    //TODO: Try to set GroupSelectable so that group nodes can also be selected.

    #region ISelection

    /// <inheritdoc />
    public virtual bool IsValid
    {
        get
        {
            var target = _assetRef.Target;

            if (_assetRef.Id != Guid.Empty && target is null)
            {
                // Reference is missing
                return false;
            }

            if (target != null)
            {
                var collection = GetCollection();
                if (collection is null || collection.GetAsset(target.AssetKey, Filter) is null)
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Gets or sets the selected key.
    /// </summary>
    public string SelectedKey
    {
        get => _assetRef.AssetKey;
        set => _assetRef.AssetKey = value;
    }

    /// <summary>
    /// Gets the asset collection for this selection.
    /// </summary>
    /// <returns>The asset collection.</returns>
    public IGeneralAssetCollection GetCollection()
    {
        if (ContentTypeId != Guid.Empty)
        {
            return AssetManager.Instance.GetAssetCollection(ContentTypeId);
        }

        if (!string.IsNullOrEmpty(ContentTypeName))
        {
            return AssetManager.Instance.GetAssetCollection(ContentTypeName);
        }

        return null;
    }

    /// <summary>
    /// Gets the selection list for this selection.
    /// </summary>
    /// <returns>The selection list.</returns>
    public virtual ISelectionList GetList()
    {
        IAssetCollection<Asset> collection = GetCollection();

        if (collection != null)
        {
            return new GroupedAssetSelectionList<Asset>(collection, ContentTypeName, Filter);
        }
        else
        {
            return EmptySelectionList.Empty;
        }
    }

    #endregion

    #region Display

    /// <summary>
    /// Gets the display text for the selected asset.
    /// </summary>
    public virtual string DisplayText => _assetRef.Target?.DisplayText ?? SelectedKey;

    /// <summary>
    /// Gets the icon of the selected asset.
    /// </summary>
    public virtual Image Icon => _assetRef.Target?.Icon;

    #endregion

    #region Sync

    /// <summary>
    /// Synchronizes the selection data with the given property sync operation.
    /// </summary>
    /// <param name="sync">The property sync object.</param>
    /// <param name="context">The sync context.</param>
    public virtual void Sync(IPropertySync sync, ISyncContext context)
    {
        sync.SyncAssetRef(_assetRef, context);

        if (sync.Intent == SyncIntent.Clone)
        {
            Filter = sync.Sync(nameof(Filter), Filter, SyncFlag.AttributeMode | SyncFlag.ByRef);
        }
    }

    public virtual void ReferenceSync(SyncPath path, IReferenceSync sync)
    {
        _assetRef.Id = sync.SyncId(path, _assetRef.Id, null);
    }

    #endregion

    /// <summary>
    /// Validates whether the given asset is valid for this selection.
    /// </summary>
    /// <param name="asset">The asset to validate.</param>
    /// <returns>True if the asset is valid; otherwise, false.</returns>
    public virtual bool GetIsValid(Asset asset)
    {
        if (asset is null)
        {
            return false;
        }

        var collection = GetCollection();
        if (collection is null || collection.GetAsset(asset.AssetKey, Filter) is null)
        {
            return false;
        }

        return true;
    }
}
#endregion

#region AssetSelection<T>
/// <summary>
/// A strongly-typed asset selection that references an asset of type T.
/// </summary>
/// <typeparam name="T">The type of asset to select.</typeparam>
public class AssetSelection<T> : AssetSelection
    where T : class
{
    // This Selection cannot be mapped to TAsset because T can be an interface, not necessarily an Asset type.

    /// <summary>
    /// Initializes a new instance of the AssetSelection class.
    /// </summary>
    public AssetSelection()
    { }

    /// <summary>
    /// Initializes a new instance of the AssetSelection class with the specified asset filter.
    /// </summary>
    /// <param name="filter">The asset filter to apply.</param>
    public AssetSelection(IAssetFilter filter)
        : base(filter)
    { }

    /// <summary>
    /// Initializes a new instance of the AssetSelection class with the specified target.
    /// </summary>
    /// <param name="target">The asset to select.</param>
    public AssetSelection(T target)
    {
        Target = target;
    }

    /// <summary>
    /// Initializes a new instance of the AssetSelection class with the specified target and filter.
    /// </summary>
    /// <param name="target">The asset to select.</param>
    /// <param name="filter">The asset filter to apply.</param>
    public AssetSelection(T target, IAssetFilter filter)
        : base(filter)
    {
        Target = target;
    }

    /// <summary>
    /// Initializes a new instance of the AssetSelection class with the specified asset key.
    /// </summary>
    /// <param name="key">The asset key to select.</param>
    public AssetSelection(string key)
    {
        base.SelectedKey = key;
    }

    /// <summary>
    /// Initializes a new instance of the AssetSelection class with the specified asset key and filter.
    /// </summary>
    /// <param name="key">The asset key to select.</param>
    /// <param name="filter">The asset filter to apply.</param>
    public AssetSelection(string key, IAssetFilter filter)
        : base(filter)
    {
        base.SelectedKey = key;
    }

    /// <summary>
    /// Gets the content type of this selection.
    /// </summary>
    public Type ContentType => typeof(T);

    /// <inheritdoc />
    public override Guid ContentTypeId => Guid.Empty;

    /// <inheritdoc />
    public override string ContentTypeName => typeof(T).ResolveAssetTypeName();


    /// <summary>
    /// Gets or sets the selected target asset.
    /// </summary>
    public T Target
    {
        get => TargetAsset as T;
        set => TargetAsset = value as Asset;
    }

    /// <summary>
    /// Gets the target asset.
    /// </summary>
    /// <returns>The target asset.</returns>
    public T GetTarget() => Target;
}
#endregion

#region IdAssetSelection
/// <summary>
/// An asset selection that resolves content type by a type identifier.
/// </summary>
public class IdAssetSelection : AssetSelection
{
    private Guid _contentTypeId;
    private string _contentTypeName;

    /// <summary>
    /// Initializes a new instance of the IdAssetSelection class.
    /// </summary>
    public IdAssetSelection() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the IdAssetSelection class with the specified asset filter.
    /// </summary>
    /// <param name="filter">The asset filter to apply.</param>
    public IdAssetSelection(IAssetFilter filter) : base(filter)
    {
    }

    /// <summary>
    /// Initializes a new instance of the IdAssetSelection class with the specified content type id.
    /// </summary>
    /// <param name="typeId">The content type identifier.</param>
    public IdAssetSelection(Guid typeId) : base()
    {
        UpdateContentTypeId(typeId);
    }

    /// <summary>
    /// Initializes a new instance of the IdAssetSelection class with the specified content type id and filter.
    /// </summary>
    /// <param name="typeId">The content type identifier.</param>
    /// <param name="filter">The asset filter to apply.</param>
    public IdAssetSelection(Guid typeId, IAssetFilter filter) : base(filter)
    {
        UpdateContentTypeId(typeId);
    }

    /// <inheritdoc />
    public override Guid ContentTypeId => _contentTypeId;

    /// <inheritdoc />
    public override string ContentTypeName => _contentTypeName;

    /// <summary>
    /// Updates the content type identifier.
    /// </summary>
    /// <param name="typeId">The new content type identifier.</param>
    public void UpdateContentTypeId(Guid typeId)
    {
        _contentTypeId = typeId;
        var type = AssetManager.Instance.GetAsset(typeId) as DType;

        string contentTypeName = type?.NativeType?.FullName;
        if (!string.IsNullOrWhiteSpace(contentTypeName))
        {
            _contentTypeName = "*AssetLink|" + contentTypeName;
        }
        else
        {
            _contentTypeName = null;
        }
    }

    /// <inheritdoc />
    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        base.Sync(sync, context);

        if (sync.Intent == SyncIntent.Clone)
        {
            _contentTypeId = sync.Sync(nameof(ContentTypeId), _contentTypeId, SyncFlag.AttributeMode | SyncFlag.ByRef);
        }
    }
}
#endregion

#region SValueSelection

/// <summary>
/// A value asset selection that supports type definition and analysis.
/// </summary>
public class SValueSelection : AssetSelection<ValueAsset>, ISupportAnalysis, ITextDisplay
{
    private readonly object _caller;

    // Tested, this value may be an old value
    private TypeDefinition _valuetype;

    /// <summary>
    /// Initializes a new instance of the SValueSelection class.
    /// </summary>
    public SValueSelection()
    {
    }

    /// <summary>
    /// Initializes a new instance of the SValueSelection class with the specified type definition.
    /// </summary>
    /// <param name="type">The type definition for the value.</param>
    public SValueSelection(TypeDefinition type)
    {
        _valuetype = type;
    }

    /// <summary>
    /// Initializes a new instance of the SValueSelection class with the specified caller.
    /// </summary>
    /// <param name="caller">The caller object.</param>
    public SValueSelection(object caller)
    {
        _caller = caller;
    }

    /// <summary>
    /// Initializes a new instance of the SValueSelection class with the specified type definition and caller.
    /// </summary>
    /// <param name="type">The type definition for the value.</param>
    /// <param name="caller">The caller object.</param>
    public SValueSelection(TypeDefinition type, object caller)
    {
        _valuetype = type;
        _caller = caller;
    }

    /// <summary>
    /// Gets the value type definition.
    /// </summary>
    public TypeDefinition ValueType => _valuetype;

    /// <inheritdoc />
    public override ISelectionList GetList()
    {
        var collection = ValueManager.Instance.GetValueCollection(_valuetype);
        if (collection != null)
        {
            //return collection;
            return new GroupedAssetSelectionList<ValueAsset>(collection, ContentTypeName, Filter);
        }
        else
        {
            return EmptySelectionList.Empty;
        }
    }

    /// <inheritdoc />
    public AnalysisResult Analysis { get; set; }

    /// <summary>
    /// Gets the value with the specified condition.
    /// </summary>
    /// <param name="condition">The condition for getting the value.</param>
    /// <returns>The value asset.</returns>
    public object GetValue(ICondition condition = null)
    {
        var valueAsset = Target;

        return valueAsset?.GetValue(this, condition);
    }

    /// <inheritdoc />
    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        base.Sync(sync, context);

        if (sync.Intent == SyncIntent.Clone)
        {
            _valuetype = sync.Sync(nameof(ValueType), _valuetype, SyncFlag.ByRef | SyncFlag.NotNull);
        }
    }

    /// <inheritdoc />
    public void CollectProblem(AnalysisProblem problems, AnalysisIntents intent)
    {
        var value = this.Target;

        do
        {
            if (value == null)
            {
                problems.Add(new AnalysisProblem(TextStatus.Error, "Value not set"));
                break;
            }

            if (value.ValueType != _valuetype)
            {
                problems.Add(new AnalysisProblem(TextStatus.Error, "Value type mismatch"));
            }
        } while (false);
    }

    internal void UpdateValueType(TypeDefinition type)
    {
        _valuetype = type;
    }

    /// <inheritdoc />
    string ITextDisplay.DisplayText
    {
        get
        {
            ValueAsset v = this.Target;
            string s;

            if (v != null)
            {
                s = v?.GetValue(_caller, null)?.ToString() ?? "null";
            }
            else
            {
                s = base.ToString();
            }

            return s ?? string.Empty;
        }
    }

    /// <inheritdoc />
    object ITextDisplay.DisplayIcon => this.Target?.Icon ?? CoreIconCache.Value;

    /// <inheritdoc />
    TextStatus ITextDisplay.DisplayStatus => Analysis?.Status ?? TextStatus.Normal;
}

#endregion
