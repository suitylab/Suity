namespace Suity.Views.Gui;

/// <summary>
/// Defines the contract for managing window lifecycle and focus operations.
/// </summary>
public interface IWindowHandle
{
    /// <summary>
    /// Closes the window.
    /// </summary>
    void Close();

    /// <summary>
    /// Sets the input focus to the window.
    /// </summary>
    void Focus();

    /// <summary>
    /// Updates the window state and redraws its contents.
    /// </summary>
    void Update();
}

/// <summary>
/// Represents a no-op implementation of <see cref="IWindowHandle"/> for cases where no window handle is available.
/// </summary>
public sealed class EmptyWindowHandle : IWindowHandle
{
    /// <summary>
    /// Gets the singleton empty window handle instance.
    /// </summary>
    public static EmptyWindowHandle Empty = new();

    private EmptyWindowHandle()
    {
    }

    /// <inheritdoc/>
    public void Close()
    {
    }

    /// <inheritdoc/>
    public void Focus()
    {
    }

    /// <inheritdoc/>
    public void Update()
    {
    }
}