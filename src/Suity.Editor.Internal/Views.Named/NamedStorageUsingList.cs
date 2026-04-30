using Suity.Editor;
using Suity.Editor.Documents;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Suity.Views.Named;

/// <summary>
/// Represents a using list item that wraps a storage location, providing navigation, display, and find-reference capabilities.
/// </summary>
internal class UsingListStorageItem : 
    INavigable, 
    IFindReferenceScope,
    ITextDisplay,
    IViewDoubleClickAction,
    IViewObject
{
    private readonly StorageLocation _location;
    private readonly object _owner;
    private readonly string _shortName;

    /// <summary>
    /// Gets the storage location wrapped by this item.
    /// </summary>
    public StorageLocation Location => _location;
    /// <summary>
    /// Gets the short file name of the storage location.
    /// </summary>
    public string ShortName => _shortName;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsingListStorageItem"/> class.
    /// </summary>
    /// <param name="location">The storage location to wrap. Must not be null.</param>
    /// <param name="owner">Optional owner object for reference scope filtering.</param>
    public UsingListStorageItem(StorageLocation location, object owner = null)
    {
        _location = location ?? throw new ArgumentNullException(nameof(location));
        _shortName = Path.GetFileName(_location.FullPath);
        _owner = owner;
    }

    /// <inheritdoc/>
    public object GetNavigationTarget()
    {
        if (_location.PhysicFileName != null && File.Exists(_location.PhysicFileName))
        {
            var doc = DocumentManager.Instance.OpenDocument(_location);
            if (doc != null)
            {
                return doc;
            }
        }

        Asset asset = _location.GetAsset();
        if (asset != null)
        {
            return asset;
        }

        return _location;
    }

    /// <inheritdoc/>
    public string DisplayText => _shortName;
    /// <inheritdoc/>
    public object DisplayIcon => EditorUtility.GetIconForFileExact(_shortName);

    /// <inheritdoc/>
    public TextStatus DisplayStatus
    {
        get
        {
            if (_location.PhysicFileName != null && File.Exists(_location.PhysicFileName))
            {
                return TextStatus.Reference;
            }
            else if (_location.GetAsset() != null)
            {
                return TextStatus.Reference;
            }
            else
            {
                return TextStatus.Error;
            }
        }
    }

    /// <inheritdoc/>
    void IViewDoubleClickAction.DoubleClick()
    {
        DocumentManager.Instance.ShowDocument(_location.FullPath);
    }

    /// <inheritdoc/>
    void IViewObject.SetupView(IViewObjectSetup setup)
    {
    }

    /// <inheritdoc/>
    void ISyncObject.Sync(IPropertySync sync, ISyncContext context)
    {
    }

    /// <inheritdoc/>
    bool IFindReferenceScope.IncludeChildAssets => true;

    /// <inheritdoc/>
    bool IFindReferenceScope.IsInScope(SyncPathReportItem item)
    {
        if (_owner != null)
        {
            return _owner == item.Owner;
        }
        else
        {
            return true;
        }
    }
}

/// <summary>
/// Provides a list view for named storage locations, enabling synchronization and display of file usages.
/// </summary>
internal class NamedStorageUsingList : INamedUsingList
{
    private readonly string _fieldDescription;
    private readonly List<UsingListStorageItem> _files = [];
    private readonly object _owner;

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedStorageUsingList"/> class.
    /// </summary>
    /// <param name="fieldDescription">Description of the field for display purposes.</param>
    /// <param name="fileNames">Collection of storage locations.</param>
    /// <param name="owner">Optional owner object for reference scope filtering.</param>
    public NamedStorageUsingList(string fieldDescription, IEnumerable<StorageLocation> fileNames, object owner = null)
    {
        _fieldDescription = fieldDescription ?? string.Empty;
        _owner = owner;

        _files.AddRange(fileNames
            .Select(file => new UsingListStorageItem(file, owner))
            .ToArray());

        _files.Sort((a, b) =>
        {
            string extA = Path.GetExtension(a.ShortName);
            string extB = Path.GetExtension(b.ShortName);

            if (extA != extB)
            {
                return extA.CompareTo(extB);
            }

            return a.ShortName.CompareTo(b.ShortName);
        });
    }

    /// <inheritdoc/>
    int IViewList.ListViewId => ViewIds.TreeView;
    /// <inheritdoc/>
    int ISyncList.Count => _files.Count;

    /// <inheritdoc/>
    void ISyncList.Sync(IIndexSync sync, ISyncContext context)
    {
        switch (sync.Mode)
        {
            case SyncMode.RequestElementType:
                sync.Sync(0, typeof(UsingListStorageItem));
                break;

            case SyncMode.Get:
                if (sync.Index >= 0 && sync.Index < _files.Count)
                {
                    sync.Sync(sync.Index, _files[sync.Index]);
                }
                break;

            case SyncMode.GetAll:
                for (int i = 0; i < _files.Count; i++)
                {
                    sync.Sync(i, _files[i]);
                }
                break;
        }
    }

    /// <inheritdoc/>
    bool IDropInCheck.DropInCheck(object value) => false;

    /// <inheritdoc/>
    object IDropInCheck.DropInConvert(object value) => null;

    /// <inheritdoc/>
    public string DisplayText => _fieldDescription;
    /// <inheritdoc/>
    public object DisplayIcon => CoreIconCache.Using;
    /// <inheritdoc/>
    public TextStatus DisplayStatus => TextStatus.Reference;
}
