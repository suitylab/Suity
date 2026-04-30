using System;
using System.Drawing;

namespace Suity.Views.Im;

/// <summary>
/// Represents a simple layout position with a point coordinate.
/// </summary>
public class GuiLayoutPosition
{
    /// <summary>
    /// The position coordinates.
    /// </summary>
    public PointF Position;
}

/// <summary>
/// Represents the current state of mouse interaction with a node.
/// </summary>
public enum GuiMouseState
{
    /// <summary>
    /// No mouse interaction.
    /// </summary>
    None,

    /// <summary>
    /// Mouse is hovering over the node.
    /// </summary>
    Hover,

    /// <summary>
    /// Mouse button is pressed on the node.
    /// </summary>
    Pressed,

    /// <summary>
    /// Mouse click has been completed on the node.
    /// </summary>
    Clicked,
}

/// <summary>
/// Gui input status
/// </summary>
public enum GuiInputState
{
    /// <summary>
    /// No action required.
    /// </summary>
    None = 0,

    /// <summary>
    /// Used for continuous updates in animation.
    /// </summary>
    KeepListening,

    /// <summary>
    /// Only perform rendering.
    /// </summary>
    Render,

    /// <summary>
    /// Only perform layout.
    /// </summary>
    Layout,

    /// <summary>
    /// Local synchronization update, when OnFullSyncContent() is executed, it will be ignored (except for initialization).
    /// </summary>
    PartialSync,

    /// <summary>
    /// Fully synchronized updates.
    /// </summary>
    FullSync,
}

/// <summary>
/// Represents the render state of a node.
/// </summary>
public enum GuiRenderState
{
    /// <summary>
    /// No render state.
    /// </summary>
    Nonde = 0,

    /// <summary>
    /// Rendering is blocked.
    /// </summary>
    Blocked,
}

/// <summary>
/// Represents the various pipeline stages for ImGui operations.
/// </summary>
[Flags]
public enum GuiPipeline
{
    /// <summary>
    /// No pipeline stage.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// Main pipeline stage.
    /// </summary>
    Main = 0x1,

    /// <summary>
    /// Pre-action pipeline stage.
    /// </summary>
    PreAction = 0x2,

    /// <summary>
    /// Post-action pipeline stage.
    /// </summary>
    PostAction = 0x4,

    /// <summary>
    /// Blocked pipeline stage.
    /// </summary>
    Blocked = 0x8,

    /// <summary>
    /// Beginning of drag operation.
    /// </summary>
    BeginDrag = 0x10,

    /// <summary>
    /// Dragging in progress.
    /// </summary>
    Dragging = 0x20,

    /// <summary>
    /// End of drag operation.
    /// </summary>
    EndDrag = 0x40,

    /// <summary>
    /// Beginning of edit operation.
    /// </summary>
    BeginEdit = 0x80,

    /// <summary>
    /// Beginning of synchronization.
    /// </summary>
    BeginSync = 0x100,

    /// <summary>
    /// Alignment stage in layout pipeline.
    /// </summary>
    Align = 0x1000,

    /// <summary>
    /// Background rendering stage.
    /// </summary>
    Background = 0x10000,

    /// <summary>
    /// Text rendering stage.
    /// </summary>
    Text = 0x20000,

    /// <summary>
    /// Header rendering stage.
    /// </summary>
    Header = 0x40000,
}

/// <summary>
/// Length mode
/// </summary>
public enum GuiLengthMode
{
    /// <summary>
    /// Fixed length, without considering the scaling of the parent level.
    /// </summary>
    Fixed,

    /// <summary>
    /// Fixed length, considering the scaling of the parent level.
    /// </summary>
    ScaledFixed,

    /// <summary>
    /// Subtract a fixed value from the entire length.
    /// </summary>
    FullExcept,

    /// <summary>
    /// Subtract a fixed value from the remaining length.
    /// </summary>
    RestExcept,

    /// <summary>
    /// Percentage of full length.
    /// </summary>
    Percentage,

