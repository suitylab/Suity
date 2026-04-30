using SkiaSharp;

namespace Suity.Controls;

/// <summary>
/// Provides utility methods and extension methods for SkiaSharp type conversions.
/// </summary>
public static class SkiaSharpUtils
{
	private static readonly Lazy<bool> isValidEnvironment = new Lazy<bool>(() =>
	{
		try
		{
			// test an operation that requires the native library
			SKPMColor.PreMultiply(SKColors.Black);
			return true;
		}
		catch (DllNotFoundException)
		{
			// If we can't load the native library,
			// we may be in some designer.
			// We can make this assumption since any other member will fail
			// at some point in the draw operation.
			return false;
		}
	});

	/// <summary>
	/// Gets a value indicating whether the SkiaSharp environment is valid for rendering.
	/// </summary>
	internal static bool IsValidEnvironment => isValidEnvironment.Value;

    /// <summary>
    /// Converts a <see cref="System.Drawing.PointF"/> to a <see cref="SKPoint"/>.
    /// </summary>
    public static SKPoint ToSKPoint(this System.Drawing.PointF point) 
		=> new SKPoint(point.X, point.Y);

    /// <summary>
    /// Converts a <see cref="System.Drawing.Point"/> to a <see cref="SKPointI"/>.
    /// </summary>
    public static SKPointI ToSKPoint(this System.Drawing.Point point) 
		=> new SKPointI(point.X, point.Y);

    /// <summary>
    /// Converts a <see cref="SKPoint"/> to a <see cref="System.Drawing.PointF"/>.
    /// </summary>
    public static System.Drawing.PointF ToDrawingPoint(this SKPoint point) 
		=> new System.Drawing.PointF(point.X, point.Y);

    /// <summary>
    /// Converts a <see cref="SKPointI"/> to a <see cref="System.Drawing.Point"/>.
    /// </summary>
    public static System.Drawing.Point ToDrawingPoint(this SKPointI point) 
		=> new System.Drawing.Point(point.X, point.Y);

    // System.Drawing.Rectangle*

