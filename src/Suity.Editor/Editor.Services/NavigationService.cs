using Suity.Editor.Documents;

namespace Suity.Editor.Services;

/// <summary>
/// Service for managing document navigation history.
/// </summary>
public abstract class NavigationService
{
    private static NavigationService _current;

    /// <summary>
    /// Gets or sets the current navigation service instance.
    /// </summary>
    public static NavigationService Current
    {
        get
        {
            if (_current != null)
            {
                return _current;
            }

            _current = Device.Current.GetService<NavigationService>();
            return _current;
        }
        internal set
        {
            _current = value;
        }
    }

    /// <summary>
    /// Adds a document to the navigation history.
    /// </summary>
    /// <param name="document">The document to add.</param>
    public abstract void AddRecord(Document document);

    /// <summary>
    /// Navigates backward in the navigation history.
    /// </summary>
    public abstract void BackwardNavigation();

    /// <summary>
    /// Navigates forward in the navigation history.
    /// </summary>
    public abstract void ForwardNavigation();

    /// <summary>
    /// Gets a value indicating whether backward navigation is available.
    /// </summary>
    public abstract bool HasBackward { get; }

    /// <summary>
    /// Gets a value indicating whether forward navigation is available.
    /// </summary>
    public abstract bool HasForward { get; }
}