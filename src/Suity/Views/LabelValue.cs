using System;

namespace Suity.Views;

/// <summary>
/// Represents a label view value.
/// </summary>
[Serializable]
public class LabelValue : IViewValue
{
    /// <summary>
    /// Gets the singleton empty instance.
    /// </summary>
    public static LabelValue Empty { get; } = new();

    private LabelValue()
    {
    }
}