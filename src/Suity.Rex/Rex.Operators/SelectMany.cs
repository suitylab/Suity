using System;
using System.Collections.Generic;

namespace Suity.Rex.Operators;

internal class SelectMany<TSource, TResult> : RexListenerBase<TSource, TResult>
{
    public SelectMany(IRexListener<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        : base(source)
    {
        if (selector is null)
        {
            throw new ArgumentNullException();
        }

        source.Subscribe(o =>
        {
            IEnumerable<TResult> result = selector(o);
            if (result != null)
            {
                foreach (var item in result)
                {
                    HandleCallBack(item);
                }
            }
        });
    }
}