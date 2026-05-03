using Suity.Collections;
using Suity.Drawing;
using Suity.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Views.Im;

/// <summary>
/// Stores an image reference for use in ImGui nodes.
/// </summary>
public class GuiImageValue
{
    /// <summary>
    /// The image to display.
    /// </summary>
    public ImageDef? Image { get; set; }
}

/// <summary>
/// Stores expandable state information for ImGui nodes.
/// </summary>
public class GuiExpandableValue
{
    /// <summary>
    /// The expanded width, if specified.
    /// </summary>
    public GuiLength? ExpandedWidth { get; set; }

    /// <summary>
    /// The expanded height, if specified.
    /// </summary>
    public GuiLength? ExpandedHeight { get; set; }

    /// <summary>
    /// Whether the node is currently expanded.
    /// </summary>
    public bool Expanded { get; set; }
}

/// <summary>
/// Stores scrollable state information for ImGui nodes.
/// </summary>
public class GuiScrollableValue
{
    /// <summary>
    /// The scroll orientation.
    /// </summary>
    public GuiOrientation ScrollOrientation { get; set; }

    /// <summary>
    /// Whether the horizontal scroll bar is visible.
    /// </summary>
    public bool HScrollBarVisible { get; set; }

    /// <summary>
    /// Whether the vertical scroll bar is visible.
    /// </summary>
    public bool VScrollBarVisible { get; set; }

    /// <summary>
    /// The current horizontal scroll offset.
    /// </summary>
    public float ScrollX { get; set; }

    /// <summary>
    /// The current vertical scroll offset.
    /// </summary>
    public float ScrollY { get; set; }

    /// <summary>
    /// The mouse down position for horizontal scroll bar dragging.
    /// </summary>
    public float? HMouseDownPos { get; set; }

    /// <summary>
    /// The mouse down position for vertical scroll bar dragging.
    /// </summary>
    public float? VMouseDownPos { get; set; }

    /// <summary>
    /// The current scroll bar rectangle.
    /// </summary>
    public RectangleF CurrentScrollBarRect { get; set; }

    /// <summary>
    /// The size of the scrollable content.
    /// </summary>
    public SizeF ContentSize { get; set; }

    /// <summary>
    /// Optional padding applied to the scrollable rectangle.
    /// </summary>
    public GuiThickness? RectPadding { get; set; }

    /// <summary>
    /// Whether scroll animations are enabled.
    /// </summary>
    public bool Animation { get; set; }

    /// <summary>
    /// Whether manual scroll input is enabled.
    /// </summary>
    public bool ManualInput { get; set; }

    /// <summary>
    /// Whether to synchronize with the GUI system.
    /// </summary>
    public bool SyncGui { get; set; } = true;

    /// <summary>
    /// Gets the view rectangle after applying padding.
    /// </summary>
    /// <param name="rect">The base rectangle.</param>
    /// <param name="scale">Optional scale factor.</param>
    /// <returns>The shrunk rectangle.</returns>
    public RectangleF GetViewRect(RectangleF rect, float? scale = null)
    {
        if (RectPadding is { } padding)
        {
            return padding.Shrink(rect, scale);
        }
        return rect;
    }
}

/// <summary>
/// Stores optional active state for a group of nodes.
/// </summary>
public class GuiOptionalValue
{
    /// <summary>
    /// The ID of the currently active node.
    /// </summary>
    public string? ActiveNodeId { get; set; }
}

/// <summary>
/// Represents an item in a dropdown list.
/// </summary>
public struct GuiDropDownItem
{
    /// <summary>
    /// The underlying value of the item.
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// The string representation of the value.
    /// </summary>
    public string? ToStringValue { get; }

    /// <summary>
    /// The display text for the item, if different from the value's string representation.
    /// </summary>
    public string? DisplayText { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GuiDropDownItem"/> struct.
    /// </summary>
    /// <param name="value">The underlying value.</param>
    /// <param name="displayText">Optional display text.</param>
    public GuiDropDownItem(object value, string? displayText = null)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
        ToStringValue = value.ToString();
        
        if (!string.IsNullOrWhiteSpace(displayText))
        {
            DisplayText = displayText;
        }
        else
        {
            DisplayText = null;
        }
    }

    /// <inheritdoc/>
    public override readonly string ToString()
    {
        return L(DisplayText) ?? Value?.ToString() ?? string.Empty;
    }

    /// <inheritdoc/>
    public override readonly int GetHashCode()
    {
        return Value?.GetHashCode() ?? 0;
    }

