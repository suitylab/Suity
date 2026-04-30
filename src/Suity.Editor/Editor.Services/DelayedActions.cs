using System;

namespace Suity.Editor.Services;

/// <summary>
/// Defines the contract for a component that manages and processes delayed actions.
/// </summary>
public interface IRunDelayed
{
    /// <summary>
    /// Adds a delayed action to be processed later.
    /// </summary>
    /// <param name="action">The delayed action to add.</param>
    void AddAction(DelayedAction action);

    /// <summary>
    /// Removes a previously added delayed action.
    /// </summary>
    /// <param name="action">The delayed action to remove.</param>
    void RemoveAction(DelayedAction action);

    /// <summary>
    /// Processes all pending delayed actions.
    /// </summary>
    void ProccessActions();
}


/// <summary>
/// Abstract base class for actions that should be delayed or executed after a certain count
/// </summary>
public abstract class DelayedAction
{
    /// <summary>
    /// Gets or sets the delay count before the action is executed
    /// </summary>
    public int DelayCount { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public DelayedAction()
    {
    }

    /// <summary>
    /// Parameterized constructor to initialize with a specific delay count
    /// </summary>
    /// <param name="initDelayCount">Initial delay count value</param>
    public DelayedAction(int initDelayCount)
    {
        DelayCount = initDelayCount;
    }

    /// <summary>
    /// Abstract method to be implemented by derived classes for the actual action
    /// </summary>
    public abstract void DoAction();

    /// <summary>
    /// Overrides GetHashCode to use the type's hash code
    /// </summary>
    /// <returns>Hash code of the object's type</returns>
    public override int GetHashCode()
    {
        return this.GetType().GetHashCode();
    }

    /// <summary>
    /// Overrides Equals to compare based on type equality
    /// </summary>
    /// <param name="obj">Object to compare with</param>
    /// <returns>True if objects are of the same type, false otherwise</returns>
    public override bool Equals(object obj)
    {
        DelayedAction other = (DelayedAction)obj;
        return this.GetType() == other.GetType();
    }
}


/// <summary>
/// Abstract base class for named actions that need to be delayed.
/// Inherits from DelayedAction to provide named functionality.
/// </summary>
public abstract class DelayedNamedAction : DelayedAction
{
    /// <summary>
    /// Gets the name of the action.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the old name of the action, if it was renamed.
    /// </summary>
    public string OldName { get; }

    /// <summary>
    /// Initializes a new instance of the DelayedNamedAction class.
    /// </summary>
    /// <param name="name">The name of the action.</param>
    /// <param name="oldName">The previous name of the action, if it was renamed.</param>
    protected DelayedNamedAction(string name, string oldName = null)
        : base()
    {
        Name = name;
        OldName = oldName;
    }

