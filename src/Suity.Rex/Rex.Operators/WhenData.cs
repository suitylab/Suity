using Suity.Rex.VirtualDom;
using System;

namespace Suity.Rex.Operators;

internal class WhenData<T, TData> : RexListenerBase<T, T>
{
    public WhenData(IRexListener<T> source, RexTree engine, RexPath path, Predicate<TData> predicate)
        : base(source)
    {
        if (engine is null)
        {
            throw new ArgumentNullException(nameof(engine));
        }

        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        source.Subscribe(o =>
        {
            TData data = engine.GetData<TData>(path);
            if (predicate(data))
            {
                HandleCallBack(o);
            }
        });
    }
}