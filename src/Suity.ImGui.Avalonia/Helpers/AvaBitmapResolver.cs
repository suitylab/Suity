using Avalonia.Media.Imaging;
using SkiaSharp;
using Suity.Drawing;

namespace Suity.Helpers;

internal class AvaBitmapResolver : IBitmapResovler
{
    public AvaBitmapResolver(Bitmap avaBmp)
    {
        AvaBmp = avaBmp ?? throw new ArgumentNullException(nameof(avaBmp));

        Width = (int)avaBmp.Size.Width;
        Height = (int)avaBmp.Size.Height;
    }

    public AvaBitmapResolver(SKImage skImg)
    {
        SKImg = skImg ?? throw new ArgumentNullException(nameof(skImg));

        Width = skImg.Width;
        Height = skImg.Height;
    }

    public Bitmap? AvaBmp { get; set; }

    public SKImage? SKImg { get; set; }

    public int Width { get; }

    public int Height { get; }

    public void Dispose()
    {
        AvaBmp?.Dispose();
        AvaBmp = null;

        SKImg?.Dispose();
        SKImg = null;
    }
}
