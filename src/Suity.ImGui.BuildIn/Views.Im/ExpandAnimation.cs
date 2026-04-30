using Suity.Helpers;
using System;

namespace Suity.Views.Im;

/// <summary>
/// Animation that smoothly expands or collapses a node.
/// </summary>
internal class ExpandAnimation : EaseAnimation
{
    private readonly ImGuiNode _node;
    private readonly GuiExpandableValue _value;
    private readonly bool _expandFlag;
    private GuiSizeStyle? _size;

    /// <summary>
    /// Initializes a new expand/collapse animation.
    /// </summary>
    /// <param name="node">The node to animate.</param>
    /// <param name="value">The expandable value to control.</param>
    /// <param name="expandFlag">True to expand, false to collapse.</param>
    /// <param name="duration">The animation duration in seconds.</param>
    public ExpandAnimation(ImGuiNode node, GuiExpandableValue value, bool expandFlag, float duration)
    {
        _node = node ?? throw new ArgumentNullException(nameof(node));
        _value = value ?? throw new ArgumentNullException(nameof(value));
        _expandFlag = expandFlag;
        if (duration < 0.1f)
        {
            duration = 0.1f;
        }
        Duration = duration;

        SetKnownEaseFunction(KnownAnimationFunctions.CubicEaseOut);
    }

    /// <inheritdoc/>
    protected override GuiInputState OnStart()
    {
        if (_expandFlag)
        {
            _value.Expanded = true;
        }

        return GuiInputState.KeepListening;
    }

    /// <inheritdoc/>
    protected override GuiInputState OnUpdate()
    {
        _size = null;

        _node.Parent?.MarkRenderDirty();

        return GuiInputState.Layout; 
    }

    /// <inheritdoc/>
    protected override GuiInputState OnFinished()
    {
        if (!_expandFlag)
        {
            _value.Expanded = false;
        }

        // Perform a full sync after animation completes
        return GuiInputState.FullSync;
    }

    /// <inheritdoc/>
    public override T? GetValue<T>() where T : class
    {
        if (typeof(T) != typeof(GuiSizeStyle))
        {
            return null;
        }

        if (_size is T v)
        {
            return v;
        }

        var width = _node.BaseWidth;
        var height = _node.BaseHeight;
        var headerHeight = _node.HeaderHeight;

        if (height is { Mode: GuiLengthMode.Fixed } && headerHeight is { })
        {
            _size = new();

            float t = TimePosition;
            if (!_expandFlag)
            {
                t = 1 - t;
            }

            float v1 = headerHeight.Value;
            float v2 = height.Value.Value;
            float value = MathHelper.Lerp(v1, v2, t);

            _size.Width = width;
            _size.Height = new GuiLength(value, GuiLengthMode.Fixed);

            //Debug.WriteLine($"value={value}");

            return _size as T;
        }

        return null;
    }
}