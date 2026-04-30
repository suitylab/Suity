using System.Collections.Generic;
using System.Linq;

namespace Suity.Rex.Operators;

internal class Take<T> : RexListenerBase<IEnumerable<T>, IEnumerable<T>>
{
    public Take(IRexListener<IEnumerable<T>> source, int count)
        : base(source)
    {
        source.Subscribe(o =>
        {
            HandleCallBack(o.Take(count));
        });
    }
}