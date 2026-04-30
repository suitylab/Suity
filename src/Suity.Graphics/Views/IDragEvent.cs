using System;

namespace Suity.Views;

/// <summary>
/// Drag and drop placement request
/// </summary>
public interface IDragEvent
{
    /// <summary>
    /// Gets the drag event data.
    /// </summary>
    DragEventData Data { get; }

    /// <summary>
    /// Gets the key state during the drag operation.
    /// </summary>
    int KeyState { get; }

    /// <summary>
    /// Gets the screen x-coordinate.
    /// </summary>
    int ScreenX { get; }

    /// <summary>
    /// Gets the screen y-coordinate.
    /// </summary>
    int ScreenY { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the event has been handled.
    /// </summary>
    bool Handled { get; }

    /// <summary>
    /// Checks if the copy effect is requested.
    /// </summary>
    /// <returns>True if copy effect is active.</returns>
    bool GetCopyEffect();

    /// <summary>
    /// Checks if the move effect is requested.
    /// </summary>
    /// <returns>True if move effect is active.</returns>
    bool GetMoveEffect();

    /// <summary>
    /// Checks if the link effect is requested.
    /// </summary>
    /// <returns>True if link effect is active.</returns>
    bool GetLinkEffect();

    /// <summary>
    /// Sets the drag effect to none.
    /// </summary>
    void SetNoneEffect();

    /// <summary>
    /// Sets the drag effect to copy.
    /// </summary>
    void SetCopyEffect();

    /// <summary>
    /// Sets the drag effect to move.
    /// </summary>
    void SetMoveEffect();

    /// <summary>
    /// Sets the drag effect to link.
    /// </summary>
    void SetLinkEffect();
}

/// <summary>
/// drag and drop interface to place request data
/// </summary>
public interface IDragData
{
    /// <summary>
    /// Gets the data in the specified format.
    /// </summary>
    /// <param name="format">The type format to retrieve.</param>
    /// <returns>The data object, or null if not available.</returns>
    object GetData(Type format);

    /// <summary>
    /// Checks if data is available in the specified format.
    /// </summary>
    /// <param name="format">The type format to check.</param>
    /// <returns>True if data is present in the format.</returns>
    bool GetDataPresent(Type format);
}

/// <summary>
/// Simple implementation of IDragData wrapping a single data object.
/// </summary>
public class SimpleDragData : IDragData
{
    /// <summary>
    /// Gets the wrapped data object.
    /// </summary>
    public object Data { get; }

    /// <summary>
    /// Creates a new instance with the specified data.
    /// </summary>
    /// <param name="data">The data to wrap.</param>
    public SimpleDragData(object data)
    {
        Data = data;
    }

    /// <inheritdoc/>
    public object GetData(Type format)
    {
        if (Data is { } data && format.IsAssignableFrom(data.GetType()))
        {
            return data;
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public bool GetDataPresent(Type format)
    {
        return Data is { } data && format.IsAssignableFrom(data.GetType());
    }
}

/// <summary>
/// Drag and drop base class to place request data
/// </summary>
public abstract class DragEventData : IDragData
{
    /// <summary>
    /// Data format constant for text.
    /// </summary>
    public const string DataFormat_Text = "Text";

    /// <summary>
    /// Data format constant for bitmap.
    /// </summary>
    public const string DataFormat_Bitmap = "Bitmap";

    /// <summary>
    /// Data format constant for file.
    /// </summary>
    public const string DataFormat_File = "File";

    /// <summary>
    /// Gets the data in the specified string format.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="autoConvert">Whether to auto-convert.</param>
    /// <returns>The data object.</returns>
    public abstract object GetData(string format, bool autoConvert = false);

    /// <inheritdoc/>
    public abstract object GetData(Type format);

    /// <summary>
    /// Checks if data is present in the specified string format.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="autoConvert">Whether to auto-convert.</param>
    /// <returns>True if data is present.</returns>
    public abstract bool GetDataPresent(string format, bool autoConvert = false);

    /// <inheritdoc/>
    public abstract bool GetDataPresent(Type format);

    /// <summary>
    /// Gets all available formats.
    /// </summary>
    /// <param name="autoConvert">Whether to include auto-convertible formats.</param>
    /// <returns>Array of format strings.</returns>
    public abstract string[] GetFormats(bool autoConvert = false);

    /// <summary>
    /// Gets the data as the specified type.
    /// </summary>
    /// <typeparam name="T">The type to retrieve.</typeparam>
    /// <returns>The data object, or null.</returns>
    public T GetData<T>() where T : class => GetData(typeof(T)) as T;

    /// <summary>
    /// Checks if data is present as the specified type.
    /// </summary>
    /// <typeparam name="T">The type to check.</typeparam>
    /// <returns>True if data is present.</returns>
    public bool GetDataPresent<T>() where T : class => GetDataPresent(typeof(T));
}

/// <summary>
/// Empty implementation of DragEventData that returns no data.
/// </summary>
public sealed class EmptyDragEventData : DragEventData
{
    /// <summary>
    /// Gets the singleton empty instance.
    /// </summary>
    public static readonly EmptyDragEventData Empty = new();

    private EmptyDragEventData()
    {
    }

    /// <inheritdoc/>
    public override object GetData(string format, bool autoConvert = false)
    {
        return null;
    }

    /// <inheritdoc/>
    public override object GetData(Type format)
    {
        return null;
    }

    /// <inheritdoc/>
    public override bool GetDataPresent(string format, bool autoConvert = false)
    {
        return false;
    }

    /// <inheritdoc/>
    public override bool GetDataPresent(Type format)
    {
        return false;
    }

    /// <inheritdoc/>
    public override string[] GetFormats(bool autoConvert = false)
    {
        return [];
    }
}

/// <summary>
/// Editor custom drag and drop placement of request data
/// </summary>
public sealed class EditorDragEventData : DragEventData
{
    private readonly IDragData _data;

    /// <summary>
    /// Creates a new instance wrapping the specified drag data.
    /// </summary>
    /// <param name="data">The drag data to wrap.</param>
    public EditorDragEventData(IDragData data)
    {
        _data = data ?? throw new ArgumentNullException(nameof(data));
    }

    /// <inheritdoc/>
    public override object GetData(string format, bool autoConvert = false)
    {
        return null;
    }

    /// <inheritdoc/>
    public override object GetData(Type format)
    {
        return _data.GetData(format);
    }

    /// <inheritdoc/>
    public override bool GetDataPresent(string format, bool autoConvert = false)
    {
        return false;
    }

    /// <inheritdoc/>
    public override bool GetDataPresent(Type format)
    {
        return _data.GetDataPresent(format);
    }

    /// <inheritdoc/>
    public override string[] GetFormats(bool autoConvert = false)
    {
        return [];
    }
}

