using Suity.Networking;
using System;
using System.Diagnostics;
using System.Security;

namespace Suity;

/// <summary>
/// Startup device
/// </summary>
public abstract class Device : IServiceProvider
{
    internal static Device _current = DefaultDevice.Default;

    /// <summary>
    /// Initialize the device
    /// </summary>
    /// <param name="device"></param>
    public static void InitializeDevice(Device device)
    {
        if (device == null)
        {
            throw new ArgumentNullException(nameof(device));
        }

        if (_current == device)
        {
            return;
        }

        if (_current is not DefaultDevice)
        {
            throw new SecurityException();
        }

        _current = device;
    }

    /// <summary>
    /// Get the current device.
    /// </summary>
    public static Device Current => _current;


    /// <summary>
    /// The location of the device
    /// </summary>
    public abstract string Location { get; }

    /// <summary>
    /// Current running time of the device
    /// </summary>
    public abstract float Time { get; }

    /// <summary>
    /// Called when an object is created.
    /// </summary>
    /// <param name="obj">The object that was created.</param>
    public abstract void ObjectCreate(Object obj);

    /// <summary>
    /// Queues an action to be executed.
    /// </summary>
    /// <param name="action">The action to queue.</param>
    public abstract void QueueAction(Action action);

    /// <summary>
    /// Adds a log message.
    /// </summary>
    /// <param name="type">The type of log message.</param>
    /// <param name="message">The log message.</param>
    public abstract void AddLog(LogMessageType type, object message);

    /// <summary>
    /// Adds a network log message.
    /// </summary>
    /// <param name="type">The type of log message.</param>
    /// <param name="direction">The network direction.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="channelId">The channel identifier.</param>
    /// <param name="message">The log message.</param>
    public abstract void AddNetworkLog(LogMessageType type, NetworkDirection direction, string sessionId, string channelId, object message);

    /// <summary>
    /// Adds an operation log entry.
    /// </summary>
    /// <param name="level">The log level.</param>
    /// <param name="category">The category.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="ip">The IP address.</param>
    /// <param name="data">The log data.</param>
    /// <param name="successful">Whether the operation was successful.</param>
    public abstract void AddOperationLog(int level, string category, string userId, string ip, object data, bool successful);

    /// <summary>
    /// Adds a resource log entry.
    /// </summary>
    /// <param name="key">The resource key.</param>
    /// <param name="path">The resource path.</param>
    public abstract void AddResourceLog(string key, string path);

    /// <summary>
    /// Adds an entity log entry.
    /// </summary>
    /// <param name="roomId">The room identifier.</param>
    /// <param name="entityId">The entity identifier.</param>
    /// <param name="entityName">The entity name.</param>
    /// <param name="actionType">The action type.</param>
    /// <param name="messageType">The message type.</param>
    /// <param name="value">The log value.</param>
    public abstract void AddEntityLog(long roomId, long entityId, string entityName, EntityActionTypes actionType, LogMessageType messageType, object value);

    /// <summary>
    /// Gets an environment configuration value.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <returns>The configuration value.</returns>
    public abstract string GetEnvironmentConfig(string key);

    /// <summary>
    /// Gets a service of the specified type.
    /// </summary>
    /// <param name="serviceType">The type of service to get.</param>
    /// <returns>The service instance.</returns>
    public abstract object GetService(Type serviceType);

    /// <summary>
    /// Gets a service of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of service to get.</typeparam>
    /// <returns>The service instance.</returns>
    public T GetService<T>() where T : class
    {
        return GetService(typeof(T)) as T;
    }
}

/// <summary>
/// The default implementation of the Device class.
/// Provides basic logging to debug output.
/// </summary>
internal sealed class DefaultDevice : Device
{
    /// <summary>
    /// Gets the singleton instance of the DefaultDevice.
    /// </summary>
    public static DefaultDevice Default { get; } = new();

    private readonly DateTime _startTime = DateTime.UtcNow;

    private DefaultDevice()
    {
    }

    /// <inheritdoc />
    public override string Location => string.Empty;

    /// <inheritdoc />
    public override float Time => (float)(DateTime.UtcNow - _startTime).TotalSeconds;

    /// <inheritdoc />
    public override void ObjectCreate(Object obj)
    {
    }

    /// <inheritdoc />
    public override void QueueAction(Action action) => action?.Invoke();

    /// <inheritdoc />
    public override void AddLog(LogMessageType type, object message)
    {
        Debug.WriteLine($"Log [{type}]: {message}");
    }

    /// <inheritdoc />
    public override void AddNetworkLog(LogMessageType type, NetworkDirection direction, string sessionId, string channelId, object message)
    {
        Debug.WriteLine($"NetworkLog [{type}] [{direction}] Session: {sessionId}, Channel: {channelId}, Message: {message}");
    }

    /// <inheritdoc />
    public override void AddOperationLog(int level, string category, string userId, string ip, object data, bool successful)
    {
        Debug.WriteLine($"OperationLog [Level: {level}] [Category: {category}] User: {userId}, IP: {ip}, Data: {data}, Successful: {successful}");
    }

    /// <inheritdoc />
    public override void AddResourceLog(string key, string path)
    {
        Debug.WriteLine($"ResourceLog Key: {key}, Path: {path}");
    }

    /// <inheritdoc />
    public override void AddEntityLog(long roomId, long entityId, string entityName, EntityActionTypes actionType, LogMessageType messageType, object value)
    {
        Debug.WriteLine($"EntityLog [Room: {roomId}] [Entity: {entityId} - {entityName}] Action: {actionType}, MessageType: {messageType}, Value: {value}");
    }

    /// <inheritdoc />
    public override string GetEnvironmentConfig(string key) => null;

    /// <inheritdoc />
    public override object GetService(Type serviceType) => null;
}