namespace Suity.Views.Gui;

/// <summary>
/// Specifies the docking position for a window within the editor layout.
/// </summary>
public enum DockHint
{
    /// <summary>
    /// Dock as a document tab in the main editor area.
    /// </summary>
    Document,
    /// <summary>
    /// Display as a floating window.
    /// </summary>
    Float,
    /// <summary>
    /// Dock at the top of the editor.
    /// </summary>
    Top,
    /// <summary>
    /// Dock at the left side of the editor.
    /// </summary>
    Left,
    /// <summary>
    /// Dock at the bottom of the editor.
    /// </summary>
    Bottom,
    /// <summary>
    /// Dock at the right side of the editor.
    /// </summary>
    Right,
    /// <summary>
    /// Hide the window from view.
    /// </summary>
    Hidden,
}