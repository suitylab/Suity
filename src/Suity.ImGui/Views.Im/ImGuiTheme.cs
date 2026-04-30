using Suity.Views.Graphics;
using System;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;

namespace Suity.Views.Im;

/// <summary>
/// Represents a theme configuration for ImGui, containing colors, fonts, spacing, and default style values.
/// </summary>
public class ImGuiTheme : StyleCollection
{
    /// <summary>
    /// Gets or sets the default font family used across the UI.
    /// </summary>
    public static FontFamily DefaultFont { get; set; }

    /// <summary>
    /// Gets or sets the default color used for drag operations.
    /// </summary>
    public static Color DefaultDragColor { get; set; } = Color.FromArgb(76, 159, 255);

    static ImGuiTheme()
    {
        DefaultFont = GetBestAvailableFont("Tahoma", "Segoe UI", "Arial");
    }

    /// <summary>
    /// Selects the best available font from the provided list of font names.
    /// </summary>
    /// <param name="fontNames">Ordered list of preferred font names.</param>
    /// <returns>The first available FontFamily, or GenericMonospace as fallback.</returns>
    public static FontFamily GetBestAvailableFont(params string[] fontNames)
    {
        using (var installedFonts = new InstalledFontCollection())
        {
            foreach (var name in fontNames)
            {
                if (installedFonts.Families.Any(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    return new FontFamily(name);
                }
            }
        }
        return FontFamily.GenericMonospace;
    }

    private IColorConfig? _colors;
    private Font? _font;

    private bool _themeBuilt = false;
    private string? _currentName;
    private string? _currentPseudo;

    /// <inheritdoc/>
    public override ImGuiTheme? GetTheme() => this;

    /// <summary>
    /// Gets or sets the color configuration for this theme.
    /// </summary>
    public IColorConfig Colors
    {
        get => _colors ?? DefaultColorConfig.Default;
        set => _colors = value;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ImGuiTheme"/> with default settings.
    /// </summary>
    public ImGuiTheme()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ImGuiTheme"/> with the specified color configuration.
    /// </summary>
    /// <param name="colors">The color configuration to use.</param>
    public ImGuiTheme(IColorConfig colors)
        : base()
    {
        _colors = colors;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ImGuiTheme"/> with the specified color configuration and font.
    /// </summary>
    /// <param name="colors">The color configuration to use.</param>
    /// <param name="font">The default font to use.</param>
    public ImGuiTheme(IColorConfig colors, Font font)
        : base()
    {
        _colors = colors;
        _font = font;
    }

    /// <summary>
    /// Builds the theme by invoking <see cref="OnBuildTheme"/>. Safe to call multiple times; subsequent calls are no-ops.
    /// </summary>
    public void BuildTheme()
    {
        if (_themeBuilt)
        {
            return;
        }

        try
        {
            OnBuildTheme();
            _themeBuilt = true;
        }
        catch (Exception err)
        {
            err.LogError();
        }
    }

    /// <summary>
    /// Override this method in derived classes to define custom theme initialization logic.
    /// </summary>
    protected virtual void OnBuildTheme()
    {
    }

    #region Function

    /// <inheritdoc/>
    public override InputFunction? GetInputFunction(string name)
    {
        return InputSystem?.GetInputFunction(name);
    }

    /// <inheritdoc/>
    public override LayoutFunction? GetLayoutFunction(string name)
    {
        return LayoutSystem?.GetLayoutFunction(name);
    }

    /// <inheritdoc/>
    public override FitFunction? GetFitFunction(string name)
    {
        return FitSystem?.GetFitFunction(name);
    }

    /// <inheritdoc/>
    public override RenderFunction? GetRenderFunction(string name)
    {
        return RenderSystem?.GetRenderFunction(name);
    }

    /// <summary>
    /// Gets or sets the input function system for this theme.
    /// </summary>
    public ImGuiInputSystem? InputSystem { get; set; }

    /// <summary>
    /// Gets or sets the layout function system for this theme.
    /// </summary>
    public ImGuiLayoutSystem? LayoutSystem { get; set; }

    /// <summary>
    /// Gets or sets the fit function system for this theme.
    /// </summary>
    public ImGuiFitSystem? FitSystem { get; set; }

    /// <summary>
    /// Gets or sets the render function system for this theme.
    /// </summary>
    public ImGuiRenderSystem? RenderSystem { get; set; }

    #endregion

    #region Style

    /// <summary>
    /// Gets or sets the current style name used for subsequent style operations.
    /// </summary>
    public string? CurrentName
    {
        get => _currentName;
        set => _currentName = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    /// <summary>
    /// Gets or sets the current pseudo-class used for subsequent style operations.
    /// </summary>
    public string? CurrentPseudo
    {
        get => _currentPseudo;
        set => _currentPseudo = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    /// <summary>
    /// Retrieves the style of type <typeparamref name="T"/> for the current name and pseudo-class.
    /// </summary>
    /// <typeparam name="T">The style type to retrieve.</typeparam>
    /// <returns>The style instance, or null if not found.</returns>
    /// <exception cref="NullReferenceException">Thrown when <see cref="CurrentName"/> is null.</exception>
    public T? GetStyle<T>() where T : class
    {
        if (_currentName is null)
        {
            throw new NullReferenceException(nameof(CurrentName));
        }

        return base.GetStyle<T>(_currentName, _currentPseudo);
    }

    /// <summary>
    /// Sets the style value for the current name and pseudo-class.
    /// </summary>
    /// <typeparam name="T">The style type to set.</typeparam>
    /// <param name="value">The style value.</param>
    /// <exception cref="NullReferenceException">Thrown when <see cref="CurrentName"/> is null.</exception>
    public void SetStyle<T>(T value) where T : class
    {
        if (_currentName is null)
        {
            throw new NullReferenceException(nameof(CurrentName));
        }

        base.SetStyle(_currentName, _currentPseudo, value);
    }

    /// <summary>
    /// Retrieves or creates a style of type <typeparamref name="T"/> for the current name and pseudo-class.
    /// </summary>
    /// <typeparam name="T">The style type.</typeparam>
    /// <param name="created">True if the style was newly created; otherwise, false.</param>
    /// <returns>The style instance.</returns>
    /// <exception cref="NullReferenceException">Thrown when <see cref="CurrentName"/> is null.</exception>
    public T GetOrCreateStyle<T>(out bool created) where T : class, new()
    {
        if (_currentName is null)
        {
            throw new NullReferenceException(nameof(CurrentName));
        }

        return base.GetOrCreateStyle<T>(_currentName, _currentPseudo, out created);
    }

    /// <summary>
    /// Retrieves or creates a style of type <typeparamref name="T"/> for the current name and pseudo-class.
    /// </summary>
    /// <typeparam name="T">The style type.</typeparam>
    /// <returns>The style instance.</returns>
    /// <exception cref="NullReferenceException">Thrown when <see cref="CurrentName"/> is null.</exception>
    public T GetOrCreateStyle<T>() where T : class, new()
    {
        if (_currentName is null)
        {
            throw new NullReferenceException(nameof(CurrentName));
        }

        return base.GetOrCreateStyle<T>(_currentName!, _currentPseudo, out _);
    }

    /// <summary>
    /// Removes the style of type <typeparamref name="T"/> for the current name and pseudo-class.
    /// </summary>
    /// <typeparam name="T">The style type to remove.</typeparam>
    /// <returns>True if the style was removed; otherwise, false.</returns>
    /// <exception cref="NullReferenceException">Thrown when <see cref="CurrentName"/> is null.</exception>
    public bool RemoveStyle<T>() where T : class
    {
        if (_currentName is null)
        {
            throw new NullReferenceException(nameof(CurrentName));
        }

        return base.RemoveStyle<T>(_currentName!, _currentPseudo);
    }

    /// <summary>
    /// Sets a transition between states for the current style name.
    /// </summary>
    /// <param name="state">The source state.</param>
    /// <param name="targetState">The target state.</param>
    /// <param name="transition">The transition factory.</param>
    /// <exception cref="NullReferenceException">Thrown when <see cref="CurrentName"/> is null.</exception>
    public void SetTransition(string? state, string? targetState, ITransitionFactory transition)
    {
        if (_currentName is null)
        {
            throw new NullReferenceException(nameof(CurrentName));
        }

        base.SetTransition(_currentName, state, targetState, transition);
    }

    /// <summary>
    /// Removes a transition between states for the current style name.
    /// </summary>
    /// <param name="state">The source state.</param>
    /// <param name="targetState">The target state.</param>
    /// <exception cref="NullReferenceException">Thrown when <see cref="CurrentName"/> is null.</exception>
    public void RemoveTransition(string? state, string? targetState)
    {
        if (_currentName is null)
        {
            throw new NullReferenceException(nameof(CurrentName));
        }

        base.RemoveTransition(_currentName!, state, targetState);
    }

    /// <summary>
    /// Merges all style sets from another <see cref="StyleCollection"/> into this theme.
    /// </summary>
    /// <param name="collection">The source style collection.</param>
    public void SetColllection(StyleCollection collection)
    {
        if (collection is ImGuiTheme theme)
        {
            theme.BuildTheme();
        }

        foreach (IStyleSet styleSet in collection.ToArray())
        {
            base._ex.SetStyleSet(styleSet);
        }
    }

    #endregion

    #region Values

    /// <summary>
    /// Gets or sets the default row height for list and grid items.
    /// </summary>
    public float DefaultRowHeight { get; set; } = 20;

    /// <summary>
    /// Gets or sets the default column width for grid items.
    /// </summary>
    public float DefaultColumnWidth { get; set; } = 50;

    /// <summary>
    /// Gets or sets the default spacing between child nodes.
    /// </summary>
    public float ChildSpacing { get; set; } = 2;

    /// <summary>
    /// Gets or sets the default border width.
    /// </summary>
    public float BorderWidth { get; set; } = 1f;

    /// <summary>
    /// Gets or sets the default padding for frame controls.
    /// </summary>
    public float FramePadding { get; set; } = 5;

    /// <summary>
    /// Gets or sets the default corner rounding for frame controls.
    /// </summary>
    public float FrameCornerRound { get; set; } = 5;

    /// <summary>
    /// Gets or sets whether frame backgrounds are enabled by default.
    /// </summary>
    public bool FrameBackgroundEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the default width of checkbox controls.
    /// </summary>
    public float CheckBoxWidth { get; set; } = 20;

    /// <summary>
    /// Gets or sets the default corner rounding for checkbox controls.
    /// </summary>
    public float CheckBoxCornerRound { get; set; } = 2;

    /// <summary>
    /// Gets or sets the default height of header controls.
    /// </summary>
    public float HeaderHeight { get; set; } = 20;

    /// <summary>
    /// Gets or sets the default padding for header controls.
    /// </summary>
    public float HeaderPadding { get; set; } = 10;

    /// <summary>
    /// Gets or sets the default padding around button text.
    /// </summary>
    public float ButtonTextPadding { get; set; } = 5;

    /// <summary>
    /// Gets or sets the default corner rounding for button controls.
    /// </summary>
    public float ButtonCornerRound { get; set; } = 5;

    /// <summary>
    /// Gets or sets whether button backgrounds are enabled by default.
    /// </summary>
    public bool ButtonBackgroundEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the default padding for text input controls.
    /// </summary>
    public float TextInputPadding { get; set; } = 5;

    /// <summary>
    /// Gets or sets the default corner rounding for text input controls.
    /// </summary>
    public float TextInputRound { get; set; } = 0;

    /// <summary>
    /// Gets or sets the default font for this theme.
    /// </summary>
    public Font Font
    {
        get => _font ?? ImGuiExternal._external.DefaultFont;
        set => _font = value;
    }

    /// <summary>
    /// Gets or sets the width of scrollbars.
    /// </summary>
    public float ScrollBarWidth { get; set; } = 10;

    /// <summary>
    /// Gets or sets the corner rounding of scrollbars.
    /// </summary>
    public float ScrollBarRound { get; set; } = 0;

    /// <summary>
    /// Gets or sets the opacity of scrollbars.
    /// </summary>
    public float ScrollBarOpacity { get; set; } = 0.5f;

    /// <summary>
    /// Gets or sets the duration of expand/collapse animations.
    /// </summary>
    public float ExpandDuration { get; set; } = 0.2f;

    /// <summary>
    /// Gets or sets the duration of scroll animations.
    /// </summary>
    public float ScrollDuration { get; set; } = 0.2f;

    /// <summary>
    /// Gets or sets the scroll delta multiplier.
    /// </summary>
    public float ScrollDelta { get; set; } = 1.0f;

    #endregion

    /// <summary>
    /// Gets or sets the image used for empty/placeholder states.
    /// </summary>
    public Image EmptyImage { get; set; } = ImGuiIcons.Empty;

    /// <summary>
    /// Gets or sets the image used for expand indicators.
    /// </summary>
    public Image ExpandImage { get; set; } = ImGuiIcons.Expand;

    /// <summary>
    /// Gets or sets the image used for collapse indicators.
    /// </summary>
    public Image CollapseImage { get; set; } = ImGuiIcons.Collapse;

    /// <summary>
    /// Gets or sets the image used for dropdown indicators.
    /// </summary>
    public Image DropDownImage { get; set; } = ImGuiIcons.Row;

    /// <summary>
    /// Gets or sets the image used for checked checkbox states.
    /// </summary>
    public Image CheckBoxCheckedImage { get; set; } = ImGuiIcons.Checked;

    /// <summary>
    /// Gets or sets the image used for pending/indeterminate checkbox states.
    /// </summary>
    public Image CheckBoxPendingImage { get; set; } = ImGuiIcons.Pending;

    /// <summary>
    /// Gets or sets the warning icon image.
    /// </summary>
    public Image Warning { get; set; } = ImGuiIcons.Warning;
}
