using Suity.Rex.VirtualDom;
using System;

namespace Suity.Rex.Operators;

internal class SetDataTo<TSource, TResult> : RexListenerBase<TSource, TResult>
{
    public SetDataTo(IRexListener<TSource> source, RexTree engine, Func<TSource, RexPath> pathFunc, Func<TSource, TResult> dataFunc)
        : base(source)
    {
        if (engine is null)
        {
            throw new ArgumentNullException(nameof(engine));
        }

        if (pathFunc is null)
        {
            throw new ArgumentNullException(nameof(pathFunc));
        }

        if (dataFunc is null)
        {
            throw new ArgumentNullException(nameof(dataFunc));
        }

        source.Subscribe(o =>
        {
            var path = pathFunc(o);
            if (path is null)
            {
                throw new NullReferenceException();
            }
            var data = dataFunc(o);

            engine.SetData(path, data);
            HandleCallBack(data);
        });
    }
}