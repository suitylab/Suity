using Suity.Collections;
using Suity.Drawing;
using Suity.Helpers;
using Suity.Views.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace Suity.Views.Im;

/// <summary>
/// Internal implementation of ImGui external operations, providing utility functions for virtual lists, styles, paths, and scroll handling.
/// </summary>
internal class ImGuiExternalBK : ImGuiExternal
{
    private static readonly FontDef _DefaultFont = new(ImGuiTheme.DefaultFont, 12f); //new(SystemFonts.DefaultFont.FontFamily, 12f);

    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    internal static ImGuiExternalBK Instance { get; } = new();

    private ImGuiExternalBK()
    { }

    private bool _isInit;
    private static readonly Dictionary<FontDef, FontSizeCache> _sizedFonts = [];

    /// <summary>
    /// Initializes the external BK and sets up the empty path singleton.
    /// </summary>
    public void Initialize()
    {
        if (_isInit)
        {
            return;
        }
        _isInit = true;

        ImGuiExternalBK._external = this;

        ImGuiPath.Empty = CreateEmptyPath();
    }

    #region VirtualList

    public override VisualListData<T> CreateFixedData<T>(IList<T> list, float height)
    {
        return new FixedVisualListData<T>(list, height);
    }

    public override VisualListData<T> CreateRangedData<T>(LengthGetter<T> heightGetter, float defaultLen)
    {
        return new RangedVisualListData<T>(heightGetter, defaultLen);
    }

    public override VisualListData<T> CreateRangedData<T>(IEnumerable<T> items, LengthGetter<T> heightGetter, float defaultLen)
    {
        return new RangedVisualListData<T>(items, heightGetter, defaultLen);
    }

    public override void SetVirtualListData(ImGuiNode node, VisualListData data, NodeFactory? factory = null)
    {
        var value = node.GetValue<GuiScrollableValue>();
        if (value is null)
        {
            return;
        }

        node.SetValue(data);
        node.OverrideChildSpacing(data.Spacing);

        // Need to reverse the input function order; by default, the last rendered is at the front and receives input first.
        // Lists don't have overlapping issues, and the Header needs to be at the front to receive input first.
        node.RevertInputOrder = true;

        if (data.HeaderHeight is { } headerH)
        {
            var scroll = node.GetOrCreateValue<GuiScrollableValue>();
            scroll.RectPadding = new GuiThickness { Top = headerH };
        }

        factory ??= node.GetStyle<GuiNodeFactoryStyle>()?.Factory ?? DefaultVirtualListItemFactory;

        node.OnContent(() =>
        {
            // Special handling: during initialization, Header is prioritized to obtain the actual width in advance
            // During normal operation, Header needs to be placed later to prioritize Input processing; the Input flow prioritizes later items.
            if (node.IsInitializing)
            {
                // Let the Fit pipeline provide an initial value before initialization
                node.FitFunction?.Invoke(GuiPipeline.Main, node, p => { });
            }

            if (data.HeaderHeight is { } h)
            {
                var headerNode = VirtualListHeader(node.Gui, GuiVirtualListExtensions.HeaderId, data);
                headerNode.InitHeight(h);
            }

            data.PropagateContents(node, factory, value.ScrollX, value.ScrollY);
        });
    }

    private static ImGuiNode VirtualListHeader(ImGui gui, string id, VisualListData data)
    {
        ImGuiNode node = gui.HorizontalLayout(id);

        if (node.IsInitializing)
        {
            node.TypeName = GuiVirtualListExtensions.VirtualListHeaderTypeName;
            node.IsFloating = true;
            node.InitFitFunctionChain(FitVirtualListHeader);
            node.SetRenderFunction(RenderVirtualListHeader);
            node.FitOrientation = GuiOrientation.Horizontal;
        }

        data.HeaderTemplate?.Invoke(node);

        // Cancel IsInitializing to support programmatic dynamic width setting
        //if (node.IsInitializing)
        //{
        // Get initial width
        node.Fit();
        //}

        // Set the total list width. When Width is not null, the total list width is based on the Header width
        if (data.Width is { })
        {
            data.Width = node.Rect.Width;
        }

        return node;
    }

    private static ImGuiNode DefaultVirtualListItemFactory(ImGui gui, string? id = null, object? data = null)
    {
        id ??= $"##list_item_{gui.CurrentNode.CurrentLayoutIndex}";
        ImGuiNode node = gui.BeginCurrentNode(id);

        if (node.IsInitializing)
        {
            node.TypeName = GuiVirtualListExtensions.VirtualListItemTypeName;
            node.SetLayoutFunction(ImGuiLayoutSystem.Horizontal);
            node.SetFitFunction(ImGuiFitSystem.Auto);
            node.SetRenderFunction(nameof(GuiCommonExtensions.Frame));
            node.FitOrientation = GuiOrientation.Vertical;

            node.BorderWidth = 0;
        }

        node.Layout();

        return node;
    }

