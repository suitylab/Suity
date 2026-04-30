using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Suity.Views;
using Suity.Views.Graphics;
using System.Diagnostics;

namespace Suity.Controls;

/// <summary>
/// Avalonia-specific implementation of the graphic input interface.
/// </summary>
internal class AvaGraphicInput : IGraphicInput
{
    /// <summary>
    /// The standard wheel delta value for mouse wheel events.
    /// </summary>
    public const int WheelDelta = 30;

    readonly Visual _parent;

    GuiEventTypes _eventTypes;
    RoutedEventArgs? _eventArgs;

    Key _key;
    GuiMouseButtons _mouseButton;
    System.Drawing.Point _lastMouseLocation;

    Func<System.Drawing.Point>? _mouseLocationGetter;
    Func<KeyModifiers>? _keyModifierGetter;


    private bool _handled;

    /// <summary>
    /// Gets the last routed event arguments processed.
    /// </summary>
    public RoutedEventArgs? LastEventArgs => _eventArgs;



    /// <summary>
    /// Initializes a new instance of the <see cref="AvaGraphicInput"/> class.
    /// </summary>
    /// <param name="parent">The parent visual element.</param>
    public AvaGraphicInput(Visual parent)
    {
        _parent = parent ?? throw new ArgumentNullException(nameof(parent));
    }

    #region IGraphicInput

    /// <inheritdoc/>
    public GuiEventTypes EventType => _eventTypes;

    /// <inheritdoc/>
    public GuiMouseButtons MouseButton => _mouseButton;

    /// <inheritdoc/>
    public System.Drawing.Point? MouseLocation => _mouseLocationGetter?.Invoke();

    /// <inheritdoc/>
    public System.Drawing.Point LastMouseLocation => _lastMouseLocation;

    /// <inheritdoc/>
    public int MouseDelta => (int)((_eventArgs as PointerWheelEventArgs)?.Delta.Y ?? 0) * WheelDelta;

    /// <inheritdoc/>
    public bool AltKey => _keyModifierGetter?.Invoke().HasFlag(KeyModifiers.Alt) == true;

    /// <inheritdoc/>
    public bool ControlKey => _keyModifierGetter?.Invoke().HasFlag(KeyModifiers.Control) == true;

    /// <inheritdoc/>
    public bool ShiftKey => _keyModifierGetter?.Invoke().HasFlag(KeyModifiers.Shift) == true;

    /// <inheritdoc/>
    public string? KeyCode => _key != Key.None ? _key.ToString() : null;

    /// <inheritdoc/>
    public IDragEvent? DragEvent { get; internal set; }

    /// <inheritdoc/>
    public bool Handled
    {
        get => _handled;
        set
        {
            _handled = value;
            _eventArgs?.Handled = value;
        }
    }

    /// <inheritdoc/>
    public bool GetMouseButtonDown(GuiMouseButtons button)
    {
        if (MouseButton == button)
        {
            return true;
        }

        return false;
    }


    #endregion


    /// <summary>
    /// Clears the stored mouse location.
    /// </summary>
    public void ClearMouseLocation()
    {
        _mouseLocationGetter = null;
    }

    /// <summary>
    /// Sets the current event type and associated event arguments.
    /// </summary>
    /// <param name="eventType">The type of GUI event.</param>
    /// <param name="e">The optional routed event arguments.</param>
    public void SetEvent(GuiEventTypes eventType, RoutedEventArgs? e = null)
    {
        _eventTypes = eventType;
        _handled = false;

        _eventArgs = e;

        _mouseLocationGetter = null;
        _keyModifierGetter = null;

        _mouseButton = GuiMouseButtons.None;
        _key = Key.None;
    }

    /// <summary>
    /// Clears all event state and resets to None.
    /// </summary>
    public void Clear() => SetEvent(GuiEventTypes.None);

    /// <summary>
    /// Sets a refresh event to trigger input processing.
    /// </summary>
    public void SetRefreshEvent() => SetEvent(GuiEventTypes.Refresh);

