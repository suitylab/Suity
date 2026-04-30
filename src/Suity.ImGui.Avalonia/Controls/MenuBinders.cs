using Avalonia.Controls;
using Suity.Views.Menu;
using System;
using System.Collections.Generic;
using System.Text;

namespace Suity.Controls;

/// <summary>
/// Binds root menu commands to Avalonia Menu controls.
/// </summary>
public class AvaMainMenuBinder
{
    /// <summary>
    /// Gets the default instance of the main menu binder.
    /// </summary>
    public static AvaMainMenuBinder Default { get; } = new();

    private readonly Dictionary<RootMenuCommand, Menu> _menus = [];
    private readonly HashSet<Type> _selectionTypes = [];

    private AvaMainMenuBinder()
    {
    }

    /// <summary>
    /// Ensures a Menu instance exists for the specified command and completes view binding.
    /// </summary>
    /// <param name="menuCommand">The root menu command.</param>
    /// <returns>The Avalonia Menu control.</returns>
    public Menu EnsureMainMenu(RootMenuCommand menuCommand)
    {
        if (menuCommand is null) throw new ArgumentNullException(nameof(menuCommand));

        if (!_menus.TryGetValue(menuCommand, out var menu))
        {
            menu = new Menu();
            // Bind using custom extension method, using Menu.Items as container
            menuCommand.SetupContainerView(new AvaMenuItemView(menu.Items));
            _menus[menuCommand] = menu;
        }

        return menu;
    }

    /// <summary>
    /// Prepares the main menu and updates state based on the selection context.
    /// </summary>
    /// <param name="menuCommand">The root menu command.</param>
    /// <param name="selection">The current selection items.</param>
    /// <returns>The prepared Menu, or null if the command is not visible.</returns>
    public Menu? PrepareMainMenu(RootMenuCommand menuCommand, IEnumerable<object> selection = null)
    {
        var menu = EnsureMainMenu(menuCommand);
        var ary = selection?.ToArray() ?? Array.Empty<object>();

        _selectionTypes.Clear();
        foreach (var item in ary)
        {
            if (item != null) _selectionTypes.Add(item.GetType());
        }

        // Apply logic
        menuCommand.ApplySelection(ary);
        menuCommand.UpdateView();
        menuCommand.PopUp(ary.Length, _selectionTypes, null, ary);

        return menuCommand.Visible ? menu : null;
    }
}

/// <summary>
/// Binds root menu commands to Avalonia ContextMenu controls.
/// </summary>
public class AvaContextMenuBinder
{
    /// <summary>
    /// Gets the default instance of the context menu binder.
    /// </summary>
    public static AvaContextMenuBinder Default { get; } = new();

    private readonly Dictionary<RootMenuCommand, ContextMenu> _menus = [];
    private readonly HashSet<Type> _selectionTypes = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaContextMenuBinder"/> class.
    /// </summary>
    public AvaContextMenuBinder()
    {
    }

    /// <summary>
    /// Ensures a ContextMenu instance exists for the specified command and completes view binding.
    /// </summary>
    /// <param name="menuCommand">The root menu command.</param>
    /// <returns>The Avalonia ContextMenu control.</returns>
    public ContextMenu EnsureContextMenu(RootMenuCommand menuCommand)
    {
        if (menuCommand is null)
        {
            throw new ArgumentNullException(nameof(menuCommand));
        }

        if (!_menus.TryGetValue(menuCommand, out var menu))
        {
            menu = new ContextMenu();
            // Bind using custom extension method
            menuCommand.SetupContainerView(new AvaMenuItemView(menu.Items));
            _menus[menuCommand] = menu;
        }

        return menu;
    }

    /// <summary>
    /// Prepares the context menu and updates state based on the selection context.
    /// </summary>
    /// <param name="menuCommand">The root menu command.</param>
    /// <param name="selection">The current selection items.</param>
    /// <returns>The prepared ContextMenu, or null if the command is not visible.</returns>
    public ContextMenu? PrepareContextMenu(RootMenuCommand menuCommand, IEnumerable<object>? selection = null)
    {
        var menu = EnsureContextMenu(menuCommand);
        var ary = selection?.ToArray() ?? Array.Empty<object>();

        _selectionTypes.Clear();
        foreach (var item in ary)
        {
            if (item != null) _selectionTypes.Add(item.GetType());
        }

        // Apply logic
        menuCommand.ApplySelection(ary);
        menuCommand.UpdateView();
        menuCommand.PopUp(ary.Length, _selectionTypes, null, ary);

        return menuCommand.Visible ? menu : null;
    }


