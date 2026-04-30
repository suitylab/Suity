using System;

namespace Suity.Views.Im;

/// <summary>
/// Style collection specific to an ImGui node, supporting inheritance from parent styles and themes.
/// </summary>
public class ImGuiNodeStyle : StyleCollection
{
    private readonly ImGuiNode _owner;
    private StyleCollection? _parent;
    private ImGuiTheme? _theme;

    private long _parentVersion;
    private long _themeVersion;

    internal ImGuiNodeStyle(ImGuiNode owner, StyleCollection? parent)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        _parent = parent;
    }

    /// <summary>
    /// Gets the node that owns this style collection.
    /// </summary>
    public ImGuiNode Owner => _owner;

    /// <summary>
    /// Gets or sets the parent style collection for inheritance.
    /// </summary>
    public StyleCollection? Parent
    {
        get => _parent;
        internal set
        {
            if (!ReferenceEquals(_parent, value))
            {
                _parent = value;
                _parentVersion = 0;
                MarkDirty();
            }
        }
    }

    /// <summary>
    /// Gets or sets the theme associated with this style.
    /// </summary>
    public ImGuiTheme? Theme
    {
        get => _theme;
        internal set
        {
            if (_theme != value)
            {
                _theme = value;
                _themeVersion = 0;
                _theme?.BuildTheme();
                MarkDirty();
            }
        }
    }

    /// <inheritdoc/>
    public override bool IsDirty
    {
        get
        {
            if (_parent != null && (_parent.IsDirty || _parent.Version != _parentVersion))
            {
                return true;
            }

            if (_theme != null && (_theme.IsDirty || _theme.Version != _themeVersion))
            {
                return true;
            }

            return base.IsDirty;
        }
    }

    /// <inheritdoc/>
    public override ImGuiTheme? GetTheme()
    {
        return _theme ?? _parent?.GetTheme();
    }

    /// <inheritdoc/>
    public override InputFunction? GetInputFunction(string name)
    {
        return _theme?.GetInputFunction(name) ?? _parent?.GetInputFunction(name);
    }

    /// <inheritdoc/>
    public override LayoutFunction? GetLayoutFunction(string name)
    {
        return _theme?.GetLayoutFunction(name) ?? _parent?.GetLayoutFunction(name);
    }

    /// <inheritdoc/>
    public override FitFunction? GetFitFunction(string name)
    {
        return _theme?.GetFitFunction(name) ?? _parent?.GetFitFunction(name);
    }

    /// <inheritdoc/>
    public override RenderFunction? GetRenderFunction(string name)
    {
        return _theme?.GetRenderFunction(name) ?? _parent?.GetRenderFunction(name);
    }

    /// <inheritdoc/>
    public override void ApplyStyleSet(string id, string? typeName, string[]? classes, ref IStyleSet? styleSet)
    {
        _parent?.ApplyStyleSet(id, typeName, classes, ref styleSet);
        _theme?.ApplyStyleSet(id, typeName, classes, ref styleSet);
        base.ApplyStyleSet(id, typeName, classes, ref styleSet);
    }

    /// <inheritdoc/>
    public override void ApplyStyles(string id, string? typeName, string[]? classes, ref IValueCollection? values, string? pseudo = null)
    {
        _parent?.ApplyStyles(id, typeName, classes, ref values, pseudo);
        _theme?.ApplyStyles(id, typeName, classes, ref values, pseudo);
        base.ApplyStyles(id, typeName, classes, ref values, pseudo);
    }

    /// <inheritdoc/>
    internal override void ClearDirty()
    {
        base.ClearDirty();

        if (_parent != null)
        {
            _parentVersion = _parent.Version;
        }

        if (_theme != null)
        {
            _theme.ClearDirty(); // Theme is a shared class and needs manual dirty clearing
            _themeVersion = _theme.Version;
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{_owner}'s style";
    }
}