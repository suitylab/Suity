using Suity.Collections;
using Suity.Drawing;
using Suity.Selecting;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Editor.Selecting;

/// <summary>
/// Represents an empty value placeholder.
/// </summary>
public sealed class EmptyValue
{
}

/// <summary>
/// A tree list for selecting assets with associated values.
/// </summary>
/// <typeparam name="TAsset">The type of asset.</typeparam>
/// <typeparam name="TValue">The type of value.</typeparam>
public class AssetSelectionTreeList<TAsset, TValue> : IViewList, IHasObjectCreationGUI, ITextDisplay
    where TAsset : class
    where TValue : class, new()
{
    private readonly List<AssetSelectionTreeItem<TAsset, TValue>> _list = [];

    /// <summary>
    /// Initializes a new instance of the AssetSelectionTreeList class.
    /// </summary>
    public AssetSelectionTreeList()
    {
    }

    /// <summary>
    /// Gets or sets the selection list provider.
    /// </summary>
    public ISelectionListProvider Provider { get; set; }

    /// <summary>
    /// Gets or sets the asset filter.
    /// </summary>
    public IAssetFilter Filter { get; set; }

    /// <summary>
    /// Gets or sets the display icon.
    /// </summary>
    public ImageDef Icon { get; set; }

    /// <summary>
    /// Event raised when an item is added.
    /// </summary>
    public event EventArgsHandler<AssetSelectionTreeItem<TAsset, TValue>> Added;

    /// <summary>
    /// Event raised when an item is removed.
    /// </summary>
    public event EventArgsHandler<AssetSelectionTreeItem<TAsset, TValue>> Removed;

    /// <summary>
    /// Determines whether the list contains an item with the specified key.
    /// </summary>
    /// <param name="key">The asset key.</param>
    /// <returns>True if the key exists; otherwise, false.</returns>
    public bool Contains(string key)
    {
        return _list.Any(type => type._assetRef.AssetKey == key);
    }

    /// <summary>
    /// Determines whether the list contains an item with the specified id.
    /// </summary>
    /// <param name="id">The asset id.</param>
    /// <returns>True if the id exists; otherwise, false.</returns>
    public bool Contains(Guid id)
    {
        return _list.Any(type => type._assetRef.Id == id);
    }

    /// <summary>
    /// Clears all items from the list.
    /// </summary>
    public void Clear()
    {
        _list.Clear();
    }

    /// <summary>
    /// Raises the Added event.
    /// </summary>
    /// <param name="item">The added item.</param>
    protected virtual void OnItemAdded(AssetSelectionTreeItem<TAsset, TValue> item)
    {
        Added?.Invoke(this, new EventArgs<AssetSelectionTreeItem<TAsset, TValue>>(item));
    }

    /// <summary>
    /// Raises the Removed event.
    /// </summary>
    /// <param name="item">The removed item.</param>
    protected virtual void OnItemRemoved(AssetSelectionTreeItem<TAsset, TValue> item)
    {
        Removed?.Invoke(this, new EventArgs<AssetSelectionTreeItem<TAsset, TValue>>(item));
    }

    /// <summary>
    /// Gets all items in the list.
    /// </summary>
    public IEnumerable<AssetSelectionTreeItem<TAsset, TValue>> Items => _list.Pass();

    /// <summary>
    /// Gets all selected assets.
    /// </summary>
    public IEnumerable<TAsset> Selections => _list.Select(o => o.SelectedContent);

    /// <summary>
    /// Gets all keys in the list.
    /// </summary>
    public IEnumerable<string> Keys => _list.Select(o => o.Key);

    #region IVisionTreeList

    /// <inheritdoc />
    int IViewList.ListViewId => ViewIds.TreeView;

    /// <summary>
    /// Gets the number of items in the list.
    /// </summary>
    public int Count => _list.Count;

    /// <inheritdoc />
    void ISyncList.Sync(IIndexSync sync, ISyncContext context)
    {
        sync.SyncGenericIList(
            _list,
            typeof(AssetSelectionTreeItem<TAsset, TValue>),
            item => true,
            () => new(),
            (item, index) =>
            {
                OnItemAdded(item);
                //Update reference relationship
                //context.DoServiceAction<DataDocument>(doc => doc.InternalUpdateAssetMainContent());
            },
            item =>
            {
                OnItemRemoved(item);
                //Update reference relationship
                //context.DoServiceAction<DataDocument>(doc => doc.InternalUpdateAssetMainContent());
            }
        );
    }

    /// <inheritdoc />
    /// <inheritdoc />
    bool IDropInCheck.DropInCheck(object value)
    {
        if (value is not AssetSelectionTreeItem<TAsset, TValue> item)
        {
            return false;
        }

        if (Contains(item.Id))
        {
            return false;
        }

        return true;
    }

    /// <inheritdoc />
    object IDropInCheck.DropInConvert(object value)
    {
        return value;
    }


    #endregion

    #region IGUICreateObjectAsync

    /// <inheritdoc />
    public IEnumerable<ObjectCreationOption> CreationOptions => null;

    /// <inheritdoc />
    public async Task<object> GuiCreateObjectAsync(Type typeHint = null)
    {
        ISelectionList selList;

        var filter = Filter ?? AssetFilters.Pending;
        if (Provider != null)
        {
            selList = Provider.GetSelectionList<TAsset>(filter);
        }
        else
        {
            selList = AssetManager.Instance.GetAssetCollection<TAsset>().WithFilter(filter);
        }

        selList ??= EmptySelectionList.Empty;

        var result = await selList.ShowSelectionGUIAsync(null);

        if (result?.IsSuccess == true)
        {
            if (!Contains(result.SelectedKey))
            {
                return new AssetSelectionTreeItem<TAsset, TValue>(result.SelectedKey);
            }
            else
            {
                await DialogUtility.ShowMessageBoxAsync("Selection item already exists");
                return null;
            }
        }
        else
        {
            return null;
        }
    }

    #endregion

    #region ITextDisplay

    /// <inheritdoc />
    string ITextDisplay.DisplayText => null;

    /// <inheritdoc />
    object ITextDisplay.DisplayIcon => this.Icon;

    /// <inheritdoc />
    TextStatus ITextDisplay.DisplayStatus => TextStatus.Normal;

    #endregion
}

/// <summary>
/// A tree list for selecting assets without associated values.
/// </summary>
/// <typeparam name="TAsset">The type of asset.</typeparam>
public class AssetSelectionTreeList<TAsset> : AssetSelectionTreeList<TAsset, EmptyValue>
    where TAsset : class
{
}