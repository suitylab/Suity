using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Media.TextFormatting;
using SkiaSharp;
using Suity.Helpers;


namespace Suity.Controls;

public static class AvaDrawingHelper
{
    public readonly record struct TextCacheKey(
        string Text,
        string FontFamily,
        float FontSize,
        FontStyle Style,
        FontWeight Weight,
        TextAlignment Alignment = TextAlignment.Left,
        TextWrapping Wrapping = TextWrapping.NoWrap,
        double MaxWidth = double.PositiveInfinity,
        double MaxHeight = double.PositiveInfinity
    );



    // Define a TextLayout cache with capacity of 1000
    // Automatically calls Dispose() when items are removed
    public static readonly LRUCache<TextCacheKey, TextLayout> LayoutCache =
        new(1000, layout => layout.Dispose());


    [ThreadStatic]
    private static SKPaint? _sharedPaint;



    /// <summary>
    /// Draws rich single-line text with dynamic color support on a SkiaSharp canvas.
    /// </summary>
    public static void DrawRichSingleLineText(this SKCanvas canvas, string text, System.Drawing.Font font, System.Drawing.Brush brush, SKPoint origin, System.Drawing.StringAlignment alignment)
    {
        if (string.IsNullOrEmpty(text)) return;

        // 1. Style conversion
        var typeface = new Typeface(font.FontFamily.Name,
            font.Italic ? FontStyle.Italic : FontStyle.Normal,
            font.Bold ? FontWeight.Bold : FontWeight.Normal);

        // 2. Create Key (Note: No longer pass Foreground-related parameters, use default parameters for Alignment/Wrapping)
        // As long as text, typeface, fontSize remain unchanged, Layout can hit the cache
        var key = new TextCacheKey(
            text,
            typeface.FontFamily.Name,
            font.Size,
            typeface.Style,
            typeface.Weight
        );

        // 3. Get from cache or create TextLayout
        var textLayout = LayoutCache.GetOrCreate(key, () =>
        {
            return new TextLayout(
                text,
                typeface,
                font.Size,
                Brushes.Black, // Internal cache uses black as placeholder
                TextAlignment.Left,
                TextWrapping.NoWrap,
                maxWidth: double.PositiveInfinity
            );
        });

        if (textLayout.TextLines.Count > 0)
        {
            var line = textLayout.TextLines[0];

            // 4. Calculate offset
            float offsetX = alignment switch
            {
                System.Drawing.StringAlignment.Near => 0,
                System.Drawing.StringAlignment.Center => -(float)(line.Width / 2),
                System.Drawing.StringAlignment.Far => -(float)line.Width,
                _ => 0
            };

            // 5. Get real-time color from current animation
            // Convert System.Drawing.Brush to Skia's SKColor (or extract directly from SolidBrush)
            SKColor dynamicColor = SKColors.Black;
            if (brush is System.Drawing.SolidBrush solidBrush)
            {
                var c = solidBrush.Color;
                dynamicColor = new SKColor(c.R, c.G, c.B, c.A);
            }

            float currentX = origin.X + offsetX;
            float baselineY = origin.Y;

            // 6. Segment rendering
            foreach (var run in line.TextRuns)
            {
                if (run.Length <= 0 || run.Text.IsEmpty) continue;

                var props = run.Properties;
                if (props != null)
                {
                    var runPaint = GetConfiguredPaint(props);

                    // [Core optimization]: Apply real-time animation color
                    runPaint.Color = dynamicColor;

                    string runText = run.Text.ToString();
                    canvas.DrawText(runText, currentX, baselineY, runPaint);

                    // Accumulate width
                    currentX += runPaint.MeasureText(runText);
                }
            }
        }
    }

