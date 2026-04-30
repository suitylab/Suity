namespace Suity.Rex.Operators;

internal class If<TResult> : RexListenerBase<bool, TResult>
{
    public If(IRexListener<bool> source, TResult truePart, TResult falsePart)
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