using System.Collections.Generic;
using System.Linq;

namespace Suity.Rex.Operators;

internal class FirstOrDefault<T> : RexListenerBase<IEnumerable<T>, T>
{
    public FirstOrDefault(IRexListener<IEnumerable<T>> source)
        : base(source)
    {
        source.Subscribe(o =>
        {
            if (o != null)
            {
                HandleCallBack(o.FirstOrDefault());
            }
            else
            {
                HandleCallBack(default);
            }
        });
    }
}