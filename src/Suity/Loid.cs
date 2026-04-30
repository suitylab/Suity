using System;

namespace Suity;

/// <summary>
/// Represents a lightweight unique identifier.
/// </summary>
public readonly struct Loid : IFormattable, IComparable, IComparable<Loid>, IEquatable<Loid>
{
    /// <summary>
    /// Gets an empty Loid instance.
    /// </summary>
    public static Loid Empty { get; } = new(string.Empty);

    /// <summary>
    /// Creates a new Loid with a generated identifier of specified length.
    /// </summary>
    /// <param name="len">The length of the identifier to generate.</param>
    /// <returns>A new Loid instance.</returns>
    public static Loid Create(int len)
    {
        string id = Generate(len);

        return new Loid(id);
    }

    /// <summary>
    /// Creates a new Loid with a specified prefix and generated identifier.
    /// </summary>
    /// <param name="prefix">The prefix to prepend to the identifier.</param>
    /// <param name="len">The length of the identifier to generate.</param>
    /// <returns>A new Loid instance.</returns>
    public static Loid Create(string prefix, int len)
    {
        string id = Generate(len);

        return new Loid($"{prefix}-{id}");
    }

    private static readonly Random _rnd = new();

    private static readonly char[] Chars = [
        'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
        '1', '2', '3', '4', '5', '6', '7', '8', '9', '0'
    ];

    /// <summary>
    /// Generates a random alphanumeric string of specified length.
    /// </summary>
    /// <param name="len">The length of the string to generate.</param>
    /// <returns>A random string.</returns>
    public static string Generate(int len)
    {
        char[] chars = new char[len];

        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] = Chars[_rnd.Next(0, Chars.Length - 1)];
        }

        return new string(chars);
    }

    private readonly string _id;

    /// <summary>
    /// Gets the identifier value.
    /// </summary>
    public string Id => _id;

    /// <summary>
    /// Initializes a new instance of the Loid struct.
    /// </summary>
    /// <param name="id">The identifier string.</param>
    public Loid(string id)
    {
        _id = id ?? string.Empty;
    }

    /// <inheritdoc />
    public int CompareTo(object obj)
    {
        if (obj == null)
        {
            return -1;
        }
        else
        {
            return ToString().CompareTo(obj.ToString());
        }
    }

    /// <inheritdoc />
    public int CompareTo(Loid other)
    {
        return _id.CompareTo(other._id);
    }

    /// <inheritdoc />
    public bool Equals(Loid other)
    {
        return _id == other._id;
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if (obj is Loid loid)
        {
            return _id == loid._id;
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc />
    public string ToString(string format, IFormatProvider formatProvider)
    {
        return ToString();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"({_id})";
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return _id.GetHashCode();
    }

    /// <summary>
    /// Determines whether two Loid instances are equal.
    /// </summary>
    /// <param name="v1">The first Loid.</param>
    /// <param name="v2">The second Loid.</param>
    /// <returns>True if the instances are equal; otherwise, false.</returns>
    public static bool operator ==(Loid v1, Loid v2)
    {
        return v1._id == v2._id;
    }

    /// <summary>
    /// Determines whether two Loid instances are not equal.
    /// </summary>
    /// <param name="v1">The first Loid.</param>
    /// <param name="v2">The second Loid.</param>
    /// <returns>True if the instances are not equal; otherwise, false.</returns>
    public static bool operator !=(Loid v1, Loid v2)
    {
        return v1._id != v2._id;
    }
}