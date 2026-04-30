using System;

namespace Suity;

/// <summary>
/// Base class for all objects in the Suity framework.
/// Provides lifecycle management and common functionality for objects.
/// </summary>
public abstract class Object
{
    /// <summary>
    /// Initializes a new instance of the Object class.
    /// </summary>
    public Object()
    {
        Device._current.ObjectCreate(this);
    }

    /// <summary>
    /// Gets or sets the name of the object.
    /// </summary>
    public string Name { get => GetName(); set => SetName(value); }

    /// <summary>
    /// Called when the object is being destroyed.
    /// Override this method to perform cleanup operations.
    /// </summary>
    protected internal virtual void OnDestroy()
    {
    }

    /// <summary>
    /// Gets the name of the object.
    /// </summary>
    /// <returns>The name of the object.</returns>
    protected virtual string GetName() => null;

    /// <summary>
    /// Sets the name of the object.
    /// </summary>
    /// <param name="name">The name to set.</param>
    protected virtual void SetName(string name)
    { }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string representation of the object.</returns>
    public override string ToString()
    {
        return GetName() ?? base.ToString();
    }

    #region Static

    /// <summary>
    /// Destroys an object and disposes of its resources.
    /// </summary>
    /// <param name="obj">The object to destroy.</param>
    public static void DestroyObject(Object obj)
    {
        if (obj is null)
        {
            return;
        }

        try
        {
            (obj as IDisposable)?.Dispose();

            obj.OnDestroy();
        }
        catch (Exception err)
        {
            Logs.LogError(err);
        }
    }

    #endregion
}

/// <summary>
/// Base class for objects with a unique identifier.
/// </summary>
public abstract class ObjectWithId : Object
{
    /// <summary>
    /// Gets the unique identifier of the object.
    /// </summary>
    public abstract long Id { get; }
}

/// <summary>
/// Base class for system objects that can be started and stopped.
/// </summary>
public abstract class SystemObject : Object
{
    /// <summary>
    /// Gets a value indicating whether the system object has been started.
    /// </summary>
    public abstract bool IsStarted { get; }

    /// <summary>
    /// Starts the system object.
    /// </summary>
    public abstract void Start();

    /// <summary>
    /// Stops the system object.
    /// </summary>
    public abstract void Stop();

    protected internal override void OnDestroy()
    {
        if (IsStarted)
        {
            Stop();
        }

        base.OnDestroy();
    }
}