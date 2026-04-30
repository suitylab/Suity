using System;

namespace Suity.Views.Im;

/// <summary>
/// Animation that smoothly scrolls to a target position.
/// </summary>
internal class ScrollAnimation : EaseAnimation
{
    private readonly ImGuiNode _node;
    private readonly GuiScrollableValue _value;

    private readonly GuiOrientation _orientation;
    private readonly float _fromScroll;
    private readonly float _toScroll;

    /// <summary>
    /// Initializes a new scroll animation.
    /// </summary>
    /// <param name="node">The scrollable node.</param>
    /// <param name="value">The scrollable value to animate.</param>
    /// <param name="orientation">The scroll orientation.</param>
    /// <param name="fromScroll">The starting scroll position.</param>
    /// <param name="toScroll">The target scroll position.</param>
    /// <param name="duration">The animation duration in seconds.</param>
    public ScrollAnimation(ImGuiNode node, GuiScrollableValue value, GuiOrientation orientation, float fromScroll, float toScroll, float duration)
    {
        _node = node ?? throw new ArgumentNullException(nameof(node));
        _value = value ?? throw new ArgumentNullException(nameof(value));
        _orientation = orientation;
        _fromScroll = fromScroll;
        _toScroll = toScroll;
        Duration = duration;
    }

    /// <inheritdoc/>
    protected override GuiInputState OnStart()
    {
        SetValue(_fromScroll);
        return GuiInputState.Layout;
    }

    /// <inheritdoc/>
    protected override GuiInputState OnUpdate()
    {
        float value = _fromScroll + (_toScroll - _fromScroll) * TimePosition;

        //Debug.WriteLine($"t={TimePosition}");

        SetValue(value);

        return GuiInputState.Layout;
    }

    /// <inheritdoc/>
    protected override GuiInputState OnFinished()
    {
        SetValue(_toScroll);

        return GuiInputState.Layout;
    }

    private void SetValue(float value)
    {
        switch (_orientation)
        {
            case GuiOrientation.Vertical:
                _value.ScrollY = value;
                break;

            case GuiOrientation.Horizontal:
                _value.ScrollX = value;
                break;
        }
    }
}