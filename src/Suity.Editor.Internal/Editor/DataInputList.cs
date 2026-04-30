using Suity.Editor;
using Suity.Editor.CodeRender;
using Suity.Editor.Documents;
using Suity.Editor.Documents.Linked;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Editor;

/// <summary>
/// Manages a collection of <see cref="DataInput"/> items with synchronization
/// and view list support.
/// </summary>
public class DataInputList : IDataInputList, IViewList, ITextDisplay, IDataInputOwner, IHasObjectCreationGUI
{
    /// <summary>
    /// Gets the parent sync path object.
    /// </summary>
    internal readonly ISyncPathObject _parent;

    /// <summary>
    /// Gets the property name used for synchronization.
    /// </summary>
    internal readonly string _propertyName;

    private readonly List<DataInput> _dataInputs = [];

    /// <summary>
    /// Occurs when a data input is added to the list.
    /// </summary>
    public event Action<IDataInputItem> DataInputAdded;

    /// <summary>
    /// Occurs when a data input is removed from the list.
    /// </summary>
    public event Action<IDataInputItem> DataInputRemoved;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataInputList"/> class.
    /// </summary>
    /// <param name="parent">The parent sync path object.</param>
    /// <param name="propertyName">The property name for synchronization.</param>
    public DataInputList(ISyncPathObject parent, string propertyName)
    {
        _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        _propertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
    }

    /// <summary>
    /// Gets the index of the specified data input in the list.
    /// </summary>
    /// <param name="data">The data input to find.</param>
    /// <returns>The zero-based index, or -1 if not found.</returns>
    public int IndexOf(DataInput data)
    {
        return _dataInputs.IndexOf(data);
    }

    /// <summary>
    /// Clears all data inputs from the list.
    /// </summary>
    public void Clear()
    {
        _dataInputs.Clear();
    }

    private void OnDataInputAdded(DataInput data)
    {
        data.UpdateList(this);
        DataInputAdded?.Invoke(data);
    }

    private void OnDataInputRemoved(DataInput data)
    {
        if (data.ParentList == this)
        {
            data.UpdateList(null);
        }
        DataInputRemoved?.Invoke(data);
    }

    #region IViewList

    /// <inheritdoc/>
    int IViewList.ListViewId => ViewIds.TreeView;
    /// <summary>
    /// Gets the number of data inputs in the list.
    /// </summary>
    public int Count => _dataInputs.Count;

    /// <inheritdoc/>
    void ISyncList.Sync(IIndexSync sync, ISyncContext context)
    {
        sync.SyncGenericIList(
            _dataInputs,
            createNew: () => null,
            added: (item, index) =>
            {
                OnDataInputAdded(item);
                // Update reference relationships
                context.DoServiceAction<IListItemNotify<DataInput>>(notify => notify.NotifyItemAdded(item));
            },
            removed: item =>
            {
                OnDataInputRemoved(item);
                // Update reference relationships
                context.DoServiceAction<IListItemNotify<DataInput>>(notify => notify.NotifyItemRemoved(item));
            }
        );
    }

    /// <inheritdoc/>
    bool IDropInCheck.DropInCheck(object value)
    {
        Guid id = ResolveAssetId(value);

        if (id == Guid.Empty)
        {
            return false;
        }

        if (ContainsDataInput(id))
        {
            return false;
        }

        var asset = AssetManager.Instance.GetAsset(id);
        if (!(asset is IRenderable))
        {
            return false;
        }

        return true;
    }

    /// <inheritdoc/>
    object IDropInCheck.DropInConvert(object value)
    {
        if (value is DataInput input)
        {
            if (input.ParentList == this)
            {
                return value;
            }
            else
            {
                return new DataInput(input);
            }
        }

        if (value is IDataInput dataInput)
        {
            return new DataInput(dataInput);
        }

        Guid id = ResolveAssetId(value);
        if (id != Guid.Empty)
        {
            return new DataInput(id);
        }

        return null;
    }

    private Guid ResolveAssetId(object value) => value switch
    {
        Guid id => id,
        string assetKey => AssetManager.Instance.GetAsset(assetKey)?.Id ?? Guid.Empty,
        DataInput dataInput => dataInput.RenderableId,
        IDataInput iDataInput => iDataInput.RenderableId,
        Asset asset => asset.Id,
        IHasAsset assetContext => assetContext.TargetAsset?.Id ?? Guid.Empty,
        Document document => document.GetAsset()?.Id ?? Guid.Empty,
        _ => Guid.Empty,
    };

    #endregion

    #region ITextDisplay

    /// <inheritdoc/>
    string ITextDisplay.DisplayText => "Data Input";

    /// <inheritdoc/>
    object ITextDisplay.DisplayIcon => CoreIconCache.Data;

    /// <inheritdoc/>
    TextStatus ITextDisplay.DisplayStatus => TextStatus.Normal;

    #endregion

    #region IDataInputOwner

    /// <summary>
    /// Gets the non-commented data inputs in this list.
    /// </summary>
    public IEnumerable<IDataInput> GetDataInputs() => _dataInputs.Where(o => !o.IsComment);

    /// <summary>
    /// Determines whether the list contains a data input with the specified renderable ID.
    /// </summary>
    /// <param name="id">The renderable ID to search for.</param>
    /// <returns>True if a matching non-commented data input exists; otherwise, false.</returns>
    public bool ContainsDataInput(Guid id)
    {
        return _dataInputs.Any(input => input.RenderableId == id && !input.IsComment);
    }

    #endregion

    #region IHasObjectCreationGUI

    /// <summary>
    /// Gets the available object creation options (null if not supported).
    /// </summary>
    public IEnumerable<ObjectCreationOption> CreationOptions => null;

    /// <summary>
    /// Shows a GUI dialog to create a new data input object asynchronously.
    /// </summary>
    /// <param name="typeHint">Optional type hint for the object to create.</param>
    /// <returns>A new <see cref="DataInput"/> instance, or null if cancelled or duplicate.</returns>
    public async Task<object> GuiCreateObjectAsync(Type typeHint = null)
    {
        var result = await DialogUtility.ShowAssetSelectionGUIAsync<IRenderable>("Select Data Input");

        if (result != null && result is Asset content)
        {
            if (_dataInputs.Any(o => o.RenderableId == content.Id))
            {
                await DialogUtility.ShowMessageBoxAsync("This data already exists");
                return null;
            }

            return new DataInput(content.Id);
        }
        else
        {
            return null;
        }
    }

    #endregion
}