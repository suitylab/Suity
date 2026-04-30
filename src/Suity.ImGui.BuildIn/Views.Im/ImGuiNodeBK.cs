using Suity.Collections;
using Suity.Views.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Suity.Views.Im;

/// <summary>
/// Internal implementation of an ImGui node, representing a single UI element in the node tree.
/// Manages layout, styling, input handling, rendering, and child node relationships.
/// </summary>
internal class ImGuiNodeBK : ImGuiNode
{
    /// <summary>
    /// Number of nodes using dictionary
    /// </summary>
    private const int UseDicCount = 10;

    // Optimized Fit operation outside bounds, which causes dynamic size refresh operations not to execute.
    private const bool OptimaizeOutOfBound = false;

    /// <summary>
    /// Whether to log ignored value types in debug mode.
    /// </summary>
    public static bool LogIgnoreValueType = false;

    /// <summary>
    /// Whether to log layout out of range warnings.
    /// </summary>
    public static bool LogLayoutOutOfRange = false;

    private static readonly HashSet<Type> IgnoredValues =
    [
        typeof(GuiColorStyle),
        typeof(GuiSizeStyle),
        typeof(GuiMarginStyle),
        typeof(GuiPaddingStyle),
        typeof(GuiFitOrientationStyle),
        typeof(GuiAlignmentStyle),
        typeof(GuiChildSpacingStyle),
        typeof(GuiHeaderStyle),
    ];

    #region Private values

    private readonly ImGuiBK _gui;
    private readonly string _id;
    private ImGuiPath? _fullPath;

    private ImGuiNodeBK? _parent;
    private int _index;

    private string? _typeName;
    private string[]? _classes;

    private ImGuiNodeFlags _flags;
    private long _inputVersion;
    private GuiMouseState _mouseState;

    private RectangleF _rect;
    private RectangleF _lastRect;
    private RectangleF? _dirtyRect;
    private RectangleF? _mouseClickRect;

    private StyleCollection? _inheritedStyleDefinition;
    private ImGuiNodeStyle? _styleDefinition;

    private IStyleSet? _styleSet;
    private string? _pseudo;
    private ValueCollection? _pseudoStyles;
    private readonly ValueCollection _values = new();
    private float _animationStartTime;
    private IGuiAnimation? _animation;

    private List<ImGuiNodeBK>? _childNodeList;
    private Dictionary<string, ImGuiNodeBK>? _childNodeDic;

    private int _layoutIndex;
    private PointF _initPos;
    private PointF _pos;
    private PointF _lastPos;
    private readonly GuiLayoutPosition _layoutPos = new();

    // Functions
    private InputFunction? _baseInputFunction;
    private LayoutFunction? _baseLayoutFunction;
    private FitFunction? _baseFitFunction;
    private RenderFunction? _baseRenderFunction;

    // Base values
    private string? _text;

    private Color? _baseColor;
    private GuiLength? _baseWidth;
    private GuiLength? _baseHeight;
    private GuiThickness? _baseMargin;
    private GuiThickness? _basePadding;
    private GuiOrientation _baseFitOrientation;
    private GuiAlignmentStyle? _baseAlignment;
    private float? _baseChildSpacing; // ****
    private GuiHeaderStyle? _baseHeader;

    private TransformNode? _transform;
    private TransformNode? _globalTransform;
    private TransformNode? _childTransform;

    // Function chain
    private IFunctionChain<InputFunction>? _inputFuncFromStyle;
    private IFunctionChain<LayoutFunction>? _layoutFuncFromStyle;
    private IFunctionChain<FitFunction>? _fitFuncFromStyle;
    private IFunctionChain<RenderFunction>? _renderFuncFromStyle;

    // Base styles
    private readonly ValueStyleSlot<GuiColorStyle> _color;
    private readonly ValueStyleSlot<GuiSizeStyle> _size;
    private readonly ValueStyleSlot<GuiMarginStyle> _margin;
    private readonly ValueStyleSlot<GuiPaddingStyle> _padding;
    private readonly ValueStyleSlot<GuiFitOrientationStyle> _fitOrientation;
    private readonly ValueStyleSlot<GuiAlignmentStyle> _alignment;
    private readonly ValueStyleSlot<GuiChildSpacingStyle> _childSpacing;
    private readonly ValueStyleSlot<GuiSiblingSpacingStyle> _siblingSpacing;
    private readonly ValueStyleSlot<GuiHeaderStyle> _header;
    private readonly ValueStyleSlot<GuiTextAlignmentStyle> _textAlignment;
    private readonly ValueStyleSlot<GuiBorderStyle> _border;
    private readonly ValueStyleSlot<GuiFontStyle> _font;
    private readonly ValueStyleSlot<GuiImageValue> _image;
    private readonly ValueStyleSlot<GuiImageFilterStyle> _imageFilter;
    private readonly ValueStyleSlot<GuiFrameStyle> _frame;
    private readonly ValueStyleSlot<GuiExpandableValue> _expandable; 

    #endregion

    internal ImGuiNodeBK(ImGuiBK gui, string id)
    {
        _gui = gui ?? throw new ArgumentNullException(nameof(gui));
        _id = id ?? throw new ArgumentNullException(nameof(id));

        _gui._statistic.NodeCreated++;

        _color = new(_values.EnsureValueItem<GuiColorStyle>());
        _size = new(_values.EnsureValueItem<GuiSizeStyle>());
        _margin = new(_values.EnsureValueItem<GuiMarginStyle>());
        _padding = new(_values.EnsureValueItem<GuiPaddingStyle>());
        _fitOrientation = new(_values.EnsureValueItem<GuiFitOrientationStyle>());
        _alignment = new(_values.EnsureValueItem<GuiAlignmentStyle>());
        _childSpacing = new(_values.EnsureValueItem<GuiChildSpacingStyle>());
        _siblingSpacing = new(_values.EnsureValueItem<GuiSiblingSpacingStyle>());
        _header = new(_values.EnsureValueItem<GuiHeaderStyle>());
        _textAlignment = new(_values.EnsureValueItem<GuiTextAlignmentStyle>());
        _border = new(_values.EnsureValueItem<GuiBorderStyle>());
        _font = new(_values.EnsureValueItem<GuiFontStyle>());
        _image = new(_values.EnsureValueItem<GuiImageValue>());
        _imageFilter = new(_values.EnsureValueItem<GuiImageFilterStyle>());
        _frame = new(_values.EnsureValueItem<GuiFrameStyle>());
        _expandable = new(_values.EnsureValueItem<GuiExpandableValue>());
    }

    internal ImGuiNodeBK(ImGuiBK gui, string id, ImGuiNodeBK parent)
        : this(gui, id)
    {
        _parent = parent ?? throw new ArgumentNullException(nameof(parent));

        UpdateGlobalTransform();
    }

    #region System

    /// <summary>
    /// ImGui drawing core
    /// </summary>
    public override ImGui Gui => _gui;

    /// <summary>
    /// Get the local Id of this node
    /// </summary>
    public override string Id => _id;

    /// <summary>
    /// Get the index of this node on its parent node
    /// </summary>
    public override int Index => _index;

    /// <summary>
    /// Get the full path name of this node
    /// </summary>
    public override ImGuiPath FullPath
    {
        get
        {
            if (_fullPath is { })
            {
                return _fullPath;
            }

            _fullPath = _parent is null ? ImGuiPath.Empty : _parent.FullPath.Append(_id);

            return _fullPath;
        }
    }

    public override string? TypeName
    {
        get => _typeName;
        set
        {
            if (!IsInitializing)
            {
#if DEBUG
                Debug.WriteLine("TypeName can not be set after node is initialized.");
                return;
#endif
            }

            if (_typeName != value)
            {
                _typeName = value;
                _flags &= ~ImGuiNodeFlags.StyleCreated;
                MarkRenderDirty();
            }
        }
    }


    public override ImGuiNode? Parent => _parent;
    internal ImGuiNodeBK? InternalParent => _parent;

    public override ImGuiNodeFlags Flags => _flags;

    

    private bool SetProperty<T>(ref T property, T value, bool markDirty = true)
    {
        if (!Equals(property, value))
        {
            property = value;
            if (markDirty)
            {
                MarkRenderDirty();
            }

            return true;
        }

        return false;
    }

    private T GetOrCreateProperty<T>(ref T? property) where T : class, new()
    {
        if (property is null)
        {
            property = new T();
            MarkRenderDirty();
        }

        return property;
    }

    public override void QueueRefresh([CallerLineNumber] int line = 0, [CallerMemberName] string? member = null, [CallerFilePath] string? path = null)
    {
        _gui?.QueueRefresh(this, line, member, path);
    }


    #endregion

    #region Transforms


    public override RectangleF Rect
    {
        get => _rect;
        set
        {
            SetProperty(ref _rect, value, false);
        }
    }
    public override RectangleF LocalRect => _transform?.Transform.Transform(_rect) ?? _rect;
    public override RectangleF GlobalRect => _globalTransform?.TransformRectInHierarchy(_rect) ?? _rect;

    
    public override RectangleF InnerRect
    {
        get
        {
            var rect = _rect;
            float headerWidth = HeaderWidth ?? 0;
            float headerHeight = HeaderHeight ?? 0;

            if (Padding is { } padding)
            {
                rect = padding.Shrink(rect);
            }

            if (headerWidth > 0)
            {
                rect.X += headerWidth;
                rect.Width -= headerWidth;
            }
            if (headerHeight > 0)
            {
                rect.Y += headerHeight;
                rect.Height -= headerHeight;
            }

            if (rect.Width < 0)
            {
                rect.Width = 0;
            }
            if (rect.Height < 0)
            {
                rect.Height = 0;
            }

            return rect;
        }
    }
    public override RectangleF LocalInnerRect => _transform?.Transform.Transform(InnerRect) ?? InnerRect;
    public override RectangleF GlobalInnerRect => _globalTransform?.TransformRectInHierarchy(InnerRect) ?? InnerRect;

    
    public override RectangleF OuterRect
    {
        get
        {
            var rect = _rect;
            if (Margin is { } margin)
            {
                rect = margin.Expand(rect);
            }

            return rect;
        }
    }
    public override RectangleF LocalOuterRect => _transform?.Transform.Transform(OuterRect) ?? OuterRect;
    public override RectangleF GlobalOuterRect => _globalTransform?.TransformRectInHierarchy(OuterRect) ?? OuterRect;

    
    public override RectangleF? DirtyRect
    {
        get
        {
            var rect = _dirtyRect ?? _rect;

            //TODO: If the parent is a scrollable control, need to get the parent's DirtyRect and intersect with current DirtyRect
            // Try to only do one level

            if (_parent is { } p)
            {
                var pInnerRect = p.InnerRect;
                pInnerRect = p.RevertTransformChildRect(pInnerRect);
                rect.Intersect(pInnerRect);
            }

            return rect.IsEmpty ? null : rect;

            var parent = _parent;
            while (parent is { })
            {
                var pInnerRect = parent.InnerRect;

                rect.Intersect(pInnerRect);

                if (rect.IsEmpty)
                {
                    return null;
                }

                parent = parent._parent;
            }

            return rect.IsEmpty ? null : rect;
        }
    }

