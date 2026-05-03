using Suity.Drawing;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Views.Im;

/// <summary>
/// Abstract base class for external ImGui functionality implementations.
/// Provides abstraction for virtual lists, paths, styles, functions, scrolling, and fonts.
/// </summary>
internal abstract class ImGuiExternal
{
    /// <summary>
    /// The current external implementation instance.
    /// </summary>
    internal static ImGuiExternal _external;

    #region VirtualList

    /// <summary>
    /// Creates a fixed-height virtual list data source.
    /// </summary>
    /// <typeparam name="T">The type of items in the list.</typeparam>
    /// <param name="list">The list of items.</param>
    /// <param name="height">The fixed height of each item.</param>
    /// <returns>A new VisualListData instance.</returns>
    public abstract VisualListData<T> CreateFixedData<T>(IList<T> list, float height);

    /// <summary>
    /// Creates a ranged-height virtual list data source with a height getter.
    /// </summary>
    /// <typeparam name="T">The type of items in the list.</typeparam>
    /// <param name="heightGetter">Function to get the height of each item.</param>
    /// <param name="defaultLen">Default height for items.</param>
    /// <returns>A new VisualListData instance.</returns>
    public abstract VisualListData<T> CreateRangedData<T>(LengthGetter<T> heightGetter, float defaultLen);

    /// <summary>
    /// Creates a ranged-height virtual list data source from an enumerable.
    /// </summary>
    /// <typeparam name="T">The type of items in the list.</typeparam>
    /// <param name="items">The items to display.</param>
    /// <param name="heightGetter">Function to get the height of each item.</param>
    /// <param name="defaultLen">Default height for items.</param>
    /// <returns>A new VisualListData instance.</returns>
    public abstract VisualListData<T> CreateRangedData<T>(IEnumerable<T> items, LengthGetter<T> heightGetter, float defaultLen);

    /// <summary>
    /// Sets virtual list data on a node.
    /// </summary>
    /// <param name="node">The target node.</param>
    /// <param name="data">The visual list data.</param>
    /// <param name="factory">Optional node factory.</param>
    public abstract void SetVirtualListData(ImGuiNode node, VisualListData data, NodeFactory? factory = null);

    #endregion

    #region ImGuiPath

    /// <summary>
    /// Checks if a path is null or empty.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>True if null or empty.</returns>
    public abstract bool IsPathNullOrEmpty(ImGuiPath? path);

    /// <summary>
    /// Combines two paths.
    /// </summary>
    /// <param name="a">First path.</param>
    /// <param name="b">Second path.</param>
    /// <returns>Combined path.</returns>
    public abstract ImGuiPath CombinePath(ImGuiPath a, ImGuiPath b);

    /// <summary>
    /// Creates an empty path.
    /// </summary>
    /// <returns>An empty ImGuiPath.</returns>
    public abstract ImGuiPath CreateEmptyPath();

    /// <summary>
    /// Creates a path from segments.
    /// </summary>
    /// <param name="path">Path segments.</param>
    /// <returns>A new ImGuiPath.</returns>
    public abstract ImGuiPath CreatePath(params string[] path);

    /// <summary>
    /// Creates a path from a chain string.
    /// </summary>
    /// <param name="pathChain">The path chain string.</param>
    /// <returns>A new ImGuiPath.</returns>
    public abstract ImGuiPath CreatePath(string pathChain);

    /// <summary>
    /// Attempts to create a path from a chain string.
    /// </summary>
    /// <param name="pathChain">The path chain string.</param>
    /// <param name="path">When this method returns, contains the created path if successful; otherwise, null.</param>
    /// <returns>True if the path was created successfully; otherwise, false.</returns>
    public abstract bool TryCreatePath(string pathChain, out ImGuiPath? path);