    private static void FitVirtualListHeader(GuiPipeline pipeline, ImGuiNode node, ChildFitFunction baseAction)
    {
        baseAction(pipeline);

        var scroll = node.Parent?.GetValue<GuiScrollableValue>();
        if (scroll is null)
        {
            return;
        }

        var rect = node.Rect;
        var computeRect = node.ComputeSize();
        rect.Width = computeRect.Width;
        rect.Height = computeRect.Height;
        node.Rect = rect;

        var parentRect = node.Parent?.InnerRect ?? rect;

        float x = parentRect.X - scroll.ScrollX;
        float y = parentRect.Y;

        if (rect.X != x || rect.Y != y)
        {
            node.OffsetPositionDeep(x - rect.X, y - rect.Y);
        }

        //Debug.WriteLine($"fit rect = {node.Rect}");
    }

    private static void RenderVirtualListHeader(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction baseAction)
    {
        if (!pipeline.HasFlag(GuiPipeline.Header))
        {
            baseAction(GuiPipeline.Blocked);
        }
        else
        {
            baseAction(GuiPipeline.Main);
        }
    }

    #endregion

    #region StyleSet

    public override IStyleSet CreateSyleSet(string name)
    {
        return new StyleSet(name);
    }

    public override StyleCollectionExternal CreateStyleCollectionEx(StyleCollection collection)
    {
        return new StyleCollectionExternalBK(collection);
    }

    public override ITransitionFactory CreateEase(float duration)
    {
        return new EaseTransitionFactory(duration);
    }

    #endregion

    #region ImGuiPath

    public override bool IsPathNullOrEmpty(ImGuiPath? path)
    {
        return path is null || path.IsEmpty;
    }

    public override ImGuiPath CombinePath(ImGuiPath pathA, ImGuiPath pathB)
    {
        if (pathA is null)
        {
            return pathB ?? ImGuiPath.Empty;
        }
        if (pathB is null)
        {
            return pathA ?? ImGuiPath.Empty;
        }

        ImGuiPathBK a = (ImGuiPathBK)pathA;
        ImGuiPathBK b = (ImGuiPathBK)pathB;

        string[] path = new string[a._path.Length + b._path.Length];
        for (int i = 0; i < a._path.Length; i++)
        {
            path[i] = a._path[i];
        }
        for (int i = 0; i < b._path.Length; i++)
        {
            path[a._path.Length + i] = b._path[i];
        }
        return new ImGuiPathBK(path);
    }

    public override ImGuiPath CreateEmptyPath() => new ImGuiPathBK();

    public override ImGuiPath CreatePath(params string[] path)
    {
        return new ImGuiPathBK(path);
    }

    public override ImGuiPath CreatePath(string pathChain)
    {
        string[] path;

        if (string.IsNullOrEmpty(pathChain))
        {
            path = [];
        }
        else
        {
            path = pathChain.Split('>');
        }
        return new ImGuiPathBK(path);
    }

    public override bool TryCreatePath(string pathChain, out ImGuiPath? path)
    {
        try
        {
            path = CreatePath(pathChain);
            return true;
        }
        catch (Exception)
        {
            path = null;
            return false;
        }
    }

    public override bool TryCreatePath(IEnumerable<string> pathChain, out ImGuiPath? path)
    {
        string[] paths = [.. pathChain];

        foreach (var item in paths)
        {
            if (item is null)
            {
                path = null;
                return false;
            }
        }

        path = new ImGuiPathBK(paths);
        return true;
    }

    #endregion

    #region Function

    public override InputFunction? ResolveInputFunction(ImGuiNode node, string funcName)
    {
        var func = (node as ImGuiNodeBK)?.StyleDefinition.GetInputFunction(funcName)
            ?? node.Gui.InputSystem.GetInputFunction(funcName);

        if (func is null)
        {
#if DEBUG
            Debug.WriteLine($"{nameof(ResolveInputFunction)} function not found : {funcName} in {node.FullPath}");
#endif
        }

        return func;
    }

    public override LayoutFunction? ResolveLayoutFunction(ImGuiNode node, string funcName)
    {
        var func = (node as ImGuiNodeBK)?.StyleDefinition.GetLayoutFunction(funcName)
            ?? node.Gui.LayoutSystem.GetLayoutFunction(funcName);

        if (func is null)
        {
#if DEBUG
            Debug.WriteLine($"{nameof(ResolveLayoutFunction)} function not found : {funcName} in {node.FullPath}");
#endif
        }

        return func;
    }

