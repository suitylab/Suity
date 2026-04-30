using Suity.Views.Gui;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Services;

/// <summary>
/// Interface defining the contract for a tool window service.
/// The service is responsible for managing and providing access to tool windows within the application.
/// </summary>
public interface IToolWindowService
{
    /// <summary>
    /// Gets a tool window instance by its unique window identifier.
    /// </summary>
    /// <param name="windowId">The unique identifier of the tool window.</param>
    /// <returns>An instance of IToolWindow that matches the specified windowId.</returns>
    IToolWindow GetToolWindow(string windowId);

    /// <summary>
    /// Gets a tool window instance by its type.
    /// </summary>
    /// <param name="toolWindowType">The type of the tool window to retrieve.</param>
    /// <returns>An instance of IToolWindow that matches the specified type.</returns>
    IToolWindow GetToolWindow(Type toolWindowType);

    /// <summary>
    /// Gets a collection of all available tool windows.
    /// </summary>
    /// <value>An IEnumerable of IToolWindow instances representing all tool windows.</value>
    IEnumerable<IToolWindow> ToolWindows { get; }

    /// <summary>
    /// Creates a new view object window.
    /// </summary>
    /// <param name="window">The view object window configuration.</param>
    /// <returns>An IWindowHandle representing the newly created window.</returns>
    IWindowHandle CreateViewObjectWindow(IViewObjectWindow window);
}
