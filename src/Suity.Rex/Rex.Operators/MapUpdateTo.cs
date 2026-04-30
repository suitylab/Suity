using Suity.Rex.VirtualDom;
using System;

namespace Suity.Rex.Operators;

internal class MapUpdateTo<T> : RexListenerBase<T, T>
{
    public MapUpdateTo(IRexListener<T> source, RexTree engine, Func<T, RexPath> pathFunc)
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

        source.Subscribe(o =>
        {
            var path = pathFunc(o);
            if (path is null)
            {
                throw new NullReferenceException();
            }

            engine.UpdateData(path);
            HandleCallBack(o);
        });
    }
}