namespace Suity.Rex.Operators;

internal class NotNull<T> : RexListenerBase<T, T> where T : class
{
    public NotNull(IRexListener<T> source)
        : base(source)
    {
        source.Subscribe(o =>
        {
            if (o != null)
            {
                HandleCallBack(o);
            }
        });
    }
}