    /// <inheritdoc/>
    public override readonly bool Equals(object obj)
    {
        if (obj is GuiDropDownItem other)
        {
            return Equals(Value, other.Value);
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Determines whether two dropdown items are equal by comparing their values.
    /// </summary>
    public static bool operator ==(GuiDropDownItem v1, GuiDropDownItem v2)
    {
        return Equals(v1.Value, v2.Value);
    }

    /// <summary>
    /// Determines whether two dropdown items are not equal.
    /// </summary>
    public static bool operator !=(GuiDropDownItem v1, GuiDropDownItem v2)
    {
        return !Equals(v1.Value, v2.Value);
    }
}

/// <summary>
/// Stores dropdown list state including items and selection.
/// </summary>
public class GuiDropDownValue
{
    GuiDropDownItem? _selectedItem;

    /// <summary>
    /// The list of dropdown items.
    /// </summary>
    public List<GuiDropDownItem> Items { get; } = [];

    /// <summary>
    /// Gets or sets the currently selected item.
    /// </summary>
    public GuiDropDownItem? SelectedItem
    {
        get => _selectedItem;
        set => _selectedItem = value;
    }

    /// <summary>
    /// Gets or sets the value of the currently selected item.
    /// </summary>
    public object? SelectedValue
    {
        get => SelectedItem?.Value;
        set
        {
            if (value != null)
            {
                var item = Items.FirstOrDefault(o => Equals(o.Value, value));
                if (item.Value is null && value is string s)
                {
                    item = Items.FirstOrDefault(o => o.ToStringValue == s);
                }
                if (item.Value != null)
                {
                    SelectedItem = item;
                }
                else
                {
                    SelectedItem = null;
                }
            }
            else
            {
                SelectedItem = null;
            }
        }
    }

    /// <summary>
    /// The height of the dropdown list, if specified.
    /// </summary>
    public float? DropDownHeight { get; set; }

    /// <summary>
    /// An optional tag for storing arbitrary data.
    /// </summary>
    public object? Tag { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GuiDropDownValue"/> class.
    /// </summary>
    public GuiDropDownValue()
    {
    }

    /// <summary>
    /// Initializes a new instance with the specified items.
    /// </summary>
    public GuiDropDownValue(params GuiDropDownItem[] items)
    {
        Items.AddRange(items);
    }

    /// <summary>
    /// Initializes a new instance with the specified values, creating items from them.
    /// </summary>
    public GuiDropDownValue(params object[] values)
    {
        Items.AddRange(values.Select(o => new GuiDropDownItem(o, null)));
    }

    /// <summary>
    /// Adds a new item to the dropdown list.
    /// </summary>
    /// <param name="value">The item value.</param>
    /// <param name="displayText">The display text.</param>
    /// <param name="selected">Whether this item should be selected.</param>
    /// <returns>The created item.</returns>
    public GuiDropDownItem AddItem(object value, string displayText, bool selected = false)
    {
        var item = new GuiDropDownItem(value, displayText);
        Items.Add(item);
        if (selected)
        {
            SelectedItem = item;
        }
        return item;
    }

    /// <summary>
    /// Adds multiple items to the dropdown list.
    /// </summary>
    /// <param name="items">The collection of items to add.</param>
    public void AddItems(IEnumerable<GuiDropDownItem> items)
    {
        Items.AddRange(items);
    }

    /// <summary>
    /// Adds items created from the specified values.
    /// </summary>
    /// <param name="objs">The collection of values to create items from.</param>
    public void AddValues(IEnumerable<object> objs)
    {
        Items.AddRange(objs.Select(o => new GuiDropDownItem(o, null)));
    }

    /// <summary>
    /// Sets up the dropdown with items from an enum type.
    /// </summary>
    /// <param name="enumType">The enum type.</param>
    public void SetupEnumType(Type enumType)
    {
        if (!enumType.IsEnum)
        {
            throw new InvalidOperationException();
        }
        Clear();
        var items = _enumTypes.GetOrAdd(enumType, _SetupEnumType);
        AddItems(items);
    }

    /// <summary>
    /// Clears all items and selection.
    /// </summary>
    public void Clear()
    {
        Items.Clear();
        SelectedItem = null;
    }

    static readonly Dictionary<Type, GuiDropDownItem[]> _enumTypes = [];
    static GuiDropDownItem[] _SetupEnumType(Type enumType)
    {
        List<GuiDropDownItem> list = [];
        foreach (var field in enumType.GetFields().Where(o => o.FieldType == enumType))
        {
            var displayName = field.Name;
            var displayNameAttr = field.GetAttributeCached<DisplayTextAttribute>();
            if (displayNameAttr != null && !string.IsNullOrWhiteSpace(displayNameAttr.DisplayText))
            {
                displayName = displayNameAttr.DisplayText;
            }
            object enumValue = Enum.Parse(enumType, field.Name);
            var item = new GuiDropDownItem(enumValue, displayName);
            list.Add(item);
        }
        return [.. list];
    }

    /// <summary>
    /// Creates a GuiDropDownValue populated with items from the specified enum type.
    /// </summary>
    public static GuiDropDownValue FromEnumType<TEnum>() where TEnum : struct, Enum
    {
        var value = new GuiDropDownValue();
        value.SetupEnumType(typeof(TEnum));
        return value;
    }
}

/// <summary>
/// Stores resizer state for draggable resize handles.
/// </summary>
public class GuiResizerValue
{
    /// <summary>
    /// The resize orientation.
    /// </summary>
    public GuiOrientation Orientation { get; set; }

    /// <summary>
    /// The minimum allowed position.
    /// </summary>
    public float? MinPosition { get; set; }

    /// <summary>
    /// The maximum allowed position.
    /// </summary>
    public float? MaxPosition { get; set; }

    /// <summary>
    /// The mouse down position when resizing started.
    /// </summary>
    public PointF? MouseDownPos { get; set; }

    /// <summary>
    /// The size of the content being resized.
    /// </summary>
    public SizeF ContentSize { get; set; }

    /// <summary>
    /// Whether resizing should affect the sibling node.
    /// </summary>
    public bool AffectSibling { get; set; }

    /// <summary>
    /// The size of the next sibling content.
    /// </summary>
    public SizeF NextContentSize { get; set; }

    /// <summary>
    /// Clamps a position value to the min/max range.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="limited">Whether the value was limited by min/max bounds.</param>
    /// <returns>The clamped value.</returns>
    public float GetPositionMinMax(float value, out bool limited)
    {
        limited = false;
        if (MinPosition is { } min && value < min)
        {
            value = min;
            limited = true;
        }
        if (MaxPosition is { } max && max > 0 && value > max)
        {
            value = max;
            limited = true;
        }
        if (value < 0)
        {
            value = 0;
            limited = true;
        }
        return value;
    }
}

/// <summary>
/// Drag and drop value
/// </summary>
public class GuiDraggableValue
{
    /// <summary>
    /// Whether it is a drag request, when reading this value as true, it is necessary to set this value to false and trigger a drag action request to the system.
    /// </summary>
    public bool DragRequest { get; set; }
}

/// <summary>
/// Represents the state of a checkbox or toggle control.
/// </summary>
public enum CheckState
{
    /// <summary>
    /// The control is unchecked.
    /// </summary>
    Unchecked,

    /// <summary>
    /// The control is checked.
    /// </summary>
    Checked,

    /// <summary>
    /// The control is in an indeterminate state.
    /// </summary>
    Indeterminate,
}

/// <summary>
/// Stores toggle/checkbox state for ImGui nodes.
/// </summary>
public class GuiToggleValue
{
    /// <summary>
    /// The current check state.
    /// </summary>
    public CheckState Value { get; set; } = CheckState.Unchecked;

    /// <summary>
    /// Gets whether the value is checked.
    /// </summary>
    public bool IsChecked => Value == CheckState.Checked;
}

/// <summary>
/// Stores tooltip information for ImGui nodes.
/// </summary>
public class GuiToolTipValue
{
    /// <summary>
    /// The tooltip text.
    /// </summary>
    public string? ToolTipText { get; set; }

    /// <summary>
    /// A function that returns the tooltip text dynamically.
    /// </summary>
    public Func<string?>? ToolTipGetter { get; set; }
}

/// <summary>
/// Stores position information for ImGui nodes.
/// </summary>
public class GuiPositionValue
{
    /// <summary>
    /// The position coordinates.
    /// </summary>
    public PointF Position { get; set; }
}

/// <summary>
/// Stores viewport state for zoomable and pannable views.
/// </summary>
public class GuiViewportValue
{
    private PointF _viewportPosition;
    private PointF? _panOffset;
    private float _zoom = 1f;
    private readonly HashSet<ImGuiNode> _cachedInBoundNodes = [];
    private readonly List<ImGuiNode> _cachedInOrder = [];

    /// <summary>
    /// Gets or sets the viewport position, including any active pan offset.
    /// </summary>
    public PointF ViewportPosition
    {
        get
        {
            if (_panOffset is { } panOffset)
            {
                return new PointF(_viewportPosition.X + panOffset.X, _viewportPosition.Y + panOffset.Y);
            }
            else
            {
                return _viewportPosition;
            }
        }
        set
        {
            _viewportPosition = value;
            _panOffset = null;
        }
    }

    /// <summary>
    /// Gets or sets the current zoom level.
    /// </summary>
    public float Zoom
    {
        get => _zoom;
        set => _zoom = value;
    }

    /// <summary>
    /// The minimum allowed zoom level.
    /// </summary>
    public float ZoomMin { get; set; } = 0.2f;

    /// <summary>
    /// The maximum allowed zoom level.
    /// </summary>
    public float ZoomMax { get; set; } = 2f;

    /// <summary>
    /// The zoom factor applied per zoom step.
    /// </summary>
    public float ZoomFactor { get; set; } = 1.5f;

    /// <summary>
    /// The mouse down position when panning started.
    /// </summary>
    public Point? MouseDownPosition { get; set; }

    /// <summary>
    /// Zooms in by the zoom factor.
    /// </summary>
    /// <returns>True if the zoom level changed.</returns>
    public bool ZoomIn()
    {
        float zoom = Math.Min(ZoomMax, _zoom * ZoomFactor);
        if (zoom != _zoom)
        {
            _zoom = zoom;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Zooms out by the zoom factor.
    /// </summary>
    /// <returns>True if the zoom level changed.</returns>
    public bool ZoomOut()
    {
        float zoom = Math.Max(ZoomMin, _zoom / ZoomFactor);
        if (zoom != _zoom)
        {
            _zoom = zoom;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Sets a pan offset that is applied on top of the viewport position.
    /// </summary>
    /// <param name="x">The X offset.</param>
    /// <param name="y">The Y offset.</param>
    public void SetPan(float x, float y)
    {
        _panOffset = new PointF(x / _zoom, y / _zoom);
    }

    /// <summary>
    /// Commits the current pan offset to the viewport position and clears the offset.
    /// </summary>
    public void UnsetPan()
    {
        if (_panOffset is { } panOffset)
        {
            _viewportPosition.X += panOffset.X;
            _viewportPosition.Y += panOffset.Y;
            _panOffset = null;
        }
    }

    /// <summary>
    /// Sorts the cached node order by index.
    /// </summary>
    public void ApplyOrder()
    {
        _cachedInOrder.Sort((a, b) => a.Index.CompareTo(b.Index));
    }

    /// <summary>
    /// Applies the viewport transformation to a viewport node and updates in-bound child nodes.
    /// </summary>
    /// <param name="viewport">The viewport node.</param>
    public void ApplyViewportNode(ImGuiNode viewport)
    {
        var pInnerRect = viewport.InnerRect;
        GuiTransform trans = GetTransform(pInnerRect);
        viewport.Transform = trans;
        _cachedInBoundNodes.Clear();
        _cachedInOrder.Clear();
        foreach (var childNode in viewport.ChildNodes)
        {
            var childRect = viewport.TransformChildRect(childNode.Rect);
            bool inBound = childRect.IntersectsWith(pInnerRect);
            childNode.IsOutOfBound = !inBound;
            if (inBound)
            {
                if (_cachedInBoundNodes.Add(childNode))
                {
                    _cachedInOrder.InsertSorted(childNode, (a, b) => a.Index.CompareTo(b.Index));
                }
            }
        }
        _cachedInOrder.Sort((a, b) => a.Index.CompareTo(b.Index));
    }

    /// <summary>
    /// Updates the in-bound status of a child node.
    /// </summary>
    /// <param name="childNode">The child node to update.</param>
    public void ApplyChildNode(ImGuiNode childNode)
    {
        var node = childNode.Parent!;
        var pInnerRect = node.InnerRect;
        var childRect = node.TransformChildRect(childNode.Rect);
        bool inBound = childRect.IntersectsWith(pInnerRect);
        childNode.IsOutOfBound = !inBound;
        if (inBound)
        {
            if (_cachedInBoundNodes.Add(childNode))
            {
                _cachedInOrder.InsertSorted(childNode, (a, b) => a.Index.CompareTo(b.Index));
            }
        }
        else
        {
            if (_cachedInBoundNodes.Remove(childNode))
            {
                _cachedInOrder.Remove(childNode);
            }
        }
    }

    /// <summary>
    /// Gets the in-bound child nodes in order.
    /// </summary>
    public IEnumerable<ImGuiNode> InBoundNodes => _cachedInOrder;

    /// <summary>
    /// Gets the in-bound child nodes in reverse order.
    /// </summary>
    public IEnumerable<ImGuiNode> RevertInBoundNodes => _cachedInOrder.ReverseEnumerable();

    private GuiTransform GetTransform(RectangleF rect)
    {
        var viewPos = ViewportPosition;
        var centerSize = new PointF(rect.Width * 0.5f, rect.Height * 0.5f);
        var trans = new GuiTransform(viewPos, Zoom, centerSize);
        return trans;
    }
}
