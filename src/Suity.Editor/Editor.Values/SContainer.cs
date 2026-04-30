using Suity.Editor.Types;
using System.Collections.Generic;

namespace Suity.Editor.Values;

/// <summary>
/// Base class for container items that hold other SItems.
/// </summary>
public abstract class SContainer : SItem
{
    private object _context;

    /// <summary>
    /// Creates an empty SContainer.
    /// </summary>
    public SContainer()
    { }

    /// <summary>
    /// Creates an SContainer with the specified input type.
    /// </summary>
    /// <param name="inputType">The input type definition.</param>
    public SContainer(TypeDefinition inputType) : base(inputType)
    {
    }

    /// <summary>
    /// Gets all values.
    /// </summary>
    /// <param name="context">The condition context.</param>
    public abstract IEnumerable<object> GetValues(ICondition context = null);

    /// <summary>
    /// Gets all child items.
    /// </summary>
    public abstract IEnumerable<SItem> Items { get; }

    /// <summary>
    /// Gets or sets the context object.
    /// </summary>
    public object Context
    {
        get => _context;
        set
        {
            if (ReferenceEquals(_context, value))
            {
                return;
            }

            object old = _context;
            _context = value;

            OnContextChanged(old);
        }
    }

    /// <summary>
    /// Removes a child item.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    public virtual void RemoveItem(SItem item)
    { }

    /// <summary>
    /// Clears all items.
    /// </summary>
    public virtual bool Clear() => true;

    /// <summary>
    /// Gets the field for an SItem.
    /// </summary>
    /// <param name="item">The SItem.</param>
    public virtual FieldObject GetField(SItem item) => null;

    /// <summary>
    /// Called when the context changes.
    /// </summary>
    /// <param name="oldContext">The old context.</param>
    protected virtual void OnContextChanged(object oldContext)
    { }
}