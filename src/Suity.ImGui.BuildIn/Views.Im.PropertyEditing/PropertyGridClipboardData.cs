using System.Collections.Generic;

namespace Suity.Views.Im.PropertyEditing;

/// <summary>
/// Represents clipboard data containing property values for copy/paste operations
/// in the property grid.
/// </summary>
internal class PropertyGridClipboardData
{
    /// <summary>
    /// Gets the collection of values stored in the clipboard.
    /// </summary>
    public IEnumerable<object?> Values { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyGridClipboardData"/> class.
    /// </summary>
    /// <param name="values">The values to store in the clipboard. Defaults to an empty collection if null.</param>
    public PropertyGridClipboardData(IEnumerable<object?>? values)
    {
        values ??= [];

        Values = values;
    }
}
