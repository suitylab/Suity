using Avalonia.Media;
using SkiaSharp;

namespace Suity.Helpers;

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

	/// <summary>
	/// Converts a <see cref="System.Drawing.Image"/> to a <see cref="SKImage"/>.
	/// </summary>
	/// <param name="bitmap">The System.Drawing image.</param>
	/// <returns>A SkiaSharp image.</returns>
	public static SKImage? ToSKImage(this System.Drawing.Image drawingImage)
	{
        if (drawingImage is not System.Drawing.Bitmap bmp || bmp.Data is null)
        {
            return null;
        }

        var resolver = bmp.Resolver as AvaBitmapResolver;
        if (resolver?.SKImg is { } skImg)
        {
            return skImg;
        }

		using SKData data = SKData.CreateCopy(bmp.Data);
        skImg = SKImage.FromEncodedData(data);

        if (resolver is null)
        {
            resolver = new AvaBitmapResolver(skImg);
            bmp.Resolver = resolver;
        }

        resolver.SKImg = skImg;

        return skImg;
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
