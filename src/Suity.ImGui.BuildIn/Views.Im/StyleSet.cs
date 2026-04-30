using Suity.Collections;
using System;
using System.Collections.Generic;

namespace Suity.Views.Im;

/// <summary>
/// Represents a named set of styles including origin values, pseudo-class values, and transitions.
/// </summary>
public class StyleSet : IStyleSet
{
    private readonly string _name;
    private readonly ValueCollection _originStyles = new();
    private Dictionary<string, ValueCollection>? _pseudoStyles;

    /// <summary>
    /// Gets the name of this style set.
    /// </summary>
    public string Name => _name;

    /// <summary>
    /// Initializes a new style set with the specified name.
    /// </summary>
    /// <param name="name">The style set name.</param>
    // ReSharper disable once EmptyConstructor
    public StyleSet(string name)
    {
        _name = name;
    }

    #region Origin

    /// <inheritdoc/>
    public IValueCollection OriginCollection => _originStyles;

    /// <inheritdoc/>
    public T? GetOrigin<T>() where T : class => _originStyles.GetValue<T>();

    /// <inheritdoc/>
    public bool SetOrigin<T>(T value) where T : class
    {
        _originStyles.SetValue(value, out bool valueSet);
        return valueSet;
    }

    /// <inheritdoc/>
    public T GetOrCreateOrigin<T>(out bool created) where T : class, new()
    {
        return _originStyles.GetOrCreateValue<T>(out created);
    }

    /// <inheritdoc/>
    public bool RemoveOrigin<T>() where T : class => _originStyles.RemoveValue<T>();

    #endregion

    #region Pseudo

    /// <inheritdoc/>
    public IValueCollection? GetPseudoCollection(string pseudo)
    {
        return _pseudoStyles?.GetValueSafe(pseudo);
    }

    /// <inheritdoc/>
    public T? GetPseudo<T>(string pseudo) where T : class => _pseudoStyles?.GetValueSafe(pseudo)?.GetValue<T>();

    private ValueCollection EnsurePseudoCollection(string pseudo)
    {
        _pseudoStyles ??= [];

        return _pseudoStyles.GetOrAdd(pseudo, _ => new ValueCollection(_originStyles));
    }