    public override RectangleF? GlobalDirtyRect => DirtyRect is { } dirtyRect ? _globalTransform?.TransformRectInHierarchy(dirtyRect) ?? dirtyRect : null;


    public override RectangleF? MouseClickRect
    {
        get => _mouseClickRect;
        set => SetProperty(ref _mouseClickRect, value, false);
    }

    public override RectangleF? GlobalMouseClickRect => _mouseClickRect is { } mouseClickRect ? _globalTransform?.TransformRectInHierarchy(mouseClickRect) ?? mouseClickRect : null;


    public override void OffsetPositionDeep(float x, float y)
    {
        _gui._statistic.OffsetPositionDeepCall++;

        // _rect original value, no Transform needed
        _rect.X += x;
        _rect.Y += y;
        MarkRenderDirty();

        if (_childNodeList is { })
        {
            foreach (var childNode in _childNodeList)
            {
                childNode.OffsetPositionDeep(x, y);
            }
        }
    }

    public override GuiTransform? Transform
    {
        get => _transform?.Transform;
        set
        {
            bool changed = false;

            if (value is { } t)
            {
                if (_transform != null)
                {
                    if (_transform.Transform != t)
                    {
                        _transform.Transform = t;
                        changed = true;
                    }
                }
                else
                {
                    _transform = new TransformNode(this, t);
                    UpdateGlobalTransform();
                    changed = true;
                }
            }
            else
            {
                if (_transform != null)
                {
                    _transform = null;
                    UpdateGlobalTransform();
                    changed = true;
                }
            }
            
            if (changed)
            {
                MarkRenderDirty();
            }
        }
    }

    public override float? LocalScale
    {
        get
        {
            if (_flags.HasFlag(ImGuiNodeFlags.NoTransform))
            {
                return null;
            }

            return _transform?.Transform.Scale;
        }
    }

    public override float? GlobalScale => _globalTransform?.GetGlobalTransform().Scale;

    public override float LocalScaleValue(float value)
    {
        if (LocalScale is { } scale && scale != 1 && scale > 0)
        {
            return value * scale;
        }

        return value;
    }

    public override float LocalReverseScaleValue(float value)
    {
        if (LocalScale is { } scale && scale != 1 && scale > 0)
        {
            return value / scale;
        }

        return value;
    }

    public override float GlobalScaleValue(float value)
    {
        //if (_flags.HasFlag(ImGuiNodeFlags.NoTransform))
        //{
        //    return value;
        //}

        if (GlobalScale is { } scale && scale != 1 && scale > 0)
        {
            return value * scale;
        }

        return value;
    }

    public override float GlobalReverseScaleValue(float value)
    {
        if (GlobalScale is { } scale && scale != 1 && scale > 0)
        {
            return value / scale;
        }

        return value;
    }

    public override PointF GlobalScalePoint(PointF point)
    {
        return _globalTransform?.TransformPointInHierarchy(point) ?? point;
    }

    public override PointF GlobalRevertScalePoint(PointF point)
    {
        return _globalTransform?.RevertTransformPointInHierarchy(point) ?? point;
    }

    public override RectangleF GlobalScaleRect(RectangleF rect)
    {
        return _globalTransform?.TransformRectInHierarchy(rect) ?? rect;
    }

    public override RectangleF GlobalRevertScaleRect(RectangleF rect)
    {
        return _globalTransform?.RevertTransformRectInHierarchy(rect) ?? rect;
    }

    public override RectangleF TransformChildRect(RectangleF rect)
    {
        return _transform?.TransformRect(rect) ?? rect;
    }

    public override RectangleF RevertTransformChildRect(RectangleF rect)
    {
        return _transform?.RevertTransformRect(rect) ?? rect;
    }

    internal void UpdateGlobalTransform()
    {
        if (_transform is { })
        {
            // Build Transform chain
            if (_flags.HasFlag(ImGuiNodeFlags.NoTransform))
            {
                _globalTransform = _parent?._childTransform;
                _transform.ParentTransform = _globalTransform;
                _childTransform = _transform;
            }
            else
            {
                _transform.ParentTransform = _parent?._childTransform;
                _childTransform = _globalTransform = _transform;
            }
        }
        else
        {
            // Pass reference value
            _childTransform = _globalTransform = _parent?._childTransform;
        }
    }

    #endregion

    #region Functions

    /// <summary>
    /// Input function
    /// </summary>
    public override InputFunction? InputFunction => _inputFuncFromStyle?.Entry ?? _baseInputFunction;

    /// <summary>
    /// Layout function
    /// </summary>
    public override LayoutFunction? LayoutFunction => _layoutFuncFromStyle?.Entry ?? _baseLayoutFunction;

    /// <summary>
    /// Fit function
    /// </summary>
    public override FitFunction? FitFunction => _fitFuncFromStyle?.Entry ?? _baseFitFunction;

    /// <summary>
    /// Render function
    /// </summary>
    public override RenderFunction? RenderFunction => _renderFuncFromStyle?.Entry ?? _baseRenderFunction;

    /// <summary>
    /// Input function
    /// </summary>
    public override InputFunction? BaseInputFunction
    {
        get => _baseInputFunction;
        set
        {
            SetProperty(ref _baseInputFunction, value); // No need to reset Style
            _gui._statistic.SetFunctionBase++;
        }
    }

    /// <summary>
    /// Layout function
    /// </summary>
    public override LayoutFunction? BaseLayoutFunction
    {
        get => _baseLayoutFunction;
        set
        {
            SetProperty(ref _baseLayoutFunction, value); // No need to reset Style
            _gui._statistic.SetFunctionBase++;
        }
    }

    /// <summary>
    /// Fit function
    /// </summary>
    public override FitFunction? BaseFitFunction
    {
        get => _baseFitFunction;
        set
        {
            SetProperty(ref _baseFitFunction, value); // No need to reset Style
            _gui._statistic.SetFunctionBase++;
        }
    }

    /// <summary>
    /// Render function
    /// </summary>
    public override RenderFunction? BaseRenderFunction
    {
        get => _baseRenderFunction;
        set
        {
            SetProperty(ref _baseRenderFunction, value); // No need to reset Style
            _gui._statistic.SetFunctionBase++;
        }
    }

    #endregion

    #region Style

    public override string[]? Classes
    {
        get => _classes ?? [];
        set
        {
            if (value?.Length == 0)
            {
                value = null;
            }

            if (!ArrayHelper.ArrayEquals(_classes, value))
            {
                _classes = value;
                _flags &= ~ImGuiNodeFlags.StyleCreated;
                MarkRenderDirty();
            }
        }
    }

    public override bool PseudoAffectsChildren
    {
        get => _flags.HasFlag(ImGuiNodeFlags.PseudoAffectsChildren);
        set
        {
            if (value == _flags.HasFlag(ImGuiNodeFlags.PseudoAffectsChildren))
            {
                return;
            }

            if (value)
            {
                _flags |= ImGuiNodeFlags.PseudoAffectsChildren;

                SetChildrenPseudoDeep(_pseudo);
            }
            else
            {
                _flags &= ~ImGuiNodeFlags.PseudoAffectsChildren;
            }
        }
    }

    public override bool AddClass(string @class)
    {
        if (string.IsNullOrWhiteSpace(@class))
        {
            return false;
        }

        bool added = false;

        if (_classes is null)
        {
            _classes = [@class];
            added = true;
        }
        else if (!_classes.Contains(@class))
        {
            Array.Resize(ref _classes, _classes.Length + 1);
            _classes[_classes.Length - 1] = @class;
            added = true;
        }

        if (added)
        {
            _flags &= ~ImGuiNodeFlags.StyleCreated;
            MarkRenderDirty();
            return true;
        }
        else
        {
            return false;
        }
    }

    public override bool RemoveClass(string @class)
    {
        if (string.IsNullOrWhiteSpace(@class) || _classes is null)
        {
            return false;
        }

        bool removed = false;

        int index = _classes.IndexOf(v => v == @class);
        if (index >= 0)
        {
            if (_classes.Length > 0)
            {
                for (int i = index; i < _classes.Length - 1; i++)
                {
                    _classes[i] = _classes[i + 1];
                }

                Array.Resize(ref _classes, _classes.Length - 1);
            }
            else
            {
                _classes = null;
            }

            removed = true;
        }

        if (removed)
        {
            _flags &= ~ImGuiNodeFlags.StyleCreated;
            MarkRenderDirty();
            return true;
        }
        else
        {
            return false;
        }
    }

    public override bool HasStyle => _pseudoStyles is { };
    public override bool HasPseudoStyle => _pseudoStyles is { BaseValues: { } };

    public override string? Pseudo { get => _pseudo; set => ApplyStyles(value); }

    public override ImGuiTheme Theme
    {
        get => StyleDefinition.GetTheme() ?? _gui.Theme;
        set
        {
            I_EnsureMyStyle().Theme = value;
            ApplyStyles();
        }
    }

