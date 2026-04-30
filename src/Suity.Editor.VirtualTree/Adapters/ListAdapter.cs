using Suity.Editor.VirtualTree.Nodes;
using System;
using System.Threading.Tasks;

namespace Suity.Editor.VirtualTree.Adapters;

public abstract class ListAdapter : VirtualNodeAdapter
{
    private ListVirtualNode _node;

    internal override void SetupNode(VirtualNode node)
    {
        base.SetupNode(node);
        _node = (ListVirtualNode)node;
    }

    public abstract int Count { get; }

    public abstract object GetItem(int index);

    public abstract void SetItem(int index, object value);

    public abstract Task<object> GuiCreateItemAsync(Type typeHint = null);

    public abstract Task<object[]> GuiCreateItemsAsync(Type typeHint = null);

    public abstract void Insert(int index, object value);

    public abstract void RemoveAt(int index);
}

public abstract class UserVirtualListAdapter<T> : ListAdapter
{
    public T SelectedObject => (T)GetValue();

    public override Task<object> GuiCreateItemAsync(Type typeHint = null)
    {
        return Task.FromResult<object>(typeof(T).CreateInstanceOf());
    }

    public override async Task<object[]> GuiCreateItemsAsync(Type typeHint = null)
    {
        var obj = await GuiCreateItemAsync(typeHint);

        return [obj];
    }
}