using Suity.Drawing;
using System.Drawing;

namespace Suity.Views.Gui;


/// <summary>
/// Interface defining the contract for tool windows within the application.
/// Tool windows are typically collapsible panels that can be docked or floated.
/// </summary>
public interface IToolWindow
{
    /// <summary>
    /// Gets the unique identifier for the tool window.
    /// This ID is used internally to track and reference the window.
    /// </summary>
    string WindowId { get; }

    /// <summary>
    /// Gets the title text displayed in the tool window's title bar.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets the icon displayed in the tool window's title bar.
    /// </summary>
    ImageDef Icon { get; }

    /// <summary>
    /// Gets the preferred docking position for the tool window.
    /// This determines where the window will try to dock when shown.
    /// </summary>
    DockHint DockHint { get; }

    /// <summary>
    /// Gets a value indicating whether the tool window can be docked with document windows.
    /// </summary>
    bool CanDockDocument { get; }

    /// <summary>
    /// Returns the UI element that represents the tool window's content.
    /// This is typically a WPF control or WinForms control.
    /// </summary>
    /// <returns>The UI object to display in the tool window</returns>
    object GetUIObject();

    /// <summary>
    /// Called when the tool window is shown.
    /// Implementations can use this to initialize content or start processes.
    /// </summary>
    void NotifyShow();

    /// <summary>
    /// Called when the tool window is hidden.
    /// Implementations can use this to clean up resources or pause processes.
    /// </summary>
    void NotifyHide();
}