    internal StyleCollection InheritedStyleDefinition => _inheritedStyleDefinition ?? Gui.Theme;
    internal StyleCollection StyleDefinition => _styleDefinition ?? _inheritedStyleDefinition ?? Gui.Theme;

    public override T? GetStyle<T>() where T : class
    {
        _gui._statistic.GetStyleCall++;

        if (_flags.HasFlag(ImGuiNodeFlags.OverrideDisabled))
        {
            return _animation?.GetValue<T>() ??
                _pseudoStyles?.GetValue<T>() ??
                _values.GetValue<T>();
        }
        else
        {
            return _values.GetValue<T>() ??
                _animation?.GetValue<T>() ??
                _pseudoStyles?.GetValue<T>();
        }
    }

    public override void SetStyle<T>(string name, T style) where T : class
    {
        I_EnsureMyStyle().SetStyle(name, style);
    }

    public override void SetStyle<T>(string name, string pseudo, T style) where T : class
    {
        I_EnsureMyStyle().SetPseudo(name, pseudo, style);
    }

    public override void ApplyStyles()
    {
        var styleDef = StyleDefinition;
        if (!_flags.HasFlag(ImGuiNodeFlags.StyleCreated) || styleDef.IsDirty)
        {
            _gui._statistic.StyleChanged++;

            (_styleSet as StyleSet)?.Clear();
            styleDef.ApplyStyleSet(Id, TypeName, Classes, ref _styleSet);
            _pseudoStyles = (_styleSet?.GetStyleCollection(_pseudo) ?? _styleSet?.GetStyleCollection(null)) as ValueCollection;

            if (_animation is { })
            {
                StopAnimation();
            }

            _flags |= ImGuiNodeFlags.StyleCreated;

            UpdateValuesFromStyle();
            MarkRenderDirty();

#if DEBUG
            // Debug.WriteLine($"ImGuiNode.ApplyStyles() : {IdPath}, count = {_styleValues?.Count ?? 0}");
#endif
        }
    }

    private void ApplyStyles(string? pseudo)
    {
        if (string.IsNullOrWhiteSpace(pseudo))
        {
            pseudo = null;
        }

        var styleDef = StyleDefinition;
        if (!_flags.HasFlag(ImGuiNodeFlags.StyleCreated) || styleDef.IsDirty)
        {
            _gui._statistic.StyleChanged++;

            _pseudo = pseudo;

            (_styleSet as StyleSet)?.Clear();
            styleDef.ApplyStyleSet(Id, TypeName, Classes, ref _styleSet);
            _pseudoStyles = (_styleSet?.GetStyleCollection(_pseudo) ?? _styleSet?.GetStyleCollection(null)) as ValueCollection;

            if (_animation is { })
            {
                StopAnimation();
            }

            _flags |= ImGuiNodeFlags.StyleCreated;

            UpdateValuesFromStyle();
            MarkRenderDirty();

#if DEBUG
            // Debug.WriteLine($"ImGuiNode.ApplyStyles() : {IdPath}, count = {_styleValues?.Count ?? 0}");
#endif
            if (PseudoAffectsChildren)
            {
                SetChildrenPseudoDeep(_pseudo);
            }
        }
        else if (_pseudo != pseudo)
        {
            var oldStyle = _pseudoStyles;
            string? oldPseudo = _pseudo;

            //Debug.WriteLine($"{Id} set pseudo : {pseudo}");
            _pseudo = pseudo;

            var targetStyles = (_styleSet?.GetStyleCollection(pseudo) ?? _styleSet?.GetStyleCollection(null)) as ValueCollection;
            if (!Equals(targetStyles, _pseudoStyles))
            {
                _gui._statistic.PseudoChanged++;

                _pseudoStyles = targetStyles;
                UpdateValuesFromStyle();
                MarkRenderDirty();
#if DEBUG
                //Debug.WriteLine($"ImGuiNode.ApplyStyles(targetState) changed : '{oldState}'>'{targetState}'");
#endif

                var transition = _styleSet?.GetTransition(oldPseudo, pseudo);
                if (transition != null)
                {
                    IValueSource v1 = oldStyle ?? (IValueSource)EmptyValueSource.Empty;
                    IValueSource v2 = _pseudoStyles ?? (IValueSource)EmptyValueSource.Empty;

                    var ani = transition.CreateTransition(v1, v2);
                    if (ani is { })
                    {
                        StartAnimation(ani);
                    }
                }
                else
                {
                    // StopAnimation();
                }
            }

            if (PseudoAffectsChildren)
            {
                SetChildrenPseudoDeep(_pseudo);
            }
        }
    }

    public override IGuiAnimation? Animation => _animation;

    public override float AnimationStartTime => _animationStartTime;

    public override void StartAnimation(IGuiAnimation animation, bool forceRestart = false)
    {
        if (animation is null)
        {
            throw new ArgumentNullException(nameof(animation));
        }

        if (ReferenceEquals(_animation, animation))
        {
            if (forceRestart)
            {
                _animationStartTime = _gui.Time;
                _animation.Start(_animationStartTime);
            }

            return;
        }

        (_animation as IDisposable)?.Dispose();

        _animation = animation;
        _animationStartTime = _gui.Time;

        if (_animation is { })
        {
            _animation.Start(_animationStartTime);
            _gui.AddTimerNode(this);
        }
    }

    public override void StopAnimation()
    {
        if (_animation is { })
        {
            (_animation as IDisposable)?.Dispose();
            _animation = null;

            _gui.RemoveTimerNode(this);
        }
    }

    private void UpdateValuesFromStyle()
    {
        _gui._statistic.UpdateValuesFromStyleCall++;

        var inputFunc = _pseudoStyles?.GetValue<GuiInputFunctionStyle>()?.Resolve(this);
        if (_inputFuncFromStyle?.OverrideFunction != inputFunc)
        {
            _inputFuncFromStyle = inputFunc is { } ? new InputFunctionChain(inputFunc, () => _baseInputFunction) : null;
        }

        var layoutFunc = _pseudoStyles?.GetValue<GuiLayoutFunctionStyle>()?.Resolve(this);
        if (_layoutFuncFromStyle?.OverrideFunction != layoutFunc)
        {
            _layoutFuncFromStyle = layoutFunc is { } ? new LayoutFunctionChain(layoutFunc, () => _baseLayoutFunction) : null;
        }

        var fitFunc = _pseudoStyles?.GetValue<GuiFitFunctionStyle>()?.Resolve(this);
        if (_fitFuncFromStyle?.OverrideFunction != fitFunc)
        {
            _fitFuncFromStyle = fitFunc is { } ? new FitFunctionChain(fitFunc, () => _baseFitFunction) : null;
        }

        var renderFunc = _pseudoStyles?.GetValue<GuiRenderFunctionStyle>()?.Resolve(this);
        if (_renderFuncFromStyle?.OverrideFunction != renderFunc)
        {
            _renderFuncFromStyle = renderFunc is { } ? new RenderFunctionChain(renderFunc, () => _baseRenderFunction) : null;
        }

        if (_pseudoStyles is { })
        {
            _color.Style = _pseudoStyles.EnsureValueItem<GuiColorStyle>();
            _size.Style = _pseudoStyles.EnsureValueItem<GuiSizeStyle>();
            _margin.Style = _pseudoStyles.EnsureValueItem<GuiMarginStyle>();
            _padding.Style = _pseudoStyles.EnsureValueItem<GuiPaddingStyle>();
            _fitOrientation.Style = _pseudoStyles.EnsureValueItem<GuiFitOrientationStyle>();
            _alignment.Style = _pseudoStyles.EnsureValueItem<GuiAlignmentStyle>();
            _childSpacing.Style = _pseudoStyles.EnsureValueItem<GuiChildSpacingStyle>();
            _siblingSpacing.Style = _pseudoStyles.EnsureValueItem<GuiSiblingSpacingStyle>();
            _header.Style = _pseudoStyles.EnsureValueItem<GuiHeaderStyle>();
            _textAlignment.Style = _pseudoStyles.EnsureValueItem<GuiTextAlignmentStyle>();
            _border.Style = _pseudoStyles.EnsureValueItem<GuiBorderStyle>();
            _font.Style = _pseudoStyles.EnsureValueItem<GuiFontStyle>();
            _image.Style = _pseudoStyles.EnsureValueItem<GuiImageValue>();
            _imageFilter.Style = _pseudoStyles.EnsureValueItem<GuiImageFilterStyle>();
            _frame.Style = _pseudoStyles.EnsureValueItem<GuiFrameStyle>();
            _expandable.Style = _pseudoStyles.EnsureValueItem<GuiExpandableValue>();
        }
        else
        {
            _color.Style = null;
            _size.Style = null;
            _margin.Style = null;
            _padding.Style = null;
            _fitOrientation.Style = null;
            _alignment.Style = null;
            _childSpacing.Style = null;
            _siblingSpacing.Style = null;
            _header.Style = null;
            _textAlignment.Style = null;
            _border.Style = null;
            _font.Style = null;
            _image.Style = null;
            _imageFilter.Style = null;
            _frame.Style = null;
            _expandable.Style = null;
        }
    }

    internal ImGuiNodeStyle I_EnsureMyStyle()
    {
        return _styleDefinition ??= new(this, InheritedStyleDefinition);
    }

    private void SetChildrenPseudoDeep(string? pseudo)
    {
        _gui._statistic.SetChildrenPseudoDeepCall++;

        if (_childNodeList is { })
        {
            foreach (var childNode in _childNodeList)
            {
                childNode.ApplyStyles(pseudo);
                childNode.SetChildrenPseudoDeep(pseudo);
            }
        }
    }

    #endregion

    #region Value

    public override T? GetValue<T>() where T : class
    {
        _gui._statistic.GetValueCall++;
        return _values.GetValue<T>();
    }

    public override object? GetValue(Type type)
    {
        _gui._statistic.GetValueCall++;
        return _values.GetValue(type);
    }

