namespace Suity.Editor.Design;

/// <summary>
/// Provides functionality for building design objects.
/// </summary>
public interface IDesignBuilder
{
    /// <summary>
    /// Sets the binding info for this builder.
    /// </summary>
    void SetBindingInfo(object bindingInfo);

    /// <summary>
    /// Updates the attributes for this builder.
    /// </summary>
    void UpdateAttributes(IAttributeDesign attributes);
}