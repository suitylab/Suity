using System;

namespace Suity.Views.Drawing;

/// <summary>
/// Represents an ordered pair of integer x- and y-coordinates that defines a point in a two-dimensional plane.
/// </summary>
public readonly struct Point : IEquatable<Point>
{
    /// <summary>
    /// Gets a value indicating whether this <see cref="Point"/> is empty.
    /// </summary>
    public bool IsEmpty => X == 0 && Y == 0;

    /// <summary>
    /// Gets the x-coordinate of this <see cref="Point"/>.
    /// </summary>
    public int X { get; }

    /// <summary>
    /// Gets the y-coordinate of this <see cref="Point"/>.
    /// </summary>
    public int Y { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Point"/> class with the specified coordinates.
    /// </summary>
    /// <param name="x">The horizontal position of the point.</param>
    /// <param name="y">The vertical position of the point.</param>
    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Point"/> class from a <see cref="Size"/>.
    /// </summary>
    /// <param name="sz">The <see cref="Size"/> specifying the coordinates.</param>
    public Point(Size sz)
    {
        X = sz.Width;
        Y = sz.Height;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Point"/> class from a <see cref="PointF"/> by truncating the values.
    /// </summary>
    /// <param name="point">The <see cref="PointF"/> to convert.</param>
    public Point(PointF point)
    {
        X = (int)point.X;
        Y = (int)point.Y;
    }

    /// <summary>
    /// Translates a <see cref="Point"/> by a specified <see cref="Size"/>.
    /// </summary>
    /// <param name="pt">The <see cref="Point"/> to translate.</param>
    /// <param name="sz">The <see cref="Size"/> specifying the translation amounts.</param>
    /// <returns>The translated <see cref="Point"/>.</returns>
    public static Point Add(Point pt, Size sz)
    {
        return new Point(pt.X + sz.Width, pt.Y + sz.Height);
    }

    /// <summary>
    /// Translates a <see cref="Point"/> by the negative of a specified <see cref="Size"/>.
    /// </summary>
    /// <param name="pt">The <see cref="Point"/> to translate.</param>
    /// <param name="sz">The <see cref="Size"/> specifying the translation amounts.</param>
    /// <returns>The translated <see cref="Point"/>.</returns>
    public static Point Subtract(Point pt, Size sz)
    {
        return new Point(pt.X - sz.Width, pt.Y - sz.Height);
    }

    /// <inheritdoc/>
    public bool Equals(Point other)
    {
        return X == other.X && Y == other.Y;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return obj is Point other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            return (X * 397) ^ Y;
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{{X={X},Y={Y}}}";
    }

    /// <summary>
    /// Translates this <see cref="Point"/> by the specified <see cref="Size"/>.
    /// </summary>
    /// <param name="sz">The <see cref="Size"/> specifying the translation amounts.</param>
    /// <returns>The translated <see cref="Point"/>.</returns>
    public Point Offset(Size sz)
    {
        return new Point(X + sz.Width, Y + sz.Height);
    }

    /// <summary>
    /// Translates this <see cref="Point"/> by the specified amounts.
    /// </summary>
    /// <param name="dx">The amount to translate on the x-axis.</param>
    /// <param name="dy">The amount to translate on the y-axis.</param>
    /// <returns>The translated <see cref="Point"/>.</returns>
    public Point Offset(int dx, int dy)
    {
        return new Point(X + dx, Y + dy);
    }

    /// <summary>
    /// Specifies a <see cref="Point"/> structure that has <see cref="X"/> and <see cref="Y"/> values set to zero.
    /// </summary>
    public static Point Empty => default;

    /// <summary>
    /// Tests whether two <see cref="Point"/> structures are equal.
    /// </summary>
    public static bool operator ==(Point left, Point right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Tests whether two <see cref="Point"/> structures are different.
    /// </summary>
    public static bool operator !=(Point left, Point right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Translates a <see cref="Point"/> by a <see cref="Size"/>.
    /// </summary>
    public static Point operator +(Point pt, Size sz)
    {
        return new Point(pt.X + sz.Width, pt.Y + sz.Height);
    }

    /// <summary>
    /// Translates a <see cref="Point"/> by the negative of a <see cref="Size"/>.
    /// </summary>
    public static Point operator -(Point pt, Size sz)
    {
        return new Point(pt.X - sz.Width, pt.Y - sz.Height);
    }
}
