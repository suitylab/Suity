using Suity.Rex.VirtualDom;
using System;

namespace Suity.Rex.Operators;

internal class MapActionTo<T> : RexListenerBase<T, T> where T : ActionArguments
{
    public MapActionTo(IRexListener<T> source, RexTree engine, RexPath path)
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

        source.Subscribe(o =>
        {
            engine.DoAction(path, (ActionArguments)o);
            HandleCallBack(o);
        });
    }
}