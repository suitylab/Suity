using System;

namespace Suity.Rex.Operators;

internal class EventListener : IRexListener<object>, IDisposable
{
    private readonly IRexEvent _event;
    private Action<object> _callBack;

    public EventListener(IRexEvent rexEvent)
    {
        _event = rexEvent ?? throw new ArgumentException();
        _event.AddListener(OnEvent);
    }

    public IRexHandle Subscribe(Action<object> callBack)
    {
        _callBack += callBack;

        return this;
    }

    public void Dispose()
    {
        _callBack = null;
        _event.RemoveListener(OnEvent);
    }

    public IRexHandle Push()
    {
        return this;
    }

    private void OnEvent()
    {
        _callBack?.Invoke(null);
    }
}

internal class EventListener<T> : IRexListener<T>, IDisposable
{
    private readonly IRexEvent<T> _event;
    private Action<T> _callBack;

    public EventListener(IRexEvent<T> rexEvent)
    {
        _event = rexEvent ?? throw new ArgumentException();
        _event.AddListener(OnEvent);
    }

    public IRexHandle Subscribe(Action<T> callBack)
    {
        _callBack += callBack;

        return this;
    }

    public void Dispose()
    {
        _callBack = null;
        _event.RemoveListener(OnEvent);
    }

    public IRexHandle Push()
    {
        return this;
    }

    private void OnEvent(T value)
    {
        _callBack?.Invoke(value);
    }
}

internal class EventListener<T1, T2> : IRexListener<ActionArgument<T1, T2>>, IDisposable
{
    private readonly IRexEvent<T1, T2> _event;
    private Action<ActionArgument<T1, T2>> _callBack;

    public EventListener(IRexEvent<T1, T2> rexEvent)
    {
        _event = rexEvent ?? throw new ArgumentException();
        _event.AddListener(OnEvent);
    }

    public IRexHandle Subscribe(Action<ActionArgument<T1, T2>> callBack)
    {
        _callBack += callBack;

        return this;
    }

    public void Dispose()
    {
        _callBack = null;
        _event.RemoveListener(OnEvent);
    }

    public IRexHandle Push()
    {
        return this;
    }


    private void OnEvent(T1 t1, T2 t2)
    {
        _callBack?.Invoke(new ActionArgument<T1, T2>(t1, t2));
    }
}

internal class EventListener<T1, T2, T3> : IRexListener<ActionArgument<T1, T2, T3>>, IDisposable
{
    private readonly IRexEvent<T1, T2, T3> _event;
    private Action<ActionArgument<T1, T2, T3>> _callBack;

    public EventListener(IRexEvent<T1, T2, T3> rexEvent)
    {
        _event = rexEvent ?? throw new ArgumentException();
        _event.AddListener(OnEvent);
    }

    public IRexHandle Subscribe(Action<ActionArgument<T1, T2, T3>> callBack)
    {
        _callBack += callBack;

        return this;
    }

    public void Dispose()
    {
        _callBack = null;
        _event.RemoveListener(OnEvent);
    }

    public IRexHandle Push()
    {
        return this;
    }


    private void OnEvent(T1 t1, T2 t2, T3 t3)
    {
        _callBack?.Invoke(new ActionArgument<T1, T2, T3>(t1, t2, t3));
    }
}

internal class EventListener<T1, T2, T3, T4> : IRexListener<ActionArgument<T1, T2, T3, T4>>, IDisposable
{
    private readonly IRexEvent<T1, T2, T3, T4> _event;
    private Action<ActionArgument<T1, T2, T3, T4>> _callBack;

    public EventListener(IRexEvent<T1, T2, T3, T4> rexEvent)
    {
        _event = rexEvent ?? throw new ArgumentException();
        _event.AddListener(OnEvent);
    }

    public IRexHandle Subscribe(Action<ActionArgument<T1, T2, T3, T4>> callBack)
    {
        _callBack += callBack;

        return this;
    }

    public void Dispose()
    {
        _callBack = null;
        _event.RemoveListener(OnEvent);
    }

    public IRexHandle Push()
    {
        return this;
    }


    private void OnEvent(T1 t1, T2 t2, T3 t3, T4 t4)
    {
        _callBack?.Invoke(new ActionArgument<T1, T2, T3, T4>(t1, t2, t3, t4));
    }
}