using Suity.Collections;
using Suity.Drawing;
using Suity.Editor.Analyzing;
using Suity.Editor.CodeRender;
using Suity.Helpers;
using Suity.Selecting;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Editor;

/// <summary>
/// Asset instance mode
/// </summary>
public enum AssetInstanceMode
{
    /// <summary>
    /// Access directly
    /// </summary>
    Normal,

    /// <summary>
    /// Access through instance
    /// </summary>
    Instance,
}

/// <summary>
/// Asset access mode
/// </summary>
public enum AssetAccessMode
{
    /// <summary>
    /// Public
    /// </summary>
    Public,

    /// <summary>
    /// Private
    /// </summary>
    Private,

    /// <summary>
    /// No access
    /// </summary>
    NoAccess,
}


/// <summary>
/// Asset
/// </summary>
public abstract class Asset : EditorObject, 
    ICodeRenderElement, 
    IViewObject,
    IViewColor,
    ISelectionItem,
    ITextDisplay
{
    /// <summary>
    /// File bunch extension
    /// </summary>
    public const string FileBunchExtension = ".sbunch";

    /// <summary>
    /// Code library extension
    /// </summary>
    public const string CodeLibraryExtension = ".scode";

    /// <summary>
    /// Log extension
    /// </summary>
    public const string LogExtension = ".slog";

    /// <summary>
    /// Meta extension
    /// </summary>
    public const string MetaExtension = ".smeta";

    /// <summary>
    /// Asset extension
    /// </summary>
    public const string SAssetExtension = ".sasset";

    internal readonly AssetExternal _ex;
    private AssetActivator _activator;
    private AssetBuilder _builder;

    private AssetAccessMode _accessMode;
    private AssetInstanceMode _instanceMode;

    private string _description;
    private Guid _iconId;
    private Color? _color;
    private string _previewText;

    private AnalysisProblem _problems;
    private bool _isLegacy;
    private bool _isPrimary;

    /// <summary>
    /// Initializes a new instance of Asset
    /// </summary>
    protected Asset()
    {
        _ex = AssetManager.Instance.CreateAssetExternal(this);
    }

    /// <summary>
    /// Initializes a new instance of Asset with local name
    /// </summary>
    protected Asset(string localName)
        : this()
    {
        _ex.LocalName = localName ?? string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of Asset with types
    /// </summary>
    protected Asset(params Type[] types)
        : this()
    {
        UpdateAssetTypes(types);
    }

    #region Naming, Parenting, Owner

    protected override string GetName() => ShortTypeName ?? string.Empty;

    /// <summary>
    /// Asset native type name
    /// </summary>
    public string NativeTypeName => this.GetType().FullName;

    /// <summary>
    /// Asset key
    /// </summary>
    public string AssetKey => _ex.AssetKey;

    /// <summary>
    /// Asset full name
    /// </summary>
    public override string FullName => _ex.AssetKey;

    /// <summary>
    /// Asset local name
    /// </summary>
    public string LocalName
    {
        get => _ex.LocalName;
        internal protected set => _ex.LocalName = value;
    }

    /// <summary>
    /// Parent asset
    /// </summary>
    public Asset ParentAsset
    {
        get => _ex.ParentAsset;
        internal set => _ex.ParentAsset = value;
    }

    /// <summary>
    /// Belonging asset library
    /// </summary>
    public LibraryAsset Library { get; internal set; }

    /// <summary>
    /// Parent object in hierarchy
    /// </summary>
    public override EditorObject Parent => _ex.ParentAsset;

    /// <summary>
    /// Root asset in hierarchy
    /// </summary>
    public Asset RootAsset => _ex.RootAsset;

    /// <summary>
    /// Checks if this asset contains the specified parent asset
    /// </summary>
    public bool ContainsParent(Asset asset) => _ex.ContainsParent(asset);

    /// <summary>
    /// Name space
    /// </summary>
    public string NameSpace
    {
        get => _ex.NameSpace;
        internal protected set => _ex.NameSpace = value;
    }

    /// <summary>
    /// Asset short type name
    /// </summary>
    public virtual string ShortTypeName => _ex.ShortTypeName;

    /// <summary>
    /// Asset full type name
    /// </summary>
    public string FullTypeName => _ex.ResourceName;

    /// <summary>
    /// Type Id from external import
    /// </summary>
    public string ImportedId
    {
        get => _ex.ImportedId;
        internal protected set => _ex.ImportedId = value;
    }

    /// <summary>
    /// Indicates whether the asset is imported externally
    /// </summary>
    public bool IsImported => !string.IsNullOrWhiteSpace(_ex.ImportedId);

    /// <summary>
    /// Indicates whether it can be exported to the library
    /// </summary>
    public virtual bool CanExportToLibrary => false;

    /// <summary>
    /// Get the storage object to which it belongs
    /// </summary>
    /// <returns></returns>
    public override object GetStorageObject(bool tryLoadStorage = true)
    {
        if (_builder is null && tryLoadStorage)
        {
            FileAssetManager.Current.EnsureStorage(this);
        }

        return _builder?.Owner;
    }

    /// <summary>
    /// Indicates whether the ID is documented
    /// </summary>
    public override bool IsIdDocumented => _ex.IsIdDocumented;

    #endregion

    #region Builders

    /// <summary>
    /// Asset entry
    /// </summary>
    protected internal AssetKeyEntry AssetEntry => _ex.AssetEntry;

    /// <summary>
    /// Multiple full type names
    /// </summary>
    protected internal INamedMultipleItem<Asset> MultipleFullTypeNames => _ex.MultipleResourceNames;

    /// <summary>
    /// Indicates whether there is an asset key conflict
    /// </summary>
    public bool AssetKeyConflict => _ex.AssetEntry?.AssetKeyConflict ?? false;

    /// <summary>
    /// Asset type names
    /// </summary>
    public IEnumerable<string> AssetTypeNames => _ex.AssetTypeNames;

    /// <summary>
    /// Asset activator
    /// </summary>
    public AssetActivator Activator { get => _activator; internal set => _activator = value; }

    /// <summary>
    /// Asset builder
    /// </summary>
    protected internal AssetBuilder Builder { get => _builder; internal set => _builder = value; }

    public AssetAccessMode AccessMode
    {
        get => _accessMode;
        internal protected set
        {
            if (_accessMode == value)
            {
                return;
            }

            _accessMode = value;

            NotifyPropertyUpdated();
        }
    }

    public AssetInstanceMode InstanceMode
    {
        get => _instanceMode;
        internal protected set
        {
            if (_instanceMode == value)
            {
                return;
            }

            _instanceMode = value;

            NotifyPropertyUpdated();
        }
    }

    /// <summary>
    /// Updates asset types with specified type names
    /// </summary>
    protected void UpdateAssetTypes(params string[] assetTypes) => _ex.UpdateAssetTypes(assetTypes);

    /// <summary>
    /// Updates asset types with specified type names
    /// </summary>
    protected void UpdateAssetTypes(IEnumerable<string> assetTypes) => _ex.UpdateAssetTypes(assetTypes);

    /// <summary>
    /// Updates asset types with specified types
    /// </summary>
    protected void UpdateAssetTypes(params Type[] types) => _ex.UpdateAssetTypes(types);

    /// <summary>
    /// Updates asset types with specified types
    /// </summary>
    protected void UpdateAssetTypes(IEnumerable<Type> types) => _ex.UpdateAssetTypes(types);

    #endregion

    #region Features

    /// <summary>
    /// Description
    /// </summary>
    public string Description
    {
        get => _description;
        internal protected set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                value = null;
            }

            if (_description == value)
            {
                return;
            }

            _description = value;

            NotifyPropertyUpdated();
        }
    }

    /// <summary>
    /// Preview text
    /// </summary>
    public string PreviewText
    {
        get => _previewText;
        internal protected set
        {
            if (_previewText == value)
            {
                return;
            }

            _previewText = value;

            NotifyPropertyUpdated();
        }
    }

    /// <summary>
    /// Icon ID
    /// </summary>
    public Guid IconId
    {
        get => _iconId;
        internal protected set
        {
            if (_iconId == value)
            {
                return;
            }

            _iconId = value;

            NotifyPropertyUpdated();
        }
    }

    /// <summary>
    /// Icon
    /// </summary>
    public ImageDef Icon
    {
        get
        {
            try
            {
                return GetIcon()?.ToIconSmall() ?? DefaultIcon?.ToIconSmall();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Indicates whether the asset is active
    /// </summary>
    public bool IsActive => Entry != null && !string.IsNullOrEmpty(_ex.AssetKey);

    /// <summary>
    /// Analysis problems
    /// </summary>
    public AnalysisProblem Problems
    {
        get => _problems;
        internal protected set
        {
            if (_problems != value)
            {
                _problems = value;
                // Set up problems without notification
            }
        }
    }

    /// <summary>
    /// Indicates whether this is a legacy asset
    /// </summary>
    public bool IsLegacy
    {
        get => _isLegacy;
        internal protected set
        {
            if (_isLegacy != value)
            {
                _isLegacy = value;

                NotifyPropertyUpdated();
            }
        }
    }

    /// <summary>
    /// Indicates whether this is a primary asset
    /// </summary>
    public bool IsPrimary
    {
        get => _isPrimary;
        internal protected set
        {
            if (_isPrimary != value)
            {
                _isPrimary = value;

                OnIsPrimaryUpdated();
                NotifyPropertyUpdated();
            }
        }
    }

    /// <summary>
    /// Gets the instance filter
    /// </summary>
    public IAssetFilter GetInstanceFilter(bool instance) => _ex.GetInstanceFilter(instance);

    /// <summary>
    /// Icon key
    /// </summary>
    protected internal string IconKey
    {
        set
        {
            Guid id = GlobalIdResolver.Resolve(value);

            if (_iconId != id)
            {
                _iconId = id;

                NotifyPropertyUpdated(nameof(IconId));
            }
        }
    }

    /// <summary>
    /// Gets the icon for this asset
    /// </summary>
    public virtual ImageDef GetIcon() => EditorUtility.GetIconById(_iconId);

    /// <summary>
    /// Default icon for this asset type
    /// </summary>
    public virtual ImageDef DefaultIcon => EditorUtility.ToDisplayIcon(this.GetType());

    /// <summary>
    /// Name displayed in tree view
    /// </summary>
    public virtual string NameInTreeView => Name;

    #endregion

    #region Override & Virtual

    internal override void InternalRaiseObjectUpdated(EntryEventArgs args)
    {
        base.InternalRaiseObjectUpdated(args);

        AssetManager.Instance.NotifyAssetUpdated(this, args);
    }

    internal override void InternalOnEntryAttached(Guid id)
    {
        base.InternalOnEntryAttached(id);

        if (!string.IsNullOrEmpty(_ex.AssetKey))
        {
            _ex.UpdateAssetEntry();
            _ex.UpdateAssetTypeEntry();
            _ex.UpdateResourceName();

            InternalOnAssetActivate(_ex.AssetKey);
        }
    }

    internal override void InternalOnEntryDetached(Guid id)
    {
        _ex.RemoveAssetEntry();
        _ex.RemoveAssetTypeEntry();
        _ex.RemoveResourceNameEntry();

        if (!string.IsNullOrEmpty(_ex.AssetKey))
        {
            InternalOnAssetDeactivate(_ex.AssetKey);
        }

        base.InternalOnEntryDetached(id);
    }

    /// <summary>
    /// Called when asset is activated internally
    /// </summary>
    internal virtual void InternalOnAssetActivate(string assetKey)
    {
        GlobalIdResolver.Record(assetKey, Id);

        try 
        {
            OnAssetActivate(assetKey);
        }
        catch (Exception err)
        {
            err.LogError();
        }
    }

    /// <summary>
    /// Called when asset is deactivated internally
    /// </summary>
    internal virtual void InternalOnAssetDeactivate(string assetKey)
    {
        try 
        {
            OnAssetDeactivate(assetKey);
        }
        catch (Exception err)
        {
            err.LogError(); 
        }
    }

    /// <summary>
    /// Reactivates the asset
    /// </summary>
    internal virtual void ReactivateAsset()
    {
        string assetKey = this.AssetKey;
        if (!string.IsNullOrEmpty(assetKey))
        {
            try 
            {
                OnAssetActivate(assetKey);
            }
            catch (Exception err) 
            {
                err.LogError();
            }
        }
    }

    /// <summary>
    /// Gets the recorded ID
    /// </summary>
    internal override Guid OnGetRecordedId() => _builder?.RecordedId ?? Guid.Empty;

    /// <summary>
    /// Called when asset is activated
    /// </summary>
    protected virtual void OnAssetActivate(string assetKey)
    { }

    /// <summary>
    /// Called when asset is deactivated
    /// </summary>
    protected virtual void OnAssetDeactivate(string assetKey)
    { }

    /// <summary>
    /// Called on startup
    /// </summary>
    protected internal virtual void OnStartup()
    { }

    /// <summary>
    /// Called when IsPrimary is updated
    /// </summary>
    protected virtual void OnIsPrimaryUpdated()
    { }

    /// <summary>
    /// Called when file name is updated
    /// </summary>
    protected internal virtual void OnFileNameUpdated()
    { }

    /// <summary>
    /// Called when parent is changed
    /// </summary>
    protected internal virtual void OnParentChanged()
    { }

    #endregion

    #region Resource Name Updating

    /// <summary>
    /// Resolves the resource name
    /// </summary>
    protected internal virtual string ResolveResourceName() => _ex.ResolveDefaultResourceName();

    /// <summary>
    /// Called when resource name is updated
    /// </summary>
    protected internal virtual void OnResourceNameUpdated()
    { }

    #endregion

    #region ITextDisplay

    /// <summary>
    /// Display text
    /// </summary>
    public override string DisplayText
    {
        get
        {
            if (EditorUtility.ShowAsDescription.Value && !string.IsNullOrEmpty(_description))
            {
                return _description;
            }

            string shortName = ShortTypeName;
            if (!string.IsNullOrEmpty(shortName))
            {
                return shortName;
            }

            return _ex.LocalName;
        }
    }

    object ITextDisplay.DisplayIcon => this.Icon;

    /// <summary>
    /// Display status
    /// </summary>
    public virtual TextStatus DisplayStatus => TextStatus.Normal;

    #endregion

    #region ISelectionItem

    string ISelectionItem.SelectionKey => _ex.AssetKey;

    #endregion

    #region IRenderModel

    /// <summary>
    /// Render type
    /// </summary>
    public virtual RenderType RenderType => null;

    /// <summary>
    /// Gets the property
    /// </summary>
    public virtual object GetProperty(CodeRenderProperty property, object argument)
    {
        switch (property.PropertyName)
        {
            case CodeRenderProperty.Id:
                return Id;

            case CodeRenderProperty.Name:
                return ShortTypeName;

            case CodeRenderProperty.FullName:
                return FullName;

            case CodeRenderProperty.FullTypeName:
                return FullTypeName;

            case CodeRenderProperty.PathName:
                return AssetKey;

            case CodeRenderProperty.Description:
                return Description;

            case CodeRenderProperty.Parent:
                return ParentAsset;

            case CodeRenderProperty.NameSpace:
                return NameSpace;

            case CodeRenderProperty.ImportedId:
                return ImportedId;

            default:
                return null;
        }
    }

    #endregion

    #region Storage

    /// <summary>
    /// File name of the storage
    /// </summary>
    public StorageLocation FileName
    {
        get => _ex.FileName;
        internal set => _ex.FileName = value;
    }

    /// <summary>
    /// Gets the storage stream
    /// </summary>
    protected IStorageItem GetStream() => _ex.FileName?.GetStorageItem();

    /// <summary>
    /// Marks the file reference as dirty
    /// </summary>
    protected void MarkFileReferenceDirty()
    {
        if (!string.IsNullOrEmpty(FileName.PhysicFileName))
        {
            var refHost = EditorUtility.EnsureReferenceHost(FileName.PhysicFileName);
            refHost?.MarkDirty();
        }
    }

    /// <summary>
    /// Indicates whether to show storage property
    /// </summary>
    public bool ShowStorageProperty { get; internal set; }

    #endregion

    #region Meta

    /// <summary>
    /// Meta data information
    /// </summary>
    internal MetaDataInfo MetaInfo => _ex.MetaInfo;

    /// <summary>
    /// Checks and loads meta file if needed
    /// </summary>
    protected internal void CheckLoadMetaFile() => _ex.CheckLoadMetaFile();

    /// <summary>
    /// Loads meta file
    /// </summary>
    protected internal void LoadMetaFile() => _ex.LoadMetaFile();

    /// <summary>
    /// Loads meta file with specified file name
    /// </summary>
    internal void LoadMetaFile(string fileName) => _ex.LoadMetaFile(fileName);

    /// <summary>
    /// Saves meta file with metadata
    /// </summary>
    protected internal void SaveMetaFile(ISyncObject metadata) => _ex.SaveMetaFile(metadata);

    /// <summary>
    /// Saves meta file
    /// </summary>
    protected internal void SaveMetaFile() => _ex.SaveMetaFile();

    /// <summary>
    /// Saves meta file with delay
    /// </summary>
    protected internal void SaveMetaFileDelayed() => _ex.SaveMetaFileDelayed();

    /// <summary>
    /// Removes meta file
    /// </summary>
    internal void RemoveMetaFile() => _ex.RemoveMetaFile();

    /// <summary>
    /// Called when metadata is updated
    /// </summary>
    protected internal virtual void OnMetaDataUpdated()
    { }

    /// <summary>
    /// Metadata
    /// </summary>
    protected internal ISyncObject MetaData
    {
        get => _ex.MetaData;
        set => _ex.MetaData = value;
    }

    /// <summary>
    /// Gets the metadata of specified type
    /// </summary>
    protected internal T GetMetaData<T>() where T : class, ISyncObject, new() => _ex.GetMetaData<T>();

    /// <summary>
    /// Full name of the package
    /// </summary>
    public string PackageFullName => _ex.PackageFullName;

    #endregion

    #region IViewObject

    void ISyncObject.Sync(IPropertySync sync, ISyncContext context)
    {
        try
        {
            OnSync(sync, context);
        }
        catch (Exception err)
        {
            Logs.LogError(err);
        }

        sync.Sync(nameof(Id), Id, SyncFlag.GetOnly);
        sync.Sync(nameof(AssetKey), AssetKey, SyncFlag.GetOnly);
        sync.Sync(nameof(NativeTypeName), NativeTypeName, SyncFlag.GetOnly);

        sync.Sync(nameof(Description), Description, SyncFlag.GetOnly);
        sync.Sync(nameof(NameSpace), NameSpace, SyncFlag.GetOnly);
        sync.Sync(nameof(FullTypeName), FullTypeName, SyncFlag.GetOnly);
        sync.Sync(nameof(ImportedId), ImportedId, SyncFlag.GetOnly);

        sync.Sync(nameof(AccessMode), AccessMode, SyncFlag.GetOnly);
        sync.Sync(nameof(InstanceMode), InstanceMode, SyncFlag.GetOnly);
        sync.Sync(nameof(PackageFullName), PackageFullName, SyncFlag.GetOnly);
    }

    void IViewObject.SetupView(IViewObjectSetup setup)
    {
        try
        {
            OnSetupView(setup);
        }
        catch (Exception err)
        {
            Logs.LogError(err);
        }

        setup.LabelWithIcon("Asset", CoreIcon.Asset);
        setup.InspectorField(Id, new ViewProperty(nameof(Id)) { ReadOnly = true });
        setup.InspectorField(AssetKey, new ViewProperty(nameof(AssetKey), "Asset key") { ReadOnly = true }.WithToolTips("Temporary name composed of file path, used for asset package import/export."));
        setup.InspectorField(NativeTypeName, new ViewProperty(nameof(NativeTypeName), "Asset type") { ReadOnly = true });
        setup.InspectorField(Description, new ViewProperty(nameof(Description), "Description") { ReadOnly = true });

        setup.LabelWithIcon("Naming", CoreIcon.Label);
        setup.InspectorField(FullTypeName, new ViewProperty(nameof(FullTypeName), "Full asset type name") { ReadOnly = true }.WithToolTips("Final resource name when this asset is exported for external use."));
        setup.InspectorField(NameSpace, new ViewProperty(nameof(NameSpace), "Name space") { ReadOnly = true });
        setup.InspectorField(ImportedId, new ViewProperty(nameof(ImportedId), "Imported Id") { ReadOnly = true });

        setup.LabelWithIcon("Features", CoreIcon.Attribute);
        setup.InspectorField(AccessMode, new ViewProperty(nameof(AccessMode), "Access mode") { ReadOnly = true });
        setup.InspectorField(InstanceMode, new ViewProperty(nameof(InstanceMode), "Instance mode") { ReadOnly = true });
        setup.InspectorField(PackageFullName, new ViewProperty(nameof(PackageFullName), "Pckage name") { ReadOnly = true });
    }

    /// <summary>
    /// Called on sync
    /// </summary>
    protected virtual void OnSync(IPropertySync sync, ISyncContext context)
    { }

    /// <summary>
    /// Called on setup view
    /// </summary>
    protected virtual void OnSetupView(IViewObjectSetup setup)
    { }

    #endregion

    #region IViewColor

    /// <summary>
    /// Specific color for each instance
    /// </summary>
    public virtual Color? ViewColor
    {
        get => _color;
        internal protected set
        {
            if (_color == Color.Empty)
            {
                _color = null;
            }

            if (_color == value)
            {
                return;
            }

            _color = value;

            NotifyPropertyUpdated();
        }
    }

    #endregion

    #region Attach

    /// <summary>
    /// Get all attached assets
    /// </summary>
    /// <returns></returns>
    public Asset[] GetAttachedAssets()
    {
        var key = this.AssetKey;

        List<Asset> attachedAssets = null;
        foreach (var ext in AssetManager.Instance.AttachedAssetExtensions)
        {
            string attachedKey = key + ext;
            var asset = AssetManager.Instance.GetAsset(attachedKey);
            if (asset != null)
            {
                (attachedAssets ??= []).Add(asset);
            }
        }

        return attachedAssets?.ToArray() ?? [];
    }

    #endregion

    /// <summary>
    /// Returns a string that represents this asset
    /// </summary>
    public override string ToString()
    {
        return DisplayText;
    }

    /// <summary>
    /// Converts to data ID
    /// </summary>
    public virtual string ToDataId(bool simplified = false)
    {
        string parentName = ParentAsset?.ImportedId;
        if (string.IsNullOrWhiteSpace(parentName))
        {
            parentName = ParentAsset?.NameSpace?.TrimStart('*');
        }

        if (!string.IsNullOrWhiteSpace(parentName))
        {
            return $"{parentName}.{LocalName}";
        }
        else
        {
            return FullTypeName ?? LocalName;
        }
    }
}

/// <summary>
/// Standalone asset base class
/// </summary>
public abstract class StandaloneAsset : Asset
{
    /// <summary>
    /// Initializes a new instance of StandaloneAsset
    /// </summary>
    public StandaloneAsset(Type[] types, bool resolveId = true)
    {
        _ex.LocalName = $"*{this.GetType().FullName}";

        VerifyTypes(types);

        if (types?.Length > 0)
        {
            UpdateAssetTypes(types);
        }

        if (resolveId)
        {
            ResolveId();
        }
    }

    /// <summary>
    /// Initializes a new instance of StandaloneAsset with name
    /// </summary>
    public StandaloneAsset(Type[] types, string name, bool resolveId = true)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        _ex.LocalName = name;

        VerifyTypes(types);

        if (types?.Length > 0)
        {
            UpdateAssetTypes(types);
        }

        if (resolveId)
        {
            ResolveId();
        }
    }

    /// <summary>
    /// Default icon for this asset type
    /// </summary>
    public override ImageDef DefaultIcon => this.GetType().ToDisplayIcon() ?? CoreIconCache.Assistant;

    /// <summary>
    /// Display text for this asset
    /// </summary>
    public override string DisplayText => this.GetType().ToDisplayText() ?? this.GetType().Name;

    private void VerifyTypes(Type[] types)
    {
        if (types is null || types.Length == 0)
        {
            return;
        }

        foreach (var type in types)
        {
            if (type is null)
            {
                throw new ArgumentException("types contains null");
            }
            if (!type.IsAssignableFrom(this.GetType()))
            {
                throw new ArgumentException("types contains non-assignable type: " + type.FullName);
            }
        }
    }
}

/// <summary>
/// Standalone asset base class
/// </summary>
public abstract class StandaloneAsset<T> : StandaloneAsset
{
    /// <summary>
    /// Initializes a new instance of StandaloneAsset
    /// </summary>
    public StandaloneAsset(bool resolveId = true)
        : base([typeof(T)], resolveId)
    {
    }

    /// <summary>
    /// Initializes a new instance of StandaloneAsset with name
    /// </summary>
    public StandaloneAsset(string name, bool active = true)
        : base([typeof(T)], name, active)
    {
    }
}