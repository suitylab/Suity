using System;
using System.Drawing;
using System.IO;

namespace Suity.Drawing;

/// <summary>
/// Represents an abstract image data structure.
/// </summary>
public abstract class ImageDef : IDisposable
{
    /// <summary>
    /// Gets the width of the image.
    /// </summary>
    public abstract int Width { get; }

    /// <summary>
    /// Gets the height of the image.
    /// </summary>
    public abstract int Height { get; }

    /// <summary>
    /// Gets the size of the image.
    /// </summary>
    public Size Size => new Size(Width, Height);

    /// <summary>
    /// Disposes of the image resources.
    /// </summary>
    public virtual void Dispose()
    {
    }

    public virtual void Save(Stream stream) { }
}
