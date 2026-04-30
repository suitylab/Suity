using Suity.Editor;
using Suity.Editor.Design;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Services;
using Suity.NodeQuery;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Views.Named;

/// <summary>
/// Represents a using list item that wraps an editor object reference, providing navigation, display, and find-reference capabilities.
/// </summary>
internal class UsingListObjectItem :
    INamedUsingListItem,
    INavigable,
    IFindReferenceScope,
    ITextDisplay,
    IViewDoubleClickAction,
    IViewObject,
    IInspectorRoute,
    IViewLocateInProject
{
    private readonly EditorObjectRef<EditorObject> _objRef = new();
    private readonly object _owner;

    /// <summary>
    /// Gets the target editor object.
    /// </summary>
    public EditorObject Target => _objRef.Target;
    /// <summary>
    /// Gets the short display name of the target object.
    /// </summary>
    public string ShortName => _objRef.Target?.Name ?? _objRef.Id.ToString();

    /// <summary>
    /// Initializes a new instance of the <see cref="UsingListObjectItem"/> class.
    /// </summary>
    /// <param name="id">The GUID identifier of the editor object.</param>
    /// <param name="owner">Optional owner object for reference scope filtering.</param>
    public UsingListObjectItem(Guid id, object owner = null)
    {
        _objRef.Id = id;
        _owner = owner;
    }

    /// <inheritdoc/>
    public object GetNavigationTarget()
    {
        return _objRef.Id;
    }

    /// <inheritdoc/>
    public string DisplayText => ShortName;
    /// <inheritdoc/>
    public object DisplayIcon => _objRef.Target?.ToDisplayIcon();

    /// <summary>
    /// Gets the asset target object for this reference.
    /// </summary>
    /// <param name="open">Whether to open the document if needed.</param>
    /// <returns>The target view object, or null if not available.</returns>
    public object GetAssetTargetObject(bool open)
    {
        if (_objRef.Target is not Asset asset)
        {
            return null;
        }

        var doc = asset.GetDocumentEntry(open)?.Content;
        if (doc is null)
        {
            return null;
        }

        if (asset.FileName != null)
        {
            if (doc is SNamedDocument sdoc)
            {
                return sdoc;
            }
            else if (doc is IViewObject vDoc)
            {
                return vDoc;
            }
        }
        else if ((doc as IMemberContainer)?.GetMember(asset.LocalName) is IViewObject vObj)
        {
            return vObj;
        }

        return null;
    }

    /// <inheritdoc/>
    public TextStatus DisplayStatus
    {
        get
        {
            if (_objRef.Target != null)
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
        EditorUtility.GotoDefinition(_objRef.Id);
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

    #region IInspectorRoute
    /// <inheritdoc/>
    object IViewRedirect.GetRedirectedObject(int viewId) => GetAssetTargetObject(true) ?? this;

    /// <inheritdoc/>
    InspectorTreeModes? IInspectorRoute.GetRoutedTreeMode() => null;

    /// <inheritdoc/>
    bool IInspectorRoute.GetRoutedReadonly() => true;

    /// <inheritdoc/>
    INodeReader IInspectorRoute.GetRoutedStyles() => null;

    #endregion
}

/// <summary>
/// Provides a list view for named object references (GUIDs), enabling synchronization and display of editor object usages.
/// </summary>
internal class NamedObjectUsingList : INamedUsingList
{
    private readonly string _fieldDescription;
    private readonly List<UsingListObjectItem> _objRefs = [];
    private readonly object _owner;

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedObjectUsingList"/> class.
    /// </summary>
    /// <param name="fieldDescription">Description of the field for display purposes.</param>
    /// <param name="ids">Collection of GUIDs representing editor objects.</param>
    /// <param name="owner">Optional owner object for reference scope filtering.</param>
    public NamedObjectUsingList(string fieldDescription, IEnumerable<Guid> ids, object owner = null)
    {
        _fieldDescription = fieldDescription ?? string.Empty;
        _owner = owner;

        _objRefs.AddRange(ids
            .Where(id => id.GetStorageLocation() != null) // Resource must exist in file
            .Select(id => new UsingListObjectItem(id))
            .ToArray());

        _objRefs.Sort((a, b) =>
        {
            string extA = a.Target?.FullName ?? string.Empty;
            string extB = b.Target?.FullName ?? string.Empty;

            return extA.CompareTo(extA);
        });
    }

    /// <inheritdoc/>
    int IViewList.ListViewId => ViewIds.TreeView;
    /// <inheritdoc/>
    int ISyncList.Count => _objRefs.Count;

    /// <inheritdoc/>
    void ISyncList.Sync(IIndexSync sync, ISyncContext context)
    {
        switch (sync.Mode)
        {
            case SyncMode.RequestElementType:
                sync.Sync(0, typeof(EditorObjectRef<EditorObject>));
                break;

            case SyncMode.Get:
                if (sync.Index >= 0 && sync.Index < _objRefs.Count)
                {
                    sync.Sync(sync.Index, _objRefs[sync.Index]);
                }
                break;

            case SyncMode.GetAll:
                for (int i = 0; i < _objRefs.Count; i++)
                {
                    sync.Sync(i, _objRefs[i]);
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