    /// <inheritdoc/>
    public bool SetPseudo<T>(string pseudo, T value) where T : class
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        if (string.IsNullOrWhiteSpace(pseudo))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(pseudo));

        _pseudoStyles ??= [];
        EnsurePseudoCollection(pseudo).SetValue(value, out bool valueSet);

        return valueSet;
    }

    /// <inheritdoc/>
    public T GetOrCreatePseudo<T>(string pseudo, out bool created) where T : class, new()
    {
        if (string.IsNullOrWhiteSpace(pseudo))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(pseudo));

        _pseudoStyles ??= [];
        return EnsurePseudoCollection(pseudo).GetOrCreateValue<T>(out created);
    }

    /// <inheritdoc/>
    public bool RemovePseudo<T>(string pseudo) where T : class
    {
        if (_pseudoStyles == null)
        {
            return false;
        }

        var value = _pseudoStyles.GetValueSafe(pseudo);
        if (value == null)
        {
            return false;
        }

        if (value.RemoveValue<T>())
        {
            if (value.Count == 0)
            {
                _pseudoStyles.Remove(pseudo);
            }

            if (_pseudoStyles.Count == 0)
            {
                _pseudoStyles = null;
            }

            return true;
        }

        return false;
    }

    #endregion

    #region Origin and Pseudo

    /// <inheritdoc/>
    public T? GetStyle<T>(string? pseudo) where T : class
    {
        if (string.IsNullOrWhiteSpace(pseudo))
        {
            return _originStyles.GetValue<T>();
        }
        else
        {
            return _pseudoStyles?.GetValueSafe(pseudo!)?.GetValue<T>();
        }
    }

    /// <inheritdoc/>
    public T GetOrCreateStyle<T>(string? pseudo, out bool created) where T : class, new()
    {
        if (string.IsNullOrWhiteSpace(pseudo))
        {
            return GetOrCreateOrigin<T>(out created);
        }
        else
        {
            return GetOrCreatePseudo<T>(pseudo!, out created);
        }
    }

    /// <inheritdoc/>
    public IValueCollection? GetStyleCollection(string? pseudo)
    {
        if (string.IsNullOrWhiteSpace(pseudo))
        {
            return _originStyles;
        }
        else
        {
            return _pseudoStyles?.GetValueSafe(pseudo!);
        }
    }

    /// <inheritdoc/>
    public bool SetStyle<T>(string? pseudo, T value) where T : class
    {
        if (string.IsNullOrWhiteSpace(pseudo))
        {
            return SetOrigin<T>(value);
        }
        else
        {
            return SetPseudo<T>(pseudo!, value);
        }
    }

    /// <inheritdoc/>
    public bool RemoveStyle<T>(string? pseudo) where T : class
    {
        if (string.IsNullOrWhiteSpace(pseudo))
        {
            return RemoveOrigin<T>();
        }
        else
        {
            return RemovePseudo<T>(pseudo!);
        }
    }

    internal ValueCollection EnsureStyleCollection(string? pseudo)
    {
        if (string.IsNullOrWhiteSpace(pseudo))
        {
            return _originStyles;
        }
        else
        {
            _pseudoStyles ??= [];
            return _pseudoStyles.GetOrAdd(pseudo!, _ => new ValueCollection(_originStyles));
        }
    }

    #endregion

    #region Transition

    /// <inheritdoc/>
    public bool SetTransition(string? state, string? targetState, ITransitionFactory transition)
    {
        if (state == targetState)
        {
            return false;
        }

        if (string.IsNullOrEmpty(state))
        {
            _originStyles.SetTransition(targetState, transition, out bool valueSet);

            return valueSet;
        }
        else
        {
            _pseudoStyles ??= [];
            EnsurePseudoCollection(state!).SetTransition(targetState, transition, out bool valueSet);

            return valueSet;
        }
    }

    /// <inheritdoc/>
    public bool RemoveTransition(string? state, string? targetState)
    {
        if (state == targetState)
        {
            return false;
        }

        if (string.IsNullOrEmpty(state))
        {
            return _originStyles.RemoveTransition(targetState);
        }
        else
        {
            return _pseudoStyles?.GetValueSafe(state!)?.RemoveTransition(targetState) == true;
        }
    }

    /// <inheritdoc/>
    public ITransitionFactory? GetTransition(string? state, string? targetState)
    {
        if (string.IsNullOrEmpty(state))
        {
            return _originStyles.GetTransition(targetState);
        }
        else
        {
            return _pseudoStyles?.GetValueSafe(state!)?.GetTransition(targetState);
        }
    }

    #endregion

    /// <summary>
    /// Clears all origin and pseudo styles.
    /// </summary>
    public void Clear()
    {
        _originStyles.Clear();
        _pseudoStyles = null;
    }

    /// <summary>
    /// Gets whether this style set has no origin or pseudo styles.
    /// </summary>
    public bool IsEmpty => _originStyles.Count == 0 && (_pseudoStyles?.Count ?? 0) == 0;

    /// <summary>
    /// Copies all styles to another style set.
    /// </summary>
    /// <param name="other">The target style set.</param>
    public void CopyTo(ref IStyleSet? other)
    {
        if (IsEmpty)
        {
            return;
        }

        if (other is not StyleSet o)
        {
            other = o = new("???");
        }

        _originStyles.CopyTo(o._originStyles);

        if (_pseudoStyles is { })
        {
            o._pseudoStyles ??= [];

            foreach (var pair in _pseudoStyles)
            {
                var otherPseudo = o.EnsurePseudoCollection(pair.Key);
                pair.Value.CopyTo(otherPseudo);
            }
        }
    }
}