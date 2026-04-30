using System;

namespace Suity.Rex.Operators;

internal class RexValueListener<T> : IRexListener<T>
{
    private readonly IRexValue<T> _value;

    private Action<T> _callBack;

    public RexValueListener(IRexValue<T> value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));

        _value.AddListener(HandleCallBack);
    }

    public void Dispose()
    {
        _callBack = null;
        _value.RemoveListener(HandleCallBack);
    }

    public IRexHandle Push()
    {
        HandleCallBack(_value.Value);

        return this;
    }

    public IRexHandle Subscribe(Action<T> callBack)
    {
        _callBack += callBack;

        return this;
    }

    private void HandleCallBack(T value)
    {
        _callBack?.Invoke(value);
    }
}