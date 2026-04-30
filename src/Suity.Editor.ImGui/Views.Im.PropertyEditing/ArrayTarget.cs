using System;
using System.Collections.Generic;

namespace Suity.Views.Im.PropertyEditing;

/// <summary>
/// Represents an abstract target for array-based property editing operations.
/// </summary>
public abstract class ArrayTarget : ITarget
{
    /// <summary>
    /// Gets the parent target of this array target.
    /// </summary>
    public virtual ITarget? Parent => OwnerTarget?.Parent;

    /// <summary>
    /// Gets the owning property target that contains this array.
    /// </summary>
    public abstract PropertyTarget OwnerTarget { get; }

    /// <summary>
    /// Gets the handler responsible for array operations.
    /// </summary>
    public abstract ArrayHandler Handler { get; }

    /// <summary>
    /// Gets the type of elements contained in the array.
    /// </summary>
    public abstract Type? ElementType { get; }

    /// <summary>
    /// Determines whether this array target can be displayed.
    /// </summary>
    /// <returns><c>true</c> if the array can be displayed; otherwise, <c>false</c>.</returns>
    public abstract bool CanDisplay();

    /// <summary>
    /// Gets the element target at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element.</param>
    /// <returns>The property target for the element, or <c>null</c> if not found.</returns>
    public abstract PropertyTarget? GetElementTarget(int index);

    /// <summary>
    /// Gets or creates an element target at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element.</param>
    /// <param name="config">An optional action to configure the newly created target.</param>
    /// <returns>The property target for the element.</returns>
    public abstract PropertyTarget GetOrCreateElementTarget(int index, Action<PropertyTarget>? config = null);

    /// <summary>
    /// Gets all element targets in the array.
    /// </summary>
    public abstract IEnumerable<PropertyTarget> Elements { get; }

    /// <summary>
    /// Gets the lengths of the arrays represented by this target.
    /// </summary>
    /// <returns>A sequence of array lengths.</returns>
    public abstract IEnumerable<int> GetArrayLength();

    /// <summary>
    /// Sets the lengths of the arrays represented by this target.
    /// </summary>
    /// <param name="counts">The new lengths to set.</param>
    public abstract void SetArrayLength(IEnumerable<int> counts);

    /// <summary>
    /// Gets or sets the starting index for array element display.
    /// </summary>
    public abstract int StartIndex { get; set; }

    /// <summary>
    /// Gets the maximum array length, if applicable.
    /// </summary>
    /// <returns>The maximum length, or <c>null</c> if not applicable.</returns>
    public abstract int? GetArrayLengthMax();

    /// <summary>
    /// Gets the underlying array objects.
    /// </summary>
    /// <returns>A sequence of array objects.</returns>
    public abstract IEnumerable<object?> GetArrays();

    /// <summary>
    /// Gets the items at the specified index across all arrays.
    /// </summary>
    /// <param name="index">The zero-based index.</param>
    /// <returns>A sequence of items at the specified index.</returns>
    public abstract IEnumerable<object?> GetArrayItemAt(int index);

    /// <summary>
    /// Sets the items at the specified index across all arrays.
    /// </summary>
    /// <param name="index">The zero-based index.</param>
    /// <param name="items">The items to set.</param>
    public abstract void SetArrayItemAt(int index, IEnumerable<object> items);

    /// <summary>
    /// Adds new items to the end of the array.
    /// </summary>
    /// <param name="objects">The objects to add.</param>
    public abstract void PushArrayItem(IEnumerable<object?> objects);

    /// <summary>
    /// Removes the last item from the array.
    /// </summary>
    public abstract void PopArrayItem();

    /// <summary>
    /// Inserts items at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which to insert.</param>
    /// <param name="objects">The objects to insert.</param>
    public abstract void InsertArrayItemAt(int index, IEnumerable<object?> objects);

    /// <summary>
    /// Removes the item at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the item to remove.</param>
    public abstract void RemoveArrayItemAt(int index);

    /// <summary>
    /// Clones the item at the specified index and inserts the clone after it.
    /// </summary>
    /// <param name="index">The zero-based index of the item to clone.</param>
    public abstract void CloneArrayItemAt(int index);

    /// <summary>
    /// Swaps the item at the specified index with the next item.
    /// </summary>
    /// <param name="index">The zero-based index of the item to swap.</param>
    public abstract void SwapArrayItemAt(int index);

    /// <summary>
    /// Gets the parent objects that contain this array.
    /// </summary>
    /// <returns>A sequence of parent objects.</returns>
    public abstract IEnumerable<object?> GetParentObjects();
}
