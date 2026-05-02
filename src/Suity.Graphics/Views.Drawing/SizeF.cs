using System;

namespace Suity.Views.Drawing;

/// <summary>
/// Stores an ordered pair of single-precision floating-point numbers, which specify a height and width.
/// </summary>
public readonly struct SizeF : IEquatable<SizeF>
{
    /// <summary>
    /// Gets a value indicating whether this <see cref="SizeF"/> is empty.
    /// </summary>
    public bool IsEmpty => Width == 0f && Height == 0f;

    /// <summary>
    /// Gets the horizontal component of this <see cref="SizeF"/>.
    /// </summary>
    public float Width { get; }

    /// <summary>
    /// Gets the vertical component of this <see cref="SizeF"/>.
    /// </summary>
    public float Height { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SizeF"/> class from the specified <see cref="PointF"/>.
    /// </summary>
    /// <param name="pt">The <see cref="PointF"/> from which to initialize this <see cref="SizeF"/>.</param>
    public SizeF(PointF pt)
    {
        Width = pt.X;
        Height = pt.Y;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SizeF"/> class from the specified dimensions.
    /// </summary>
    /// <param name="width">The width component of the new <see cref="SizeF"/>.</param>
    /// <param name="height">The height component of the new <see cref="SizeF"/>.</param>
    public SizeF(float width, float height)
    {
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SizeF"/> class from a <see cref="Size"/>.
    /// </summary>
    /// <param name="size">The <see cref="Size"/> to convert.</param>
    public SizeF(Size size)
    {
        Width = size.Width;
        Height = size.Height;
    }

    /// <summary>
    /// Adds the width and height of one <see cref="SizeF"/> structure to the width and height of another <see cref="SizeF"/> structure.
    /// </summary>
    /// <param name="sz1">The first <see cref="SizeF"/> to add.</param>
    /// <param name="sz2">The second <see cref="SizeF"/> to add.</param>
    /// <returns>A <see cref="SizeF"/> structure that is the result of the addition.</returns>
    public static SizeF Add(SizeF sz1, SizeF sz2)
    {
        return new SizeF(sz1.Width + sz2.Width, sz1.Height + sz2.Height);
    }

    /// <summary>
    /// Subtracts the width and height of one <see cref="SizeF"/> structure from the width and height of another <see cref="SizeF"/> structure.
    /// </summary>
    /// <param name="sz1">The first <see cref="SizeF"/> structure.</param>
    /// <param name="sz2">The second <see cref="SizeF"/> structure.</param>
    /// <returns>A <see cref="SizeF"/> structure that is the result of the subtraction.</returns>
    public static SizeF Subtract(SizeF sz1, SizeF sz2)
    {
        return new SizeF(sz1.Width - sz2.Width, sz1.Height - sz2.Height);
    }

    /// <inheritdoc/>
    public bool Equals(SizeF other)
    {
        return Width.Equals(other.Width) && Height.Equals(other.Height);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return obj is SizeF other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            return (Width.GetHashCode() * 397) ^ Height.GetHashCode();
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{{Width={Width}, Height={Height}}}";
    }

    /// <summary>
    /// Specifies a <see cref="SizeF"/> structure that has <see cref="Width"/> and <see cref="Height"/> values set to zero.
    /// </summary>
    public static SizeF Empty => default;

    /// <summary>
    /// Tests whether two <see cref="SizeF"/> structures are equal.
    /// </summary>
    public static bool operator ==(SizeF left, SizeF right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Tests whether two <see cref="SizeF"/> structures are different.
    /// </summary>
    public static bool operator !=(SizeF left, SizeF right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Adds the width and height of one <see cref="SizeF"/> structure to the width and height of another <see cref="SizeF"/> structure.
    /// </summary>
    public static SizeF operator +(SizeF sz1, SizeF sz2)
    {
        return new SizeF(sz1.Width + sz2.Width, sz1.Height + sz2.Height);
    }

    /// <summary>
    /// Subtracts the width and height of one <see cref="SizeF"/> structure from the width and height of another <see cref="SizeF"/> structure.
    /// </summary>
    public static SizeF operator -(SizeF sz1, SizeF sz2)
    {
        return new SizeF(sz1.Width - sz2.Width, sz1.Height - sz2.Height);
    }
}
