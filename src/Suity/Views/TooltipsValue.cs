using System;

namespace Suity.Views;

/// <summary>
/// Represents a tooltip view value.
/// </summary>
[Serializable]
public class TooltipsValue : IViewValue
{
    /// <summary>
    /// Gets the singleton empty instance.
    /// </summary>
    public static TooltipsValue Empty { get; } = new();

    private TooltipsValue()
    {
    }
}
