using System;

namespace Suity.Rex.Operators;

internal class Where<T> : RexListenerBase<T, T>
{
    public Where(IRexListener<T> source, Predicate<T> predicate)
        : base(source)
    {
        if (predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        source.Subscribe(o =>
        {
            if (predicate(o))
            {
                HandleCallBack(o);
            }
        });
    }
}