    public override T? GetValueInHierarchy<T>() where T : class
    {
        ImGuiNode? node = this;

        while (node is not null)
        {
            if (node.GetValue<T>() is { } value)
            {
                return value;
            }

            node = node.Parent;
        }

        return this._gui?.GetValue<T>();
    }

    public override object? GetValueInHierarchy(Type type)
    {
        ImGuiNode? node = this;

        while (node is not null)
        {
            if (node.GetValue(type) is { } value)
            {
                return value;
            }

            node = node.Parent;
        }

        return this._gui?.GetValue(type);
    }

    public override T GetOrCreateValue<T>() where T : class
    {
        _gui._statistic.GetValueCall++;

#if DEBUG
        if (LogIgnoreValueType && IgnoredValues.Contains(typeof(T)))
        {
            Debug.WriteLine($"Value type is ignored : {typeof(T).Name}");
        }
#endif

        T value = _values.GetOrCreateValue<T>(out bool created);
        if (created)
        {
            MarkRenderDirty();
        }

        return value;
    }

    public override T GetOrCreateValue<T>(Func<T> creation) where T : class
    {
        _gui._statistic.GetValueCall++;

#if DEBUG
        if (LogIgnoreValueType && IgnoredValues.Contains(typeof(T)))
        {
            Debug.WriteLine($"Value type is ignored : {typeof(T).Name}");
        }
#endif

        T value = _values.GetOrCreateValue(creation, out bool created);
        if (created)
        {
            MarkRenderDirty();
        }

        return value;
    }

    public override T GetOrCreateValue<T>(out bool created) where T : class
    {
        _gui._statistic.GetValueCall++;

#if DEBUG
        if (LogIgnoreValueType && IgnoredValues.Contains(typeof(T)))
        {
            Debug.WriteLine($"Value type is ignored : {typeof(T).Name}");
        }
#endif

        T value = _values.GetOrCreateValue<T>(out created);
        if (created)
        {
            MarkRenderDirty();
        }

        return value;
    }

    public override T GetOrCreateValue<T>(Func<T> creation, out bool created) where T : class
    {
        _gui._statistic.GetValueCall++;

#if DEBUG
        if (LogIgnoreValueType && IgnoredValues.Contains(typeof(T)))
        {
            Debug.WriteLine($"Value type is ignored : {typeof(T).Name}");
        }
#endif

        T value = _values.GetOrCreateValue(creation, out created);
        if (created)
        {
            MarkRenderDirty();
        }

        return value;
    }

    public override bool SetValue<T>(T value) where T : class
    {
        _gui._statistic.SetValueCall++;
#if DEBUG
        if (LogIgnoreValueType && IgnoredValues.Contains(typeof(T)))
        {
            Debug.WriteLine($"Value type is ignored : {typeof(T).Name}");
        }
#endif

        _values.SetValue(value, out bool valueSet);
        if (valueSet)
        {
            MarkRenderDirty();
            return true;
        }

        return false;
    }

    public override bool RemoveValue<T>() where T : class
    {
        _gui._statistic.SetValueCall++;

        bool removed = _values.RemoveValue<T>();
        if (removed)
        {
            MarkRenderDirty();
            return true;
        }

        return false;
    }

    #endregion

    #region Values

    public override string? Text { get => _text; set => SetProperty(ref _text, value); }

    public override GuiAlignment? TextAlignment => _textAlignment.GetValue(_flags, _animation)?.Alignment;

    public override Color? Color { get => _color.GetValue(_flags, _animation)?.Color ?? _baseColor; set => SetProperty(ref _baseColor, value); }

    public override GuiLength? Width { get => _size.GetValue(_flags, _animation)?.Width ?? _baseWidth; set { SetProperty(ref _baseWidth, value, false); } }

    public override GuiLength? BaseWidth { get => _baseWidth; set => SetProperty(ref _baseWidth, value, false); }

    public override GuiLength? Height
    { 
        get => _size.GetValue(_flags, _animation)?.Height ?? _baseHeight;
        set => SetProperty(ref _baseHeight, value, false);
    }
    public override GuiLength? BaseHeight { get => _baseHeight; set => SetProperty(ref _baseHeight, value, false); }

    public override GuiThickness? Margin { get => _margin.GetValue(_flags, _animation)?.Margin ?? _baseMargin; set => SetProperty(ref _baseMargin, value); }
    public override GuiThickness? Padding { get => _padding.GetValue(_flags, _animation)?.Padding ?? _basePadding; set => SetProperty(ref _basePadding, value); }

