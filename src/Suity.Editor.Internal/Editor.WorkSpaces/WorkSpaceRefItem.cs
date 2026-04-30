using Suity.Editor.CodeRender;
using Suity.Editor.Selecting;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;

namespace Suity.Editor.WorkSpaces;

/// <summary>
/// Base class for workspace reference items that represent various asset types contributing render targets.
/// </summary>
public abstract class WorkSpaceRefItem : IWorkSpaceRefItem, ISyncObject, IViewObject, IComparable<WorkSpaceRefItem>
{
    private bool _enabled = true;
    private bool _suspended = false;

    private bool _userCodeEnabled = true;
    private AssetSelection<ICodeLibrary> _userCode = new();
    private bool _autoRestoreUserCode = false;

    private readonly UserCodeFilter _userCodeFilter = new();
    
    /// <summary>
    /// Initializes a new instance of <see cref="WorkSpaceRefItem"/>.
    /// </summary>
    public WorkSpaceRefItem()
    {
        LastUpdateTime = DateTime.Now;

        // _userCode.Filter = _userCodeFilter;
    }

    /// <summary>
    /// Gets the workspace this reference item belongs to.
    /// </summary>
    public WorkSpace WorkSpace { get; internal set; }

    /// <summary>
    /// Gets or sets whether this reference item is enabled for rendering.
    /// </summary>
    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (_enabled != value)
            {
                _enabled = value;
                Modified?.Invoke(this);
            }
        }
    }

    /// <summary>
    /// Gets or sets whether rendering is suspended for this item.
    /// </summary>
    public bool Suspended
    {
        get => _suspended;
        set
        {
            if (_suspended != value)
            {
                _suspended = value;
                Modified?.Invoke(this);
            }
        }
    }

    /// <summary>
    /// Gets or sets the last update timestamp for this reference item.
    /// </summary>
    public DateTime LastUpdateTime { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for this reference item.
    /// </summary>
    public virtual Guid Id
    {
        get => _userCodeFilter.AffectedId;
        set
        {
            if (_userCodeFilter.AffectedId != value)
            {
                _userCodeFilter.AffectedId = value;
                Modified?.Invoke(this);
            }
        }
    }

    /// <summary>
    /// Gets the display order priority for sorting reference items.
    /// </summary>
    public abstract int Order { get; }

    /// <summary>
    /// Gets whether this item operates in upload mode.
    /// </summary>
    public abstract bool UploadMode { get; }


    /// <summary>
    /// Gets or sets whether user code generation is enabled for this item.
    /// </summary>
    public bool UserCodeEnabled
    {
        get => _userCodeEnabled;
        set
        {
            if (_userCodeEnabled != value)
            {
                _userCodeEnabled = value;
                Modified?.Invoke(this);
            }
        }
    }

    /// <summary>
    /// Gets the resolved user code library, or null if not available.
    /// </summary>
    public ICodeLibrary UserCode => _userCode.Target;

    /// <summary>
    /// Gets or sets the asset ID of the user code library.
    /// </summary>
    public Guid UserCodeId
    {
        get => _userCode.Id;
        set
        {
            if (_userCode.Id != value)
            {
                _userCode.Id = value;
                Modified?.Invoke(this);
            }
        }
    }

    /// <summary>
    /// Gets or sets whether auto-restore of user code is enabled.
    /// </summary>
    public bool AutoRestoreUserCode
    {
        get => _autoRestoreUserCode;
        set
        {
            if (_autoRestoreUserCode != value)
            {
                _autoRestoreUserCode = value;
                Modified?.Invoke(this);
            }
        }
    }

    /// <summary>
    /// Sets up the user code file by resolving the specified file path to an asset.
    /// </summary>
    /// <param name="restoreFileName">The file path to resolve.</param>
    /// <returns>True if the user code file was successfully set up.</returns>
    public bool SetupUserCodeFile(string restoreFileName)
    {
        var asset = EditorUtility.GetFileAsset(restoreFileName);

        if (asset != null)
        {
            _userCode.Id = asset.Id;

            return true;
        }
        else
        {
            string assetKey = EditorUtility.MakeFileAssetKey(restoreFileName);
            _userCode.Id = GlobalIdResolver.Resolve(assetKey);

            return true;
        }
    }

    /// <summary>
    /// Event invoked when this reference item is modified.
    /// </summary>
    internal Action<WorkSpaceRefItem> Modified;

    #region ISyncObject, IInspectorObject

    /// <inheritdoc/>
    void ISyncObject.Sync(IPropertySync sync, ISyncContext context)
    {
        _enabled = sync.Sync("Enabled", _enabled, SyncFlag.NotNull | SyncFlag.AffectsOthers, true);
        _suspended = sync.Sync("Suspended", _suspended, SyncFlag.NotNull | SyncFlag.AffectsOthers, false);

        OnSync(sync, context);

        _userCodeEnabled = sync.Sync("UserCodeEnabled", _userCodeEnabled, SyncFlag.None, true);
        _userCode = sync.Sync("UserCode", _userCode, SyncFlag.NotNull);
        _autoRestoreUserCode = sync.Sync("AutoRestoreUserCode", _autoRestoreUserCode, SyncFlag.None, false);

        if (sync.IsSetter())
        {
            LastUpdateTime = DateTime.Now;
            Modified?.Invoke(this);
        }

        // _userCode.Filter = _userCodeFilter;
    }

    /// <inheritdoc/>
    void IViewObject.SetupView(IViewObjectSetup setup)
    {
        setup.InspectorField(_enabled, new ViewProperty("Enabled", "Enable"));

        if (_enabled)
        {
            setup.InspectorField(_suspended, new ViewProperty("Suspended", "Suspend"));
        }

        OnSetupView(setup);

        setup.InspectorField(_userCodeEnabled, new ViewProperty("UserCodeEnabled", "Enable User Code"));
        setup.InspectorField(_userCode, new ViewProperty("UserCode", "User Code Library").WithEnabeld(_userCodeEnabled));
        setup.InspectorField(_autoRestoreUserCode, new ViewProperty("AutoRestoreUserCode", "Auto Restore User Code").WithEnabeld(_userCodeEnabled));
    }

    /// <summary>
    /// Called during synchronization to allow derived classes to sync their own properties.
    /// </summary>
    /// <param name="sync">The property synchronizer.</param>
    /// <param name="context">The synchronization context.</param>
    protected virtual void OnSync(IPropertySync sync, ISyncContext context)
    {
    }

    /// <summary>
    /// Called during view setup to allow derived classes to configure their own inspector fields.
    /// </summary>
    /// <param name="setup">The view setup helper.</param>
    protected virtual void OnSetupView(IViewObjectSetup setup)
    {
    }

    #endregion

    /// <inheritdoc/>
    public int CompareTo(WorkSpaceRefItem other)
    {
        if (Order > other.Order)
        {
            return 1;
        }
        else if (Order < other.Order)
        {
            return -1;
        }

        string fullName = EditorObjectManager.Instance.GetObject(Id)?.FullName ?? GlobalIdResolver.RevertResolve(Id) ?? string.Empty;
        string otherFullName = EditorObjectManager.Instance.GetObject(other.Id)?.FullName ?? GlobalIdResolver.RevertResolve(other.Id) ?? string.Empty;

        int cmp = string.Compare(fullName, otherFullName);
        if (cmp != 0)
        {
            return cmp;
        }

        return Id.CompareTo(other.Id);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Id.ToDescriptionText();
    }

    /// <summary>
    /// Gets the render targets produced by this reference item.
    /// </summary>
    /// <returns>A collection of render targets.</returns>
    public abstract IEnumerable<RenderTarget> GetRenderTargets();
}