    /// <summary>
    /// Attempts to create a path from a collection of segments.
    /// </summary>
    /// <param name="pathChain">The collection of path segments.</param>
    /// <param name="path">When this method returns, contains the created path if successful; otherwise, null.</param>
    /// <returns>True if the path was created successfully; otherwise, false.</returns>
    public abstract bool TryCreatePath(IEnumerable<string> pathChain, out ImGuiPath? path);

    #endregion

    #region Style

    /// <summary>
    /// Creates a new style set with the given name.
    /// </summary>
    /// <param name="name">The style set name.</param>
    /// <returns>A new IStyleSet instance.</returns>
    public abstract IStyleSet CreateSyleSet(string name);

    /// <summary>
    /// Creates an extended style collection wrapper.
    /// </summary>
    /// <param name="collection">The base style collection.</param>
    /// <returns>A new StyleCollectionExternal instance.</returns>
    public abstract StyleCollectionExternal CreateStyleCollectionEx(StyleCollection collection);

    /// <summary>
    /// Creates an ease transition factory with the specified duration.
    /// </summary>
    /// <param name="duration">The transition duration.</param>
    /// <returns>A new ITransitionFactory instance.</returns>
    public abstract ITransitionFactory CreateEase(float duration);

    #endregion

    #region Function

    /// <summary>
    /// Resolves an input function by name for the given node.
    /// </summary>
    /// <param name="node">The target node.</param>
    /// <param name="funcName">The name of the function to resolve.</param>
    /// <returns>The resolved input function, or null if not found.</returns>
    public abstract InputFunction? ResolveInputFunction(ImGuiNode node, string funcName);

    /// <summary>
    /// Resolves a layout function by name for the given node.
    /// </summary>
    /// <param name="node">The target node.</param>
    /// <param name="funcName">The name of the function to resolve.</param>
    /// <returns>The resolved layout function, or null if not found.</returns>
    public abstract LayoutFunction? ResolveLayoutFunction(ImGuiNode node, string funcName);

    /// <summary>
    /// Resolves a fit function by name for the given node.
    /// </summary>
    /// <param name="node">The target node.</param>
    /// <param name="funcName">The name of the function to resolve.</param>
    /// <returns>The resolved fit function, or null if not found.</returns>
    public abstract FitFunction? ResolveFitFunction(ImGuiNode node, string funcName);

    /// <summary>
    /// Resolves a render function by name for the given node.
    /// </summary>
    /// <param name="node">The target node.</param>
    /// <param name="funcName">The name of the function to resolve.</param>
    /// <returns>The resolved render function, or null if not found.</returns>
    public abstract RenderFunction? ResolveRenderFunction(ImGuiNode node, string funcName);

    /// <summary>
    /// Sets the input function chain on a node.
    /// </summary>
    /// <param name="node">The target node.</param>
    /// <param name="func">The input function to set.</param>
    public abstract void SetInputFunctionChain(ImGuiNode node, InputFunction func);

    /// <summary>
    /// Sets the layout function chain on a node.
    /// </summary>
    /// <param name="node">The target node.</param>
    /// <param name="func">The layout function to set.</param>
    public abstract void SetLayoutFunctionChain(ImGuiNode node, LayoutFunction func);

    /// <summary>
    /// Sets the fit function chain on a node.
    /// </summary>
    /// <param name="node">The target node.</param>
    /// <param name="func">The fit function to set.</param>
    public abstract void SetFitFunctionChain(ImGuiNode node, FitFunction func);

    /// <summary>
    /// Sets the render function chain on a node.
    /// </summary>
    /// <param name="node">The target node.</param>
    /// <param name="func">The render function to set.</param>
    public abstract void SetRenderFunctionChain(ImGuiNode node, RenderFunction func);

    #endregion

    #region Scroll

    /// <summary>
    /// Gets the horizontal scroll rate (0-1) for a node.
    /// </summary>
    /// <param name="node">The target node.</param>
    /// <returns>The horizontal scroll rate between 0 and 1.</returns>
    public abstract float GetScrollRateX(ImGuiNode node);