    /// <summary>
    /// Creates a new ContextMenu for the specified command without caching.
    /// </summary>
    /// <param name="menuCommand">The root menu command.</param>
    /// <param name="sender">Optional sender object to apply.</param>
    /// <returns>A new Avalonia ContextMenu.</returns>
    public static ContextMenu CreateMenuMenu(RootMenuCommand menuCommand, object? sender = null)
    {
        // MenuFlyout manages menu items via Items collection in Avalonia 11
        var contextMenu = new ContextMenu();

        // Also bind using your custom extension method
        // Note: MenuFlyout.Items is also an ItemCollection
        menuCommand.SetupContainerView(new AvaMenuItemView(contextMenu.Items));

        if (sender != null)
        {
            menuCommand.ApplySender(sender);
        }

        return contextMenu;
    }
}

/// <summary>
/// Binds root menu commands to Avalonia MenuFlyout controls.
/// </summary>
public class AvaMenuFlyoutBinder
{
    /// <summary>
    /// Gets the default instance of the menu flyout binder.
    /// </summary>
    public static AvaMenuFlyoutBinder Default { get; } = new();

    private readonly Dictionary<RootMenuCommand, MenuFlyout> _flyouts = [];
    private readonly HashSet<Type> _selectionTypes = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaMenuFlyoutBinder"/> class.
    /// </summary>
    public AvaMenuFlyoutBinder()
    {
    }

    /// <summary>
    /// Ensures a MenuFlyout instance exists for the specified command and completes view binding.
    /// </summary>
    /// <param name="menuCommand">The root menu command.</param>
    /// <returns>The Avalonia MenuFlyout control.</returns>
    public MenuFlyout EnsureMenuFlyout(RootMenuCommand menuCommand)
    {
        if (menuCommand is null) throw new ArgumentNullException(nameof(menuCommand));

        if (!_flyouts.TryGetValue(menuCommand, out var flyout))
        {
            // MenuFlyout manages menu items via Items collection in Avalonia 11
            flyout = new MenuFlyout();

            // Also bind using your custom extension method
            // Note: MenuFlyout.Items is also an ItemCollection
            menuCommand.SetupContainerView(new AvaMenuItemView(flyout.Items));

            _flyouts[menuCommand] = flyout;
        }

        return flyout;
    }

    /// <summary>
    /// Prepares the flyout and updates state based on the selection context.
    /// </summary>
    /// <param name="menuCommand">The root menu command.</param>
    /// <param name="selection">The current selection items.</param>
    /// <returns>The prepared MenuFlyout, or null if the command is not visible.</returns>
    public MenuFlyout? PrepareMenuFlyout(RootMenuCommand menuCommand, IEnumerable<object>? selection = null)
    {
        var flyout = EnsureMenuFlyout(menuCommand);
        var selectionArray = selection?.ToArray() ?? Array.Empty<object>();

        _selectionTypes.Clear();
        foreach (var item in selectionArray)
        {
            if (item != null) _selectionTypes.Add(item.GetType());
        }

        // Call business logic notification
        menuCommand.ApplySelection(selectionArray);
        menuCommand.UpdateView();
        menuCommand.PopUp(selectionArray.Length, _selectionTypes, null, selectionArray);

        // If command logic is set to invisible, return null to prevent popup
        return menuCommand.Visible ? flyout : null;
    }

    /// <summary>
    /// Creates a new MenuFlyout for the specified command without caching.
    /// </summary>
    /// <param name="menuCommand">The root menu command.</param>
    /// <param name="sender">Optional sender object to apply.</param>
    /// <returns>A new Avalonia MenuFlyout.</returns>
    public static MenuFlyout CreateMenuFlyout(RootMenuCommand menuCommand, object? sender = null)
    {
        // MenuFlyout manages menu items via Items collection in Avalonia 11
        var flyout = new MenuFlyout();

        // Also bind using your custom extension method
        // Note: MenuFlyout.Items is also an ItemCollection
        menuCommand.SetupContainerView(new AvaMenuItemView(flyout.Items));

        if (sender != null)
        {
            menuCommand.ApplySender(sender);
        }

        return flyout;
    }
}