    /// <summary>
    /// Remaining percentage.
    /// </summary>
    RestPercentage,

    /// <summary>
    /// Adaptive sizing based on content.
    /// </summary>
    Adapt,
}

/// <summary>
/// Represents a directional orientation for layout and fitting operations.
/// </summary>
public enum GuiOrientation
{
    /// <summary>
    /// No orientation specified.
    /// </summary>
    None,

    /// <summary>
    /// Horizontal orientation.
    /// </summary>
    Horizontal,

    /// <summary>
    /// Vertical orientation.
    /// </summary>
    Vertical,

    /// <summary>
    /// Both horizontal and vertical orientations.
    /// </summary>
    Both,
}

/// <summary>
/// Represents a length value with a specific mode of calculation.
/// </summary>
public readonly struct GuiLength
{
    /// <summary>
    /// Gets a GuiLength that fills the entire available space.
    /// </summary>
    public static GuiLength FullLength { get; } = new GuiLength(0, GuiLengthMode.FullExcept);

    /// <summary>
    /// Gets a GuiLength that adapts to content size.
    /// </summary>
    public static GuiLength Adapt { get; } = new GuiLength(0, GuiLengthMode.Adapt);

    /// <summary>
    /// The numeric value of the length.
    /// </summary>
    public readonly float Value;

    /// <summary>
    /// The mode used to calculate the length.
    /// </summary>
    public readonly GuiLengthMode Mode;

    /// <summary>
    /// Initializes a new instance of the <see cref="GuiLength"/> struct with a fixed value.
    /// </summary>
    /// <param name="value">The fixed length value.</param>
    public GuiLength(float value)
    {
        Value = value;
        Mode = GuiLengthMode.Fixed;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GuiLength"/> struct with a value and mode.
    /// </summary>
    /// <param name="value">The length value.</param>
    /// <param name="mode">The calculation mode.</param>
    public GuiLength(float value, GuiLengthMode mode)
    {
        Value = value;
        Mode = mode;
    }

    /// <summary>
    /// Calculates the actual length based on the available space and current position.
    /// </summary>
    /// <param name="fullValue">The total available length.</param>
    /// <param name="position">The current position within the available space.</param>
    /// <param name="scale">Optional scale factor.</param>
    /// <returns>The calculated length, or null if adaptive.</returns>
    public float? GetValue(float fullValue, float position, float? scale = null)
    {
        float value;

        switch (Mode)
        {
            case GuiLengthMode.FullExcept:
                value = fullValue - Value;
                break;

            case GuiLengthMode.RestExcept:
                value = fullValue - position - Value;
                break;

            case GuiLengthMode.Percentage:
                value = fullValue * (Value * 0.01f);
                break;

            case GuiLengthMode.RestPercentage:
                value = (fullValue - position) * (Value * 0.01f);
                break;

            case GuiLengthMode.Adapt:
                return null;

            case GuiLengthMode.ScaledFixed:
                value = Value;
                break;

            case GuiLengthMode.Fixed:
            default:
                return Value > 0 ? Value : 0;
        }

        if (value < 0)
        {
            value = 0;
        }

        if (scale is { } vScale && vScale != 1f && vScale > 0)
        {
            value /= vScale;
        }

        return value;
    }

    /// <summary>
    /// Calculates the actual length with min/max constraints applied.
    /// </summary>
    /// <param name="fullValue">The total available length.</param>
    /// <param name="position">The current position within the available space.</param>
    /// <param name="scale">Optional scale factor.</param>
    /// <param name="minMaxValue">The min/max constraints.</param>
    /// <returns>The calculated length with constraints applied, or null if adaptive.</returns>
    public float? GetValue(float fullValue, float position, float? scale, GuiMinMaxValue minMaxValue)
    {
        var value = GetValue(fullValue, position, scale);
        if (!value.HasValue)
        {
            return null;
        }

        if (minMaxValue.MinValue.HasValue)
        {
            value = Math.Max(minMaxValue.MinValue.Value, value.Value);
        }
        if (minMaxValue.MaxValue.HasValue)
        {
            value = Math.Min(minMaxValue.MaxValue.Value, value.Value);
        }

        return value;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Mode} {Value}";
    }

    /// <summary>
    /// Implicitly converts a double to a GuiLength with fixed mode.
    /// </summary>
    public static implicit operator GuiLength(double value)
    {
        return new GuiLength((float)value);
    }

    /// <summary>
    /// Implicitly converts a float to a GuiLength with fixed mode.
    /// </summary>
    public static implicit operator GuiLength(float value)
    {
        return new GuiLength(value);
    }

    /// <summary>
    /// Implicitly converts an int to a GuiLength with fixed mode.
    /// </summary>
    public static implicit operator GuiLength(int value)
    {
        return new GuiLength(value);
    }

    /// <summary>
    /// Adds an integer value to a GuiLength, preserving the mode.
    /// </summary>
    public static GuiLength operator +(GuiLength length, int value)
    {
        return new GuiLength(length.Value + value, length.Mode);
    }

    /// <summary>
    /// Adds a float value to a GuiLength, preserving the mode.
    /// </summary>
    public static GuiLength operator +(GuiLength length, float value)
    {
        return new GuiLength(length.Value + value, length.Mode);
    }

    /// <summary>
    /// Adds a double value to a GuiLength, preserving the mode.
    /// </summary>
    public static GuiLength operator +(GuiLength length, double value)
    {
        return new GuiLength(length.Value + (float)value, length.Mode);
    }
}