    /// <summary>
    /// Gets the vertical scroll rate (0-1) for a node.
    /// </summary>
    /// <param name="node">The target node.</param>
    /// <returns>The vertical scroll rate between 0 and 1.</returns>
    public abstract float GetScrollRateY(ImGuiNode node);

    /// <summary>
    /// Sets the horizontal scroll rate for a node.
    /// </summary>
    /// <param name="node">The target node.</param>
    /// <param name="rate">The scroll rate between 0 and 1.</param>
    /// <returns>The modified node.</returns>
    public abstract ImGuiNode SetScrollRateX(ImGuiNode node, float rate);

    /// <summary>
    /// Sets the vertical scroll rate for a node.
    /// </summary>
    /// <param name="node">The target node.</param>
    /// <param name="rate">The scroll rate between 0 and 1.</param>
    /// <returns>The modified node.</returns>
    public abstract ImGuiNode SetScrollRateY(ImGuiNode node, float rate);

    /// <summary>
    /// Gets the rectangle for the vertical scroll bar.
    /// </summary>
    /// <param name="node">The target node.</param>
    /// <param name="value">The scrollable value.</param>
    /// <returns>The rectangle representing the vertical scroll bar.</returns>
    public abstract RectangleF GetVerticalScrollBarRect(ImGuiNode node, GuiScrollableValue value);

    /// <summary>
    /// Gets the rectangle for the horizontal scroll bar.
    /// </summary>
    /// <param name="node">The target node.</param>
    /// <param name="value">The scrollable value.</param>
    /// <returns>The rectangle representing the horizontal scroll bar.</returns>
    public abstract RectangleF GetHorizontalScrollBarRect(ImGuiNode node, GuiScrollableValue value);

    /// <summary>
    /// Adjusts the scroll bar position to fit the node's content.
    /// </summary>
    /// <param name="node">The target node.</param>
    public abstract void FitScrollBarPosition(ImGuiNode node);

    /// <summary>
    /// Adjusts the scroll bar position to fit the node's content with specific scrollable value.
    /// </summary>
    /// <param name="node">The target node.</param>
    /// <param name="value">The scrollable value.</param>
    public abstract void FitScrollBarPosition(ImGuiNode node, GuiScrollableValue value);

    /// <summary>
    /// Scrolls to make the specified rectangle visible.
    /// </summary>
    /// <param name="node">The scrollable node.</param>
    /// <param name="rect">The rectangle to scroll to.</param>
    /// <param name="relative">Whether the rectangle coordinates are relative.</param>
    /// <returns>True if scrolling occurred.</returns>
    public abstract bool ScrollToPositionY(ImGuiNode node, RectangleF rect, bool relative);

    /// <summary>
    /// Automatically scrolls the node to the bottom.
    /// </summary>
    /// <param name="node">The target node.</param>
    /// <returns>The modified node.</returns>
    public abstract ImGuiNode AutoScrollToBottom(ImGuiNode node);

    #endregion

    #region Font

    /// <summary>
    /// Gets the default font for the ImGui system.
    /// </summary>
    /// <returns>The default font.</returns>
    public abstract FontDef DefaultFont { get; }

    /// <summary>
    /// Gets the font for a node, considering theme and style settings.
    /// </summary>
    /// <param name="node">The target node.</param>
    /// <returns>The font for the node.</returns>
    public abstract FontDef GetFont(ImGuiNode node);

    /// <summary>
    /// Gets the scaled font for a node, adjusted for the node's scale factor.
    /// </summary>
    /// <param name="node">The target node.</param>
    /// <returns>The scaled font for the node.</returns>
    public abstract FontDef GetScaledFont(ImGuiNode node);

    #endregion
}

/// <summary>
/// Abstract base class for extended style collection operations.
/// </summary>
internal abstract class StyleCollectionExternal
{
    #region Values