    public override FitFunction? ResolveFitFunction(ImGuiNode node, string funcName)
    {
        var func = (node as ImGuiNodeBK)?.StyleDefinition.GetFitFunction(funcName)
            ?? node.Gui.FitSystem.GetFitFunction(funcName);

        if (func is null)
        {
#if DEBUG
            Debug.WriteLine($"{nameof(ResolveFitFunction)} function not found : {funcName} in {node.FullPath}");
#endif
        }

        return func;
    }

    public override RenderFunction? ResolveRenderFunction(ImGuiNode node, string funcName)
    {
        var func = (node as ImGuiNodeBK)?.StyleDefinition.GetRenderFunction(funcName)
            ?? node.Gui.RenderSystem.GetRenderFunction(funcName);

        if (func is null)
        {
#if DEBUG
            Debug.WriteLine($"{nameof(ResolveRenderFunction)} function not found : {funcName} in {node.FullPath}");
#endif
        }

        return func;
    }

    public override void SetInputFunctionChain(ImGuiNode node, InputFunction func)
    {
        var current = node.BaseInputFunction;
        if (current is { })
        {
            var chain = new InputFunctionChain(func, current);
            node.BaseInputFunction = chain.Entry;
        }
        else
        {
            node.BaseInputFunction = func;
        }
    }

    public override void SetLayoutFunctionChain(ImGuiNode node, LayoutFunction func)
    {
        var current = node.BaseLayoutFunction;
        if (current is { })
        {
            var chain = new LayoutFunctionChain(func, current);
            node.BaseLayoutFunction = chain.Entry;
        }
        else
        {
            node.BaseLayoutFunction = func;
        }
    }

    public override void SetFitFunctionChain(ImGuiNode node, FitFunction func)
    {
        var current = node.BaseFitFunction;
        if (current is { })
        {
            var chain = new FitFunctionChain(func, current);
            node.BaseFitFunction = chain.Entry;
        }
        else
        {
            node.BaseFitFunction = func;
        }
    }

    public override void SetRenderFunctionChain(ImGuiNode node, RenderFunction func)
    {
        if (node.IsInitializing)
        {
            var current = node.BaseRenderFunction;
            if (current is { })
            {
                var chain = new RenderFunctionChain(func, current);
                node.BaseRenderFunction = chain.Entry;
            }
            else
            {
                node.BaseRenderFunction = func;
            }
        }
    }

    #endregion

    #region Scroll

    public override float GetScrollRateX(ImGuiNode node)
    {
        var value = node.GetOrCreateValue<GuiScrollableValue>();

        var rect = node.InnerRect;
        var cSize = value.ContentSize;
        return value.ScrollX / (float)(cSize.Width - rect.Width);
    }

    public override float GetScrollRateY(ImGuiNode node)
    {
        var value = node.GetOrCreateValue<GuiScrollableValue>();

        var rect = node.InnerRect;
        var cSize = value.ContentSize;
        return value.ScrollY / (float)(cSize.Height - rect.Height);
    }

    public override ImGuiNode SetScrollRateX(ImGuiNode node, float rate)
    {
        var value = node.GetOrCreateValue<GuiScrollableValue>();
        var rect = node.InnerRect;

        float cWidth = value.ContentSize.Width;

        float scroll;

        if (cWidth > rect.Width)
        {
            MathHelper.Clamp(ref rate, 0, 1);

            scroll = rate * (cWidth - rect.Width);
        }
        else
        {
            scroll = 0;
        }

        if (scroll != value.ScrollX)
        {
            value.ScrollX = scroll;
            node.QueueRefresh();
        }

        return node;
    }

    public override ImGuiNode SetScrollRateY(ImGuiNode node, float rate)
    {
        var value = node.GetOrCreateValue<GuiScrollableValue>();
        var rect = node.InnerRect;

        float cHeight = value.ContentSize.Height;

        float scroll;

        if (cHeight > rect.Height)
        {
            MathHelper.Clamp(ref rate, 0, 1);

            scroll = rate * (cHeight - rect.Height);
        }
        else
        {
            scroll = 0;
        }

        if (scroll != value.ScrollY)
        {
            value.ScrollY = scroll;
            //TODO: //node.QueueRefresh(); // Executing queue refresh here would set ScrollRate again after refresh, causing an infinite loop
            node.MarkRenderDirty();
        }

        return node;
    }

    public override RectangleF GetVerticalScrollBarRect(ImGuiNode node, GuiScrollableValue value)
    {
        var theme = node.Theme;
        var vRect = value.GetViewRect(node.InnerRect);
        var cSize = value.ContentSize;

        float barTall = vRect.Height * (vRect.Height / cSize.Height);
        float ry = value.ScrollY / (cSize.Height - vRect.Height);
        float barTop = (vRect.Height - barTall) * ry;
        float barWide = theme.ScrollBarWidth;

        var rect = new RectangleF(vRect.X + vRect.Width - barWide, vRect.Y + barTop, barWide, barTall);

        return rect;
    }

