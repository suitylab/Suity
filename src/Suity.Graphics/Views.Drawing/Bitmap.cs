using System;

namespace Suity.Views.Drawing;

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
    public override int Width { get; }

    /// <inheritdoc/>
    public override int Height { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Bitmap"/> class with the specified dimensions and data.
    /// </summary>
    /// <param name="width">The width of the bitmap.</param>
    /// <param name="height">The height of the bitmap.</param>
    /// <param name="data">The raw byte data.</param>
    public Bitmap(int width, int height, byte[] data)
    {
        Width = width;
        Height = height;
        Data = data ?? throw new ArgumentNullException(nameof(data));
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Bitmap [Width={Width}, Height={Height}, DataLength={Data.Length}]";
    }
}
