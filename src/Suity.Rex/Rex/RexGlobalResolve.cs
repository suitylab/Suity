using System;

namespace Suity.Rex;

/// <summary>
/// Provides access to the global <see cref="IRexResolver"/> instance used for resolving services and data.
/// </summary>
public abstract class RexGlobalResolve 
{
    /// <summary>
    /// Gets or sets the current global resolver instance.
    /// </summary>
    public static IRexResolver Current { get; set; }
}

/// <summary>
/// Defines a resolver for accessing properties, data objects, and providing queued execution within the Rex system.
/// </summary>
public interface IRexResolver : IServiceProvider
{
    /// <summary>
    /// Gets the names of all properties available on the specified object.
    /// </summary>
    /// <param name="obj">The object to inspect.</param>
    /// <returns>An array of property names.</returns>
    string[] GetPropertyNames(object obj);

    /// <summary>
    /// Gets the value of a property from the specified object.
    /// </summary>
    /// <param name="obj">The object to get the property from.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The property value.</returns>
    object GetProperty(object obj, string propertyName);

    /// <summary>
    /// Gets the unique data identifier for the specified object.
    /// </summary>
    /// <param name="o">The object to get the ID for.</param>
    /// <returns>The data identifier string.</returns>
    string GetDataId(object o);

    /// <summary>
    /// Gets a registered service object by key.
    /// </summary>
    /// <typeparam name="T">The type of service to retrieve.</typeparam>
    /// <param name="key">The key used to identify the service.</param>
    /// <returns>The service instance, or null if not found.</returns>
    T GetObject<T>(string key) where T : class;

    /// <summary>
    /// Logs an exception that occurred during Rex operations.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    void LogException(Exception exception);

    /// <summary>
    /// Queues an action for deferred execution within the Rex system.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    void DoQueuedAction(Action action);
}