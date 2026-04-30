namespace Suity.Views.Im;

/// <summary>
/// Provides extension methods for orientation-related value checks.
/// </summary>
public static class GuiValueExtensions
{
    /// <summary>
    /// Determines whether the orientation includes horizontal direction.
    /// </summary>
    /// <param name="orientation">The orientation to check.</param>
    /// <returns>True if the orientation is Horizontal or Both; otherwise, false.</returns>
    public static bool IsHorizontal(this GuiOrientation orientation)
    {
        return orientation == GuiOrientation.Both || orientation == GuiOrientation.Horizontal;
    }

    /// <summary>
    /// Determines whether the orientation includes vertical direction.
    /// </summary>
    /// <param name="orientation">The orientation to check.</param>
    /// <returns>True if the orientation is Vertical or Both; otherwise, false.</returns>
    public static bool IsVertical(this GuiOrientation orientation)
    {
        return orientation == GuiOrientation.Both || orientation == GuiOrientation.Vertical;
    }

    /// <summary>
    /// Determines whether the nullable orientation includes horizontal direction.
    /// </summary>
    /// <param name="orientation">The nullable orientation to check.</param>
    /// <returns>True if the orientation is Horizontal or Both; otherwise, false.</returns>
    public static bool IsHorizontal(this GuiOrientation? orientation)
    {
        return (orientation ?? GuiOrientation.None).IsHorizontal();
    }

    /// <summary>
    /// Determines whether the nullable orientation includes vertical direction.
    /// </summary>
    /// <param name="orientation">The nullable orientation to check.</param>
    /// <returns>True if the orientation is Vertical or Both; otherwise, false.</returns>
    public static bool IsVertical(this GuiOrientation? orientation)
    {
        return (orientation ?? GuiOrientation.None).IsVertical();
    }
}
