using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;

namespace Suity.Editor.Libraries;

/// <summary>
/// Represents a read-only storage item for accessing files within a library archive.
/// </summary>
internal class LibraryStorageItem : IStorageItem
{
    private Stream _fs;
    private ZipFile _zf;

    private Stream _inputStream;

    /// <summary>
    /// Initializes a new instance targeting a specific entry in a library archive.
    /// </summary>
    /// <param name="libraryFileName">The path to the library archive file.</param>
    /// <param name="index">The index of the entry within the archive.</param>
    public LibraryStorageItem(string libraryFileName, int index)
    {
        LibraryFileName = libraryFileName ?? throw new ArgumentNullException(nameof(libraryFileName));
        LibraryFileIndex = index;
    }

    /// <summary>
    /// Gets the path to the library archive file.
    /// </summary>
    public string LibraryFileName { get; }

    /// <summary>
    /// Gets the index of the entry within the library archive.
    /// </summary>
    public int LibraryFileIndex { get; }

    /// <inheritdoc/>
    public string FileName => null;

    /// <inheritdoc/>
    public Stream GetInputStream()
    {
        if (_inputStream != null)
        {
            return _inputStream;
        }

        _zf?.Close();
        _fs?.Close();
        _fs?.Dispose();

        _fs = File.OpenRead(LibraryFileName);
        _zf = new ZipFile(_fs);
        if (!string.IsNullOrEmpty(LibraryAssetBK._xx))
        {
            _zf.Password = LibraryAssetBK._xx;
        }

        _inputStream = _zf.GetInputStream(LibraryFileIndex);
        return _inputStream;
    }

    /// <inheritdoc/>
    public Stream GetOutputStream()
    {
        return null;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _inputStream?.Close();
        _zf?.Close();
        _fs?.Close();
        _fs?.Dispose();
    }
}