    /// <summary>
    /// Computes a hash code for the current instance.
    /// Combines the base hash code with hashes of Name and OldName.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        int hash = base.GetHashCode();
        if (Name != null) hash ^= Name.GetHashCode();
        if (OldName != null) hash ^= OldName.GetHashCode();
        return hash;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// Compares base equality and both Name and OldName properties.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object obj)
    {
        if (!base.Equals(obj)) return false;

        DelayedNamedAction other = (DelayedNamedAction)obj;
        return Name == other.Name && OldName == other.OldName;
    }
}


/// <summary>
/// Generic delayed action with a single value.
/// </summary>
public abstract class DelayedAction<T> : DelayedAction where T : class
{
    /// <summary>
    /// Gets the value associated with this action.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// Initializes a new instance of the DelayedAction{T} class.
    /// </summary>
    /// <param name="value">The value to associate with this action.</param>
    public DelayedAction(T value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Initializes a new instance of the DelayedAction{T} class with a delay count.
    /// </summary>
    /// <param name="value">The value to associate with this action.</param>
    /// <param name="initDelayCount">The initial delay count.</param>
    public DelayedAction(T value, int initDelayCount)
        : this(value)
    {
        DelayCount = initDelayCount;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        int hash = 0;
        if (Value != null) hash ^= Value.GetHashCode();
        return hash;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;

        DelayedAction<T> other = (DelayedAction<T>)obj;
        return object.Equals(Value, other.Value);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Value?.ToString();
    }
}

/// <summary>
/// Generic delayed action with two values.
/// </summary>
public abstract class DelayedAction<T1, T2> : DelayedAction
    where T1 : class
    where T2 : class
{
    /// <summary>
    /// Gets the first value.
    /// </summary>
    public T1 Value1 { get; }

    /// <summary>
    /// Gets the second value.
    /// </summary>
    public T2 Value2 { get; }

    /// <summary>
    /// Initializes a new instance of the DelayedAction{T1, T2} class.
    /// </summary>
    /// <param name="value1">The first value.</param>
    /// <param name="value2">The second value.</param>
    public DelayedAction(T1 value1, T2 value2)
    {
        Value1 = value1 ?? throw new ArgumentNullException(nameof(value1));
        Value2 = value2 ?? throw new ArgumentNullException(nameof(value2));
    }

    /// <summary>
    /// Initializes a new instance of the DelayedAction{T1, T2} class with a delay count.
    /// </summary>
    /// <param name="value1">The first value.</param>
    /// <param name="value2">The second value.</param>
    /// <param name="initDelayCount">The initial delay count.</param>
    public DelayedAction(T1 value1, T2 value2, int initDelayCount)
        : this(value1, value2)
    {
        DelayCount = initDelayCount;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        int hash = 0;
        if (Value1 != null) hash ^= Value1.GetHashCode();
        if (Value2 != null) hash ^= Value2.GetHashCode();
        return hash;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;

        DelayedAction<T1, T2> other = (DelayedAction<T1, T2>)obj;
        return object.Equals(Value1, other.Value1)
            && object.Equals(Value2, other.Value2);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Value1} {Value2}";
    }
}

/// <summary>
/// Generic delayed action with three values.
/// </summary>
public abstract class DelayedAction<T1, T2, T3> : DelayedAction
    where T1 : class
    where T2 : class
    where T3 : class
{
    /// <summary>
    /// Gets the first value.
    /// </summary>
    public T1 Value1 { get; }

    /// <summary>
    /// Gets the second value.
    /// </summary>
    public T2 Value2 { get; }

    /// <summary>
    /// Gets the third value.
    /// </summary>
    public T3 Value3 { get; }

    /// <summary>
    /// Initializes a new instance of the DelayedAction{T1, T2, T3} class.
    /// </summary>
    /// <param name="value1">The first value.</param>
    /// <param name="value2">The second value.</param>
    /// <param name="value3">The third value.</param>
    public DelayedAction(T1 value1, T2 value2, T3 value3)
    {
        Value1 = value1 ?? throw new ArgumentNullException(nameof(value1));
        Value2 = value2 ?? throw new ArgumentNullException(nameof(value2));
        Value3 = value3 ?? throw new ArgumentNullException(nameof(value3));
    }

    /// <summary>
    /// Initializes a new instance of the DelayedAction{T1, T2, T3} class with a delay count.
    /// </summary>
    /// <param name="value1">The first value.</param>
    /// <param name="value2">The second value.</param>
    /// <param name="value3">The third value.</param>
    /// <param name="initDelayCount">The initial delay count.</param>
    public DelayedAction(T1 value1, T2 value2, T3 value3, int initDelayCount)
        : this(value1, value2, value3)
    {
        DelayCount = initDelayCount;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        int hash = 0;
        if (Value1 != null) hash ^= Value1.GetHashCode();
        if (Value2 != null) hash ^= Value2.GetHashCode();
        if (Value3 != null) hash ^= Value3.GetHashCode();
        return hash;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;

        DelayedAction<T1, T2, T3> other = (DelayedAction<T1, T2, T3>)obj;
        return object.Equals(Value1, other.Value1)
            && object.Equals(Value2, other.Value2)
            && object.Equals(Value3, other.Value3);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Value1} {Value2} {Value3}";
    }
}

/// <summary>
/// Generic delayed action with a name and optional old name.
/// </summary>
public abstract class DelayedNamedAction<T> : DelayedAction<T> where T : class
{
    /// <summary>
    /// Gets the name of the action.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the old name if the action was renamed.
    /// </summary>
    public string OldName { get; }

    /// <summary>
    /// Initializes a new instance of the DelayedNamedAction{T} class.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="name">The name.</param>
    /// <param name="oldName">The old name if renamed.</param>
    protected DelayedNamedAction(T value, string name, string oldName = null)
        : base(value)
    {
        Name = name;
        OldName = oldName;
    }

    /// <summary>
    /// Initializes a new instance of the DelayedNamedAction{T} class with a delay count.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="name">The name.</param>
    /// <param name="oldName">The old name if renamed.</param>
    /// <param name="initDelayCount">The initial delay count.</param>
    public DelayedNamedAction(T value, string name, string oldName, int initDelayCount)
        : this(value, name, oldName)
    {
        DelayCount = initDelayCount;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        int hash = base.GetHashCode();
        if (Name != null) hash ^= Name.GetHashCode();
        if (OldName != null) hash ^= OldName.GetHashCode();
        return hash;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        if (!base.Equals(obj)) return false;

        DelayedNamedAction<T> other = (DelayedNamedAction<T>)obj;
        return Name == other.Name && OldName == other.OldName;
    }
}

/// <summary>
/// A delayed action that invokes a delegate when executed.
/// </summary>
public sealed class DelayedDelegateAction : DelayedAction<Action>
{
    /// <summary>
    /// Initializes a new instance of the DelayedDelegateAction class.
    /// </summary>
    /// <param name="value">The delegate to invoke.</param>
    public DelayedDelegateAction(Action value) : base(value)
    {
    }

    /// <summary>
    /// Initializes a new instance of the DelayedDelegateAction class with a delay count.
    /// </summary>
    /// <param name="value">The delegate to invoke.</param>
    /// <param name="initDelayCount">The initial delay count.</param>
    public DelayedDelegateAction(Action value, int initDelayCount) : base(value, initDelayCount)
    {
    }

    /// <inheritdoc/>
    public override void DoAction()
    {
        Value?.Invoke();
    }
}