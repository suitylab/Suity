using Suity.Editor.VirtualTree.Nodes;
using System;
using System.Collections;

namespace Suity.Editor.VirtualTree.Adapters;

public abstract class IListAdapter : VirtualNodeAdapter
{
    private IListVirtualNode _adapter;

    internal override void SetupNode(VirtualNode adapter)
    {
        base.SetupNode(adapter);
        _adapter = (IListVirtualNode)adapter;
    }

    public override bool CanDropIn(object value)
    {
        if (value == null)
        {
            return false;
        }

        Type elementType = ReflectionHelper.GetIListElementType(Owner.EditedType);
        if (!elementType.IsAssignableFrom(value.GetType()))
        {
            return false;
        }

        return true;
    }

    public abstract Type GetListElementType();

    public abstract IList ResolveIList();

    public virtual object CreateNewItem()
    { return null; }

    public virtual void OnItemAdded(object item)
    {
    }

    public virtual void OnItemRemoved(object item)
    {
    }
}

public abstract class UserVirtualListResolver<T> : IListAdapter
{
    public T SelectedObject => (T)GetValue();

    public override object CreateNewItem()
    {
        return typeof(T).CreateInstanceOf();
    }
}

public class EmptyIListAdapter : IListAdapter
{
    public static EmptyIListAdapter Instance { get; } = new();

    private EmptyIListAdapter()
    {
    }

    public override Type GetListElementType()
    {
        return typeof(Object);
    }

    public override IList ResolveIList()
    {
        return new object[0];
    }
}