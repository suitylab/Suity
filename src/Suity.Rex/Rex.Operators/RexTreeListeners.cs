using Suity.Rex.VirtualDom;
using System;

namespace Suity.Rex.Operators;

internal class RexTreeListener<T> : IRexListener<T>
{
    private readonly RexTree _model;
    private readonly RexPath _path;

    private Action<T> _callBack;

    public RexTreeListener(RexTree model, RexPath path, string tag = null)
    {
        _model = model;
        _path = path;

        model.AddDataListener<T>(path, HandleCallBack, tag);
    }

    public void Dispose()
    {
        _callBack = null;
        _model.RemoveListener<T>(_path, HandleCallBack);
    }

    public IRexHandle Push()
    {
        HandleCallBack(_model.GetData<T>(_path));

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

internal class RexTreeBeforeListener<T> : IRexListener<T>
{
    private readonly RexTree _model;
    private readonly RexPath _path;

    private Action<T> _callBack;

    public RexTreeBeforeListener(RexTree model, RexPath path, string tag = null)
    {
        _model = model;
        _path = path;

        model.AddBeforeListener<T>(path, HandleCallBack, tag);
    }

    public void Dispose()
    {
        _callBack = null;
        _model.RemoveBeforeListener<T>(_path, HandleCallBack);
    }

    public IRexHandle Push()
    {
        HandleCallBack(_model.GetData<T>(_path));

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

internal class RexTreeAfterListener<T> : IRexListener<T>
{
    private readonly RexTree _model;
    private readonly RexPath _path;

    private Action<T> _callBack;

    public RexTreeAfterListener(RexTree model, RexPath path, string tag = null)
    {
        _model = model;
        _path = path;

        model.AddAfterListener<T>(path, HandleCallBack, tag);
    }

    public void Dispose()
    {
        _callBack = null;
        _model.UnsetAfterListener<T>(_path, HandleCallBack);
    }

    public IRexHandle Push()
    {
        HandleCallBack(_model.GetData<T>(_path));

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