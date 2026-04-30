using Suity.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Views.Im;

/// <summary>
/// Internal implementation of style collection external operations, managing named style sets.
/// </summary>
internal class StyleCollectionExternalBK : StyleCollectionExternal
{
    private readonly StyleCollection _collection;

    internal readonly Dictionary<string, StyleSet> _styles = [];
    private long _version = 1;
    private long _dirtyVersion;

    internal StyleCollectionExternalBK(StyleCollection collection)
    {
        _collection = collection ?? throw new System.ArgumentNullException(nameof(collection));
    }

    #region Values

    /// <inheritdoc/>
    public override IStyleSet? GetStyleSet(string name) => _styles.GetValueSafe(name);

    /// <inheritdoc/>
    public override IStyleSet? GetStyleSet(string name, string? pseudo) =>
        _styles.GetValueSafe(name);

    /// <inheritdoc/>
    public override T? GetStyle<T>(string name) where T : class => _styles.GetValueSafe(name)?.GetOrigin<T>();

    /// <inheritdoc/>
    public override void SetStyle<T>(string name, T value) where T : class
    {
        bool valueSet = _styles.GetOrAdd(name, n => new StyleSet(n)).SetOrigin(value);
        if (valueSet)
        {
            MarkDirty();
        }
    }

    /// <inheritdoc/>
    public override T GetOrCreateStyle<T>(string name, out bool created)
    {
        var value = _styles.GetOrAdd(name, n => new StyleSet(n)).GetOrCreateOrigin<T>(out created);

        // Calling this method is usually for writing values, so MarkDirty by default
        MarkDirty();

        return value;
    }