/// <summary>
/// Represents a minimum and maximum value constraint pair.
/// </summary>
/// <param name="minValue">The minimum allowed value.</param>
/// <param name="maxValue">The maximum allowed value.</param>
public readonly struct GuiMinMaxValue(float minValue, float maxValue)
{
    /// <summary>
    /// The minimum allowed value.
    /// </summary>
    public readonly float? MinValue = minValue;

    /// <summary>
    /// The maximum allowed value, guaranteed to be at least equal to MinValue.
    /// </summary>
    public readonly float? MaxValue = Math.Max(minValue, maxValue);
}

/// <summary>
/// Represents the four sides of a rectangle or element.
/// </summary>
public enum GuiSides
{
    /// <summary>
    /// The top side.
    /// </summary>
    Top,

    /// <summary>
    /// The bottom side.
    /// </summary>
    Bottom,

    /// <summary>
    /// The left side.
    /// </summary>
    Left,

    /// <summary>
    /// The right side.
    /// </summary>
    Right,
}

/// <summary>
/// Represents a thickness value for all four sides of a rectangle.
/// </summary>
public struct GuiThickness
{
    /// <summary>
    /// The top thickness value.
    /// </summary>
    public float Top;

    /// <summary>
    /// The bottom thickness value.
    /// </summary>
    public float Bottom;

    /// <summary>
    /// The left thickness value.
    /// </summary>
    public float Left;

    /// <summary>
    /// The right thickness value.
    /// </summary>
    public float Right;

    /// <summary>
    /// Initializes a new instance of the <see cref="GuiThickness"/> struct with zero values.
    /// </summary>
    public GuiThickness()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GuiThickness"/> struct with equal values on all sides.
    /// </summary>
    /// <param name="value">The thickness value for all sides.</param>
    public GuiThickness(float value)
    {
        Top = Bottom = Left = Right = value;
    }

    /// <summary>
    /// Gets the thickness value for the specified side.
    /// </summary>
    /// <param name="side">The side to retrieve.</param>
    /// <returns>The thickness value for the specified side.</returns>
    public readonly float GetValue(GuiSides side) => side switch
    {
        GuiSides.Top => Top,
        GuiSides.Bottom => Bottom,
        GuiSides.Left => Left,
        GuiSides.Right => Right,
        _ => 0,
    };

