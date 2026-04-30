using System.Drawing;

namespace Suity.Views.Im;

/// <summary>
/// Helper methods for scroll bar calculations and positioning.
/// </summary>
public static class ScrollHelper
{
    /// <summary>
    /// Clamps the horizontal scroll bar position within the given rectangle.
    /// </summary>
    /// <param name="scrollBarRect">The scroll bar rectangle to clamp.</param>
    /// <param name="rect">The bounding rectangle.</param>
    /// <returns>True if the position was adjusted, false otherwise.</returns>
    public static bool ClampSliderPositionH(ref RectangleF scrollBarRect, RectangleF rect)
    {
        if (scrollBarRect.X < rect.X)
        {
            scrollBarRect.X = rect.X;
            return true;
        }

        if (scrollBarRect.Right > rect.Right)
        {
            scrollBarRect.X = rect.Right - scrollBarRect.Width;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Clamps the vertical scroll bar position within the given rectangle.
    /// </summary>
    /// <param name="scrollBarRect">The scroll bar rectangle to clamp.</param>
    /// <param name="rect">The bounding rectangle.</param>
    /// <returns>True if the position was adjusted, false otherwise.</returns>
    public static bool ClampSliderPositionV(ref RectangleF scrollBarRect, RectangleF rect)
    {
        if (scrollBarRect.Y < rect.Y)
        {
            scrollBarRect.Y = rect.Y;
            return true;
        }

        if (scrollBarRect.Bottom > rect.Bottom)
        {
            scrollBarRect.Y = rect.Bottom - scrollBarRect.Height;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Calculates the horizontal scroll rate based on the slider position.
    /// </summary>
    /// <param name="rect">The track rectangle.</param>
    /// <param name="sRect">The slider rectangle.</param>
    /// <returns>A value between 0 and 1 representing the scroll position.</returns>
    public static float GetSliderRateH(RectangleF rect, RectangleF sRect)
    {
        float a = sRect.X - rect.X;
        float b = rect.Width - sRect.Width;

        if (b > 0)
        {
            return a / b;
        }
        else
        {
            return 0;
        }
    }

    /// <summary>
    /// Calculates the vertical scroll rate based on the slider position.
    /// </summary>
    /// <param name="rect">The track rectangle.</param>
    /// <param name="sRect">The slider rectangle.</param>
    /// <returns>A value between 0 and 1 representing the scroll position.</returns>
    public static float GetSliderRateV(RectangleF rect, RectangleF sRect)
    {
        float a = sRect.Y - rect.Y;
        float b = rect.Height - sRect.Height;

        if (b > 0)
        {
            return a / b;
        }
        else
        {
            return 0;
        }
    }
}