    public override GuiOrientation FitOrientation 
    {
        get => _fitOrientation.GetValue(_flags, _animation)?.FitOrientation ?? _baseFitOrientation;
        set => SetProperty(ref _baseFitOrientation, value, false); 
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public override GuiAlignmentStyle? Alignment 
    { 
        get => _alignment.GetValue(_flags, _animation) ?? _baseAlignment;
        set => SetProperty(ref _baseAlignment, value, false); 
    }

    public override bool IsDisabled
    {
        get => _flags.HasFlag(ImGuiNodeFlags.Disabled) || _flags.HasFlag(ImGuiNodeFlags.DisabledFromParent);
        set
        {
            bool current = _flags.HasFlag(ImGuiNodeFlags.Disabled);
            if (current == value)
            {
                return;
            }

            if (value)
            {
                _flags |= ImGuiNodeFlags.Disabled;
            }
            else
            {
                _flags &= ~ImGuiNodeFlags.Disabled;
            }

            MarkRenderDirty();
            MarkChildNodesDisabled(IsDisabled);
        }
    }

    public override bool IsReadOnly
    {
        get => _flags.HasFlag(ImGuiNodeFlags.ReadOnly) || _flags.HasFlag(ImGuiNodeFlags.ReadOnlyFromParent);
        set
        {
            bool current = _flags.HasFlag(ImGuiNodeFlags.ReadOnly);
            if (current == value)
            {
                return;
            }

            if (value)
            {
                _flags |= ImGuiNodeFlags.ReadOnly;
            }
            else
            {
                _flags &= ~ImGuiNodeFlags.ReadOnly;
            }

            MarkRenderDirty();
            MarkChildNodesReadonly(IsReadOnly);
        }
    }

    public override bool OverrideDisabled
    {
        get => _flags.HasFlag(ImGuiNodeFlags.OverrideDisabled);
        set
        {
            bool current = _flags.HasFlag(ImGuiNodeFlags.OverrideDisabled);
            if (current == value)
            {
                return;
            }

            if (value)
            {
                _flags |= ImGuiNodeFlags.OverrideDisabled;
            }
            else
            {
                _flags &= ~ImGuiNodeFlags.OverrideDisabled;
            }

            MarkRenderDirty();
        }
    }

    public override bool IsFloating
    {
        get => _flags.HasFlag(ImGuiNodeFlags.Floating);
        set
        {
            bool current = _flags.HasFlag(ImGuiNodeFlags.Floating);
            if (current == value)
            {
                return;
            }

            if (value)
            {
                _flags |= ImGuiNodeFlags.Floating;
            }
            else
            {
                _flags &= ~ImGuiNodeFlags.Floating;
            }

            Layout();
            MarkRenderDirty();
        }
    }

    public override bool IsEdited
    {
        get => _flags.HasFlag(ImGuiNodeFlags.Edited);
        set
        {
            bool current = _flags.HasFlag(ImGuiNodeFlags.Edited);
            if (current == value)
            {
                return;
            }

            if (value)
            {
                _flags |= ImGuiNodeFlags.Edited;
            }
            else
            {
                _flags &= ~ImGuiNodeFlags.Edited;
            }
        }
    }

    public override bool IsOutOfBound
    {
        get => _flags.HasFlag(ImGuiNodeFlags.OutOfBound);
        set
        {
            bool current = _flags.HasFlag(ImGuiNodeFlags.OutOfBound);
            if (current == value)
            {
                return;
            }

            if (value)
            {
                _flags |= ImGuiNodeFlags.OutOfBound;
            }
            else
            {
                _flags &= ~ImGuiNodeFlags.OutOfBound;
            }
        }
    }

    public override float? ChildSpacing 
    { 
        get => _childSpacing.GetValue(_flags, _animation)?.ChildSpacing ?? _baseChildSpacing ?? Theme.ChildSpacing;
        set => SetProperty(ref _baseChildSpacing, value);
    }

    public override float SiblingSpacing
    {
        get => _siblingSpacing.GetValue(_flags, _animation)?.SiblingSpacing ?? _parent?.ChildSpacing ?? Theme.ChildSpacing;
        set
        {
            var prop = GetOrCreateValue<GuiSiblingSpacingStyle>();
            if (prop.SiblingSpacing != value)
            {
                prop.SiblingSpacing = value;
                MarkRenderDirty();
            }
        }
    }

    public override bool RevertInputOrder
    {
        get => _flags.HasFlag(ImGuiNodeFlags.RevertInputOrder);
        set
        {
            bool current = _flags.HasFlag(ImGuiNodeFlags.RevertInputOrder);
            if (current == value)
            {
                return;
            }

            if (value)
            {
                _flags |= ImGuiNodeFlags.RevertInputOrder;
            }
            else
            {
                _flags &= ~ImGuiNodeFlags.RevertInputOrder;
            }
        }
    }

    public override bool IsCompact
    {
        get => _flags.HasFlag(ImGuiNodeFlags.Compact);
        set
        {
            bool current = _flags.HasFlag(ImGuiNodeFlags.Compact);
            if (current == value)
            {
                return;
            }

            if (value)
            {
                _flags |= ImGuiNodeFlags.Compact;
            }
            else
            {
                _flags &= ~ImGuiNodeFlags.Compact;
            }
        }
    }

    public override bool IsOverlapped
    {
        get => _flags.HasFlag(ImGuiNodeFlags.Overlapped);
        set
        {
            bool current = _flags.HasFlag(ImGuiNodeFlags.Overlapped);
            if (current == value)
            {
                return;
            }

            if (value)
            {
                _flags |= ImGuiNodeFlags.Overlapped;
            }
            else
            {
                _flags &= ~ImGuiNodeFlags.Overlapped;
            }
        }
    }

    public override bool IsMouseDragOutSideEvent
    {
        get => _flags.HasFlag(ImGuiNodeFlags.MouseDragOutSideEvent);
        set
        {
            bool current = _flags.HasFlag(ImGuiNodeFlags.MouseDragOutSideEvent);
            if (current == value)
            {
                return;
            }

            if (value)
            {
                _flags |= ImGuiNodeFlags.MouseDragOutSideEvent;
            }
            else
            {
                _flags &= ~ImGuiNodeFlags.MouseDragOutSideEvent;
            }
        }
    }

    public override bool IsNoTransform
    {
        get => _flags.HasFlag(ImGuiNodeFlags.NoTransform);
        set
        {
            bool current = _flags.HasFlag(ImGuiNodeFlags.NoTransform);
            if (current == value)
            {
                return;
            }

            if (value)
            {
                _flags |= ImGuiNodeFlags.NoTransform;
            }
            else
            {
                _flags &= ~ImGuiNodeFlags.NoTransform;
            }

            UpdateGlobalTransform();
        }
    }

    public override bool IsDoubleLayout
    {
        get => _flags.HasFlag(ImGuiNodeFlags.DoubleLayout);
        set
        {
            bool current = _flags.HasFlag(ImGuiNodeFlags.DoubleLayout);
            if (current == value)
            {
                return;
            }

            if (value)
            {
                _flags |= ImGuiNodeFlags.DoubleLayout;
            }
            else
            {
                _flags &= ~ImGuiNodeFlags.DoubleLayout;
            }
        }
    }

    public override bool MarkDeleted
    {
        get => _flags.HasFlag(ImGuiNodeFlags.MarkDeleted);
        set
        {
            bool current = _flags.HasFlag(ImGuiNodeFlags.MarkDeleted);
            if (current == value)
            {
                return;
            }

            if (value)
            {
                _flags |= ImGuiNodeFlags.MarkDeleted;
            }
            else
            {
                _flags &= ~ImGuiNodeFlags.MarkDeleted;
            }
        }
    }

    private void MarkChildNodesDisabled(bool disabled)
    {
        if (_childNodeList is null)
        {
            return;
        }

        if (disabled)
        {
            foreach (var childNode in _childNodeList)
            {
                childNode._flags |= ImGuiNodeFlags.DisabledFromParent;

                if (!childNode._flags.HasFlag(ImGuiNodeFlags.Disabled))
                {
                    childNode.MarkChildNodesDisabled(disabled);
                }
            }
        }
        else
        {
            foreach (var childNode in _childNodeList)
            {
                childNode._flags &= ~ImGuiNodeFlags.DisabledFromParent;

                if (!childNode._flags.HasFlag(ImGuiNodeFlags.Disabled))
                {
                    childNode.MarkChildNodesDisabled(disabled);
                }
            }
        }
    }

    private void MarkChildNodesReadonly(bool readOnly)
    {
        if (_childNodeList is null)
        {
            return;
        }

        if (readOnly)
        {
            foreach (var childNode in _childNodeList)
            {
                childNode._flags |= ImGuiNodeFlags.ReadOnlyFromParent;

                if (!childNode._flags.HasFlag(ImGuiNodeFlags.ReadOnly))
                {
                    childNode.MarkChildNodesReadonly(readOnly);
                }
            }
        }
        else
        {
            foreach (var childNode in _childNodeList)
            {
                childNode._flags &= ~ImGuiNodeFlags.ReadOnlyFromParent;

                if (!childNode._flags.HasFlag(ImGuiNodeFlags.ReadOnly))
                {
                    childNode.MarkChildNodesReadonly(readOnly);
                }
            }
        }
    }

    #endregion

    #region Values extended

    public override GuiAlignment? HorizontalAlignment
    {
        get => Alignment?.HorizontalAlignment;
        set
        {
            var prop = GetOrCreateProperty(ref _baseAlignment);
            if (prop.HorizontalAlignment != value)
            {
                prop.HorizontalAlignment = value;
                MarkRenderDirty();
            }
        }
    }

    public override GuiAlignment? VerticalAlignment
    {
        get => Alignment?.VerticalAlignment;
        set
        {
            var prop = GetOrCreateProperty(ref _baseAlignment);
            if (prop.VerticalAlignment != value)
            {
                prop.VerticalAlignment = value;
                MarkRenderDirty();
            }
        }
    }

    public override bool AlignmentStretch
    {
        get => Alignment?.Stretch ?? false;
        set
        {
            var prop = GetOrCreateProperty(ref _baseAlignment);
            if (prop.Stretch != value)
            {
                prop.Stretch = value;
                MarkRenderDirty();
            }
        }
    }

    public override GuiHeaderStyle? HeaderStyle => _header.GetValue(_flags, _animation) ?? _baseHeader;

    public override float? HeaderWidth
    {
        get => _header.GetValue(_flags, _animation)?.Width ?? _baseHeader?.Width;
        set
        {
            var prop = GetOrCreateProperty(ref _baseHeader);
            if (prop.Width != value)
            {
                prop.Width = value;
                MarkRenderDirty();
            }
        }
    }

    public override float? HeaderHeight
    {
        get => _header.GetValue(_flags, _animation)?.Height ?? _baseHeader?.Height;
        set
        {
            var prop = GetOrCreateProperty(ref _baseHeader);
            if (prop.Height != value)
            {
                prop.Height = value;
                MarkRenderDirty();
            }
        }
    }

    public override float? HeaderPadding
    {
        get => _header.GetValue(_flags, _animation)?.Padding ?? _baseHeader?.Padding ?? Theme.HeaderPadding;
        set
        {
            var prop = GetOrCreateProperty(ref _baseHeader);
            if (prop.Padding != value)
            {
                prop.Padding = value;
                MarkRenderDirty();
            }
        }
    }

    public override Color? HeaderColor
    {
        get => _header.GetValue(_flags, _animation)?.Color ?? _baseHeader?.Color;
        set
        {
            var prop = GetOrCreateProperty(ref _baseHeader);
            if (prop.Color != value)
            {
                prop.Color = value;
                MarkRenderDirty();
            }
        }
    }

    public override float? BorderWidth
    {
        get => _border.GetValue(_flags, _animation)?.Width ?? Theme.BorderWidth;
        set
        {
            var prop = _border.GetOrCreateValue();

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (!prop.Width.HasValue || prop.Width.Value != value)
            {
                prop.Width = value;
                MarkRenderDirty();
            }
        }
    }

    public override float ScaledBorderWidth
    {
        get
        {
            var style = _border.GetValue(_flags, _animation);
            if (style is { } && style.Width is { } w)
            {
                return style.Scaled ? GlobalScaleValue(w) : w;
            }
            else
            {
                return GlobalScaleValue(Theme.BorderWidth);
            }
        }
    }

    public override Color? BorderColor
    {
        get => _border.GetValue(_flags, _animation)?.Color ?? Theme.Colors.GetColor(ColorStyle.Border);
        set
        {
            var prop = _border.GetOrCreateValue();
            if (!prop.Color.HasValue || prop.Color.Value != value)
            {
                prop.Color = value;
                MarkRenderDirty();
            }
        }
    }

    public override Color? ScrollBarColor
    {
        get => _border.GetValue(_flags, _animation)?.Color ?? Theme.Colors.GetColor(ColorStyle.ScrollBar);
        set
        {
            var prop = _border.GetOrCreateValue();
            if (!prop.Color.HasValue || prop.Color.Value != value)
            {
                prop.Color = value;
                MarkRenderDirty();
            }
        }
    }

    public override Font? Font
    {
        get => _font.GetValue(_flags, _animation)?.Font ?? Theme.Font;
        set
        {
            var prop = _font.GetOrCreateValue();
            if (!Equals(prop.Font, value))
            {
                prop.Font = value;
                MarkRenderDirty();
            }
        }
    }

    public override Color? FontColor
    {
        get => _font.GetValue(_flags, _animation)?.Color ?? Theme.Colors.GetColor(ColorStyle.Normal);
        set
        {
            var prop = _font.GetOrCreateValue();
            if (!prop.Color.HasValue || prop.Color.Value != value)
            {
                prop.Color = value;
                MarkRenderDirty();
            }
        }
    }

    public override Image? Image
    {
        get => _image.GetValue(_flags, _animation)?.Image;
        set
        {
            var prop = _image.GetOrCreateValue();
            if (!Equals(prop.Image, value))
            {
                prop.Image = value;
                MarkRenderDirty();
            }
        }
    }

    public override Color? ImageFilterColor
    {
        get => _imageFilter.GetValue(_flags, _animation)?.Color;
        set
        {
            var prop = _imageFilter.GetOrCreateValue();
            if (!Equals(prop.Color, value))
            {
                prop.Color = value;
                MarkRenderDirty();
            }
        }
    }

    public override float? CornerRound
    {
        get => _frame.GetValue(_flags, _animation)?.CornerRound;
        set
        {
            var prop = _frame.GetOrCreateValue();
            if (!prop.CornerRound.HasValue || prop.CornerRound.Value != value)
            {
                prop.CornerRound = value;
                MarkRenderDirty();
            }
        }
    }

    public override bool? Expanded
    {
        get => _expandable.GetValue(_flags, _animation)?.Expanded;
        set
        {
            if (!value.HasValue)
            {
                _expandable.RemoveValue();
                return;
            }

            var prop = _expandable.GetOrCreateValue();
            if (prop.Expanded != value)
            {
                prop.Expanded = value.Value;
                MarkRenderDirty();
            }
        }
    }

    #endregion

    #region Input

    public override long InputVersion => _inputVersion;

    public override bool IsMouseInRect
    {
        get
        {
            if (_inputVersion != _gui.InputVersion)
            {
                UpdateInputVersion(_gui.Input);
            }

            return _flags.HasFlag(ImGuiNodeFlags.MouseInRect);
        }
    }

    public override bool IsMouseInClickRect
    {
        get
        {
            if (_inputVersion != _gui.InputVersion)
            {
                UpdateInputVersion(_gui.Input);
            }

            return _flags.HasFlag(ImGuiNodeFlags.MouseInClickRect);
        }
    }

    public override bool IsMouseInInnerRect
    {
        get
        {
            if (_inputVersion != _gui.InputVersion)
            {
                UpdateInputVersion(_gui.Input);
            }

            return _flags.HasFlag(ImGuiNodeFlags.MouseInInnerRect);
        }
    }

    internal GuiInputState HandleInput(IGraphicInput input, out ImGuiNode? hoverNode)
    {
        bool isKeyEvent = input.GetIsKeyEvent();

        if (!IsMouseInRect && !isKeyEvent)
        {
            hoverNode = null;
            _flags &= ~ImGuiNodeFlags.HasInput;
            return GuiInputState.None;
        }

        GuiInputState state = GuiInputState.None;

        ImGuiNode? childHoverNode = null;
        bool childNodeInvoked = false;
        GuiInputState childState = GuiInputState.None;

        GuiInputState InputChildNodes(GuiPipeline pipeLineChild, IEnumerable<ImGuiNode>? childNodeSelector)
        {
            if (childNodeInvoked)
            {
                return GuiInputState.None;
            }
            childNodeInvoked = true;

            if (pipeLineChild == GuiPipeline.Blocked)
            {
                return GuiInputState.None;
            }

            if (_childNodeList is { })
            {
                ImGuiNode? localNode = null;

                // Nodes appearing later are on higher layers, so process upper-layer nodes first
                int count = _childNodeList.Count;

                IEnumerable<ImGuiNodeBK> childNodes = childNodeSelector?.OfType<ImGuiNodeBK>()
                    ?? (_flags.HasFlag(ImGuiNodeFlags.RevertInputOrder) ? _childNodeList : _childNodeList.ReverseEnumerable());

                foreach (var childNode in childNodes)
                {
                    var localState = childNode.HandleInput(input, out localNode);
                    if (localNode is { } || (isKeyEvent && localState != GuiInputState.None))
                    {
                        childHoverNode = localNode;
                        childState = localState;
                        return localState;
                    }
                }
            }

            return GuiInputState.None;
        }

        var inputState = InputFunction?.Invoke(GuiPipeline.Main, this, input, InputChildNodes) ?? GuiInputState.None;
        ImGui.MergeState(ref state, inputState);
        _gui._statistic.InputFunctionCall++;

        if (inputState == GuiInputState.FullSync)
        {
            _flags |= ImGuiNodeFlags.HasInput;

            // Note: After this node receives input, there is no need to process its child nodes.
        }
        else
        {
            _flags &= ~ImGuiNodeFlags.HasInput;

            if (!childNodeInvoked)
            {
                InputChildNodes(GuiPipeline.Main, null);
            }
        }

        // Only with an input function can hoverNode be set, so layout nodes won't receive hover, allowing penetration to search for entity nodes behind.
        hoverNode = childHoverNode ?? (this is { InputFunction: not null, IsMouseInClickRect: true } ? this : null);

        ImGui.MergeState(ref state, childState);

        return state;
    }

    internal GuiInputState HandleTimerUpdate(IGraphicInput input)
    {
        // GuiCoreExtensions.OnUpdate uses InputFunction for updates.
        var state = InputFunction?.Invoke(GuiPipeline.Main, this, input, (_, _) => GuiInputState.None) ?? GuiInputState.None;
        _gui._statistic.InputFunctionCall++;

        if (_animation is { } animation)
        {
            var aniState = animation.Update(_animationStartTime, _gui);
            if (aniState == GuiInputState.None)
            {
                (animation as IDisposable)?.Dispose();
                _animation = null;
                _gui.RemoveTimerNode(this);

                MarkRenderDirty();
            }

            ImGui.MergeState(ref state, aniState);
        }

        if (state >= GuiInputState.Render)
        {
            MarkRenderDirty();
        }

        return state;
    }

    internal void UpdateInputVersion(IGraphicInput input)
    {
        if (_inputVersion == _gui.InputVersion)
        {
            return;
        }

        UpdateGlobalTransform();

        _gui._statistic.UpdateInputVersionCall++;

        _inputVersion = _gui.InputVersion;

        _flags &= ~ImGuiNodeFlags.IsRendered;

        if (_parent is { IsMouseInInnerRect: false } || input.EventType == GuiEventTypes.MouseOut)
        {
            _flags &= ~ImGuiNodeFlags.MouseInRect;
            _flags &= ~ImGuiNodeFlags.MouseInClickRect;
            _flags &= ~ImGuiNodeFlags.MouseInInnerRect;
            return;
        }

        if (!(input.MouseLocation is { } pos))
        {
            return;
        }

        // Handle mouse drag outside case
        if (input.GetIsMouseDragEvent() && _flags.HasFlag(ImGuiNodeFlags.MouseDragOutSideEvent))
        {
            _flags |= ImGuiNodeFlags.MouseInRect;
            _flags |= ImGuiNodeFlags.MouseInClickRect;
            _flags |= ImGuiNodeFlags.MouseInInnerRect;

            if (_parent is { })
            {
                ApplyMouseState(input);
            }

            return;
        }

        // _rect transformed value, needs Transform
        var rect = GlobalRect;
        if (rect.Contains(pos))
        {
            _flags |= ImGuiNodeFlags.MouseInRect;
        }
        else
        {
            _flags &= ~ImGuiNodeFlags.MouseInRect;
        }

        if (GlobalMouseClickRect is { } mouseClickRect)
        {
            if (mouseClickRect.Contains(pos))
            {
                _flags |= ImGuiNodeFlags.MouseInClickRect;
            }
            else
            {
                _flags &= ~ImGuiNodeFlags.MouseInClickRect;
            }
        }
        else
        {
            if (_flags.HasFlag(ImGuiNodeFlags.MouseInRect))
            {
                _flags |= ImGuiNodeFlags.MouseInClickRect;
            }
            else
            {
                _flags &= ~ImGuiNodeFlags.MouseInClickRect;
            }
        }

        if (GlobalInnerRect.Contains(pos))
        {
            _flags |= ImGuiNodeFlags.MouseInInnerRect;
        }
        else
        {
            _flags &= ~ImGuiNodeFlags.MouseInInnerRect;
        }

        if (_parent is { })
        {
            ApplyMouseState(input);
        }
    }

    public override GuiMouseState MouseState => _mouseState;

    private void ApplyMouseState(IGraphicInput input)
    {
        if (_mouseState == GuiMouseState.Clicked)
        {
            _mouseState = GuiMouseState.Hover;
        }

        if (_parent is { IsMouseInInnerRect: false })
        {
            SetMouseState(GuiMouseState.None);
            //Console.WriteLine(_cButtonState);
            return;
        }

        if (!IsMouseInClickRect)
        {
            SetMouseState(GuiMouseState.None);
            //Console.WriteLine(_cButtonState);
            return;
        }

        switch (input.EventType)
        {
            case GuiEventTypes.MouseDown:
                bool leftButton = input.GetMouseButtonDown(GuiMouseButtons.Left);
                if (leftButton)
                {
                    SetMouseState(GuiMouseState.Pressed);
                }

                //Debug.WriteLine($"state = {MouseState}");
                break;

            case GuiEventTypes.MouseUp:
                SetMouseState(_mouseState switch
                {
                    GuiMouseState.None => GuiMouseState.Hover,
                    GuiMouseState.Hover => GuiMouseState.Hover,
                    GuiMouseState.Pressed => GuiMouseState.Clicked,
                    GuiMouseState.Clicked => GuiMouseState.Hover,
                    _ => GuiMouseState.Hover
                });

                //Debug.WriteLine($"state = {MouseState}");
                break;

            case GuiEventTypes.MouseOut:
                SetMouseState(GuiMouseState.None);
                break;

            default:
                return;
        }
    }

    private void SetMouseState(GuiMouseState state)
    {
        _gui._statistic.SetMouseStateCall++;
        SetProperty(ref _mouseState, state, false);
    }

    #endregion

    #region Update & child nodes

    public override ImGuiNode BeginNode(string id)
    {
        //TODO: Will cause errors when executing the same id in a unified flow, need to avoid in advance

        if (string.IsNullOrWhiteSpace(id))
        {
            // ReSharper disable once LocalizableElement
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(id));
        }

        if (!ReferenceEquals(Gui.CurrentNode, this))
        {
            throw new InvalidOperationException($"this method can only be invoked from Gui.CurrentNode");
        }

        // Complete the previous node
        _gui.EndNode(this);

        var node = GetChildNode(id) as ImGuiNodeBK;
        if (node is not null)
        {
            if (node.MarkDeleted)
            {
                _childNodeList?.Remove(node);
                _childNodeDic?.Remove(id);
                node = CreateNode(id);
            }
            else
            {
                _childNodeList ??= [];
                if (_layoutIndex >= _childNodeList.Count)
                {
                    _gui._statistic.LayoutOutOfRange++;
                    if (LogLayoutOutOfRange)
                    {
                        Logs.LogWarning($"Layout Index {_layoutIndex} out of range, child count {_childNodeList.Count}, Node id = {id}");
                    }
                }

                //if ((_childNodeList ??= [])[_layoutIndex] != node)
                if (_childNodeList.GetListItemSafe(_layoutIndex) != node)
                {
        // Re-sort
        _childNodeList.Remove(node);
                    if (_layoutIndex > _childNodeList.Count)
                    {
                        _gui._statistic.LayoutOutOfRange++;
                        if (LogLayoutOutOfRange)
                        {
                            Logs.LogWarning($"Layout Index {_layoutIndex} out of range, child count {_childNodeList.Count}, Node id = {id}");
                        }
                        _layoutIndex = _childNodeList.Count;
                    }
                    _childNodeList.Insert(_layoutIndex, node);
                }
            }
        }
        else
        {
            node = CreateNode(id);
        }

        node.BeginSync(_layoutIndex, _styleDefinition ?? _inheritedStyleDefinition, _initPos);

        _layoutIndex++;
        _pos = _layoutPos.Position;

        _gui.LastNode = node;

        return node;
    }

