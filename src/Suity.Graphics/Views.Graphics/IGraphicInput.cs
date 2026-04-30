using System.Drawing;

namespace Suity.Views.Graphics;

/// <summary>
/// Graphic input
/// </summary>
public interface IGraphicInput
{
    /// <summary>
    /// Gets the type of GUI event.
    /// </summary>
    GuiEventTypes EventType { get; }

    /// <summary>
    /// Gets the mouse button associated with the event.
    /// </summary>
    GuiMouseButtons MouseButton { get; }

    /// <summary>
    /// Gets the mouse location, or null if not applicable.
    /// </summary>
    Point? MouseLocation { get; }

    /// <summary>
    /// Gets the mouse wheel delta.
    /// </summary>
    int MouseDelta { get; }

    /// <summary>
    /// Gets a value indicating whether the Alt key is pressed.
    /// </summary>
    bool AltKey { get; }

    /// <summary>
    /// Gets a value indicating whether the Control key is pressed.
    /// </summary>
    bool ControlKey { get; }

    /// <summary>
    /// Gets a value indicating whether the Shift key is pressed.
    /// </summary>
    bool ShiftKey { get; }

    /// <summary>
    /// Gets the key code for keyboard events.
    /// </summary>
    string KeyCode { get; }

    /// <summary>
    /// Gets the drag event data, or null if not a drag operation.
    /// </summary>
    IDragEvent DragEvent { get; }

    /// <summary>
    /// Checks if a specific mouse button is pressed.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns>True if the button is pressed.</returns>
    bool GetMouseButtonDown(GuiMouseButtons button);

    /// <summary>
    /// Indicates whether the input has been processed. If true, it indicates that the input has been processed and will not be passed to other controls.
    /// </summary>
    bool Handled { get; set; }
}

/// <summary>
/// Common implementation of IGraphicInput for standard events.
/// </summary>
public class CommonGraphicInput : IGraphicInput
{
    /// <summary>
    /// Represents a position outside the visible area.
    /// </summary>
    public static readonly Point OutsidePosition = new Point(-10000, -10000);

    /// <summary>
    /// Gets the empty input instance.
    /// </summary>
    public static readonly CommonGraphicInput Empty = new();

    /// <summary>
    /// Gets the refresh event input.
    /// </summary>
    public static readonly CommonGraphicInput Refresh = new(GuiEventTypes.Refresh);

    /// <summary>
    /// Gets the begin sync event input.
    /// </summary>
    public static readonly CommonGraphicInput BeginSync = new(GuiEventTypes.BeginSync);

    /// <summary>
    /// Gets the mouse enter event input.
    /// </summary>
    public static readonly CommonGraphicInput MouseIn = new(GuiEventTypes.MouseIn);

    /// <summary>
    /// Gets the mouse leave event input.
    /// </summary>
    public static readonly CommonGraphicInput MouseOut = new(GuiEventTypes.MouseOut);

    /// <summary>
    /// Gets the hover enter event input.
    /// </summary>
    public static readonly CommonGraphicInput HoverIn = new(GuiEventTypes.HoverIn);

    /// <summary>
    /// Gets the hover leave event input.
    /// </summary>
    public static readonly CommonGraphicInput HoverOut = new(GuiEventTypes.HoverOut);

    /// <summary>
    /// Gets the focus enter event input.
    /// </summary>
    public static readonly CommonGraphicInput FocusIn = new(GuiEventTypes.FocusIn);

    /// <summary>
    /// Gets the focus leave event input.
    /// </summary>
    public static readonly CommonGraphicInput FocusOut = new(GuiEventTypes.FocusOut);

    /// <summary>
    /// Gets the controlling enter event input.
    /// </summary>
    public static readonly CommonGraphicInput ControllingIn = new(GuiEventTypes.ControllingIn);

    /// <summary>
    /// Gets the controlling leave event input.
    /// </summary>
    public static readonly CommonGraphicInput ControllingOut = new(GuiEventTypes.ControllingOut);

