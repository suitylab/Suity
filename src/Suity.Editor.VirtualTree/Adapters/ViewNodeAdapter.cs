using static Suity.Helpers.GlobalLocalizer;
using Suity.Editor.VirtualTree.Nodes;
using Suity.NodeQuery;
using Suity.Synchonizing;
using Suity.Synchonizing.Preset;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Suity.Editor.VirtualTree.Adapters;

internal class ViewNodeAdapter : ListAdapter, IViewObjectSetup, IViewSetValue
{
    public override int Count
    {
        get
        {
            IViewNode list = ResolveNode();

            return list?.GetList()?.Count ?? 0;
        }
    }

    public override object GetItem(int index)
    {
        IViewNode list = ResolveNode();
        if (list is null)
        {
            return null;
        }

        IIndexSync sync = SingleIndexSync.CreateGetter(index);
        list.GetList()?.Sync(sync, this);

        return sync.Value;
    }

    public override void SetItem(int index, object value)
    {
        IViewNode list = ResolveNode();
        if (list is null)
        {
            return;
        }

        IIndexSync sync = SingleIndexSync.CreateSetter(index, value);
        list.GetList()?.Sync(sync, this);
    }

    public override async Task<object> GuiCreateItemAsync(Type typeHint = null)
    {
        if (ResolveNode() is not IHasObjectCreationGUI list)
        {
            await DialogUtility.ShowMessageBoxAsyncL("Create new item failed.");
            return null;
        }

        return list.GuiCreateObjectAsync(typeHint);
    }

    public override async Task<object[]> GuiCreateItemsAsync(Type typeHint = null)
    {
        if (ResolveNode() is not IHasObjectCreationGUI list)
        {
            await DialogUtility.ShowMessageBoxAsyncL("Create new item failed.");
            return null;
        }

        var obj = await list.GuiCreateObjectAsync(typeHint);

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
        IViewNode list = ResolveNode();
        if (list is null)
        {
            return;
        }

        IIndexSync sync = SingleIndexSync.CreateInserter(index, value);
        list.GetList()?.Sync(sync, this);
    }

    public override void RemoveAt(int index)
    {
        IViewNode list = ResolveNode();
        if (list is null)
        {
            return;
        }

        IIndexSync sync = SingleIndexSync.CreateRemover(index);
        list.GetList()?.Sync(sync, this);
    }

    public override bool CanDropIn(object value)
    {
        var node = ResolveNode();
        if (ReferenceEquals(node, value))
        {
            return false;
        }

        return (ResolveNode())?.DropInCheck(value) ?? false;
    }

    public override object DropInConvert(object value)
    {
        var node = ResolveNode();
        if (ReferenceEquals(node, value))
        {
            return null;
        }

        return (ResolveNode())?.DropInConvert(value) ?? value;
    }

    #region IViewObjectSetup

    bool IViewObjectSetup.IsTypeSupported(Type type)
    {
        return type.IsAssignableFrom(Owner.EditedType);
    }

    bool IViewObjectSetup.IsViewIdSupported(int viewId)
    {
        return Owner.IsViewIdSupported(viewId);
    }

    IEnumerable<object> IViewObjectSetup.GetObjects() => [GetValue()];

    public void AddField(Type type, ViewProperty property)
    {
    }

    public void FieldOf<T>(ViewProperty property)
    {
    }

    public void Field<T>(T value, ViewProperty property)
    {
    }

    public INodeReader Styles => null;

    #endregion

    #region ISetValueAction

    public void SetValue(string name, object value)
    {
        var model = Owner.FindModel();
        if (model != null)
        {
            model.HandleSetterAction(new CompondSyncSetterAction((ListVirtualNode)Owner, name, value));
        }
        else
        {
            SinglePropertySync sync = SinglePropertySync.CreateSetter(name, value);
            ((ISyncObject)GetValue()).Sync(sync, this);
        }
    }

    #endregion

    #region CompondSyncSetterAction

    private class CompondSyncSetterAction : VirtualNodeSetterAction
    {
        private readonly string _name;
        private readonly object _oldValue;
        private readonly object _newValue;

        private readonly ISyncObject _obj;

        public override string Name => _name;

        public CompondSyncSetterAction(ListVirtualNode node, string name, object value)
            : base(node, name)
        {
            _name = name;
            _newValue = value;

            _obj = (ISyncObject)node.DisplayedValue;

            SinglePropertySync sync = SinglePropertySync.CreateGetter(_name);
            _obj.Sync(sync, this);

            _oldValue = sync.Value;
        }

        public override void Do()
        {
            SinglePropertySync sync = SinglePropertySync.CreateSetter(_name, _newValue);
            _obj.Sync(sync, this);

            base.Do();
        }

        public override void Undo()
        {
            SinglePropertySync sync = SinglePropertySync.CreateSetter(_name, _oldValue);
            _obj.Sync(sync, this);

            base.Undo();
        }
    }

    #endregion

    public IViewNode ResolveNode()
    {
        if (GetValue() is not IViewNode node)
        {
            return null;
        }

        var model = FindModel();
        int viewId = node.ListViewId;
        if (viewId == 0 || viewId == ViewIds.TreeView || viewId == model.ViewId)
        {
            return node;
        }
        else
        {
            return null;
        }
    }
}