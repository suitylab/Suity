using System.IO;

namespace System.Drawing;

public interface IBitmapResovler : IDisposable
{
    int Width { get; }
    int Height { get; }
}

/// <summary>
/// Represents a bitmap image containing raw byte data.
/// This class stores image data as a byte array without parsing its content.
/// </summary>
public sealed class Bitmap : Image
{
    /// <summary>
    /// Gets the raw byte data of the bitmap.
    /// </summary>
    public byte[] Data { get; }

    /// <inheritdoc/>
    public override int Width => Resolver?.Width ?? 0;

    /// <inheritdoc/>
    public override int Height => Resolver?.Height ?? 0;

    public IBitmapResovler Resolver { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Bitmap"/> class with the specified dimensions and data.
    /// </summary>
    /// <param name="width">The width of the bitmap.</param>
    /// <param name="height">The height of the bitmap.</param>
    /// <param name="data">The raw byte data.</param>
    public Bitmap(byte[] data)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Bitmap [Width={Width}, Height={Height}, DataLength={Data.Length}]";
    }

    public override void Dispose()
    {
        base.Dispose();

        Resolver?.Dispose();
        Resolver = null;
    }

    /// <summary>
    /// Creates a <see cref="Bitmap"/> from the specified file path.
    /// </summary>
    /// <param name="path">The path to the image file.</param>
    /// <returns>A new <see cref="Bitmap"/> containing the file's byte data.</returns>
    public static Bitmap FromPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentNullException(nameof(path));

        return new Bitmap(File.ReadAllBytes(path));
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
