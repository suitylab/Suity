using System.Collections.Generic;
using System.Linq;

namespace Suity.Rex.Operators;

internal class First<T> : RexListenerBase<IEnumerable<T>, T>
{
    public First(IRexListener<IEnumerable<T>> source)
        : base(source)
    {
        source.Subscribe(o =>
        {
            HandleCallBack(o.First());
        });
    }
}