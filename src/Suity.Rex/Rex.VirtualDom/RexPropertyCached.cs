using System;

namespace Suity.Rex.VirtualDom;

/// <summary>
/// A cached property that maintains a local copy of the value and automatically syncs with the RexTree.
/// Reduces tree lookups by caching the current value locally.
/// </summary>
/// <typeparam name="T">The type of the property value.</typeparam>
public sealed class RexPropertyCached<T> : IRexProperty<T>, IDisposable
{
    private readonly RexTree _model;
    private readonly RexPath _path;
    private T _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexPropertyCached{T}"/> class.
    /// </summary>
    /// <param name="model">The RexTree to associate with this property.</param>
    /// <param name="path">The path within the RexTree.</param>
    public RexPropertyCached(RexTree model, RexPath path)
    {
        _model = model;
        _path = path;
        _value = model.GetData<T>(_path);
        _model.SetData<T>(_path, _value); // Mandatory recording of default values during initialization
        _model.AddDataListener<T>(_path, HandleCallBack);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexPropertyCached{T}"/> class with an initial value.
    /// </summary>
    /// <param name="model">The RexTree to associate with this property.</param>
    /// <param name="path">The path within the RexTree.</param>
    /// <param name="initValue">The initial value to set.</param>
    public RexPropertyCached(RexTree model, RexPath path, T initValue)
    {
        _model = model;
        _path = path;
        _value = initValue;
        _model.SetData<T>(_path, initValue);
        _model.AddDataListener<T>(_path, HandleCallBack);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexPropertyCached{T}"/> class using a property definition.
    /// </summary>
    /// <param name="model">The RexTree to associate with this property.</param>
    /// <param name="define">The property definition to use.</param>
    public RexPropertyCached(RexTree model, RexPropertyDefine<T> define)
        : this(model, define.Path)
    {
    }

    /// <summary>
    /// Gets the RexTree associated with this property.
    /// </summary>
    public RexTree Tree => _model;

    /// <summary>
    /// Gets the path within the RexTree for this property.
    /// </summary>
    public RexPath Path => _path;

    /// <summary>
    /// Gets or sets the cached value of the property.
    /// Setting the value also updates the RexTree.
    /// </summary>
    public T Value
    {
        get
        {
            return _value;
        }
        set
        {
            _value = value;
            _model.SetData<T>(_path, value);
        }
    }

    /// <summary>
    /// Sets the property value in a queued manner for deferred execution.
    /// </summary>
    /// <param name="value">The value to set.</param>
    public void SetValueQueued(T value)
    {
        _model.SetDataQueued<T>(_path, value);
    }

    /// <summary>
    /// Sets the property value deeply, replacing all nested child data.
    /// </summary>
    /// <param name="value">The value to set deeply.</param>
    public void SetValueDeep(T value)
    {
        _model.SetDataDeep<T>(_path, value);
    }

    /// <summary>
    /// Triggers an update notification for this property's path.
    /// </summary>
    public void Update()
    {
        _model.UpdateData(_path);
    }

    /// <summary>
    /// Triggers a queued update notification for this property's path.
    /// </summary>
    public void UpdateQueued()
    {
        _model.UpdateDataQueued(_path);
    }

    /// <summary>
    /// Handles the callback when the property value changes in the RexTree.
    /// Updates the local cached value.
    /// </summary>
    /// <param name="value">The new value from the RexTree.</param>
    private void HandleCallBack(T value)
    {
        _value = value;
    }

    /// <summary>
    /// Removes the data listener from the RexTree.
    /// </summary>
    public void Dispose()
    {
        _model.RemoveListener<T>(_path, HandleCallBack);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        T value = Value;
        if (value != null)
        {
            return _path.ToString() + " = " + value.ToString();
        }
        else
        {
            return _path.ToString() + " = null";
        }
    }

    /// <summary>
    /// Explicitly converts a <see cref="RexPropertyCached{T}"/> to its value type.
    /// </summary>
    /// <param name="prop">The cached property to convert.</param>
    /// <returns>The current value of the property.</returns>
    public static explicit operator T(RexPropertyCached<T> prop)
    {
        return prop.Value;
    }
}
