using System.Collections.Generic;
using System.Linq;

namespace Suity.Rex.Operators;

internal class Skip<T> : RexListenerBase<IEnumerable<T>, IEnumerable<T>>
{
    public Skip(IRexListener<IEnumerable<T>> source, int count)
        : base(source)
    {
        source.Subscribe(o =>
        {
            HandleCallBack(o.Skip(count));
        });
    }
}