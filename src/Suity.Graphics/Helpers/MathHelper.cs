using System;
using System.Drawing;

namespace Suity.Helpers;

/// <summary>
/// Helper methods for mathematical operations and geometry.
/// </summary>
public static class MathHelper
{
    /// <summary>
    /// Clamps an integer value to a specified range.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <returns>The clamped value.</returns>
    public static int Clamp(this int value, int min, int max)
    {
        if (value < min)
        {
            return min;
        }

        if (value > max)
        {
            return max;
        }

        return value;
    }

    /// <summary>
    /// Clamps an integer value to a specified range by reference.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <returns>True if the value was modified.</returns>
    public static bool Clamp(ref int value, int min, int max)
    {
        if (value < min)
        {
            value = min;
            return true;
        }

        if (value > max)
        {
            value = max;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Clamps an integer value using getter and setter functions.
    /// </summary>
    /// <param name="getter">Function to get the current value.</param>
    /// <param name="setter">Action to set the new value.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <returns>True if the value was modified.</returns>
    public static bool Clamp(Func<int> getter, Action<int> setter, int min, int max)
    {
        int value = getter();

        if (value < min)
        {
            setter(min);
            return true;
        }

        if (value > max)
        {
            setter(max);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Clamps a float value to a specified range.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <returns>The clamped value.</returns>
    public static float Clamp(this float value, float min, float max)
    {
        if (value < min)
        {
            return min;
        }

        if (value > max)
        {
            return max;
        }

        return value;
    }

    /// <summary>
    /// Clamps a float value to a specified range by reference.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <returns>True if the value was modified.</returns>
    public static bool Clamp(ref float value, float min, float max)
    {
        if (value < min)
        {
            value = min;
            return true;
        }

        if (value > max)
        {
            value = max;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Clamps a float value using getter and setter functions.
    /// </summary>
    /// <param name="getter">Function to get the current value.</param>
    /// <param name="setter">Action to set the new value.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <returns>True if the value was modified.</returns>
    public static bool Clamp(Func<float> getter, Action<float> setter, float min, float max)
    {
        float value = getter();

        if (value < min)
        {
            setter(min);
            return true;
        }

        if (value > max)
        {
            setter(max);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Linearly interpolates between two float values.
    /// </summary>
    /// <param name="v1">The first value.</param>
    /// <param name="v2">The second value.</param>
    /// <param name="t">The interpolation factor (0-1).</param>
    /// <returns>The interpolated value.</returns>
    public static float Lerp(float v1, float v2, float t)
    {
        return v1 + (v2 - v1) * t;
    }

    /// <summary>
    /// Linearly interpolates between two integer values.
    /// </summary>
    /// <param name="v1">The first value.</param>
    /// <param name="v2">The second value.</param>
    /// <param name="t">The interpolation factor (0-1).</param>
    /// <returns>The interpolated value.</returns>
    public static int Lerp(int v1, int v2, float t)
    {
        return (int)(v1 + (v2 - v1) * t);
    }

    /// <summary>
    /// Converts a PointF to an integer Point.
    /// </summary>
    /// <param name="point">The point to convert.</param>
    /// <returns>The converted point.</returns>
    public static Point ToInt(this PointF point)
    {
        return new Point((int)point.X, (int)point.Y);
    }

    /// <summary>
    /// Converts a RectangleF to an integer Rectangle.
    /// </summary>
    /// <param name="rect">The rectangle to convert.</param>
    /// <returns>The converted rectangle.</returns>
    public static Rectangle ToInt(this RectangleF rect)
    {
        return new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
    }

    /// <summary>
    /// Scales a rectangle around its center.
    /// </summary>
    /// <param name="rect">The rectangle to scale.</param>
    /// <param name="scale">The scale factor.</param>
    /// <returns>The scaled rectangle.</returns>
    public static RectangleF Scale(this RectangleF rect, float scale)
    {
        // Obtain the central point
        float centerX = rect.X + rect.Width * 0.5f;
        float centerY = rect.Y + rect.Height * 0.5f;

        // Calculate the new width and height
        float newWidth = rect.Width * scale;
        float newHeight = rect.Height * scale;

        // Calculate the new upper left corner coordinates
        float newX = centerX - newWidth * 0.5f;
        float newY = centerY - newHeight * 0.5f;

        return new RectangleF(newX, newY, newWidth, newHeight);
    }

    /// <summary>
    /// Offsets a rectangle by expanding all sides equally.
    /// </summary>
    /// <param name="rect">The rectangle to offset.</param>
    /// <param name="value">The offset value.</param>
    /// <returns>The offset rectangle.</returns>
    public static RectangleF Offset(this RectangleF rect, float value)
    {
        float v2 = value * 2;

        rect.X -= value;
        rect.Y -= value;

        rect.Width += v2;
        rect.Height += v2;

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
    /// Offsets a rectangle by half the specified value on all sides.
    /// </summary>
    /// <param name="rect">The rectangle to offset.</param>
    /// <param name="value">The offset value.</param>
    /// <returns>The offset rectangle.</returns>
    public static RectangleF OffsetHalf(this RectangleF rect, float value)
    {
        float v2 = value * 0.5f;

        rect.X -= v2;
        rect.Y -= v2;

        rect.Width += value;
        rect.Height += value;

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

    //public static RectangleF Expand(this RectangleF rect, GuiThickness thickness)
    //{
    //    rect.X -= thickness.Left;
    //    rect.Y -= thickness.Top;

    //    rect.Width += thickness.Right + thickness.Left;
    //    rect.Height += thickness.Top + thickness.Bottom;

    //    if (rect.Width < 0)
    //    {
    //        rect.Width = 0;
    //    }

    //    if (rect.Height < 0)
    //    {
    //        rect.Height = 0;
    //    }

    //    return rect;
    //}

    //public static RectangleF Shrink(this RectangleF rect, GuiThickness thickness)
    //{
    //    rect.X += thickness.Left;
    //    rect.Y += thickness.Top;

    //    rect.Width -= thickness.Right + thickness.Left;
    //    rect.Height -= thickness.Top + thickness.Bottom;

    //    if (rect.Width < 0)
    //    {
    //        rect.Width = 0;
    //    }

    //    if (rect.Height < 0)
    //    {
    //        rect.Height = 0;
    //    }

    //    return rect;
    //}
}