    /// <summary>
    /// Sets a timer event, preserving the last known mouse location.
    /// </summary>
    public void SetTimerEvent()
    {
        SetEvent(GuiEventTypes.Timer);

        // Cannot directly get current mouse position, can only use last one
        _mouseLocationGetter = () => _lastMouseLocation;
    }

    /// <summary>
    /// Sets a pointer event (mouse/touch) and updates input state.
    /// </summary>
    /// <param name="eventType">The type of GUI event.</param>
    /// <param name="e">The pointer event arguments.</param>
    public void SetPointerEvent(GuiEventTypes eventType, PointerEventArgs e)
    {
        SetEvent(eventType, e);

        var p = e.GetPosition(_parent);
        _lastMouseLocation = new System.Drawing.Point((int)p.X, (int)p.Y);

        _mouseLocationGetter = () => _lastMouseLocation;
        _keyModifierGetter = () => e.KeyModifiers;

        // No need to get released keys.
        if (e is PointerReleasedEventArgs r)
        {
            switch (r.InitialPressMouseButton)
            {
                case Avalonia.Input.MouseButton.Left:
                    _mouseButton = GuiMouseButtons.Left;
                    break;

                case Avalonia.Input.MouseButton.Right:
                    _mouseButton = GuiMouseButtons.Right;
                    break;

                case Avalonia.Input.MouseButton.Middle:
                    _mouseButton = GuiMouseButtons.Middle;
                    break;

                default:
                    _mouseButton = GuiMouseButtons.None;
                    break;
            }
        }
        else
        {
            // Get pointer point properties relative to current control
            var properties = e.GetCurrentPoint(_parent).Properties;
            if (properties.IsLeftButtonPressed)
            {
                _mouseButton = GuiMouseButtons.Left;
            }
            else if (properties.IsRightButtonPressed)
            {
                _mouseButton = GuiMouseButtons.Right;
            }
            else if (properties.IsMiddleButtonPressed)
            {
                _mouseButton = GuiMouseButtons.Middle;
            }
            else
            {
                _mouseButton = GuiMouseButtons.None;
            }
        }
    }


    /// <summary>
    /// Sets a tapped event and updates input state.
    /// </summary>
    /// <param name="eventType">The type of GUI event.</param>
    /// <param name="e">The tapped event arguments.</param>
    public void SetTappedEvent(GuiEventTypes eventType, TappedEventArgs e)
    {
        SetEvent(eventType, e);

        var p = e.GetPosition(_parent);
        _lastMouseLocation = new System.Drawing.Point((int)p.X, (int)p.Y);
        _mouseLocationGetter = () => _lastMouseLocation;
        _keyModifierGetter = () => e.KeyModifiers;
    }

    /// <summary>
    /// Sets a key event and updates input state.
    /// </summary>
    /// <param name="eventType">The type of GUI event.</param>
    /// <param name="e">The key event arguments.</param>
    public void SetKeyEvent(GuiEventTypes eventType, KeyEventArgs e)
    {
        SetEvent(eventType, e);

        _keyModifierGetter = () => e.KeyModifiers;
        _key = e.Key;
    }

    /// <summary>
    /// Sets a resize event.
    /// </summary>
    public void SetResizeEvent() => SetEvent(GuiEventTypes.Resize);


    /// <summary>
    /// Sets a drag event and updates input state with drag data.
    /// </summary>
    /// <param name="eventType">The type of GUI event.</param>
    /// <param name="control">The target control.</param>
    /// <param name="e">The drag event arguments.</param>
    /// <returns>The global drag event instance.</returns>
    public AvaDragEvent SetDragEvent(GuiEventTypes eventType, Control control, DragEventArgs e)
    {
        SetEvent(eventType, e);

        e.DragEffects = DragDropEffects.None;

        var avaPoint = e.GetPosition(control);
        _lastMouseLocation = new System.Drawing.Point((int)avaPoint.X, (int)avaPoint.Y);
        _mouseLocationGetter = () => _lastMouseLocation;

        //Debug.WriteLine($"DropEvent: {DragEvent != null}");

        return AvaDragEvent.Global;
    }


    
}
