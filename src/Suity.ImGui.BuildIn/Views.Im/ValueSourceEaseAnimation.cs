using System;
using System.Collections.Generic;

namespace Suity.Views.Im;

/// <summary>
/// Animation that interpolates between two value sources using easing functions.
/// </summary>
public class ValueSourceEaseAnimation : EaseAnimation
{
    private readonly Dictionary<Type, object> _lerpValues = new();

    private readonly IValueSource _v1;
    private readonly IValueSource _v2;

    /// <summary>
    /// Initializes a new animation that transitions between two value sources.
    /// </summary>
    /// <param name="v1">The source value.</param>
    /// <param name="v2">The target value.</param>
    public ValueSourceEaseAnimation(IValueSource v1, IValueSource v2)
    {
        _v1 = v1;
        _v2 = v2;
    }

    /// <inheritdoc/>
    protected override GuiInputState OnUpdate()
    {
        if (_lerpValues.Count > 0)
        {
            foreach (var value in _lerpValues.Values)
            {
                ValuePool.Recycle(value);
            }
            _lerpValues.Clear();
        }

        return GuiInputState.Render;
    }

    /// <inheritdoc/>
    public override T? GetValue<T>() where T : class
    {
        if (_lerpValues.TryGetValue(typeof(T), out var current) && current is T t)
        {
            return t;
        }

        var v1 = _v1.GetValue<T>();
        var v2 = _v2.GetValue<T>();

        if (v1 is IValueTransition<T> lerp && v2 is { })
        {
            var v = lerp.Lerp(v2, TimePosition);
            _lerpValues[typeof(T)] = v;

            return v;
        }

        return v1;
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        (_v1 as IDisposable)?.Dispose();
        (_v2 as IDisposable)?.Dispose();

        foreach (var value in _lerpValues.Values)
        {
            ValuePool.Recycle(value);
        }

        _lerpValues.Clear();
    }
}

/// <summary>
/// Factory that creates ease-based transition animations.
/// </summary>
public class EaseTransitionFactory : ITransitionFactory
{
    /// <summary>
    /// Initializes a new instance with a default duration of 0.5 seconds.
    /// </summary>
    public EaseTransitionFactory()
    {
        Duration = 0.5f;
    }

    /// <summary>
    /// Initializes a new instance with the specified duration.
    /// </summary>
    /// <param name="duration">The animation duration in seconds.</param>
    public EaseTransitionFactory(float duration)
    {
        Duration = duration;
    }

    /// <summary>
    /// Gets or sets the animation duration in seconds.
    /// </summary>
    public float Duration { get; set; }

    /// <inheritdoc/>
    public IGuiAnimation CreateTransition(IValueSource v1, IValueSource v2)
    {
        return new ValueSourceEaseAnimation(v1, v2) { Duration = Duration };
    }
}