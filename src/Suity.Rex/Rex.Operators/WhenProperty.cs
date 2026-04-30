using Suity.Rex.VirtualDom;
using System;

namespace Suity.Rex.Operators;

internal class WhenProperty<T, TData> : RexListenerBase<T, T>
{
    public WhenProperty(IRexListener<T> source, IRexProperty<TData> property, Predicate<TData> predicate)
        : base(source)
    {
        if (property is null)
        {
            throw new ArgumentNullException(nameof(property));
        }

        if (predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        source.Subscribe(o =>
        {
            if (predicate(property.Value))
            {
                HandleCallBack(o);
            }
        });
    }
}