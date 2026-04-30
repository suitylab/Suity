namespace Suity.Rex.Operators;

internal class And : RexListenerBase<bool, bool, bool>
{
    private bool? _value1;
    private bool? _value2;

    public And(IRexListener<bool> source1, IRexListener<bool> source2) : base(source1, source2)
    {
        source1.Subscribe(v =>
        {
            _value1 = v;
            if (_value2.HasValue)
            {
                HandleCallBack(v && _value2.Value);
            }
        });

        source2.Subscribe(v =>
        {
            _value2 = v;
            if (_value1.HasValue)
            {
                HandleCallBack(_value1.Value && v);
            }
        });
    }
}