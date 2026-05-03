using Avalonia.Media.Imaging;
using SkiaSharp;
using Suity.Drawing;

namespace Suity.Helpers;

internal class AvaBitmapResolver : IBitmapResovler
{
    public AvaBitmapResolver()
    {
    }

    public AvaBitmapResolver(Bitmap avaloniaCache)
    {
        SetAvaloniaCache(avaloniaCache);
    }

    public AvaBitmapResolver(SKImage skiaCache)
    {
        SetSkiaCache(skiaCache);
    }

    public void SetAvaloniaCache(Bitmap avaloniaCache)
    {
        AvaloniaCache = avaloniaCache ?? throw new ArgumentNullException(nameof(avaloniaCache));

        Width = (int)avaloniaCache.Size.Width;
        Height = (int)avaloniaCache.Size.Height;
    }

    public void SetSkiaCache(SKImage skiaCache)
    {
        SkiaCache = skiaCache ?? throw new ArgumentNullException(nameof(skiaCache));

        Width = skiaCache.Width;
        Height = skiaCache.Height;
    }


    public Bitmap? AvaloniaCache { get; private set; }

    public SKImage? SkiaCache { get; private set; }

    public int Width { get; private set; }

    public int Height { get; private set; }

    public void Dispose()
    {
        AvaloniaCache?.Dispose();
        AvaloniaCache = null;

        SkiaCache?.Dispose();
        SkiaCache = null;
    }
}
