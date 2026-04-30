using System;

namespace Suity.Views;

/// <summary>
/// Specifies that a class should be inserted into a specific position in the view hierarchy.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class InsertIntoAttribute : Attribute
{
    private readonly string _position;

    /// <summary>
    /// Initializes a new instance of <see cref="InsertIntoAttribute"/> with the specified position.
    /// </summary>
    /// <param name="positionalString">The position string indicating where to insert the class.</param>
    public InsertIntoAttribute(string positionalString)
    {
        this._position = positionalString;
    }

    /// <summary>
    /// Gets the position string indicating where to insert the class.
    /// </summary>
    public string Position => _position;
}

/// <summary>
/// Indicates that a class requests to override default behavior or implementation.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class RequestOverrideAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of <see cref="RequestOverrideAttribute"/>.
    /// </summary>
    public RequestOverrideAttribute()
    { }
}