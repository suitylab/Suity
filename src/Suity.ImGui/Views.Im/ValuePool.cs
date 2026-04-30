using Suity.Views.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Views.Im;

/// <summary>
/// Provides a thread-safe object pool for reusing instances of reference types.
/// </summary>
public static class ValuePool
{
    private static readonly ConcurrentDictionary<Type, ValuePoolItem> Pool = new();

    /// <summary>
    /// Gets an object from the pool, creating a new one if the pool is empty.
    /// </summary>
    /// <typeparam name="T">The type of object to get. Must be a reference type with a parameterless constructor.</typeparam>
    /// <returns>A pooled or newly created object of type T.</returns>
    public static T Get<T>() where T : class, new()
    {
        ValuePoolItem<T> stack = (ValuePoolItem<T>)Pool.GetOrAdd(typeof(T), _ => new ValuePoolItem<T>())!;

        return stack.GetValue(() => new T());
    }

    /// <summary>
    /// Gets an object from the pool, using the provided factory if the pool is empty.
    /// </summary>
    /// <typeparam name="T">The type of object to get. Must be a reference type.</typeparam>
    /// <param name="creation">A factory function to create a new object when the pool is empty.</param>
    /// <returns>A pooled or newly created object of type T.</returns>
    public static T Get<T>(Func<T> creation) where T : class
    {
        ValuePoolItem<T> stack = (ValuePoolItem<T>)Pool.GetOrAdd(typeof(T), _ => new ValuePoolItem<T>())!;

        return stack.GetValue(creation);
    }

    /// <summary>
    /// Gets an object from the pool and fills it with values using the provided action.
    /// </summary>
    /// <typeparam name="T">The type of object to get. Must be a reference type with a parameterless constructor.</typeparam>
    /// <param name="fillValue">An action to initialize the object's properties.</param>
    /// <returns>A pooled or newly created object of type T with values filled.</returns>
    public static T Get<T>(Action<T> fillValue) where T : class, new()
    {
        ValuePoolItem<T> stack = (ValuePoolItem<T>)Pool.GetOrAdd(typeof(T), _ => new ValuePoolItem<T>())!;

        T value = stack.GetValue(() => new T());
        fillValue(value);

        return value;
    }

    /// <summary>
    /// Gets an object from the pool using a factory and fills it with values.
    /// </summary>
    /// <typeparam name="T">The type of object to get. Must be a reference type.</typeparam>
    /// <param name="creation">A factory function to create a new object when the pool is empty.</param>
    /// <param name="fillValue">An action to initialize the object's properties.</param>
    /// <returns>A pooled or newly created object of type T with values filled.</returns>
    public static T Get<T>(Func<T> creation, Action<T> fillValue) where T : class
    {
        ValuePoolItem<T> stack = (ValuePoolItem<T>)Pool.GetOrAdd(typeof(T), _ => new ValuePoolItem<T>())!;

        T value = stack.GetValue(creation);
        fillValue(value);

        return value;
    }

    /// <summary>
    /// Returns an object to the pool for reuse.
    /// </summary>
    /// <typeparam name="T">The type of object to recycle.</typeparam>
    /// <param name="value">The object to return to the pool.</param>
    public static void Recycle<T>(T value) where T : class
    {
        ValuePoolItem<T> stack = (ValuePoolItem<T>)Pool.GetOrAdd(typeof(T), _ => new ValuePoolItem<T>())!;

        stack.Recycle(value);
    }

    /// <summary>
    /// Returns an object to the pool for reuse using its runtime type.
    /// </summary>
    /// <param name="value">The object to return to the pool.</param>
    public static void Recycle(object value)
    {
        if (Pool.TryGetValue(value.GetType(), out ValuePoolItem pool))
        {
            pool.RecycleObject(value);
        }
    }

    #region ValuePoolItem

    private abstract class ValuePoolItem
    {
        public abstract void RecycleObject(object value);
    }

    private class ValuePoolItem<T> : ValuePoolItem where T : class
    {
        private readonly ConcurrentStack<T> _stack = new();

        public T GetValue(Func<T> creation)
        {
            if (_stack.TryPop(out T result))
            {
                return result;
            }

            return creation();
        }

        public void Recycle(T value)
        {
            _stack.Push(value);
        }

        public override void RecycleObject(object value)
        {
            _stack.Push((T)value);
        }
    }

    #endregion
}
