namespace Suity.Rex.Operators;

internal class Queued<T> : RexListenerBase<T, T>
{
    public Queued(IRexListener<T> source)
        : base(source)
    {
        source.Subscribe(o =>
        {
            RexGlobalResolve.Current?.DoQueuedAction(() =>
            {
                HandleCallBack(o);
            });
        });
    }
}