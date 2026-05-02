using System;

namespace Suity.Views.Drawing;

/// <summary>
/// Stores an ordered pair of integers, which specify a height and width.
/// </summary>
public readonly struct Size : IEquatable<Size>
{
    /// <summary>
    /// Gets a value indicating whether this <see cref="Size"/> is empty.
    /// </summary>
    public bool IsEmpty => Width == 0 && Height == 0;

    /// <summary>
    /// Gets the horizontal component of this <see cref="Size"/>.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the vertical component of this <see cref="Size"/>.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Size"/> class from the specified <see cref="Point"/>.
    /// </summary>
    /// <param name="pt">The <see cref="Point"/> from which to initialize this <see cref="Size"/>.</param>
    public Size(Point pt)
    {
        Width = pt.X;
        Height = pt.Y;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Size"/> class from the specified dimensions.
    /// </summary>
    /// <param name="width">The width component of the new <see cref="Size"/>.</param>
    /// <param name="height">The height component of the new <see cref="Size"/>.</param>
    public Size(int width, int height)
    {
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Size"/> class from a <see cref="SizeF"/> by truncating the values.
    /// </summary>
    /// <param name="size">The <see cref="SizeF"/> to convert.</param>
    public Size(SizeF size)
    {
        Width = (int)size.Width;
        Height = (int)size.Height;
    }

    /// <summary>
    /// Adds the width and height of one <see cref="Size"/> structure to the width and height of another <see cref="Size"/> structure.
    /// </summary>
    /// <param name="sz1">The first <see cref="Size"/> to add.</param>
    /// <param name="sz2">The second <see cref="Size"/> to add.</param>
    /// <returns>A <see cref="Size"/> structure that is the result of the addition.</returns>
    public static Size Add(Size sz1, Size sz2)
    {
        return new Size(sz1.Width + sz2.Width, sz1.Height + sz2.Height);
    }

    /// <summary>
    /// Subtracts the width and height of one <see cref="Size"/> structure from the width and height of another <see cref="Size"/> structure.
    /// </summary>
    /// <param name="sz1">The first <see cref="Size"/> structure.</param>
    /// <param name="sz2">The second <see cref="Size"/> structure.</param>
    /// <returns>A <see cref="Size"/> structure that is the result of the subtraction.</returns>
    public static Size Subtract(Size sz1, Size sz2)
    {
        return new Size(sz1.Width - sz2.Width, sz1.Height - sz2.Height);
    }

    /// <inheritdoc/>
    public bool Equals(Size other)
    {
        return Width == other.Width && Height == other.Height;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return obj is Size other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            return (Width * 397) ^ Height;
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{{Width={Width}, Height={Height}}}";
    }

    /// <summary>
    /// Specifies a <see cref="Size"/> structure that has <see cref="Width"/> and <see cref="Height"/> values set to zero.
    /// </summary>
    public static Size Empty => default;

    /// <summary>
    /// Tests whether two <see cref="Size"/> structures are equal.
    /// </summary>
    public static bool operator ==(Size left, Size right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Tests whether two <see cref="Size"/> structures are different.
    /// </summary>
    public static bool operator !=(Size left, Size right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Adds the width and height of one <see cref="Size"/> structure to the width and height of another <see cref="Size"/> structure.
    /// </summary>
    public static Size operator +(Size sz1, Size sz2)
    {
        return new Size(sz1.Width + sz2.Width, sz1.Height + sz2.Height);
    }

    /// <summary>
    /// Subtracts the width and height of one <see cref="Size"/> structure from the width and height of another <see cref="Size"/> structure.
    /// </summary>
    public static Size operator -(Size sz1, Size sz2)
    {
        return new Size(sz1.Width - sz2.Width, sz1.Height - sz2.Height);
    }
}
