using System.Linq;

namespace Suity;

/// <summary>
/// Represents a two-stage field code consisting of a main name and a field name.
/// Used for hierarchical field identification.
/// </summary>
public readonly struct FieldCode
{
    /// <summary>
    /// The main name component of the field code.
    /// </summary>
    public readonly string MainName;
    /// <summary>
    /// The field name component of the field code.
    /// </summary>
    public readonly string FieldName;


    /// <summary>
    /// Initializes a new instance of the FieldCode struct with empty values.
    /// </summary>
    public FieldCode()
    {
        MainName = string.Empty;
        FieldName = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the FieldCode struct from a string.
    /// </summary>
    /// <param name="str">The string to parse, expected to be in the format "MainName.FieldName".</param>
    public FieldCode(string str)
    {
        str ??= string.Empty;

        int index = str.LastIndexOf('.');

        if (index >= 0)
        {
            MainName = str.Substring(0, index);
            FieldName = str.Substring(index + 1, str.Length - index - 1);
        }
        else
        {
            MainName = str;
            FieldName = string.Empty;
        }
    }

    /// <summary>
    /// Initializes a new instance of the FieldCode struct with specified main and field names.
    /// </summary>
    /// <param name="mainName">The main name component.</param>
    /// <param name="fieldName">The field name component.</param>
    public FieldCode(string mainName, string fieldName)
    {
        MainName = mainName ?? string.Empty;
        FieldName = fieldName ?? string.Empty;
    }

    /// <summary>
    /// Returns a string representation of the field code.
    /// </summary>
    /// <returns>The combined field code string.</returns>
    public override string ToString()
    {
        return Combine(MainName, FieldName);
    }

    /// <summary>
    /// Gets a value indicating whether the field code has a field component.
    /// </summary>
    public bool HasField => !string.IsNullOrWhiteSpace(FieldName);

    /// <summary>
    /// Gets the terminal part of the field code (field name if present, otherwise main name).
    /// </summary>
    public string Terminal => !string.IsNullOrWhiteSpace(FieldName) ? FieldName : MainName;

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
        if (obj.GetType() != typeof(FieldCode)) return false;

        FieldCode other = (FieldCode)obj;

        return MainName == other.MainName &&
            FieldName == other.FieldName;
    }

    /// <summary>
    /// Determines whether two FieldCode instances are equal.
    /// </summary>
    /// <param name="v1">The first FieldCode.</param>
    /// <param name="v2">The second FieldCode.</param>
    /// <returns>True if the instances are equal; otherwise, false.</returns>
    public static bool operator ==(FieldCode v1, FieldCode v2)
    {
        if (Equals(v1, null)) return Equals(v2, null); else return v1.Equals(v2);
    }

    /// <summary>
    /// Determines whether two FieldCode instances are not equal.
    /// </summary>
    /// <param name="v1">The first FieldCode.</param>
    /// <param name="v2">The second FieldCode.</param>
    /// <returns>True if the instances are not equal; otherwise, false.</returns>
    public static bool operator !=(FieldCode v1, FieldCode v2)
    {
        if (Equals(v1, null)) return !Equals(v2, null); else return !v1.Equals(v2);
    }

    #endregion

    /// <summary>
    /// Combines a main name and field name into a single field code string.
    /// </summary>
    /// <param name="mainName">The main name.</param>
    /// <param name="fieldName">The field name.</param>
    /// <returns>The combined field code string.</returns>
    public static string Combine(string mainName, string fieldName)
    {
        mainName ??= string.Empty;

        if (string.IsNullOrEmpty(fieldName))
        {
            return mainName;
        }
        else
        {
            return mainName + "." + fieldName;
        }
    }

    /// <summary>
    /// Determines whether a string contains a field code (has a dot separator).
    /// </summary>
    /// <param name="str">The string to check.</param>
    /// <returns>True if the string contains a field code; otherwise, false.</returns>
    public static bool HasFieldCode(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return false;
        }

        return str.Contains('.');
    }

    /// <summary>
    /// Parses a full field code string that may include element keys.
    /// </summary>
    /// <param name="str">The string to parse.</param>
    /// <returns>A FieldCode representing the parsed value.</returns>
    public static FieldCode ParseFullFieldCode(string str)
    {
        var keyCode = new KeyCode(str);
        if (!string.IsNullOrWhiteSpace(keyCode.ElementKey))
        {
            var fieldCode = new FieldCode(keyCode.ElementKey);
            string fullKeyCode = $"{keyCode.MainKey}|{fieldCode.MainName}";
            string fieldName = fieldCode.FieldName;

            return new FieldCode(fullKeyCode, fieldName);
        }
        else
        {
            return new FieldCode(keyCode.MainKey);
        }
    }

    /// <summary>
    /// Parses a full field name from a string.
    /// </summary>
    /// <param name="str">The string to parse.</param>
    /// <returns>The field name component.</returns>
    public static string ParseFullFieldName(string str)
    {
        var keyCode = new KeyCode(str);
        var fieldCode = new FieldCode(keyCode.Terminal);

        return fieldCode.FieldName;
    }
}