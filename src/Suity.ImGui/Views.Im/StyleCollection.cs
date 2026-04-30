using System.Collections;
using System.Collections.Generic;

namespace Suity.Views.Im;

/// <summary>
/// Abstract base class for style collections that manage CSS-like styles, pseudo states, and transitions for ImGui nodes.
/// </summary>
public abstract class StyleCollection : IEnumerable<IStyleSet>
{
    /// <summary>
    /// Internal extended style collection operations.
    /// </summary>
    internal readonly StyleCollectionExternal _ex;

    /// <summary>
    /// Initializes a new instance of the <see cref="StyleCollection"/> class.
    /// </summary>
    internal StyleCollection()
    {
        _ex = ImGuiExternal._external.CreateStyleCollectionEx(this);
    }

    /// <summary>
    /// Gets the ImGuiTheme if this collection is a theme, otherwise null.
    /// </summary>
    /// <returns>The <see cref="ImGuiTheme"/> if this is a theme collection; otherwise, null.</returns>
    public virtual ImGuiTheme? GetTheme() => null;

    #region Functions

    /// <summary>
    /// Resolves an input function by name from this style collection.
    /// </summary>
    /// <param name="name">The name of the input function to resolve.</param>
    /// <returns>The resolved <see cref="InputFunction"/>, or null if not found.</returns>
    public virtual InputFunction? GetInputFunction(string name) => null;

    /// <summary>
    /// Resolves a layout function by name from this style collection.
    /// </summary>
    /// <param name="name">The name of the layout function to resolve.</param>
    /// <returns>The resolved <see cref="LayoutFunction"/>, or null if not found.</returns>
    public virtual LayoutFunction? GetLayoutFunction(string name) => null;

    /// <summary>
    /// Resolves a fit function by name from this style collection.
    /// </summary>
    /// <param name="name">The name of the fit function to resolve.</param>
    /// <returns>The resolved <see cref="FitFunction"/>, or null if not found.</returns>
    public virtual FitFunction? GetFitFunction(string name) => null;

    /// <summary>
    /// Resolves a render function by name from this style collection.
    /// </summary>
    /// <param name="name">The name of the render function to resolve.</param>
    /// <returns>The resolved <see cref="RenderFunction"/>, or null if not found.</returns>
    public virtual RenderFunction? GetRenderFunction(string name) => null;

    #endregion

    #region Values

    /// <summary>
    /// Gets a style value by name.
    /// </summary>
    /// <typeparam name="T">The type of the style value.</typeparam>
    /// <param name="name">The name of the style.</param>
    /// <returns>The style value, or null if not found.</returns>
    public T? GetStyle<T>(string name) where T : class => _ex.GetStyle<T>(name);

    /// <summary>
    /// Sets a style value by name.
    /// </summary>
    /// <typeparam name="T">The type of the style value.</typeparam>
    /// <param name="name">The name of the style.</param>
    /// <param name="value">The style value to set.</param>
    public void SetStyle<T>(string name, T value) where T : class => _ex.SetStyle<T>(name, value);

    /// <summary>
    /// Gets or creates a style value by name.
    /// </summary>
    /// <typeparam name="T">The type of the style value.</typeparam>
    /// <param name="name">The name of the style.</param>
    /// <param name="created">True if a new style value was created; otherwise, false.</param>
    /// <returns>The existing or newly created style value.</returns>
    public T GetOrCreateStyle<T>(string name, out bool created) where T : class, new()
        => _ex.GetOrCreateStyle<T>(name, out created);

    /// <summary>
    /// Removes a style value by name.
    /// </summary>
    /// <typeparam name="T">The type of the style value.</typeparam>
    /// <param name="name">The name of the style to remove.</param>
    /// <returns>True if the style was found and removed; otherwise, false.</returns>
    public bool RemoveStyle<T>(string name) where T : class
        => _ex.RemoveStyle<T>(name);

    /// <summary>
    /// Gets a pseudo style value by name and pseudo state.
    /// </summary>
    /// <typeparam name="T">The type of the style value.</typeparam>
    /// <param name="name">The name of the style.</param>
    /// <param name="pseudo">The pseudo state (e.g., ":hover", ":active").</param>
    /// <returns>The pseudo style value, or null if not found.</returns>
    public T? GetPseudo<T>(string name, string pseudo) where T : class => _ex.GetPseudo<T>(name, pseudo);

    /// <summary>
    /// Sets a pseudo style value.
    /// </summary>
    /// <typeparam name="T">The type of the style value.</typeparam>
    /// <param name="name">The name of the style.</param>
    /// <param name="pseudo">The pseudo state (e.g., ":hover", ":active").</param>
    /// <param name="value">The style value to set.</param>
    public void SetPseudo<T>(string name, string pseudo, T value) where T : class => _ex?.SetPseudo<T>(name, pseudo, value);

    /// <summary>
    /// Gets or creates a pseudo style value.
    /// </summary>
    /// <typeparam name="T">The type of the style value.</typeparam>
    /// <param name="name">The name of the style.</param>
    /// <param name="pseudo">The pseudo state (e.g., ":hover", ":active").</param>
    /// <param name="created">True if a new pseudo style value was created; otherwise, false.</param>
    /// <returns>The existing or newly created pseudo style value.</returns>
    public T GetOrCreatePseudo<T>(string name, string pseudo, out bool created) where T : class, new() => _ex.GetOrCreatePseudo<T>(name, pseudo, out created);

