using System;
using System.Collections.Generic;
using System.Text;

namespace Suity.Editor.Services;

internal class CliDrawingService : IDrawingService
{
    public static CliDrawingService Instance { get; } = new();

    public string GetBestAvailableFont(params string[] fontNames) => null;

    public string[] GetInstalledFontNames() => [];
}
