using System;

namespace Suity.Rex.Operators;

internal class Select<TSource, TResult> : RexListenerBase<TSource, TResult>
{
    public Select(IRexListener<TSource> source, Func<TSource, TResult> selector)
        : base(source)
    {
        if (selector is null)
        {
            throw new ArgumentNullException();
        }

        source.Subscribe(o =>
        {
            TResult result = selector(o);
            HandleCallBack(result);
        });
    }
}