    /// <summary>
    /// Removes a pseudo style value.
    /// </summary>
    /// <typeparam name="T">The type of the style value.</typeparam>
    /// <param name="name">The name of the style.</param>
    /// <param name="pseudo">The pseudo state (e.g., ":hover", ":active").</param>
    /// <returns>True if the pseudo style was found and removed; otherwise, false.</returns>
    public bool RemovePseudo<T>(string name, string pseudo) where T : class => _ex.RemovePseudo<T>(name, pseudo);

    /// <summary>
    /// Gets a style value by name with optional pseudo state.
    /// </summary>
    /// <typeparam name="T">The type of the style value.</typeparam>
    /// <param name="name">The name of the style.</param>
    /// <param name="pseudo">The optional pseudo state, or null for the base style.</param>
    /// <returns>The style value, or null if not found.</returns>
    public T? GetStyle<T>(string name, string? pseudo) where T : class => _ex.GetStyle<T>(name, pseudo);

    /// <summary>
    /// Sets a style value by name with optional pseudo state.
    /// </summary>
    /// <typeparam name="T">The type of the style value.</typeparam>
    /// <param name="name">The name of the style.</param>
    /// <param name="pseudo">The optional pseudo state, or null for the base style.</param>
    /// <param name="value">The style value to set.</param>
    public void SetStyle<T>(string name, string? pseudo, T value) where T : class => _ex.SetStyle<T>(name, pseudo, value);

    /// <summary>
    /// Gets or creates a style value by name with optional pseudo state.
    /// </summary>
    /// <typeparam name="T">The type of the style value.</typeparam>
    /// <param name="name">The name of the style.</param>
    /// <param name="pseudo">The optional pseudo state, or null for the base style.</param>
    /// <param name="created">True if a new style value was created; otherwise, false.</param>
    /// <returns>The existing or newly created style value.</returns>
    public T GetOrCreateStyle<T>(string name, string? pseudo, out bool created) where T : class, new()
        => _ex.GetOrCreateStyle<T>(name, pseudo, out created);

    /// <summary>
    /// Removes a style value by name with optional pseudo state.
    /// </summary>
    /// <typeparam name="T">The type of the style value.</typeparam>
    /// <param name="name">The name of the style to remove.</param>
    /// <param name="pseudo">The optional pseudo state, or null for the base style.</param>
    /// <returns>True if the style was found and removed; otherwise, false.</returns>
    public bool RemoveStyle<T>(string name, string? pseudo) where T : class
        => _ex.RemoveStyle<T>(name, pseudo);

    /// <summary>
    /// Clears all styles and pseudo styles.
    /// </summary>
    public void Clear() => _ex.Clear();

    #endregion

    #region Apply values

    /// <summary>
    /// Applies a style set based on id, type name, and classes.
    /// </summary>
    /// <param name="id">The element id.</param>
    /// <param name="typeName">The element type name.</param>
    /// <param name="classes">The element classes.</param>
    /// <param name="styleSet">The style set to apply, passed by reference.</param>
    public virtual void ApplyStyleSet(string id, string? typeName, string[]? classes, ref IStyleSet? styleSet)
        => _ex.ApplyStyleSet(id, typeName, classes, ref styleSet);

    /// <summary>
    /// Applies styles based on id, type name, and classes.
    /// </summary>
    /// <param name="id">The element id.</param>
    /// <param name="typeName">The element type name.</param>
    /// <param name="classes">The element classes.</param>
    /// <param name="values">The value collection to apply styles to, passed by reference.</param>
    /// <param name="pseudo">The optional pseudo state to apply.</param>
    public virtual void ApplyStyles(string id, string? typeName, string[]? classes, ref IValueCollection? values, string? pseudo = null)
        => _ex.ApplyStyles(id, typeName, classes, ref values, pseudo);

    #endregion

    #region Dirty

    /// <summary>
    /// Gets the current version number of the style collection.
    /// </summary>
    internal long Version => _ex.Version;

    /// <summary>
    /// Gets whether the style collection has been modified.
    /// </summary>
    /// <returns>True if the collection has been modified since the last clear; otherwise, false.</returns>
    public virtual bool IsDirty => _ex.IsDirty;

    /// <summary>
    /// Marks the style collection as dirty.
    /// </summary>
    public void MarkDirty() => _ex.MarkDirty();

    /// <summary>
    /// Clears the dirty flag.
    /// </summary>
    internal virtual void ClearDirty() => _ex.ClearDirty();

    #endregion

    /// <summary>
    /// Sets a transition for a style property.
    /// </summary>
    /// <param name="name">The name of the style property.</param>
    /// <param name="pseudo">The optional pseudo state, or null for the base style.</param>
    /// <param name="targetState">The optional target state for the transition.</param>
    /// <param name="transition">The transition factory to use.</param>
    public void SetTransition(string name, string? pseudo, string? targetState, ITransitionFactory transition)
        => _ex.SetTransition(name, pseudo, targetState, transition);

    /// <summary>
    /// Removes a transition for a style property.
    /// </summary>
    /// <param name="name">The name of the style property.</param>
    /// <param name="pseudo">The optional pseudo state, or null for the base style.</param>
    /// <param name="targetState">The optional target state for the transition.</param>
    public void RemoveTransition(string name, string? pseudo, string? targetState)
        => _ex.RemoveTransition(name, pseudo, targetState);

    #region IEnumerable<ValueStyleCollection>

    /// <inheritdoc/>
    IEnumerator<IStyleSet> IEnumerable<IStyleSet>.GetEnumerator()
    {
        return _ex.StyleSets.GetEnumerator();
    }

    /// <inheritdoc/>
    public IEnumerator GetEnumerator()
    {
        return ((IEnumerable)_ex.StyleSets).GetEnumerator();
    }

    #endregion
}
