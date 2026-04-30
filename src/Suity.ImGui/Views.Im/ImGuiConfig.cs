using Suity.Collections;
using Suity.Views.Graphics;
using System;
using System.Collections.Generic;

namespace Suity.Views.Im;

/// <summary>
/// Configuration settings for an ImGui instance, including color schemes, systems, and theme.
/// </summary>
public sealed class ImGuiConfig
{
    private IColorConfig? _colors;

    /// <summary>
    /// The input system responsible for handling user input processing.
    /// </summary>
    internal ImGuiInputSystem? _inputSystem;

    /// <summary>
    /// The layout system responsible for determining node positioning.
    /// </summary>
    internal ImGuiLayoutSystem? _layoutSystem;

    /// <summary>
    /// The fit system responsible for calculating node sizing.
    /// </summary>
    internal ImGuiFitSystem? _fitSystem;

    /// <summary>
    /// The render system responsible for handling drawing operations.
    /// </summary>
    internal ImGuiRenderSystem? _renderSystem;

    private readonly Dictionary<Type, object> _systems = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiConfig"/> class with default color configuration.
    /// </summary>
    public ImGuiConfig()
    {
        _colors = DefaultColorConfig.Default;
    }

    /// <summary>
    /// Gets or sets the name of this ImGui configuration.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the color configuration used for theming.
    /// </summary>
    public IColorConfig ColorConfig
    {
        get => _colors ?? DefaultColorConfig.Default;
        set => _colors = value;
    }

    /// <summary>
    /// Gets or sets the input system that handles user input processing.
    /// </summary>
    public ImGuiInputSystem InputSystem
    {
        get => _inputSystem ?? ImGuiInputSystem.Default;
        set => _inputSystem = value;
    }

    /// <summary>
    /// Gets or sets the layout system that determines node positioning.
    /// </summary>
    public ImGuiLayoutSystem LayoutSystem
    {
        get => _layoutSystem ?? ImGuiLayoutSystem.Default;
        set => _layoutSystem = value;
    }

    /// <summary>
    /// Gets or sets the fit system that calculates node sizing.
    /// </summary>
    public ImGuiFitSystem FitSystem
    {
        get => _fitSystem ?? ImGuiFitSystem.Default;
        set => _fitSystem = value;
    }

    /// <summary>
    /// Gets or sets the render system that handles drawing operations.
    /// </summary>
    public ImGuiRenderSystem RenderSystem
    {
        get => _renderSystem ?? ImGuiRenderSystem.Default;
        set => _renderSystem = value;
    }

    /// <summary>
    /// Gets or sets the theme used for styling ImGui elements.
    /// </summary>
    public ImGuiTheme? Theme { get; set; }

    /// <summary>
    /// Adds a custom system to the configuration.
    /// </summary>
    /// <typeparam name="T">The type of the system.</typeparam>
    /// <param name="value">The system instance to add.</param>
    public void AddSystem<T>(T value) where T : class
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        _systems.Add(typeof(T), value);
    }

    /// <summary>
    /// Gets an existing system of the specified type, or creates and adds a new one if it doesn't exist.
    /// </summary>
    /// <typeparam name="T">The type of the system.</typeparam>
    /// <returns>The existing or newly created system instance.</returns>
    public T GetOrAddSystem<T>() where T : class, new()
    {
        return (_systems.GetOrAdd(typeof(T), _ => new T()) as T)!;
    }

    /// <summary>
    /// Gets a system of the specified type if it exists.
    /// </summary>
    /// <typeparam name="T">The type of the system.</typeparam>
    /// <returns>The system instance, or null if not found.</returns>
    public T? GetSystem<T>() where T : class
    {
        return _systems.GetValueSafe(typeof(T)) as T;
    }
}
