using System.IO;

namespace Suity.Editor;

/// <summary>
/// Implementation of <see cref="IStorageItem"/> that uses the file system for persistent storage, managing separate input and output file streams.
/// </summary>
/// <param name="fileName">The path of the file to use for storage operations.</param>
public class FileStorageItem(string fileName) : IStorageItem
{
    // Stream used for reading data from the file.
    private Stream _inputStream;
    // Stream used for writing data to the file.
    private Stream _outputStream;

    /// <summary>
    /// Gets the file path associated with this storage item.
    /// </summary>
    public string FileName { get; } = fileName;

    /// <inheritdoc/>
    public Stream GetInputStream()
    {
        if (_inputStream != null)
        {
            return _inputStream;
        }
        else if (!string.IsNullOrEmpty(FileName) && File.Exists(FileName))
        {
            // Open the existing file for reading.
            _inputStream = File.OpenRead(FileName);
            return _inputStream;
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public Stream GetOutputStream()
    {
        if (_outputStream != null)
        {
            return _outputStream;
        }
        else if (!string.IsNullOrEmpty(FileName))
        {
            // Delete the existing file to overwrite it.
            if (File.Exists(FileName))
            {
                File.Delete(FileName);
            }

            // Ensure the parent directory exists before creating the file.
            string dir = Path.GetDirectoryName(FileName);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            _outputStream = File.OpenWrite(FileName);

            return _outputStream;
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_inputStream != null)
        {
            _inputStream.Dispose();
            _inputStream = null;
        }

        if (_outputStream != null)
        {
            _outputStream.Dispose();
            _outputStream = null;
        }
    }
}