    /// <summary>
    /// Draws rich text within a rectangular area with dynamic color support.
    /// </summary>
    public static void DrawRichTextArea(this SKCanvas canvas, string text, System.Drawing.Font font, System.Drawing.Color color, System.Drawing.RectangleF rect, System.Drawing.StringAlignment alignment = System.Drawing.StringAlignment.Near)
    {
        if (string.IsNullOrEmpty(text)) return;

        // 1. Convert basic properties
        var typeface = new Typeface(font.FontFamily.Name,
            font.Italic ? FontStyle.Italic : FontStyle.Normal,
            font.Bold ? FontWeight.Bold : FontWeight.Normal);

        var textAlignment = ToAvaloniaAlignment(alignment);

        // 2. Build cache key -- [Note]: No longer includes color
        var key = new TextCacheKey(
            text,
            typeface.FontFamily.Name,
            font.Size,
            typeface.Style,
            typeface.Weight,
            textAlignment,
            TextWrapping.Wrap,
            rect.Width,
            rect.Height
        );

        // 3. Get from cache or create TextLayout (internal default uses black)
        var textLayout = LayoutCache.GetOrCreate(key, () =>
            new TextLayout(
                text,
                typeface,
                font.Size,
                Brushes.Black, // The color stored here doesn't matter anymore
                textAlignment,
                TextWrapping.Wrap,
                maxWidth: rect.Width,
                maxHeight: rect.Height
            )
        );

        // 4. Convert animation/dynamic color to Skia color
        var skOverrideColor = new SKColor(color.R, color.G, color.B, color.A);

        // 5. Call draw and pass dynamic color
        DrawRichTextArea(canvas, textLayout, new SKPoint(rect.X, rect.Y), new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom), skOverrideColor);
    }

    /// <summary>
    /// Draws rich text using a pre-built TextLayout with optional color override.
    /// </summary>
    public static void DrawRichTextArea(this SKCanvas canvas, TextLayout textLayout, SKPoint origin, SKRect constraintRect, SKColor? overrideColor = null)
    {
        if (textLayout == null) return;

        float currentY = (float)origin.Y;

        foreach (var line in textLayout.TextLines)
        {
            if (currentY > constraintRect.Bottom) break;

            float currentX = (float)origin.X;
            float baselineY = currentY + (float)line.Baseline;

            foreach (var run in line.TextRuns)
            {
                if (run.Length <= 0 || run.Text.IsEmpty) continue;

                var props = run.Properties;
                if (props != null)
                {
                    var runPaint = GetConfiguredPaint(props);

                    // [Key modification]: If override color is passed (e.g., animation color), ignore Layout internal color
                    if (overrideColor.HasValue)
                    {
                        runPaint.Color = overrideColor.Value;
                    }

                    string runTextPtr = run.Text.ToCleanString();
                    canvas.DrawText(runTextPtr, currentX, baselineY, runPaint);

                    float runWidth = runPaint.MeasureText(runTextPtr);
                    currentX += runWidth;
                }
            }
            currentY += (float)line.Height;
        }
    }

    /// <summary>
    /// Converts a ReadOnlyMemory of characters to a clean string, stripping trailing control characters.
    /// </summary>
    public static string ToCleanString(this ReadOnlyMemory<char> memory)
    {
        var span = memory.Span;
        int length = span.Length;

        // Search backwards, skip all control characters (CR, LF)
        while (length > 0 && char.IsControl(span[length - 1]))
        {
            length--;
        }

        if (length <= 0) return string.Empty;

        // ToString only reuses Substring when length hasn't changed and internal is string, otherwise creates new string
        // Here directly slice and convert to string
        return span.Slice(0, length).ToString();
    }

    /// <summary>
    /// Convert Avalonia's TextRunProperties to SkiaSharp's SKPaint
    /// </summary>
    private static SKPaint GetConfiguredPaint(TextRunProperties props)
    {
        // Add .Handle check to prevent unmanaged objects from holding references after being accidentally released
        if (_sharedPaint == null || _sharedPaint.Handle == nint.Zero)
        {
            _sharedPaint = new SKPaint
            {
                IsAntialias = true,
                SubpixelText = true,
                LcdRenderText = true,
                HintingLevel = SKPaintHinting.Normal
            };
        }

        // 1. Get cached font
        var tf = props.Typeface;
        var tfKey = new TypefaceCacheKey(tf.FontFamily.Name, (SKFontStyleWeight)tf.Weight, (SKFontStyleSlant)tf.Style);

        // Dirty check optimization: only reset when font actually changes, reducing cross-language call overhead
        var targetTf = SkiaSharpExtensions.GetCachedTypeFace(tfKey);
        if (_sharedPaint.Typeface != targetTf)
        {
            _sharedPaint.Typeface = targetTf;
        }

        // 2. Set properties
        float targetSize = (float)props.FontRenderingEmSize;
        if (Math.Abs(_sharedPaint.TextSize - targetSize) > 0.01f)
        {
            _sharedPaint.TextSize = targetSize;
        }

        return _sharedPaint;
    }

    /// <summary>
    /// Converts a System.Drawing brush to an Avalonia IBrush.
    /// </summary>
    public static IBrush ToAvaloniaBrush(System.Drawing.Brush drawingBrush)
    {
        // Check if it's the most common solid brush
        if (drawingBrush is System.Drawing.SolidBrush solidBrush)
        {
            var sColor = solidBrush.Color;
            var avColor = Avalonia.Media.Color.FromArgb(sColor.A, sColor.R, sColor.G, sColor.B);
            return new ImmutableSolidColorBrush(avColor);
        }

        // If gradient or other complex brushes are passed, currently fallback to black
        // System.Drawing.Drawing2D.LinearGradientBrush etc. require more complex parsing logic
        return Brushes.Black;
    }

    /// <summary>
    /// Converts a System.Drawing string alignment to an Avalonia TextAlignment.
    /// </summary>
    public static TextAlignment ToAvaloniaAlignment(System.Drawing.StringAlignment alignment)
    {
        return alignment switch
        {
            System.Drawing.StringAlignment.Near => TextAlignment.Left,
            System.Drawing.StringAlignment.Center => TextAlignment.Center,
            System.Drawing.StringAlignment.Far => TextAlignment.Right,
            // StringAlignment also has baseline alignment, but in TextAlignment it usually maps to Left/Right
            _ => TextAlignment.Left
        };
    }

    /// <summary>
    /// Measures the size of text as a single line, ignoring line breaks.
    /// </summary>
    public static System.Drawing.SizeF MeasureSingleLineString(string text, System.Drawing.Font font)
    {
        var typeface = new Typeface(font.FontFamily.Name,
            font.Italic ? FontStyle.Italic : FontStyle.Normal,
            font.Bold ? FontWeight.Bold : FontWeight.Normal);

        if (string.IsNullOrEmpty(text))
        {
            text = "";
        }

        var key = new TextCacheKey(text, typeface.FontFamily.Name, font.Size,
                                   typeface.Style, typeface.Weight,
                                   TextAlignment.Left, TextWrapping.NoWrap, double.PositiveInfinity);

        var textLayout = LayoutCache.GetOrCreate(key, () =>
            new TextLayout(text, typeface, font.Size, Brushes.Black,
                           TextAlignment.Left, TextWrapping.NoWrap, maxWidth: double.PositiveInfinity)
        );

        return new System.Drawing.SizeF((float)textLayout.Width, (float)textLayout.Height);
    }

    /// <summary>
    /// Measures the size of text within a constrained width.
    /// </summary>
    public static System.Drawing.SizeF MeasureTextArea(string text, System.Drawing.Font font, float maxLineWidth)
    {
        var typeface = new Typeface(font.FontFamily.Name,
            font.Italic ? FontStyle.Italic : FontStyle.Normal,
            font.Bold ? FontWeight.Bold : FontWeight.Normal);

        if (string.IsNullOrEmpty(text))
        {
            text = "";
        }

        // Construct a Key for measurement
        var key = new TextCacheKey(text, typeface.FontFamily.Name, font.Size,
                                   typeface.Style, typeface.Weight,
                                   TextAlignment.Left, TextWrapping.Wrap, maxLineWidth);

        // Reuse LayoutCache
        var textLayout = LayoutCache.GetOrCreate(key, () =>
            new TextLayout(text, typeface, font.Size, Brushes.Black,
                           TextAlignment.Left, TextWrapping.Wrap, maxWidth: maxLineWidth)
        );

        return new System.Drawing.SizeF((float)textLayout.Width, (float)textLayout.Height);
    }
}
