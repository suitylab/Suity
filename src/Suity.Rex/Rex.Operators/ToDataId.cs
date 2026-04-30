namespace Suity.Rex.Operators;

internal class ToDataId<TSource> : RexListenerBase<TSource, string> where TSource : class
{
    public ToDataId(IRexListener<TSource> source)
        : base(source)
    {
        source.Subscribe(o =>
        {
            string result = RexGlobalResolve.Current?.GetDataId(o);
            HandleCallBack(result);
        });
    }
}