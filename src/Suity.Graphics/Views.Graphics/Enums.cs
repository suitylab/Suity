namespace Suity.Views.Graphics;

/// <summary>
/// Enumeration of GUI event types.
/// </summary>
public enum GuiEventTypes
{
    /// <summary>
    /// No event.
    /// </summary>
    None,

    /// <summary>
    /// Mouse button pressed.
    /// </summary>
    MouseDown,

    /// <summary>
    /// Mouse button released.
    /// </summary>
    MouseUp,

    /// <summary>
    /// Mouse single click.
    /// </summary>
    MouseClick,

    /// <summary>
    /// Mouse double click.
    /// </summary>
    MouseDoubleClick,

    /// <summary>
    /// Mouse moved.
    /// </summary>
    MouseMove,

    /// <summary>
    /// Mouse wheel scrolled.
    /// </summary>
    MouseWheel,

    /// <summary>
    /// Mouse entered the control area.
    /// </summary>
    MouseIn,

    /// <summary>
    /// Mouse left the control area.
    /// </summary>
    MouseOut,

    /// <summary>
    /// Hover state entered.
    /// </summary>
    HoverIn,

    /// <summary>
    /// Hover state exited.
    /// </summary>
    HoverOut,

    /// <summary>
    /// Focus entered.
    /// </summary>
    FocusIn,

    /// <summary>
    /// Focus exited.
    /// </summary>
    FocusOut,

    /// <summary>
    /// Controlling state entered.
    /// </summary>
    ControllingIn,

    /// <summary>
    /// Controlling state exited.
    /// </summary>
    ControllingOut,

    /// <summary>
    /// Key pressed.
    /// </summary>
    KeyDown,

    /// <summary>
    /// Key released.
    /// </summary>
    KeyUp,

    /// <summary>
    /// Command key pressed.
    /// </summary>
    CommandKey,

    /// <summary>
    /// Control resized.
    /// </summary>
    Resize,

    /// <summary>
    /// Timer event.
    /// </summary>
    Timer,

    /// <summary>
    /// Refresh requested.
    /// </summary>
    Refresh,

    /// <summary>
    /// Begin synchronization.
    /// </summary>
    BeginSync,

    /// <summary>
    /// Drag over operation.
    /// </summary>
    DragOver,

    /// <summary>
    /// Drag drop operation.
    /// </summary>
    DragDrop,

    /// <summary>
    /// Tooltip requested.
    /// </summary>
    ToolTip,
}

/// <summary>
/// Enumeration of mouse buttons.
/// </summary>
public enum GuiMouseButtons
{
    /// <summary>
    /// No button.
    /// </summary>
    None,

    /// <summary>
    /// Left mouse button.
    /// </summary>
    Left,

    /// <summary>
    /// Middle mouse button.
    /// </summary>
    Middle,

    /// <summary>
    /// Right mouse button.
    /// </summary>
    Right,
}

/// <summary>
/// Enumeration of cursor types.
/// </summary>
public enum GuiCursorTypes
{
    /// <summary>
    /// Default cursor.
    /// </summary>
    Default,

    /// <summary>
    /// Hand cursor.
    /// </summary>
    Hand,

    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Text selection cursor (I-beam).
    /// </summary>
    IBeam,

    /// <summary>
    /// Vertical split cursor.
    /// </summary>
    VSplit,

    /// <summary>
    /// Horizontal split cursor.
    /// </summary>
    HSplit,

    /// <summary>
    /// No vertical movement cursor.
    /// </summary>
    NoMoveVert,

    /// <summary>
    /// No horizontal movement cursor.
    /// </summary>
    NoMoveHoriz,

    /// <summary>
    /// Size all directions cursor.
    /// </summary>
    SizeAll,

    /// <summary>
    /// Size northeast-southwest cursor.
    /// </summary>
    SizeNESW,

    /// <summary>
    /// Size north-south cursor.
    /// </summary>
    SizeNS,

    /// <summary>
    /// Size northwest-southeast cursor.
    /// </summary>
    SizeNWSE,

    /// <summary>
    /// Size west-east cursor.
    /// </summary>
    SizeWE,
}

/// <summary>
/// Enumeration of color styles for UI theming.
/// </summary>
public enum ColorStyle
{
    /// <summary>
    /// Normal/default color.
    /// </summary>
    Normal,

    /// <summary>
    /// MDI background color.
    /// </summary>
    MdiBackground,

    /// <summary>
    /// Primary background color.
    /// </summary>
    Background,

    /// <summary>
    /// Inner background color.
    /// </summary>
    BackgroundInner,

    /// <summary>
    /// Focus color.
    /// </summary>
    Focus,

    /// <summary>
    /// Inactive focus color.
    /// </summary>
    FocusInactive,

    /// <summary>
    /// Button color.
    /// </summary>
    Button,

    /// <summary>
    /// CheckBox color.
    /// </summary>
    CheckBox,

    /// <summary>
    /// Border color.
    /// </summary>
    Border,

    /// <summary>
    /// Scroll bar color.
    /// </summary>
    ScrollBar,

    /// <summary>
    /// Progress bar color.
    /// </summary>
    ProgressBar,
}