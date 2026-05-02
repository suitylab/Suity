using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using SkiaSharp;

namespace Suity.Controls;

/// <summary>
/// Base Avalonia control that provides SkiaSharp rendering through a custom draw operation.
/// </summary>
public class AvaSKBitmapControl : Control
{
    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == Visual.ClipToBoundsProperty)
        {
            InvalidateVisual();
        }
    }

    protected virtual void OnSKDraw(ImmediateDrawingContext? context, SKCanvas canvas, Rect bounds)
    {
    }

    public void Invalidate()
    {
        this.InvalidateVisual();
    }

    public void Invalidate(Rect rect)
    {
        this.InvalidateVisual();
    }
}