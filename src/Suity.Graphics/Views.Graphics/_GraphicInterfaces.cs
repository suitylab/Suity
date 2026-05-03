using Suity.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Views.Graphics;

/// <summary>
/// Interface for registering and showing context menus in graphic views.
/// </summary>
public interface IGraphicContextMenu
{
    /// <summary>
    /// Registers a context menu command.
    /// </summary>
    /// <param name="menuCommand">The menu command to register.</param>
    void RegisterContextMenu(object menuCommand);

    /// <summary>
    /// Shows the context menu at the current position.
    /// </summary>
    /// <param name="menuCommand">The menu command to show.</param>
    /// <param name="selectedItems">Optional collection of selected items.</param>
    void ShowContextMenu(object menuCommand, IEnumerable<object> selectedItems = null);
}

/// <summary>
/// Interface for creating custom controls in graphic views.
/// </summary>
public interface IGraphicCustomControl
{
    /// <summary>
    /// Creates a custom control of the specified type.
    /// </summary>
    /// <param name="type">The type of control to create.</param>
    /// <param name="name">The name of the control.</param>
    /// <param name="rect">The bounding rectangle for the control.</param>
    /// <returns>A new instance of the custom control.</returns>
    ICustomControl CreateControl(string type, string name, Rectangle rect);
}

/// <summary>
/// Interface representing a custom control that can be moved and disposed.
/// </summary>
public interface ICustomControl : IDisposable
{
    /// <summary>
    /// Moves the control to a new position and size.
    /// </summary>
    /// <param name="rect">The new bounding rectangle.</param>
    void Move(Rectangle rect);
}

/// <summary>
/// Interface for initiating drag-and-drop operations in graphic views.
/// </summary>
public interface IGraphicDragDrop
{
    /// <summary>
    /// Initiates a drag-and-drop operation with the specified object.
    /// </summary>
    /// <param name="obj">The object to drag.</param>
    void DoDragDrop(object obj);
}

/// <summary>
/// Interface for showing combo box drop-down menus in graphic views.
/// </summary>
public interface IGraphicDropDownEdit
{
    /// <summary>
    /// Shows a drop-down list for combo box editing.
    /// </summary>
    /// <param name="rect">The rectangle where the drop-down should appear.</param>
    /// <param name="items">The collection of items to display.</param>
    /// <param name="selectedItem">The currently selected item.</param>
    /// <param name="callBack">The callback invoked when an item is selected.</param>
    void ShowComboBoxDropDown(Rectangle rect, IEnumerable<object> items, object selectedItem, Action<object> callBack);
}

/// <summary>
/// Text box submit mode.
/// </summary>
public enum TextBoxEditSubmitMode
{
    /// <summary>
    /// Submit when pointer leaves the control.
    /// </summary>
    Auto,

    /// <summary>
    /// Submit when Enter key is pressed or pointer leaves the control.
    /// </summary>
    Enter,

    /// <summary>
    /// Submit when text is changed.
    /// </summary>
    TextChanged,
}

/// <summary>
/// Options for configuring text box editing behavior.
/// </summary>
public record TextBoxEditOptions
{
    /// <summary>
    /// Gets or sets whether the text box is read-only.
    /// </summary>
    public bool IsReadonly { get; init; }

    /// <summary>
    /// Gets or sets whether the text box is in password mode.
    /// </summary>
    public bool IsPassword { get; init; }

    /// <summary>
    /// Gets or sets whether the text box supports multiple lines.
    /// </summary>
    public bool MultiLine { get; init; }

    /// <summary>
    /// Gets or sets whether the text box should auto-fit its content.
    /// </summary>
    public bool AutoFit { get; init; }

    /// <summary>
    /// Gets or sets the submit mode for the text box.
    /// </summary>
    public TextBoxEditSubmitMode SubmitMode { get; init; } = TextBoxEditSubmitMode.Auto;

    /// <summary>
    /// Gets or sets the font used for the text box.
    /// </summary>
    public FontDef Font { get; init; }

    /// <summary>
    /// Gets or sets the callback invoked when the text is edited.
    /// </summary>
    public Action<string> EditedCallBack { get; init; }
}

/// <summary>
/// Interface for text box editing in graphic views.
/// </summary>
public interface IGraphicTextBoxEdit
{
    /// <summary>
    /// Begins text editing in the specified rectangle.
    /// </summary>
    /// <param name="rect">The bounding rectangle for the text box.</param>
    /// <param name="text">The initial text to display.</param>
    /// <param name="option">The options for text editing.</param>
    void BeginTextEdit(Rectangle rect, string text, TextBoxEditOptions option);

    /// <summary>
    /// Ends the current text editing session.
    /// </summary>
    void EndTextEdit();
}

/// <summary>
/// Interface for showing tooltips in graphic views.
/// </summary>
public interface IGraphicToolTip
{
    /// <summary>
    /// Shows a tooltip at the specified position.
    /// </summary>
    /// <param name="toolTip">The tooltip text to display.</param>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    void ShowToolTip(string toolTip, int x, int y);
}

/// <summary>
/// Interface for showing color pickers in graphic views.
/// </summary>
public interface IGraphicColorPicker
{
    /// <summary>
    /// Shows a color picker dialog.
    /// </summary>
    /// <param name="rect">The rectangle where the color picker should appear.</param>
    /// <param name="color">The initial color to display.</param>
    /// <param name="selected">The callback invoked when a color is selected.</param>
    void ShowColorPicker(Rectangle rect, Color color, Action<Color, bool> selected);
}