using System;

namespace Suity.Rex.VirtualDom;

/// <summary>
/// Represents a reactive property in the Rex tree that supports getting, setting, and computed values.
/// </summary>
/// <typeparam name="T">The type of the property value.</typeparam>
public sealed class RexProperty<T> : IRexProperty<T>
{
    private readonly RexTree _model;
    private readonly RexPath _path;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexProperty{T}"/> class.
    /// </summary>
    /// <param name="model">The RexTree to associate with this property.</param>
    /// <param name="path">The path within the RexTree.</param>
    public RexProperty(RexTree model, RexPath path)
    {
        _model = model;
        _path = path;
        var value = model.GetData<T>(_path);
        _model.SetData<T>(_path, value); // Force record initialization default value
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexProperty{T}"/> class with an initial value.
    /// </summary>
    /// <param name="model">The RexTree to associate with this property.</param>
    /// <param name="path">The path within the RexTree.</param>
    /// <param name="initValue">The initial value to set.</param>
    public RexProperty(RexTree model, RexPath path, T initValue)
    {
        _model = model;
        _path = path;
        _model.SetData<T>(_path, initValue);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexProperty{T}"/> class using a property definition.
    /// </summary>
    /// <param name="model">The RexTree to associate with this property.</param>
    /// <param name="define">The property definition to use.</param>
    public RexProperty(RexTree model, RexPropertyDefine<T> define)
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
    /// Gets or sets the current value of the property.
    /// </summary>
    public T Value
    {
        get
        {
            return _model.GetData<T>(_path);
        }
        set
        {
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
    /// Sets a computed data getter and setter for this property.
    /// </summary>
    /// <param name="getter">The function to compute the value. Can be null.</param>
    /// <param name="setter">The action to set the value. Can be null.</param>
    /// <returns>A disposable that can remove the computed data when disposed.</returns>
    public IDisposable SetComputed(Func<T> getter = null, Action<T> setter = null)
    {
        return _model.SetComputedData(_path, getter, setter);
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
    /// Sets the property value deeply, replacing all nested child data.
    /// </summary>
    /// <param name="value">The value to set deeply.</param>
    public void SetValueDeep(T value)
    {
        _model.SetDataDeep<T>(_path, value);
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
    /// Explicitly converts a <see cref="RexProperty{T}"/> to its value type.
    /// </summary>
    /// <param name="prop">The property to convert.</param>
    /// <returns>The current value of the property.</returns>
    public static explicit operator T(RexProperty<T> prop)
    {
        return prop.Value;
    }
}