    /// <summary>
    /// Converts a <see cref="System.Drawing.RectangleF"/> to a <see cref="SKRect"/>.
    /// </summary>
    public static SKRect ToSKRect(this System.Drawing.RectangleF rect) 
		=> new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);

    /// <summary>
    /// Converts a <see cref="System.Drawing.RectangleF"/> to a <see cref="SKRect"/> with integer coordinates.
    /// </summary>
    public static SKRect ToSKRectToInt(this System.Drawing.RectangleF rect) 
		=> new SKRect((int)rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Bottom);

    /// <summary>
    /// Converts a <see cref="System.Drawing.Rectangle"/> to a <see cref="SKRect"/>.
    /// </summary>
    public static SKRect ToSKRect(this System.Drawing.Rectangle rect) 
		=> new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);

    /// <summary>
    /// Converts a <see cref="System.Drawing.RectangleF"/> to a <see cref="SKRectI"/>.
    /// </summary>
    public static SKRectI ToSKRectI(this System.Drawing.RectangleF rect)
		=> new SKRectI((int)rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Bottom);

    /// <summary>
    /// Converts a <see cref="System.Drawing.Rectangle"/> to a <see cref="SKRectI"/>.
    /// </summary>
    public static SKRectI ToSKRectI(this System.Drawing.Rectangle rect) 
		=> new SKRectI(rect.Left, rect.Top, rect.Right, rect.Bottom);

    /// <summary>
    /// Converts a <see cref="SKRect"/> to a <see cref="System.Drawing.RectangleF"/>.
    /// </summary>
    public static System.Drawing.RectangleF ToDrawingRect(this SKRect rect) 
		=> System.Drawing.RectangleF.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);

    /// <summary>
    /// Converts a <see cref="SKRectI"/> to a <see cref="System.Drawing.Rectangle"/>.
    /// </summary>
    public static System.Drawing.Rectangle ToDrawingRect(this SKRectI rect) 
		=> System.Drawing.Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);

    // System.Drawing.Size*

    /// <summary>
    /// Converts a <see cref="System.Drawing.SizeF"/> to a <see cref="SKSize"/>.
    /// </summary>
    public static SKSize ToSKSize(this System.Drawing.SizeF size) 
		=> new SKSize(size.Width, size.Height);

    /// <summary>
    /// Converts a <see cref="System.Drawing.Size"/> to a <see cref="SKSizeI"/>.
    /// </summary>
    public static SKSizeI ToSKSize(this System.Drawing.Size size) 
		=> new SKSizeI(size.Width, size.Height);

    /// <summary>
    /// Converts a <see cref="SKSize"/> to a <see cref="System.Drawing.SizeF"/>.
    /// </summary>
    public static System.Drawing.SizeF ToDrawingSize(this SKSize size) 
		=> new System.Drawing.SizeF(size.Width, size.Height);

    /// <summary>
    /// Converts a <see cref="SKSizeI"/> to a <see cref="System.Drawing.Size"/>.
    /// </summary>
    public static System.Drawing.Size ToDrawingSize(this SKSizeI size) 
		=> new System.Drawing.Size(size.Width, size.Height);

    // System.Drawing.Image

    /// <summary>
    /// Converts a <see cref="SKPicture"/> to a <see cref="System.Drawing.Image"/> with specified dimensions.
    /// </summary>
    /// <param name="picture">The SkiaSharp picture.</param>
    /// <param name="dimensions">The target dimensions.</param>
    /// <returns>A System.Drawing.Image representation.</returns>
    public static System.Drawing.Image ToBitmap(this SKPicture picture, SKSizeI dimensions)
	{
        using var image = SKImage.FromPicture(picture, dimensions);
        return image.ToBitmap();
    }

	/// <summary>
	/// Converts a <see cref="SKImage"/> to a <see cref="System.Drawing.Image"/>.
	/// </summary>
	/// <param name="skiaImage">The SkiaSharp image.</param>
	/// <returns>A System.Drawing.Image representation.</returns>
	public static System.Drawing.Image ToBitmap(this SKImage skiaImage)
	{
		// TODO: maybe keep the same color types where we can, instead of just going to the platform default

		var bitmap = new System.Drawing.Bitmap(skiaImage.Width, skiaImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
		var data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, bitmap.PixelFormat);

		// copy
		using (var pixmap = new SKPixmap(new SKImageInfo(data.Width, data.Height), data.Scan0, data.Stride))
		{
			skiaImage.ReadPixels(pixmap, 0, 0);
		}

		bitmap.UnlockBits(data);
		return bitmap;
	}

	/// <summary>
	/// Converts a <see cref="SKBitmap"/> to a <see cref="System.Drawing.Image"/>.
	/// </summary>
	/// <param name="skiaBitmap">The SkiaSharp bitmap.</param>
	/// <returns>A System.Drawing.Image representation.</returns>
	public static System.Drawing.Image ToBitmap(this SKBitmap skiaBitmap)
	{
        using var pixmap = skiaBitmap.PeekPixels();
        using var image = SKImage.FromPixels(pixmap);
        var bmp = image.ToBitmap();
        GC.KeepAlive(skiaBitmap);
        return bmp;
    }

	/// <summary>
	/// Converts a <see cref="SKPixmap"/> to a <see cref="System.Drawing.Image"/>.
	/// </summary>
	/// <param name="pixmap">The SkiaSharp pixmap.</param>
	/// <returns>A System.Drawing.Image representation.</returns>
	public static System.Drawing.Image ToBitmap(this SKPixmap pixmap)
	{
        using var image = SKImage.FromPixels(pixmap);
        return image.ToBitmap();
    }

	/// <summary>
	/// Converts a <see cref="System.Drawing.Image"/> to a <see cref="SKBitmap"/>.
	/// </summary>
	/// <param name="bitmap">The System.Drawing image.</param>
	/// <returns>A SkiaSharp bitmap.</returns>
	public static SKBitmap ToSKBitmap(this System.Drawing.Image bitmap)
	{
		// TODO: maybe keep the same color types where we can, instead of just going to the platform default

		var info = new SKImageInfo(bitmap.Width, bitmap.Height);
		var skiaBitmap = new SKBitmap(info);
		using (var pixmap = skiaBitmap.PeekPixels())
		{
			bitmap.ToSKPixmap(pixmap);
		}
		return skiaBitmap;
	}

	/// <summary>
	/// Converts a <see cref="System.Drawing.Image"/> to a <see cref="SKImage"/>.
	/// </summary>
	/// <param name="bitmap">The System.Drawing image.</param>
	/// <returns>A SkiaSharp image.</returns>
	public static SKImage ToSKImage(this System.Drawing.Image bitmap)
	{
		// TODO: maybe keep the same color types where we can, instead of just going to the platform default

		var info = new SKImageInfo(bitmap.Width, bitmap.Height);
		var image = SKImage.Create(info);
		using (var pixmap = image.PeekPixels())
		{
			bitmap.ToSKPixmap(pixmap);
		}
		return image;
	}

	/// <summary>
	/// Converts a <see cref="System.Drawing.Image"/> to a <see cref="SKPixmap"/>.
	/// </summary>
	/// <param name="bitmap">The System.Drawing image.</param>
	/// <param name="pixmap">The target SkiaSharp pixmap to write pixels into.</param>
	public static void ToSKPixmap(this System.Drawing.Image bitmap, SKPixmap pixmap)
	{
		// TODO: maybe keep the same color types where we can, instead of just going to the platform default

		if (pixmap.ColorType == SKImageInfo.PlatformColorType)
		{
			var info = pixmap.Info;
			using (var tempBitmap = new System.Drawing.Bitmap(info.Width, info.Height, info.RowBytes, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, pixmap.GetPixels()))
			using (var gr = System.Drawing.Graphics.FromImage(tempBitmap))
			{
				// Clear graphic to prevent display artifacts with transparent bitmaps					
				gr.Clear(System.Drawing.Color.Transparent);
				
				gr.DrawImageUnscaled(bitmap, 0, 0);
				//gr.DrawImage(bitmap, 0, 0);
			}
		}
		else
		{
			// we have to copy the pixels into a format that we understand
			// and then into a desired format
			// TODO: we can still do a bit more for other cases where the color types are the same
			using (var tempImage = bitmap.ToSKImage())
			{
				tempImage.ReadPixels(pixmap, 0, 0);
			}
		}
	}

    // System.Drawing.Color

    /// <summary>
    /// Converts a <see cref="System.Drawing.Color"/> to a <see cref="SKColor"/>.
    /// </summary>
    public static SKColor ToSKColor(this System.Drawing.Color color) 
		=> (SKColor)(uint)color.ToArgb();

    /// <summary>
    /// Converts a <see cref="SKColor"/> to a <see cref="System.Drawing.Color"/>.
    /// </summary>
    public static System.Drawing.Color ToDrawingColor(this SKColor color) 
		=> System.Drawing.Color.FromArgb((int)(uint)color);

}
