using System;

namespace Suity.Views.Drawing;

/// <summary>
/// Stores a set of four integers that represent the location and size of a rectangle.
/// </summary>
public readonly struct Rectangle : IEquatable<Rectangle>
{
    /// <summary>
    /// Gets the x-coordinate of the upper-left corner of this <see cref="Rectangle"/> structure.
    /// </summary>
    public int X { get; }

    /// <summary>
    /// Gets the y-coordinate of the upper-left corner of this <see cref="Rectangle"/> structure.
    /// </summary>
    public int Y { get; }

    /// <summary>
    /// Gets the width of this <see cref="Rectangle"/> structure.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the height of this <see cref="Rectangle"/> structure.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets the x-coordinate of the left edge of this <see cref="Rectangle"/> structure.
    /// </summary>
    public int Left => X;

    /// <summary>
    /// Gets the y-coordinate of the top edge of this <see cref="Rectangle"/> structure.
    /// </summary>
    public int Top => Y;

    /// <summary>
    /// Gets the x-coordinate of the right edge of this <see cref="Rectangle"/> structure.
    /// </summary>
    public int Right => X + Width;

    /// <summary>
    /// Gets the y-coordinate of the bottom edge of this <see cref="Rectangle"/> structure.
    /// </summary>
    public int Bottom => Y + Height;

    /// <summary>
    /// Gets a value indicating whether this <see cref="Rectangle"/> is empty.
    /// </summary>
    public bool IsEmpty => Width == 0 && Height == 0;

    /// <summary>
    /// Gets the <see cref="Size"/> of this <see cref="Rectangle"/>.
    /// </summary>
    public Size Size => new Size(Width, Height);

    /// <summary>
    /// Gets the <see cref="Point"/> that represents the upper-left corner of this <see cref="Rectangle"/>.
    /// </summary>
    public Point Location => new Point(X, Y);

    /// <summary>
    /// Initializes a new instance of the <see cref="Rectangle"/> class with the specified location and size.
    /// </summary>
    /// <param name="x">The x-coordinate of the upper-left corner of the rectangle.</param>
    /// <param name="y">The y-coordinate of the upper-left corner of the rectangle.</param>
    /// <param name="width">The width of the rectangle.</param>
    /// <param name="height">The height of the rectangle.</param>
    public Rectangle(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Rectangle"/> class with the specified location and size.
    /// </summary>
    /// <param name="location">A <see cref="Point"/> that represents the upper-left corner of the rectangle.</param>
    /// <param name="size">A <see cref="Size"/> that represents the width and height of the rectangle.</param>
    public Rectangle(Point location, Size size)
    {
        X = location.X;
        Y = location.Y;
        Width = size.Width;
        Height = size.Height;
    }

    /// <summary>
    /// Creates a <see cref="Rectangle"/> structure from a <see cref="RectangleF"/> by truncating the values.
    /// </summary>
    /// <param name="rect">The <see cref="RectangleF"/> to convert.</param>
    public Rectangle(RectangleF rect)
    {
        X = (int)rect.X;
        Y = (int)rect.Y;
        Width = (int)rect.Width;
        Height = (int)rect.Height;
    }

    /// <summary>
    /// Determines if the specified point is contained within this <see cref="Rectangle"/> structure.
    /// </summary>
    /// <param name="x">The x-coordinate of the point to test.</param>
    /// <param name="y">The y-coordinate of the point to test.</param>
    /// <returns>true if the point is contained within this <see cref="Rectangle"/>; otherwise, false.</returns>
    public bool Contains(int x, int y)
    {
        return x >= Left && x < Right && y >= Top && y < Bottom;
    }

    /// <summary>
    /// Determines if the specified point is contained within this <see cref="Rectangle"/> structure.
    /// </summary>
    /// <param name="pt">The <see cref="Point"/> to test.</param>
    /// <returns>true if the point is contained within this <see cref="Rectangle"/>; otherwise, false.</returns>
    public bool Contains(Point pt)
    {
        return Contains(pt.X, pt.Y);
    }

    /// <summary>
    /// Determines if the rectangular region represented by <paramref name="rect"/> is entirely contained within this <see cref="Rectangle"/> structure.
    /// </summary>
    /// <param name="rect">The <see cref="Rectangle"/> to test.</param>
    /// <returns>true if the region is contained within this <see cref="Rectangle"/>; otherwise, false.</returns>
    public bool Contains(Rectangle rect)
    {
        return Left <= rect.Left && Right >= rect.Right && Top <= rect.Top && Bottom >= rect.Bottom;
    }

    /// <summary>
    /// Gets the rectangle that represents the union of two rectangles.
    /// </summary>
    /// <param name="a">A rectangle to union.</param>
    /// <param name="b">A rectangle to union.</param>
    /// <returns>A <see cref="Rectangle"/> that bounds the union of the two rectangles.</returns>
    public static Rectangle Union(Rectangle a, Rectangle b)
    {
        int x = Math.Min(a.Left, b.Left);
        int y = Math.Min(a.Top, b.Top);
        int width = Math.Max(a.Right, b.Right) - x;
        int height = Math.Max(a.Bottom, b.Bottom) - y;
        return new Rectangle(x, y, width, height);
    }

    /// <summary>
    /// Creates a rectangle that represents the intersection of two rectangles.
    /// </summary>
    /// <param name="a">A rectangle to intersect.</param>
    /// <param name="b">A rectangle to intersect.</param>
    /// <returns>A <see cref="Rectangle"/> that represents the intersection of the two rectangles.</returns>
    public static Rectangle Intersect(Rectangle a, Rectangle b)
    {
        int x = Math.Max(a.Left, b.Left);
        int y = Math.Max(a.Top, b.Top);
        int width = Math.Min(a.Right, b.Right) - x;
        int height = Math.Min(a.Bottom, b.Bottom) - y;

        if (width < 0 || height < 0)
            return Empty;

        return new Rectangle(x, y, width, height);
    }

    /// <summary>
    /// Determines whether this <see cref="Rectangle"/> intersects with the specified rectangle.
    /// </summary>
    /// <param name="rect">The rectangle to test.</param>
    /// <returns>true if the rectangles intersect; otherwise, false.</returns>
    public bool IntersectsWith(Rectangle rect)
    {
        return Left < rect.Right && Right > rect.Left && Top < rect.Bottom && Bottom > rect.Top;
    }

    /// <summary>
    /// Inflates this <see cref="Rectangle"/> by the specified amount.
    /// </summary>
    /// <param name="x">The amount to inflate horizontally.</param>
    /// <param name="y">The amount to inflate vertically.</param>
    /// <returns>The inflated rectangle.</returns>
    public Rectangle Inflate(int x, int y)
    {
        return new Rectangle(X - x, Y - y, Width + (x * 2), Height + (y * 2));
    }

    /// <summary>
    /// Inflates this <see cref="Rectangle"/> by the specified amount.
    /// </summary>
    /// <param name="size">The amount to inflate horizontally and vertically.</param>
    /// <returns>The inflated rectangle.</returns>
    public Rectangle Inflate(Size size)
    {
        return Inflate(size.Width, size.Height);
    }

    /// <summary>
    /// Creates and returns an inflated copy of the specified <see cref="Rectangle"/> structure.
    /// </summary>
    /// <param name="rect">The rectangle to inflate.</param>
    /// <param name="x">The amount to inflate horizontally.</param>
    /// <param name="y">The amount to inflate vertically.</param>
    /// <returns>The inflated rectangle.</returns>
    public static Rectangle Inflate(Rectangle rect, int x, int y)
    {
        return rect.Inflate(x, y);
    }

    /// <summary>
    /// Replaces this <see cref="Rectangle"/> with the intersection of itself and the specified <see cref="Rectangle"/>.
    /// </summary>
    /// <param name="rect">The rectangle to intersect with.</param>
    /// <returns>The intersected rectangle.</returns>
    public Rectangle Intersect(Rectangle rect)
    {
        return Intersect(this, rect);
    }

    /// <summary>
    /// Adjusts the location of this rectangle by the specified amount.
    /// </summary>
    /// <param name="x">The amount to offset the location horizontally.</param>
    /// <param name="y">The amount to offset the location vertically.</param>
    /// <returns>The offset rectangle.</returns>
    public Rectangle Offset(int x, int y)
    {
        return new Rectangle(X + x, Y + y, Width, Height);
    }

    /// <summary>
    /// Adjusts the location of this rectangle by the specified amount.
    /// </summary>
    /// <param name="pos">The <see cref="Point"/> specifying the offset amounts.</param>
    /// <returns>The offset rectangle.</returns>
    public Rectangle Offset(Point pos)
    {
        return Offset(pos.X, pos.Y);
    }

    /// <inheritdoc/>
    public bool Equals(Rectangle other)
    {
        return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return obj is Rectangle other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + X;
            hash = hash * 31 + Y;
            hash = hash * 31 + Width;
            hash = hash * 31 + Height;
            return hash;
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{{X={X},Y={Y},Width={Width},Height={Height}}}";
    }

    /// <summary>
    /// Specifies a <see cref="Rectangle"/> structure that has <see cref="X"/>, <see cref="Y"/>, <see cref="Width"/>, and <see cref="Height"/> values set to zero.
    /// </summary>
    public static Rectangle Empty => default;

    /// <summary>
    /// Tests whether two <see cref="Rectangle"/> structures are equal.
    /// </summary>
    public static bool operator ==(Rectangle left, Rectangle right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Tests whether two <see cref="Rectangle"/> structures are different.
    /// </summary>
    public static bool operator !=(Rectangle left, Rectangle right)
    {
        return !left.Equals(right);
    }
}