    /// <summary>
    /// Shrinks a rectangle by the thickness values.
    /// </summary>
    /// <param name="rect">The rectangle to shrink.</param>
    /// <param name="scale">Optional scale factor.</param>
    /// <returns>The shrunk rectangle.</returns>
    public readonly RectangleF Shrink(RectangleF rect, float? scale = null)
    {
        if (scale is { } s && s != 1f)
        {
            rect.X += Left * s;
            rect.Y += Top * s;
            rect.Width -= (Left + Right) * s;
            rect.Height -= (Top + Bottom) * s;
        }
        else
        {
            rect.X += Left;
            rect.Y += Top;
            rect.Width -= Left + Right;
            rect.Height -= Top + Bottom;
        }

        if (rect.Width < 0)
        {
            rect.Width = 0;
        }

        if (rect.Height < 0)
        {
            rect.Height = 0;
        }

        return rect;
    }

    /// <summary>
    /// Expands a rectangle by the thickness values.
    /// </summary>
    /// <param name="rect">The rectangle to expand.</param>
    /// <param name="scale">Optional scale factor.</param>
    /// <returns>The expanded rectangle.</returns>
    public readonly RectangleF Expand(RectangleF rect, float? scale = null)
    {
        if (scale is { } s && s != 1f)
        {
            rect.X -= Left * s;
            rect.Y -= Top * s;
            rect.Width += (Left + Right) * s;
            rect.Height += (Top + Bottom) * s;
        }
        else
        {
            rect.X -= Left;
            rect.Y -= Top;
            rect.Width += Left + Right;
            rect.Height += Top + Bottom;
        }

        return rect;
    }

    /// <inheritdoc/>
    public override readonly string ToString()
    {
        return $"top:{Top} bottom:{Bottom} left:{Left} right:{Right}";
    }

    /// <summary>
    /// Implicitly converts a float to a GuiThickness with equal values on all sides.
    /// </summary>
    public static implicit operator GuiThickness(float value)
    {
        return new()
        {
            Top = value,
            Bottom = value,
            Left = value,
            Right = value,
        };
    }

    /// <summary>
    /// Implicitly converts an int to a GuiThickness with equal values on all sides.
    /// </summary>
    public static implicit operator GuiThickness(int value)
    {
        return new()
        {
            Top = value,
            Bottom = value,
            Left = value,
            Right = value,
        };
    }
}

/// <summary>
/// Represents a 2D transformation with scale and offset.
/// </summary>
public struct GuiTransform
{
    /// <summary>
    /// A zero transformation with no scale or offset.
    /// </summary>
    public static readonly GuiTransform Zero = new();

    /// <summary>
    /// The scale factor.
    /// </summary>
    public float Scale;

    /// <summary>
    /// The X offset.
    /// </summary>
    public float OffsetX;

    /// <summary>
    /// The Y offset.
    /// </summary>
    public float OffsetY;

