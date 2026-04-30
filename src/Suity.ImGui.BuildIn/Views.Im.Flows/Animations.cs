namespace Suity.Views.Im.Flows;

/// <summary>
/// Animation that flashes connector points at regular intervals.
/// </summary>
internal class ConnectorPointFlashing : IGuiAnimation
{
    /// <summary>
    /// Time interval between flashes in seconds.
    /// </summary>
    public const float FlashingInterval = 0.2f;

    /// <summary>
    /// Duration of each flash in seconds.
    /// </summary>
    public const float FlashingDuration = 0.1f;

    /// <summary>
    /// Gets the singleton instance of this animation.
    /// </summary>
    public static ConnectorPointFlashing Instance { get; } = new ConnectorPointFlashing();

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectorPointFlashing"/> class.
    /// </summary>
    protected ConnectorPointFlashing()
    {
    }

    /// <inheritdoc/>
    public T? GetValue<T>() where T : class => null;

    /// <inheritdoc/>
    public void Start(float startTime)
    {
    }

    /// <inheritdoc/>
    public virtual GuiInputState Update(float startTime, IFloatTime time)
    {
        var v1 = GetValue(startTime, time.Time);
        var v2 = GetValue(startTime, time.Time - time.DeltaTime);

        return v1 != v2 ? GuiInputState.Render : GuiInputState.KeepListening;

        //return GuiInputState.Render;
    }

    /// <summary>
    /// Gets the flash state at the specified time.
    /// </summary>
    /// <param name="startTime">The time when the animation started.</param>
    /// <param name="time">The current time.</param>
    /// <returns><c>true</c> if the connector should be flashed at this time; otherwise, <c>false</c>.</returns>
    public bool GetValue(float startTime, float time)
    {
        //return (time - startTime) % FlashingInterval < FlashingDuration;
        return time % FlashingInterval < FlashingDuration;
    }
}

/// <summary>
/// Animation that flashes connector points once for a single interval.
/// </summary>
internal class ConnectorPointFlashingOnce : ConnectorPointFlashing
{
    /// <summary>
    /// Gets the singleton instance of this animation.
    /// </summary>
    public new static ConnectorPointFlashingOnce Instance { get; } = new ConnectorPointFlashingOnce();

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectorPointFlashingOnce"/> class.
    /// </summary>
    protected ConnectorPointFlashingOnce()
    {
    }

    /// <inheritdoc/>
    public override GuiInputState Update(float startTime, IFloatTime time)
    {
        if (time.Time - startTime > FlashingInterval)
        {
            return GuiInputState.None;
        }

        //return GuiInputState.Render;

        return base.Update(startTime, time);
    }
}
