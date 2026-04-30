using Dock.Model.Core;
using Suity.Views.Gui;

namespace Suity.Editor.Controls;

public static class DockExtensions
{
    public static DockMode? ToDockMode(this DockHint dockHint)
    {
        switch (dockHint)
        {
            case DockHint.Document:
                return DockMode.Center;

            case DockHint.Float:
                return DockMode.Center;

            case DockHint.Top:
                return DockMode.Top;

            case DockHint.Left:
                return DockMode.Left;

            case DockHint.Bottom:
                return DockMode.Bottom;

            case DockHint.Right:
                return DockMode.Right;

            default:
                return null;
        }
    }
}
