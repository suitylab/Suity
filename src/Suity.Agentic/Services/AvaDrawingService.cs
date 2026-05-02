using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Services;

internal class AvaDrawingService : IDrawingService
{
    public static readonly AvaDrawingService Instance = new();

    private AvaDrawingService()
    {
    }

    public string[] GetInstalledFontNames()
    {
        return FontManager.Current.SystemFonts
        .Select(f => f.Name)
        .OrderBy(name => name)
        .ToArray();
    }

    public string? GetBestAvailableFont(params string[] fontNames)
    {
        var names = FontManager.Current.SystemFonts.Select(f => f.Name);

        HashSet<string> set = new(names, StringComparer.OrdinalIgnoreCase);
        foreach (var name in fontNames)
        {
            if (set.Contains(name))
            {
                return name;
            }
        }

        return null;
    }
}
