using static Suity.Helpers.GlobalLocalizer;
using Suity.Collections;
using Suity.Helpers;
using Suity.Reflecting;
using Suity.Views;
using Suity.Views.Menu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Services;

/// <summary>
/// Manages editor menu registration and hierarchical menu building.
/// </summary>
internal sealed class MenuService : IMenuService
{
    /// <summary>
    /// Singleton instance of the menu service.
    /// </summary>
    public static readonly MenuService Instance = new();

    private bool _init = false;

    private readonly UniqueMultiDictionary<string, Type> _menuTypes = new();
    private readonly HashSet<MenuCommand> _preparedMenus = [];

    private MenuService()
    {
        EditorRexes.EditorBeforeAwake.AddActionListener(Initialize);
    }

    /// <summary>
    /// Initializes the menu service by scanning and registering all menu command types.
    /// </summary>
    private void Initialize()
    {
        if (_init)
        {
            return;
        }
        _init = true;

        EditorServices.SystemLog.AddLog("MenuManager Initializing...");
        EditorServices.SystemLog.PushIndent();

        foreach (var type in typeof(MenuCommand).GetAvailableClassTypes())
        {
            if (typeof(RootMenuCommand).IsAssignableFrom(type))
            {
                continue;
            }

            var attrs = type.GetAttributesCached<InsertIntoAttribute>();
            foreach (var attr in attrs)
            {
                if (string.IsNullOrWhiteSpace(attr?.Position))
                {
                    continue;
                }

                EditorServices.SystemLog.AddLog($"Register menu : ${attr.Position} for ${type.Name}");

                _menuTypes.Add(attr.Position, type);
            }
        }

        EditorServices.SystemLog.PopIndent();
        EditorServices.SystemLog.AddLog("MenuManager Initialized.");
    }

    /// <summary>
    /// Prepares a root menu by building its hierarchical structure with child commands.
    /// </summary>
    /// <param name="rootMenu">The root menu command to prepare.</param>
    public void PrepareMenu(RootMenuCommand rootMenu)
    {
        P_PrepareMenu(rootMenu, rootMenu.Id);
    }

    /// <summary>
    /// Recursively prepares a menu and its children by adding registered command types.
    /// </summary>
    /// <param name="menu">The menu to prepare.</param>
    /// <param name="key">The hierarchical key for menu type lookup.</param>
    private void P_PrepareMenu(MenuCommand menu, string key)
    {
        if (menu is null)
        {
            return;
        }

        if (_preparedMenus.Contains(menu))
        {
            return;
        }

        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        _preparedMenus.Add(menu);

        foreach (var type in _menuTypes[key])
        {
            try
            {
                if (typeof(RootMenuCommand).IsAssignableFrom(type))
                {
                    continue;
                }

                MenuCommand childMenu = (MenuCommand)type.CreateInstanceOf();

                menu.AddCommand(childMenu);
            }
            catch (Exception err)
            {
                err.LogError(L("Failed to create menu"));
            }
        }

        foreach (var childMenu in menu.ChildCommands.OfType<MenuCommand>())
        {
            P_PrepareMenu(childMenu, $"{key}/{childMenu.Id}");
        }
    }
}