    /// <inheritdoc/>
    public override bool RemoveStyle<T>(string name) where T : class
    {
        var valueSet = _styles.GetValueSafe(name);
        if (valueSet is null)
        {
            return false;
        }

        if (valueSet.RemoveOrigin<T>())
        {
            if (valueSet.IsEmpty)
            {
                _styles.Remove(name);
            }

            MarkDirty();
            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public override T? GetPseudo<T>(string name, string pseudo) where T : class => _styles.GetValueSafe(name)?.GetPseudo<T>(pseudo);

    /// <inheritdoc/>
    public override void SetPseudo<T>(string name, string pseudo, T value) where T : class
    {
        bool valueSet = _styles.GetOrAdd(name, n => new StyleSet(n)).SetPseudo(pseudo, value);
        if (valueSet)
        {
            MarkDirty();
        }
    }

    /// <inheritdoc/>
    public override T GetOrCreatePseudo<T>(string name, string pseudo, out bool created)
    {
        var value = _styles.GetOrAdd(name, n => new StyleSet(n)).GetOrCreatePseudo<T>(pseudo, out created);

        // Calling this method is usually for writing values, so MarkDirty by default
        MarkDirty();

        return value;
    }

    /// <inheritdoc/>
    public override bool RemovePseudo<T>(string name, string pseudo) where T : class
    {
        var valueSet = _styles.GetValueSafe(name);
        if (valueSet is null)
        {
            return false;
        }

        if (valueSet.RemovePseudo<T>(pseudo))
        {
            if (valueSet.IsEmpty)
            {
                _styles.Remove(name);
            }

            MarkDirty();
            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public override T? GetStyle<T>(string name, string? pseudo) where T : class => _styles.GetValueSafe(name)?.GetStyle<T>(pseudo);

    /// <inheritdoc/>
    public override void SetStyle<T>(string name, string? pseudo, T value) where T : class
    {
        bool valueSet = _styles.GetOrAdd(name, n => new StyleSet(n)).SetStyle(pseudo, value);

        if (valueSet)
        {
            MarkDirty();
        }
    }

    /// <inheritdoc/>
    public override T GetOrCreateStyle<T>(string name, string? pseudo, out bool created)
    {
        var value = _styles.GetOrAdd(name, n => new StyleSet(n)).GetOrCreateStyle<T>(pseudo, out created);

        // Calling this method is usually for writing values, so MarkDirty by default
        MarkDirty();

        return value;
    }

    /// <inheritdoc/>
    public override bool RemoveStyle<T>(string name, string? pseudo) where T : class
    {
        var valueSet = _styles.GetValueSafe(name);
        if (valueSet is null)
        {
            return false;
        }

        bool removed = valueSet.RemoveStyle<T>(pseudo);

        if (removed)
        {
            if (valueSet.IsEmpty)
            {
                _styles.Remove(name);
            }

            MarkDirty();
            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public override void SetStyleSet(IStyleSet styleSet)
    {
        if (styleSet is null)
        {
            throw new ArgumentNullException(nameof(styleSet));
        }

        var vStyleSet = styleSet as StyleSet ?? throw new ArgumentException(nameof(styleSet));

        _styles[vStyleSet.Name] = vStyleSet;
    }

    /// <inheritdoc/>
    public override void Clear()
    {
        _styles.Clear();
        MarkDirty();
    }

    #endregion

    #region Apply values

    /// <inheritdoc/>
    public override void ApplyStyleSet(string id, string? typeName, string[]? classes, ref IStyleSet? styleSet)
    {
        if (_styles.Count > 0)
        {
            // * represents all wildcards
            ApplyStyleSet("*", ref styleSet);

            if (!string.IsNullOrWhiteSpace(typeName))
            {
                // Default is type name, no decoration needed
                ApplyStyleSet(typeName!, ref styleSet);
            }

            if (classes is not null)
            {
                foreach (var cls in classes.Where(v => !string.IsNullOrWhiteSpace(v)))
                {
                    // class needs to be decorated with .
                    ApplyStyleSet("." + cls, ref styleSet);
                }
            }

            if (classes is not null && !string.IsNullOrWhiteSpace(typeName))
            {
                foreach (var cls in classes.Where(v => !string.IsNullOrWhiteSpace(v)))
                {
                    // Type.class style decoration
                    ApplyStyleSet($"{typeName}.{cls}", ref styleSet);
                }
            }

            if (!string.IsNullOrWhiteSpace(id))
            {
                // id is decorated with #
                ApplyStyleSet("#" + id, ref styleSet);

                if (classes != null)
                {
                    foreach (var cls in classes.Where(v => !string.IsNullOrWhiteSpace(v)))
                    {
                        ApplyStyleSet($"#{id}.{cls}", ref styleSet);
                    }
                }
            }
        }

        if (styleSet?.IsEmpty == true)
        {
            styleSet = null;
        }
    }

    /// <inheritdoc/>
    public override void ApplyStyles(string id, string? typeName, string[]? classes, ref IValueCollection? values, string? pseudo = null)
    {
        ValueCollection? v = values as ValueCollection;
        if (v is null)
        {
            values = v = new ValueCollection();
        }

        if (_styles.Count > 0)
        {
            ApplyStyles("*", ref v, pseudo);

            if (!string.IsNullOrWhiteSpace(typeName))
            {
                ApplyStyles(typeName!, ref v, pseudo);
            }

            if (classes is not null)
            {
                foreach (var cls in classes.Where(v => !string.IsNullOrWhiteSpace(v)))
                {
                    ApplyStyles("." + cls, ref v, pseudo);
                }
            }

            if (classes is not null && !string.IsNullOrWhiteSpace(typeName))
            {
                foreach (var cls in classes.Where(v => !string.IsNullOrWhiteSpace(v)))
                {
                    ApplyStyles($"{typeName}.{cls}", ref v, pseudo);
                }
            }

            if (!string.IsNullOrWhiteSpace(id))
            {
                ApplyStyles("#" + id, ref v, pseudo);

                if (classes != null)
                {
                    foreach (var cls in classes.Where(v => !string.IsNullOrWhiteSpace(v)))
                    {
                        ApplyStyles($"#{id}.{cls}", ref v, pseudo);
                    }
                }
            }
        }

        if ((v?.Count ?? 0) == 0)
        {
            v = null;
            values = null;
        }
    }

    private bool ApplyStyleSet(string name, ref IStyleSet? target)
    {
        var styleSet = _styles.GetValueSafe(name);

        if (styleSet is not null)
        {
            styleSet.CopyTo(ref target);
            return true;
        }

        return false;
    }

    private bool ApplyStyles(string name, ref ValueCollection? values, string? pseudo = null)
    {
        var styleSet = _styles.GetValueSafe(name);

        if (styleSet != null)
        {
            if (pseudo is not null)
            {
                (styleSet.GetPseudoCollection(pseudo) as ValueCollection)?.CopyTo(ref values);
            }
            else
            {
                (styleSet.OriginCollection as ValueCollection)?.CopyTo(ref values);
            }

            return true;
        }

        return false;
    }

    #endregion

    #region Dirty

    /// <inheritdoc/>
    internal override long Version => _version;

    /// <inheritdoc/>
    public override bool IsDirty => _dirtyVersion != _version;

    /// <inheritdoc/>
    public override void MarkDirty()
    {
        if (_dirtyVersion == _version)
        {
            _version++;
        }
    }

    /// <inheritdoc/>
    internal override void ClearDirty()
    {
        _dirtyVersion = _version;
    }

    #endregion

    /// <inheritdoc/>
    public override void SetTransition(string name, string? pseudo, string? targetState, ITransitionFactory transition)
    {
        bool valueSet = _styles.GetOrAdd(name, n => new StyleSet(n)).SetTransition(pseudo, targetState, transition);
        if (valueSet)
        {
            MarkDirty();
        }
    }

    /// <inheritdoc/>
    public override void RemoveTransition(string name, string? pseudo, string? targetState)
    {
        bool removed = _styles.GetValueSafe(name)?.RemoveTransition(pseudo, targetState) == true;
        if (removed)
        {
            MarkDirty();
        }
    }

    /// <inheritdoc/>
    public override IEnumerable<IStyleSet> StyleSets => _styles.Values;
}