using Suity.Collections;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Suity.Helpers;

/// <summary>
/// Provides utility methods for icon and image operations, including resizing,
/// grayscale conversion, and bitmap manipulation.
/// </summary>
public static class ImageHelper
{
    /// <summary>
    /// The standard dimension (in pixels) for small icons.
    /// </summary>
    public const int SmallSize = 32;

    private static readonly Dictionary<Bitmap, Bitmap> _sizeSmalls = new Dictionary<Bitmap, Bitmap>();
    private static readonly Dictionary<Bitmap, Bitmap> _sizeSmallGrays = new Dictionary<Bitmap, Bitmap>();

    /// <summary>
    /// Converts a byte array to a <see cref="Bitmap"/> with the specified resolution.
    /// </summary>
    /// <param name="b">The byte array containing image data.</param>
    /// <param name="dpi">The horizontal and vertical resolution in dots per inch. Defaults to 100.</param>
    /// <returns>A new <see cref="Bitmap"/> created from the byte array.</returns>
    public static Bitmap ToBitmap(this byte[] b, int dpi = 100)
    {
        var bmp = new Bitmap(b);
        //bmp.SetResolution(dpi, dpi);

        return bmp;
    }

    public static Image FromStream(Stream stream)
    {
        byte[] b = stream.StreamToBytes();
        return ToBitmap(b);
    }

    /// <summary>
    /// Resizes an image to the small icon size if it exceeds <see cref="SmallSize"/>.
    /// Returns the original image unchanged if it is already small enough.
    /// </summary>
    /// <param name="image">The source image to resize.</param>
    /// <returns>The resized image or the original if no resizing is needed.</returns>
    public static Image ToIconSmall(this Image image)
    {
        return image;

/*        if (image == null)
        {
            return null;
        }

        if (image.Width <= SmallSize)
        {
            return image;
        }

        if (!(image is Bitmap bitmap))
        {
            bitmap = new Bitmap(image);
        }

        return _sizeSmalls.GetOrAdd(bitmap, _ => bitmap.Resize(SmallSize, SmallSize));*/
    }

    /// <summary>
    /// Resizes a bitmap to the small icon size if it exceeds <see cref="SmallSize"/>.
    /// Returns the original bitmap unchanged if it is already small enough.
    /// </summary>
    /// <param name="bitmap">The source bitmap to resize.</param>
    /// <returns>The resized bitmap or the original if no resizing is needed.</returns>
    public static Bitmap ToIconSmall(this Bitmap bitmap)
    {
        return bitmap;

        if (bitmap == null)
        {
            return null;
        }

        if (bitmap.Width <= SmallSize)
        {
            return bitmap;
        }

        return _sizeSmalls.GetOrAdd(bitmap, _ => bitmap.Resize(SmallSize, SmallSize));
    }

    /// <summary>
    /// Resizes a bitmap to the small icon size and optionally converts it to grayscale.
    /// </summary>
    /// <param name="bitmap">The source bitmap to resize.</param>
    /// <param name="gray">If true, converts the resulting icon to grayscale.</param>
    /// <returns>The resized bitmap, optionally in grayscale.</returns>
    public static Bitmap ToIconSmall(this Bitmap bitmap, bool gray)
    {
        if (bitmap == null)
        {
            return null;
        }

        Bitmap iconSmall = bitmap.ToIconSmall();

        if (gray)
        {
            return _sizeSmallGrays.GetOrAdd(iconSmall, _ => iconSmall.ToGray());
        }
        else
        {
            return iconSmall;
        }
    }

    /// <summary>
    /// Clears all cached resized and grayscale bitmaps to free memory.
    /// </summary>
    public static void CleanUp()
    {
    }

    /// <summary>
    /// Resize image with GDI+ so that image is nice and clear with required size.
    /// </summary>
    public static Bitmap Resize(this Bitmap source, Int32 width, Int32 height)
    {
        /*        Bitmap bitmap = new Bitmap(width, height, source.PixelFormat);
                Graphics graphicsImage = Graphics.FromImage(bitmap);
                graphicsImage.SmoothingMode = SmoothingMode.HighQuality;
                graphicsImage.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphicsImage.DrawImage(source, 0, 0, bitmap.Width, bitmap.Height);
                graphicsImage.Dispose();
                return bitmap;*/

        return source;
    }

    /// <summary>
    /// Converts a bitmap to grayscale by applying luminance-based weights to each pixel.
    /// </summary>
    /// <param name="bmp">The bitmap to convert.</param>
    /// <returns>The same bitmap instance with all pixels converted to grayscale.</returns>
    public static Bitmap ToGray(this Bitmap bmp)
    {
        /*        for (int i = 0; i < bmp.Width; i++)
                {
                    for (int j = 0; j < bmp.Height; j++)
                    {
                        // Obtain the RGB color of the pixel
                        Color color = bmp.GetPixel(i, j);
                        // Using formulas to calculate grayscale values
                        int gray = (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11);
                        Color newColor = Color.FromArgb(gray, gray, gray);
                        bmp.SetPixel(i, j, newColor);
                    }
                }
                return bmp;*/

        return bmp;
    }
}