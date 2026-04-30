namespace Suity.Rex.Operators;

internal class SelectIf<TResult> : RexListenerBase<bool, TResult>
{
    public SelectIf(IRexListener<bool> source, TResult truePart, TResult falsePart)
        : base(source)
    {
        source.Subscribe(o =>
        {
            if (o)
            {
                HandleCallBack(truePart);
            }
            else
            {
                HandleCallBack(falsePart);
            }
        });
    }
}