using System;

namespace Suity.Views.Drawing;

/// <summary>
/// Stores a set of four single-precision floating-point numbers that represent the location and size of a rectangle.
/// </summary>
public readonly struct RectangleF : IEquatable<RectangleF>
{
    /// <summary>
    /// Gets the x-coordinate of the upper-left corner of this <see cref="RectangleF"/> structure.
    /// </summary>
    public float X { get; }

    /// <summary>
    /// Gets the y-coordinate of the upper-left corner of this <see cref="RectangleF"/> structure.
    /// </summary>
    public float Y { get; }

    /// <summary>
    /// Gets the width of this <see cref="RectangleF"/> structure.
    /// </summary>
    public float Width { get; }

    /// <summary>
    /// Gets the height of this <see cref="RectangleF"/> structure.
    /// </summary>
    public float Height { get; }

    /// <summary>
    /// Gets the x-coordinate of the left edge of this <see cref="RectangleF"/> structure.
    /// </summary>
    public float Left => X;

    /// <summary>
    /// Gets the y-coordinate of the top edge of this <see cref="RectangleF"/> structure.
    /// </summary>
    public float Top => Y;

    /// <summary>
    /// Gets the x-coordinate of the right edge of this <see cref="RectangleF"/> structure.
    /// </summary>
    public float Right => X + Width;

    /// <summary>
    /// Gets the y-coordinate of the bottom edge of this <see cref="RectangleF"/> structure.
    /// </summary>
    public float Bottom => Y + Height;

    /// <summary>
    /// Gets a value indicating whether this <see cref="RectangleF"/> is empty.
    /// </summary>
    public bool IsEmpty => Width == 0f && Height == 0f;

    /// <summary>
    /// Gets the <see cref="SizeF"/> of this <see cref="RectangleF"/>.
    /// </summary>
    public SizeF Size => new SizeF(Width, Height);

    /// <summary>
    /// Gets the <see cref="PointF"/> that represents the upper-left corner of this <see cref="RectangleF"/>.
    /// </summary>
    public PointF Location => new PointF(X, Y);

    /// <summary>
    /// Initializes a new instance of the <see cref="RectangleF"/> class with the specified location and size.
    /// </summary>
    /// <param name="x">The x-coordinate of the upper-left corner of the rectangle.</param>
    /// <param name="y">The y-coordinate of the upper-left corner of the rectangle.</param>
    /// <param name="width">The width of the rectangle.</param>
    /// <param name="height">The height of the rectangle.</param>
    public RectangleF(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RectangleF"/> class with the specified location and size.
    /// </summary>
    /// <param name="location">A <see cref="PointF"/> that represents the upper-left corner of the rectangle.</param>
    /// <param name="size">A <see cref="SizeF"/> that represents the width and height of the rectangle.</param>
    public RectangleF(PointF location, SizeF size)
    {
        X = location.X;
        Y = location.Y;
        Width = size.Width;
        Height = size.Height;
    }

    /// <summary>
    /// Creates a <see cref="RectangleF"/> structure from a <see cref="Rectangle"/>.
    /// </summary>
    /// <param name="rect">The <see cref="Rectangle"/> to convert.</param>
    public RectangleF(Rectangle rect)
    {
        X = rect.X;
        Y = rect.Y;
        Width = rect.Width;
        Height = rect.Height;
    }

    /// <summary>
    /// Determines if the specified point is contained within this <see cref="RectangleF"/> structure.
    /// </summary>
    /// <param name="x">The x-coordinate of the point to test.</param>
    /// <param name="y">The y-coordinate of the point to test.</param>
    /// <returns>true if the point is contained within this <see cref="RectangleF"/>; otherwise, false.</returns>
    public bool Contains(float x, float y)
    {
        return x >= Left && x < Right && y >= Top && y < Bottom;
    }

    /// <summary>
    /// Determines if the specified point is contained within this <see cref="RectangleF"/> structure.
    /// </summary>
    /// <param name="pt">The <see cref="PointF"/> to test.</param>
    /// <returns>true if the point is contained within this <see cref="RectangleF"/>; otherwise, false.</returns>
    public bool Contains(PointF pt)
    {
        return Contains(pt.X, pt.Y);
    }

    /// <summary>
    /// Determines if the rectangular region represented by <paramref name="rect"/> is entirely contained within this <see cref="RectangleF"/> structure.
    /// </summary>
    /// <param name="rect">The <see cref="RectangleF"/> to test.</param>
    /// <returns>true if the region is contained within this <see cref="RectangleF"/>; otherwise, false.</returns>
    public bool Contains(RectangleF rect)
    {
        return Left <= rect.Left && Right >= rect.Right && Top <= rect.Top && Bottom >= rect.Bottom;
    }

    /// <summary>
    /// Gets the rectangle that represents the union of two rectangles.
    /// </summary>
    /// <param name="a">A rectangle to union.</param>
    /// <param name="b">A rectangle to union.</param>
    /// <returns>A <see cref="RectangleF"/> that bounds the union of the two rectangles.</returns>
    public static RectangleF Union(RectangleF a, RectangleF b)
    {
        float x = Math.Min(a.Left, b.Left);
        float y = Math.Min(a.Top, b.Top);
        float width = Math.Max(a.Right, b.Right) - x;
        float height = Math.Max(a.Bottom, b.Bottom) - y;
        return new RectangleF(x, y, width, height);
    }

    /// <summary>
    /// Creates a rectangle that represents the intersection of two rectangles.
    /// </summary>
    /// <param name="a">A rectangle to intersect.</param>
    /// <param name="b">A rectangle to intersect.</param>
    /// <returns>A <see cref="RectangleF"/> that represents the intersection of the two rectangles.</returns>
    public static RectangleF Intersect(RectangleF a, RectangleF b)
    {
        float x = Math.Max(a.Left, b.Left);
        float y = Math.Max(a.Top, b.Top);
        float width = Math.Min(a.Right, b.Right) - x;
        float height = Math.Min(a.Bottom, b.Bottom) - y;

        if (width < 0 || height < 0)
            return Empty;

        return new RectangleF(x, y, width, height);
    }

    /// <summary>
    /// Determines whether this <see cref="RectangleF"/> intersects with the specified rectangle.
    /// </summary>
    /// <param name="rect">The rectangle to test.</param>
    /// <returns>true if the rectangles intersect; otherwise, false.</returns>
    public bool IntersectsWith(RectangleF rect)
    {
        return Left < rect.Right && Right > rect.Left && Top < rect.Bottom && Bottom > rect.Top;
    }

    /// <summary>
    /// Inflates this <see cref="RectangleF"/> by the specified amount.
    /// </summary>
    /// <param name="x">The amount to inflate horizontally.</param>
    /// <param name="y">The amount to inflate vertically.</param>
    /// <returns>The inflated rectangle.</returns>
    public RectangleF Inflate(float x, float y)
    {
        return new RectangleF(X - x, Y - y, Width + (x * 2), Height + (y * 2));
    }

    /// <summary>
    /// Inflates this <see cref="RectangleF"/> by the specified amount.
    /// </summary>
    /// <param name="size">The <see cref="SizeF"/> specifying the inflation amounts.</param>
    /// <returns>The inflated rectangle.</returns>
    public RectangleF Inflate(SizeF size)
    {
        return Inflate(size.Width, size.Height);
    }

    /// <summary>
    /// Creates and returns an inflated copy of the specified <see cref="RectangleF"/> structure.
    /// </summary>
    /// <param name="rect">The rectangle to inflate.</param>
    /// <param name="x">The amount to inflate horizontally.</param>
    /// <param name="y">The amount to inflate vertically.</param>
    /// <returns>The inflated rectangle.</returns>
    public static RectangleF Inflate(RectangleF rect, float x, float y)
    {
        return rect.Inflate(x, y);
    }

    /// <summary>
    /// Replaces this <see cref="RectangleF"/> with the intersection of itself and the specified <see cref="RectangleF"/>.
    /// </summary>
    /// <param name="rect">The rectangle to intersect with.</param>
    /// <returns>The intersected rectangle.</returns>
    public RectangleF Intersect(RectangleF rect)
    {
        return Intersect(this, rect);
    }

    /// <summary>
    /// Adjusts the location of this rectangle by the specified amount.
    /// </summary>
    /// <param name="x">The amount to offset the location horizontally.</param>
    /// <param name="y">The amount to offset the location vertically.</param>
    /// <returns>The offset rectangle.</returns>
    public RectangleF Offset(float x, float y)
    {
        return new RectangleF(X + x, Y + y, Width, Height);
    }

    /// <summary>
    /// Adjusts the location of this rectangle by the specified amount.
    /// </summary>
    /// <param name="pos">The <see cref="PointF"/> specifying the offset amounts.</param>
    /// <returns>The offset rectangle.</returns>
    public RectangleF Offset(PointF pos)
    {
        return Offset(pos.X, pos.Y);
    }

    /// <summary>
    /// Rounds the <see cref="RectangleF"/> values to the nearest integer values.
    /// </summary>
    /// <returns>A <see cref="Rectangle"/> with rounded values.</returns>
    public Rectangle Round()
    {
        return new Rectangle(
            (int)Math.Round(X),
            (int)Math.Round(Y),
            (int)Math.Round(Width),
            (int)Math.Round(Height));
    }

    /// <summary>
    /// Truncates the <see cref="RectangleF"/> values to integer values.
    /// </summary>
    /// <returns>A <see cref="Rectangle"/> with truncated values.</returns>
    public Rectangle Truncate()
    {
        return new Rectangle(
            (int)X,
            (int)Y,
            (int)Width,
            (int)Height);
    }

    /// <inheritdoc/>
    public bool Equals(RectangleF other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y) && Width.Equals(other.Width) && Height.Equals(other.Height);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return obj is RectangleF other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + X.GetHashCode();
            hash = hash * 31 + Y.GetHashCode();
            hash = hash * 31 + Width.GetHashCode();
            hash = hash * 31 + Height.GetHashCode();
            return hash;
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{{X={X},Y={Y},Width={Width},Height={Height}}}";
    }

    /// <summary>
    /// Specifies a <see cref="RectangleF"/> structure that has <see cref="X"/>, <see cref="Y"/>, <see cref="Width"/>, and <see cref="Height"/> values set to zero.
    /// </summary>
    public static RectangleF Empty => default;

    /// <summary>
    /// Tests whether two <see cref="RectangleF"/> structures are equal.
    /// </summary>
    public static bool operator ==(RectangleF left, RectangleF right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Tests whether two <see cref="RectangleF"/> structures are different.
    /// </summary>
    public static bool operator !=(RectangleF left, RectangleF right)
    {
        return !left.Equals(right);
    }
}
