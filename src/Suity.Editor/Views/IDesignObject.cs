using Suity.Editor.Values;

namespace Suity.Views;

/// <summary>
/// Represents a design object with design-time property information and design items.
/// </summary>
public interface IDesignObject : IViewObject
{
    /// <summary>
    /// Gets the name of the design property.
    /// </summary>
    string DesignPropertyName { get; }

    /// <summary>
    /// Gets the description of the design property.
    /// </summary>
    string DesignPropertyDescription { get; }

    /// <summary>
    /// Gets the collection of design items.
    /// </summary>
    SArray DesignItems { get; }
}
