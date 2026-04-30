namespace Suity.Editor.AIGC.TaskPages;

/// <summary>
/// Provides extension methods for nullable boolean values used in task-related operations.
/// </summary>
public static class TaskExtensions
{
    /// <summary>
    /// Returns true if the nullable boolean is null or has a value of true.
    /// Returns false only when the value is explicitly false.
    /// </summary>
    /// <param name="done">The nullable boolean value to evaluate.</param>
    /// <returns>True if the value is null or true; false otherwise.</returns>
    public static bool IsTrueOrEmpty(this bool? done)
    {
        if (done is { } v)
        {
            return v;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// Returns true if the nullable boolean has a value of true; false otherwise.
    /// </summary>
    /// <param name="done">The nullable boolean value to evaluate.</param>
    /// <returns>True if the value is true; false if null or false.</returns>
    public static bool IsTrue(this bool? done) => done == true;


    /// <summary>
    /// Returns true if the nullable boolean has a value of false; false otherwise.
    /// </summary>
    /// <param name="done">The nullable boolean value to evaluate.</param>
    /// <returns>True if the value is false; false if null or true.</returns>
    public static bool IsFalse(this bool? done) => done == false;
}
