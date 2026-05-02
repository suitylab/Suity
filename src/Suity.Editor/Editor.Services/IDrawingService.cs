using System.Drawing;
namespace Suity.Editor.Services;

public interface IDrawingService
{
    string[] GetInstalledFontNames();

    string GetBestAvailableFont(params string[] fontNames);
}