    /// <summary>
    /// Gets the tooltip event input.
    /// </summary>
    public static readonly CommonGraphicInput ToolTip = new(GuiEventTypes.ToolTip);

    private CommonGraphicInput()
    {
        EventType = GuiEventTypes.None;
    }

    /// <summary>
    /// Creates a new CommonGraphicInput for the specified event type.
    /// </summary>
    /// <param name="eventType">The event type.</param>
    public CommonGraphicInput(GuiEventTypes eventType)
    {
        EventType = eventType;
    }

    /// <inheritdoc/>
    public GuiEventTypes EventType { get; }

    /// <inheritdoc/>
    public GuiMouseButtons MouseButton => GuiMouseButtons.None;

    /// <inheritdoc/>
    public Point? MouseLocation => null;

    /// <inheritdoc/>
    public int MouseDelta => 0;

    /// <inheritdoc/>
    public bool AltKey => false;

    /// <inheritdoc/>
    public bool ControlKey => false;

    /// <inheritdoc/>
    public bool ShiftKey => false;

    /// <inheritdoc/>
    public string KeyCode => null;

    /// <inheritdoc/>
    public bool GetMouseButtonDown(GuiMouseButtons button) => false;

    /// <inheritdoc/>
    public IDragEvent DragEvent => null;

    /// <inheritdoc/>
    public bool Handled { get; set; }
}

/// <summary>
/// Extension methods for IGraphicInput.
/// </summary>
public static class GraphicInputExtensions
{
    /// <summary>
    /// Checks if the input event is a keyboard event.
    /// </summary>
    /// <param name="input">The input to check.</param>
    /// <returns>True if the event is KeyDown or KeyUp.</returns>
    public static bool GetIsKeyEvent(this IGraphicInput input)
    {
        switch (input.EventType)
        {
            case GuiEventTypes.KeyDown:
            case GuiEventTypes.KeyUp:
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// Checks if the input event is a mouse drag event.
    /// </summary>
    /// <param name="input">The input to check.</param>
    /// <returns>True if the event is MouseMove or MouseUp with a button pressed.</returns>
    public static bool GetIsMouseDragEvent(this IGraphicInput input)
    {
        if (input.MouseButton != GuiMouseButtons.None)
        {
            // Need to include mouse movement and mouse lift events
            return input.EventType == GuiEventTypes.MouseMove || input.EventType == GuiEventTypes.MouseUp;
        }

        return false;
    }

    /// <summary>
    /// Checks if no compound keys (Shift, Control, Alt) are pressed.
    /// </summary>
    /// <param name="input">The input to check.</param>
    /// <returns>True if none of the modifier keys are pressed.</returns>
    public static bool GetNoCompondKey(this IGraphicInput input)
    {
        return !input.ShiftKey && !input.ControlKey && !input.AltKey;
    }

    /// <summary>
    /// Checks if only the Shift key is pressed.
    /// </summary>
    /// <param name="input">The input to check.</param>
    /// <returns>True if only Shift is pressed.</returns>
    public static bool GetOnlyShiftKey(this IGraphicInput input)
    {
        return input.ShiftKey && !input.ControlKey && !input.AltKey;
    }

    /// <summary>
    /// Checks if only the Control key is pressed.
    /// </summary>
    /// <param name="input">The input to check.</param>
    /// <returns>True if only Control is pressed.</returns>
    public static bool GetOnlyControlKey(this IGraphicInput input)
    {
        return !input.ShiftKey && input.ControlKey && !input.AltKey;
    }

    /// <summary>
    /// Checks if only the Alt key is pressed.
    /// </summary>
    /// <param name="input">The input to check.</param>
    /// <returns>True if only Alt is pressed.</returns>
    public static bool GetOnlyAltKey(this IGraphicInput input)
    {
        return !input.ShiftKey && !input.ControlKey && input.AltKey;
    }
}