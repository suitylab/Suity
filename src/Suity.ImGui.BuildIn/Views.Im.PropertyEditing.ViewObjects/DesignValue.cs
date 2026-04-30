using Suity.Editor;
using Suity.Editor.Values;
using Suity.Synchonizing;

namespace Suity.Views.Im.PropertyEditing.ViewObjects;

/// <summary>
/// Represents a design value wrapper around an <see cref="SObject"/> that supports synchronization and navigation.
/// </summary>
internal class DesignValue : IDesignValue, ISyncObject, INavigable
{
    private SObject _value;

    /// <summary>
    /// Gets or sets the underlying SObject value. Setting this value updates the parent array if applicable.
    /// </summary>
    public SObject Value
    {
        get => _value;
        set
        {
            if (value is null)
            {
                return;
            }

            if (ReferenceEquals(value, _value))
            {
                return;
            }

            int index = _value.Index;

            _value = value;

            if (_value.Parent is SArray ary && index >= 0)
            {
                ary[index] = _value;
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DesignValue"/> class with an empty SObject.
    /// </summary>
    public DesignValue()
    {
        _value = new SObject();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DesignValue"/> class with the specified SObject value.
    /// </summary>
    /// <param name="value">The SObject value to wrap.</param>
    public DesignValue(SObject value)
    {
        _value = value;
    }

    /// <inheritdoc/>
    public object? GetNavigationTarget() => Value?.ObjectType;

    /// <inheritdoc/>
    void ISyncObject.Sync(IPropertySync sync, ISyncContext context)
    {
        _value = sync.Sync(nameof(Value), _value, SyncFlag.NotNull, Value) ?? new SObject();
    }
}

//internal class PushDesignObjesctAction : ValueAction
//{
//    readonly IValueTarget _target;

//    SObject[] _objects;

//    public PushDesignObjesctAction(IValueTarget target, SObject obj)
//        : base(target)
//    {
//        _target = target ?? throw new ArgumentNullException(nameof(target));

//        int count = target.GetValues().OfType<IViewDesignObject>().Count();
//        _objects = new SObject[count];
//        for (int i = 0; i < count; i++)
//        {
//            _objects[i] = Cloner.Clone(obj);
//        }
//    }

//    public override void DoAction()
//    {
//        IViewDesignObject[] ds = _target.GetValues().OfType<IViewDesignObject>().ToArray();

//        for (int i = 0; i < ds.Length; i++)
//        {
//            ds[i].DesignItems.Add(_objects.GetArrayItemSafe(i));
//        }
//    }

//    public override void UndoAction()
//    {
//        IViewDesignObject[] ds = _target.GetValues().OfType<IViewDesignObject>().ToArray();

//        for (int i = 0; i < ds.Length; i++)
//        {
//            ds[i].DesignItems.RemoveAt(ds[i].DesignItems.Count - 1);
//        }
//    }
//}