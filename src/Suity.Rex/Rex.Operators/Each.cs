using System.Collections.Generic;

namespace Suity.Rex.Operators;

internal class Each<T> : RexListenerBase<IEnumerable<T>, T>
{
    public Each(IRexListener<IEnumerable<T>> source)
        : base(source)
    {
        source.Subscribe(o =>
        {
            if (o != null)
            {
                foreach (var item in o)
                {
                    HandleCallBack(item);
                }
            }
        });
    }
}