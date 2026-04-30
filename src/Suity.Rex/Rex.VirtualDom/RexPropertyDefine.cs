namespace Suity.Rex.VirtualDom;

/// <summary>
/// Defines a property in the Rex tree with a specific path and type.
/// Provides methods to get, set, and create property instances.
/// </summary>
/// <typeparam name="T">The type of the property value.</typeparam>
public sealed class RexPropertyDefine<T> : IRexTreeDefine<T>
{
    private readonly RexPath _path;

    /// <summary>
    /// Gets the path within the RexTree for this property definition.
    /// </summary>
    public RexPath Path => _path;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexPropertyDefine{T}"/> class with the specified path.
    /// </summary>
    /// <param name="path">The path within the RexTree.</param>
    public RexPropertyDefine(RexPath path)
    {
        _path = path;
    }

    /// <summary>
    /// Gets the current value of this property from the specified RexTree.
    /// </summary>
    /// <param name="model">The RexTree to retrieve the value from.</param>
    /// <returns>The current value of type <typeparamref name="T"/>.</returns>
    public T GetValue(RexTree model)
    {
        return model.GetData<T>(_path);
    }

    /// <summary>
    /// Sets the value of this property in the specified RexTree.
    /// </summary>
    /// <param name="model">The RexTree to set the value in.</param>
    /// <param name="value">The value to set.</param>
    public void SetValue(RexTree model, T value)
    {
        model.SetData<T>(_path, value);
    }

    /// <summary>
    /// Creates a new <see cref="RexProperty{T}"/> instance for this definition.
    /// </summary>
    /// <param name="model">The RexTree to associate with the property.</param>
    /// <returns>A new <see cref="RexProperty{T}"/> instance.</returns>
    public RexProperty<T> MakeProperty(RexTree model)
    {
        return new RexProperty<T>(model, _path);
    }

    /// <summary>
    /// Creates a new <see cref="RexPropertyCached{T}"/> instance for this definition.
    /// </summary>
    /// <param name="model">The RexTree to associate with the cached property.</param>
    /// <returns>A new <see cref="RexPropertyCached{T}"/> instance.</returns>
    public RexPropertyCached<T> MakePropertyCached(RexTree model)
    {
        return new RexPropertyCached<T>(model, _path);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return _path.ToString();
    }
}
