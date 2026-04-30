using System;

namespace Suity.Views.Im;

/// <summary>
/// Interface for objects that can provide values by type.
/// </summary>
public interface IValueSource
{
    /// <summary>
    /// Gets a value of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of value to retrieve.</typeparam>
    /// <returns>The value, or null if not found.</returns>
    T? GetValue<T>() where T : class;
}

/// <summary>
/// Represents a collection of GUI values that can be queried by type.
/// Extends <see cref="IValueSource"/> with the ability to check if the collection is empty.
/// </summary>
public interface IValueCollection : IValueSource
{
    /// <summary>
    /// Gets whether this collection is empty.
    /// </summary>
    bool IsEmpty { get; }
}

/// <summary>
/// Represents a named set of styles with support for pseudo states and transitions.
/// </summary>
public interface IStyleSet
{
    /// <summary>
    /// Gets the name of this style set.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the style collection for a specific pseudo state.
    /// </summary>
    /// <param name="pseudo">The pseudo state name, or null for the default state.</param>
    /// <returns>The style collection, or null if not found.</returns>
    IValueCollection? GetStyleCollection(string? pseudo);

    /// <summary>
    /// Gets a transition factory for transitioning between states.
    /// </summary>
    /// <param name="state">The current state.</param>
    /// <param name="targetState">The target state.</param>
    /// <returns>The transition factory, or null if not found.</returns>
    ITransitionFactory? GetTransition(string? state, string? targetState);

    /// <summary>
    /// Gets whether this style set is empty.
    /// </summary>
    bool IsEmpty { get; }
}

/// <summary>
/// An empty value source that always returns null.
/// </summary>
public class EmptyValueSource : IValueSource
{
    /// <summary>
    /// Gets the singleton empty value source instance.
    /// </summary>
    public static EmptyValueSource Empty { get; } = new();

    private EmptyValueSource()
    {
    }

    /// <inheritdoc/>
    public T? GetValue<T>() where T : class
    {
        return null;
    }
}

/// <summary>
/// Interface for values that support linear interpolation (lerp) for animations.
/// </summary>
/// <typeparam name="T">The type of value that can be interpolated.</typeparam>
public interface IValueTransition<T>
{
    /// <summary>
    /// Interpolates between this value and another value.
    /// </summary>
    /// <param name="v2">The target value.</param>
    /// <param name="t">The interpolation factor (0-1).</param>
    /// <returns>A new value interpolated between this and v2.</returns>
    T Lerp(T v2, float t);
}

/// <summary>
/// Interface for objects that provide floating-point time information.
/// </summary>
public interface IFloatTime
{
    /// <summary>
    /// Gets the current time in seconds.
    /// </summary>
    float Time { get; }

    /// <summary>
    /// Gets the time elapsed since the last frame in seconds.
    /// </summary>
    float DeltaTime { get; }
}

/// <summary>
/// Interface for GUI animations that can be started and updated over time.
/// </summary>
public interface IGuiAnimation : IValueSource
{
    /// <summary>
    /// Starts the animation at the specified time.
    /// </summary>
    /// <param name="startTime">The time at which the animation starts.</param>
    void Start(float startTime);

    /// <summary>
    /// Updates the animation and returns the required input state.
    /// </summary>
    /// <param name="startTime">The current time.</param>
    /// <param name="time">The time provider.</param>
    /// <returns>The input state indicating what operations are needed.</returns>
    GuiInputState Update(float startTime, IFloatTime time);
}

/// <summary>
/// Factory interface for creating value transitions between two value sources.
/// </summary>
public interface ITransitionFactory
{
    /// <summary>
    /// Creates a transition animation between two value sources.
    /// </summary>
    /// <param name="v1">The starting value source.</param>
    /// <param name="v2">The target value source.</param>
    /// <returns>An animation for transitioning between values, or null if transition is not possible.</returns>
    IGuiAnimation? CreateTransition(IValueSource v1, IValueSource v2);
}