    private ImGuiNodeBK CreateNode(string id)
    {
        //TODO: ImGuiNode can use Pool
        ImGuiNodeBK node = new ImGuiNodeBK(_gui, id, this);
        (_childNodeList ??= []).Insert(_layoutIndex, node);

        // After exceeding a certain count, use dictionary to accelerate lookup
        if (_childNodeList.Count > UseDicCount)
        {
            if (_childNodeDic is null)
            {
                _childNodeDic = [];
                foreach (var n in _childNodeList)
                {
                    _childNodeDic.Add(n.Id, n);
                }
            }
            else
            {
                _childNodeDic.Add(node.Id, node);
            }
        }

        if (IsDisabled)
        {
            node._flags |= ImGuiNodeFlags.DisabledFromParent;
        }

        if (IsReadOnly)
        {
            node._flags |= ImGuiNodeFlags.ReadOnlyFromParent;
        }

        MarkRenderDirty();
        return node;
    }

    internal ImGuiNode? PassNode(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        if (!ReferenceEquals(Gui.CurrentNode, this))
        {
            throw new InvalidOperationException($"this method can only be invoked from Gui.CurrentNode");
        }

        // Complete the previous node
        _gui.EndNode(this);

        if (GetChildNode(id) is not ImGuiNodeBK node)
        {
            return null;
        }

        // if ((_childNodeList ??= [])[_layoutIndex] != node)
        if ((_childNodeList ??= []).GetListItemSafe(_layoutIndex) != node)
        {
            // Re-sort
            _childNodeList.Remove(node);
            if (_layoutIndex < _childNodeList.Count)
            {
                _childNodeList.Insert(_layoutIndex, node);
            }
            else
            {
                _layoutIndex = _childNodeList.Count;
                _childNodeList.Add(node);
            }
        }

        // Note: when bypassing execution, need to set the node's index
        node._index = _layoutIndex;

        _layoutIndex++;
        //_pos = _layoutPos.Position;

        return node;
    }

