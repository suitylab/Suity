using Suity.Views.Menu;

namespace Suity.Editor.Services;

/// <summary>
/// Service interface for menu operations.
/// </summary>
public interface IMenuService
{
    /// <summary>
    /// Adds menu items from plugins to the main menu
    /// </summary>
    /// <param name="rootMenu">The root menu command to prepare.</param>
    void PrepareMenu(RootMenuCommand rootMenu);
}