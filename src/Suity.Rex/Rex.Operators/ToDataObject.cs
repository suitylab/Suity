namespace Suity.Rex.Operators;

internal class ToDataObject<TResult> : RexListenerBase<string, TResult> where TResult : class
{
    public ToDataObject(IRexListener<string> source)
        : base(source)
    {
        source.Subscribe(key =>
        {
            TResult result = RexGlobalResolve.Current?.GetObject<TResult>(key);
            HandleCallBack(result);
        });
    }
}