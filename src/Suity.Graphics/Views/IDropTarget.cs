namespace Suity.Views;

/// <summary>
/// Interface for objects that can receive drag-and-drop operations.
/// </summary>
public interface IDropTarget
{
    /// <summary>
    /// Called when a drag operation is over this target.
    /// </summary>
    /// <param name="e">The drag event data.</param>
    void DragOver(IDragEvent e);

    /// <summary>
    /// Called when a drop operation occurs on this target.
    /// </summary>
    /// <param name="e">The drag event data.</param>
    void DragDrop(IDragEvent e);
}