namespace Suity.Editor.Flows.SubFlows;

/// <summary>
/// Provides extension methods for AIGC task pages and related types.
/// </summary>
public static class TaskPageExtensions
{
    /// <summary>
    /// Determines whether the specified <see cref="PageElementMode"/> represents a task or page mode.
    /// </summary>
    /// <param name="mode">The page element mode to check.</param>
    /// <returns><c>true</c> if the mode is <see cref="PageElementMode.Page"/> or <see cref="PageElementMode.Task"/>; otherwise, <c>false</c>.</returns>
    public static bool IsTaskOrPage(this PageElementMode mode)
    {
        switch (mode)
        {
            case PageElementMode.Page:
            case PageElementMode.Task:
                return true;

            default:
                return false;
        }
    }

}
