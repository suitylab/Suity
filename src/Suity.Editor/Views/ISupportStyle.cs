using Suity.NodeQuery;

namespace Suity.Views;

/// <summary>
/// Represents a view that supports styling with read-only capability.
/// </summary>
public interface ISupportStyle
{
    /// <summary>
    /// Gets or sets a value indicating whether the styles are read-only.
    /// </summary>
    bool ReadOnly { get; set; }

    /// <summary>
    /// Gets or sets the node reader for accessing style information.
    /// </summary>
    INodeReader Styles { get; set; }
}
