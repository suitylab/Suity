using Suity.Synchonizing;
using Suity.Synchonizing.Preset;
using Suity.Views;
using System;
using System.Threading.Tasks;

namespace Suity.Editor.VirtualTree.Adapters;

internal class ViewListAdapter : ListAdapter, ISyncContext
{
    public override int Count
    {
        get
        {
            ISyncList list = ResolveList();

            return list?.Count ?? 0;
        }
    }

    public override object GetItem(int index)
    {
        ISyncList list = ResolveList();
        if (list is null)
        {
            return null;
        }

        IIndexSync sync = SingleIndexSync.CreateGetter(index);
        list.Sync(sync, this);

        return sync.Value;
    }

    public override void SetItem(int index, object value)
    {
        ISyncList list = ResolveList();
        if (list is null)
        {
            return;
        }

        IIndexSync sync = SingleIndexSync.CreateSetter(index, value);
        list.Sync(sync, this);
    }

    public override async Task<object> GuiCreateItemAsync(Type typeHint = null)
    {
        var list = ResolveList();

        if (list is not IHasObjectCreationGUI creator)
        {
            await DialogUtility.ShowMessageBoxAsyncL("List has not creation option.");
            return null;
        }

        return await creator.GuiCreateObjectAsync(typeHint);
    }

    public override async Task<object[]> GuiCreateItemsAsync(Type typeHint = null)
    {
        var list = ResolveList();

        if (list is not IHasObjectCreationGUI creator)
        {
            await DialogUtility.ShowMessageBoxAsyncL("List has not creation option.");
            return null;
        }

        var obj = await creator.GuiCreateObjectAsync(typeHint);

        if (obj is object[] ary)
        {
            return ary;
        }
        else
        {
            return [obj];
        }
    }

    public override void Insert(int index, object value)
    {
        ISyncList list = ResolveList();
        if (list is null)
        {
            return;
        }

        IIndexSync sync = SingleIndexSync.CreateInserter(index, value);
        list.Sync(sync, this);
    }

    public override void RemoveAt(int index)
    {
        ISyncList list = ResolveList();
        if (list is null)
        {
            return;
        }

        IIndexSync sync = SingleIndexSync.CreateRemover(index);
        list.Sync(sync, this);
    }

    public override bool CanDropIn(object value)
    {
        return (GetValue() as IDropInCheck)?.DropInCheck(value) ?? false;
    }

    public override object DropInConvert(object value)
    {
        return (GetValue() as IDropInCheck)?.DropInConvert(value) ?? value;
    }

    public ISyncList ResolveList()
    {
        var value = GetValue();

        var list = value as IViewList;
        if (list is null)
        {
            return value as ISyncList;
        }

        var model = FindModel();

        int viewId = list.ListViewId;
        if (viewId == 0 || viewId == ViewIds.TreeView || viewId == model.ViewId)
        {
            return list;
        }
        else
        {
            return null;
        }
    }
}