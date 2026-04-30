using System.Collections.Generic;
using System.Drawing;

namespace Suity.Helpers;

/// <summary>
/// Provides utilities for placing non-overlapping rectangles on a 2D canvas.
/// Uses a simple grid-based algorithm to find suitable gaps between existing rectangles.
/// </summary>
public static class RectanglePlacer
{
    /// <summary>
    /// Finds a suitable position to place a new rectangle without overlapping existing ones.
    /// </summary>
    /// <param name="newRectSize">The size of the new rectangle to place.</param>
    /// <param name="rects">The list of existing rectangles on the canvas.</param>
    /// <param name="canvasMaxWidth">The maximum width of the canvas.</param>
    /// <param name="spacing">The minimum spacing to maintain between rectangles.</param>
    /// <returns>A point representing the top-left position where the new rectangle can be placed.</returns>
    public static Point PlaceRectangle(Size newRectSize, List<Rectangle> rects, int canvasMaxWidth, int spacing)
    {
        // First try to find a suitable gap between existing rectangles
        for (int y = 0; y <= GetMaxY(rects) + newRectSize.Height; y += spacing)
        {
            for (int x = 0; x <= canvasMaxWidth - newRectSize.Width; x += spacing)
            {
                var newRect = new Rectangle(x, y, newRectSize.Width, newRectSize.Height);

                if (!IsOverlapping(newRect, rects, spacing))
                {
                    return new Point(x, y);
                }
            }
        }

        // If no suitable gap is found, place according to rules to the right or down
        int newX = 0;
        int newY = GetMaxY(rects) + spacing;

        return new Point(newX, newY);
    }

    /// <summary>
    /// Gets the maximum Y coordinate (bottom edge) of all rectangles in the list.
    /// </summary>
    /// <param name="rects">The list of rectangles.</param>
    /// <returns>The maximum bottom Y coordinate, or 0 if the list is empty.</returns>
    private static int GetMaxY(List<Rectangle> rects)
    {
        int maxY = 0;
        foreach (var rect in rects)
        {
            if (rect.Bottom > maxY)
            {
                maxY = rect.Bottom;
            }
        }
        return maxY;
    }

    /// <summary>
    /// Checks whether a new rectangle overlaps with any existing rectangles, including the specified spacing.
    /// </summary>
    /// <param name="newRect">The new rectangle to check.</param>
    /// <param name="rects">The list of existing rectangles.</param>
    /// <param name="spacing">The minimum spacing to consider.</param>
    /// <returns>True if the new rectangle overlaps with any existing rectangle; otherwise, false.</returns>
    private static bool IsOverlapping(Rectangle newRect, List<Rectangle> rects, int spacing)
    {
        foreach (var rect in rects)
        {
            if (newRect.IntersectsWith(rect) ||
                new Rectangle(newRect.X - spacing, newRect.Y - spacing,
                              newRect.Width + 2 * spacing, newRect.Height + 2 * spacing).IntersectsWith(rect))
            {
                return true;
            }
        }
        return false;
    }
}