    /// <summary>
    /// Gets a style set by name.
    /// </summary>
    /// <param name="name">The style set name.</param>
    /// <returns>The style set, or null if not found.</returns>
    public abstract IStyleSet? GetStyleSet(string name);

    /// <summary>
    /// Gets a style set by name and pseudo state.
    /// </summary>
    /// <param name="name">The style set name.</param>
    /// <param name="pseudo">The pseudo state.</param>
    /// <returns>The style set, or null if not found.</returns>
    public abstract IStyleSet? GetStyleSet(string name, string? pseudo);

    /// <summary>
    /// Gets a style value by name.
    /// </summary>
    /// <typeparam name="T">The type of the style value.</typeparam>
    /// <param name="name">The style name.</param>
    /// <returns>The style value, or null if not found.</returns>
    public abstract T? GetStyle<T>(string name) where T : class;

    /// <summary>
    /// Sets a style value by name.
    /// </summary>
    /// <typeparam name="T">The type of the style value.</typeparam>
    /// <param name="name">The style name.</param>
    /// <param name="value">The style value to set.</param>
    public abstract void SetStyle<T>(string name, T value) where T : class;

    /// <summary>
    /// Gets or creates a style value by name.
    /// </summary>
    /// <typeparam name="T">The type of the style value.</typeparam>
    /// <param name="name">The style name.</param>
    /// <param name="created">True if a new value was created; otherwise, false.</param>
    /// <returns>The existing or newly created style value.</returns>
    public abstract T GetOrCreateStyle<T>(string name, out bool created) where T : class, new();

    /// <summary>
    /// Removes a style value by name.
    /// </summary>
    /// <typeparam name="T">The type of the style value.</typeparam>
    /// <param name="name">The style name.</param>
    /// <returns>True if the style was removed; otherwise, false.</returns>
    public abstract bool RemoveStyle<T>(string name) where T : class;

    /// <summary>
    /// Gets a pseudo style value.
    /// </summary>
    /// <typeparam name="T">The type of the style value.</typeparam>
    /// <param name="name">The style name.</param>
    /// <param name="pseudo">The pseudo state.</param>
    /// <returns>The pseudo style value, or null if not found.</returns>
    public abstract T? GetPseudo<T>(string name, string pseudo) where T : class;

    /// <summary>
    /// Sets a pseudo style value.
    /// </summary>
    /// <typeparam name="T">The type of the style value.</typeparam>
    /// <param name="name">The style name.</param>
    /// <param name="pseudo">The pseudo state.</param>
    /// <param name="value">The style value to set.</param>
    public abstract void SetPseudo<T>(string name, string pseudo, T value) where T : class;

    /// <summary>
    /// Gets or creates a pseudo style value.
    /// </summary>
    /// <typeparam name="T">The type of the style value.</typeparam>
    /// <param name="name">The style name.</param>
    /// <param name="pseudo">The pseudo state.</param>
    /// <param name="created">True if a new value was created; otherwise, false.</param>
    /// <returns>The existing or newly created pseudo style value.</returns>
    public abstract T GetOrCreatePseudo<T>(string name, string pseudo, out bool created) where T : class, new();

    /// <summary>
    /// Removes a pseudo style value.
    /// </summary>
    /// <typeparam name="T">The type of the style value.</typeparam>
    /// <param name="name">The style name.</param>
    /// <param name="pseudo">The pseudo state.</param>
    /// <returns>True if the pseudo style was removed; otherwise, false.</returns>
    public abstract bool RemovePseudo<T>(string name, string pseudo) where T : class;

    /// <summary>
    /// Gets a style value by name and optional pseudo state.
    /// </summary>
    /// <typeparam name="T">The type of the style value.</typeparam>
    /// <param name="name">The style name.</param>
    /// <param name="pseudo">The optional pseudo state.</param>
    /// <returns>The style value, or null if not found.</returns>
    public abstract T? GetStyle<T>(string name, string? pseudo) where T : class;

