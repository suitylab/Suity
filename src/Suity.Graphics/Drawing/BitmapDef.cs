using System;
using System.IO;

namespace Suity.Drawing;

/// <summary>
/// Defines a resolver interface for bitmap images that provides width and height information.
/// </summary>
public interface IBitmapResovler : IDisposable
{
    /// <summary>
    /// Gets the width of the bitmap.
    /// </summary>
    int Width { get; }

    /// <summary>
    /// Gets the height of the bitmap.
    /// </summary>
    int Height { get; }
}

/// <summary>
/// Represents a bitmap image containing raw byte data.
/// This class stores image data as a byte array without parsing its content.
/// </summary>
public sealed class BitmapDef : ImageDef
{
    /// <summary>
    /// Gets the raw byte data of the bitmap.
    /// </summary>
    public byte[] Data { get; }

    /// <inheritdoc/>
    public override int Width => Resolver?.Width ?? 0;

    /// <inheritdoc/>
    public override int Height => Resolver?.Height ?? 0;

    /// <summary>
    /// Gets or sets the bitmap resolver that provides width and height information.
    /// </summary>
    public IBitmapResovler Resolver { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BitmapDef"/> class with the specified data.
    /// </summary>
    /// <param name="data">The raw byte data of the bitmap.</param>
    public BitmapDef(byte[] data)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Bitmap [Width={Width}, Height={Height}, DataLength={Data.Length}]";
    }

    /// <summary>
    /// Releases all resources used by the <see cref="BitmapDef"/> and its resolver.
    /// </summary>
    public override void Dispose()
    {
        base.Dispose();

        Resolver?.Dispose();
        Resolver = null;
    }

    /// <summary>
    /// Creates a <see cref="BitmapDef"/> from the specified file path.
    /// </summary>
    /// <param name="path">The path to the image file.</param>
    /// <returns>A new <see cref="BitmapDef"/> containing the file's byte data.</returns>
    public static BitmapDef FromPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentNullException(nameof(path));

        return new BitmapDef(File.ReadAllBytes(path));
    }

    /// <summary>
    /// Saves the bitmap data to the specified file path.
    /// </summary>
    /// <param name="path">The path where the bitmap will be saved.</param>
    public void Save(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentNullException(nameof(path));

        File.WriteAllBytes(path, Data);
    }

    /// <summary>
    /// Saves the bitmap data to the specified stream.
    /// </summary>
    /// <param name="stream">The stream to which the bitmap data will be written.</param>
    public override void Save(Stream stream)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        stream.Write(Data, 0, Data.Length);
    }
}