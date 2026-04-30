using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Editor.AIGC.Flows.Pages;

/// <summary>
/// Represents a node in a rectangle-based spatial tree, holding data and a bounding rectangle.
/// </summary>
/// <typeparam name="T">The type of data associated with this node.</typeparam>
public class RectNode<T>
{
    /// <summary>
    /// Gets the data associated with this node.
    /// </summary>
    public T Data { get; }

    /// <summary>
    /// Gets the bounding rectangle of this node.
    /// </summary>
    public Rectangle Rect { get; }

    /// <summary>
    /// Gets the collection of child nodes contained within this node's rectangle.
    /// </summary>
    public List<RectNode<T>> Children { get; } = [];

    /// <summary>
    /// Initializes a new instance of <see cref="RectNode{T}"/> with an unbounded default rectangle.
    /// </summary>
    public RectNode()
    {
        Rect = new Rectangle(int.MinValue / 2, int.MinValue / 2, int.MaxValue, int.MaxValue);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="RectNode{T}"/> with the specified rectangle and no data.
    /// </summary>
    /// <param name="rect">The bounding rectangle for this node.</param>
    public RectNode(Rectangle rect)
    {
        Rect = rect;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="RectNode{T}"/> with data and a rectangle derived from a getter function.
    /// </summary>
    /// <param name="target">The data to associate with this node.</param>
    /// <param name="rectGetter">A function that retrieves the bounding rectangle from the data.</param>
    public RectNode(T target, Func<T, Rectangle> rectGetter)
    {
        Data = target;
        Rect = rectGetter(target);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="RectNode{T}"/> with the specified data and rectangle.
    /// </summary>
    /// <param name="target">The data to associate with this node.</param>
    /// <param name="rect">The bounding rectangle for this node.</param>
    public RectNode(T target, Rectangle rect)
    {
        Data = target;
        Rect = rect;
    }


    /// <summary>
    /// Compares two rectangle nodes for sorting, first by Y coordinate, then by X coordinate.
    /// </summary>
    /// <param name="a">The first node to compare.</param>
    /// <param name="b">The second node to compare.</param>
    /// <returns>A signed integer indicating the relative order of the nodes.</returns>
    public static int RectNodeSort(RectNode<T> a, RectNode<T> b)
    {
        int v = a.Rect.Y.CompareTo(b.Rect.Y);
        if (v != 0)
        {
            return v;
        }

        return a.Rect.X.CompareTo(b.Rect.X);
    }
}

/// <summary>
/// Builds a hierarchical tree structure from a collection of rectangles based on spatial containment.
/// </summary>
public class RectTreeBuilder
{
    /// <summary>
    /// Builds a rectangle containment tree from a collection of items.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="rects">The collection of items to build the tree from.</param>
    /// <param name="rectGetter">A function that retrieves the bounding rectangle from each item.</param>
    /// <returns>The root node of the constructed rectangle tree.</returns>
    public static RectNode<T> BuildTree<T>(IEnumerable<T> rects, Func<T, Rectangle> rectGetter)
    {
        // 1. Create a virtual root node, initialized to a large enough range (or logical root)
        // Here we assume the root node does not represent a specific physical rectangle, or set it as a minimum large rectangle containing all rectangles
        var root = new RectNode<T>();

        int getSquare(T t)
        {
            var r = rectGetter(t);
            return r.Width * r.Height;
        }

        // 2. Sort by area from largest to smallest
        // This ensures parent nodes are always inserted before child nodes
        var sortedRects = rects
            .OrderByDescending(getSquare)
            .ToList();

        // 3. Insert each rectangle into the tree one by one
        foreach (var rect in sortedRects)
        {
            InsertNode(root, new RectNode<T>(rect, rectGetter));
        }

        return root;
    }

    private static void InsertNode<T>(RectNode<T> parent, RectNode<T> newNode)
    {
        // Find if any child node of the current parent node contains the new node
        RectNode<T> potentialParent = null;
        foreach (var child in parent.Children)
        {
            // Use Rectangle.Contains to check containment
            if (child.Rect.Contains(newNode.Rect))
            {
                potentialParent = child;
                break; // Found a deeper parent node
            }
        }

        if (potentialParent != null)
        {
            // If a more suitable child node is found as parent, recursively search downward
            InsertNode(potentialParent, newNode);
        }
        else
        {
            // If no child node contains this new node, it is a direct child node of the current parent
            parent.Children.Add(newNode);
        }
    }
}
