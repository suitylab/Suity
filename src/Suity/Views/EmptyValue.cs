using System;

namespace Suity.Views;

/// <summary>
/// Represents an empty view value.
/// </summary>
[Serializable]
public class EmptyValue : IViewValue
{
    /// <summary>
    /// Gets the singleton empty instance.
    /// </summary>
    public static EmptyValue Empty { get; } = new();

    private EmptyValue()
    {
    }
}