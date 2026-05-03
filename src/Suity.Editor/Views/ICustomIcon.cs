using Suity.Drawing;

namespace Suity.Views;

/// <summary>
/// Represents an object that provides a custom icon.
/// </summary>
public interface ICustomIcon
{
    /// <summary>
    /// Gets the custom icon image.
    /// </summary>
    ImageDef CustomIcon { get; }
}