    // Ensure called only once per update
    private void BeginSync(int index, StyleCollection? inheritedStyles, PointF initPos)
    {
        if (_flags.HasFlag(ImGuiNodeFlags.NodeSync))
        {
            throw new InvalidOperationException();
        }

        _flags |= ImGuiNodeFlags.NodeSync;

        _index = index;
        _inheritedStyleDefinition = inheritedStyles;
        if (_styleDefinition is { })
        {
            _styleDefinition.Parent = _inheritedStyleDefinition ?? Gui.Theme;
        }
        _layoutIndex = 0;
        _layoutPos.Position = _pos = initPos;
        //_mouseState = _cMouseState;

        UpdateGlobalTransform();

        InputFunction?.Invoke(GuiPipeline.BeginSync, this, CommonGraphicInput.BeginSync, (_, _) => GuiInputState.None);
    }

    // Ensure called only once per update
    internal void EndSync(bool layout)
    {
        if (!_flags.HasFlag(ImGuiNodeFlags.NodeSync))
        {
            // EndCurrentNode operation is now supported, so no longer throwing exception here
            //throw new InvalidOperationException();

            return;
        }

        bool requireInitialize = !_flags.HasFlag(ImGuiNodeFlags.NodeInitialized);
        if (requireInitialize)
        {
            layout = true;
        }

        if (layout)
        {
            _gui._statistic.FitFunctionCall++;
            Layout();

            // During initialization, execute Layout once more on each child node to ensure auto-fit nodes can obtain info from their subsequent (ImGuiNode.Next) nodes
            if (requireInitialize && _childNodeList is { })
            {
                _layoutIndex = 0;
                _layoutPos.Position = _pos = _initPos;

                foreach (var childNode in _childNodeList)
                {
                    childNode.ApplyStyles();
                    InternalLayoutChildNode(childNode);

                    _pos = _layoutPos.Position;
                    _layoutIndex++;
                }
            }

            Fit();

            if (LayoutFunction is { } layoutFunc && _childNodeList is { })
            {
                foreach (var childNode in _childNodeList)
                {
                    layoutFunc(GuiPipeline.Align, childNode, _layoutPos, _ => { });
                }
            }

            if (_flags.HasFlag(ImGuiNodeFlags.DoubleLayout))
            {
                LayoutContentsDeep();
                _flags &= ~ImGuiNodeFlags.DoubleLayout;
            }
        }

        _flags |= ImGuiNodeFlags.NodeInitialized;
        _flags &= ~ImGuiNodeFlags.NodeSync;

        // _rect original value, no Transform needed
        if (_rect != _lastRect)
        {
            float left = Math.Min(_rect.X, _lastRect.X);
            float top = Math.Min(_rect.Y, _lastRect.Y);
            float right = Math.Max(_rect.Right, _lastRect.Right);
            float bottom = Math.Max(_rect.Bottom, _lastRect.Bottom);

            _dirtyRect = new RectangleF(left, top, right - left, bottom - top);

            _lastRect = _rect;
            MarkRenderDirty();
        }
        else
        {
            _dirtyRect = null;
        }

        if (_mouseState == GuiMouseState.Clicked)
        {
            _mouseState = GuiMouseState.Hover;
        }

        _flags &= ~ImGuiNodeFlags.Edited;
    }

    internal void InternalBeginContent()
    {
        _layoutIndex = 0;
        _layoutPos.Position = _pos = _initPos;
    }

    internal void InternalEndContent()
    {
        if (_childNodeList is null)
        {
            return;
        }

        int len = _childNodeList.Count;

        _pos = _layoutPos.Position;
        if (_pos != PointF.Empty)
        {
            // Used to keep position unchanged during PassContents operation
            _lastPos = _pos;
        }
        else
        {
            //if (_lastPos != PointF.Empty && len > 0)
            //{
            //    Debug.WriteLine($"Last layout pos is zero : {this.FullPath}");
            //}
        }

        if (_layoutIndex < len)
        {
            for (int i = len - 1; i >= _layoutIndex; i--)
            {
                var node = _childNodeList[i];
                _childNodeList.RemoveAt(i);
                _childNodeDic?.Remove(node.Id);
                node.Release();
            }

            MarkRenderDirty();
        }

        if (_childNodeList.Count <= UseDicCount && _childNodeDic is { })
        {
            _childNodeDic = null;
        }
    }

    public override void ClearContents()
    {
        if (_childNodeList is { })
        {
            foreach (var childNode in _childNodeList)
            {
                childNode.Release();
            }
            _childNodeList.Clear();
        }

        _childNodeDic = null;
        _layoutIndex = 0;
        _layoutPos.Position = _pos = _initPos;
    }

    internal void PassContents()
    {
        // _layoutIndex needs to exceed total count + 1
        _layoutIndex = (_childNodeList?.Count ?? 0) + 1;
        _layoutPos.Position = _pos = _lastPos;
    }

    internal void LayoutContentsDeep(bool fit = true, bool align = true)
    {
        if (_childNodeList is { })
        {
            // _layoutIndex needs to exceed total count + 1
            _layoutIndex = 1;
            _layoutPos.Position = _pos = _initPos;

            for (int i = 0; i < _childNodeList.Count; i++)
            {
                var childNode = _childNodeList[i];

                childNode.ApplyStyles();
                InternalLayoutChildNode(childNode);

                if (childNode.ChildNodeCount > 0)
                {
                    childNode.LayoutContentsDeep(fit, align);
                }

                _pos = _layoutPos.Position;
                _layoutIndex++;
            }

            if (_pos != PointF.Empty)
            {
                // Used to keep position unchanged during PassContents operation
                _lastPos = _pos;
            }

            if (fit)
            {
                this.Fit();
            }
            //this.Layout();

            if (align)
            {
                if (LayoutFunction is { } layoutFunc)
                {
                    foreach (var childNode in _childNodeList)
                    {
                        layoutFunc(GuiPipeline.Align, childNode, _layoutPos, _ => { });
                    }
                }
            }
        }
    }

