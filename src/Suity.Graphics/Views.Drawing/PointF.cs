using System;

namespace Suity.Views.Drawing;

/// <summary>
/// Represents an ordered pair of single-precision floating-point x- and y-coordinates that defines a point in a two-dimensional plane.
/// </summary>
public readonly struct PointF : IEquatable<PointF>
{
    /// <summary>
    /// Gets a value indicating whether this <see cref="PointF"/> is empty.
    /// </summary>
    public bool IsEmpty => X == 0f && Y == 0f;

    /// <summary>
    /// Gets the x-coordinate of this <see cref="PointF"/>.
    /// </summary>
    public float X { get; }

    /// <summary>
    /// Gets the y-coordinate of this <see cref="PointF"/>.
    /// </summary>
    public float Y { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PointF"/> class with the specified coordinates.
    /// </summary>
    /// <param name="x">The horizontal position of the point.</param>
    /// <param name="y">The vertical position of the point.</param>
    public PointF(float x, float y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PointF"/> class from a <see cref="SizeF"/>.
    /// </summary>
    /// <param name="sz">The <see cref="SizeF"/> specifying the coordinates.</param>
    public PointF(SizeF sz)
    {
        X = sz.Width;
        Y = sz.Height;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PointF"/> class from a <see cref="Point"/>.
    /// </summary>
    /// <param name="point">The <see cref="Point"/> to convert.</param>
    public PointF(Point point)
    {
        X = point.X;
        Y = point.Y;
    }

    /// <summary>
    /// Translates a <see cref="PointF"/> by a specified <see cref="SizeF"/>.
    /// </summary>
    /// <param name="pt">The <see cref="PointF"/> to translate.</param>
    /// <param name="sz">The <see cref="SizeF"/> specifying the translation amounts.</param>
    /// <returns>The translated <see cref="PointF"/>.</returns>
    public static PointF Add(PointF pt, SizeF sz)
    {
        return new PointF(pt.X + sz.Width, pt.Y + sz.Height);
    }

    /// <summary>
    /// Translates a <see cref="PointF"/> by the negative of a specified <see cref="SizeF"/>.
    /// </summary>
    /// <param name="pt">The <see cref="PointF"/> to translate.</param>
    /// <param name="sz">The <see cref="SizeF"/> specifying the translation amounts.</param>
    /// <returns>The translated <see cref="PointF"/>.</returns>
    public static PointF Subtract(PointF pt, SizeF sz)
    {
        return new PointF(pt.X - sz.Width, pt.Y - sz.Height);
    }

    /// <inheritdoc/>
    public bool Equals(PointF other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return obj is PointF other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            return (X.GetHashCode() * 397) ^ Y.GetHashCode();
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{{X={X},Y={Y}}}";
    }

    /// <summary>
    /// Translates this <see cref="PointF"/> by the specified <see cref="SizeF"/>.
    /// </summary>
    /// <param name="sz">The <see cref="SizeF"/> specifying the translation amounts.</param>
    /// <returns>The translated <see cref="PointF"/>.</returns>
    public PointF Offset(SizeF sz)
    {
        return new PointF(X + sz.Width, Y + sz.Height);
    }

    /// <summary>
    /// Translates this <see cref="PointF"/> by the specified amounts.
    /// </summary>
    /// <param name="dx">The amount to translate on the x-axis.</param>
    /// <param name="dy">The amount to translate on the y-axis.</param>
    /// <returns>The translated <see cref="PointF"/>.</returns>
    public PointF Offset(float dx, float dy)
    {
        return new PointF(X + dx, Y + dy);
    }

    /// <summary>
    /// Specifies a <see cref="PointF"/> structure that has <see cref="X"/> and <see cref="Y"/> values set to zero.
    /// </summary>
    public static PointF Empty => default;

    /// <summary>
    /// Tests whether two <see cref="PointF"/> structures are equal.
    /// </summary>
    public static bool operator ==(PointF left, PointF right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Tests whether two <see cref="PointF"/> structures are different.
    /// </summary>
    public static bool operator !=(PointF left, PointF right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Translates a <see cref="PointF"/> by a <see cref="SizeF"/>.
    /// </summary>
    public static PointF operator +(PointF pt, SizeF sz)
    {
        return new PointF(pt.X + sz.Width, pt.Y + sz.Height);
    }

    /// <summary>
    /// Translates a <see cref="PointF"/> by the negative of a <see cref="SizeF"/>.
    /// </summary>
    public static PointF operator -(PointF pt, SizeF sz)
    {
        return new PointF(pt.X - sz.Width, pt.Y - sz.Height);
    }
}