    /// <summary>
    /// Initializes a new instance of the <see cref="GuiTransform"/> struct.
    /// </summary>
    /// <param name="offset">The offset coordinates.</param>
    /// <param name="scale">The scale factor.</param>
    /// <param name="center">Optional center point for the transformation.</param>
    public GuiTransform(PointF offset, float scale, PointF? center = null)
    {
        OffsetX = offset.X * scale;
        OffsetY = offset.Y * scale;
        Scale = scale;

        if (center is { } c)
        {
            OffsetX += c.X;
            OffsetY += c.Y;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GuiTransform"/> struct.
    /// </summary>
    /// <param name="offsetX">The X offset.</param>
    /// <param name="offsetY">The Y offset.</param>
    /// <param name="scale">The scale factor.</param>
    /// <param name="centerX">Optional center X coordinate.</param>
    /// <param name="centerY">Optional center Y coordinate.</param>
    public GuiTransform(float offsetX, float offsetY, float scale, float? centerX = null, float? centerY = null)
    {
        OffsetX = offsetX * scale;
        OffsetY = offsetY * scale;
        Scale = scale;

        if (centerX is { } cx)
        {
            OffsetX += cx;
        }

        if (centerY is { } cy)
        {
            OffsetY += cy;
        }
    }

    /// <summary>
    /// Combines this transformation with a parent transformation.
    /// </summary>
    /// <param name="parent">The parent transformation.</param>
    /// <returns>A new GuiTransform representing the combined transformation.</returns>
    public GuiTransform AddTransform(GuiTransform parent)
    {
        float ox = OffsetX * parent.Scale + parent.OffsetX;
        float oy = OffsetY * parent.Scale + parent.OffsetY;
        float scale = Scale * parent.Scale;

        var result = new GuiTransform
        {
            OffsetX = ox,
            OffsetY = oy,
            Scale = scale,
        };

        return result;
    }

    /// <summary>
    /// Transforms a point using this transformation.
    /// </summary>
    /// <param name="point">The point to transform.</param>
    /// <returns>The transformed point.</returns>
    public readonly PointF Transform(PointF point)
    {
        float x = point.X * Scale + OffsetX;
        float y = point.Y * Scale + OffsetY;

        return new PointF(x, y);
    }

    /// <summary>
    /// Transforms a rectangle using this transformation.
    /// </summary>
    /// <param name="rect">The rectangle to transform.</param>
    /// <returns>The transformed rectangle.</returns>
    public readonly RectangleF Transform(RectangleF rect)
    {
        var p = Transform(new PointF(rect.X, rect.Y));
        var size = new SizeF(rect.Width * Scale, rect.Height * Scale);

        return new RectangleF(p, size);
    }

    /// <summary>
    /// Reverts a point transformation.
    /// </summary>
    /// <param name="point">The point to revert.</param>
    /// <returns>The reverted point.</returns>
    public readonly PointF RevertTransform(PointF point)
    {
        float x = (point.X - OffsetX) / Scale;
        float y = (point.Y - OffsetY) / Scale;

        return new PointF(x, y);
    }

    /// <summary>
    /// Reverts a rectangle transformation.
    /// </summary>
    /// <param name="rect">The rectangle to revert.</param>
    /// <returns>The reverted rectangle.</returns>
    public readonly RectangleF RevertTransform(RectangleF rect)
    {
        var p = RevertTransform(new PointF(rect.X, rect.Y));
        var size = new SizeF(rect.Width / Scale, rect.Height / Scale);

        return new RectangleF(p, size);
    }

    /// <inheritdoc/>
    public override readonly string ToString()
    {
        return $"x:{OffsetX} y:{OffsetY} scale:{Scale}";
    }

    /// <inheritdoc/>
    public override readonly bool Equals(object obj)
    {
        if (obj is GuiTransform other)
        {
            return OffsetX == other.OffsetX && OffsetY == other.OffsetY && Scale == other.Scale;
        }
        return false;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked // Prevent overflow without throwing exceptions
        {
            int hash = 17; // Initialize a non-zero constant
            hash = hash * 31 + Scale.GetHashCode();
            hash = hash * 31 + OffsetX.GetHashCode();
            hash = hash * 31 + OffsetY.GetHashCode();
            return hash;
        }
    }

    /// <summary>
    /// Determines whether two transformations are equal.
    /// </summary>
    public static bool operator ==(GuiTransform a, GuiTransform b)
    {
        return a.Equals(b);
    }

    /// <summary>
    /// Determines whether two transformations are not equal.
    /// </summary>
    public static bool operator !=(GuiTransform a, GuiTransform b)
    {
        return !a.Equals(b);
    }
}

/// <summary>
/// Represents alignment options for positioning elements.
/// </summary>
public enum GuiAlignment
{
    /// <summary>
    /// Align to the near edge (top or left).
    /// </summary>
    Near,

    /// <summary>
    /// Align to the center.
    /// </summary>
    Center,

    /// <summary>
    /// Align to the far edge (bottom or right).
    /// </summary>
    Far,
}