    internal void Release()
    {
        this._gui?.ReleaseNode(this);

        _parent = null;
        _fullPath = null;

        foreach (var d in _values.Values.OfType<IDisposable>())
        {
            d.Dispose();
        }

        ClearContents();
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public override ImGuiNode? GetChildNode(string id)
    {
        if (id is null)
        {
            return null;
        }

        if (_childNodeList is null)
        {
            return null;
        }

        if (_childNodeDic is { })
        {
            return _childNodeDic.GetValueSafe(id);
        }
        else
        {
            return _childNodeList.FirstOrDefault(o => o.Id == id);
        }
    }

    public override ImGuiNode? GetChildNode(int index)
    {
        return _childNodeList?.GetListItemSafe(index);
    }

    public override ImGuiNode? FindNodeInChildren(string id)
    {
        if (GetChildNode(id) is { } node)
        {
            return node;
        }

        if (_childNodeList is null)
        {
            return null;
        }

        foreach (var childNode in _childNodeList)
        {
            node = childNode.FindNodeInChildren(id);
            if (node != null)
            {
                return node;
            }
        }

        return null;
    }

    public override int ChildNodeCount => _childNodeList?.Count ?? 0;

    public override IEnumerable<ImGuiNode> ChildNodes => _childNodeList ?? (IEnumerable<ImGuiNode>)[];

    internal IEnumerable<ImGuiNodeBK> InternalChildNodes => _childNodeList ?? (IEnumerable<ImGuiNodeBK>)[];

    public override ImGuiNode? GetNodeAt(int index)
    {
        var node = _childNodeList.GetListItemSafe(index);
        if (node != null)
        {
            // Check if node's Index has been updated to avoid infinite loop
            if (node.Index == index)
            {
                return node;
            }
        }

        return null;
    }

    // ReSharper disable once MemberCanBePrivate.Global
    internal ImGuiNodeBK? CurrentLayoutNode => _childNodeList.GetListItemSafe(_layoutIndex - 1);

    #endregion

    #region Layout

    public override int CurrentLayoutIndex => _layoutIndex;
    public override PointF InitialLayoutPosition => _initPos;
    public override PointF LayoutPosition => _pos;
    public override PointF CurrentLayoutPosition => _layoutPos.Position;

    public override void SetInitialLayoutPosition(PointF pos)
    {
        _layoutPos.Position = _pos = _initPos = pos;
    }

    public override void Layout()
    {
        ApplyStyles();
        _parent?.InternalLayoutChildNode(this);
    }

    private void InternalLayoutChildNode(ImGuiNode childNode)
    {
        // If this node is out of layout scope and already initialized, skip layout
        if (OptimaizeOutOfBound)
        {
            if (_flags.HasFlag(ImGuiNodeFlags.OutOfBound) && _flags.HasFlag(ImGuiNodeFlags.NodeInitialized))
            {
                return;
            }
        }

        // If child node is a floating node, skip layout
        if (childNode.Flags.HasFlag(ImGuiNodeFlags.Floating))
        {
            return;
        }

        _layoutPos.Position = _pos;
        LayoutFunction?.Invoke(GuiPipeline.Main, childNode, _layoutPos, _ => { });
        _gui._statistic.LayoutFunctionCall++;
    }

    #endregion

    #region Fit

    public override void Fit()
    {
        // If this node is out of layout scope and already initialized, Fit operation cannot be performed due to missing information.
        if (OptimaizeOutOfBound)
        {
            if (_flags.HasFlag(ImGuiNodeFlags.OutOfBound) && _flags.HasFlag(ImGuiNodeFlags.NodeInitialized))
            {
                return;
            }
        }

        FitFunction?.Invoke(GuiPipeline.Main, this, _ => { });
    }

    #endregion

    #region Render

    public override bool NeedRender => _flags.HasFlag(ImGuiNodeFlags.IsRenderDirty) || _flags.HasFlag(ImGuiNodeFlags.IsChildrenRenderDirty);

    public override bool IsRendered
    {
        get
        {
            if (_inputVersion != _gui.InputVersion)
            {
                UpdateInputVersion(_gui.Input);
            }

            return _flags.HasFlag(ImGuiNodeFlags.IsRendered);
        }
    }

    public override void Render(GuiPipeline pipeline, IGraphicOutput output)
    {
        // _rect transformed value, needs Transform
        HandleRender(pipeline, output, GlobalRect, false, false);
    }

    readonly Pool<List<ImGuiNodeBK>> _tempNodes = new(() => []);
    internal void HandleRender(GuiPipeline pipeline, IGraphicOutput output, RectangleF clip, bool dirtyMode, bool cleanUp)
    {
        UpdateGlobalTransform();

        var rect = GlobalRect;
        var innerRect = GlobalInnerRect;

        // _rect transformed value, needs Transform
        if (!clip.IntersectsWith(rect))
        {
            if (cleanUp)
            {
                ClearRenderDirty();
            }

            return;
        }

        ApplyStyles();

        if (dirtyMode && !NeedRender && _parent is { }/*not root*/)
        {
            if (cleanUp)
            {
                ClearRenderDirty();
            }

            return;
        }

        // _rect transformed value, needs Transform
        clip.Intersect(rect);

        bool childRendered = false;
        var tempNodes = _tempNodes.Acquire();
        tempNodes.Clear();


        void RenderChildNodes(GuiPipeline childPipeLine, IEnumerable<ImGuiNode>? childNodeSelector = null)
        {
            // Child nodes have already been rendered, and no specific child node selector is specified, so no longer render to avoid duplicate rendering.
            // But when a child node selector is specified, still need to render, because it may be a partial refresh that needs to render specific child nodes.
            if (childRendered && childNodeSelector is null)
            {
                return;
            }
            childRendered = true;

            if (childPipeLine.HasFlag(GuiPipeline.Blocked))
            {
                return;
            }

            if (_childNodeList is { })
            {
                if (Padding is { })
                {
                    clip.Intersect(innerRect);
                }

                IEnumerable<ImGuiNodeBK> childNodes = childNodeSelector?.OfType<ImGuiNodeBK>() ?? _childNodeList;

                if (dirtyMode)
                {
                    if (IsRenderDirty)
                    {
                        // Force render all child nodes
                        foreach (var node in childNodes)
                        {
                            node.HandleRender(childPipeLine, output, clip, false, cleanUp);
                        }
                    }
                    else if (_flags.HasFlag(ImGuiNodeFlags.Overlapped))
                    {
                        // Overlap mode, need to dynamically check if child nodes have overlaps.
                        // For nodes in front, if they overlap with subsequent nodes, render the subsequent nodes
                        foreach (var node in childNodes)
                        {
                            if (node.NeedRender)
                            {
                                node.HandleRender(childPipeLine, output, clip, false, cleanUp);
                                tempNodes.Add(node);
                            }
                            else
                            {
                                // Render subsequent nodes that overlap with previous ones
                                var rect = node.Rect;
                                if (tempNodes.Any(o => o.Rect.IntersectsWith(rect)))
                                {
                                    node.HandleRender(childPipeLine, output, clip, false, cleanUp);
                                    tempNodes.Add(node);
                                }
                            }
                        }

                        //_tempNodes.Clear();
                    }
                    else
                    {
                        // Only render dirty child nodes
                        foreach (var node in childNodes.Where(o => o.NeedRender))
                        {
                            node.HandleRender(childPipeLine, output, clip, dirtyMode, cleanUp);
                        }
                    }
                }
                else
                {
                    foreach (var node in childNodes)
                    {
                        node.HandleRender(childPipeLine, output, clip, false, cleanUp);
                    }
                }
            }
        }

        if (RenderFunction is { } renderFunc)
        {
            _animation?.Update(_animationStartTime, _gui);

            try
            {
                output.SetClipRect(clip);
                //Debug.WriteLine($"render:{IdPath} dirty:{IsRenderDirty} childDirty:{IsChildrenRenderDirty}");
                renderFunc(pipeline, this, output, dirtyMode, RenderChildNodes);
                _gui._statistic.RenderFunctionCall++;

                //output.DrawRectangle(new Pen(System.Drawing.Color.Cyan, 1), rect);
            }
            catch (Exception err)
            {
                Logs.LogError(err);
            }
            finally
            {
                output.RestoreClip();
            }
        }

        if (!childRendered)
        {
            try
            {
                output.SetClipRect(clip);
                RenderChildNodes(pipeline);
            }
            catch (Exception err)
            {
                Logs.LogError(err);
            }
            finally
            {
                output.RestoreClip();
            }
        }

        if (cleanUp)
        {
            ClearRenderDirty();
        }

        tempNodes.Clear();
        _tempNodes.Release(tempNodes);
    }

    #endregion

    #region Dirty

    public override void MarkRenderDirty()
    {
        MarkRenderDirtyBubble();
        _flags &= ~ImGuiNodeFlags.PartialDirtyRect;
    }

    public override void MarkRenderDirty(RectangleF dirtyRect)
    {
        MarkRenderDirtyBubble();
        _flags |= ImGuiNodeFlags.PartialDirtyRect;
        _dirtyRect = dirtyRect;
    }

    public override void MarkRenderDirty(bool mouseClickRectDirty)
    {
        MarkRenderDirtyBubble();
        _flags |= ImGuiNodeFlags.PartialDirtyRect;
        if (mouseClickRectDirty)
        {
            // _rect original value, no Transform needed
            _dirtyRect = _mouseClickRect ?? _rect;
        }
    }

    private void MarkRenderDirtyBubble()
    {
        _gui.QueueInputState(GuiInputState.Render);

        if (_parent is { }/*not root*/ && !_flags.HasFlag(ImGuiNodeFlags.IsRenderDirty))
        {
            _flags |= ImGuiNodeFlags.IsRenderDirty;

            var parent = _parent;
            while (parent is { IsChildrenRenderDirty: false })
            {
                parent._flags |= ImGuiNodeFlags.IsChildrenRenderDirty;
                parent = parent._parent;
            }
        }
    }

    internal void ClearRenderDirty()
    {
        if (IsChildrenRenderDirty && _childNodeList is { })
        {
            foreach (var childNode in _childNodeList)
            {
                childNode.ClearRenderDirty();
            }
        }

        _styleDefinition?.ClearDirty();
        _flags &= ~ImGuiNodeFlags.IsRenderDirty;
        _flags &= ~ImGuiNodeFlags.IsChildrenRenderDirty;
        _flags &= ~ImGuiNodeFlags.PartialDirtyRect;
        _flags |= ImGuiNodeFlags.IsRendered;

        _dirtyRect = null;

        //if (_animation is { IsFinished: true })
        //{
        //    StopAnimation();
        //}
    }

    internal void CollectRenderDirtyNodes(ICollection<ImGuiNode> collection)
    {
        if (IsRenderDirty)
        {
            collection.Add(this);

            // Full dirty area, no need to search child nodes, trigger full area refresh            if (!_flags.HasFlag(ImGuiNodeFlags.PartialDirtyRect))
            {
                return;
            }
        }

        if (IsChildrenRenderDirty && _childNodeList is { })
        {
            foreach (var node in _childNodeList)
            {
                node.CollectRenderDirtyNodes(collection);
            }
        }
    }

    #endregion

    #region Global state

    public override void SetIsFocused(bool isFocused)
    {
        if (isFocused)
        {
            _gui.SetFocusNode(this);
        }
        else
        {
            if (_gui.FocusNode == this)
            {
                _gui.SetFocusNode(null);
            }
        }
    }

    public override void SetIsControlling(bool isControlling)
    {
        if (isControlling)
        {
            if (_gui.ControllingNode != this)
            {
                _gui.SetControllingNode(this);
                MarkRenderDirty();
            }
        }
        else
        {
            if (_gui.ControllingNode == this)
            {
                _gui.SetControllingNode(null);
            }
        }
    }

    #endregion

    public override string ToString()
    {
        return FullPath.ToString();
    }
}