namespace Suity.Rex.Operators;

internal class IfHasValue<TResult> : RexListenerBase<object, TResult>
{
    public IfHasValue(IRexListener<object> source, TResult truePart, TResult falsePart)
        : base(source)
    {
        source.Subscribe(o =>
        {
            if (o != null)
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