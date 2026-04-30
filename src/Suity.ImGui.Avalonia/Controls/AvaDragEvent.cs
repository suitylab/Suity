using Avalonia.Input;
using Suity.Helpers;
using Suity.Views;

namespace Suity.Controls;

/// <summary>
/// Avalonia-specific implementation of the drag event interface.
/// </summary>
internal class AvaDragEvent : IDragEvent
{
    /// <summary>
    /// Gets the global singleton instance.
    /// </summary>
    public static AvaDragEvent Global = new();


    private DragEventArgs? _args;
    private readonly AvaDragEventData _data = new();



    /// <summary>
    /// Initializes a new instance of the <see cref="AvaDragEvent"/> class.
    /// </summary>
    public AvaDragEvent()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaDragEvent"/> class with specified event arguments.
    /// </summary>
    /// <param name="args">The drag event arguments.</param>
    public AvaDragEvent(DragEventArgs args)
    {
        _args = args;
    }

    /// <summary>
    /// Stores an object internally and returns a unique identifier for it.
    /// </summary>
    /// <param name="obj">The object to store.</param>
    /// <returns>A unique identifier string.</returns>
    public string SetInternalData(object obj) => _data.SetInternalData(obj);

    /// <summary>
    /// Updates the internal event arguments and data transfer.
    /// </summary>
    /// <param name="args">The drag event arguments, or null to clear.</param>
    public void UpdateEventArgs(DragEventArgs? args)
    {
        _args = args;
        if (args != null)
        {
            _data.UpdateData(args.DataTransfer);

            _args?.DragEffects = DragDropEffects.Move;
        }
        else
        {
            _data.UpdateData(null);
        }
    }

    /// <inheritdoc/>
    public DragEventData Data => _data;

    /// <inheritdoc/>
    public int KeyState => 0;

    /// <inheritdoc/>
    public int ScreenX => 0;

    /// <inheritdoc/>
    public int ScreenY => 0;

    /// <inheritdoc/>
    public bool Handled => _args?.Handled == true;

    /// <inheritdoc/>
    public bool GetCopyEffect()
    {
        return _args?.DragEffects == DragDropEffects.Copy;
    }

    /// <inheritdoc/>
    public bool GetLinkEffect()
    {
        return _args?.DragEffects == DragDropEffects.Link;
    }

    /// <inheritdoc/>
    public bool GetMoveEffect()
    {
        return _args?.DragEffects == DragDropEffects.Move;
    }

    /// <inheritdoc/>
    public void SetCopyEffect()
    {
        _args?.DragEffects = DragDropEffects.Copy;
        _args?.Handled = true;
    }

    /// <inheritdoc/>
    public void SetLinkEffect()
    {
        _args?.DragEffects = DragDropEffects.Link;
        _args?.Handled = true;
    }

    /// <inheritdoc/>
    public void SetMoveEffect()
    {
        _args?.DragEffects = DragDropEffects.Move;
        _args?.Handled = true;
    }

    /// <inheritdoc/>
    public void SetNoneEffect()
    {
        _args?.DragEffects = DragDropEffects.None;
        _args?.Handled = false;
    }
}

/// <summary>
/// Internal drag event data implementation for Avalonia.
/// </summary>
internal class AvaDragEventData : DragEventData
{
    private IDataTransfer? _transfer;

    bool _isInternalData;
    string? _internalDataId;
    DragEventData? _internalData;
    IDragData? _simpleData;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaDragEventData"/> class.
    /// </summary>
    public AvaDragEventData()
    {
    }

    /// <summary>
    /// Stores an object internally and returns a unique identifier for it.
    /// </summary>
    /// <param name="obj">The object to store.</param>
    /// <returns>A unique identifier string.</returns>
    public string SetInternalData(object obj)
    {
        _internalDataId = "suity#drag://" + AvaIdGenerator.GenerateId(12);
        if (obj is IDragData dropData)
        {
            _internalData = new EditorDragEventData(dropData);
            _simpleData = null;
        }
        else
        {
            _internalData = null;
            _simpleData = new SimpleDragData(obj);
        }

        _isInternalData = false;

        return _internalDataId;
    }

    /// <summary>
    /// Updates the internal data transfer reference.
    /// </summary>
    /// <param name="dataTransfer">The data transfer, or null to clear.</param>
    public void UpdateData(IDataTransfer? dataTransfer)
    {
        _transfer = dataTransfer;

        if (dataTransfer != null)
        {
            var formats = _transfer?.Formats.Select(o => o.Identifier).ToArray() ?? [];
        }

        string? text = dataTransfer?.TryGetText();
        _isInternalData = !string.IsNullOrEmpty(text) && text == _internalDataId;

        if (_isInternalData)
        {
        }
        else
        {
            _internalData = null;
            _simpleData = null;
            _internalDataId = null;
        }
    }

    /// <inheritdoc/>
    public override object? GetData(string format, bool autoConvert = false)
    {
        if (_isInternalData)
        {
            return _internalData?.GetData(format, autoConvert);
        }
        else
        {
            if (_transfer is not IDataTransfer transfer)
            {
                return null;
            }
            
            switch (format)
            {
                case DataFormat_Text:
                    return transfer.TryGetText();

                case DataFormat_Bitmap:
                    return transfer.TryGetBitmap();

                case DataFormat_File:
                    return transfer.TryGetFiles()?.Select(o => o.Path.LocalPath).ToArray() ?? [];

                default:
                    return null;
            }
        }
    }

    /// <inheritdoc/>
    public override object? GetData(Type format)
    {
        if (_isInternalData)
        {
            return _internalData?.GetData(format) ?? _simpleData?.GetData(format);
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public override bool GetDataPresent(string format, bool autoConvert = false)
    {
        if (_isInternalData)
        {
            return _internalData?.GetDataPresent(format, autoConvert) ?? false;
        }
        else
        {
            return _transfer?.Formats.Any(o => o.Identifier == format) == true;
        }
    }

    /// <inheritdoc/>
    public override bool GetDataPresent(Type format)
    {
        if (_isInternalData)
        {
            return _internalData?.GetDataPresent(format) ?? _simpleData?.GetDataPresent(format) ?? false;
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public override string[]? GetFormats(bool autoConvert = false)
    {
        if (_isInternalData)
        {
            return _internalData?.GetFormats(autoConvert) ?? [];
        }
        else
        {
            return _transfer?.Formats.Select(o => o.Identifier).ToArray() ?? [];
        }
    }
}
