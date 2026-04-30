using Suity.Helpers;
using System;

namespace Suity.Views.Im;

/// <summary>
/// Base class for easing animations that interpolate over time.
/// </summary>
public class EaseAnimation : IGuiAnimation
{
    private enum AnimationState
    {
        None,
        Start,
        Finish,
    }

    private float _duration;
    private float _delay;
    private AnimationFunctions.Function _function = AnimationFunctions.Linear;

    private float _startTime;
    private float _t;
    private AnimationState _aniState = AnimationState.None;
    private GuiInputState _guiState = GuiInputState.KeepListening;

    /// <summary>
    /// Initializes a new ease animation.
    /// </summary>
    public EaseAnimation()
    {
    }

    /// <summary>
    /// Gets or sets the easing function used for interpolation.
    /// </summary>
    public AnimationFunctions.Function EaseFunction
    {
        get => _function;
        set => _function = value;
    }

    /// <summary>
    /// Gets or sets the total duration of the animation in seconds.
    /// </summary>
    public float Duration
    {
        get => _duration;
        set => _duration = value;
    }

    /// <summary>
    /// Gets or sets the delay before the animation starts, in seconds.
    /// </summary>
    public float Delay
    {
        get => _delay;
        set => _delay = value;
    }

    /// <summary>
    /// Gets the current time position of the animation, normalized between 0 and 1.
    /// </summary>
    public float TimePosition => _t;

    /// <summary>
    /// Gets whether the animation has finished.
    /// </summary>
    public bool IsFinished => _t >= 1;

    /// <summary>
    /// Sets the easing function from a known preset.
    /// </summary>
    /// <param name="f">The known animation function preset.</param>
    public void SetKnownEaseFunction(KnownAnimationFunctions f)
    {
        _function = AnimationFunctions.FromKnown(f);
    }

    /// <inheritdoc/>
    public void Start(float startTime)
    {
        if (_aniState != AnimationState.None)
        {
            return;
        }

        var now = startTime;
        _startTime = now + _delay;
        if (now >= _startTime)
        {
            _aniState = AnimationState.Start;
            _guiState = OnStart();
        }
    }

    /// <inheritdoc/>
    public GuiInputState Update(float startTime, IFloatTime time)
    {
        float now = time.Time;

        switch (_aniState)
        {
            case AnimationState.None:
                if (now >= _startTime)
                {
                    _aniState = AnimationState.Start;
                    _guiState = OnStart();
                }
                else
                {
                    _guiState = GuiInputState.KeepListening;
                    return _guiState;
                }
                break;

            case AnimationState.Finish:
                return GuiInputState.None;
        }

        if (IsFinished)
        {
            _aniState = AnimationState.Finish;
            _guiState = OnFinished();
            return _guiState;
        }

        _t = _function(now - _startTime, 0, 1, _duration);
        MathHelper.Clamp(ref _t, 0, 1);
        _guiState = OnUpdate();

        if (IsFinished)
        {
            _aniState = AnimationState.Finish;
            _guiState = OnFinished();
        }

        return _guiState;
    }

    /// <inheritdoc/>
    public virtual T? GetValue<T>() where T : class => null;

    /// <inheritdoc/>
    public virtual void Dispose()
    {
    }

    /// <summary>
    /// Called when the animation starts. Override to perform initialization.
    /// </summary>
    /// <returns>The input state to return.</returns>
    protected virtual GuiInputState OnStart() => GuiInputState.KeepListening;

    /// <summary>
    /// Called each frame while the animation is running. Override to update animated values.
    /// </summary>
    /// <returns>The input state to return.</returns>
    protected virtual GuiInputState OnUpdate() => GuiInputState.Render;

    /// <summary>
    /// Called when the animation finishes. Override to perform cleanup.
    /// </summary>
    /// <returns>The input state to return.</returns>
    protected virtual GuiInputState OnFinished() => GuiInputState.Render;
}

/// <summary>
/// Generic ease animation that interpolates between two values of a specific type.
/// </summary>
/// <typeparam name="TValue">The type of value to interpolate. Must implement <see cref="IValueTransition{TValue}"/>.</typeparam>
public class EaseAnimation<TValue> : EaseAnimation
    where TValue : class, IValueTransition<TValue>
{
    private readonly TValue _sourceValue;
    private readonly TValue _targetValue;
    private readonly GuiInputState _updateState;
    private TValue? _value;

    /// <summary>
    /// Initializes a new generic ease animation.
    /// </summary>
    /// <param name="sourceValue">The starting value.</param>
    /// <param name="targetValue">The target value.</param>
    /// <param name="updateState">The input state to return during updates.</param>
    public EaseAnimation(TValue sourceValue, TValue targetValue, GuiInputState updateState = GuiInputState.Render)
    {
        _sourceValue = sourceValue ?? throw new ArgumentNullException(nameof(sourceValue));
        _targetValue = targetValue ?? throw new ArgumentNullException(nameof(targetValue));
        _updateState = updateState;

        _value = _sourceValue;
    }

    /// <inheritdoc/>
    protected override GuiInputState OnUpdate()
    {
        _value = null;

        return _updateState;
    }

    /// <inheritdoc/>
    public override T? GetValue<T>() where T : class
    {
        if (typeof(T) != typeof(TValue))
        {
            return null;
        }

        if (_value is T result)
        {
            return result;
        }

        _value = _sourceValue.Lerp(_targetValue, TimePosition);

        return _value as T;
    }
}

/// <summary>
/// Ease animation that transitions from a node's current style value to a target value.
/// </summary>
/// <typeparam name="TValue">The type of value to interpolate.</typeparam>
public class ImGuiToEaseAnimation<TValue> : EaseAnimation<TValue>
    where TValue : class, IValueTransition<TValue>
{
    /// <summary>
    /// Initializes an animation from the node's current style to the target value.
    /// </summary>
    /// <param name="node">The ImGui node.</param>
    /// <param name="targetValue">The target style value.</param>
    /// <param name="updateState">The input state to return during updates.</param>
    public ImGuiToEaseAnimation(ImGuiNode node, TValue targetValue, GuiInputState updateState = GuiInputState.Render)
        : base(node.GetStyle<TValue>() ?? targetValue, targetValue, updateState)
    {
    }
}

/// <summary>
/// Ease animation that transitions from a source value to a node's current style value.
/// </summary>
/// <typeparam name="TValue">The type of value to interpolate.</typeparam>
public class ImGuiFromEaseAnimation<TValue> : EaseAnimation<TValue>
    where TValue : class, IValueTransition<TValue>
{
    /// <summary>
    /// Initializes an animation from the source value to the node's current style.
    /// </summary>
    /// <param name="node">The ImGui node.</param>
    /// <param name="sourceValue">The source style value.</param>
    /// <param name="updateState">The input state to return during updates.</param>
    public ImGuiFromEaseAnimation(ImGuiNode node, TValue sourceValue, GuiInputState updateState = GuiInputState.Render)
        : base(sourceValue, node.GetStyle<TValue>() ?? sourceValue, updateState)
    {
    }
}