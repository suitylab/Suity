using Suity.Views.Graphics;
using System;
using System.Collections.Generic;

namespace Suity.Views.Im;

/// <summary>
/// Registry of systems that handle specific ImGui pipeline operations.
/// </summary>
public class ImGuiInputSystem
{
    /// <summary>Input function name for click handling.</summary>
    public const string Click = ".Click";
    /// <summary>Input function name for mouse-in render events.</summary>
    public const string MouseInRender = ".MouseInRender";
    /// <summary>Input function name for mouse-in refresh events.</summary>
    public const string MouseInRefresh = ".MouseInRefresh";
    /// <summary>Input function name for hover handling.</summary>
    public const string Hover = ".Hover";
    /// <summary>Input function name for key down events.</summary>
    public const string KeyDown = ".KeyDown";
    /// <summary>Input function name for key up events.</summary>
    public const string KeyUp = ".KeyUp";
    /// <summary>Input function name for resizer handling.</summary>
    public const string Resizer = ".Resizer";
    /// <summary>Input function name for resizer fitting.</summary>
    public const string ResizerFitter = ".ResizerFitter";
    /// <summary>Input function name for grouped resizer handling.</summary>
    public const string GroupedResizer = ".GroupedResizer";
    /// <summary>Input function name for tree view handling.</summary>
    public const string TreeView = ".TreeView";
    /// <summary>Input function name for tree node handling.</summary>
    public const string TreeNode = ".TreeNode";
    /// <summary>Input function name for simple string input.</summary>
    public const string SimpleStringInput = ".SimpleStringInput";
    /// <summary>Input function name for double-click string input.</summary>
    public const string DoubleClickStringInput = ".DoubleClickStringInput";
    /// <summary>Input function name for drag and drop handling.</summary>
    public const string DragDrop = ".DragDrop";
    /// <summary>Input function name for viewport handling.</summary>
    public const string Viewport = ".Viewport";

    private static ImGuiInputSystem @default = new();

    /// <summary>
    /// Gets or sets the default input system instance.
    /// </summary>
    public static ImGuiInputSystem Default
    {
        get => @default;
        set => @default = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Resolves an input function by name. Override to provide custom input functions.
    /// </summary>
    /// <param name="name">The name of the input function.</param>
    /// <returns>The input function, or null if not found.</returns>
    public virtual InputFunction? GetInputFunction(string name) => null;
}

/// <summary>
/// Registry of layout systems that determine how nodes are positioned.
/// </summary>
public class ImGuiLayoutSystem
{
    /// <summary>Layout function name for fill layout.</summary>
    public const string Fill = ".Fill";
    /// <summary>Layout function name for horizontal layout.</summary>
    public const string Horizontal = ".Horizontal";
    /// <summary>Layout function name for vertical layout.</summary>
    public const string Vertical = ".Vertical";
    /// <summary>Layout function name for horizontal reverse layout.</summary>
    public const string HorizontalReverse = ".HorizontalReverse";
    /// <summary>Layout function name for vertical reverse layout.</summary>
    public const string VerticalReverse = ".VerticalReverse";
    /// <summary>Layout function name for overlay layout.</summary>
    public const string Overlay = ".Overlay";
    /// <summary>Layout function name for viewport layout.</summary>
    public const string Viewport = ".Viewport";

    private static ImGuiLayoutSystem @default = new();

    /// <summary>
    /// Gets or sets the default layout system instance.
    /// </summary>
    public static ImGuiLayoutSystem Default
    {
        get => @default;
        set => @default = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Resolves a layout function by name. Override to provide custom layout functions.
    /// </summary>
    /// <param name="name">The name of the layout function.</param>
    /// <returns>The layout function, or null if not found.</returns>
    public virtual LayoutFunction? GetLayoutFunction(string name) => null;
}

/// <summary>
/// Registry of fit systems that calculate node sizing.
/// </summary>
public class ImGuiFitSystem
{
    /// <summary>Fit function name for auto fitting.</summary>
    public const string Auto = ".Auto";
    /// <summary>Fit function name for expandable fitting.</summary>
    public const string Expandable = ".Expandable";
    /// <summary>Fit function name for overlay fitting.</summary>
    public const string Overlay = ".Overlay";
    /// <summary>Fit function name for scrollable fitting.</summary>
    public const string Scrollable = ".Scrollable";

    private static ImGuiFitSystem @default = new();

    /// <summary>
    /// Gets or sets the default fit system instance.
    /// </summary>
    public static ImGuiFitSystem Default
    {
        get => @default;
        set => @default = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Resolves a fit function by name. Override to provide custom fit functions.
    /// </summary>
    /// <param name="name">The name of the fit function.</param>
    /// <returns>The fit function, or null if not found.</returns>
    public virtual FitFunction? GetFitFunction(string name) => null;
}

/// <summary>
/// Registry of render systems that handle drawing operations.
/// </summary>
public class ImGuiRenderSystem
{
    private static ImGuiRenderSystem @default = new();

    /// <summary>
    /// Gets or sets the default render system instance.
    /// </summary>
    public static ImGuiRenderSystem Default
    {
        get => @default;
        set => @default = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Resolves a render function by name. Override to provide custom render functions.
    /// </summary>
    /// <param name="name">The name of the render function.</param>
    /// <returns>The render function, or null if not found.</returns>
    public virtual RenderFunction? GetRenderFunction(string name) => null;
}
