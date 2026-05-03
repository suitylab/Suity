using System;

namespace Suity.Drawing;

/// <summary>
/// Specifies the format of the image.
/// </summary>
public sealed class ImageFormat
{
    /// <summary>
    /// Gets the GUID for this <see cref="ImageFormat"/>.
    /// </summary>
    public Guid Guid { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageFormat"/> class with the specified GUID.
    /// </summary>
    /// <param name="guid">The GUID for the image format.</param>
    public ImageFormat(Guid guid)
    {
        Guid = guid;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"ImageFormat [Guid={Guid}]";
    }

    /// <summary>
    /// Gets the Bitmap (BMP) image format.
    /// </summary>
    public static ImageFormat Bmp => new ImageFormat(new Guid("b96b3cab-0728-11d3-9d7b-0000f81ef32e"));

    /// <summary>
    /// Gets the Enhanced Metafile (EMF) image format.
    /// </summary>
    public static ImageFormat Emf => new ImageFormat(new Guid("b96b3cac-0728-11d3-9d7b-0000f81ef32e"));

    /// <summary>
    /// Gets the Exchangeable Image Format (Exif) image format.
    /// </summary>
    public static ImageFormat Exif => new ImageFormat(new Guid("b96b3cb2-0728-11d3-9d7b-0000f81ef32e"));

    /// <summary>
    /// Gets the Graphics Interchange Format (GIF) image format.
    /// </summary>
    public static ImageFormat Gif => new ImageFormat(new Guid("b96b3cb0-0728-11d3-9d7b-0000f81ef32e"));

    /// <summary>
    /// Gets the Icon (ICO) image format.
    /// </summary>
    public static ImageFormat Icon => new ImageFormat(new Guid("b96b3cb5-0728-11d3-9d7b-0000f81ef32e"));

    /// <summary>
    /// Gets the Joint Photographic Experts Group (JPEG) image format.
    /// </summary>
    public static ImageFormat Jpeg => new ImageFormat(new Guid("b96b3cae-0728-11d3-9d7b-0000f81ef32e"));

    /// <summary>
    /// Gets the Memory Bitmap image format.
    /// </summary>
    public static ImageFormat MemoryBmp => new ImageFormat(new Guid("b96b3caa-0728-11d3-9d7b-0000f81ef32e"));

    /// <summary>
    /// Gets the W3C Portable Network Graphics (PNG) image format.
    /// </summary>
    public static ImageFormat Png => new ImageFormat(new Guid("b96b3caf-0728-11d3-9d7b-0000f81ef32e"));

    /// <summary>
    /// Gets the Tagged Image File Format (TIFF) image format.
    /// </summary>
    public static ImageFormat Tiff => new ImageFormat(new Guid("b96b3cb1-0728-11d3-9d7b-0000f81ef32e"));

    /// <summary>
    /// Gets the Windows Metafile (WMF) image format.
    /// </summary>
    public static ImageFormat Wmf => new ImageFormat(new Guid("b96b3cad-0728-11d3-9d7b-0000f81ef32e"));
}
