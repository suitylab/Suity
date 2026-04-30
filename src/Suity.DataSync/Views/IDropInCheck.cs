namespace Suity.Views;

/// <summary>
/// Drop action check
/// </summary>
public interface IDropInCheck
{
    /// <summary>
    /// Retrieve the specified value to determine if this operation is valid
    /// </summary>
    /// <param name="value">The object to be dragged</param>
    /// <returns>Return whether it is draggable or not</returns>
    bool DropInCheck(object value);

    /// <summary>
    /// Convert the incoming value to the current suitable value. After<see cref="DropInCheck"/>passes, execute before starting to transfer values.
    /// </summary>
    /// <param name="value">Drag and drop the incoming value</param>
    /// <returns>The converted value, if no conversion is required, returns the 'value' directly.</returns>
    object DropInConvert(object value);
}