    /// <summary>
    /// Sets a style value by name and optional pseudo state.
    /// </summary>
    /// <typeparam name="T">The type of the style value.</typeparam>
    /// <param name="name">The style name.</param>
    /// <param name="pseudo">The optional pseudo state.</param>
    /// <param name="value">The style value to set.</param>
    public abstract void SetStyle<T>(string name, string? pseudo, T value) where T : class;

    /// <summary>
    /// Gets or creates a style value by name and optional pseudo state.
    /// </summary>
    /// <typeparam name="T">The type of the style value.</typeparam>
    /// <param name="name">The style name.</param>
    /// <param name="pseudo">The optional pseudo state.</param>
    /// <param name="created">True if a new value was created; otherwise, false.</param>
    /// <returns>The existing or newly created style value.</returns>
    public abstract T GetOrCreateStyle<T>(string name, string? pseudo, out bool created) where T : class, new();

    /// <summary>
    /// Removes a style value by name and optional pseudo state.
    /// </summary>
    /// <typeparam name="T">The type of the style value.</typeparam>
    /// <param name="name">The style name.</param>
    /// <param name="pseudo">The optional pseudo state.</param>
    /// <returns>True if the style was removed; otherwise, false.</returns>
    public abstract bool RemoveStyle<T>(string name, string? pseudo) where T : class;

    /// <summary>
    /// Sets a style set directly.
    /// </summary>
    /// <param name="styleSet">The style set to set.</param>
    public abstract void SetStyleSet(IStyleSet styleSet);

    /// <summary>
    /// Clears all styles and pseudo styles.
    /// </summary>
    public abstract void Clear();

    #endregion

    #region Apply values

    /// <summary>
    /// Applies a style set to the given parameters.
    /// </summary>
    /// <param name="id">The element ID.</param>
    /// <param name="typeName">The optional type name.</param>
    /// <param name="classes">The optional class names.</param>
    /// <param name="styleSet">The style set to apply.</param>
    public abstract void ApplyStyleSet(string id, string? typeName, string[]? classes, ref IStyleSet? styleSet);

    /// <summary>
    /// Applies styles to the given parameters.
    /// </summary>
    /// <param name="id">The element ID.</param>
    /// <param name="typeName">The optional type name.</param>
    /// <param name="classes">The optional class names.</param>
    /// <param name="values">The value collection to apply styles to.</param>
    /// <param name="pseudo">The optional pseudo state.</param>
    public abstract void ApplyStyles(string id, string? typeName, string[]? classes, ref IValueCollection? values, string? pseudo = null);

    #endregion

    #region Dirty

    /// <summary>
    /// Gets the current version number of the style collection.
    /// </summary>
    internal abstract long Version { get; }

    /// <summary>
    /// Gets whether the style collection has been modified.
    /// </summary>
    public abstract bool IsDirty { get; }

    /// <summary>
    /// Marks the style collection as dirty.
    /// </summary>
    public abstract void MarkDirty();

    /// <summary>
    /// Clears the dirty flag.
    /// </summary>
    internal abstract void ClearDirty();

    #endregion

    /// <summary>
    /// Sets a transition for a style property.
    /// </summary>
    /// <param name="name">The style property name.</param>
    /// <param name="pseudo">The optional pseudo state.</param>
    /// <param name="targetState">The optional target state.</param>
    /// <param name="transition">The transition factory.</param>
    public abstract void SetTransition(string name, string? pseudo, string? targetState, ITransitionFactory transition);

    /// <summary>
    /// Removes a transition for a style property.
    /// </summary>
    /// <param name="name">The style property name.</param>
    /// <param name="pseudo">The optional pseudo state.</param>
    /// <param name="targetState">The optional target state.</param>
    public abstract void RemoveTransition(string name, string? pseudo, string? targetState);

    /// <summary>
    /// Gets all style sets in this collection.
    /// </summary>
    public abstract IEnumerable<IStyleSet> StyleSets { get; }
}
