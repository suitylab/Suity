namespace Suity.Rex.Operators;

internal class Cast<TSource, TResult> : RexListenerBase<TSource, TResult>
{
    public Cast(IRexListener<TSource> source)
        : base(source)
    {
        source.Subscribe(o =>
        {
            object obj = o;
            HandleCallBack((TResult)obj);
        });
    }
}