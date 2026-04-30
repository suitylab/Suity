using System.IO;

namespace Suity.Editor;

/// <summary>
/// Implementation of <see cref="IStorageItem"/> that uses an in-memory <see cref="MemoryStream"/> for storage, suitable for temporary or volatile data operations.
/// </summary>
public class MemoryStorageItem : IStorageItem
{
    // Underlying memory stream used for both input and output operations.
    private MemoryStream _stream;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryStorageItem"/> class with no associated file name.
    /// </summary>
    public MemoryStorageItem()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryStorageItem"/> class with the specified file name.
    /// </summary>
    /// <param name="fileName">The logical file name associated with this storage item.</param>
    public MemoryStorageItem(string fileName)
    {
        FileName = fileName;
    }

    /// <summary>
    /// Gets the logical file name associated with this storage item.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Gets the underlying <see cref="MemoryStream"/> used for storage, or <c>null</c> if no stream has been created yet.
    /// </summary>
    public MemoryStream Stream => _stream;

    /// <inheritdoc/>
    public Stream GetInputStream()
    {
        if (_stream != null)
        {
            return _stream;
        }

        _stream = new MemoryStream();
        return _stream;
    }

    /// <inheritdoc/>
    public Stream GetOutputStream()
    {
        if (_stream != null)
        {
            return _stream;
        }

        _stream = new MemoryStream();
        return _stream;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_stream != null)
        {
            _stream.Dispose();
            _stream = null;
        }
    }
}