    public override RectangleF GetHorizontalScrollBarRect(ImGuiNode node, GuiScrollableValue value)
    {
        var theme = node.Theme;
        var vRect = value.GetViewRect(node.InnerRect);
        var cSize = value.ContentSize;

        float barTall = vRect.Width * (vRect.Width / cSize.Width);
        float rx = value.ScrollX / (cSize.Width - vRect.Width);
        float barTop = (vRect.Width - barTall) * rx;
        float barWide = theme.ScrollBarWidth;

        var rect = new RectangleF(vRect.X + barTop, vRect.Y + vRect.Height - barWide, barTall, barWide);

        return rect;
    }

    public override void FitScrollBarPosition(ImGuiNode node)
    {
        var value = node.GetValue<GuiScrollableValue>();
        if (value != null)
        {
            FitScrollBarPosition(node, value);
        }
    }

    public override void FitScrollBarPosition(ImGuiNode node, GuiScrollableValue value)
    {
        var vRect = value.GetViewRect(node.InnerRect);
        var cSize = value.ContentSize;

        // When the page doesn't need scrolling, reset scroll coordinates to 0
        if (cSize.Width > vRect.Width)
        {
            if (!value.HScrollBarVisible)
            {
                value.HScrollBarVisible = true;
                node.MarkRenderDirty();
            }

            if (MathHelper.Clamp(() => value.ScrollX, v => value.ScrollX = v, 0, cSize.Width - vRect.Width))
            {
                node.MarkRenderDirty();
            }
        }
        else
        {
            if (value.HScrollBarVisible)
            {
                value.HScrollBarVisible = false;
                node.MarkRenderDirty();
            }

            if (value.ScrollX != 0)
            {
                value.ScrollX = 0;
                node.MarkRenderDirty();
            }
        }

        if (cSize.Height > vRect.Height)
        {
            if (!value.VScrollBarVisible)
            {
                value.VScrollBarVisible = true;
                node.MarkRenderDirty();
            }

            if (MathHelper.Clamp(() => value.ScrollY, v => value.ScrollY = v, 0, cSize.Height - vRect.Height))
            {
                node.MarkRenderDirty();
            }
        }
        else
        {
            if (value.VScrollBarVisible)
            {
                value.VScrollBarVisible = false;
                node.MarkRenderDirty();
            }

            if (value.ScrollY != 0)
            {
                value.ScrollY = 0;
                node.MarkRenderDirty();
            }
        }
    }

    public override bool ScrollToPositionY(ImGuiNode node, RectangleF rect, bool relative)
    {
        var value = node.GetValue<GuiScrollableValue>();
        if (value is null)
        {
            return false;
        }

        var parentRect = value.GetViewRect(node.InnerRect);
        //Size size = value.ContentSize;

        // Compensate for current scroll Y; at this point, rect is the value starting from 0
        if (relative)
        {
            rect.Y += value.ScrollY;
        }

        if (rect.Top < parentRect.Top + value.ScrollY)
        {
            //int d = rect.Top - parentRect.Top - value.ScrollY;
            value.ScrollY = rect.Top - parentRect.Top;
            if (value.ScrollY < 0)
            {
                value.ScrollY = 0;
                node.MarkRenderDirty();
            }

            return true;
        }
        else if (rect.Bottom > parentRect.Bottom + value.ScrollY)
        {
            //int d = rect.Bottom - parentRect.Bottom - value.ScrollY;
            value.ScrollY = rect.Bottom - parentRect.Bottom;
            if (value.ScrollY < 0)
            {
                value.ScrollY = 0;
                node.MarkRenderDirty();
            }

            return true;
        }

        return false;
    }

    public override ImGuiNode AutoScrollToBottom(ImGuiNode node)
    {
        node.InitFitFunctionChain(AutoScrollToBottomFunction);
        return node;
    }

    private static void AutoScrollToBottomFunction(GuiPipeline pipeline, ImGuiNode node, ChildFitFunction baseAction)
    {
        baseAction(pipeline);

        if (!node.GetScrollManualInput())
        {
            node.SetScrollRateY(1);
        }
        else if (node.GetScrollRateY() == 1)
        {
            node.SetScrollManualInput(false);
        }
    }

    #endregion

    #region Font

    public override FontDef DefaultFont => _DefaultFont;

    public override FontDef GetFont(ImGuiNode node)
    {
        return node.Font ?? DefaultFont;
    }

    public override FontDef GetScaledFont(ImGuiNode node)
    {
        var font = node.Font ?? DefaultFont;
        if (node.GlobalScale is { } scale && scale != 1f)
        {
            font = _sizedFonts.GetOrAdd(font, _ => new FontSizeCache(font)).GetFont(scale);
        }

        return font;
    }

    #endregion
}