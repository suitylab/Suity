using System;
using System.Linq;

namespace Suity;

/// <summary>
/// Represents a two-stage key code consisting of a main key and an optional element key.
/// </summary>
public readonly struct KeyCode
{
    private static readonly char[] Splitter = ['|'];

    /// <summary>
    /// The main key component.
    /// </summary>
    public readonly string MainKey;
    /// <summary>
    /// The element key component.
    /// </summary>
    public readonly string ElementKey;

    /// <summary>
    /// Initializes a new instance of the KeyCode struct with empty values.
    /// </summary>
    public KeyCode()
    {
        MainKey = string.Empty;
        ElementKey = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the KeyCode struct from a string.
    /// </summary>
    /// <param name="str">The string to parse, expected to be in the format "MainKey|ElementKey".</param>
    public KeyCode(string str)
    {
        str ??= string.Empty;

        string[] split = str.Split(Splitter, 2);
        if (split.Length == 2)
        {
            MainKey = split[0] ?? string.Empty;
            ElementKey = split[1] ?? string.Empty;
        }
        else if (split.Length == 1)
        {
            MainKey = split[0] ?? string.Empty;
            ElementKey = string.Empty;
        }
        else
        {
            MainKey = string.Empty;
            ElementKey = string.Empty;
        }
    }

    /// <summary>
    /// Initializes a new instance of the KeyCode struct with specified main and element keys.
    /// </summary>
    /// <param name="mainKey">The main key component.</param>
    /// <param name="elementKey">The element key component.</param>
    public KeyCode(string mainKey, string elementKey)
    {
        mainKey ??= string.Empty;

        if (mainKey.Contains('|'))
        {
            throw new ArgumentException("mainKey");
        }

        MainKey = mainKey ?? string.Empty;
        ElementKey = elementKey ?? string.Empty;
    }

    /// <summary>
    /// Gets the terminal part of the key code (element key if present, otherwise main key).
    /// </summary>
    public string Terminal => !string.IsNullOrWhiteSpace(ElementKey) ? ElementKey : MainKey;

    /// <summary>
    /// Gets a value indicating whether the key code is empty.
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(MainKey) && string.IsNullOrEmpty(ElementKey);

    /// <inheritdoc />
    public override string ToString()
    {
        return Combine(MainKey, ElementKey);
    }

    #region Equality

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        if (obj.GetType() != typeof(KeyCode)) return false;

        KeyCode other = (KeyCode)obj;

        return MainKey == other.MainKey &&
            ElementKey == other.ElementKey;
    }

    /// <summary>
    /// Determines whether two KeyCode instances are equal.
    /// </summary>
    /// <param name="v1">The first KeyCode.</param>
    /// <param name="v2">The second KeyCode.</param>
    /// <returns>True if the instances are equal; otherwise, false.</returns>
    public static bool operator ==(KeyCode v1, KeyCode v2)
    {
        if (Equals(v1, null)) return Equals(v2, null); else return v1.Equals(v2);
    }

    /// <summary>
    /// Determines whether two KeyCode instances are not equal.
    /// </summary>
    /// <param name="v1">The first KeyCode.</param>
    /// <param name="v2">The second KeyCode.</param>
    /// <returns>True if the instances are not equal; otherwise, false.</returns>
    public static bool operator !=(KeyCode v1, KeyCode v2)
    {
        if (Equals(v1, null)) return !Equals(v2, null); else return !v1.Equals(v2);
    }

    #endregion

    /// <summary>
    /// Combines a main key and element key into a single key code string.
    /// </summary>
    /// <param name="mainKey">The main key.</param>
    /// <param name="elementKey">The element key.</param>
    /// <returns>The combined key code string.</returns>
    public static string Combine(string mainKey, string elementKey)
    {
        mainKey ??= string.Empty;

        if (string.IsNullOrEmpty(elementKey))
        {
            return mainKey;
        }
        else
        {
            return mainKey + "|" + elementKey;
        }
    }

    /// <summary>
    /// Determines whether a string contains an element key (has a pipe separator).
    /// </summary>
    /// <param name="keyString">The string to check.</param>
    /// <returns>True if the string contains an element key; otherwise, false.</returns>
    public static bool HasElement(string keyString)
    {
        if (string.IsNullOrEmpty(keyString))
        {
            return false;
        }

        return keyString.Contains('|');
    }
}