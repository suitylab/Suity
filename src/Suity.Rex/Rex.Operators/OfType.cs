namespace Suity.Rex.Operators;

internal class OfType<TSource, TResult> : RexListenerBase<TSource, TResult> where TResult : class
{
    public OfType(IRexListener<TSource> source)
        : base(source)
    {
        source.Subscribe(o =>
        {
            TResult result = o as TResult;
            if (o != null)
            {
                HandleCallBack(result);
            }
        });
    }
}